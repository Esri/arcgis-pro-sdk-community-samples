// Copyright 2019 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
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
using System.Xml;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows;

namespace ImpersonateMapPane
{
  internal class ImpersonateMapPane1ViewModel : TOCMapPaneProviderPane
  {
    private const string _viewPaneID = "ImpersonateMapPane_ImpersonateMapPane1";
    private string _viewDefinition = "";
    private string _customValue = "";

    /// <summary>
    /// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
    /// </summary>
    public ImpersonateMapPane1ViewModel(CIMView view)
      : base(view)
    {
      //Set to true to change docking behavior
      _dockUnderMapView = false;
      //At 2.x - using Xml
      //_viewDefinition = FormatXml(view.ToXml());

      _viewDefinition = view.ToJson();//Json at 3.0
    }

    /// <summary>
    /// Create a new instance of the pane.
    /// </summary>
    internal static ImpersonateMapPane1ViewModel Create(MapView mapView)
    {
      var view = new CIMGenericView();
      view.ViewType = _viewPaneID;
      view.ViewProperties = new Dictionary<string, object>();
      view.ViewProperties["MAPURI"] = mapView.Map.URI;

      var newPane = FrameworkApplication.Panes.Create(_viewPaneID, new object[] { view }) as ImpersonateMapPane1ViewModel;
      newPane.Caption = $"Impersonate {mapView.Map.Name}";
      return newPane;
    }

    #region Pane Overrides

    /// <summary>
    /// Must be overridden in child classes used to persist the state of the view to the CIM.
    /// </summary>
    public override CIMView ViewState
    {
      get
      {
        _cimView.InstanceID = (int)InstanceID;
        //Cache content in _cimView.ViewProperties
        //((CIMGenericView)_cimView).ViewProperties["custom"] = "custom value";
        //((CIMGenericView)_cimView).ViewProperties["custom2"] = "custom value2";
        

        //At 3.0 - use view.ViewProperties
        ((CIMGenericView)_cimView).ViewProperties["custom"] = _customValue;
        //can also include serialized content
        ((CIMGenericView)_cimView).ViewProperties["self"] = _cimView.ToJson();
        //etc
        ((CIMGenericView)_cimView).ViewProperties["foo"] = "bar";
        return _cimView;
      }
    }

    /// <summary>
    /// Called when the pane is initialized.
    /// </summary>
    protected async override Task InitializeAsync()
    {
      var uri = ((CIMGenericView)_cimView).ViewProperties["MAPURI"] as string;
      //Custom content may originally have been persisted in the view.ViewXML property
      //by the addin at 2.x.

      //When a 2.x aprx is opened, any custom view content persisted in the aprx
      //in the _cimView.ViewXML property will be moved to the ViewProperties dictionary
      //and added as the key "ViewXML"
      if (((CIMGenericView)_cimView).ViewProperties.ContainsKey("ViewXML"))//Legacy content
      {
        //This is content that would have been persisted at 2.x in the
        //deprecated _cimView.ViewXML property...
        var previous_custom_content = ((CIMGenericView)_cimView).ViewProperties["ViewXML"] as string;
        //TODO - handle previous content from 2.x
        //...

        //For the sample, we convert to JSON
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(previous_custom_content);
        //This is particular example is using Newtonsoft...
        _customValue = $"From 2.x: {Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc)}";

        //remove the legacy content
        ((CIMGenericView)_cimView).ViewProperties.Remove("ViewXML");
      }
      //3.0 check ViewProperties for any other custom keys
      if (((CIMGenericView)_cimView).ViewProperties.ContainsKey("custom"))
      {
        _customValue = ((CIMGenericView)_cimView).ViewProperties["custom"] as string;
      }
      //etc....
      if (string.IsNullOrEmpty(_customValue))
      {

      }
      await this.SetMapURI(uri);
      await base.InitializeAsync();
    }

    /// <summary>
    /// Called when the pane is uninitialized.
    /// </summary>
    protected async override Task UninitializeAsync()
    {
      await base.UninitializeAsync();
    }

    #endregion Pane Overrides

    public string ViewDefinition
    {
      get
      {
        return _viewDefinition;
      }
    }

    //private static string FormatXml(string xml)
    //{
    //  var doc = new XmlDocument();
    //  var sb = new StringBuilder();
    //  doc.LoadXml(xml);
    //  var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
    //  doc.Save(XmlWriter.Create(sb, xmlWriterSettings));
    //  return sb.ToString();
    //}
  }

  /// <summary>
  /// Button implementation to create a new instance of the pane and activate it.
  /// </summary>
  internal class ImpersonateMapPane1_OpenButton : Button
  {
    protected override void OnClick()
    {
      ImpersonateMapPane1ViewModel.Create(MapView.Active);
    }
  }
}
