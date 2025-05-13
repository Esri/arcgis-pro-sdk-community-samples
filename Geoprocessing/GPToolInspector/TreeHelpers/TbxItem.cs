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
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Internal.KnowledgeGraph;
using ArcGIS.Desktop.Internal.Mapping.TOC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static GPToolInspector.TreeHelpers.TbxReader;

namespace GPToolInspector.TreeHelpers
{
  internal class TbxItem : TbxItemBase
  {
    private static TbxItem LazyloadTbxItem = null;

    public TbxItem(TbxItemBase parent, string tbxPath) : base(parent)
    {
      IsSelected = false;
      Path = tbxPath;
      Name = System.IO.Path.GetFileNameWithoutExtension(tbxPath);
      Alias = Name;
    }

    public TbxItem(TbxItemBase parent, string name, string alias, string tbxPath) : base (parent)
    {
      IsSelected = false;
      Path = tbxPath;
      Name = name;
      Alias = System.IO.Path.GetFileNameWithoutExtension(tbxPath);
    }

    public override TbxItemBase AddTbxItem(TbxItemBase parent, string name, string tbxPath, TbxItemBase parentTbxItem)
    {
      if (name == "<root>") return null;
      if (name.Contains('\\'))
      {
        // has children
        var parts = name.Split('\\');
        var category = parts[0];

        TbxItemBase item = parentTbxItem.Children.Find((i) => i.Name == category);
        var itemDidExist = item != null;
        item ??= new TbxItem(this, category, category, tbxPath);
        var remainderName = name.Substring(category.Length + 1);

        var tipOfTree = item.AddTbxItem(item, remainderName, tbxPath, item);
        
        if (!itemDidExist)
        {
          AddTbxItem(item);
        }
        return tipOfTree;
      }
      else
      {
        // last tbxItem node on branch
        var category = name;
        var item = new TbxItem(parentTbxItem, category, category, tbxPath);
        var existingItem = parentTbxItem.Children.Find((i) => i.Name == name);
        if (existingItem != null)
        {
          return item;
        }
        parentTbxItem.AddTbxItem(item);
        return item;
      }
    }

    public void AddTbxItem(string tbxPath)
    {
      var item = new TbxItem(this, tbxPath);
      AddTbxItem(item);
    }

		#region Child functions

		/// <summary>
		/// Used for Lazy loading of the ToolBox(es)
		/// First LeveL: Folders in 'toolboxes' folder containing toolbox.content files
		/// Second Level: toolbox.content file containing toolsets, 
		///               second level is only the '&lt;root&gt;' node and
		///               all root level toolsets (name split on '\')
		/// Third Level: all second level toolsets (name after split on '\')
		/// Last Level: all tools in the toolsets
		/// </summary>
		public override void LoadChildren()
    {
      if (HasToolSets)
      {
				// All levels except the first level are loaded here:
				// lazy load tools in toolList
				// use ToolListRootFolder to determine the root folder to process
				foreach (var children in ProcessHeader (ToolListRootFolder, ToolSets, CurrentFolder))
				{
          if (children.IsTool)
          {
            // Process tools here
            var itemToolPath = System.IO.Path.Combine(this.Path, children.Path);
            var itemTool = new TbxItemTool(this, this.Alias, itemToolPath);
            AddTbxItem(itemTool);
          }
          else
          {
						var tbxItem = new TbxItem(this, this.Path);
            var parts = children.Name.Split('\\');
            var lastPart = parts[^1];
            tbxItem.ToolSets = this.ToolSets;
            tbxItem.Alias = this.Alias;
						tbxItem.Name = children.Name;
            tbxItem.Title = lastPart;
            tbxItem.CurrentFolder = children.Path;
            AddTbxItem(tbxItem);
					}
				}
				return;
			}
			// First LeveL: Folders
			foreach (var path in Directory.EnumerateDirectories(this.Path))
      {
        var tbxItem = new TbxItem(this, path);
        var toolBoxHeader = ReadToolBoxHeader(path);
				tbxItem.ToolSets = toolBoxHeader.ToolSets;
        tbxItem.Alias = toolBoxHeader.Alias;
        tbxItem.Name = toolBoxHeader.Title;
        tbxItem.Title = toolBoxHeader.Title;
        AddTbxItem(tbxItem);
      }
    }

    #endregion

  }
}
