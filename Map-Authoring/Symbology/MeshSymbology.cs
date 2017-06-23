/*

   Copyright 2017 Esri

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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
    internal static class MeshSymbology
    {

        public static async Task<Dictionary<CIMSymbol, string>> GetAll3DSymbolsAsync()
        {
            var lineSymbols = new Dictionary<CIMSymbol, string>
            {
                {await CreateMeshSymbol(), "Orange Mesh" },
                {await CreateProceduralMeshSymbol(), "Textures procedural" }

            };
            return lineSymbols;
        }
        #region Snippet Mesh material fill symbol
        /// <summary>
        /// Create a mesh symbol that can be applied to a multi-patch feature layer.
        /// </summary>
        /// <remarks>
        /// A mesh symbol is a CIMMeshSymbol object.  Define an array of CIMSymbolLayers which contains a CIMMaterialSymbol layer with the specified properties such as Color, etc.
        /// Assign this array of CIMSymbolLayers to the CIMMeshSybol.
        /// ![MeshSymbolOrange](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/mesh-material-orange.png)
        /// </remarks>
        /// <returns></returns>
        public static async Task<CIMSymbol> CreateMeshSymbol()
        {
            var meshSymbol = await QueuedTask.Run(() =>
           {
               CIMSymbolLayer[] materialSymbolLayer =
              {
                    new CIMMaterialSymbolLayer()
                    {
                        Color = ColorFactory.Instance.CreateRGBColor(230,152,0),
                        MaterialMode = MaterialMode.Multiply
                    }
               };
               var myMeshSymbol = new CIMMeshSymbol()
               {
                   SymbolLayers = materialSymbolLayer
               };
               return myMeshSymbol;
           });
            return meshSymbol;
        }
        #endregion

        #region Snippet Mesh procedural texture symbol
        /// <summary>
        /// Creates Mesh procedural symbol with various textures.
        /// ![MeshProceduralTexture](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/mesh-procedural-texture.png)
        /// </summary>
        /// <remarks>Note: The rule package used in this method can be obtained from the Sample Data included in the arcgis-pro-sdk-community-samples repository.</remarks>
        /// <returns></returns>
        private static readonly string _rulePkgPath = @"C:\Data\RulePackages\MultipatchTextures.rpk";
        public static async Task<CIMSymbol> CreateProceduralMeshSymbol()
        {
            var myProceduralSymbol = await QueuedTask.Run(() =>
            {                  
                CIMSymbolLayer[] proceduralSymbolLyr =
                {
                    new CIMProceduralSymbolLayer()
                    {
                        PrimitiveName = "Textures",
                        RulePackage = _rulePkgPath,
                        RulePackageName = "Textures",
                    }
                };
                var myMeshSymbol = new CIMMeshSymbol()
                {
                    SymbolLayers = proceduralSymbolLyr
                };

                return myMeshSymbol;
            });
            return myProceduralSymbol;
        }
        #endregion
    }
}
