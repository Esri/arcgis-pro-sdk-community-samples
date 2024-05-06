/*

   Copyright 2024 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Editing;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RowEventTest
{
    /// <summary>
    /// This sample shows how to add row events to track creating and or modifying features in a map.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data
    /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
    /// 1. In Visual Studio click the Build menu.Then select Build Solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
    /// 1. Select the 'Row Events' tab on the ArcGIS Pro ribbon select 10 for the '# Records' then click the 'Test Polygon Row Create' button to create 10 polygon features in the current map extent.
    /// ![UI](Screenshots/Screenshot1.png)
    /// 1. See the 'Show Events' dockpane for the events that were fired.
    /// 1. Click the "Test Polygon Row Modify" button to modify all polygon features in the current map extent. 
    /// ![UI](Screenshots/Screenshot2.png)
    /// 1. See the 'Show Events' dockpane for the events that were fired.
    /// </remarks>
    internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("RowEventTest_Module");

    #region Updates to Show Events Dockpane
    internal static ShowEventsViewModel ShowEventsViewModel { get; set; }

    internal static void AddEntry(string entry)
    {
      ShowEventsViewModel?.AddEntry(entry);
      ShowEventsViewModel?.RefreshEvents();
    }

    internal static void ClearEntries()
    {
      ShowEventsViewModel.RunOnUiThread(() => ShowEventsViewModel?.ClearEntries());
    }
    #endregion

    #region Color Utilities

    private static readonly Random rand = new Random();

    internal static CIMColor GetRandomColour()
    {
      return ColorFactory.Instance.CreateRGBColor(rand.Next(256), rand.Next(256), rand.Next(256));
    }

    #endregion Color Utilities

    #region Manage Event Listeners

    private static Dictionary<string, List<SubscriptionToken>> _rowevents = new();
    private static SubscriptionToken _editCompletedToken = null;
    internal static int EventModifiedCount { get; set; } = 0;
    internal static int EventCreatedCount { get; set; } = 0;
    public static int TestAnnoCycles { get; internal set; }
    public static int TestPolyCycles { get; internal set; }

    internal static async Task StartListening()
    {
      var layers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>();
      Module1.ClearEntries();
      await QueuedTask.Run(() =>
      {
        foreach (var fl in layers)
        {
          var table = fl.GetTable();
          Module1.AddEntry($"Listening to {table.GetName()}");
          List<SubscriptionToken> tokens = new()
          {
            //These events are fired once ~per feature~,
            //per table
            RowCreatedEvent.Subscribe((rc) => RowEventHandler(rc), table),
            RowChangedEvent.Subscribe((rc) => RowEventHandler(rc), table),
            RowDeletedEvent.Subscribe((rc) => RowEventHandler(rc), table)
          };
          _rowevents[fl.Name] = tokens;
        }
        _editCompletedToken = EditCompletedEvent.Subscribe((ec) => EditCompletedHandler(ec), true);
      });
      Module1.AddEntry("Listener started");
    }

    internal static async Task StopListening()
    {
      // Careful here - events have to be unregistered on the same
      // thread they were registered on...hence the use of the
      // Queued Task
      await QueuedTask.Run(() =>
      {
        // One kvp per layer....of which there is only one in the sample
        // out of the box but you can add others and register for events
        foreach (var kvp in _rowevents)
        {
          RowCreatedEvent.Unsubscribe(kvp.Value[0]);
          RowChangedEvent.Unsubscribe(kvp.Value[1]);
          RowDeletedEvent.Unsubscribe(kvp.Value[2]);
          kvp.Value.Clear();
        }
        _rowevents.Clear();
        EditCompletedEvent.Unsubscribe(_editCompletedToken);
        _editCompletedToken = null;
      });
      Module1.AddEntry("Stopped Listener");
    }

    private static Task EditCompletedHandler(EditCompletedEventArgs ec)
    {
      switch (ec.CompletedType)
      {
        case EditCompletedType.Operation:
          {
            List <string> mapMembers = new List<string>();
            foreach (var mapMember in ec.Members)
            {
              mapMembers.Add(mapMember.Name);
            }
            Module1.AddEntry($"EditCompletedEvent Operation complete: {string.Join (",", mapMembers)}");
          }          
          break;
        default:
          Module1.AddEntry($"EditCompletedEvent other type");
          break;
      }
      return Task.CompletedTask;
    }

    private static void RowEventHandler(RowChangedEventArgs rc)
    {
      using var table = rc.Row.GetTable();
      var eventName = $"Row{rc.EditType.ToString()}d Event";
      switch (rc.EditType)
      {
        case EditType.Create:
          EventCreatedCount++;
          break;
        case EditType.Change:
          EventModifiedCount++;
          break;
        case EditType.Delete:
          eventName = "RowDeleted Event";
          break;
      }
      var entry = $"{table.GetName()}, oid:{rc.Row.GetObjectID()}";
      var dateTime = DateTime.Now.ToString("G");
      Module1.AddEntry($"{dateTime}: {eventName} {entry}");
    }

    #endregion Manage Event Listeners

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}
