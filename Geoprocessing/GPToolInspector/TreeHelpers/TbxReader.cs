/*

   Copyright 2026 Esri

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
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Internal.Core.Assistant;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static ArcGIS.Core.Data.NetworkDiagrams.AngleDirectedDiagramLayoutParameters;
using static ArcGIS.Desktop.Internal.Mapping.Views.PropertyPages.Map.TransformationViewModel;
using static GPToolInspector.TreeHelpers.TbxReader;

namespace GPToolInspector.TreeHelpers
{
  internal class TbxReader
  {
    public static readonly string ToolBoxContentFileName = "toolbox.content";
    public static readonly string ToolBoxContentRcFileName = "toolbox.content.rc";
    public static readonly string ToolContentFileName = "tool.content";
    public static readonly string ToolContentRcFileName = "tool.content.rc";
    public static readonly string ToolKeyWordsFileName = "tool.keywords.rc";

    internal static int _productCode = -1;

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
    }

    /// <summary>
    /// read all bytes from a file or internal zip file, skip UTF-8 BOM
    /// </summary>
    internal static ReadOnlySpan<byte> ReadUtf8(string path)
    {
      System.IO.Stream stm = null;
      try
      {
        stm = System.IO.File.OpenRead(path);
        using var ms = new System.IO.MemoryStream();
        stm.CopyTo(ms);
        var buff = ms.GetBuffer();
        //check and skip BOM
        if (ms.Length > 2 && buff[0] == 0xEF && buff[1] == 0xBB && buff[2] == 0xBF)
          return new(buff, 3, (int)ms.Length - 3);
        return buff;
      }
      catch { return null; }
      finally { stm?.Dispose(); }
    }

    #region ToolBoxSet classes

    record json_toolset(string[] tools);

    record json_toolbox_content(
      string alias,
      string displayname,
      Dictionary<string, json_toolset> toolsets
    );

    record json_rc_map
    {
      public Dictionary<string, string> map { get; init; }
    }

    public class ToolSet : IComparable
    {
      public string ToolName { get; set; }
      public List<ToolSetTool> Tools { get; set; }

      public int CompareTo(object obj)
      {
        var compTo = obj as ToolSet;
        if (compTo == null) return 0;
        return ToolName.CompareTo(compTo.ToolName);
      }

      internal void SetToolsFromStrings(string contentBasePath, string[] tools)
      {
        Tools = [];
        foreach (var tool in tools)
        {
          ToolSetTool addTool = new()
          {
            ToolName = tool,
            ToolRelativePath = tool + ".tool"
          };
          var parts = tool.Split(':');
          if (parts.Length > 1)
          {
            addTool.ToolName = parts[0];
            addTool.ToolRelativePath = parts[1];
          }
          var tbxPath = System.IO.Path.Combine(contentBasePath, addTool.ToolRelativePath);
          var (Ok, ErrorMessage) = TbxToolInfo.ReadToolContent(tbxPath, 
                                    out TbxToolInfo toolDescription);
          if (!Ok)
          {
            MessageBox.Show(ErrorMessage);
          }
          addTool.ToolDisplayname = toolDescription.displayname;
          Tools.Add(addTool);
        }
      }
    }

    public class ToolSetTool
    {
      public string ToolName { get; set; }
      public string ToolDisplayname { get; set; }
      public string ToolRelativePath { get; set; }
      public TbxToolInfo TbxToolInfoPython { get; set; }
    }

    internal class ToolBoxSet 
    {
      public string ToolBoxAlias { get; set; }
      public string ToolBoxName { get; set; }
      public string ToolBoxPath { get; set; }
      public List<ToolSet> ToolSets { get; set; }

      public ToolBoxSet()
      {
        ToolBoxAlias = string.Empty;
        ToolBoxName = string.Empty;
        ToolBoxPath = string.Empty;
        ToolSets = [];
      }
      public ToolBoxSet(string alias, string tbxName, string tbxPath)
      {
        ToolBoxAlias = alias;
        ToolBoxName = tbxName;
        ToolBoxPath = tbxPath;
        ToolSets = [];
      }
    }

		#endregion ToolBoxSet classes

    static public string InstallPath { get; } = System.IO.Path.GetDirectoryName(       // <InstallPath>/bin
                                                  System.IO.Path.GetDirectoryName(     // <InstallPath>/bin/Extensions
                                                    System.IO.Path.GetDirectoryName(   // <InstallPath>/bin/Extensions/GeoProcessing
                                                      System.IO.Path.GetDirectoryName( // <InstallPath>/bin/Extensions/GeoProcessing/ArcGIS.Desktop.GeoProcessing.dll
                                                        System.Reflection.Assembly.GetCallingAssembly().Location
                                                 ))));

    public static string gpHelpPathEN { get; } = System.IO.Path.Combine(InstallPath, "Resources", "Help", "gp");

    static Lazy<string> _gpHelpPath = new(() =>
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

    internal static IDictionary<string, string> ReadContentRcFile(
      string path, 
      string path_localized = null)
    {
      try
      {
        var utf8 = ReadUtf8(path);
        var reader = new System.Text.Json.Utf8JsonReader(utf8, new System.Text.Json.JsonReaderOptions() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, MaxDepth = 128 });
        var a_table = System.Text.Json.JsonSerializer.Deserialize<json_rc_map>(ref reader);
        if (string.IsNullOrEmpty(path_localized))
          return a_table.map;
        //load localized toolbox: folder based
        System.Diagnostics.Debug.WriteLine(path_localized);
        var b_table = ReadContentRcFile(path_localized);
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

    internal static (string Alias, string Title, IEnumerable<ToolSet> ToolSets) 
      ReadToolBoxHeader (string toolBoxBasePath)
    {
      List<(string Name, string Dir)> tools = [];
      List<TbxToolInfo> tbxToolInfoList = [];
      var contentFilePath = System.IO.Path.Combine(toolBoxBasePath, ToolBoxContentFileName);
      json_toolbox_content toolbox_content = null;
      if (File.Exists(contentFilePath))
      {
        // here we have a GeoProcessing Toolbox
        var keyWordMap = TbxUtils.GetToolBoxContentRcMap(toolBoxBasePath);
        var json = System.IO.File.ReadAllText(contentFilePath, Encoding.UTF8);
        json = TbxReader.ReplaceUsingMap(json, keyWordMap);
        toolbox_content = System.Text.Json.JsonSerializer.Deserialize<json_toolbox_content>(json, TbxUtils.JsonOpt);
      }
      else 
      {
        // here we have a .pyt file
        // check if we have a .pyt file with embedded content
        var pytFilePath = System.IO.Path.Combine(toolBoxBasePath, "toolboxes");
        var pytFiles = Directory.GetFiles(pytFilePath, "*.pyt", SearchOption.TopDirectoryOnly);
        if (pytFiles.Length == 0) throw new FileNotFoundException($"No {ToolBoxContentFileName} found for toolbox {toolBoxBasePath} and no .pyt file found in expected location {pytFilePath}");
        foreach (var file in pytFiles)
        {
          using (Py.GIL())
          {
            using var scope = Py.CreateScope();
            // Get the python file as raw text
            string code = File.ReadAllText(file);
            // Compile the code/file
            var scriptCompiled = PythonEngine.Compile(code, file);
            // Execute the compiled python so we can start calling it.
            scope.Execute(scriptCompiled);
            // Get the class in python
            dynamic toolboxClass = scope.GetAttr("Toolbox"); 
            // create an instance of the Toolbox class (this calls __init__ in python)
            using PyObject toolboxInstance = toolboxClass.Invoke();
            var toolboxLabel = toolboxInstance.GetAttr("label").As<string>();
            var toolboxAlias = toolboxInstance.GetAttr("alias").As<string>();
            toolbox_content = new json_toolbox_content(toolboxAlias, toolboxAlias, []);
            // for python scripts we have to collect all properties by running the class script
            TbxToolInfo tbxToolInfo = new()
            {
              displayname = toolboxAlias,
              description = toolboxLabel,
              Category = toolboxLabel,
              @params = [],
              FileName = toolboxAlias,
              environments = []
            };
            using PyObject toolboxToolList = toolboxInstance.GetAttr("tools");
            for (int idx = 0; idx < toolboxToolList.Length(); idx++)
            {
              using PyObject toolClass = toolboxToolList[idx];
              using PyObject toolClassInstance = toolClass.Invoke(); // call __init__ for each tool to ensure properties are populated, we don't actually need the instance for anything else
              using PyObject paramInfoList = toolClassInstance.InvokeMethod("getParameterInfo");
              var toolName = toolClassInstance.GetAttr("label").As<string>();
              tbxToolInfo.displayname = toolName;
              toolbox_content.toolsets.Add(toolName, new json_toolset([]));
              for (int idxParam = 0; idxParam < paramInfoList.Length(); idxParam++)
              {
                using PyObject param = paramInfoList[idxParam];
                var paramName = param.GetAttr("name").As<string>();
                var paramDisplayName = param.GetAttr("displayName").As<string>();
                var paramDataType = param.GetAttr("datatype").As<string>();
                var paramParameterType = param.GetAttr("parameterType").As<string>();
                var paramDirection = param.GetAttr("direction").As<string>();
                tbxToolInfo.@params.Add(paramName, 
                  new Param(paramName, paramDirection, 
                            paramParameterType, paramDisplayName, 
                            new Datatype() { type = paramDataType}, 
                            new Domain() { }));
              }
              System.Diagnostics.Trace.WriteLine($@"### Created python toolbox instance from {file}");
            }
            tbxToolInfoList.Add(tbxToolInfo);
          }
        }
      }
      List<ToolSet> toolSets = [];
      List<string> names = [];
      var displayname = toolbox_content.displayname;
      System.Diagnostics.Trace.WriteLine($@"### {toolBoxBasePath} {displayname}: {toolbox_content.toolsets.Count()}");
			foreach (var toolset in toolbox_content.toolsets)
			{
				var name = toolset.Key;
				names.Add(name);
				var newToolSet = new ToolSet
				{
					ToolName = name == "<root>" ? string.Empty : name
				};
        if (tbxToolInfoList.Count > 0)
        {
          // Python script
          newToolSet.Tools = tbxToolInfoList.Select(it => new ToolSetTool() { ToolName = it.displayname, ToolRelativePath = it.type + ".tool", ToolDisplayname = it.FileName, TbxToolInfoPython = it }).ToList();
        }
        else
        {
          // regular toolbox with .tool files
          newToolSet.SetToolsFromStrings(Directory.GetParent(contentFilePath).FullName, toolset.Value.tools);
        }
				toolSets.Add(newToolSet);
			}
			return (toolbox_content.alias, displayname, toolSets);
    }

		internal static IEnumerable<(string Name, string Path, TbxToolInfo TbxToolInfoPython, bool IsTool)>
			ProcessHeader(string toolBoxBasePath, IEnumerable<ToolSet> ToolSets, string currentFolder)
		{
      List<(string Name, string Path, TbxToolInfo TbxToolInfoPython, bool IsTool)> children = [];
      var addTools = string.IsNullOrEmpty(currentFolder);
      if (!addTools)
      {
        var folderParts = currentFolder.Split('\\');
        addTools = folderParts.Length <= 1;
      }
      if (addTools)
      {
        foreach (var toolset in ToolSets)
        {
          if (string.IsNullOrEmpty(toolset.ToolName) || toolset.ToolName == "<root>") continue;
          if (string.IsNullOrEmpty(currentFolder))
          {
            // folders at the 'root'
            var parts = toolset.ToolName.Split('\\');
            if (parts.Length <= 1)
              children.Add((toolset.ToolName, toolset.ToolName, null, false));
            continue;
          }
          if (toolset.ToolName.StartsWith(currentFolder))
          {
            var parts = toolset.ToolName.Split('\\');
            if (parts.Length <= 1) continue;
            var name = parts[1];
            children.Add((toolset.ToolName, toolset.ToolName, null, false));
          }
        }
      }
      // add tools
      foreach (var toolset in ToolSets)
      {
        if (string.IsNullOrEmpty(currentFolder))
        {
          // tools at the root level
          if (string.IsNullOrEmpty(toolset.ToolName) || toolset.ToolName == "<root>")
            children.AddRange(toolset.Tools.Select(it => (it.ToolName, it.ToolRelativePath, it.TbxToolInfoPython, true)));
          continue;
        }
        if (toolset.ToolName == currentFolder)
        {
          children.AddRange(toolset.Tools.Select(it => (it.ToolName, it.ToolRelativePath, it.TbxToolInfoPython, true)));
        }
      }
      return children;
		}

		#region Utility functions

		internal static string ReplaceUsingMap(string json, IDictionary<string, string> keyWordMap)
    {
      var output = new StringBuilder(json);

      foreach (var kvp in keyWordMap)
        output.Replace(@"$rc:"+kvp.Key, EscapeForJson (kvp.Value));

      return output.ToString();
    }

    internal static string EscapeForJson(string sUnescaped)
    {
      if (sUnescaped == null || sUnescaped.Length == 0)
      {
        return "";
      }

      char c = '\0';
      int i;
      int len = sUnescaped.Length;
      StringBuilder sb = new StringBuilder(len + 4);
      string t;
      for (i = 0; i < len; i += 1)
      {
        c = sUnescaped[i];
        switch (c)
        {
          case '\\':
          case '"':
            sb.Append('\\');
            sb.Append(c);
            break;
          case '/':
            sb.Append('\\');
            sb.Append(c);
            break;
          case '\b':
            sb.Append("\\b");
            break;
          case '\t':
            sb.Append("\\t");
            break;
          case '\n':
            sb.Append("\\n");
            break;
          case '\f':
            sb.Append("\\f");
            break;
          case '\r':
            sb.Append("\\r");
            break;
          default:
            if (c < ' ')
            {
              t = "000" + String.Format("X", c);
              sb.Append("\\u" + t.Substring(t.Length - 4));
            }
            else
            {
              sb.Append(c);
            }
            break;
        }
      }
      return sb.ToString();
    }

    #endregion Utility functions
  }
}
