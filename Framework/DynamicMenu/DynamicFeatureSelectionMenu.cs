//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;


namespace FeatureDynamicMenu
{


    internal class DynamicFeatureSelectionMenu : DynamicMenu
    {
        public delegate void FeatureSelectedDelegate(BasicFeatureLayer layer, long oid);

        private readonly FeatureSelectedDelegate _delegate = null;
        private readonly List<Tuple<BasicFeatureLayer, long>> _selectedFeatures = new List<Tuple<BasicFeatureLayer, long>>();

       public DynamicFeatureSelectionMenu()
        {
            _delegate = OnFeatureSelected;
        }
        protected override void OnPopup()
        {
            _selectedFeatures.Clear();
            
            if (FeatureSelectionDynamic.FeatureSelection.Count == 0)
            {
                this.Add("No features found", "", false, true, true);
            }
            else
            {
                Add("Select item to flash the feature:", "", false, true, true);
                foreach (var kvp in FeatureSelectionDynamic.FeatureSelection)
                {
                    string layer = kvp.Key.Name;
                    string imageFile = GetImageFileName(kvp.Key);
                    var oids = kvp.Value;
                    foreach (var oid in oids)
                    {
                        Add(string.Format("{0}: oid {1}", layer, oid),
                            $"pack://application:,,,/FeatureDynamicMenu;Component/Images/{imageFile}", false, true, false, _delegate, kvp.Key, oid);
                        //This is a hack here                        
                        _selectedFeatures.Add(new Tuple<BasicFeatureLayer, long>(kvp.Key, oid));                                                
                    }                    
                }
            }

        }

        private string GetImageFileName(BasicFeatureLayer layer)
        {
            string imageFileName = "";
            switch (layer.ShapeType)
            {
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint:
                    imageFileName = "esri_PntFeature.png";
                    break;
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryLine:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryPath:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryCircularArc:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryEllipticArc:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultiPatch:
                    imageFileName = "esri_LinFeature.png";
                    break;
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryEnvelope:
                case ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon:
                    imageFileName = "esri_PolFeature.png";
                    break;
                default:
                    imageFileName = "esri_PntFeature.png";
                    break;
            }

            return imageFileName;
        }
        
        void OnFeatureSelected(BasicFeatureLayer layer, long oid)
        {
            var mapView = MapView.Active;
            mapView?.FlashFeature(layer, oid);
            Thread.Sleep(1000);
            mapView?.FlashFeature(layer, oid);
            System.Windows.Point mousePnt = MouseCursorPosition.GetMouseCursorPosition();
            var popupDef = new PopupDefinition()
            {
                Append = true,      // if true new record is appended to existing (if any)
                Dockable = true,    // if true popup is dockable - if false Append is not applicable
                Position = mousePnt,  // Position of top left corner of the popup (in pixels)
                Size = new System.Windows.Size(200, 400)    // size of the popup (in pixels)
            };
            //Show pop-up of feature
            mapView?.ShowPopup(layer, oid, popupDef);
        }
    }

   

}
