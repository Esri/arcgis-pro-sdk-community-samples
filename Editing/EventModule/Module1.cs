//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace EventModule
{
  /// <summary>This sample demonstrates how to setup edit events in a module to act like an extension.</summary>
  /// <remarks>
  /// 	<para>In Pro there are two types of edit events you can subscribe to; row based events for create/change/delete on rows, and edit completed events for when
  /// an edit operation completes. Both can get you similar information but the row events give you more control and information during the edit
  /// as compared to after it. You can subscribe to these events within a custom control or using a module without any controls, similar to extensions in ArcMap.</para>
  /// 	<para>To create an add-in that acts like an extension, the module must be set to autoLoad = true in Config.daml. Your code is then placed in the Initialize method
  /// for the module.<br/>
  /// Edit completed events listen to all layers in all maps. Row events listen for specific changes to specific tables. Since the module is initialized before the
  /// project's maps and layers, you subscribe to these events after the project is opened.</para>
  /// 	<para>To use this sample you can either compile and open Pro or run through the Visual Studio debugger.<br/>
  /// You should see edit and row events fired while editing data.<br/>
  /// To stop this add-in you can delete it with the add-in manager in the application.</para>
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private ArcGIS.Core.Events.SubscriptionToken _rowCreateToken, _rowDeleteToken, _rowChangedToken;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("EventModule_Module"));
      }
    }

    protected override bool Initialize()
    {
      //listen to project open/close events
      ProjectOpenedEvent.Subscribe(onProjectOpened);
      ProjectClosedEvent.Subscribe(onProjectClosed);

      return base.Initialize();
    }

    private void onProjectOpened(ProjectEventArgs obj)
    {
      //subscribe to edit completed event
      //this is across all maps and layers in the project
      EditCompletedEvent.Subscribe(onEditCompleted);

      //subscribe to row events for a certain layer in a certain map
      //look for a map named 'Layers' in the project
      var mapProjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name == "Layers");
      if (mapProjItem == null)
        return;
      
      //run on MCT
      QueuedTask.Run(() =>
      {
        var theMap = mapProjItem.GetMap();

        //look for a layer named 'Parcels' in the map
        var featLayer = theMap.FindLayers("Parcels").FirstOrDefault() as FeatureLayer;
        if (featLayer == null)
          return;
        var layerTable = featLayer.GetTable();

        //setup row events
        _rowCreateToken = RowCreatedEvent.Subscribe(onRowCreateEvent, layerTable);
        _rowDeleteToken = RowDeletedEvent.Subscribe(onRowDeleteEvent, layerTable);
        _rowChangedToken = RowChangedEvent.Subscribe(onRowChangedEvent, layerTable);
      });
    }

    protected Task onEditCompleted(EditCompletedEventArgs args)
    {
      //show the type and number of edits
      MessageBox.Show("Creates: " + args.Creates.Values.Sum(list => list.Count).ToString() + "\n" +
                      "Modifies: " + args.Modifies.Values.Sum(list => list.Count).ToString() + "\n" +
                      "Deletes: " + args.Deletes.Values.Sum(list => list.Count).ToString(), "Edit Completed Event");
      return Task.FromResult(0);
    }

    private void onRowCreateEvent(RowChangedEventArgs obj)
    {
      //do something on row create
      MessageBox.Show("Created row id " + obj.Row.GetObjectID().ToString(), "Row Created Event");
    }

    private void onRowDeleteEvent(RowChangedEventArgs obj)
    {
      //do something on row delete
      MessageBox.Show("Deleted row id " + obj.Row.GetObjectID().ToString(), "Row Deleted Event");
    }

    private void onRowChangedEvent(RowChangedEventArgs obj)
    {
      //do something on row changed
      MessageBox.Show("Changed row id " + obj.Row.GetObjectID().ToString(), "Row Changed Event");
    }

    private void onProjectClosed(ProjectEventArgs obj)
    {
      //Unsubscribe from events
      EditCompletedEvent.Unsubscribe(onEditCompleted);

      QueuedTask.Run(() =>
      {
        RowCreatedEvent.Unsubscribe(_rowCreateToken);
        RowDeletedEvent.Unsubscribe(_rowDeleteToken);
        RowChangedEvent.Unsubscribe(_rowChangedToken);
      });
    }
  }
}
