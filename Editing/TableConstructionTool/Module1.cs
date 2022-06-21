/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace TableConstructionTool
{
  /// <summary>
  /// This sample demonstrates 3 workflows that use the "Table Construction tool" - Basic, Intermediate and Advanced. To use this sample, you will need the TableConstructionTool.ppkx project package available in the ArcGIS Pro Community sample data.
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the Build menu and select Build Solution.
  /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
  /// 1. In ArcGIS Pro, open the TableConstructionTool.ppkx project package.
  /// 1. Open the "Map" project item, if it is not already open.  This map will have a "USCities" feature layer and a "CityDemographicsTable" standalone table.
  /// 1. Activate the "Edit" tab on the ribbon and click the "Create" button to open the "Create Features" dockpane.
  /// 1. Select the CityDemographicsTable template. This will display the various Table construction tools available for this template. The highlighted 3 tools are custom table constructions tools that have been added by this sample.
  /// ![UI](screenshots/TableConstructionTools.png)
  /// ### Basic table construction tool
  /// 1. Click the "Basic Table Construction Tool" to activate it.  This is the button with the Lion icon. 
  /// 1. Click the "Create" button. This will invoke the code in the "OnSketchCompleteAsync" callback in the BasicTableConstTool.cs file in the Visual Studio solution.
  /// 1. One record will be added to the "CityDemographicsTable" table. Dismiss the message box that displays this information. At this point, you can open the CityDemographicsTable standalone table and check the newly added record.
  /// ### Intermediate table construction tool
  /// 1. Click the "Intermediate Table Construction Tool" to activate it.  This is the button with the Wolf icon. 
  /// 1. Using the mouse cursor, sketch a rectangle geometry on the map to select a few of features in the USCities feature layer.
  /// 1. When you complete the sketch, the code in the "OnSketchCompleteAsync" callback in the IntermediateTableConstTool.cs file is invoked.
  /// 1. One record per selected feature will be added to the "CityDemographicsTable" table. The "Name" field of the table will be pre-populated with the "CITY_NAME" attribute of the selected USCities feature. At this point, you can open the CityDemographicsTable standalone table and check the newly added records.
  /// ### Advanced table construction tool
  /// 1. Click the "Advanced Table Construction Tool" to activate it.  This is the button with the Fire dragon icon. 
  /// 1. Activating this tool will display the "Options" pane for the Advanced Table Construction tool.
  /// ![UI](screenshots/Options.png)
  /// 1. Select the "USCities" layer in the "Select Layer" drop down. Select the "CITY_NAME" field in the "Select Field" dropdown.  Select "Name" in the "Select Table field" drop down.
  /// 1. Using the mouse cursor, sketch a rectangle geometry on the map to select a few of features in the USCities feature layer.
  /// 1. When you complete the sketch, the code in the "OnSketchCompleteAsync" callback in the AdvancedTableConstructionTool.cs file is invoked.
  /// 1. One record per selected feature will be added to the "CityDemographicsTable" table. The "Name" field of the table will be pre-populated with the "CITY_NAME" attribute of the selected USCities feature. At this point, you can open the CityDemographicsTable standalone table and check the newly added records.
  /// </remarks>
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("TableConstructionTool_Module"));
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

  }
}
