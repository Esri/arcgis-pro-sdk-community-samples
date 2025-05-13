/*

   Copyright 2025 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPToolRightClick
{
  internal class GPToolRightClick : Button
  {
    protected override void OnClick()
    {
      try
      {
        var toolInfo = FrameworkApplication.ContextMenuDataContext; //as ArcGIS.Desktop.GeoProcessing.ToolInfoViewModel;
                                                                    // internal class ToolInfo:
                                                                    // public string Name
                                                                    // public string Description
                                                                    // public string ToolType
                                                                    // public bool IsValid
                                                                    // public string ToolBoxName
                                                                    // public string FullPath
                                                                    // public bool IsSystem
                                                                    // public string toolName
        string name = (string)toolInfo.GetType().GetProperty("Name").GetValue(toolInfo, null);
        string description = (string)toolInfo.GetType().GetProperty("Description").GetValue(toolInfo, null);
        string toolType = (string)toolInfo.GetType().GetProperty("ToolType").GetValue(toolInfo, null);
        string toolBoxName = (string)toolInfo.GetType().GetProperty("ToolBoxName").GetValue(toolInfo, null);
        string fullPath = (string)toolInfo.GetType().GetProperty("FullPath").GetValue(toolInfo, null);
        string toolName = (string)toolInfo.GetType().GetProperty("toolName").GetValue(toolInfo, null);
        bool isValid = (bool)toolInfo.GetType().GetProperty("IsValid").GetValue(toolInfo, null);
        bool isSystem = (bool)toolInfo.GetType().GetProperty("IsSystem").GetValue(toolInfo, null);
        string nl = Environment.NewLine;
        MessageBox.Show($@"Type of toolInfo: {toolInfo.GetType().ToString()}{nl}"
          + $@"name: {name}{nl}"
          + $@"description: {description}{nl}"
          + $@"toolType: {toolType}{nl}"
          + $@"toolBoxName: {toolBoxName}{nl}"
          + $@"fullPath: {fullPath}{nl}"
          + $@"toolName: {toolName}{nl}"
          + $@"isValid: {isValid}{nl}"
          + $@"isSystem: {isSystem}{nl}");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
  }
}
