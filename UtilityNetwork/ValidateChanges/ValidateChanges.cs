/*

   Copyright 2020 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using UtilityNetworkSamples;
using ArcGIS.Core.Data.UtilityNetwork.Extensions;

using Version = ArcGIS.Core.Data.Version;

namespace ValidateChanges
{
  internal class ValidateChanges : Button
  {
    protected async override void OnClick()
    {
      // Start by checking to make sure we have a single feature layer selected

      if (MapView.Active == null)
      {
        MessageBox.Show("Please select a utility network layer.", "Validate Changes");
        return;
      }

      MapViewEventArgs mapViewEventArgs = new MapViewEventArgs(MapView.Active);
      if (mapViewEventArgs.MapView.GetSelectedLayers().Count != 1)
      {
        MessageBox.Show("Please select a utility network layer.", "Validate Changes");
        return;
      }

      Layer selectionLayer = mapViewEventArgs.MapView.GetSelectedLayers()[0];
      if (!(selectionLayer is UtilityNetworkLayer) && !(selectionLayer is FeatureLayer) && !(selectionLayer is SubtypeGroupLayer))
      {
        MessageBox.Show("Please select a utility network layer.", "Validate Changes");
        return;
      }

      string message = "";
      // Generate our report.  The LoadTraceResults class is used to pass back results from the worker thread to the UI thread that we're currently executing.

      await QueuedTask.Run( () =>
      {
        message = ValidateChangedFeatures(selectionLayer);
      });

      MessageBox.Show(message);
    }

    private string ValidateChangedFeatures(Layer layer)
    {
      StringBuilder resultString = new StringBuilder();

      // Get utility network and geodatabase
      using (UtilityNetwork utilityNetwork = UtilityNetworkUtils.GetUtilityNetworkFromLayer(layer))
      using (Geodatabase geodatabase = utilityNetwork.GetDatastore() as Geodatabase)
      {

        // Determine what to validate
        //    File geodatabase - validate everything, synchronously
        //    Default version - validate everything, asynchronously
        //    Branch version - validate changes only, synchronously

        bool shouldValidateEverything;
        bool runAsync;

        if (!geodatabase.IsVersioningSupported())
        {
          shouldValidateEverything = true;
          runAsync = false;
        }
        else
        {
          using (VersionManager versionManager = geodatabase.GetVersionManager())
          using (Version currentVersion = versionManager.GetCurrentVersion())
          {
            if (IsDefaultVersion(currentVersion))
            {
              shouldValidateEverything = true;
              runAsync = true;
            }
            else
            {
              shouldValidateEverything = false;
              runAsync = false;
            }
          }
        }

        // If we validating everything, get an envelope from the dirty areas table
        EnvelopeBuilder envelopeBuilder = new EnvelopeBuilder(layer.GetSpatialReference());

        if (shouldValidateEverything)
        {
          using (Table dirtyAreaTable = utilityNetwork.GetSystemTable(SystemTableType.DirtyAreas))
          using (RowCursor rowCursor = dirtyAreaTable.Search())
          {
            envelopeBuilder = GetExtentFromRowCursor(envelopeBuilder, rowCursor);
          }
        }

        // else get an envelope using version differences
        else
        {
          using (VersionManager versionManager = geodatabase.GetVersionManager())
          using (Version currentVersion = versionManager.GetCurrentVersion())
          using (Version defaultVersion = currentVersion.GetParent())
          using (Geodatabase defaultGeodatabase = defaultVersion.Connect())
          using (UtilityNetwork defaultUtilityNetwork = defaultGeodatabase.OpenDataset<UtilityNetwork>(utilityNetwork.GetName()))
          using (Table dirtyAreaTable = utilityNetwork.GetSystemTable(SystemTableType.DirtyAreas))
          using (Table defaultDirtyAreaTable = defaultUtilityNetwork.GetSystemTable(SystemTableType.DirtyAreas))
          using (DifferenceCursor inserts = dirtyAreaTable.Differences(defaultDirtyAreaTable, DifferenceType.Insert))
          {
            envelopeBuilder = GetExtentFromDifferenceCursor(envelopeBuilder, inserts);
          }
        }

        // Run validate topology on our envelope
        Envelope extent = envelopeBuilder.ToGeometry();
        ValidationResult result = utilityNetwork.ValidateNetworkTopologyInEditOperation(extent, runAsync ? InvocationTarget.AsynchronousService : InvocationTarget.SynchronousService);
        if (result.HasErrors)
        {
          resultString.AppendLine("Errors found.");
        }
        else
        {
          resultString.AppendLine("No errors found.");
        }
      }

      return resultString.ToString();
    }

    public bool IsDefaultVersion(Version version)
    {
      Version parentVersion = version.GetParent();
      if (parentVersion == null)
      {
        return true;
      }
      parentVersion.Dispose();
      return false;
    }

    private EnvelopeBuilder GetExtentFromRowCursor(EnvelopeBuilder envelopeBuilder, RowCursor rowCursor)
    {
      while (rowCursor.MoveNext())
      {
        using (Feature feature = rowCursor.Current as Feature)
        {
          Envelope newEnvelope = feature.GetShape().Extent;
          envelopeBuilder = Union(envelopeBuilder, newEnvelope);
        }
      }
      return envelopeBuilder;
    }

    private EnvelopeBuilder GetExtentFromDifferenceCursor(EnvelopeBuilder envelopeBuilder, DifferenceCursor differenceCursor)
    {
      while (differenceCursor.MoveNext())
      {
        using (Feature differenceFeature = differenceCursor.Current as Feature)
        {
          Envelope newEnvelope = differenceFeature.GetShape().Extent;
          envelopeBuilder = Union(envelopeBuilder, newEnvelope);
        }
      }
      return envelopeBuilder;
    }

    private EnvelopeBuilder Union(EnvelopeBuilder envelopeBuilder, Envelope newEnvelope)
    {
      if (envelopeBuilder.IsEmpty)
      {
        return new EnvelopeBuilder(newEnvelope);
      }
      else
      {
        envelopeBuilder.Union(newEnvelope);
        return envelopeBuilder;
      }
    }



  }
}
