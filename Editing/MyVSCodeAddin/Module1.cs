using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyVSCodeAddin
{
  /// <summary>
  /// Creates 3D multipatch lots from selected parcel features through a dockpane that displays parcel identifiers and provides a configurable geometry resolution.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). Make sure that the Sample data is unzipped in c:\data and "C:\Data\Elevation\" is available.
  /// 1. The ArcGIS Pro project used for this sample is 'C:\Data\Elevation\HawaiiElevation.ppkx'
  /// 1. Follow the steps in the ProGuide to see how GitHub Copilot in VS Code can assist you in writing an addin for ArcGIS Pro.
  /// [ProGuide Visual Studio Code Addin Development](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Visual-Studio-Code-Addin-Development)
  /// </remarks>
  internal class Module1 : Module
{
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("MyVSCodeAddin_Module");

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
        //TODO - add your business logic
        //return false to ~cancel~ Application close
        return true;
    }

    #endregion Overrides
  }
}
