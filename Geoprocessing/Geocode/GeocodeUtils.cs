//Copyright 2017 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;


namespace Geocode
{
    internal static class GeocodeUtils
    {

        private static Dictionary<int, CIMPointGraphicHelper> _lookup = new Dictionary<int, CIMPointGraphicHelper>();
        /// <summary>
        /// Do a search for the contents of the specified URL
        /// </summary>
        /// <param name="text"></param>
        /// <param name="numResults"></param>
        /// <returns></returns>
        public static CandidateResponse SearchFor(string text, int numResults = 2)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", "GeocodeExample");
            wc.Headers.Add("referer", "GeocodeExample");
            wc.Encoding = System.Text.Encoding.UTF8;

            CandidateResponse geocodeResult = null;
            using (StreamReader sr = new StreamReader(wc.OpenRead(new Geocode.GeocodeURI(text, numResults).Uri),
                                                      System.Text.Encoding.UTF8, true))
            {
                string response = sr.ReadToEnd();
                if (ResponseIsError(response))
                {
                    //throw
                    throw new System.ApplicationException(response);
                }
                geocodeResult = ObjectSerialization.JsonToObject<CandidateResponse>(response);
            }
            return geocodeResult;
        }

        /// <summary>
        /// Check if the returned response is an error
        /// message from the AGS
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static bool ResponseIsError(string response)
        {
            return response.Substring(0, "{\"error\":".Length).CompareTo("{\"error\":") == 0;
        }

        #region Display
        /// <summary>
        /// Zoom to the specified location
        /// </summary>
        /// <param name="extent">The extent of the geocoded candidate</param>
        /// <returns></returns>
        public static Task ZoomToLocation(CandidateExtent extent)
        {
            return QueuedTask.Run(() =>
            {                
                ArcGIS.Core.Geometry.SpatialReference spatialReference = SpatialReferenceBuilder.CreateSpatialReference(extent.WKID);
                ArcGIS.Core.Geometry.Envelope envelope = EnvelopeBuilder.CreateEnvelope(extent.XMin, extent.YMin, extent.XMax, extent.YMax, spatialReference);

                //apply extent
                MapView.Active.ZoomTo(GeometryEngine.Instance.Expand(envelope, 3, 3, true));
            });
        }

        #endregion Display

        #region Graphics Support

        private static System.IDisposable _overlayObject = null;
     
        /// <summary>
        /// Add a point to the specified mapview
        /// </summary>
        /// <param name="point">The location of the graphic</param>
        /// <param name="mapView">The mapview to whose overlay the graphic will be added</param>
        /// <returns></returns>
        public static async void AddToMapOverlay(ArcGIS.Core.Geometry.MapPoint point, MapView mapView)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
               {
                   // Construct point symbol
                   symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Star);
               });

            //Get symbol reference from the symbol 
            CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

            await QueuedTask.Run(() =>
                {
                    _overlayObject = mapView.AddOverlay(point, symbolReference);
                });

        }

        /// <summary>
        /// All-in-one. Update the graphic on the overlay if it was previously added
        /// otherwise, make it and add it
        /// </summary>
        /// <param name="point">The new location to be added to the map</param>
        /// <param name="mapView"></param>
        /// <returns></returns>
        public static void UpdateMapOverlay(ArcGIS.Core.Geometry.MapPoint point, MapView mapView)
        {
            
            if (_overlayObject != null)
            {
                _overlayObject.Dispose();
                _overlayObject = null;

               AddToMapOverlay(point, mapView);
            }
            else
            {
                //first time
                AddToMapOverlay(point, mapView);
            }
        }

        /// <summary>
        /// Remove the Point Graphic from the specified mapview
        /// </summary>
        /// <param name="mapView"></param>
        public static void RemoveFromMapOverlay(MapView mapView)
        {
            if (_overlayObject != null)
            {
                _overlayObject.Dispose();
                _overlayObject = null;
            }
        }

        #endregion Graphics Support
    }
}
