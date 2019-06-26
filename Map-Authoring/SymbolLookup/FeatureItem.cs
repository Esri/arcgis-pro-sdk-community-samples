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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SymbolLookup
{
    /// <summary>
    /// Represents the selected feature item
    /// </summary>
    internal class FeatureItem
    {
       

        public FeatureItem(FeatureLayer featureLayer, long oid, CIMSymbol cimSymbol)
        {
            _featureLayer = featureLayer;
            _layerName = featureLayer.Name;
            _oid = oid.ToString();
            _cimSymbol = cimSymbol;
            var si = new SymbolStyleItem()
            {
                Symbol = cimSymbol,
                PatchHeight = 32,
                PatchWidth = 32
            };
            var bm = si.PreviewImage as BitmapSource;
            bm.Freeze();
            _symbolImageSource = bm;

        }

        private FeatureLayer _featureLayer;
        public FeatureLayer MapFeatureLayer => _featureLayer;

        private string _layerName;
        public string LayerName => _layerName;
        private string _oid;
        public string OID => _oid;
        private CIMSymbol _cimSymbol;
        private ImageSource _symbolImageSource;

        public CIMSymbol MapCIMSymbol => _cimSymbol;

        public ImageSource SymbolImageSource => _symbolImageSource;
    }
}
