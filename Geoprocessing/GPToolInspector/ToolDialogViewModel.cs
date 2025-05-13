/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using GPToolInspector.TreeHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GPToolInspector
{
  public class ParamGrid
  {
    public bool Output { get; set; }
    public string Type { get; set; }
    public string CodeName { get; set; }
    public string Name { get; set; }
    public string Explanation { get; set; }

    public string DataType { get; set; }

    public string Domain { get; set; }
  }

  /// <summary>
  /// View model for the Tool Dialog
  /// </summary>
  public class ToolDialogViewModel : PropertyChangedBase
  {
    /// <summary>
    /// Ctor
    /// </summary>
    public ToolDialogViewModel()
    {
    }

    #region Properties

    private TbxToolInfo _TbxToolInfo;

    /// <summary>
    /// ToolBox Info for the current selected tool
    /// </summary>
    public TbxToolInfo TbxToolInfo
    {
      get { return _TbxToolInfo; }
      set
      {
        SetProperty(ref _TbxToolInfo, value);
      }
    }

    /// <summary>
    /// Tool name
    /// </summary>
    public string Name
    {
      get { return TbxToolInfo?.displayname; }
    }

    /// <summary>
    /// Tool summary description
    /// </summary>
    public string Summary
    {
      get { return TbxToolInfo?.description; }
    }

    private bool _IncludeToolEnvironments;

    /// <summary>
    /// True to include tool environment setttings in the code snippet
    /// </summary>
    public bool IncludeToolEnvironments
    {
      get
      {
        return _IncludeToolEnvironments;
      }
      set
      {
        SetProperty(ref _IncludeToolEnvironments, value);
        NotifyPropertyChanged(() => SourceString);
      }
    }

    private bool _IncludeOptionalParameters;

    /// <summary>
    /// True to include optional parameters in the code snippet
    /// </summary>
    public bool IncludeOptionalParameters
    {
      get
      {
        return _IncludeOptionalParameters;
      }
      set
      {
        SetProperty(ref _IncludeOptionalParameters, value);
        NotifyPropertyChanged(() => SourceString);
      }
    }

    /// <summary>
    /// The code snippet to be copied to the clipboard
    /// </summary>
    public string CodeSnippet { get; set; }

    /// <summary>
    /// Source string for the HTML control
    /// </summary>
    public string SourceString
    {
      get
      {
        var html = Properties.Resources.HtmlTemplate;
        CodeSnippet = GetCodeName();
        if (IncludeToolEnvironments) CodeSnippet += GetCodeEnvironment();
        CodeSnippet += GetCodeParameters(IncludeOptionalParameters, out string outputParameter);
        CodeSnippet += GetCodeExecution(IncludeToolEnvironments, string.Empty, outputParameter);
        html = html.Replace("$codehere$", CodeSnippet);
        return html;
      }
      set
      {
        System.Diagnostics.Trace.WriteLine($@"New Source string: {value}");
      }
    }

    /// <summary>
    /// Parameters for the tool
    /// </summary>
    /// <returns>Collection of all Parameters</returns>
    public ObservableCollection<ParamGrid> GetParameters()
    {
      var paramGrids = new ObservableCollection<ParamGrid>();
      foreach (var paramKeyValue in TbxToolInfo?.@params)
      {
        var param = paramKeyValue.Value;
        var dataType = param.datatype?.type;
        dataType ??= string.Join(',', param.datatype?.type);
        var domain = string.Empty;
        if (param.RealDomains != null)
          domain = string.Join(',', param.RealDomains);
        paramGrids.Add(new ParamGrid
        {
          Output = ((string.IsNullOrEmpty(param.direction) ? false : param.direction.StartsWith("out"))),
          Type = param.type,
          CodeName = paramKeyValue.Key,
          Name = param.ParamName,
          Explanation = param.description,
          DataType = dataType,
          Domain = domain
        });
      }
      return paramGrids;
    }

    #endregion Properties

    #region Html output

    private static readonly string _nl = System.Environment.NewLine;

    private string GetCodeExecution(bool withEnvironment, string prefix, string outputParameter)
    {
      var code = $@"{_nl}// Running tool: {TbxToolInfo.displayname}{_nl}";
      code += $@"{prefix}var toolName = ""{TbxToolInfo.ToolName}"";{_nl}";
      code += $@"{prefix}IGPResult gpResult = await Geoprocessing.ExecuteToolAsync(toolName, parameters{(withEnvironment ? ", envSettings" : string.Empty)});{_nl}";
      code += $@"{prefix}if (gpResult != null) {{{_nl}";
      code += $@"{prefix}  Geoprocessing.ShowMessageBox(gpResult.Messages, ""Geoprocessing Result"",{_nl}";
      code += $@"{prefix}      gpResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);{_nl}";
      if (string.IsNullOrEmpty(prefix))
      {
        if (!string.IsNullOrEmpty(outputParameter))
        {
          code += $@"  // get output for {TbxToolInfo.displayname}{_nl}";
          code += $@"  {outputParameter} = gpResult.ReturnValue;{_nl}";
        }
        code += $@"  // get multiple output results for {TbxToolInfo.displayname}{_nl}";
        code += $@"  if (gpResult.Values != null && gpResult.ValueTypes != null){_nl}";
        code += $@"  {{{_nl}";
        code += $@"    using (IEnumerator&lt;string&gt; enumeratValues = gpResult.Values.GetEnumerator()){_nl}";
        code += $@"    using (IEnumerator&lt;string&gt; enumeratValueTypes = gpResult.ValueTypes.GetEnumerator()){_nl}";
        code += $@"      while (enumeratValues.MoveNext() && enumeratValueTypes.MoveNext()){_nl}";
        code += $@"      {{{_nl}";
        code += $@"        System.Diagnostics.Trace.WriteLine($""Value: {{enumeratValues.Current}}, Type: {{enumeratValueTypes.Current}}"");{_nl}";
        code += $@"      }}{_nl}";
        code += $@"  }}{_nl}";
      }
      code += $@"{prefix}}}{_nl}";
      return code;
    }

    private string GetCodeParameters(bool includeOptionalParameters, out string outputParameter)
    {
      outputParameter = string.Empty;
      var code = $@"// Parameters {TbxToolInfo.displayname}{_nl}";
      List<string> paramList = [];
      foreach (var param in GetParameters().Where(p => !p.Output))
      {
        if (!includeOptionalParameters && param.Type == "optional") continue;
        var comment = FormatAsCodeComment(param.Explanation).Trim();
        var theType = param.Type != null ? $@" ({param.Type})" : "";
        code += $@"{_nl}// {param.CodeName}{theType}:{_nl}";
        if (!string.IsNullOrEmpty(comment)) code += $@"/* {comment}*/{_nl}";
        code += $@"// datatype: {param.DataType}{_nl}";
        code += $@"var {param.CodeName} = """";{_nl}";
        paramList.Add(param.CodeName);
      }

      code += $@"{_nl}var parameters = Geoprocessing.MakeValueArray({string.Join(',', paramList)});{_nl}";

      code += $@"{_nl}// Derived output for {TbxToolInfo.displayname}";

      foreach (var param in GetParameters().Where(p => p.Output))
      {
        var theType = param.Type != null ? $@" ({param.Type})" : "";
        code += $@"{_nl}// {param.CodeName}{theType}:{_nl}";
        code += $@"/* {FormatAsCodeComment(param.Explanation)}*/{_nl}";
        code += $@"// datatype: {param.DataType}{_nl}";
        code += $@"var {param.CodeName} = """";{_nl}";
        if (string.IsNullOrEmpty(outputParameter))
          outputParameter = $@"{param.CodeName}";
      }
      return code;
    }

    private string GetCodeEnvironment()
    {
      var code = string.Empty;
      if (TbxToolInfo.environments == null)
      {
        code += $@"// No environment settings for ""{TbxToolInfo.displayname}""{_nl}";
        return code;
      }
      code += $@"// Environment settings for ""{TbxToolInfo.displayname}""{_nl}";
      code += $@"// to override default Environment settings only specify desired settings and add the envSettings parameter to ExecuteToolAsync {_nl}";
      code += $@"var envSettings = Geoprocessing.MakeEnvironmentArray(";
      var first = true;
      foreach (var env in TbxToolInfo.environments)
      {
        code += (!first) ? $@",{_nl}" : _nl;
        code += $@"{"    "}{env}: null";
        first = false;
      }
      code += $@");{_nl}";
      return code;
    }

    private string GetCodeName()
    {
      var code = $@"// GP Tool: ""{TbxToolInfo.displayname}""{_nl}";
      code += GetCodeExecution(IncludeToolEnvironments, "//", string.Empty);
      code += Environment.NewLine;
      return code;
    }

    #endregion Html output

    #region Image Sources

    /// <summary>
    /// Close button imagesource
    /// </summary>
    public ImageSource CloseImageSrc
    {
      get
      {
        var imageSource = Application.Current.Resources["Close16"] as ImageSource;
        return imageSource;
      }
    }

    /// <summary>
    /// Copy Snippet button imagesource
    /// </summary>
    public ImageSource CopySnippetImageSrc
    {
      get
      {
        var imageSource = Application.Current.Resources["EditCopy16"] as ImageSource;
        return imageSource;
      }
    }

    #endregion Image Sources

    #region Commands

    /// <summary>
    /// Command to copy the code snippet to the clipboard
    /// </summary>
    public ICommand CmdCopySnippet => new RelayCommand((proWindow) =>
    {
      var txt = System.Net.WebUtility.HtmlDecode(CodeSnippet);
      Clipboard.SetText(txt);
    }, () => true);


    /// <summary>
    /// Command to close the dialog
    /// </summary>
    public ICommand CmdClose => new RelayCommand((proWindow) =>
      {
        (proWindow as ProWindow).DialogResult = true;
        (proWindow as ProWindow).Close();
      }, () => true);

    #endregion

    #region Helpers

    private string FormatAsCodeComment(string commentText)
    {
      if (string.IsNullOrEmpty(commentText)) return string.Empty;
      return commentText?.Replace(_nl, $@"{_nl}\\");
    }

    #endregion

  }
}

