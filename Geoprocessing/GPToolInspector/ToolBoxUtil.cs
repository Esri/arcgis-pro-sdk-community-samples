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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.GeoProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GPToolInspector
{
  using LocalResources = ArcGIS.Desktop.Internal.GeoProcessing.LocalResources;
  internal static class ToolboxUtil
  {
    public static readonly string system_toolboxes = @"Toolboxes\System Toolboxes";

#if false
    static void makeit(XElement t, string path, ref List<ToolInfo> ret)
    {
      var nodetype = t.Name.LocalName;
      switch (nodetype)
      {
        case "tool":
          {
            var e = t.Element("fullpath");
            System.Diagnostics.Debug.Assert(e == null);
            if (e == null)
            {
              var name = toolInfoExt.getName(t);
              t.Add(new XElement("fullpath", System.IO.Path.Combine(path, name)));
            }
            ret.Add(new ToolInfo(t));
          }
          return;
        case "toolset":
          {
            var name = toolInfoExt.getName(t);
            System.Diagnostics.Debug.Assert(path != null);
            path = System.IO.Path.Combine(path, name);
          } break;
        case "toolbox":
          {
            System.Diagnostics.Debug.Assert(path == null);
            if (t.Attribute("failed") != null)
              return;
            path = toolInfoExt.getFullPath(t);
          } break;
        case "toolboxes":
          foreach (var ix in t.Elements("toolbox"))
            makeit(ix, null, ref ret);
          return;
        default:
          return;
      }

      //foreach (var ix in t.Elements())
      //  makeit(ix, path, ref ret);
      foreach (var ix in t.Elements("toolset").Concat(t.Elements("tool")))
        makeit(ix, path, ref ret);
    }
    static List<ToolInfo> makeList(XElement root)
    {
      var ret = new List<ToolInfo>();
      try
      {
        makeit(root, null, ref ret);
      }
      catch
      {
        System.Diagnostics.Debug.WriteLine("BOOM!!!");
      }
      return ret;
    }

#endif
    static internal IEnumerable<ToolInfoViewModel> toflat(IEnumerable<ToolInfoViewModel> tree)
    {
      if (tree == null) return null;
      return tree.SelectMany(it => ToolInfoViewModel.get_flat_tools(it, null, (t, p) => t));
    }

    internal sealed class cached_tooloxes
    {
      private XDocument _xml_doc;
      private IReadOnlyList<ToolInfo> _flat;

      private ToolInfoViewModel[] _tree;
      public ToolInfoViewModel[] tree
      {
        get
        {
          if (_tree == null)
          {
            if (_xml_doc == null)
              _tree = new ToolInfoViewModel[] { };
            else
              _tree = _xml_doc.Root.Elements("toolbox").
                       OrderBy(e => toolInfoExt.getDisplayName(e)).
                       Select(it => ToolInfoViewModel.create_VM(it, null)).
                       ToArray();
          }
          return _tree;
        }
      }
      public IReadOnlyList<ToolInfo> flat
      {
        get
        {
          if (_flat == null && _xml_doc != null)
          {
            lock (_xml_doc)
            {
              if (_flat == null)
                _flat = ToolboxUtil.toflat(tree).Cast<ToolInfo>().ToList();
            }
          }
          return _flat ?? Enumerable.Empty<ToolInfo>().ToList();
        }
      }
      public cached_tooloxes(XDocument xml_doc)
      {
        _xml_doc = xml_doc;

      }
    }

    static Lazy<cached_tooloxes> _all_systemTools = new Lazy<cached_tooloxes>(() =>
    {
      var system_tools_xml = check_system_cache();
      if (system_tools_xml != null)
        return new cached_tooloxes(system_tools_xml);
      //create cache file
      var progress = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("!!! LocalResources.GpResources.CreatingCache");
      try
      {
        progress.Show();
        var xml = load_xml(ToolboxUtil.system_toolboxes);
        var xml_doc = XDocument.Parse(xml);
        //find deprecated tools and remove
        System.Diagnostics.Debug.WriteLine("-------- processing all GP systemTools ------------");

        var toolboxes_dir = System.IO.Path.Combine(SysToolsUtil.InstallPath, "Resources", "ArcToolBox", "Toolboxes");
        var depricated_list_file = System.IO.Path.Combine(toolboxes_dir, "deprecated.list");

        ILookup<string, string> remove_tools_map = null;
        if (System.IO.File.Exists(depricated_list_file))
        {
          remove_tools_map = System.IO.File.ReadAllLines(depricated_list_file).Select(it => it.Trim()).
                            Where(it => !string.IsNullOrEmpty(it) && it.First() == '-').
                            Select(it => it.Substring(2).Trim('"').Split('/')).ToLookup(it => it[0], it => it[1]);
        }

        var tbx_dir = System.IO.Path.Combine(SysToolsUtil.gpHelpPath, "Toolboxes");
        //var tbx_dirEN = System.IO.Path.Combine(SysToolsUtil.gpHelpPathEN, "Toolboxes");
        var toolboxes = xml_doc.Root.Elements("toolbox");
        var rm_me = new List<XElement>();
        int tb_count = 0;
        string failed_tbx = string.Empty;
        foreach (var tb in toolboxes)
        {
          if (tb.Attribute("failed") != null)
          {
            failed_tbx = tb.Attribute("fullpath").Value;
            xml_doc = null;
            break;
          }
          var alias = tb.Attribute("alias").Value;
          if (!SysToolsUtil.isSystemAlias(alias))
          {
            // custom system tbx
            SysToolsUtil.register_alias_as_system(alias, tb.Element("displayname").Value, string.Empty);
            //rm_me.Add(tb);
            continue;
          }
          if (remove_tools_map != null)
          {
            Dictionary<string, XElement> tool_dict = new();
            foreach (var elem in tb.Descendants("tool"))
              tool_dict.Add(elem.Attribute("name").Value.ToLower(), elem);
            var path_tbx = SysToolsUtil.AliasToPath(alias).ToLower();
            var toolbox_name = System.IO.Path.GetFileNameWithoutExtension(path_tbx);
            var is_zip = toolbox_name.EndsWith(".zip");
            if (remove_tools_map.Contains(toolbox_name))
            {
              var rm_map = remove_tools_map[toolbox_name];
              var tool_remove = tool_dict.Where(it => rm_map.Contains(it.Key)).ToList();
              tool_remove.ForEach(it => { tool_dict.Remove(it.Key); it.Value.Remove(); });
            }
            var full_toolbox_path = System.IO.Path.Combine(toolboxes_dir, toolbox_name + ".tbx");
            if (is_zip || System.IO.Directory.Exists(full_toolbox_path)) //handle utbx
            {
              //add missing keys
              SysToolsUtil.utbx.select_tools(full_toolbox_path, null, (archive, name, tool_path) =>
              {
                var name_l = name.ToLower();
                if (tool_dict.TryGetValue(name_l, out var elem))
                {
                  var keys = SysToolsUtil.utbx.read_tool_keywords(archive, tool_path);
                  var keys_l10 = SysToolsUtil.utbx.read_tool_keywords(archive, SysToolsUtil.utbx.get_localized_tool_path(tool_path));
                  var key_all = new HashSet<string>(keys.Concat(keys_l10));
                  elem.Add(key_all.Select(key => new XElement("key", key)));
                }
              });//.Where(it => it != null).ToDictionary(it => it.name as string, it => it.keys as string[]);
            }
          }
          //remove empty toolsets
          var toolset_remove = tb.Descendants("toolset").Where(it => it.Descendants("tool").FirstOrDefault() is null).ToList();
          toolset_remove.ForEach(it => it.Remove());
          tb_count++;
        }
        rm_me.ForEach(it => it.Remove());
        var minTbCount = 11;
        // Not Used
        //if (Internal.Framework.FrameworkApplication.IsRunningAsProProductType(Internal.Framework.ProProductType.ProductDrone2Map))
        //{
        //  //d2m has a subset of tools
        //  minTbCount = 5;
        //}
        if (tb_count >= minTbCount && xml_doc != null)
          dump2system_cache(xml_doc);
        else
        {
          progress.Hide();
          var msg = "!!! LocalResources.GpResources.CreatingCacheFailedMessage";
          msg += "\n\n";
          msg += failed_tbx;
          ArcGIS.Desktop.Internal.Framework.DialogManager.ShowMessageBox(msg);
        }
        return new cached_tooloxes(xml_doc);
      }
      catch
      {
        progress.Hide();
        ArcGIS.Desktop.Internal.Framework.DialogManager.ShowMessageBox("!!! LocalResources.GpResources.CreatingCacheFailedMessage");
        return new cached_tooloxes(null);
      }
      finally
      {
        progress.Hide();
        progress.Dispose();
        // Not Used
        // Internal.Framework.Events.UpdateSearchIndexEvent.Publish(new Internal.Framework.Events.UpdateSearchIndexEventArgs(new GeoprocessingToolsComponent()));
      }
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    static public Task<List<ToolInfo>> GetToolsAsync(string toolbox_path)
    {
      if (toolbox_path == ToolboxUtil.system_toolboxes && _all_systemTools.IsValueCreated)
      {
        try
        {
          return Task.FromResult(_all_systemTools.Value.flat.ToList());
        }
        catch
        {
          return Task.FromResult(new List<ToolInfo>());
        }
      }

      return QueuedTask.Run(() => getTools(toolbox_path));
    }

    internal static void EnsureWorkerThread()
    {
      ArcGIS.Core.Internal.Interop.ThrowOnWrongThread();
    }

    static public List<ToolInfo> getTools(string toolbox_path)
    {
      EnsureWorkerThread();
      try
      {
        if (toolbox_path == ToolboxUtil.system_toolboxes)
          return _all_systemTools.Value.flat.ToList();
        return toflat(loadToolbox(toolbox_path)).Cast<ToolInfo>().ToList();
      }
      catch { return new List<ToolInfo>(); }
    }

    static ToolInfoViewModel[] _all_portal_tools;
    static public void reload_portal()
    {
      _all_portal_tools = null;
    }

    static bool? _isPortalEnabled;
    static HashSet<string> query_portal_services()
    {
      //System.Threading.Thread.Sleep(5000);
      try
      {
        // Not used
        //var s = GeoprocessingModule.getIGPService();
        //var xml = s.GetOption("portal:helperServices");
        //if (string.IsNullOrEmpty(xml))
        //  return new HashSet<string>();
        //var doc = XDocument.Parse(xml);
        //var hs = new HashSet<string>(doc.Root.Elements("service").Select(it => it.Attribute("name").Value));
        //hs.Add("agolservice");
        //return hs;
        return null;
      }
      catch { return new HashSet<string>() { "agolservice" }; }
    }
    public static bool IsPortalEnabled()
    {
      if (_isPortalEnabled.HasValue)
        return _isPortalEnabled.Value;
      var dir = System.IO.Path.Combine(SysToolsUtil.gpHelpPath, "Portal");
      if (System.IO.Directory.Exists(dir))
        _isPortalEnabled = System.IO.Directory.EnumerateFiles(dir, "*.xml", System.IO.SearchOption.AllDirectories).FirstOrDefault() != null;
      else
        _isPortalEnabled = false;
      return _isPortalEnabled.Value;
    }



    static public Task<ToolInfoViewModel[]> LoadPortalToolsAsync(bool asFlat)
    {
      return QueuedTask.Run(() => LoadPortalTools(asFlat));
    }
    static internal ToolInfoViewModel[] LoadPortalTools(bool asFlat)
    {
      EnsureWorkerThread();
      try
      {
        if (_all_portal_tools != null)
        {
          if (asFlat)
            return ToolboxUtil.toflat(_all_portal_tools).ToArray();
          return _all_portal_tools;
        }

        var dir = System.IO.Path.Combine(SysToolsUtil.gpHelpPath, "Portal");
        if (!System.IO.Directory.Exists(dir))
        {
          _all_portal_tools = new ToolInfoViewModel[] { };
          return _all_portal_tools;
        }
        var portal_items = System.IO.Directory.EnumerateFiles(dir, "*.xml", System.IO.SearchOption.AllDirectories).OrderBy(it => it).Select(it =>
        {
          try
          {
            var xml = XDocument.Load(it);
            var dn = System.IO.Path.GetDirectoryName(it);
            string ctrl_id = System.IO.Path.GetFileNameWithoutExtension(dn);
            var name = System.IO.Path.GetFileNameWithoutExtension(it);
            var dot_parts = name.Split('.');
            //string ctrl_id = dot_parts.LastOrDefault();
            xml.Root.SetAttributeValue("ctrl_id", string.IsNullOrEmpty(ctrl_id) ? string.Empty : ctrl_id.ToLower());
            xml.Root.SetAttributeValue("overlay_style", "icon_overlay_cloud_type"); //show portal icon

            var service_filter = dot_parts.Length > 1 ? dot_parts.Last() : string.Empty;

            //SysToolsUtil._portal_aliases.Add(xml.Root.Attribute("alias").Value);
            return new Tuple<string, XElement>(service_filter, xml.Root);
            //ToolInfoViewModel.create_VM(xml.Root, null);
          }
          catch { return null; }
        }).Where(t => t != null).ToList();

        var filtered_items = portal_items.Where(it => !string.IsNullOrEmpty(it.Item1)).ToList();
        if (filtered_items.Count > 1)
        {
          //validate with server
          var filter = query_portal_services();
          var remove_items = filtered_items.Where(it => !filter.Contains(it.Item1));
          _all_portal_tools = portal_items.Except(remove_items).Select(it => ToolInfoViewModel.create_VM(it.Item2, null)).ToArray();
        }
        else
          _all_portal_tools = portal_items.Select(it => ToolInfoViewModel.create_VM(it.Item2, null)).ToArray();

        try
        {
          var aliases = new Dictionary<string, string>();
          foreach (var it in _all_portal_tools)
            aliases[it.Node.Attribute("alias").Value] = it.Name;

          SysToolsUtil._portal_aliases = aliases;//agol_items.ToDictionary(it => it.Node.Attribute("alias").Value, it => it.Name);
        }
        catch { }

        if (asFlat)
          return ToolboxUtil.toflat(_all_portal_tools).ToArray();
        return _all_portal_tools;
      }
      catch
      {
        _all_portal_tools = new ToolInfoViewModel[] { };
        return _all_portal_tools;
      }
    }

    /// <summary>
    /// return sorted by name hierarchical collection of ToolInfoViewModel items
    /// </summary>
    /// <param name="toolbox_path"></param>
    /// <returns></returns>
    static public Task<ToolInfoViewModel[]> LoadToolboxAsync(string toolbox_path)
    {
      if (toolbox_path == ToolboxUtil.system_toolboxes && _all_systemTools.IsValueCreated)
        return Task<ToolInfoViewModel[]>.FromResult(_all_systemTools.Value.tree);

      return QueuedTask.Run(() => loadToolbox(toolbox_path));
    }

    static internal ToolInfoViewModel[] loadToolbox(string toolbox_path)
    {
      EnsureWorkerThread();

      if (toolbox_path == ToolboxUtil.system_toolboxes)
        return _all_systemTools.Value.tree;

      var xml = load_xml(toolbox_path);
      try
      {
        var parsed = XDocument.Parse(xml);
        return new ToolInfoViewModel[] { ToolInfoViewModel.create_VM(parsed.Root, null) };
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Assert(false, e.Message);
        return new ToolInfoViewModel[] { };
      }
    }

    #region internal
    static private string _sys_metadata_md5 = null;
    static object mx_sys_metadata_md5 = new object();
    static public string get_metadata_checksum(bool ui_culture_aware = true)
    {
      if (Volatile.Read<string>(ref _sys_metadata_md5) != null)
        return _sys_metadata_md5;

      lock (mx_sys_metadata_md5)
      {
        if (_sys_metadata_md5 != null)
          return _sys_metadata_md5;
        try
        {
          var dir_list = new List<KeyValuePair<string, string[]>>()
          {
            new KeyValuePair<string, string[]>(System.IO.Path.Combine(SysToolsUtil.InstallPath, @"Resources\ArcToolBox\Toolboxes"),new string[]{"*.tbx", "*.pyt", "*.atbx", "deprecated.list" }),
          };
          dir_list.AddRange(SysToolsUtil.get_site_packages().Select(it => new KeyValuePair<string, string[]>(it, new string[] { "*.atbx", "*.tbx", "*.pyt", "*.xml" })));
          dir_list.AddRange(SysToolsUtil.GetAddinToolboxes().Select(it => new KeyValuePair<string, string[]>(it, new string[] { "*.atbx", "*.tbx", "*.pyt", "*.xml" })));

          var files_last_write_date = dir_list.SelectMany(it =>
          {
            try
            {
              var dir = new System.IO.DirectoryInfo(it.Key);
              if (!dir.Exists)
                return new System.IO.FileSystemInfo[] { };
              return it.Value.SelectMany(ex => dir.EnumerateFileSystemInfos(ex)).SelectMany(fi =>
              {
                if ((fi.Attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory && fi.Extension == ".tbx")
                  return new System.IO.DirectoryInfo(fi.FullName).EnumerateFileSystemInfos("*.content", System.IO.SearchOption.AllDirectories);
                return new[] { fi };
              });
            }
            catch { return new System.IO.FileSystemInfo[] { }; }
          }).OrderBy(it => it.Name).Select(it => it.LastWriteTimeUtc.ToOADate());

          string hash_str = string.Empty;
          using (var md5 = System.Security.Cryptography.SHA1.Create())
          {
            md5.Initialize();
            var buff = System.Text.Encoding.ASCII.GetBytes("ArcGISPro"); // !!! Internal.Framework.FrameworkApplication.ProductID);
            md5.TransformBlock(buff, 0, buff.Length, buff, 0);
            buff = new byte[] { 12 }; //internal ver
            md5.TransformBlock(buff, 0, buff.Length, buff, 0);

            var x = files_last_write_date.Sum(it =>
            {
              byte[] block = BitConverter.GetBytes(it);
              return md5.TransformBlock(block, 0, block.Length, block, 0);
            });
            if (x > 0)
            {
              buff = System.Text.Encoding.ASCII.GetBytes(SysToolsUtil.InstallPath);
              md5.TransformBlock(buff, 0, buff.Length, buff, 0);

              //Last - add language
              if (ui_culture_aware)
              {
                buff = System.Text.Encoding.ASCII.GetBytes(System.Globalization.CultureInfo.CurrentUICulture.EnglishName);
                md5.TransformFinalBlock(buff, 0, buff.Length);
              }
              hash_str = BitConverter.ToString(md5.Hash).Replace("-", "");
            }
          }
          _sys_metadata_md5 = hash_str;
        }
        catch { _sys_metadata_md5 = string.Empty; }
        return _sys_metadata_md5;
      }
    }
    static string _sys_dump_file()
    {
      {
        try
        {
          return gen_cache_filename(1);
        }
        catch
        {
          return SysToolsUtil.InstallPath;
        }
      }
    }

    static Lazy<string> app_cache_folder = new Lazy<string>(() =>
    {
      try
      {
        var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\ESRI\{ArcGIS.Desktop.Internal.Framework.FrameworkApplication.ProductID}\Settings");
        if (regKey != null)
        {
          var path = regKey.GetValue("Cache Path")?.ToString();
          if (!string.IsNullOrEmpty(path))
            return System.Environment.ExpandEnvironmentVariables(path);
        }

        var service = ArcGIS.Desktop.Internal.Framework.Utilities.ServiceManager.Find<ArcGIS.Desktop.Internal.Framework.IDisplaySettingsService>();
        var dir = System.Environment.ExpandEnvironmentVariables(service.GetLocalCachesFolder());
        if (!System.IO.Directory.Exists(dir))
          dir = System.IO.Path.GetTempPath();
        return dir;
      }
      catch { return System.IO.Path.GetTempPath(); }
    });

    internal static string gen_cache_filename(int n, bool ui_culture_aware = true) => System.IO.Path.Combine(app_cache_folder.Value, $"g{n}.{get_metadata_checksum(ui_culture_aware)}.cache");

    static XDocument check_system_cache()
    {
      var dump_file_path = _sys_dump_file();
      if (!System.IO.File.Exists(dump_file_path))
        return null;
      try
      {
        System.Diagnostics.Debug.WriteLine($"*** loading system tools from cache:({dump_file_path})");

        //return System.Xml.Linq.XDocument.Load(dump_file_path);
        Dictionary<string, toolbox_cacheitem> toolboxes =
          System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, toolbox_cacheitem>>(System.IO.File.ReadAllBytes(dump_file_path));

        return new XDocument(new XElement("toolboxes",
          toolboxes.Select(kv =>
          {
            var toolbox = kv.Value;
            var toolsets = new Dictionary<string, XElement>();
            var ttoolbox = new XElement("toolbox", new XAttribute("alias", kv.Key),
              new XElement("displayname", toolbox.n),
              new XElement("description", toolbox.d));
            //add all toolsets
            foreach (var ts_name in toolbox.c)
            {
              var parts = ts_name.Split('/');
              toolsets.Add(ts_name, new XElement("toolset", new XElement("displayname", parts[parts.Length - 1])));
            }
            foreach (var ts_kv in toolsets)
            {
              var parts = ts_kv.Key.Split('/');
              if (parts.Length == 1)
                ttoolbox.Add(ts_kv.Value);
              else for (int i = 1; i < parts.Length; i++)
                {
                  var parent_name = string.Join('/', parts.Take(i));
                  System.Diagnostics.Debug.Assert(toolsets.ContainsKey(parent_name));
                  toolsets[parent_name].Add(ts_kv.Value);
                }
            }

            foreach (var tkv in kv.Value.t)
            {
              var tool = tkv.Value;
              XElement ttoolset = null;
              if (tool.c != -1)
              {
                toolsets.TryGetValue(toolbox.c[tool.c], out ttoolset);
                System.Diagnostics.Debug.Assert(ttoolset != null); // something wrong
              }
              var ttool = new XElement("tool", new XAttribute("name", tkv.Key),
                new XAttribute("type", tool.t switch
                {
                  0 => "function",
                  1 => "script",
                  2 => "model",
                  3 => "custom",
                  4 => "server",
                  5 => "pythonscript",
                  _ => "unknown"
                }),
                new XAttribute("toolboxalias", kv.Key),
                new XElement("displayname", tool.n),
                new XElement("description", tool.d));
              if (!string.IsNullOrEmpty(tool.a))
                ttool.SetAttributeValue("attr", tool.a);
              ttool.Add(tool.k.Select(k => new XElement("key", k)).ToArray());
              if (ttoolset is null)
                ttoolbox.Add(ttool);
              else
                ttoolset.Add(ttool);
            }
            return ttoolbox;
          }).ToArray()));
      }
      catch { return null; }
    }

    // obsolete
    static void read_classic_xml_keys(XElement t)
    {
      var help_dir = SysToolsUtil.gpHelpPath;//new System.IO.DirectoryInfo(SysToolsUtil.gpHelpPath);
      var dirEN = SysToolsUtil.gpHelpPathEN;// new System.IO.DirectoryInfo(SysToolsUtil.gpHelpPathEN);
      try
      {
        var ti = new ToolInfo(t);
        var toolbox_alias = t.Attribute("toolboxalias").Value;
        var xml_file = $"{ti.toolName}_{toolbox_alias}.xml";
        var fn = System.IO.Path.Combine(help_dir, xml_file);
        if (!System.IO.File.Exists(fn))
          fn = System.IO.Path.Combine(dirEN, xml_file);
        if (System.IO.File.Exists(fn))
        {
          var metadata = System.Xml.Linq.XDocument.Load(fn);
          var dataIdInfo = metadata.Root.Element("dataIdInfo");
          var new_desc = dataIdInfo.Element("idAbs").Element("para").Value.Trim();
          if (new_desc.Length > 1)
            ti.update_description(new_desc);
          var keys = new HashSet<string>(dataIdInfo.Element("descKeys").Elements("keyword").
                    Where(k => !string.IsNullOrEmpty(k.Value?.Trim())).
                    Select(k => k.Value.ToLower()));
          //add toolset as key
          var toolset_name = t.Parent?.Element("displayname")?.Value?.ToLower();
          if (!string.IsNullOrEmpty(toolset_name))
            keys.Add(toolset_name);
          t.Add(keys.Select(key => new XElement("key", key)));
        }
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Assert(false, string.Format("dump2system_cache #0 : {0} Exception caught", e));
      }
    }
    record struct tool_cacheitem(
      string n, // displayname
      string d, // description
      int c, // category index
      int t, // tool type
      string a, // attributes
      string[] k // keywords
      );
    record toolbox_cacheitem(
      string n, // displayname
      string d, // description
      List<string> c, // toolset names
      Dictionary<string, tool_cacheitem> t //tools
      );

    static void dump2system_cache(XDocument xml_doc)
    {
      var dump_file_path = _sys_dump_file();
      if (string.IsNullOrEmpty(dump_file_path))
        return;
      (string, tool_cacheitem) make_tool_item(XElement it, int c)
      {
        if (!it.Elements("key").Any()) //utbx - skip
          read_classic_xml_keys(it);

        return (it.Attribute("name").Value,
          new tool_cacheitem(it.Element("displayname").Value, it.Element("description").Value,
                        c,
                        it.Attribute("type").Value switch
                        {
                          "function" => 0,
                          "script" => 1,
                          "model" => 2,
                          "custom" => 3,
                          "server" => 4,
                          "pythonscript" => 5,
                          _ => 6
                        },
                        it.Attribute("attr")?.Value ?? "",
                        it.Elements("key").Select(it => it.Value).ToArray()));
      }
      void parce_toolsets(XElement tb, string ts_parent, ref List<string> toolsets, ref Dictionary<string, tool_cacheitem> tools)
      {
        foreach (var ts in tb.Elements("toolset"))
        {
          int c = toolsets.Count();
          var st_name = ts.Element("displayname").Value;
          if (!string.IsNullOrEmpty(ts_parent))
            st_name = $"{ts_parent}/{st_name}";
          toolsets.Add(st_name);
          foreach (var it in ts.Elements("tool"))
          {
            var (k, v) = make_tool_item(it, c);
            tools.Add(k, v);
          }
          parce_toolsets(ts, st_name, ref toolsets, ref tools);
        }
      }
      try
      {
        System.Collections.Concurrent.ConcurrentDictionary<string, toolbox_cacheitem> toolboxes = new();
        System.Threading.Tasks.Parallel.ForEach(xml_doc.Root.Descendants("toolbox"), tb =>
        {
          Dictionary<string, tool_cacheitem> tools = new();
          List<string> toolsets = new();
          foreach (var it in tb.Elements("tool"))
          {
            var (k, v) = make_tool_item(it, -1);
            tools.Add(k, v);
          }

          parce_toolsets(tb, null, ref toolsets, ref tools);
          //TEST if tools.Count() == 0
          toolboxes.TryAdd(tb.Attribute("alias").Value, new toolbox_cacheitem(
                tb.Element("displayname").Value,
                tb.Element("description").Value,
                toolsets, tools));
        });
        using (var stm = System.IO.File.Create(dump_file_path))
        using (var u8jw = new System.Text.Json.Utf8JsonWriter(stm, new System.Text.Json.JsonWriterOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }))
        {
          System.Text.Json.JsonSerializer.Serialize(u8jw, toolboxes);
        }
        try
        {
          System.IO.File.SetAttributes(dump_file_path, System.IO.FileAttributes.ReadOnly |
          System.IO.FileAttributes.Normal |
          System.IO.FileAttributes.NotContentIndexed);
        }
        catch { }
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Assert(false, string.Format("dump2system_cache #2 : {0} Exception caught", e));
      }
    }

    static readonly string _failed_toolbox = "<toolbox failed='true'/>";
    static private string load_xml(string toolbox_path)
    {
      System.Diagnostics.Debug.WriteLine($"*** load_xml({toolbox_path})");
      try
      {
        if (GeoprocessingModule.getIGPService() is Internal.DesktopService.IGPService gpService)
        {
          return gpService.QueryTools(toolbox_path);
        }
        else
        {
          System.Diagnostics.Debug.Assert(false);
          return _failed_toolbox;
        }
      }
      catch
      {
        return _failed_toolbox;
      }
    }
    #endregion
  }
}
