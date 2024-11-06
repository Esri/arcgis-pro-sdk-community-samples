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

namespace LocateNonspatialObject
{
  /// <summary>
  /// This example demonstrates how to obtain spatial ancestors or containers for a non-spatial object in a Utility Network.
  /// </summary>
  /// <remarks>
  /// For sample data, download CommunitySampleData-NetworkDiagrams-mm-dd-yyyy.zip from https://github.com/Esri/arcgis-pro-sdk-community-samples/releases and unzip it into c:\. We will be using the C:\Data\CreateDiagramWithACustomLayout\Communications_UtilityNetwork.geodatabase as the sample data for the addin.
  /// 1. In Visual Studio open this solution and then rebuild the solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. Add Utility Network to the map from Communications_UtilityNetwork.geodatabase
  /// 4. Then, click the LocateObject button.
  /// ![UI] (Screenshots/Screenshot1.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("LocateNonspatialObject_Module");

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
