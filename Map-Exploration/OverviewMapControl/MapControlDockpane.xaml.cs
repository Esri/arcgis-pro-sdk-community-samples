//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using ArcGIS.Desktop.Mapping.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Polyline = ArcGIS.Core.Geometry.Polyline;


namespace OverviewMapControl
{
    
    /// <summary>
    /// Interaction logic for MapControlDockpaneView.xaml
    /// </summary>
    public partial class MapControlDockpaneView : UserControl
    {
        private readonly MapControl _mapControl = null;
        private CIMLineSymbol _lineSymbol = null;
        private IDisposable _graphic = null;
        private Envelope _overviewEnvelope = null;
        private Polyline _polyLine = null;
        public MapControlDockpaneView()
        {
            InitializeComponent();
            _mapControl = this.MapControl;  //Link _mapControl variable to this map control
            InitializeMapControl();  //Initializes the map control with content                    
        }      

        /// <summary>
        /// Initializes the MapControl with Content and listen to events.
        /// </summary>
        private async void InitializeMapControl()
        {
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged); //Update the content of the MapControl when the active map changes.
            MapViewCameraChangedEvent.Subscribe(OnMapViewCameraChanged);
            
            if (MapView.Active == null)
                return;
            if (MapView.Active.Extent == null)
                return;
            //2D
            if (MapView.Active.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.Map)
            {
                Envelope mapControlExtent = await QueuedTask.Run(() => 
                    (MapView.Active.Extent.Clone() as Envelope)?.Expand(4, 4, true));
                //Define 2D Extent that should be displayed inside the mapcontrol.
                _mapControl.ViewContent = MapControlContentFactory.Create(MapView.Active.Map, mapControlExtent, MapView.Active.Map.DefaultViewingMode);
                //Event handler: When mapcontrol's extent changes, the active map view reflects the extent.
                _mapControl.ExtentChanged += OnMapControlExtentChanged;                
                return;
            }
            //3D
            //Define 3D View that should be displayed inside the mapcontrol.
            if (MapView.Active.Camera == null)
                return;
            Camera newCamera = new Camera(MapView.Active.Camera.X, MapView.Active.Camera.Y,
                MapView.Active.Camera.Z + 100, MapView.Active.Camera.Pitch, MapView.Active.Camera.Heading);
            _mapControl.ViewContent = MapControlContentFactory.Create(MapView.Active.Map, newCamera,
                MapView.Active.Map.DefaultViewingMode);             
           
            _mapControl.CameraChanged += OnMapControlCameraChanged;
        }
        /// <summary>
        /// When the active map view changes, update the graphic overlay on the map control
        /// </summary>
        /// <param name="args"></param>
        private void OnMapViewCameraChanged(MapViewCameraChangedEventArgs args)
        {
            if (MapView.Active == null)
                return;
            if (MapView.Active.Extent == null)
                return;
            if (MapView.Active.ViewingMode != ArcGIS.Core.CIM.MapViewingMode.Map)
                return;

            //get the Map view's extent
            var viewExtent = MapView.Active.Extent.Clone() as Envelope;         

            QueuedTask.Run(() =>
            {
                //Line symbol to be used to draw the overview rectangle.
                if (_lineSymbol == null) _lineSymbol = 
                    SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 2.0, SimpleLineStyle.Solid);
               
                _graphic?.Dispose();//Clear out the old overlay
                
                _overviewEnvelope = viewExtent; //overview envelope based on active map view

                if (_overviewEnvelope == null) return;
              //Creating the overview rectangle
              IList<MapPoint> segments = new List<MapPoint>
              {
                MapPointBuilder.CreateMapPoint(_overviewEnvelope.XMin, _overviewEnvelope.YMin, _overviewEnvelope.SpatialReference),
                MapPointBuilder.CreateMapPoint(_overviewEnvelope.XMax, _overviewEnvelope.YMin, _overviewEnvelope.SpatialReference),
                MapPointBuilder.CreateMapPoint(_overviewEnvelope.XMax, _overviewEnvelope.YMax, _overviewEnvelope.SpatialReference),
                MapPointBuilder.CreateMapPoint(_overviewEnvelope.XMin, _overviewEnvelope.YMax, _overviewEnvelope.SpatialReference),
                MapPointBuilder.CreateMapPoint(_overviewEnvelope.XMin, _overviewEnvelope.YMin, _overviewEnvelope.SpatialReference)
              };
              _polyLine = PolylineBuilder.CreatePolyline(segments, _overviewEnvelope.SpatialReference);
              
                _graphic = _mapControl.AddOverlay(_polyLine, _lineSymbol.MakeSymbolReference());
            });
            
        }

        /// <summary>
        /// Event handler modifies Active Map's Extent when MapControl's Extent changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnMapControlExtentChanged(object sender, EventArgs e)
        {
            _mapControl.CameraChanged -= OnMapControlCameraChanged;
            var mapControlExtent = _mapControl.Extent.Clone() as Envelope;
            await QueuedTask.Run(() =>
            {
                if ((MapView.Active.IsReady) && (mapControlExtent != null))
                    MapView.Active.ZoomTo(mapControlExtent.Expand(0.25, 0.25, true));
            });
        }
        /// <summary>
        /// Event handler modifies Active scene's camera when MapControl's camera changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnMapControlCameraChanged(object sender, EventArgs e)
        {
            //_mapControl.ExtentChanged -= OnMapControlExtentChanged;
            var newCamera = new Camera(_mapControl.Camera.X, _mapControl.Camera.Y, _mapControl.Camera.Z-100, _mapControl.Camera.Pitch, _mapControl.Camera.Heading);
            await QueuedTask.Run(() =>
            {
                if (MapView.Active.IsReady)
                    MapView.Active.ZoomTo(newCamera);
            });
        }
        /// <summary>
        /// Event Handler to update the MapControl when the ActiveMap changes.
        /// </summary>
        /// <param name="args"></param>
        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            if (args.IncomingView == null)
                return;
            if (MapView.Active == null)
                return;
            var vm = FrameworkApplication.DockPaneManager.Find("OverviewMapControl_MapControlDockpane") as MapControlDockpaneViewModel;
            if (vm != null) vm.ActiveMap = args.IncomingView.Map.Name;
            InitializeMapControl();
        }      
    }
}
