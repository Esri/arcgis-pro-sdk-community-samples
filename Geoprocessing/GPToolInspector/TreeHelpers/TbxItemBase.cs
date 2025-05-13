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
using ArcGIS.Desktop.Framework.Contracts;
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
  /// <summary>
  /// Base class for all items in the toolbox
  /// </summary>
  public class TbxItemBase : PropertyChangedBase
  {
    private static TbxItemBase LazyloadChild = new(null);

    /// <summary>
    /// Constructor for the TbxItemBase
    /// </summary>
    /// <param name="parent">Parent TbxItem</param>
		public TbxItemBase(TbxItemBase parent)
    {
      IsSelected = false;
			Parent = parent;
			// do the 'lazy' load for all items
			// 'load' is triggered only when item is selected
			Children = [LazyloadChild];
    }

    #region Common Properties

    /// <summary>
    /// ToolBox Info for the current selected tool
    /// </summary>
    public TbxToolInfo TbxToolInfo;


    private bool _IsScriptingTool;

    public bool IsScriptingTool
    {
      get { return _IsScriptingTool; }
      set
      {
        SetProperty(ref _IsScriptingTool, value);
      }
    }

    private bool _isExpanded;

    /// <summary>
    /// Is the item expanded
    /// </summary>
    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        SetProperty(ref _isExpanded, value);
        if (HasLazyloadChild)
        {
          Children.Remove(LazyloadChild);
          LoadChildren();
        }
      }
    }

    internal bool _isSelected;
    /// <summary>
    /// Is the item selected
    /// </summary>
    public virtual bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        SetProperty(ref _isSelected, value);
        if (!value) return;
        System.Diagnostics.Trace.WriteLine(this.Path);
      }
    }

    internal TbxItemBase _Parent;
    /// <summary>
    /// Parent TbxItem
    /// </summary>
    public TbxItemBase Parent
    {
      get { return _Parent; }
      set
      {
        SetProperty(ref _Parent, value);
      }
    }

    private string _name;
    /// <summary>
    /// Name of the item
    /// </summary>
    public string Name
    {
      get { return _name; }
      set
      {
        SetProperty(ref _name, value);
      }
    }

    private string _title;
    /// <summary>
    /// Title of the item
    /// </summary>
    public string Title
    {
      get { return _title; }
      set
      {
        SetProperty(ref _title, value);
      }
    }

    private string _Alias;
    /// <summary>
    /// Alias of the toolbox item
    /// </summary>
    public string Alias
    {
      get { return _Alias; }
      set
      {
        SetProperty(ref _Alias, value);
      }
    }

    private string _Path;
    /// <summary>
    /// Path of the toolbox item
    /// </summary>
    public string Path
    {
      get { return _Path; }
      set
      {
        var ext = System.IO.Path.GetExtension(value);
        if (ext != null && ext.Length > 1)
        {
          Extension = ext.Substring(1).ToLower();
        }
        SetProperty(ref _Path, value);
      }
    }

    private string _Extension;
    /// <summary>
    /// Extension of the toolbox item
    /// </summary>
    public string Extension
    {
      get { return _Extension; }
      set
      {
        SetProperty(ref _Extension, value);
      }
    }

    /// <summary>
    /// Default image source for the toolbox item
    /// </summary>
    public virtual ImageSource ToolImgSrc => Application.Current.Resources["GeoprocessingTool32"] as ImageSource;

    private string _CurrentFolder;

    /// <summary>
    /// Current active folder string
    /// </summary>
		public string CurrentFolder {
			get { return _CurrentFolder; }
			set
			{
				SetProperty(ref _CurrentFolder, value);
			}
		}

		private List<TbxItemBase> _children;
    /// <summary>
    /// Children of the toolbox item
    /// </summary>
    public List<TbxItemBase> Children
    {
      get { return _children; }
      set
      {
        SetProperty(ref _children, value);
      }
    }

		private IEnumerable<ToolSet> _ToolSet;

    /// <summary>
    /// True if the toolbox has toolsets
    /// </summary>
    public bool HasToolSets => _ToolSet != null && _ToolSet.Count() > 0;

		/// <summary>
		/// For the Lazyload the tool list contains the list of tools in the toolbox
		/// </summary>
		internal IEnumerable<ToolSet> ToolSets
		{
			get { return _ToolSet; }
			set
			{
				SetProperty(ref _ToolSet, value);
			}
		}

    private string _ToolListRootFolder = string.Empty;

		/// <summary>
		/// Root folder applies for the current ToolList
		/// </summary>
		public string ToolListRootFolder
		{
			get { return _ToolListRootFolder; }
			set
			{
				SetProperty(ref _ToolListRootFolder, value);
			}
		}

		#endregion Common Properties

		#region Child functions

		public virtual TbxItemBase AddTbxItem(TbxItemBase parent, string name, string tbxPath, TbxItemBase parentTbxItem)
    {
      return null;
    }

    public virtual void AddTbxItem(TbxItemBase tbxItembase)
    {
      Children.Add(tbxItembase);
    }

    public virtual void InsertTbxItem(TbxItemBase tbxItembase)
    {
      Children.Insert(0, tbxItembase);
    }

    public virtual void LoadChildren() { }

    /// <summary>
    /// Returns true if this object's Children have not yet been populated.
    /// </summary>
    public bool HasLazyloadChild
    {
      get { return this.Children.Count == 1 && this.Children[0] == LazyloadChild; }
    }

    #endregion Child functions
  }
}
