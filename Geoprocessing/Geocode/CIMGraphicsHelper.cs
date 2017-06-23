//Copyright 2017 Esri
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
using ArcGIS.Core.CIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;

namespace Geocode
{
    public class CIMPointGraphicHelper : IDisposable
    {
        private CIMPointGraphic _graphic = null;
        private CIMHelpers _cimHelper = null;
        private bool _isDisposed = false;
        /// <summary>
        /// Default constructor
        /// </summary>
        public CIMPointGraphicHelper(PointN point)
        {
            _cimHelper = new CIMHelpers();
            _graphic = _cimHelper.MakeCIMPointGraphic(point);
            GraphicID = -1;
        }

        ~CIMPointGraphicHelper()
        {
            Dispose(false);
        }
        /// <summary>
        /// Gets and sets id for this graphic relevant to the specific mapview
        /// overlay it has been added to
        /// </summary>
        public int GraphicID { get; set; }

        /// <summary>
        /// Gets the XML representation of the CIMPointGraphic
        /// </summary>
        public string XML
        {
            get
            {
                return _graphic != null ? XmlUtil.SerializeCartoXObject(_graphic) : "";
            }
        }

        /// <summary>
        /// Gets the wrapped CIMPointGraphic
        /// </summary>
        public CIMPointGraphic CIMPointGraphic
        {
            get
            {
                return _graphic;
            }
        }
        /// <summary>
        /// Update the CIMPointGraphic location
        /// </summary>
        /// <param name="point"></param>
        public void UpdateLocation(PointN point)
        {
            ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(point.SpatialReference.WKID);
            MapPoint mapPoint = MapPointBuilder.CreateMapPoint(point.X, point.Y, sptlRef);
            _graphic.Location = mapPoint;           
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                if (_graphic != null)
                {
                    ((CIMPointSymbol)_graphic.Symbol.Symbol).SymbolLayers = null;
                    _graphic.Symbol.Symbol = null;
                    _graphic.Symbol = null;
                    _graphic.Location = null;
                    _graphic = null;
                }
            }
            System.Diagnostics.Debug.WriteLine("CIMPointGraphic disposed {0}", disposing.ToString());
            _isDisposed = true;
        }
    }

    public class CIMHelpers
    {

        public static CIMColor Red = new CIMRGBColor
        {
            R = 255,
            G = 0,
            B = 0,
            Alpha = 200// 0 for transparent and 255 for opaque
        };
        public static CIMColor Black = new CIMRGBColor
        {
            R = 0,
            G = 0,
            B = 0,
            Alpha = 200// 0 for transparent and 255 for opaque
        };
        public static CIMColor Navy = new CIMRGBColor
        {
            R = 0,
            G = 0,
            B = 128,
            Alpha = 200// 0 for transparent and 255 for opaque
        };
        /// <summary>
        /// Create a CIMPointGaphic which can be added to the MapView overlay.
        /// </summary>
        /// <param name="point">The location for the point (as a CIM point)</param>
        /// <returns></returns>
        public CIMPointGraphic MakeCIMPointGraphic(PointN point)
        {
            CIMMarker marker = SymbolFactory.Instance.ConstructMarker(Red, 10, SimpleMarkerStyle.Star);

            CIMSymbolLayer[] layers = new CIMSymbolLayer[1];
            layers[0] = marker;

      CIMPointSymbol pointSymbol = new CIMPointSymbol()
      {
        SymbolLayers = layers,
        ScaleX = 1
      };
      CIMSymbolReference symbolRef = new CIMSymbolReference()
      {
        Symbol = pointSymbol
      };
      CIMPointGraphic pointGraphic = new CIMPointGraphic();
            ArcGIS.Core.Geometry.SpatialReference spatialRef = SpatialReferenceBuilder.CreateSpatialReference(point.SpatialReference.WKID);
            MapPoint mapPoint = MapPointBuilder.CreateMapPoint(point.X, point.Y, spatialRef);
            pointGraphic.Location = mapPoint;
            pointGraphic.Symbol = symbolRef;

            return pointGraphic;
        }

        /// <summary>
        /// Make a CIM Point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="wkid"></param>
        /// <returns></returns>
        public PointN MakePointN(double x, double y, int wkid)
        {
            PointN pointN = new PointN()
            {
                X = x,
                Y = y,
                SpatialReference = this.CreateSpatialReference(wkid)
            };
            return pointN;
        }

        /// <summary>
        /// Create a CIM Spatial Reference. Has to be created from scratch
        /// </summary>
        /// <param name="WKID">The wkid for the spatial reference to be created</param>
        /// <returns>A CIM spatial reference</returns>
        public ArcGIS.Core.Internal.CIM.SpatialReference CreateSpatialReference(int WKID)
        {
            ArcGIS.Core.Internal.CIM.SpatialReference sr = null;
            if (WKID == 104115 || WKID == 4326)
            {
        GeographicCoordinateSystem gs = new GeographicCoordinateSystem()
        {
          WKID = 104115,
          WKIDSpecified = true,
          LatestWKID = 104115,
          LatestVCSWKIDSpecified = true,
          LeftLongitude = -180,
          LeftLongitudeSpecified = true,
          HighPrecision = true,
          HighPrecisionSpecified = true,
          MTolerance = 0.001,
          MToleranceSpecified = true,
          ZTolerance = 0.001,
          ZToleranceSpecified = true,
          XYTolerance = 8.9831528411952133E-09,
          XYToleranceSpecified = true,
          MScale = 10000,
          MScaleSpecified = true,
          MOrigin = -100000,
          MOriginSpecified = true,
          ZScale = 10000,
          ZScaleSpecified = true,
          ZOrigin = -100000,
          ZOriginSpecified = true,
          XYScale = 999999999.99999988,
          XYScaleSpecified = true,
          XOrigin = -400,
          XOriginSpecified = true,
          YOrigin = -400,
          YOriginSpecified = true,
          WKT = "GEOGCS[\"GCS_ITRF_1988\",DATUM[\"D_ITRF_1988\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433],AUTHORITY[\"ESRI\",104115]]"
        };
        sr = gs as ArcGIS.Core.Internal.CIM.SpatialReference;

            }
            else if (WKID == 102100)
            {
        //Note: In testing, I could not get Web Mercator to
        //work on the mapview.
        ProjectedCoordinateSystem pcs = new ProjectedCoordinateSystem()
        {
          HighPrecision = true,
          HighPrecisionSpecified = true,
          LatestVCSWKID = 0,
          LatestVCSWKIDSpecified = false,
          LatestWKID = 3857,
          LatestWKIDSpecified = true,
          LeftLongitude = 0.0,
          LeftLongitudeSpecified = false,
          MOrigin = -100000.0,
          MOriginSpecified = true,
          MScale = 10000.0,
          MScaleSpecified = true,
          MTolerance = 0.001,
          MToleranceSpecified = true,
          VCSWKID = 0,
          VCSWKIDSpecified = false,
          WKID = 102100,
          WKIDSpecified = true,
          WKT = "PROJCS[\"WGS_1984_Web_Mercator_Auxiliary_Sphere\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Mercator_Auxiliary_Sphere\"],PARAMETER[\"false ;_Easting\",0.0],PARAMETER[\"false ;_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0],PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Auxiliary_Sphere_Type\",0.0],UNIT[\"Meter\",1.0],AUTHORITY[\"EPSG\",3857]]",
          XOrigin = -20037700.0,
          XOriginSpecified = true,
          XYScale = 10000.0,
          XYScaleSpecified = true,
          XYTolerance = 0.001,
          XYToleranceSpecified = true,
          YOrigin = -30241100.0,
          YOriginSpecified = true,
          ZOrigin = -100000.0,
          ZOriginSpecified = true,
          ZScale = 10000.0,
          ZScaleSpecified = true,
          ZTolerance = 0.001,
          ZToleranceSpecified = true
        };
        sr = pcs as ArcGIS.Core.Internal.CIM.SpatialReference;
            }
            else
            {
                throw new System.NotImplementedException(string.Format("Sorry. Support for wkid {0} is not implemented in this example", WKID));
            }
            return sr;
        }
    }
}
