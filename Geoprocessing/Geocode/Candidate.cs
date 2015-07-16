//Copyright 2015 Esri

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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Internal.CIM;

namespace Geocode
{
    ///This is what the json response looks like:
    /*
    "locations": [
  {
   "name": "380 New York St, Redlands, California, 92373",
   "extent": {
    "xmin": -13046277.077971535,
    "ymin": 4036255.4132860559,
    "xmax": -13046054.438989947,
    "ymax": 4036524.1432637931
   },
   "feature": {
    "geometry": {
     "x": -13046161.844799552,
     "y": 4036389.8752686069
    },
    "attributes": {
     "match_addr": "380 New York St, Redlands, California, 92373",
     "addr_type": "PointAddress",
     "region": "California",
     "postal": "92373",
     "country": "USA",
     "score": 100
    }
   }
  },
  { .....
   },
   { ......
   },  
   etc, etc.
     */
    /// <summary>
    /// Datacontract for the JSON response returned from the ArcGIS Online geocoder
    /// </summary>
    [DataContract]
    public class CandidateResponse : ObservableObject
    {

        private ObservableCollection<CandidateLocation> _results = null;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _results = new ObservableCollection<CandidateLocation>();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            InitializeResults();
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public CandidateResponse()
        {
            //Ensure results are never null
            if (null == _results)
            {
                _results = new ObservableCollection<CandidateLocation>();
            }
        }

        [DataMember(Name = "spatialReference")]
        public CandidateSpatialReference SpatialReference { get; set; }
        [DataMember(Name = "locations")]
        public List<CandidateLocation> Locations { get; set; }

        private void InitializeResults()
        {
            if (Locations == null)
                return;

            IOrderedEnumerable<CandidateLocation> oca = Locations.OrderByDescending(ca => ca.Feature.Attributes.Score);
            foreach (CandidateLocation result in oca)
            {
                result.WKID = this.SpatialReference.WKID;//transfer
                result.Extent.WKID = this.SpatialReference.WKID;//transfer
                _results.Add(result);
            }
            RaisePropertyChanged("OrderedResults");
        }
        /// <summary>
        /// The list of candidate results returned from the
        /// geocode sorted on score
        /// </summary>
        public ObservableCollection<CandidateLocation> OrderedResults { get { return _results; } }

    }

    [DataContract]
    public class CandidateSpatialReference : ObservableObject
    {
        [DataMember(Name = "wkid")]
        public int WKID { get; set; }
    }

    /// <summary>
    /// Holds Gaz or GeocodeButton results
    /// </summary>
    [DataContract]
    public class CandidateLocation : ObservableObject
    {

        private string _candidateDetails = "";
        private PointN _point = null;
        private MapPoint _mapPoint = null;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CandidateLocation()
            : base()
        {
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            //Check if we have a US place vs an International Place
            bool isUSA = false;
            if (!string.IsNullOrEmpty(Feature.Attributes.Country))
            {
                if (Feature.Attributes.Country.ToLower().Trim().CompareTo("usa") == 0)
                {
                    isUSA = true;
                }
            }
            string details = Name;
            if (isUSA && !string.IsNullOrEmpty(Feature.Attributes.Region))
            {
                details += (", " + Feature.Attributes.Region);
            }
            else
            {
                details += (", " + Feature.Attributes.Country);
            }
            _candidateDetails = String.Format("{0}, {1:##0.0}, ({2:0#.0##},{3:0#.0##})", details, Feature.Attributes.Score,
                                                                         Feature.Location.X, Feature.Location.Y);

            RaisePropertyChanged("CandidateDetails");
        }


        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "extent")]
        public CandidateExtent Extent { get; set; }
        [DataMember(Name = "feature")]
        public CandidateFeature Feature { get; set; }

        /// <summary>
        /// The displayed information - formatted as address, score, and X,Y
        /// </summary>
        public string CandidateDetails { get { return _candidateDetails; } }
        /// <summary>
        /// The spatial reference wkid for the candidate
        /// </summary>
        public int WKID { get; set; }
        /// <summary>
        /// Convert the location to a CIM Point
        /// </summary>
        /// <returns></returns>
        public PointN ToPointN()
        {
            if (_point == null)
            {

                CIMHelpers cimHelper = new CIMHelpers();

                _point = cimHelper.MakePointN(Feature.Location.X,
                    Feature.Location.Y,
                    WKID);
            }
            return _point;
        }

        public async Task<MapPoint> ToMapPoint()
        {
            if (_mapPoint == null)
            {
               await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Geometry.SpatialReference sr = SpatialReferenceBuilder.CreateSpatialReference(WKID);
                _mapPoint = MapPointBuilder.CreateMapPoint(Feature.Location.X, Feature.Location.Y, sr);                
            });
            }

            return _mapPoint;
        }
    }

    [DataContract]
    public class CandidateExtent : ObservableObject
    {
        [DataMember(Name = "xmin")]
        public double XMin { get; set; }
        [DataMember(Name = "ymin")]
        public double YMin { get; set; }
        [DataMember(Name = "xmax")]
        public double XMax { get; set; }
        [DataMember(Name = "ymax")]
        public double YMax { get; set; }
        /// <summary>
        /// The spatial reference wkid
        /// </summary>
        public int WKID { get; set; }
    }

    [DataContract]
    public class CandidateFeature : ObservableObject
    {

        public CandidateFeature() : base() { }

        [DataMember(Name = "geometry")]
        public CandidateGeometry Location { get; set; }
        [DataMember(Name = "attributes")]
        public CandidateAttributes Attributes { get; set; }
    }


    [DataContract]
    public class CandidateGeometry : ObservableObject
    {

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            RaisePropertyChanged("X");
            RaisePropertyChanged("Y");
        }

        public CandidateGeometry() : base() { }

        [DataMember(Name = "x")]
        public double X { get; set; }
        [DataMember(Name = "y")]
        public double Y { get; set; }
    }

    /// <summary>
    /// Results from a Gazetteer search
    /// </summary>
    [DataContract]
    public class CandidateAttributes : ObservableObject
    {
        /// <summary>
        /// These are the fields that must be included in the geocode query
        /// </summary>
        public static string outFields = "match_addr,addr_type,region,postal,country,score";

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Init();
            RaisePropertyChanged("MatchedAddress");
            RaisePropertyChanged("AddressType");
            RaisePropertyChanged("Region");
            RaisePropertyChanged("PostalCode");
            RaisePropertyChanged("Country");
            RaisePropertyChanged("Score");
        }
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public CandidateAttributes()
        {
            Init();
        }

        private void Init()
        {
            //make sure there are no nulls
            if (string.IsNullOrEmpty(MatchedAddress)) MatchedAddress = "";
            if (string.IsNullOrEmpty(AddressType)) AddressType = "";
            if (string.IsNullOrEmpty(Region)) Region = "";
            if (string.IsNullOrEmpty(PostalCode)) PostalCode = "";
            if (string.IsNullOrEmpty(Country)) Country = "";
        }
        [DataMember(Name = "match_addr")]
        public string MatchedAddress { get; set; }
        [DataMember(Name = "addr_type")]
        public string AddressType { get; set; }
        [DataMember(Name = "region")]
        public string Region { get; set; }
        [DataMember(Name = "postal")]
        public string PostalCode { get; set; }
        [DataMember(Name = "country")]
        public string Country { get; set; }
        [DataMember(Name = "score")]
        public double Score { get; set; }
    }
}
