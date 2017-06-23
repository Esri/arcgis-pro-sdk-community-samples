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

    /// <summary>
    /// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
    /// </summary>
    public ImpersonateMapPane1ViewModel(CIMView view)
      : base(view) {
      //Set to true to change docking behavior
      _dockUnderMapView = false;
      _viewDefinition = FormatXml(view.ToXml());
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
        //Cache content in _cimView.ViewProperties or in _cimView.ViewXML
        //_cimView.ViewXML = new XDocument(new XElement("Root",
        //new XElement("custom", "custom value"))).ToString(SaveOptions.DisableFormatting);
        return _cimView;
      }
    }

    /// <summary>
    /// Called when the pane is initialized.
    /// </summary>
    protected async override Task InitializeAsync()
    {
      var uri = ((CIMGenericView)_cimView).ViewProperties["MAPURI"] as string;
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

    /// <summary>
    /// Gets the underlying definition of the associated map
    /// </summary>
    public string ViewXML
    {
      get
      {
        return _viewDefinition;
      }
    }

    private static string FormatXml(string xml)
          {
      var doc = new XmlDocument();
      var sb = new StringBuilder();
      doc.LoadXml(xml);
      var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
      doc.Save(XmlWriter.Create(sb, xmlWriterSettings));
      return sb.ToString();
        }
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
