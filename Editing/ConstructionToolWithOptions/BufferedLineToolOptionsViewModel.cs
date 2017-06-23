// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using ArcGIS.Desktop.Editing;
using System.Windows.Media;

namespace ConstructionToolWithOptions
{
  internal class BufferedLineToolOptionsViewModel : EmbeddableControl, IEditingCreateToolControl
  {
    public BufferedLineToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }


    internal const string BufferOptionName = "Buffer";
    internal const double DefaultBuffer = 25.0;

    private ToolOptions ToolOptions { get; set; }
    private void InitializeOptions()
    {
      // no options
      if (ToolOptions == null)
        return;

      // if buffer exists in options, retrieve it
      if (ToolOptions.ContainsKey(BufferOptionName))
        _buffer = (double)ToolOptions[BufferOptionName];
      else
      {
        // otherwise assign the default value and add to the ToolOptions dictionary
        _buffer = DefaultBuffer;
        ToolOptions.Add(BufferOptionName, _buffer);
        // ensure options are notified that changes have been made
        NotifyPropertyChanged(BufferOptionName);
      }
    }

    // binds in xaml
    private double _buffer;
    public double Buffer
    {
      get { return _buffer; }
      set
      {
        if (SetProperty(ref _buffer, value))
        {
          _isDirty = true;
          _isValid = true;
          // add/update the buffer value to the tool options
          if (!ToolOptions.ContainsKey(BufferOptionName))
            ToolOptions.Add(BufferOptionName, value);
          else
            ToolOptions[BufferOptionName] = value;
          // ensure options are notified
          NotifyPropertyChanged(BufferOptionName);
        }
      }
    }

    #region IEditingCreateToolControl

    /// <summary>
    /// Gets the optional icon that will appear in the active template pane as a button selector.  
    /// If returning null then the Tool's small image that is defined in DAML will be used. 
    /// </summary>
    ImageSource IEditingCreateToolControl.ActiveTemplateSelectorIcon => null;

    private bool _isValid;
    /// <summary>
    /// Set this flag to indicate if too options are valid and ready for saving.
    /// </summary>
    /// <remarks>
    /// When this IEditingCreateToolControl is being displayed in the Template Properties
    /// dialog, calling code will use this property to determine if the current Template
    /// Properties may be saved.
    /// </remarks>
    bool IEditingCreateToolControl.IsValid => _isValid;

    private bool _isDirty;
    /// <summary>
    /// Set this flag when any tool options have been changed.
    /// </summary>
    /// <remaarks>
    /// When this IEditingCreateToolControl is being displayed in the Template Properties
    /// dialog, the calling code will use this property to determine if any changes have
    /// been made.  
    /// </remaarks>
    bool IEditingCreateToolControl.IsDirty => _isDirty;

    /// <summary>
    /// Gets if the contents of this control is auto-Opened in the Active Template Pane when the 
    /// tool is activated.
    /// </summary>
    /// <param name="toolID">the ID of the current tool</param>
    /// <returns>True the active template pane will be opened to the tool's options view.
    /// False, nothing in the active template pane changes when the tool is selected.</returns>
    bool IEditingCreateToolControl.AutoOpenActiveTemplatePane(string toolID)
    {
      return true;
    }

    /// <summary>
    /// Called just before ArcGIS.Desktop.Framework.Controls.EmbeddableControl.OpenAsync
    /// when this IEditingCreateToolControl is being used within the ActiveTemplate pane.
    /// </summary>
    /// <param name="options">tool options obtained from the template for the given toolID</param>
    /// <returns>true if the control is to be displayed in the ActiveTemplate pane.. False otherwise</returns>
    bool IEditingCreateToolControl.InitializeForActiveTemplate(ToolOptions options)
    {
      // assign the current options
      ToolOptions = options;
      // initialize the view
      InitializeOptions();
      return true;    // true <==> do show me in ActiveTemplate;   false <==> don't show me
    }

    /// <summary>
    /// Called just before ArcGIS.Desktop.Framework.Controls.EmbeddableControl.OpenAsync
    /// when this IEditingCreateToolControl is being used within the Template Properties
    /// dialog
    /// </summary>
    /// <param name="options">tool options obtained from the template for the given toolID</param>
    /// <returns>true if the control is to be displayed in Template Properties. False otherwise</returns>
    bool IEditingCreateToolControl.InitializeForTemplateProperties(ToolOptions options)
    {
      return false;     // don't show the options in template properties
    }

    #endregion

  }
}
