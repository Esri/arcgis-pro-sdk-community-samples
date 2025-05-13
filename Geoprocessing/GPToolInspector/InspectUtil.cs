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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GPToolInspector
{
  using LocalResources = ArcGIS.Desktop.Internal.GeoProcessing.LocalResources;

  internal class InspectUtil
  { 
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


    static Lazy<cached_tooloxes> _all_systemTools = new Lazy<cached_tooloxes>(() =>
    {
      var system_tools_xml = check_system_cache();
      if (system_tools_xml != null)
        return new cached_tooloxes(system_tools_xml);
      //create cache file
      var progress = new Framework.Threading.Tasks.ProgressDialog(LocalResources.GpResources.CreatingCache);
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
        // not used
        //if (Internal.Framework.FrameworkApplication.IsRunningAsProProductType(Internal.Framework.ProProductType.ProductDrone2Map))
        //{
        //  //d2m has a subset of tools
        //  minTbCount = 5;
        //}
        if (tb_count >= minTbCount && xml_doc != null)
          //dump2system_cache(xml_doc);
          System.Diagnostics.Trace.WriteLine("dump2system_cache(xml_doc);");
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
        ArcGIS.Desktop.Internal.Framework.DialogManager.ShowMessageBox(LocalResources.GpResources.CreatingCacheFailedMessage);
        return new cached_tooloxes(null);
      }
      finally
      {
        progress.Hide();
        progress.Dispose();
        Internal.Framework.Events.UpdateSearchIndexEvent.Publish(new Internal.Framework.Events.UpdateSearchIndexEventArgs(new GeoprocessingToolsComponent()));
      }
    }, LazyThreadSafetyMode.ExecutionAndPublication);
  }
}
