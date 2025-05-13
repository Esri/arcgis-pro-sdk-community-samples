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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GPToolInspector
{
  using LocalResources = ArcGIS.Desktop.Internal.GeoProcessing.LocalResources;

  internal class ToolInfoUtils
  {
  }

  internal class ToolInfoViewModelBase : ToolInfo, INotifyPropertyChanged
  {
    public enum KindType
    {
      Unknown,
      SearchResult,
      Favorite,
      MyFavorite,
      HistoryRecent,
      Galley,
      Suggested
    }
    protected ToolInfoViewModelBase(XElement it, in KindType kind) : base(it) { Kind = kind; }

    public event PropertyChangedEventHandler PropertyChanged;
    static protected void notify<T>(System.Linq.Expressions.Expression<System.Func<T>> expr)
    {
      var m = expr.Body as MemberExpression;
      if (m.NodeType == ExpressionType.MemberAccess)
      {
        var t = (m.Expression as ConstantExpression).Value as ToolInfoViewModelBase;
        if (t.PropertyChanged != null)
          t.PropertyChanged(t, new PropertyChangedEventArgs(m.Member.Name));
      }
    }
    protected bool _is_selected;
    public bool IsSelected
    {
      get { return _is_selected; }
      set
      {
        if (value == _is_selected) return;
        _is_selected = value;
        notify(() => IsSelected);
      }
    }
    public KindType Kind { get; protected set; }

    async void validate_license()
    {
      _isLocked = await SysToolsUtil.isLocked(string.Format("{0}.{1}", toolboxalias, toolName));
      if (_isLocked.Value == true)
      {
        notify(() => HasLock);
        notify(() => ShowLockType);
      }
    }
    bool? _isLocked;
    public bool HasLock
    {
      get
      {
        if (_isLocked.HasValue) return _isLocked.Value;
        if (!IsSystem || !executable)
        {
          _isLocked = false;
          return false;
        }
        validate_license();
        return false;
      }
    }

    public bool IsSeparator { get; init; }

    public string SeparatorLabel { get; init; }

    public bool IsPortal => SysToolsUtil.isPortal(this);

    public bool ShowCoinImage => this.has_attribute("credits");

    public bool ShowBigDataIcon => toolboxalias == "geoanalytics";

    public bool ShowLockType => HasLock;

    public bool ShowCloudType => IsPortal;

    public bool ShowSparkIcon => this.has_attribute("spark");

    public bool ShowGPULeverageIcon => this.has_attribute("GPU");

    public bool ShowParallelProcessingIcon => this.has_attribute("mCPU") || this.has_attribute("spark"); //toolboxalias == "geoanalytics" || toolboxalias == "ra" || toolboxalias == "gapro";

    internal string highlights;
    internal string payload;
  }
  internal sealed class ToolInfoViewModel : ToolInfoViewModelBase
  {
    // not used
    //static Lazy<System.Windows.Controls.ContextMenu> _menu = new Lazy<System.Windows.Controls.ContextMenu>(() => FrameworkApplication.CreateContextMenu("esri_geoprocessing_SystemToolMenu"));
    //static Lazy<System.Windows.Controls.ContextMenu> _menu_script = new Lazy<System.Windows.Controls.ContextMenu>(() => FrameworkApplication.CreateContextMenu("esri_geoprocessing_SystemScriptToolMenu"));
    //static Lazy<System.Windows.Controls.ContextMenu> _menu_python_tool = new Lazy<System.Windows.Controls.ContextMenu>(() => FrameworkApplication.CreateContextMenu("esri_geoprocessing_PythonToolMenu"));

    public bool IsExecutable => executable;
    public bool IsEnabled => true;
    private bool _is_expanded;
    public bool IsExpanded
    {
      get { return _is_expanded; }
      set
      {
        if (value == _is_expanded) return;
        _is_expanded = value;
        notify(() => IsExpanded);
      }
    }

    bool _isVisible = true;
    public bool IsVisible
    {
      get => _isVisible;
      set
      {
        if (_isVisible != value)
        {
          _isVisible = value;
          notify(() => IsVisible);
        }
      }
    }

    public void Reload()
    {
      if (!executable)
      {
        _list = null;
        notify(() => Items);
      }
    }
    static XElement portal_converter(XElement e, XElement parent)
    {
      var ret = e;//new XElement(e);
      var dna = ret.Attribute("displayname");
      if (dna == null)
        return e; //no convertion
      ret.AddFirst(new XElement("displayname", dna.Value));
      if (parent != null)
      {
        var alias = parent.Attribute(parent.Name.LocalName == "toolbox" ? "alias" : "toolboxalias");
        System.Diagnostics.Debug.Assert(alias != null);
        if (alias != null)
          ret.SetAttributeValue("toolboxalias", parent.Attribute(parent.Name.LocalName == "toolbox" ? "alias" : "toolboxalias").Value);
      }
      if (ret.Name.LocalName == "tool")
      {
        var ref_uri = ret.Attribute("ref_uri");
        System.Diagnostics.Debug.Assert(ref_uri != null);
        var path = ref_uri?.Value;
        Uri uri;
        if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uri))
        {
          var name = uri.Segments.Last();
          ret.SetAttributeValue("name", name);
          string full_path = toolInfoExt.uri2file(path);
          ret.Add(new XElement("fullpath", full_path));
        }
        ref_uri.Remove();
      }
      dna.Remove();
      return ret;
    }

    public static ToolInfoViewModel create_Separator(string label)
    {
      return new ToolInfoViewModel(null, KindType.Unknown) { IsSeparator = true, SeparatorLabel = label };
    }
    public static ToolInfoViewModel create_VM(XElement it, ToolInfoViewModel parent)
    {
      var vm = new ToolInfoViewModel(it, parent?.Kind ?? KindType.Unknown) { IsSeparator = false };
      if (!vm.IsValid)
        return vm;
      //vm.OnClick = new RelayCommand(() => doCllick(vm), () => true, false, false);

      bool isSystem;
      bool isPortal = false;
      string overlay_style = string.Empty;
      if (parent == null)
      {
        isSystem = SysToolsUtil.isSystem(vm);
        if (!isSystem)
          isPortal = SysToolsUtil.isPortal(vm);
      }
      else
      {
        overlay_style = parent.overlay_style;
        //if (!string.IsNullOrEmpty(parent.toolboxalias))
        //  vm._node.SetAttributeValue("toolboxalias",parent.toolboxalias);
        isSystem = SysToolsUtil.isSystem(parent);
        if (!isSystem)
          isPortal = SysToolsUtil.isPortal(parent);
      }

      if (!string.IsNullOrEmpty(overlay_style))
        it.SetAttributeValue("overlay_style", overlay_style);

      //if (isPortal)
      vm._node = portal_converter(it, parent?._node);

      if (parent != null && !isSystem && !isPortal)
      {
        var parent_path = parent._node.Element("fullpath");
        if (vm._node.Element("fullpath") == null && parent_path != null)
        {
          var path = System.IO.Path.Combine(parent_path.Value, toolInfoExt.getName(it));
          vm._node.Add(new XElement("fullpath", path));
        }
      }
      return vm;
    }
    private ToolInfoViewModel(XElement it, KindType kind) : base(it, kind) { }

    internal List<ToolInfoViewModel> _list = null;
    public IReadOnlyList<ToolInfoViewModel> Items
    {
      get
      {
        if (_list != null)
          return _list;

        if (_node == null)
          return _list;

        if (_node.Name.LocalName == "tool")
        {
          _list = new List<ToolInfoViewModel>(); //end
          return _list;
        }
        var ts = _node.Elements("toolset").Select(it => create_VM(it, this)).OrderBy(it => it.Name);
        _list = ts.Concat(
              _node.Elements("tool").Select(it => create_VM(it, this)).OrderBy(it => it.Name)
              ).ToList();
        return _list;
      }
    }
    public static IEnumerable<T> get_flat_tools<T>(ToolInfoViewModel ti, ToolInfoViewModel parent, Func<ToolInfoViewModel, ToolInfoViewModel, T> fn)
    {
      if (ti.executable)
        yield return fn(ti, parent);
      foreach (var it in ti.Items.SelectMany(it => get_flat_tools(it, ti, fn)))
        yield return it;
    }

    public System.Windows.Controls.ContextMenu GetMenu
    {
      get
      {
        if (!this.executable)
          return null;
        if (ToolType == "script")
          return _menu_script.Value;
        if (ToolType == "pythonscript")
          return _menu_python_tool.Value;
        return _menu.Value;
      }
    }
  }

}
