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
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Internal.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using System.Windows;

namespace ParcelFabricMultistoryTool
{
    ///<summary>
    /// This sample illustrates how an add-in can create a new group of controls focused in support of a 3D parcel fabric editing workflow.  The 3D scene provides rendering of multistory parcel polygons, allowing an editor to accurately select and vertically duplicate parcels.   
    ///</summary>
    ///<remarks>
    /// Using the sample:
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called ParcelFabricMultistoryTool.  Make sure that the Sample data is unzipped under C:\Data and the folder "C:\Data\ParcelFabric" is available.  
    /// 1. Open the solution in Visual Studio 2017 or 2019.
    /// 1. Click the Build menu and select Build Solution.
    /// 1. Click the Start button to open ArcGIS Pro. 
    /// 1. Open the project “NapervilleParcels.aprx" found in folder: “C:\data\ParcelFabric”.
    /// 1. The project opens in the 2D map view.  You can see two polygon layers displayed, Tax Parcels and Records, with an imagery basemap.
    /// 1. Click on the Condominium Units bookmark and you will find centered in the map a condominium complex, seen in the screenshot below with the red outline.  This will be the area for your parcel editing.
    /// 1. As a parcel fabric layer is contained in the map, the Parcels ribbon becomes available, as seen in the green outline in the screenshot below.  The Parcels ribbon consists of a Records tab, which contains tools and commands for editing parcels in a parcel fabric.
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Click on the Records tab and review the tools which are organized into several groups.  The Pro add-in you built has added a new group called Multistory Tools, as seen below, which is located on the ribbon to the right of the Tools gallery.  This new group provides the tools needed to help sequence and streamline the editing work in this workflow.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Click on the 3D Scene map to view the dataset in 3D.  Using the scroll wheel on the mouse, tilt the perspective so that you can see the parcel polygons extruded in the view, as seen in the screenshot below.  Notice that some polygons for the second and third floors have been created.
    /// ![UI](Screenshots/Screen3.png)
    /// 1. Zoom into any parcel, such as the one outlined in red above.  Click on the Records tab, and in the add-in Multistory Tools group, click on the Activate tool.  This custom tool allows you a spatial approach to activating a parcel fabric record for a location, by clicking on an available polygon feature in the Records layer.  With the Activate tool, click on the location for the new tax parcel polygon.  You should see a message box appear confirming the activated record.  This record should also now be visible in the upper-left corner of the scene, for example:
    /// ![UI](Screenshots/Screen4.png)
    /// 1. Next, from the Multistory Tools group, click on the Select button, which will activate the Select By Rectangle tool.  Use the tool to select the parcel polygon outlined in red in the screenshot further above.
    /// 1. The workflow requires a new parcel name value which will be applied to the new created duplicate.  This will help ensure a unique name value when viewing the data in the attribute table.  Type a new value into the Parcel Name edit box in the Multistory Tools group.  Any lower-case letters will be set to uppercase when the new feature is created.
    /// 1. After setting the parcel name, press the Duplicate Vertical button.  The previously selected parcel will be duplicated and a new parcel feature will appear selected above it.
    /// 1. Next, press the Show Attributes button to open the Attributes pane.  You should see the attributes for your new parcel feature which is currently selected.  You can confirm that the Name attribute value you entered on the ribbon is reflected for the new feature.  You can make any additional attribute updates in the pane, and view and update for the selected record in the Tax Parcels attribute table.
    ///   
    ///   
    /// Additional things you can try:
    /// - Create new parcels in either 2D or 3D and modify existing parcels.
    /// - Validate topology using the validate extent button and build parcels with the parcel fabric using the build extent or build with active record buttons.
    ///</remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ParcelFabricMultistoryTool_Module"));
            }
        }

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

        #region Business Logic

        public ParcelName_EditBox ParcelName_EditBox1 { get; set; }

        public void ActivateRecord(Geometry geometry)
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    var layers = MapView.Active.Map.GetLayersAsFlattenedList();
                    var pfL = layers.FirstOrDefault(l => l is ParcelLayer) as ParcelLayer;
                    // if there is no fabric in the map then exit
                    if (pfL == null)
                        return;

                    var recordsLayer = MapView.Active.Map.FindLayers("Records").FirstOrDefault() as BasicFeatureLayer;
                    if (recordsLayer == null)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Records Layer is not found.", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    RowCursor rowCursor = null;
                    // define a spatial query filter
                    var spatialQueryFilter = new SpatialQueryFilter
                    {
                        // passing the search geometry to the spatial filter
                        FilterGeometry = geometry,
                        // define the spatial relationship between search geometry and feature class
                        SpatialRelationship = SpatialRelationship.Intersects
                    };
                    // apply the spatial filter to the feature layer in question
                    rowCursor = recordsLayer.Search(spatialQueryFilter);

                    Guid guid = new Guid();
                    long lOID = -1;
                    string featName = "";
                    while (rowCursor.MoveNext())
                    {
                        var feature = rowCursor.Current as Feature;
                        guid = feature.GetGlobalID();
                        lOID = feature.GetObjectID();
                        featName = Convert.ToString(feature["NAME"]);
                    }

                    // Reference the parcel record and set it as the active record
                    var parcelRecord = new ParcelRecord(pfL.Map, featName, guid, lOID);
                    pfL.SetActiveRecord(parcelRecord);

                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Record activated:  " + featName, "Info", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exc)
                {
                    // Catch any exception found and display in a message box
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught: " + exc.Message);
                    return;
                }
            });
        }

        public void DuplicateParcelPolygons()
        {
            QueuedTask.Run(async () =>
            {
                // Check for an active mapview, if not, then prompt and exit
                if (MapView.Active == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No MapView currently active. Exiting...", "Info");
                    return;
                }
                // Check for the 'Tax' parcel type layer
                var featLayer = MapView.Active.Map.FindLayers("Tax Parcels").FirstOrDefault() as BasicFeatureLayer;
                if (featLayer == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Cannot duplicate. 'Tax Parcels' layer not found in current mapview.", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                // Check for selection of a single parcel layer
                int recCount = featLayer.GetSelection().GetCount();
                if (recCount != 1)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("A single parcel record must be selected.", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                // Make sure the parcel name is set
                if (ParcelName_EditBox1.Text == "")
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Editbox for parcel name is empty. Type in value. Exiting...", "Value Needed");
                    return;
                }
                try
                {
                    // Get the parcel name and set uppercase
                    string parcelName = ParcelName_EditBox1.Text;
                    parcelName = parcelName.ToUpper();

                    // Get selected polygon
                    var featureSelection = featLayer.GetSelection();

                    // Create edit operation
                    var duplicateFeatures = new EditOperation();
                    duplicateFeatures.Name = "Duplicate Features";
                    duplicateFeatures.SelectModifiedFeatures = true;

                    // Duplicate selected parcel with vertical offset of 10 feet
                    duplicateFeatures.Duplicate(featLayer, featureSelection.GetObjectIDs().FirstOrDefault(), 0, 0, 1000.0);
                    duplicateFeatures.Execute();

                    // Set the 'Name' attribute for the new parcel polygon from the value in the edit box
                    featureSelection = featLayer.GetSelection();
                    var oid = featureSelection.GetObjectIDs().FirstOrDefault();
                    // Update name attribute for newly duplicated record 
                    var inspector = new Inspector();
                    await inspector.LoadAsync(featLayer, oid);
                    inspector["NAME"] = parcelName;
                    // Define edit operation and execute 
                    var op = new EditOperation();
                    op.Name = string.Format("Duplicate {0}", oid);
                    op.Modify(inspector);
                    await op.ExecuteAsync();
                }
                catch (Exception exc)
                {
                    // Catch any exception found and display in a message box
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught: " + exc.Message);
                    return;
                }
            });
        }

        public void BuildParcelsByRecord()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    var layers = MapView.Active.Map.GetLayersAsFlattenedList();
                    var pfL = layers.FirstOrDefault(l => l is ParcelLayer) as ParcelLayer;

                    var pRec = pfL.GetActiveRecord();
                    pfL.SetActiveRecord(pRec);
                    var guid = pRec.Guid;
                    if (pfL == null)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Cannot Build Parcels. No Active Record.", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    var editOperation = new EditOperation()
                    {
                        Name = "Build Parcels",
                        ProgressMessage = "Build Parcels...",
                        ShowModalMessageAfterFailure = true,
                        SelectNewFeatures = true,
                        SelectModifiedFeatures = true
                    };

                    // Run build parcels by record  
                    editOperation.BuildParcelsByRecord(pfL, guid);
                    editOperation.ExecuteAsync();
                }
                catch (Exception exc)
                {
                    // Catch any exception found and display in a message box
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught: " + exc.Message);
                    return;
                }
            });
        }

        #endregion

    }
}
