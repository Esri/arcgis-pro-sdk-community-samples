/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace EditEventsSample.Dockpane
{
  internal class EditorEventsSpyViewModel : DockPane
  {
    private const string _dockPaneID = "EditEventsSample_Dockpane_EditorEventsSpy";
    private double _delta = 200.0;
    private static double _offset = 0.0;
    private int _numCreatesAtOneTime = 2;//change this to trigger more or less creates
                                         //when the create button is clicked

    private ICommand _createCmd;
    private ICommand _changeCmd;
    private ICommand _deleteCmd;
    private ICommand _showCmd;

    //For the events
    private Dictionary<string, List<SubscriptionToken>> _rowevents = new Dictionary<string, List<SubscriptionToken>>();
    private List<SubscriptionToken> _editevents = new List<SubscriptionToken>();
    private ICommand _startStopCmd;
    private ICommand _clearCmd = null;
    private bool _listening = false;
    private static readonly object _lock = new object();
    private List<string> _entries = new List<string>();
    private bool _firstStart = true;


    //Flags for the row event handling
    private bool _cancelEdit = false;
    private bool _validate = false;
    private bool _failvalidate = false;
    private string _validateMsg = "";


    protected EditorEventsSpyViewModel() {
      AddEntry("Click 'Start Events' to start listening to edit events");
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }


    public string EventLog
    {
      get
      {
        string contents = "";
        lock (_lock)
        {
          contents = string.Join("\r\n", _entries.ToArray());
        }
        return contents;
      }
    }

    public override OperationManager OperationManager
    {
      get
      {
        return MapView.Active?.Map.OperationManager;
      }
    }


    #region Flags

    public bool CancelEdits
    {
      get
      {
        return _cancelEdit;
      }
      set
      {
        SetProperty(ref _cancelEdit, value, () => CancelEdits);
      }
    }

    public bool ValidateEdits
    {
      get
      {
        return _validate;
      }
      set
      {
        SetProperty(ref _validate, value, () => ValidateEdits);
      }
    }

    public bool FailValidateEdits
    {
      get
      {
        return _failvalidate;
      }
      set
      {
        SetProperty(ref _failvalidate, value, () => FailValidateEdits);
      }
    }

    #endregion Flags

    #region Editing Commands

    public ICommand CreateCmd
    {
      get
      {
        if (_createCmd == null)
          _createCmd = new RelayCommand(() => DoCreate());
        return _createCmd;
      }
    }

    public ICommand ChangeCmd
    {
      get
      {
        if (_changeCmd == null)
          _changeCmd = new RelayCommand(() => DoChange());
        return _changeCmd;
      }
    }

    public ICommand DeleteCmd
    {
      get
      {
        if (_deleteCmd == null)
          _deleteCmd = new RelayCommand(() => DoDelete());
        return _deleteCmd;
      }
    }

    private void DoCreate()
    {
      var x1 = 7683671.0;
      var y1 = 687075.0;

      var crimes = Module1.Current.Crimes;
      if (NoCrimes(crimes))
        return;

      var editOp = new EditOperation();
      editOp.Name = $"Create {Module1.Current.CrimesLayerName}";
      editOp.SelectNewFeatures = true;

      QueuedTask.Run(() =>
      {
        //do multiple creates if specified
        for(int i = 0; i < _numCreatesAtOneTime; i++)
        {
          x1 += _offset;
          y1 += _offset;
          _offset += _delta;
          var pt = MapPointBuilder.CreateMapPoint(x1, y1, crimes.GetSpatialReference());
          Dictionary<string, object> attributes = new Dictionary<string, object>();
          attributes["SHAPE"] = pt;
          attributes["OFFENSE_TYPE"] = 4;
          attributes["MAJOR_OFFENSE_TYPE"] = "DUI";
          //do the create
          editOp.Create(crimes, attributes);
        }
        
        editOp.Execute();
      });
    }

    private async void DoChange()
    {
      var crimes = Module1.Current.Crimes;
      if (NoCrimes(crimes))
        return;

      bool noSelect = true;
      var editOp = new EditOperation();
      editOp.Name = $"Change {Module1.Current.CrimesLayerName}";
      editOp.SelectModifiedFeatures = true;

      await QueuedTask.Run(() =>
      {
        using (var select = crimes.GetSelection())
        {
          if (select.GetCount() > 0)
          {
            noSelect = false;
            foreach (var oid in select.GetObjectIDs())
            {
              //change an attribute
              Dictionary<string, object> attributes = new Dictionary<string, object>();
              attributes["POLICE_DISTRICT"] = "999";
              editOp.Modify(crimes, oid, attributes);
            }
            editOp.Execute();
          }
        }
          
      });
      if (noSelect) NothingSelected();
    }

    private async void DoDelete()
    {
      var crimes = Module1.Current.Crimes;
      if (NoCrimes(crimes))
        return;

      bool noSelect = true;
      var editOp = new EditOperation();
      editOp.Name = $"Delete {Module1.Current.CrimesLayerName}";

      await QueuedTask.Run(() =>
      {
        using (var select = crimes.GetSelection())
        {
          if (select.GetCount() > 0)
          {
            noSelect = false;
            editOp.Delete(crimes, select.GetObjectIDs());
            editOp.Execute();
          }
        }
        if (!_cancelEdit && !_failvalidate)
          crimes.ClearSelection();
      });
      if (noSelect) NothingSelected();
    }

    private bool NoCrimes(FeatureLayer crimes)
    {
      if (crimes == null)
      {
        MessageBox.Show($"Please add the {Module1.Current.CrimesLayerName} feature layer from the sample data to your map",
                        $"{Module1.Current.CrimesLayerName} missing");
        return true;
      }
      return false;
    }

    private void NothingSelected()
    {
      MessageBox.Show($"Please select some {Module1.Current.CrimesLayerName} then re-execute",
                        $"No {Module1.Current.CrimesLayerName} selected");
    }
    #endregion Editing Commands

    public ICommand ShowSelectedCmd
    {
      get
      {
        if (_showCmd == null)
        {
          _showCmd = new RelayCommand(async () =>
          {
            bool noSelect = true;
            await QueuedTask.Run(() =>
            {
              var select = Module1.Current.Crimes?.GetSelection();
              if (select != null)
              {
                if (select.GetCount() > 0)
                {
                  noSelect = false;
                  var camera = MapView.Active.Camera;
                  MapView.Active.ZoomTo(Module1.Current.Crimes, true,
                    new TimeSpan(0, 0, 2));
                  MapView.Active.ZoomTo(camera, new TimeSpan(0, 0, 0,0,500));
                  select.Dispose();
                }
              }
            });
            if (noSelect) NothingSelected();
          });
        }
        return _showCmd;
      }
    }
    #region Events

    public string ButtonText => _listening ? "Stop Events" : "Start Events";

    #region RegisterUnregister

    public ICommand StartStopCmd
    {
      get
      {
        if (_startStopCmd == null)
        {
          _startStopCmd = new RelayCommand(() => {
            if (_firstStart)
            {
              ClearEntries();
              _firstStart = false;
            }
            if (_rowevents.Count > 0)
            {
              _listening = Unregister();
              AddEntry("Not listening");
            }
            else
            {
              _listening = Register();
              AddEntry("Listening");
            }
            NotifyPropertyChanged("ButtonText");
          });
        }
        return _startStopCmd;
      }
    }

    private bool Register()
    {
      var layers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
      QueuedTask.Run(() => {
        foreach (var fl in layers)
        {
          var fc = fl.GetFeatureClass();
          var tokens = new List<SubscriptionToken>();
          //These events are fired once ~per feature~,
          //per table
          tokens.Add(RowCreatedEvent.Subscribe((rc) => RowEventHandler(rc), fc));
          tokens.Add(RowChangedEvent.Subscribe((rc) => RowEventHandler(rc), fc));
          tokens.Add(RowDeletedEvent.Subscribe((rc) => RowEventHandler(rc), fc));
          _rowevents[fl.Name] = tokens;
        }

        //This event is fired once per edit execute
        //Note: This event won't fire if the edits were cancelled
        _editevents.Add(EditCompletingEvent.Subscribe((ec) =>
        {
          RecordEvent("EditCompletingEvent", "");
          //can also cancel edit in the completing event...
          //cancels everything (that is cancealable)
          //
          //you'll need to modify the RowEventHandler() to prevent if
          //from doing the cancel or this will never get called
          if (_cancelEdit)
          {
            ec.CancelEdit($"EditCompletingEvent, edit cancelled");
            AddEntry("*** edits cancelled");
            AddEntry("---------------------------------");
          }
        }));
      //This event is fired after all the edits are completed (and on
      //save, discard, undo, redo) and is fired once
      _editevents.Add(EditCompletedEvent.Subscribe((ec) => {
            HandleEditCompletedEvent(ec);
            return Task.FromResult(0);
          }));

      });
      return true;
    }

    private bool Unregister()
    {
      //Careful here - events have to be unregistered on the same
      //thread they were registered on...hence the use of the
      //Queued Task
      QueuedTask.Run(() =>
      {
        //One kvp per layer....of which there is only one in the sample
        //out of the box but you can add others and register for events
        foreach (var kvp in _rowevents)
        {
          RowCreatedEvent.Unsubscribe(kvp.Value[0]);
          RowChangedEvent.Unsubscribe(kvp.Value[1]);
          RowDeletedEvent.Unsubscribe(kvp.Value[2]);
          kvp.Value.Clear();
        }
        _rowevents.Clear();

          //Editing and Edit Completed.
          EditCompletingEvent.Unsubscribe(_editevents[0]);
          EditCompletingEvent.Unsubscribe(_editevents[1]);
        _editevents.Clear();
      });
      
      return false;
    }

    #endregion RegisterUnregister

    public ICommand ClearTextCmd
    {
      get
      {
        if (_clearCmd == null)
          _clearCmd = new RelayCommand(() => ClearEntries());
        return _clearCmd;
      }
    }

    #region Event Handlers

    private void HandleEditCompletedEvent(EditCompletedEventArgs args)
    {

      RecordEvent("EditCompletedEvent", args.CompletedType.ToString());
      StringBuilder adds = new StringBuilder();
      StringBuilder mods = new StringBuilder();
      StringBuilder dels = new StringBuilder();
      adds.AppendLine("Adds");
      mods.AppendLine("Modifies");
      dels.AppendLine("Deletes");

      if (args.Creates != null)
      {
        foreach (var kvp in args.Creates)
        {
          var oids = string.Join(",", kvp.Value.Select(n => n.ToString()).ToArray());
          adds.AppendLine($" {kvp.Key.Name} {oids}");
        }
      }
      else
      {
        adds.AppendLine("  No Adds");
      }
      if (args.Modifies != null)
      {
        foreach (var kvp in args.Modifies)
        {
          var oids = string.Join(",", kvp.Value.Select(n => n.ToString()).ToArray());
          mods.AppendLine($" {kvp.Key.Name} {oids}");
        }
      }
      else
      {
        mods.AppendLine("  No Modifies");
      }
      if (args.Deletes != null)
      {
        foreach (var kvp in args.Deletes)
        {
          var oids = string.Join(",", kvp.Value.Select(n => n.ToString()).ToArray());
          dels.AppendLine($" {kvp.Key.Name} {oids}");
        }
      }
      else
      {
        dels.AppendLine("  No Deletes");
      }
      AddEntry(adds.ToString());
      AddEntry(mods.ToString());
      AddEntry(dels.ToString());
      AddEntry("---------------------------------");
    }

    private void RowEventHandler(RowChangedEventArgs rc)
    {
      using (var table = rc.Row.GetTable())
      {
        
        RecordEvent(rc, table);
        //validate flag is set
        //Note, we are validating deletes as well...if that makes sense ;-)
        //if not, change the sample to check the rc.EditType for 
        //EditType.Delete and skip...
        if (_validate)
        {
          //You can use 'rc.Row.HasValueChanged(fieldIndex)` to determine if
          //a value you need to validate has changed
          //
          //call your validation method as needed...
          //our validate method is a placeholder
          if (!ValidateTheRow(rc.Row))
          {
            //if your validation fails take the appropriate action..
            //we cancel the edit in this example
            AddEntry($"*** {_validateMsg}");
            rc.CancelEdit(_validateMsg);
            AddEntry("*** edit cancelled");
            AddEntry("---------------------------------");
            return;
          }
          AddEntry("*** row validated");
        }

        //Cancel flag is set. If you have _failvalidate checked you won't
        //get here - validation will have failed and canceled the edit
        if (_cancelEdit)
        {
          //cancel the edit
          rc.CancelEdit($"{rc.EditType} for {table.GetName()} cancelled");
          AddEntry("*** edit cancelled");
          AddEntry("---------------------------------");
        }
      }

    }

    internal bool ValidateTheRow(Row row)
    {
      //in the sample we are either returning true or deliberately
      //failing the row validation.
      if (_failvalidate)
      {
        var idx = row.FindField("POLICE_DISTRICT");
				if (idx < 0) _validateMsg = $@"Force a failed validation: 'POLICE_DISTRICT' not found";
				else
				{
					var district = (string)row[idx];
					_validateMsg = $"Invalid district: {district}, row {row.GetObjectID()}";
				}
      }
      return !_failvalidate;
    }

    private void AddEntry(string entry)
    {
      lock (_lock)
      {
        _entries.Add($"{entry}");
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    private void ClearEntries()
    {
      lock (_lock)
      {
        _entries.Clear();
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    private void RecordEvent(string eventName, string entry)
    {
      var dateTime = DateTime.Now.ToString("G");
      lock (_lock)
      {
        _entries.Add($"{dateTime}: {eventName} {entry}");
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    private void RecordEvent(RowChangedEventArgs rc, Table table)
    {
      var eventName = $"Row{rc.EditType.ToString()}dEvent";

      var entry = $"{table.GetName()}, oid:{rc.Row.GetObjectID()}";
      RecordEvent(eventName, entry);
    }

    #endregion Event Handlers

    #endregion Events
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class EditorEventsSpy_ShowButton : Button
  {
    protected override void OnClick()
    {
      EditorEventsSpyViewModel.Show();
    }
  }
}
