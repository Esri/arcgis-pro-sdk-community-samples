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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using BingStreetside.Utility;

namespace BingStreetside
{
    /// <summary>
    /// This sample demonstrates the usagle of the WebBrowser control and how to interface between C# and HTML5/JavaScript and vise versa.  
    /// The sample is using a Bing Map's Streetside API to demonstrate these functions.  In order to use this sample you have to apply with Bing Maps for a Bing Maps API developer key.  You can find the instructions on how to do this below.  
    /// </summary>
    /// <remarks>
    /// Using Bing Maps API: To use the Bing Maps APIs, you must have a (Bing Maps Key)[https://msdn.microsoft.com/en-us/library/dd877180.aspx].
    /// Note: When you use the Bing Maps APIs with a Bing Maps Key, usage transactions are logged. See Understanding (Bing Maps Transactions)[https://msdn.microsoft.com/en-us/library/ff859477.aspx] for more information.
    /// Creating a Bing Maps Key
    /// 1. Go to the Bing Maps Dev Center at https://www.bingmapsportal.com/. 
    /// ** If you have a Bing Maps account, sign in with the Microsoft account that you used to create the account or create a new one.For new accounts, follow the instructions in (Creating a Bing Maps Account)[https://msdn.microsoft.com/en-us/library/gg650598.aspx].
    /// 2. Select Keys under My Account.
    /// 3. Provide the following information to create a key:
    /// ** Application name: Required.The name of the application.
    /// ** Application URL: The URL of the application.
    /// ** Key type: Required. Select the key type that you want to create.You can find descriptions of key and application types (here)[https://www.microsoft.com/maps/create-a-bing-maps-key.aspx].
    /// ** Application type: Required. Select the application type that best represents the application that will use this key.You can find descriptions of key and application types (here)[https://www.microsoft.com/maps/create-a-bing-maps-key.aspx].  
    /// 4.	Type the characters of the security code, and then click Create. The new key displays in the list of available keys.Use this key to authenticate your Bing Maps application as described in the documentation for the Bing Maps API you are using.
    ///  
    /// Note: the Bing map preview SDK overview used in this sample can be found here: https://www.bing.com/mapspreview/sdk/mapcontrol/isdk#overview
    /// 
    /// Using the sample:
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Create a new project using the Map.aptx template.  
    /// 1. With a map view active go to the "Bing Streetside" tab and click the "Show Bing Streetside Pane" button.
    /// 1. This will open the "Bing Streetside Viewer" dock pane.
    /// ![UI](Screenshots/screenshot1.png)
    /// 1. Paste the "Bing Maps Key" that you obtained from Microsoft (see instructions above) and click the "Define Bing Map Key" button.  
    /// 1. For convenience you can also define your Bing Key under the following code comment: "TODO: define your bing map key here:"
    /// 1. The "Bing Streetside Viewer" dock pane now displays Bing Map's street view pane (starting at Esri).
    /// ![UI](Screenshots/screenshot2.png)
    /// 1. Click on the "N New York St" arrow pointing north on the "Bing Streetside Viewer" and see the location on the map pane being updated.  
    /// ![UI](Screenshots/screenshot3.png)
    /// 1. The view heading on the "Bing Map Streetside" view can be changed by clicking on the "Change Heading" control above the "Bing Map Streetside" control and dragging the heading arrow into a new direction.  
    /// ![UI](Screenshots/screenshot4.png)
    /// 1. Click the "Bing Streetside View Tool" button and click on a new street location on the map pane.
    /// 1. Notice that "Bing Map Streetside" will update it's view to the new clicked on location.
    /// ![UI](Screenshots/screenshot5.png)
    /// </remarks>
    internal class BingStreetsideModule : Module
    {
        private static BingStreetsideModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static BingStreetsideModule Current
        {
            get
            {
                return _this ?? (_this = (BingStreetsideModule)FrameworkApplication.FindModule("BingStreetside_Module"));
            }
        }

        public BingStreetsideModule()
        {
            SetupOverlaySymbols();
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

        #region Overlay symbols/add/remove graphics

        private static readonly List<IDisposable> BingMapCoords = new List<IDisposable>();
        private static CIMPointSymbol _pointCoordSymbol = null;

        private readonly static object Lock = new object();
        
        public static void ShowCurrentBingMapCoord(MapPoint mapPoint)
        {
            var activeMapView = MapView.Active;
            if (activeMapView == null) return;
            lock (Lock)
            {
                foreach (var graphic in BingMapCoords)
                    graphic.Dispose();
                BingMapCoords.Clear();
                Debug.WriteLine($"SetCurrentBingMapCoord: {mapPoint.X} {mapPoint.Y}");
                BingMapCoords.Add(
                    activeMapView.AddOverlay(
                        mapPoint,
                        _pointCoordSymbol.MakeSymbolReference()));
            }
        }

        private static void SetupOverlaySymbols()
        {
            QueuedTask.Run(() =>
            {
                var markerCoordPoint = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.GreenRGB, 12,
                    SimpleMarkerStyle.Circle);
                _pointCoordSymbol = SymbolFactory.Instance.ConstructPointSymbol(markerCoordPoint);
            });
        }

        #endregion Overlay symbols/add/remove graphics

        private static bool _bFirst = true;

        /// <summary>Get the Lat, Long from the Bing StreetSide View to set the location on the Pro Map</summary>
        public static Task SetMapLocationFromBing(double? longitude, double? latitude, int heading)
        {
            #region Process Heading

            var activeMapView = MapView.Active;
            if (activeMapView == null) return null;

            #endregion

            return QueuedTask.Run(() => {
                try
                {
                    var cam = activeMapView.Camera;
                    var bHeadingChange = Convert.ToInt32(cam.Heading) != heading;
                    cam.Heading = Convert.ToDouble(heading);
                    if (longitude.HasValue && latitude.HasValue)
                    {
                        var pt = MapPointBuilder.CreateMapPoint(longitude.Value, latitude.Value, SpatialReferences.WGS84);
                        var center = GeometryEngine.Instance.Project(pt, activeMapView.Map.SpatialReference) as MapPoint;
                        if (center == null) return;
                        ShowCurrentBingMapCoord(center);

                        #region Update Map

                        // check if the center is outside the map view extent
                        var env = activeMapView.Extent.Expand(0.75, 0.75, true);
                        var bWithin = GeometryEngine.Instance.Within(center, env);
                        if (!bWithin)
                        {
                            cam.X = center.X;
                            cam.Y = center.Y;
                        }
                        if (_bFirst)
                        {
                            cam.Scale = 2000;
                            activeMapView.ZoomTo(cam, TimeSpan.FromMilliseconds(100));
                            _bFirst = false;
                        }
                        else activeMapView.PanTo(cam, TimeSpan.FromMilliseconds(1000));
                        bHeadingChange = false;

                        #endregion
                    }
                    if (bHeadingChange)
                    {
                        activeMapView.PanTo(cam, TimeSpan.FromMilliseconds(300));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($@"Error in SetMapLocationFromBing: {ex.Message}");
                }
            });
        }

        public static void SetMapLocationOnBingMaps(double lng, double lat)
        {
           WebBrowserUtility.InvokeScript("setViewCenterFromWPF", new Object[] {lng, lat});
        }
    }
}
