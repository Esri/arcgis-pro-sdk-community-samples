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
using ArcGIS.Desktop.GeoProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GPToolInspector
{

  static class toolInfoExt
  {
    public static string getName(XElement e)
    {
      if (e.Attribute("name")?.Value is string name) return name;
      if (e.Element("displayname")?.Value is string dname) return dname;
      throw new Exception("invalid xml node");
    }

    public static string getDisplayName(XElement e)
    {
      if (e == null)
        return string.Empty;
      if (e.Element("displayname")?.Value is string dn) return dn;
      if (e.Attribute("displayname")?.Value is string dn2) return dn2;
      if (e.Attribute("name")?.Value is string name) return name;
      if (e.Element("fullpath")?.Value is string path) return System.IO.Path.GetFileName(path);
      System.Diagnostics.Debug.Assert(false);
      return string.Empty;
    }

    /// <summary>
    /// "toolbox\\toolset\\tool"
    /// or
    ///  "toolbox\\tool" execute path
    /// or 
    ///  "toolboxalias.tool" short execute path
    /// </summary>
    public static string getFullPath(XElement e)
    {
      //if <fullpath> is null check system alias
      try
      {
        if (e == null) return string.Empty;
        var node_type = e.Name.LocalName;
        var x = e.Element("fullpath");
        switch (node_type)
        {
          case "toolbox":
            {
              if (x != null)
                return x.Value;
              var alias = e.Attribute("alias");
              System.Diagnostics.Debug.Assert(alias != null);
              return alias == null ? string.Empty : SysToolsUtil.AliasToPath(alias.Value);
            }
          case "toolset":
            if (!x.HasElements)
              return x.Value;
            return string.Empty;
          default:
            {
              if (x != null)
              {
                if (!x.HasElements)
                  return x.Value;
                //System.IO.TextWriter.
                var m = x.ToString(SaveOptions.DisableFormatting);
                m = m.Substring(10, m.Length - 21);
                return m;
              }
              if (e.Attribute("toolboxalias")?.Value is string alias)
              {
                var toolbox_path = SysToolsUtil.AliasToPath(alias);
                if (!string.IsNullOrEmpty(toolbox_path))
                  return System.IO.Path.Combine(toolbox_path, getName(e));
                return string.Format("{0}.{1}", alias, getName(e));
              }
              return string.Empty;
            }
        }
      }
      catch
      {
        return string.Empty;
      }
    }
    public static string getExecutePathShort(this ToolInfo ti, bool for_python = false)
    {
      System.Diagnostics.Debug.Assert(ti.executable);
      var toolbox_alias = string.IsNullOrEmpty(ti.toolboxalias) ? ti.ToolBoxName : ti.toolboxalias;
      return string.Format("{0}.{1}", for_python ? SysToolsUtil.AliasToArcpyModuleAlias(toolbox_alias) : toolbox_alias, ti.toolName);
    }

    public static string getExecutePath(this ToolInfo ti)
    {
      try
      {
        System.Diagnostics.Debug.Assert(ti.executable);
        //for server tools exec path = full path
        if (ti.ToolType == "server")
          return getFullPath(ti.Node);
        if (ti.IsSystem)
          return getExecutePathShort(ti);

        var tb_path = getToolboxPath(ti);
        if (string.IsNullOrEmpty(tb_path))
          return getExecutePathShort(ti);
        return System.IO.Path.Combine(tb_path, ti.toolName);
      }
      catch { return string.Empty; }
    }

    public static string getToolboxPath(this ToolInfo ti)
    {
      if (ti.ToolType == "toolbox")
        return getFullPath(ti.Node);
      var path = getFullPath(ti.Node);
      if (ti.executable && path == getExecutePathShort(ti))
        return string.Empty;

      return getToolboxPath(path);
    }

    public static string getToolboxPath(string path)
    {
      try
      {
        if (string.IsNullOrEmpty(path))
          return path;
        if (path[0] == '<')
        {
          //TODO
          System.Diagnostics.Debug.Assert(false);
          return string.Empty;
        }
        var str_l = path.ToLower();
        if (str_l.EndsWith(".tbx") || str_l.EndsWith(".pyt") || str_l.EndsWith(".atbx"))
          return path;
        int idx = str_l.LastIndexOf(".tbx\\");
        if (idx > 0)
          return path.Substring(0, idx + 4);
        idx = str_l.LastIndexOf(".pyt\\");
        if (idx > 0)
          return path.Substring(0, idx + 4);
        idx = str_l.LastIndexOf(".atbx\\");
        if (idx > 0)
          return path.Substring(0, idx + 5);

        idx = str_l.LastIndexOf(".gdb\\");
        if (idx > 0)
        {
          var idx2 = path.IndexOf('\\', idx + 6);
          if (idx2 > 0)
            return path.Substring(0, idx2);
          return path;
        }
        idx = str_l.LastIndexOf(".ags\\");
        if (idx > 0)
          return path.Substring(0, path.IndexOf('\\', idx + 6));
        idx = str_l.LastIndexOf(".sde\\");
        if (idx > 0)
        {
          var idx2 = path.IndexOf('\\', idx + 6);
          if (idx2 > 0)
            return path.Substring(0, idx2);
          return path;
        }
        return string.Empty;
      }
      catch
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// returns toolset name
    /// </summary>
    public static string getCategory(this ToolInfo ti)
    {
      var node_type = ti.Node.Name.LocalName;
      if (node_type != "tool")
        return string.Empty;
      var x = ti.Node.Element("fullpath");
      if (x == null)
        return string.Empty;
      string path = x.Value;
      string tb_path = getToolboxPath(path);
      if (string.IsNullOrEmpty(tb_path))
        return string.Empty;
      int n = tb_path.Length;
      try
      {
        return System.IO.Path.GetDirectoryName(path.Substring(n).TrimStart('\\'));
      }
      catch
      {
        return string.Empty;
      }
    }

    public static void update_description(this ToolInfo ti, string desc)
    {
      var d = ti.Node.Element("description");
      if (d == null)
        ti.Node.Add(new XElement("description", desc));
      else
        d.Value = desc;
    }
    static internal string description(this ToolInfo ti)
    {
      if (ti?.Node == null) return string.Empty;
      var d = ti.Node.Element("description");
      return d == null ? string.Empty : d.Value;
    }

    /// <summary>
    /// check permition, type = [rename, delete, copy, create, displayname]
    /// </summary>
    public static bool limit(this ToolInfo ti, string type)
    {
      if (ti?.Node == null) return true; //invalid
      var all_limits = ti.Node.Attribute("limits")?.Value;
      //if no attributes, allow all
      if (string.IsNullOrEmpty(all_limits)) return false;
      return all_limits.Contains(type);
    }

    public static bool has_attribute(this ToolInfo ti, string attribute)
    {
      var all_attributes = ti?.Node?.Attribute("attr")?.Value;
      if (all_attributes == null) return false;
      return $",{all_attributes},".Contains($",{attribute},") == true;
    }
    public static HashSet<string> all_attribute(this ToolInfo ti)
    {
      HashSet<string> ret = new();
      var all_attributes = ti?.Node?.Attribute("attr")?.Value;
      if (!string.IsNullOrEmpty(all_attributes))
      {
        foreach (var a in all_attributes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
          ret.Add(a);
      }
      return ret;
    }

    public static string uri2file(string uri_str)
    {
      Uri uri;
      if (Uri.TryCreate(uri_str, UriKind.RelativeOrAbsolute, out uri))
      {
        var full_path = string.Empty;
        if (uri.Scheme == "resources")
          full_path = string.Format("{0}/Resources{1}", SysToolsUtil.InstallPath, uri.LocalPath);
        else
          full_path = uri.LocalPath;
        return System.IO.Path.GetFullPath(full_path);
      }
      return uri_str;
    }

    internal static string get_image(this ToolInfo ti)
    {
      var node = ti?.Node;
      if (node == null) return null;
      if (node.Attribute("icon_path")?.Value is string icon_path)
        return icon_path;
      if (node.Attribute("icon")?.Value is string full_path)
      {
        full_path = toolInfoExt.uri2file(full_path);
        if (ArcGIS.Desktop.Framework.FrameworkApplication.ApplicationTheme == ArcGIS.Desktop.Framework.ApplicationTheme.Dark)
        {
          var ext = System.IO.Path.GetExtension(full_path);
          var path_dark = full_path.Substring(0, full_path.Length - ext.Length);
          path_dark += "_dark";
          path_dark = System.IO.Path.ChangeExtension(path_dark, ext);
          if (System.IO.File.Exists(path_dark))
            full_path = path_dark;
        }
        return full_path;
      }

      if (ti.IsSystem)
      {
        if (ti.executable)
          return SysToolsUtil.GetToolImagePath(node.Attribute("toolboxalias").Value, ti.toolName, false);
        return SysToolsUtil.GetToolboxImagePath(node.Attribute("alias").Value, false);
      }
      return null;
    }

    static public ToolInfo Clone(this ToolInfo ti)
    {
      if (ti?.Node is null)
        return new ToolInfo(null);
      return new ToolInfo(new XElement(ti.Node));
    }
  }

  internal class ToolInfo
  {
    protected XElement _node;

    /// <summary>
    /// Display Name
    /// </summary>
    public string Name => toolInfoExt.getDisplayName(_node);

    public string Description
    {
      get
      {
        string str = this.description();
#if DEBUG
        //if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        //{
        //  if (_node != null && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
        //    str = $"{str}\n\n--- Debug XML---\n{_node.ToString()}\n{Internal.GeoProcessing.embeddings.make_source(this)}";
        //}
#endif
        return str;
      }
    }
    /// <summary>
    /// return ["function", "model", "script", "custom", "server", "pythonscript", "task","toolbox", "toolset"]
    /// </summary>
    public string ToolType
    {
      get
      {
        if (_node == null) return "unknown";
        var str = _node.Name.LocalName;//ToString();
        if (str == "tool")
          return _node.Attribute("type").Value;
        return str;
      }
    }

    private bool? _isValid;
    public bool IsValid
    {
      get
      {
        if (_isValid.HasValue) return _isValid.Value;
        _isValid = _node == null ? false : _node.Attribute("failed") != null ? false : true;
        return _isValid.Value;
      }
    }

    public bool HasImage => !string.IsNullOrEmpty(ImageSource);
    public string ImageSource => this.get_image();

    /// <summary>
    ///override default tool icon (16x16). default - null
    /// </summary>
    public System.Windows.Media.DrawingImage TypeIcon { get; internal set; } = null;

    /// <summary>
    /// display toolbox name
    /// </summary>
    public string ToolBoxName
    {
      get
      {
        if (_node == null)
          return string.Empty;
        try
        {
          if (IsSystem)
            return SysToolsUtil.AliasToDisplayName(this.toolboxalias);
          if (SysToolsUtil.isPortalAlias(this.toolboxalias))
            return SysToolsUtil._portal_aliases[this.toolboxalias];

          var parent = _node.Parent;
          while (parent?.Parent != null)
            parent = parent.Parent;
          var toolbox_name = parent?.Element("displayname")?.Value;
          return toolbox_name == null ? this.toolboxalias : toolbox_name;
        }
        catch { return this.toolboxalias; }
      }
    }

    /// <summary>
    /// true if tool, false - toolbox, toolset
    /// </summary>
    internal bool executable { get { if (!IsValid) return false; else return _node == null ? false : _node.Name.LocalName == "tool"; } }

    public string FullPath => toolInfoExt.getFullPath(_node);

    internal string toolboxalias => _node?.Attribute("toolboxalias")?.Value ?? string.Empty;

    bool? _isSystem;
    public bool IsSystem
    {
      get
      {
        if (!_isSystem.HasValue)
          _isSystem = SysToolsUtil.isSystem(this);
        return _isSystem.Value;
      }
    }
    //internal void setSystem()
    /// <summary>
    /// real name
    /// </summary>
    public string toolName
    {
      get
      {
        if (_node == null) // && this is FavoritesToolInfoViewModel)
          return string.Empty;
        return _node.Attribute("name").Value;
      }
    }

    internal XElement Node => _node;

    //protected ToolInfo() {}
    public ToolInfo(XElement element) => _node = element;
    public string overlay_style { get { if (!IsValid) return string.Empty; var a = _node.Attribute("overlay_style"); return a == null ? string.Empty : a.Value; } }

    System.Lazy<HashSet<string>> _keys = null;
    public ISet<string> keys
    {
      get
      {
        if (_keys == null)
        {
          _keys = new System.Lazy<HashSet<string>>(() =>
          {
            var e = _node.Elements("key");
            if (e != null)
              return new HashSet<string>(e.Select(it => it.Value));
            else
              return new HashSet<string>();
          });
        }
        return _keys.Value;
      }
    }
    /*
    public override bool Equals(object obj)
    {
      ToolInfo o = obj as ToolInfo;
      if (o == null) return false;
      if (this._node == null) return false;

      if (o.executable && this.executable)
        return o.FullPath == this.FullPath;//(o.toolName == this.toolName && o.ParentPath == this.ParentPath);
      return o.ToString() == this.ToString();
    }

    public override int GetHashCode()
    {
      if (this._node != null)
      {
        if (this.executable)
          return this.FullPath.GetHashCode() ^ this.FullPath.GetHashCode();
        return this._node.GetHashCode();
      }
      return 0;
    }*/
    public override string ToString() => _node == null ? base.ToString() : _node.ToString();
  }

  static class SysToolsUtil
  {
    static public string InstallPath { get; } = System.IO.Path.GetDirectoryName(       // <InstallPath>/bin
                                                  System.IO.Path.GetDirectoryName(     // <InstallPath>/bin/Extensions
                                                    System.IO.Path.GetDirectoryName(   // <InstallPath>/bin/Extensions/GeoProcessing
                                                      System.IO.Path.GetDirectoryName( // <InstallPath>/bin/Extensions/GeoProcessing/ArcGIS.Desktop.GeoProcessing.dll
                                                        System.Reflection.Assembly.GetCallingAssembly().Location
                                                 ))));

    public static string GetRegistryKeyString(string regKeyName)
    {
      string[] keys = { Microsoft.Win32.Registry.CurrentUser.Name, Microsoft.Win32.Registry.LocalMachine.Name };
      foreach (var key in keys)
      {
        try
        {
          var keyName = System.IO.Path.Combine(key, $@"Software\ESRI\{ArcGIS.Desktop.Internal.Framework.FrameworkApplication.ProductID}");
          var keyValue = Microsoft.Win32.Registry.GetValue(keyName, regKeyName, string.Empty)?.ToString();
          if (!string.IsNullOrEmpty(keyValue))
          {
            return keyValue;
          }
        }
        catch { }
      }
      return String.Empty;
    }

    public static IEnumerable<string> get_site_packages()
    {
      string env_path = null;

      var python_path = GetRegistryKeyString("PythonCondaRoot");
      var conda_env = GetRegistryKeyString("PythonCondaEnv");

      if (!string.IsNullOrEmpty(conda_env))
      {
        try
        {
          if (System.IO.Directory.Exists(conda_env))
          {
            env_path = conda_env;
          }
          else
          {
            var global_env_path = System.IO.Path.Combine(python_path, "envs", conda_env);
            var local_env_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ESRI\conda" + conda_env;
            if (System.IO.Directory.Exists(global_env_path))
            {
              env_path = global_env_path;
            }
            else if (System.IO.Directory.Exists(local_env_path))
            {
              env_path = local_env_path;
            }
          }
        }
        catch { }
      }
      if (string.IsNullOrEmpty(env_path) || !System.IO.Directory.Exists(env_path))
        env_path = System.IO.Path.Combine(InstallPath, @"bin\Python\envs\arcgispro-py3");

      string site_packages_path = System.IO.Path.Combine(env_path, "Lib", "site-packages");

      try
      {
        return System.IO.Directory.EnumerateDirectories(site_packages_path).Where(it =>
        {
          try
          {
            return System.IO.Directory.Exists(System.IO.Path.Combine(it, "esri", "toolboxes"));
          }
          catch { return false; }
        }).Select(it => System.IO.Path.Combine(it, "esri", "toolboxes"));
      }
      catch { return Enumerable.Empty<string>(); }
    }

    public static IEnumerable<string> GetAddinToolboxes()
    {
      try
      {
        var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ESRI", ArcGIS.Desktop.Internal.Framework.FrameworkApplication.ProductID, "Toolboxes");
        return System.IO.Directory.EnumerateDirectories(path).Where(it =>
        {
          try
          {
            return System.IO.Directory.Exists(System.IO.Path.Combine(it, "toolboxes"));
          }
          catch { return false; }
        }).Select(it => System.IO.Path.Combine(it, "toolboxes"));
      }
      catch
      {
        return Enumerable.Empty<string>();
      }
    }

    public static string gpHelpPathEN { get; } = System.IO.Path.Combine(InstallPath, "Resources", "Help", "gp");

    static System.Lazy<string> _gpHelpPath = new Lazy<string>(() =>
    {
      foreach (var lng in new string[]{ System.Globalization.CultureInfo.CurrentUICulture.Name,
                                         System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName})
      {
        try
        {
          var path = System.IO.Path.Combine(InstallPath, "Resources", "Help", lng, "gp");
          if (System.IO.Directory.Exists(path))
            return path;
        }
        catch { }
      }
      return gpHelpPathEN;
    });
    public static string gpHelpPath => _gpHelpPath.Value;
    public static string gpImagesPath { get; } = System.IO.Path.Combine(InstallPath, "Resources", "ArcToolBox", "Images");//is icon localizable???
    public static string AliasToPath(string alias)
    {
      try
      {
        string[] pair;
        if (!_is_cache0loaded.Value)
          return string.Empty;
        if (!string.IsNullOrEmpty(alias) && _toolboxes.TryGetValue(alias, out pair))
          return pair[0];
        return string.Empty;
      }
      catch { return string.Empty; }
    }

    internal static string shortcat2path(string tool_name)
    {
      try
      {
        if (tool_name[0] == '@') return tool_name;
        //handle aliases
        string tbx_path = null;
        string name = null;
        if (tool_name.IndexOf('.') > 0)
        {
          var parts = tool_name.Split('.');
          tbx_path = SysToolsUtil.AliasToPath(parts[0]);
          name = parts[1];
        }
        else if (tool_name.IndexOf('_') > 0)
        {
          var parts = tool_name.Split('_');
          tbx_path = SysToolsUtil.AliasToPath(parts[1]);
          name = parts[0];
          System.Diagnostics.Debug.WriteLine($"\nObsolete GP tool naming convention. Consider the change: '{tool_name}' to '{parts[1]}.{parts[0]}'\n");
        }
        if (string.IsNullOrEmpty(tbx_path))
          return tool_name;
        return System.IO.Path.Combine(tbx_path, name);
      }
      catch { return tool_name; }
    }

    /// <summary>
    /// to reduce System.IO.File.Exists()
    /// </summary>
    static System.Collections.Concurrent.ConcurrentDictionary<string, bool> _image_file_exists = new();
    static private string to_image_file(string key)
    {
      try
      {
        var path = System.IO.Path.Combine(gpImagesPath, key);
        return _image_file_exists.GetOrAdd(key, static (k, p) => System.IO.File.Exists(p), path) ? path : null;
      }
      catch { return null; }
    }
    public static string GetToolImagePath(string toolbox_alias, string toolname, bool small = true)
    {
      if (small) return null;
      if (!isSystemAlias(toolbox_alias))
        return null;
      string path;
      if (ArcGIS.Desktop.Framework.FrameworkApplication.ApplicationTheme == ArcGIS.Desktop.Framework.ApplicationTheme.Dark)
      {
        path = to_image_file($"GeoprocessingTool_{toolname}_{toolbox_alias}_dark.png");
        if (!string.IsNullOrEmpty(path))
          return path;
      }
      path = to_image_file($"GeoprocessingTool_{toolname}_{toolbox_alias}.png");
      if (!string.IsNullOrEmpty(path))
        return path;
      return GetToolboxImagePath(toolbox_alias, small);
    }
    public static string GetToolboxImagePath(string toolbox_alias, bool small = true)
    {
      if (small) return null;
      if (!isSystemAlias(toolbox_alias))
        return null;
      if (ArcGIS.Desktop.Framework.FrameworkApplication.ApplicationTheme == ArcGIS.Desktop.Framework.ApplicationTheme.Dark)
      {
        var ret = to_image_file($"GeoprocessingToolbox_{toolbox_alias}_dark.png");
        if (!string.IsNullOrEmpty(ret))
          return ret;
      }
      return to_image_file($"GeoprocessingToolbox_{toolbox_alias}.png");
    }

    static public bool isSystem(ToolInfo ti)
    {
      if (ti == null || !ti.IsValid) return false;
      if (ti.executable)
      {
        var b = SysToolsUtil.isSystemAlias(ti.toolboxalias);
        return (b && ti.Node.Element("fullpath") == null);
      }
      if (ti.Node.Name.LocalName != "toolbox")
        return false;
      if (ti.Node.Element("fullpath") != null)
        return false;

      //ver < 1.2
      var sys_attr = ti.Node.Attribute("system");
      if (sys_attr != null)
        return sys_attr.Value == "true";
      var alias = ti.Node.Attribute("alias");
      if (alias == null)
        return false;
      return SysToolsUtil.isSystemAlias(alias.Value);
    }

    static public bool isSystemAlias(string alias)
    {
      if (string.IsNullOrEmpty(alias))
        return false;
      if (_is_cache0loaded.Value && _toolboxes.ContainsKey(alias))
        return true;
      return false;
    }
    static public void register_alias_as_system(string alias, string display_name, string path)
    {
      if (_is_cache0loaded.Value && !_toolboxes.ContainsKey(alias))
        _toolboxes.Add(alias, new string[] { path, display_name });
    }

    static public bool isPortal(ToolInfo ti)
    {
      if (ti.Node?.Name.LocalName == "toolbox" && ti.Node?.Element("fullpath") == null)
      {
        var alias = ti.Node.Attribute("alias");
        return alias != null ? isPortalAlias(alias.Value) : false;
      }
      return isPortalAlias(ti.toolboxalias);
    }

    static public bool isPortalAlias(string alias)
    {
      if (string.IsNullOrEmpty(alias))
        return false;
      if (_portal_aliases.ContainsKey(alias))
        return true;
      return false;
    }
    public static string AliasToDisplayName(string alias)
    {
      string[] val;
      if (!string.IsNullOrEmpty(alias) && _is_cache0loaded.Value && _toolboxes.TryGetValue(alias, out val))
        return val[1];
      return string.Empty;
    }

    // Common system toolbox aliases that are *not* the module name
    // in arcpy due to length or syntax constraints
    public static string AliasToArcpyModuleAlias(string alias) => alias?.ToLower() switch
    {
      "3d" => "ddd",
      "tracking analyst tools" => "ta",
      "mobile tools" => "mobile",
      _ => alias
    };

    static System.Lazy<IReadOnlyDictionary<string, string>> _environments_help = new Lazy<IReadOnlyDictionary<string, string>>(() =>
    {
      try
      {
        var env_file = System.IO.Path.Combine(SysToolsUtil.gpHelpPath, "environment_settings.xml");
        if (!System.IO.File.Exists(env_file))
          env_file = System.IO.Path.Combine(SysToolsUtil.gpHelpPathEN, "environment_settings.xml");

        var xml = XDocument.Load(env_file);
        var env_nodes = xml.Root.Element("tool").Element("parameters").Elements("param");
        return env_nodes.Select(it => it).ToDictionary(it => it.Attribute("name").Value, it =>
        {
          var help_node = it.Element("dialogReference");
          return help_node == null ? string.Empty : help_node.ToString();
        });
      }
      catch { return new Dictionary<string, string>(); }
    });

    static public string get_environment_help(string env_name)
    {
      try
      {
        string ret;
        if (_environments_help.Value.TryGetValue(env_name, out ret))
          return ret;
      }
      catch { }
      return string.Empty;
    }

    sealed record json_g0(Dictionary<string, string[]> toolboxes, Dictionary<string, int[]> tools);

    internal sealed class tool_codes
    {
      bool? _locked; //cached result
      public int product_code;
      public int[] extension_codes;
      public int helpid;
      public bool isLocked(ISet<int> availableCodes)
      {
        if (_locked.HasValue)
          return _locked.Value;
        if (extension_codes == null || _productCode < 0)
          return false;

        if (extension_codes.Length == 0)
        {
          _locked = _productCode >= product_code ? false : true;
          return _locked.Value;
        }

        if (product_code > 40 && _productCode >= product_code)
          _locked = false;
        else
          _locked = !availableCodes.Overlaps(extension_codes);

        return _locked.Value;
      }
    };

    private static bool generate_cache_file0(string file_path, out IDictionary<string, string[]> tbx_pair, out IReadOnlyDictionary<string, tool_codes> codes)
    {
      try
      {
        var all_codes = new Dictionary<string, tool_codes>();
        var all_tbx = new Dictionary<string, string[]>();
        bool save_file = true;
        var dir_list = new List<string>()
          {
            System.IO.Path.Combine(SysToolsUtil.InstallPath, @"Resources\ArcToolBox\Toolboxes"),
          };
        dir_list.AddRange(SysToolsUtil.get_site_packages());
        dir_list.AddRange(SysToolsUtil.GetAddinToolboxes());

        foreach (var path in dir_list.SelectMany(it => System.IO.Directory.EnumerateFileSystemEntries(it)))
        {
          string alias;
          string title;
          switch (System.IO.Path.GetExtension(path).ToLower())
          {
            case ".atbx":
              utbx.read_toolbox(path, out alias, out title, all_codes);
              break;
            case ".tbx":
              bool is_utbx = System.IO.File.GetAttributes(path).HasFlag(System.IO.FileAttributes.Directory);
              save_file = is_utbx ? utbx.read_toolbox(path, out alias, out title, all_codes) :
                                    classic.read_toolbox(path, out alias, out title, all_codes);
              break;
            case ".pyt":
              save_file = classic.read_toolbox(path, out alias, out title, all_codes);
              break;
            default: continue;
          }
          if (!save_file)
            break;
          if (!string.IsNullOrEmpty(alias))
            all_tbx.Add(alias, new string[] { $"@\\{System.IO.Path.GetFileName(path)}", title });
        };

        if (save_file)
        {
          var cs = all_codes.ToDictionary(it => it.Key, it => {
            var dd = new List<int> { it.Value.helpid, it.Value.product_code };
            dd.AddRange(it.Value.extension_codes);
            return dd.ToArray();
          });
          try
          {
            using (var stm = System.IO.File.Create(file_path))
            using (var u8jw = new System.Text.Json.Utf8JsonWriter(stm, new System.Text.Json.JsonWriterOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }))
            {
              System.Text.Json.JsonSerializer.Serialize(u8jw, new json_g0(all_tbx, cs));
            }
            System.IO.File.SetAttributes(file_path, System.IO.FileAttributes.ReadOnly |
              System.IO.FileAttributes.Normal |
              System.IO.FileAttributes.NotContentIndexed);
          }
          catch { }
          codes = all_codes;
          tbx_pair = all_tbx;
          return true;
        }
      }
      catch
      {
        System.Diagnostics.Debug.Write("*** generate_cache_file - FAILED");
        //System.Diagnostics.Debug.Assert(false);
      }
      tbx_pair = new Dictionary<string, string[]>();
      codes = new Dictionary<string, tool_codes>();
      return false;
    }

    private static bool load_cache_file0(string full_path, out IDictionary<string, string[]> tbx, out IReadOnlyDictionary<string, tool_codes> tool_codes)
    {
      try
      {
        //var utf8 = utbx.read_utf8(full_path);
        // we don't need Utf8JsonReader
        var g0 = System.Text.Json.JsonSerializer.Deserialize<json_g0>(System.IO.File.ReadAllBytes(full_path));
        tbx = g0.toolboxes;
        tool_codes = g0.tools.ToDictionary(it => it.Key, it => new tool_codes { helpid = it.Value[0], product_code = it.Value[1], extension_codes = it.Value.Skip(2).ToArray() });
        return true;
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.WriteLine(e.Message);
      }
      tbx = new Dictionary<string, string[]>();
      tool_codes = new Dictionary<string, tool_codes>();
      return false;
    }

    static IDictionary<string, string[]> _toolboxes;
    static IReadOnlyDictionary<string, tool_codes> _tool_codes;
    private static readonly Lazy<bool> _is_cache0loaded = new Lazy<bool>(() =>
    {
      try
      {
        // need to fix later
        //var file = ToolboxUtil.gen_cache_filename(0);
        //if (System.IO.File.Exists(file))
        //  return load_cache_file0(file, out _toolboxes, out _tool_codes);
        //return generate_cache_file0(file, out _toolboxes, out _tool_codes);
      }
      catch { }
      _toolboxes = new Dictionary<string, string[]>();
      _tool_codes = new Dictionary<string, tool_codes>();
      return false;
    });
    internal static Dictionary<string, string> _portal_aliases = new Dictionary<string, string>();
    internal static int _productCode = -1;

    static HashSet<int> get_availableCodes()
    {
      try
      {
        if (!_is_cache0loaded.Value) return new HashSet<int>();
        var unique = _tool_codes.Values.SelectMany(it => it.extension_codes).Distinct().ToArray();//.Select(it => unique.Add(it)).Sum(it => it ? 1 : 0);
        // need to fix later
        //var s = GeoprocessingModule.getIGPService();
        //var cods = s.GetOption(string.Format(":verify:{0}", string.Join(" ", unique)));
        //if (!string.IsNullOrEmpty(cods))
          //return new HashSet<int>(cods.Trim().Split(' ').Select(it => System.Convert.ToInt32(it)));
      }
      catch { }
      return new HashSet<int>();
    }
    public static Lazy<Task<HashSet<int>>> _availableCodes = new Lazy<Task<HashSet<int>>>(() => Task.Run(get_availableCodes), false);

    public static string code_to_licence_name(int ext) => ext switch
    {
      4 => "ArcPress",      //esriLicenseExtensionCodeArcPress
      6 => "GeoStats",      //esriLicenseExtensionCodeGeoStats - Geostatistical Analyst
      8 => "Network",       //esriLicenseExtensionCodeNetwork - Network Analyst
      9 => "3D",            //esriLicenseExtensionCode3DAnalyst - 3D Analyst"
      10 => "spatial",      //esriLicenseExtensionCodeSpatialAnalyst - Spatial Analyst
      12 => "StreetMap",    //esriLicenseExtensionCodeStreetMap
      32 => "Tracking",     //esriLicenseExtensionCodeTracking
      33 => "BusinessPrem", //esriLicenseExtensionCodeBusinessStandard - Business Standard
      34 => "ArcScan",      //esriLicenseExtensionCodeArcScan
      35 => "Business",     //esriLicenseExtensionCodeBusiness - Business Analyst
      36 => "Schematics",   //esriLicenseExtensionCodeSchematics
      40 => "JTX",          //Workflow Manager
      45 => "DataInteroperability", //esriLicenseExtensionCodeSchematics
      46 => "Foundation",   // esriLicenseExtensionCodeProductionMapping - Production Mapping
      47 => "DataReviewer", // esriLicenseExtensionCodeDataReViewer - Data Reviewer
      48 => "MPSAtlas",     //esriLicenseExtensionCodeMPSAtlas),
      49 => "Defense",      //esriLicenseExtensionCodeDefense
      50 => "Nautical",     //esriLicenseExtensionCodeNautical
      51 => "IntelAgency",  //esriLicenseExtensionCodeIntelAgency
      52 => "MappingAgency",// esriLicenseExtensionCodeMappingAgency
      53 => "Aeronautical", //esriLicenseExtensionCodeAeronautical
      67 => "Highways",     //esriLicenseExtensionCodeHighways
      69 => "Bathymetry",   //esriLicenseExtensionCodeBathymetry
      70 => "Airports",     //esriLicenseExtensionCodeAirports
      72 => "SMPNorthAmerica", //esriLicenseExtensionCodeSMPNorthAmerica
      73 => "SMPEurope",    //esriLicenseExtensionCodeSMPEurope
      74 => "SMPLatinAmerica", //esriLicenseExtensionCodeSMPLatinAmerica
      75 => "SMPAsiaPacific", //esriLicenseExtensionCodeSMPAsiaPacific
      76 => "SMPMiddleEastAfrica", //esriLicenseExtensionCodeSMPMiddleEastAfrica
      77 => "SMPJapan",       //esriLicenseExtensionCodeSMPJapan
      78 => "LocationReferencing",// esriLicenseExtensionCodeLocref),
      79 => "UtilityNetwork", // esriLicenseExtensionCodeUtilityNetwork
      85 => "WorldGeocoder",  // esriLicenseExtensionCodeWorldGeocoder
      86 => "ImageAnalyst",   //Image Analyst
      87 => "LocateXT",       //esriLicenseExtensionCodeLocateXT
      88 => "NotebooksAdvanced", // esriLicenseExtensionCodeNotebooksAdvanced
      89 => "NotebooksStandard", //esriLicenseExtensionCodeNotebooksStandard
      _ => string.Empty
    };

    internal static bool get_tool_codes(string short_name, out int[] extension_codes)
    {
      extension_codes = null;
      if (_productCode < 0) return false;
      if (!_is_cache0loaded.Value) return false;

      if (_tool_codes.TryGetValue(short_name, out tool_codes tc))
      {
        extension_codes = tc.extension_codes;
        return true;
      }
      return false;
    }

    async internal static Task<bool> isLocked(string short_name)
    {
      if (_productCode < 0) return false;
      if (!_is_cache0loaded.Value) return false;

      tool_codes codes;
      if (!_tool_codes.TryGetValue(short_name, out codes))
        return false;
      var ac = await _availableCodes.Value;
      return codes.isLocked(ac);
    }

    internal static int query_system_tool_helpID(string short_name)
    {
      if (!_is_cache0loaded.Value) return 0;
      tool_codes codes;
      if (!_tool_codes.TryGetValue(short_name, out codes))
        return 0;
      return codes.helpid;
    }
    //////////////
    static class classic
    {
      public static bool read_toolbox(string path, out string alias, out string title, Dictionary<string, tool_codes> codes)
      {
        alias = string.Empty;
        title = string.Empty;
        try
        {
          var tbx_name = System.IO.Path.GetFileNameWithoutExtension(path);
          var tbx_path = System.IO.Path.Combine(gpHelpPath, "Toolboxes", tbx_name);
          tbx_path = System.IO.Path.ChangeExtension(tbx_path, ".xml");

          if (!System.IO.File.Exists(tbx_path))
          {
            var is_localazed = gpHelpPath != gpHelpPathEN;
            if (!is_localazed)
              return true;
            //try english
            tbx_path = System.IO.Path.Combine(gpHelpPathEN, "Toolboxes", tbx_name);
            tbx_path = System.IO.Path.ChangeExtension(tbx_path, ".xml");
            if (!System.IO.File.Exists(tbx_path))
              return true;
          }

          var doc = XDocument.Load(tbx_path);
          if (doc == null)
            return false;

          //copy attributes
          var alias_str = (string)doc.Root.Attribute("Alias");
          if (string.IsNullOrEmpty(alias_str))
            return false;

          var tmp_lic = doc.Root.Descendants("GPToolInfo").All(it =>
          {
            var tc = new tool_codes() { product_code = 0 };
            var key = $"{alias_str}.{it.Element("Name").Value}";
            var pc = (int?)it.Element("ProductCode");
            if (pc != null && pc.Value > 0)
            {
              tc.product_code = pc.Value;
              var ec = it.Element("ExtensionCodes");
              try
              {
                if (ec != null)
                  tc.extension_codes = ec.Value.Split(',').Select(part => System.Convert.ToInt32(part)).ToArray();
                else
                  tc.extension_codes = new int[0];
              }
              catch { tc.extension_codes = new int[0]; }
            }
            else
              tc.extension_codes = new int[0];
            try
            {
              tc.helpid = (int)it.Element("Help");
            }
            catch { tc.helpid = 0; }
            codes.Add(key, tc);
            return true;
          });
          var toolbox_name = doc.Root.Attribute("Name").Value;
          System.Diagnostics.Debug.Assert(tbx_name.ToLower() == toolbox_name.ToLower());
          alias = alias_str;
          title = doc.Root.Element("DisplayName").Value;
          return true;
        }
        catch
        {
          System.Diagnostics.Debug.Assert(false);
          return false;
        }
      }
    }
    internal static class utbx
    {
      public static readonly string toolbox_content_file = "toolbox.content";
      public static readonly string toolbox_content_rc_file = "toolbox.content.rc";
      public static readonly string tool_content_file = "tool.content";
      public static readonly string tool_content_rc_file = "tool.content.rc";
      public static readonly string tool_keywords_file = "tool.keywords.rc";
      record json_tool_content
      {
        public int helpcontent { get; init; }
        public int product { get; init; }
        public int[] extensions { get; init; }
      }
      static bool read_tool_content(string path, out tool_codes tc, System.IO.Compression.ZipArchive archive = null)
      {
        tc = new tool_codes();

        string content_file_path = string.Empty;
        try
        {
          content_file_path = System.IO.Path.Combine(path, tool_content_file);
          var utf8 = read_utf8(content_file_path, archive);
          var reader = new System.Text.Json.Utf8JsonReader(utf8, new System.Text.Json.JsonReaderOptions() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, MaxDepth = 128 });
          var tool = System.Text.Json.JsonSerializer.Deserialize<json_tool_content>(ref reader, new System.Text.Json.JsonSerializerOptions() { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString });
          tc.helpid = tool.helpcontent;
          tc.product_code = tool.product switch { 100 => 40, 200 => 50, 300 => 60, _ => tool.product };
          if (tool.extensions != null)
            tc.extension_codes = tool.extensions;
          else
            tc.extension_codes = new int[0];

          return true;
        }
        catch (Exception e)
        {
          var msg = $"Error reading '{content_file_path}' : {e.Message}";
          System.Diagnostics.Debug.Assert(false, msg); return false;
        }
      }
      static IEnumerable<string[]> _select_tools(json_toolbox_content toolbox_content)
      {
        try
        {
          //var x = (System.Text.Json.JsonElement)json_toolbox["toolsets"];
          //return x.EnumerateObject().SelectMany(it => it.Value.EnumerateObject()).SelectMany(it => it.Value.EnumerateArray()).Select(it =>
          return toolbox_content.toolsets.SelectMany(it => it.Value.tools).Select(name =>
          {
            try
            {
              //string name = it;
              var parts = name.Split(':');
              var dir = parts.Length == 2 ? parts[1] : (name + ".tool");
              return [name, dir];
            }
            catch { }
            return new string[] { };
          }).Where(it => it.Length == 2);
        }
        catch { return Enumerable.Empty<string[]>(); }
      }

      /// <summary>
      /// read all bytes from a file or internal zip file, skip UTF-8 BOM
      /// </summary>
      internal static ReadOnlySpan<byte> read_utf8(string path, System.IO.Compression.ZipArchive archive = null)
      {
        System.IO.Stream stm = null;
        try
        {
          if (archive is null)
          {
            //return System.IO.File.ReadAllBytes(path);
            stm = System.IO.File.OpenRead(path);
          }
          else
          {
            var e = archive.GetEntry(path.Replace('\\', '/'));
            if (e == null) return null;
            stm = e.Open();
          }
          using (var ms = new System.IO.MemoryStream())
          {
            stm.CopyTo(ms);
            var buff = ms.GetBuffer();
            //check and skip BOM
            if (ms.Length > 2 && buff[0] == 0xEF && buff[1] == 0xBB && buff[2] == 0xBF)
              return new(buff, 3, (int)ms.Length - 3);
            return buff;
          }
        }
        catch { return null; }
        finally { stm?.Dispose(); }
      }

      record json_rc_map
      {
        public Dictionary<string, string> map { get; init; }
      }

      static IDictionary<string, string> read_content_rc_file(System.IO.Compression.ZipArchive archive, string path, string path_localized = null)
      {
        try
        {
          var utf8 = read_utf8(path, archive);
          var reader = new System.Text.Json.Utf8JsonReader(utf8, new System.Text.Json.JsonReaderOptions() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, MaxDepth = 128 });
          var a_table = System.Text.Json.JsonSerializer.Deserialize<json_rc_map>(ref reader);
          if (string.IsNullOrEmpty(path_localized))
            return a_table.map;
          //load localized toobox: folder based
          var b_table = read_content_rc_file(null, path_localized, null);
          if (!b_table.Any())
            return a_table.map;
          //join tables
          var ab_table = a_table.map.Select(it =>
          {
            string val;
            if (b_table.TryGetValue(it.Key, out val))
              return new { Key = it.Key, Value = val };
            return new { Key = it.Key, Value = it.Value as string };
          }).ToDictionary(it => it.Key, it => it.Value);
          return ab_table;
        }
        catch { return new Dictionary<string, string>(); }
      }

      internal static bool read_toolbox(string tbx_path, out string alias, out string title, Dictionary<string, tool_codes> codes)
      {
        string alias0 = string.Empty;
        string title0 = string.Empty;
        var ret = select_tools(tbx_path, (archive, toolbox_base, o) =>
        {
          var toolbox = o as json_toolbox_content;
          alias0 = toolbox.alias;
          title0 = toolbox.displayname;
          if (title0.StartsWith("$rc:"))
          {
            var rc_localized_path = get_localized_tbx_path(tbx_path);
            if (!string.IsNullOrEmpty(rc_localized_path))
              rc_localized_path = System.IO.Path.Combine(rc_localized_path, toolbox_content_rc_file);
            var rc_path = System.IO.Path.Combine(toolbox_base, toolbox_content_rc_file);
            string title_txt;
            if (read_content_rc_file(archive, rc_path, rc_localized_path).TryGetValue(title0.Substring(4), out title_txt))
              title0 = title_txt;
          }
        },
        (archive, name, too_path) =>
        {
          tool_codes tc = null;
          if (read_tool_content(too_path, out tc, archive))
          {
            codes.Add($"{alias0}.{name}", tc);
            //return new { name = $"{alias0}.{name}", codes = tc };
          }
        });
        alias = alias0;
        title = title0;
        return ret;
      }

      static string get_localized_tbx_path(string full_tbx_path)
      {
        try
        {
          if (string.IsNullOrEmpty(full_tbx_path) || SysToolsUtil.gpHelpPath == SysToolsUtil.gpHelpPathEN)
            return string.Empty;
          var tbx_name = System.IO.Path.GetFileName(full_tbx_path);
          return System.IO.Path.Combine(SysToolsUtil.gpHelpPath, "Toolboxes", tbx_name);
        }
        catch { return string.Empty; }
      }

      internal static string get_localized_tool_path(string full_tool_path)
      {
        try
        {
          if (string.IsNullOrEmpty(full_tool_path) || SysToolsUtil.gpHelpPath == SysToolsUtil.gpHelpPathEN)
            return string.Empty;
          var tbx_path = get_localized_tbx_path(System.IO.Path.GetDirectoryName(full_tool_path));
          if (string.IsNullOrEmpty(tbx_path))
            return string.Empty;
          var tool_name = System.IO.Path.GetFileName(full_tool_path);
          return System.IO.Path.Combine(tbx_path, tool_name);
        }
        catch { return string.Empty; }
      }

      record json_keywords_set
      {
        public string[] set { get; init; }
      }
      /// <summary>
      /// returns unique array of searching keywords (localized + based)
      /// </summary>
      public static string[] read_tool_keywords(System.IO.Compression.ZipArchive archive, string tool_path)
      {
        if (string.IsNullOrEmpty(tool_path))
          return [];
        //var ret = try_to_localize ? read_tool_keywords(archive, get_localized_tool_path(tool_path), false) : new string[] { };
        string rc_file_path = string.Empty;
        try
        {
          rc_file_path = System.IO.Path.Combine(tool_path, tool_keywords_file);
          var utf8 = read_utf8(rc_file_path, archive);
          if (utf8 == null)
            return [];
          var reader = new System.Text.Json.Utf8JsonReader(utf8, new System.Text.Json.JsonReaderOptions() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, MaxDepth = 128 });
          var set = System.Text.Json.JsonSerializer.Deserialize<json_keywords_set>(ref reader).set;
          return new HashSet<string>(set.Select(it => it.ToLower())).ToArray();
        }
        catch (Exception e)
        {
          var msg = $"Error reading '{rc_file_path}' : {e.Message}";
          System.Diagnostics.Debug.Assert(false, msg);
          //System.Diagnostics.Debug.WriteLine(msg);
          return [];
        }
      }

      public static string[] read_tool_content_rc(System.IO.Compression.ZipArchive archive, string tool_path, params string[] keys)
      {
        if (string.IsNullOrEmpty(tool_path))
          return null;
        IDictionary<string, string> map;
        try
        {
          var rc_file_path = System.IO.Path.Combine(tool_path, tool_content_rc_file);
          map = read_content_rc_file(archive, rc_file_path, null);
        }
        catch (Exception e)
        {
          var msg = $"Error reading description:'{e.Message}";
          System.Diagnostics.Debug.Assert(false, msg);
          //System.Diagnostics.Debug.WriteLine(msg);
          return null;
        }
        if (map is null)
          return null;
        return keys.Select(it =>
        {
          if (map.TryGetValue(it, out var val))
            return val;
          return "";
        }).ToArray();
      }

      record json_toolset(string[] tools);
      record json_toolbox_content(
        string alias,
        string displayname,
        Dictionary<string, json_toolset> toolsets
      );

      public static bool select_tools(string tbx_path, Action<System.IO.Compression.ZipArchive, string, object> fn_open_content, Action<System.IO.Compression.ZipArchive, string, string> fn_select)
      {
        string content_file_path = string.Empty;
        System.IO.Compression.ZipArchive archive = null;

        try
        {
          string toolbox_base = tbx_path;
          bool asZip = tbx_path.EndsWith(".atbx") || tbx_path.EndsWith(".zip.tbx");
          if (asZip)
          {
            archive = System.IO.Compression.ZipFile.OpenRead(tbx_path);
            var content_file = archive.Entries.FirstOrDefault(it => it.Name == toolbox_content_file);
            if (content_file != null)
              toolbox_base = System.IO.Path.GetDirectoryName(content_file.FullName);
          }

          content_file_path = System.IO.Path.Combine(toolbox_base, toolbox_content_file);
          var utf8 = read_utf8(content_file_path, archive);
          if (utf8 == null)
            return false;// Enumerable.Empty<TResult>();

          var reader = new System.Text.Json.Utf8JsonReader(utf8, new System.Text.Json.JsonReaderOptions() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, MaxDepth = 128 });
          var toolbox_content = System.Text.Json.JsonSerializer.Deserialize<json_toolbox_content>(ref reader);
          if (fn_open_content != null)
            fn_open_content(archive, toolbox_base, toolbox_content);

          foreach (var pair in _select_tools(toolbox_content))
          {
            var dir = pair[1];
            if (dir.StartsWith("..") && archive != null)
              continue;
            fn_select(archive, pair[0], System.IO.Path.Combine(toolbox_base, dir));
          }
          return true;
        }
        catch { return false; }
        finally
        {
          archive?.Dispose();
        }
        //return Enumerable.Empty<TResult>();
      }
    }
  }
}
