/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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
using System.Windows.Input;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilder
{
  /// <summary>
  /// This sample demonstrates how to construct multipatch geometries using the MultipatchBuilderEx class found in the ArcGIS.Core.Geometry namespace. 
  /// The MultipatchBuilderEx class also allows modifying the multipatch geometry including the materials and textures.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.    
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project which contains the required data needed for this sample.
  /// 1. Open the "Portland 3D City" mapview.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. One of the building footprint polygons has already been selected.  Select 'Glass' from the texture drop down and then click the 'Create Multipatch Geometry' button to generate the Multipatch geomtry for the selected footprint. 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Select the next building footprint and change the texture dropdown to 'Red-solid'. Then click the 'Create Multipatch Geometry' button to generate the Multipatch geomtry for this footprint. 
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Select the next building footprint and change the texture dropdown to 'Gray-solid'. Then click the 'Create Multipatch Geometry' button to generate the Multipatch geomtry for this footprint. 
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MultipatchBuilderEx_Module"));
            }
        }

        public static async Task<CIMSymbolReference> GetDefaultMeshSymbolAsync()
        {
            // find a 3d symbol
            var spi = Project.Current.GetItems<StyleProjectItem>().First(s => s.Name == "ArcGIS 3D");
            if (spi == null)
                return null;

            // create new structural features
            var sym = await QueuedTask.Run<CIMSymbolReference>(() =>
            {
                var style_item = spi.SearchSymbols(StyleItemType.MeshSymbol, "").FirstOrDefault(si => si.Key == "White (use textures) with Edges_Material Color_12");
                var symbol = style_item?.Symbol as CIMMeshSymbol;
                if (symbol == null)
                    return null;
                var meshSymbol = symbol.MakeSymbolReference();
                return meshSymbol;
            });
            return sym;
        }

        internal static long NewMultipatchOID = -1;
        internal static Multipatch NewMultipatch = null;
        internal static string SelectedTexture;
        internal static Multipatch MultiPatch;
        private static IDisposable _screenGraphic = null;
        private static CIMSymbolReference _symRef = null;

        internal static void UpdateOverlay (Multipatch multiPatch)
        {
            if (_symRef == null) _symRef = GetDefaultMeshSymbolAsync().Result;
            MultiPatch = multiPatch;
            var multiPatchGraphic = new CIMMultiPatchGraphic()
            {
                MultiPatch = multiPatch,
                Symbol = _symRef
            };
            if (_screenGraphic != null) _screenGraphic.Dispose();
            _screenGraphic = MapView.Active.AddOverlay(multiPatchGraphic);           
        }

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
