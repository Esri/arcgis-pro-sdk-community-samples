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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static GPToolInspector.TreeHelpers.TbxReader;

namespace GPToolInspector.TreeHelpers
{
  internal class TbxItemTool : TbxItemBase
  {
    public TbxItemTool(TbxItemBase parent, string category, string tbxPath) : base(parent) 
    {
      CurrentFolder = string.Empty;
      IsSelected = false;
      Path = tbxPath;
      Name = $@"{System.IO.Path.GetFileNameWithoutExtension(tbxPath)}";
      Children.Clear();
      var (Ok, ErrorMessage) = TbxToolInfo.ReadToolContent(tbxPath, out TbxToolInfo);
      if (!Ok)
      {
        MessageBox.Show(ErrorMessage);
      }
      else
      {
        TbxToolInfo.FileName = Name;
        Title = TbxToolInfo.displayname;
        IsScriptingTool = TbxToolInfo.type == "ScriptTool";
        if (parent == null)
        {
          MessageBox.Show($@"The parent for this tool is missing: {Title} {Name}");
          return;
        }
        TbxToolInfo.Category = category;
      }
    }

    public override bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        SetProperty(ref _isSelected, value);
        if (!value) return;
        // popup the tool dialog
        var toolDialog = new ToolDialog();
        (toolDialog.DataContext as ToolDialogViewModel).TbxToolInfo = TbxToolInfo;
        toolDialog.ShowDialog();
      }
    }

    public string SearchTitle
    {
      get
      {
        var searchTitle = Parent != null ? Parent.Name : "no title";
        return searchTitle;
      }
    }

    public override ImageSource ToolImgSrc
    {
      get
      {
        if (IsScriptingTool)
        {
          return Application.Current.Resources["GeoprocessingScript32"] as ImageSource;
        }
        else {
          return Application.Current.Resources["GeoprocessingTool32"] as ImageSource;
        }
      }
    }

  }
}
