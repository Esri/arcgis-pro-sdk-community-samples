//   Copyright 2014 Esri
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

namespace ArcGIS.Desktop.Tests.APIHelpers.SharingDataContracts
{
    #region agoUser and related
    [DataContract]
    public class agoUser
    {
        [DataMember(Name = "username")]
        public string username { get; set; }

        [DataMember(Name = "orgId")]
        public string orgID { get; set; }

        [DataMember(Name = "role")]
        public string role { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "groups")]
        public UserGroups[] groups { get; set; }
    }

    [DataContract]
    public class UserGroups
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

        [DataMember(Name = "created")]
        public Int64 created { get; set; }

        [DataMember(Name = "snippet")]
        public string snippet { get; set; }

        [DataMember(Name = "featuredItemsId")]
        public string FeaturedItemsId { get; set; }

        [DataMember(Name = "isPublic")]
        public bool IsPublic { get; set; }

        [DataMember(Name = "isInvitationOnly")]
        public bool IsInvitationOnly { get; set; }

        [DataMember(Name = "isViewOnly")]
        public bool IsViewOnly { get; set; }
    }
    #endregion

    #region Delete online item
    [DataContract]
    public partial class DeleteItemResponse
    {

        [DataMember(Name = "success")]
        public bool success;

        [DataMember(Name = "itemId")]
        public string itemId;

        [DataMember(Name = "error")]
        public Error error;
    }

    [DataContract]
    public partial class Error
    {
        [DataMember(Name = "code")]
        public int code;

        [DataMember(Name = "messageCode")]
        public string messageCode;

        [DataMember(Name = "message")]
        public string message;

        [DataMember(Name = "details")]
        public object[] details;
    }
    #endregion

    #region FeatureService DataContracts
    /// <summary>
    /// DataContract for JSON response of a feature service. Unnecessary elements are commented out.
    /// Uncomment them, add classes if necessary to make them usable.
    /// </summary>
    [DataContract]
    public class FeatureService
    {
        //public double currentVersion { get; set; }
        //public string serviceDescription { get; set; }
        //public bool hasVersionedData { get; set; }
        //public bool supportsDisconnectedEditing { get; set; }
        //public bool hasStaticData { get; set; }
        //public int maxRecordCount { get; set; }
        //public string supportedQueryFormats { get; set; }
        //public bool syncEnabled { get; set; }

        [DataMember(Name = "capabilities")]
        public string capabilities { get; set; }

        //public string description { get; set; }
        //public string copyrightText { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference spatialReference { get; set; }

        [DataMember(Name = "initialExtent")]
        public InitialExtent initialExtent { get; set; }

        [DataMember(Name = "fullExtent")]
        public FullExtent fullExtent { get; set; }
        //public bool allowGeometryUpdates { get; set; }
        //public string units { get; set; }
        //public DocumentInfo documentInfo { get; set; }

        [DataMember(Name = "layers")]
        public List<Layer> layers { get; set; }

        [DataMember(Name = "tables")]
        public List<object> tables { get; set; }

        //public bool enableZDefaults { get; set; }
    }

    /// <summary>
    /// Datacontract class to be used with FeatureServeice class, WebMapServiceInfo class
    /// </summary>
    [DataContract]
    public class Layer
    {
        [DataMember(Name = "defaultVisibility")]
        public bool defaultVisibility { get; set; }

        [DataMember(Name = "id")]
        public string id { get; set; }

        [DataMember(Name = "layerDefinition")]
        public LayerDefinition layerDefinition { get; set; }

        [DataMember(Name = "legendUrl")]
        public string legendUrl { get; set; }

        [DataMember(Name = "maxScale")]
        public float maxScale { get; set; }

        [DataMember(Name = "minScale")]
        public float minScale { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "parentLayerId")]
        public string parentLayerId { get; set; }

        [DataMember(Name = "popupInfo")]
        public PopupInfo popupInfo { get; set; }

        [DataMember(Name = "showLegend")]
        public bool showLegend { get; set; }
    }

    #region returned by analyze request
    [DataContract]
    public class AnalyzedService
    {
        [DataMember(Name = "filesize")]
        public double filesize { get; set; }

        [DataMember(Name = "fileUrl")]
        public string fileUrl { get; set; }

        [DataMember(Name = "publishParameters")]
        public PublishParameters publishParameters { get; set; }

        [DataMember(Name = "records")]
        public List<Attributes> records { get; set; }
    }

    [DataContract]
    public class Attributes
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "latitude")]
        public float latitude { get; set; }

        [DataMember(Name = "longitude")]
        public float longitude { get; set; }

        [DataMember(Name = "Address")]
        public string Address { get; set; }
    }

    [DataContract]
    public class PublishParameters
    {
        [DataMember(Name = "columnDelimiter")]
        public string columnDelimiter { get; set; }

        [DataMember(Name = "editorTrackingInfo")]
        public EditorTrackingInfo editorTrackingInfo { get; set; }

        [DataMember(Name = "latitudeFieldName")]
        public string latitudeFieldName { get; set; }

        [DataMember(Name = "longitudeFieldName")]
        public string longitudeFieldName { get; set; }

        [DataMember(Name = "layerInfo")]
        public AnalyzedServiceLayerInfo layerInfo { get; set; }

        [DataMember(Name = "locationType")]
        public string locationType { get; set; }

        [DataMember(Name = "maxRecordCount")]
        public double maxRecordCount { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "qualifier")]
        public string qualifier { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "sourceSR")]
        public SpatialReference sourceSR { get; set; }

        [DataMember(Name = "targetSR")]
        public SpatialReference targetSR { get; set; }
    }

    [DataContract]
    public class AnalyzedServiceLayerInfo
    {
        [DataMember(Name = "advancedQueryCapabilities")]
        public AdvancedQueryCapabilities advancedQueryCapabilities { get; set; }

        [DataMember(Name = "allowGeometryUpdates")]
        public bool allowGeometryUpdates { get; set; }

        [DataMember(Name = "id")]
        public int id { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "description")]
        public string description { get; set; }

        [DataMember(Name = "copyrightText")]
        public string copyrightText { get; set; }

        [DataMember(Name = "capabilities")]
        public string capabilities { get; set; }

        [DataMember(Name = "currentVersion")]
        public string currentVersion { get; set; }

        [DataMember(Name = "defaultVisibility")]
        public bool defaultVisibility { get; set; }

        [DataMember(Name = "drawingInfo")]
        public LayerDefinition.DrawingInfo drawingInfo { get; set; }

        [DataMember(Name = "fields")]
        public List<field> fields { get; set; }

        [DataMember(Name = "geometryType")]
        public string geometryType { get; set; }

        [DataMember(Name = "hasAttachments")]
        public bool hasAttachments { get; set; }

        [DataMember(Name = "hasStaticData")]
        public bool hasStaticData { get; set; }

        [DataMember(Name = "type")]
        public string ASLI_type { get; set; }
    }

    [DataContract]
    public class AdvancedQueryCapabilities
    {
        [DataMember(Name = "supportsDistinct")]
        public bool supportsDistinct { get; set; }

        [DataMember(Name = "supportsOrderBy")]
        public bool supportsOrderBy { get; set; }

        [DataMember(Name = "supportsPagination")]
        public bool supportsPagination { get; set; }

        [DataMember(Name = "supportsQueryWithDistance")]
        public bool supportsQueryWithDistance { get; set; }

        [DataMember(Name = "supportsReturningQueryExtent")]
        public bool supportsReturningQueryExtent { get; set; }

        [DataMember(Name = "supportsStatistics")]
        public bool supportsStatistics { get; set; }
    }

    [DataContract]
    public class EditorTrackingInfo
    {
        [DataMember(Name = "allowOthersToDelete")]
        public bool allowOthersToDelete { get; set; }

        [DataMember(Name = "allowOthersToUpdate")]
        public bool allowOthersToUpdate { get; set; }

        [DataMember(Name = "enableEditorTracking")]
        public bool enableEditorTracking { get; set; }

        [DataMember(Name = "enableOwnershipAccessControl")]
        public bool enableOwnershipAccessControl { get; set; }
    }
    #endregion

    #region returned by isServiceNameAvailable request
    [DataContract]
    public class AvailableResult
    {
        [DataMember(Name = "available")]
        public bool available { get; set; }
    }
    #endregion

    #region returned by publish request
    [DataContract]
    public class PublishedServices
    {
        [DataMember(Name = "services")]
        public List<PublishedService> services { get; set; }
    }

    [DataContract]
    public class PublishedService
    {
        [DataMember(Name = "encodedServiceURL")]
        public string encodedServiceURL { get; set; }

        [DataMember(Name = "jobId")]
        public string jobId { get; set; }

        [DataMember(Name = "serviceItemId")]
        public string serviceItemId { get; set; }

        [DataMember(Name = "serviceurl")]
        public string serviceurl { get; set; }

        [DataMember(Name = "size")]
        public double size { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }
    }
    #endregion

    #region returned by applyEdits request
    [DataContract]
    public class EditsResponse
    {
        [DataMember(Name = "id")]
        public int id { get; set; }

        [DataMember(Name = "addResults")]
        public List<SingleEditsResult> addResults { get; set; }

        [DataMember(Name = "addResults")]
        public List<SingleEditsResult> updateResults { get; set; }

        [DataMember(Name = "addResults")]
        public List<SingleEditsResult> deleteResults { get; set; }
    }

    [DataContract]
    public class SingleEditsResult
    {
        [DataMember(Name = "objectId")]
        public int objectId { get; set; }

        [DataMember(Name = "globalId")]
        public int globalId { get; set; }

        [DataMember(Name = "success")]
        public bool success { get; set; }
    }
    #endregion

    #region returned by query request
    [DataContract]
    public class QueryLayers
    {
        [DataMember(Name = "layers")]
        public List<QueryLayer> layers { get; set; }
    }

    [DataContract]
    public class QueryLayer
    {
        [DataMember(Name = "id")]
        public int id { get; set; }

        [DataMember(Name = "fields")]
        public List<field> fields { get; set; }

        [DataMember(Name = "features")]
        public List<feature> features { get; set; }
    }

    [DataContract]
    public class field
    {
        [DataMember(Name = "name")]
        public string name { get; set; }
    }

    [DataContract]
    public class feature
    {
        [DataMember(Name = "attributes")]
        public FeatureAttributes attributes { get; set; }

        [DataMember(Name = "geometry")]
        public FeatureGeometry geometry { get; set; }
    }

    [DataContract]
    public class FeatureAttributes
    {
        [DataMember(Name = "OBJECTID")]
        public string OBJECTID { get; set; }
    }

    [DataContract]
    public class FeatureGeometry
    {
        [DataMember(Name = "x")]
        public double x { get; set; }

        [DataMember(Name = "y")]
        public double y { get; set; }
    }
    #endregion

    #region returned by userlicenses request
    [DataContract]
    public class userLicenses
    {
        [DataMember(Name = "signature")]
        public string signature { get; set; }

        [DataMember(Name = "userEntitlementsString")]
        public string userEntitlementsString { get; set; }
    }

    [DataContract]
    public class userEntitlementsString
    {
        [DataMember(Name = "username")]
        public string username { get; set; }

        [DataMember(Name = "lastLogin")]
        public long lastLogin { get; set; }

        [DataMember(Name = "disconnected")]
        public bool disconnected { get; set; }

        [DataMember(Name = "entitlements")]
        public List<string> entitlements { get; set; }

        [DataMember(Name = "licenses")]
        public List<string> licenses { get; set; }

        [DataMember(Name = "nonce")]
        public string nonce { get; set; }

        [DataMember(Name = "timestamp")]
        public long timestamp { get; set; }
    }
    #endregion

    #region returned by portal self request
    [DataContract]
    public class PortalSelf
    {
        [DataMember(Name = "appInfo")]
        public appInfo appInfo { get; set; }

        [DataMember(Name = "created")]
        public long created { get; set; }

        [DataMember(Name = "user")]
        public user user { get; set; }

        [DataMember(Name = "id")]
        public string id { get; set; }
    }

    [DataContract]
    public class appInfo
    {
        [DataMember(Name = "itemId")]
        public string itemId { get; set; }
    }

    [DataContract]
    public class user
    {
        [DataMember(Name = "lastLogin")]
        public long lastLogin { get; set; }
    }
    #endregion

    [DataContract]
    public class PopupInfo
    {
        [DataMember(Name = "description")]
        public string description { get; set; }

        [DataMember(Name = "fieldInfos")]
        public List<FieldInfo> fieldInfos { get; set; }

        [DataMember(Name = "mediaInfos")]
        public List<MediaInfo> mediaInfos { get; set; }

        [DataMember(Name = "Title")]
        public string Title { get; set; }

        [DataMember(Name = "showAttachments")]
        public bool showAttachments { get; set; }

        [DataContract]
        public class FieldInfo
        {
            [DataMember(Name = "fieldName")]
            public string fieldName { get; set; }

            [DataMember(Name = "format")]
            public Format format { get; set; }

            [DataMember(Name = "isEditable")]
            public bool isEditable { get; set; }

            [DataMember(Name = "label")]
            public string label { get; set; }

            [DataMember(Name = "stringFieldOption")]
            public string stringFieldOption { get; set; }

            [DataMember(Name = "tooltip")]
            public string tooltip { get; set; }

            [DataMember(Name = "visible")]
            public bool visible { get; set; }

            [DataContract]
            public class Format
            {
                [DataMember(Name = "dateFormat")]
                public string dateFormat { get; set; }

                [DataMember(Name = "digitSeparator")]
                public bool digitSeparator { get; set; }

                [DataMember(Name = "places")]
                public int places { get; set; }
            }
        }

        [DataContract]
        public class MediaInfo
        {
            [DataMember(Name = "caption")]
            public string caption { get; set; }

            [DataMember(Name = "title")]
            public string title { get; set; }

            [DataMember(Name = "type")]
            public string type { get; set; }

            [DataMember(Name = "value")]
            public Value value { get; set; }

            [DataContract]
            public class Value
            {
                [DataMember(Name = "fields")]
                public List<string> fields { get; set; }

                [DataMember(Name = "linkURL")]
                public string linkURL { get; set; }

                [DataMember(Name = "normalizeField")]
                public string normalizeField { get; set; }

                [DataMember(Name = "sourceURL")]
                public string sourceURL { get; set; }
            }
        }
    }

    [DataContract]
    public class Extent
    {
        [DataMember(Name = "xmin")]
        public double xmin { get; set; }

        [DataMember(Name = "ymin")]
        public double ymin { get; set; }

        [DataMember(Name = "xmax")]
        public double xmax { get; set; }

        [DataMember(Name = "ymax")]
        public double ymax { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference spatialReference { get; set; }
    }

    /// <summary>
    /// Datacontract class to be used with FeatureServeice class
    /// </summary>
    [DataContract]
    public class FullExtent : Extent
    {
    }

    /// <summary>
    /// Datacontract class to be used with FeatureServeice class
    /// </summary>
    [DataContract]
    public class InitialExtent : Extent
    {
    }

    /// <summary>
    /// Datacontract class to be used with FeatureServeice class
    /// </summary>
    [DataContract]
    public class SpatialReference
    {
        [DataMember(Name = "wkid")]
        public int wkid { get; set; }

        [DataMember(Name = "wkt")]
        public string wkt { get; set; }

        [DataMember(Name = "latestWkid")]
        public int latestWkid { get; set; }

        [DataMember(Name = "vscWkid")]
        public int vcsWkid { get; set; }

        [DataMember(Name = "latestVcsWkid")]
        public int latestVcsWkid { get; set; }
    }
    #endregion

    #region MapService DataContracts
    /// <summary>
    /// DataContract for JSON response of a hosted map service. Unnecessary elements are commented out.
    /// Uncomment them, add classes if necessary to make them usable.
    /// Needs cleaning up
    /// </summary>
    [DataContract]
    public class MapService
    {
        //public double currentVersion { get; set; }
        //public string serviceDescription { get; set; }
        //public bool hasVersionedData { get; set; }
        //public bool supportsDisconnectedEditing { get; set; }
        //public bool syncEnabled { get; set; }
        //public string supportedQueryFormats { get; set; }
        //public int maxRecordCount { get; set; }

        [DataMember(Name = "capabilities")]
        public string capabilities { get; set; }

        //public string description { get; set; }
        //public string copyrightText { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference spatialReference { get; set; }

        [DataMember(Name = "initialExtent")]
        public InitialExtent initialExtent { get; set; }

        [DataMember(Name = "fullExtent")]
        public FullExtent fullExtent { get; set; }
        //public bool allowGeometryUpdates { get; set; }
        //public string units { get; set; }
        //public DocumentInfo documentInfo { get; set; }

        [DataMember(Name = "layers")]
        public List<Layer> layers { get; set; }

        [DataMember(Name = "tables")]
        public List<object> tables { get; set; }

        //public bool enableZDefaults { get; set; }

        [DataMember(Name = "lods")]
        public List<Lod> lods { get; set; }

        [DataMember(Name = "tileInfo")]
        public TileInfo tileInfo { get; set; }

        #region SubObjects
        [DataContract]
        public class TileInfo
        {
            [DataMember(Name = "rows")]
            public int rows { get; set; }

            [DataMember(Name = "cols")]
            public int cols { get; set; }

            [DataMember(Name = "dpi")]
            public int dpi { get; set; }

            [DataMember(Name = "format")]
            public string format { get; set; }

            [DataMember(Name = "compressionQuality")]
            public int compressionQuality { get; set; }

            [DataMember(Name = "storageFormat")]
            public string storageFormat { get; set; }

            [DataMember(Name = "Origin")]
            public Origin origin { get; set; }

            [DataMember(Name = "spatialReference")]
            public SpatialReference spatialReference { get; set; }

            [DataMember(Name = "lods")]
            public List<Lod> lods { get; set; }
        }

        [DataContract]
        public class Origin
        {
            [DataMember(Name = "x")]
            public double x { get; set; }

            [DataMember(Name = "y")]
            public double y { get; set; }
        }

        [DataContract]
        public class Lod
        {
            [DataMember(Name = "level")]
            public int level { get; set; }

            [DataMember(Name = "resolution")]
            public double resolution { get; set; }

            [DataMember(Name = "scale")]
            public double scale { get; set; }
        }
        #endregion
    }

    [DataContract]
    public class MapServiceLayersResource
    {
        [DataMember(Name = "layers")]
        public List<Layer> layers { get; set; }
    }
    #endregion

    #region SceneService DataContracts
    /// <summary>
    /// Data contract - for /serviceName/sceneServer/ resource
    /// </summary>
    [DataContract]
    public class SceneService
    {
        [DataMember(Name = "serviceName")]
        public string serviceName { get; set; }

        [DataMember(Name = "serviceVersion")]
        public string serviceVersion { get; set; }

        [DataMember(Name = "supportedBindings")]
        public List<string> supportedBindings { get; set; }

        [DataMember(Name = "supportedOperationsProfile")]
        public List<string> supportedOperationsProfile { get; set; }

        [DataMember(Name = "layers")]
        public List<LayerSummary> layers { get; set; }

        [DataContract]
        public class LayerSummary
        {
            [DataMember(Name = "id")]
            public int id { get; set; }

            [DataMember(Name = "name")]
            public string name { get; set; }

            [DataMember(Name = "alias")]
            public string alias { get; set; }

            [DataMember(Name = "lodType")]
            public string lodType { get; set; }

            [DataMember(Name = "href")]
            public string href { get; set; }
        }
    }

    [DataContract]
    public class SceneServiceLayersResource
    {
        [DataMember(Name = "layers")]
        public List<SceneLayerInfo> layers { get; set; }
    }

    #region sub objects
    [DataContract]
    public class SceneLayerInfo
    {
        [DataMember(Name = "id")]
        public int id { get; set; }

        [DataMember(Name = "version")]
        public string version { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "alias")]
        public string alias { get; set; }

        [DataMember(Name = "description")]
        public string description { get; set; }

        [DataMember(Name = "copyrightText")]
        public string copyrightText { get; set; }

        [DataMember(Name = "capabilities")]
        public List<Capabilities> capabilities { get; set; }

        [DataMember(Name = "store")]
        public Store store { get; set; }

        [DataContract]
        public enum Capabilities
        {
            [EnumMember]
            View,

            [EnumMember]
            Edit,

            [EnumMember]
            Query
        }

        [DataContract]
        public class Store
        {
            [DataMember(Name = "id")]
            public string id { get; set; }

            [DataMember(Name = "profile")]
            public string profile { get; set; }

            [DataMember(Name = "geometryType")]
            public GeometryType geometryType { get; set; }

            [DataMember(Name = "resourcePattern")]
            public ResourceType resourcePattern { get; set; }

            [DataMember(Name = "rootNode")]
            public string rootNode { get; set; }

            [DataMember(Name = "version")]
            public string version { get; set; }

            [DataMember(Name = "extent")]
            public float[] extent { get; set; }

            [DataMember(Name = "indexCRS")]
            public string indexCRS { get; set; }

            [DataMember(Name = "vertexCRS")]
            public string vertexCRS { get; set; }

            [DataMember(Name = "nidEncoding")]
            public string nidEncoding { get; set; }

            [DataMember(Name = "featureEncoding")]
            public string featureEncoding { get; set; }

            [DataMember(Name = "geometryEncoding")]
            public string geometryEncoding { get; set; }

            [DataMember(Name = "textureEncoding")]
            public string textureEncoding { get; set; }

            [DataMember(Name = "lodType")]
            public LodType lodType { get; set; }

            [DataMember(Name = "indexingScheme")]
            public IndexingScheme indexingScheme { get; set; }

            [DataMember(Name = "featureOrdering")]
            public FeatureOrdering featureOrdering { get; set; }

            [DataMember(Name = "defaultGeomterySchema")]
            public GeometrySchema defaultGeometryschema { get; set; }

            [DataMember(Name = "fields")]
            public List<AttributeField> fields { get; set; }

            //TO-DO not added TextureDefinition and MaterialDefinition. This not written out
            //in any of the scene services published as of 3/11/2015.

            [DataContract]
            public enum ResourceType
            {
                [EnumMember(Value = "3dNodeIndexDocument")]
                dddNodeIndexDocument,

                [EnumMember]
                FeatureData,

                [EnumMember]
                SharedResource,

                [EnumMember]
                Geometry,

                [EnumMember]
                Texture
            }

            [DataContract]
            public enum LodType
            {
                [EnumMember]
                FeatureTree,

                [EnumMember]
                MeshPyramid
            }

            [DataContract]
            public enum IndexingScheme
            {
                [EnumMember]
                esriRTree,

                [EnumMember]
                QuadTree,

                [EnumMember]
                AGOLTilingScheme
            }

            [DataContract]
            public enum FeatureOrdering
            {
                [EnumMember]
                ID,

                [EnumMember]
                Prominence,

                [EnumMember]
                Layer
            }

            [DataContract]
            public enum GeometryType
            {
                [EnumMember]
                featuremesh,

                [EnumMember]
                points,

                [EnumMember]
                lines,

                [EnumMember]
                polygons
            }
        }

        [DataContract]
        public class GeometrySchema
        {
            [DataMember(Name = "header")]
            public List<HeaderDefinition> header { get; set; }

            [DataMember(Name = "ordering")]
            public List<string> ordering { get; set; }

            [DataMember(Name = "vertexAttributes")]
            public List<VertexAttribute> vertexAttributes { get; set; }

            [DataMember(Name = "faces")]
            public List<Faces> faces { get; set; }

            [DataMember(Name = "featureAttributeOrder")]
            public List<string> featureAttributeOrder { get; set; }

            [DataMember(Name = "featureAttributes")]
            public FeatureAttributes featureAttributes { get; set; }

            #region sub objects
            [DataContract]
            public class HeaderDefinition
            {
                [DataMember(Name = "property")]
                public string property { get; set; }

                [DataMember(Name = "type")]
                public numericDataType type { get; set; }
            }

            [DataContract]
            public class VertexAttribute : GeometryAttribute { }

            [DataContract]
            public class Faces : GeometryAttribute { }

            [DataContract]
            public class FeatureAttributes
            {
                [DataMember(Name = "id")]
                public FeatureAttributeId id { get; set; }

                [DataMember(Name = "faceRange")]
                public FaceRange faceRange { get; set; }

                public class FeatureAttributeId : GeometryAttribute.CommonGeometryAttributes { }

                public class FaceRange : GeometryAttribute.CommonGeometryAttributes { }
            }

            public class GeometryAttribute
            {
                [DataMember(Name = "position")]
                public Position position { get; set; }

                [DataMember(Name = "normal")]
                public Normal normal { get; set; }

                [DataMember(Name = "uv0")]
                public Uv0 uv0 { get; set; }

                [DataMember(Name = "color")]
                public Color color { get; set; }

                public class CommonGeometryAttributes
                {
                    [DataMember(Name = "valueType")]
                    public numericDataType valueType { get; set; }

                    [DataMember(Name = "valuesPerElement")]
                    public int valuesPerElement { get; set; }
                }

                [DataContract]
                public class Position : CommonGeometryAttributes { }

                [DataContract]
                public class Normal : CommonGeometryAttributes { }

                [DataContract]
                public class Uv0 : CommonGeometryAttributes { }

                [DataContract]
                public class Color : CommonGeometryAttributes { }
            }
            #endregion
        }

        [DataContract]
        public class AttributeField
        {
            [DataMember(Name = "name")]
            public string name { get; set; }

            [DataMember(Name = "type")]
            public FieldType type { get; set; }

            [DataMember(Name = "alias")]
            public string alias { get; set; }
        }

        public enum numericDataType
        {
            [EnumMember]
            UInt8,
            [EnumMember]
            UInt16,
            [EnumMember]
            UInt32,
            [EnumMember]
            UInt64,
            [EnumMember]
            Int8,
            [EnumMember]
            Int16,
            [EnumMember]
            Int32,
            [EnumMember]
            Int64,
            [EnumMember]
            Float32,
            [EnumMember]
            Float64
        }

        public enum FieldType
        {
            [EnumMember]
            esriFieldTypeBlob,
            [EnumMember]
            esriFieldTypeDate,
            [EnumMember]
            esriFieldTypeDouble,
            [EnumMember]
            esriFieldTypeGeometry,
            [EnumMember]
            esriFieldTypeGlobalID,
            [EnumMember]
            esriFieldTypeGUID,
            [EnumMember]
            esriFieldTypeInteger,
            [EnumMember]
            esriFieldTypeOID,
            [EnumMember]
            esriFieldTypeSmallInteger,
            [EnumMember]
            esriFieldTypeString,
            [EnumMember]
            esriFieldTypeGroup
        }
    }

    #endregion
    #endregion

    #region WebScene item data DataContracts

    /// <summary>
    /// A generic service layer in a WebScene
    /// </summary>
    [DataContract]
    public class WebSceneServiceLayer
    {
        [DataMember(Name = "title")]
        public string title { get; set; }

        [DataMember(Name = "visibility")]
        public bool visibility { get; set; }

        [DataMember(Name = "opacity")]
        public double opacity { get; set; }

        [DataMember(Name = "url")]
        public string url { get; set; }

        [DataMember(Name = "layerType")]
        public string layerType { get; set; }

        [DataMember(Name = "layers")]
        public List<WebSceneServiceLayer> layers { get; set; }
    }

    /// <summary>
    /// Elevation layer within a basemap layer for a webscene
    /// </summary>
    [DataContract]
    public class ElevationLayer
    {
        [DataMember(Name = "url")]
        public string url { get; set; }

        [DataMember(Name = "id")]
        public string id { get; set; }

        [DataMember(Name = "layerType")]
        public string layerType { get; set; }
    }

    /// <summary>
    /// Root class when deserializing webscene item's data REST endpoint.
    /// </summary>
    [DataContract]
    public class WebSceneLayerInfo
    {
        [DataMember(Name = "operationalLayers")]
        public List<WebSceneServiceLayer> operationalLayers { get; set; }

        [DataMember(Name = "baseMap")]
        public BaseMap baseMap { get; set; }

        [DataMember(Name = "version")]
        public string version { get; set; }

        [DataMember(Name = "authoringApp")]
        public string authoringApp { get; set; }

        [DataMember(Name = "authoringAppVersion")]
        public string authoringAppVersion { get; set; }
    }
    #endregion

    #region WebMap item's data DataContracts
    /// <summary>
    /// Root class when deserializing webmap item's data REST endpoint.
    /// This is the starting point.
    /// </summary>
    [DataContract]
    public class WebMapLayerInfo
    {
        [DataMember(Name = "operationalLayers")]
        public List<WebMapServiceLayer> operationalLayers { get; set; }

        [DataMember(Name = "baseMap")]
        public BaseMap baseMap { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference spatialReference { get; set; }

        [DataMember(Name = "bookmarks")]
        public List<Bookmark> bookmarks { get; set; }

        [DataMember(Name = "widgets")]
        public List<Widget> widgets { get; set; }

        [DataMember(Name = "applicationProperties")]
        public ApplicationProperties applicationProperties { get; set; }

        [DataMember(Name = "version")]
        public float version { get; set; }

        [DataContract]
        public class ApplicationProperties
        {
            [DataMember(Name = "viewing")]
            public Viewing viewing { get; set; }

            [DataMember(Name = "editing")]
            public Editing editing { get; set; }

            [DataContract]
            public class Viewing
            {
                [DataMember(Name = "routing")]
                public Dictionary<string, bool> routing { get; set; }

                [DataMember(Name = "measure")]
                public Dictionary<string, bool> measure { get; set; }

                [DataMember(Name = "basemapGallery")]
                public Dictionary<string, bool> basemapGallery { get; set; }

                [DataMember(Name = "search")]
                public Search search { get; set; }

                [DataContract]
                public class Search
                {
                    [DataMember(Name = "enabled")]
                    public bool enabled { get; set; }

                    [DataMember(Name = "disablePlaceFinder")]
                    public bool disablePlaceFinder { get; set; }

                    [DataMember(Name = "hintText")]
                    public string hintText { get; set; }

                    [DataMember(Name = "layers")]
                    public List<Layer> layers { get; set; }
                }
            }

            [DataContract]
            public class Editing
            {
                [DataMember(Name = "locationTracking")]
                public LocationTracking locationTracking { get; set; }

                [DataContract]
                public class LocationTracking
                {
                    [DataMember(Name = "enabled")]
                    public bool enabled { get; set; }

                    [DataMember(Name = "info")]
                    public Info info { get; set; }
                }

                [DataContract]
                public class Info
                {
                    [DataMember(Name = "layerId")]
                    public string layerId { get; set; }

                    [DataMember(Name = "updateInterval")]
                    public double updateInterval { get; set; }
                }
            }
        }
    }

    #region OperationalLayers
    /// <summary>
    /// Spec for layers in a WebMap
    /// Complete WebMap Layer Spec. Up to date as of Nov 18 2014.
    /// </summary>
    [DataContract]
    public class WebMapServiceLayer
    {
        [DataMember(Name = "capabilities")]
        public string capabilities { get; set; }

        [DataMember(Name = "defaultVisibility")]
        public bool defaultVisibility { get; set; }

        [DataMember(Name = "disablePopup")]
        public bool disablePopup { get; set; }

        [DataMember(Name = "featureCollection")]
        public FeatureCollection featureCollection { get; set; } //yet to complete this object

        [DataMember(Name = "id")]
        public string id { get; set; }

        [DataMember(Name = "itemId")]
        public string itemId { get; set; }

        [DataMember(Name = "layerDefinition")]
        public LayerDefinition layerDefinition { get; set; }

        [DataMember(Name = "layers")]
        public List<Layer> layers { get; set; }

        [DataMember(Name = "layerType")]
        public string layerType { get; set; }

        [DataMember(Name = "maxScale")]
        public float maxScale { get; set; }

        [DataMember(Name = "minScale")]
        public float minScale { get; set; }

        [DataMember(Name = "mode")]
        public long mode { get; set; }

        [DataMember(Name = "opacity")]
        public double opacity { get; set; }

        [DataMember(Name = "parentLayerId")]
        public long parentLayerId { get; set; }

        [DataMember(Name = "popupInfo")]
        public PopupInfo popupInfo { get; set; }

        [DataMember(Name = "refreshInterval")]
        public float refreshInterval { get; set; }

        [DataMember(Name = "showLabels")]
        public bool showLabels { get; set; }

        [DataMember(Name = "showLegend")]
        public bool showLegend { get; set; }

        [DataMember(Name = "subLayerIds")]
        public List<long> subLayerIDs { get; set; }

        [DataMember(Name = "timeAnimation")]
        public bool timeAnimation { get; set; }

        [DataMember(Name = "title")]
        public string title { get; set; }

        [DataMember(Name = "token")]
        public string secureServiceToken { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "url")]
        public string url { get; set; }

        [DataMember(Name = "visibility")]
        public bool visibility { get; set; }

        [DataMember(Name = "visibleFolders")]
        public List<long> visibleFolders { get; set; }

        //fields for services from CSV files
        [DataMember(Name = "columnDelimiter")]
        public string columnDelimiter { get; set; }

        [DataMember(Name = "locationInfo")]
        public LocationInfo locationInfo { get; set; }

        //fields for WMS layer types
        [DataMember(Name = "copyright")]
        public string copyright { get; set; }

        [DataMember(Name = "extent")]
        public Extent extent { get; set; }

        [DataMember(Name = "format")]
        public string format { get; set; }

        [DataMember(Name = "legendUrl")]
        public string legendUrl { get; set; }

        [DataMember(Name = "maxHeight")]
        public double maxHeight { get; set; }

        [DataMember(Name = "maxWidth")]
        public double maxWidth { get; set; }

        [DataMember(Name = "mapUrl")]
        public string mapUrl { get; set; }

        [DataMember(Name = "spatialReferences")]
        public List<int> SRwkids { get; set; }

        [DataMember(Name = "version")]
        public string version { get; set; }

        //fields for image service layer
        [DataMember(Name = "bandIds")]
        public List<int> bandIds { get; set; }

        [DataMember(Name = "compressionQuality")]
        public int compressionQuality { get; set; }

        [DataMember(Name = "interpolation")]
        public string interpolation { get; set; }

        [DataMember(Name = "noData")]
        public string noData { get; set; }

        [DataMember(Name = "mosaicRule")]
        public string mosaicRule { get; set; }

        [DataMember(Name = "noDataInterpretation")]
        public string noDataInterpretation { get; set; }

        [DataMember(Name = "pixelType")]
        public string pixelType { get; set; }

        [DataMember(Name = "renderingRule")]
        public RenderingRule renderingRule { get; set; }
    }

    [DataContract]
    public class LayerDefinition
    {
        [DataMember(Name = "definitionExpression")]
        public string definitionExpression { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "displayField")]
        public string displayField { get; set; }

        [DataMember(Name = "definitionEditor")]
        public DefinitionEditor definitionEditor { get; set; }

        [DataMember(Name = "source")]
        public Source source { get; set; }

        [DataMember(Name = "extent")]
        public Extent extent { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference spatialReference { get; set; }

        [DataMember(Name = "drawingInfo")]
        public DrawingInfo drawingInfo { get; set; }

        [DataMember(Name = "hasAttachments")]
        public bool hasAttachments { get; set; }

        [DataMember(Name = "objectIdField")]
        public string objectIdField { get; set; }

        [DataMember(Name = "typeIdField")]
        public string typeIdField { get; set; }

        [DataMember(Name = "fields")]
        public List<Field> fields { get; set; }

        [DataMember(Name = "types")]
        public List<TypeIdType> types { get; set; }

        [DataMember(Name = "minScale")]
        public float minScale { get; set; }

        [DataMember(Name = "maxScale")]
        public float maxScale { get; set; }

        //For feature collection and CSV types
        [DataMember(Name = "geometryType")]
        public string geometryType { get; set; }

        [DataMember(Name = "templates")]
        public List<TypeIdType.Template> templates { set; get; }

        #region subObjects
        [DataContract]
        public class DefinitionEditor
        {
            [DataMember(Name = "parameterizedExpression")]
            public string parameterizedExpression { get; set; }

            [DataMember(Name = "inputs")]
            public List<Input> inputs { get; set; }
        }

        [DataContract]
        public class Input
        {
            [DataMember(Name = "hint")]
            public string hint { get; set; }

            [DataMember(Name = "prompt")]
            public string prompt { get; set; }

            [DataMember(Name = "parameters")]
            public List<Parameter> parameters { get; set; }
        }

        [DataContract]
        public class Parameter
        {
            [DataMember(Name = "type")]
            public string type { get; set; }

            [DataMember(Name = "fieldName")]
            public string fieldName { get; set; }

            [DataMember(Name = "parameterId")]
            public int parameterId { get; set; }

            [DataMember(Name = "defaultValue")]
            public string defaultValue { get; set; }
        }

        [DataContract]
        public class Source
        {
            [DataMember(Name = "type")]
            public string type { get; set; }

            [DataMember(Name = "mapLayerId")]
            public string mapLayerId { get; set; }

            [DataMember(Name = "gdbVersion")]
            public string gdbVersion { get; set; }
        }

        [DataContract]
        public class DrawingInfo
        {
            [DataMember(Name = "renderer")]
            public Renderer renderer { get; set; }

            [DataMember(Name = "transparency")]
            public int transparency { get; set; }

            [DataMember(Name = "labelingInfo")]
            public LabelingInfo labelingInfo { get; set; }

            [DataMember(Name = "fixedSymbols")]
            public bool fixedSymbols { get; set; }
        }

        [DataContract]
        public class Renderer
        {
            //Base and simple renderer
            [DataMember(Name = "type")]
            public string RenderType { get; set; }

            [DataMember(Name = "label")]
            public string label { get; set; }

            [DataMember(Name = "description")]
            public string description { get; set; }

            [DataMember(Name = "symbol")]
            public Symbol symbol { get; set; }

            [DataMember(Name = "rotationType")]
            public string rotationType { get; set; }

            [DataMember(Name = "rotationExpression")]
            public string rotationExpression { get; set; }

            //Unique value renderer
            [DataMember(Name = "field1")]
            public string field1 { get; set; }

            [DataMember(Name = "field2")]
            public string field2 { get; set; }

            [DataMember(Name = "field3")]
            public string field3 { get; set; }

            [DataMember(Name = "fieldDelimiter")]
            public string fieldDelimiter { get; set; }

            [DataMember(Name = "defaultSymbol")]
            public Symbol defaultSymbol { get; set; }

            [DataMember(Name = "uniqueValueInfo")]
            public List<UniqueValueInfo> uniqueValueInfo { get; set; }

            //Class break renderer
            [DataMember(Name = "field")]
            public string field { get; set; }

            [DataMember(Name = "classificationMethod")]
            public string classificationMethod { get; set; }

            [DataMember(Name = "normalizationType")]
            public string normalizationType { get; set; }

            [DataMember(Name = "normalizationField")]
            public string normalizationField { get; set; }

            [DataMember(Name = "normalizationTotal")]
            public string normalizationTotal { get; set; }

            [DataMember(Name = "backgroundFillSymbol")]
            public Symbol backgroundFillSymbol { get; set; }

            [DataMember(Name = "minValue")]
            public float minValue { get; set; }

            [DataMember(Name = "classBreakInfos")]
            public List<ClassBreakInfo> classBreakInfos { get; set; }

            #region Sub Objects
            [DataContract]
            public class Symbol : Outline
            {
                //[DataMember(Name = "type")]
                //public string type { get; set; }

                //[DataMember(Name = "style")]
                //public string style { get; set; }

                //[DataMember(Name = "color")]
                //public int[] color { get; set; }

                [DataMember(Name = "size")]
                public float size { get; set; }

                [DataMember(Name = "angle")]
                public float angle { get; set; }

                [DataMember(Name = "xoffset")]
                public float xOffset { get; set; }

                [DataMember(Name = "yoffset")]
                public float yOffset { get; set; }

                //[DataMember(Name = "outline")]
                //public Outline outline { get; set; }

                [DataMember(Name = "url")]
                public string url { get; set; }

                [DataMember(Name = "imageData")]
                public string imageData { get; set; }

                [DataMember(Name = "contentType")]
                public string contentType { get; set; }

                //[DataMember(Name = "width")]
                //public float width { get; set; }

                [DataMember(Name = "height")]
                public float height { get; set; }

                [DataMember(Name = "xscale")]
                public float xScale { get; set; }

                [DataMember(Name = "yscale")]
                public float yScale { get; set; }

                [DataMember(Name = "backgroundColor")]
                public int[] backgroundColor { get; set; }

                [DataMember(Name = "borderLineSize")]
                public float borderLineSize { get; set; }

                [DataMember(Name = "borderLineColor")]
                public int[] borderLineColor { get; set; }

                [DataMember(Name = "font")]
                public Font font { get; set; }

                [DataMember(Name = "haloColor")]
                public int[] haloColor { get; set; }

                [DataMember(Name = "haloSize")]
                public float haloSize { get; set; }

                [DataMember(Name = "horizontalAlignment")]
                public string horizontalAlignment { get; set; }

                [DataMember(Name = "kerning")]
                public bool kerning { get; set; }

                [DataMember(Name = "rightToLeft")]
                public bool rightToLeft { get; set; }

                [DataMember(Name = "rotated")]
                public bool rotated { get; set; }

                [DataMember(Name = "text")]
                public string text { get; set; }

                [DataMember(Name = "verticalAlignment")]
                public string verticalAlignment { get; set; }
            }

            [DataContract]
            public class Outline
            {
                [DataMember(Name = "type")]
                public string type { get; set; }

                [DataMember(Name = "style")]
                public string style { get; set; }

                [DataMember(Name = "color")]
                public int[] OutlineColor { get; set; }

                [DataMember(Name = "width")]
                public float width { get; set; }
            }

            [DataContract]
            public class Font
            {
                [DataMember(Name = "family")]
                public string family { get; set; }

                [DataMember(Name = "size")]
                public float size { get; set; }

                [DataMember(Name = "style")]
                public string style { get; set; }

                [DataMember(Name = "weight")]
                public string weight { get; set; }

                [DataMember(Name = "decoration")]
                public string decoration { get; set; }
            }

            [DataContract]
            public class UniqueValueInfo
            {
                [DataMember(Name = "value")]
                public string value { get; set; }

                [DataMember(Name = "label")]
                public string label { get; set; }

                [DataMember(Name = "description")]
                public string description { get; set; }

                [DataMember(Name = "symbol")]
                public Symbol symbol { get; set; }
            }

            [DataContract]
            public class ClassBreakInfo
            {
                [DataMember(Name = "classMinValue")]
                public float classMinValue { get; set; }

                [DataMember(Name = "classMaxValue")]
                public float classMaxValue { get; set; }

                [DataMember(Name = "label")]
                public string label { get; set; }

                [DataMember(Name = "description")]
                public string description { get; set; }

                [DataMember(Name = "symbol")]
                public Symbol symbol { get; set; }
            }
            #endregion
        }

        [DataContract]
        public class LabelingInfo
        {
            [DataMember(Name = "labelExpression")]
            public string labelExpression { get; set; }

            [DataMember(Name = "labelExpressionInfo")]
            public LabelExpressionInfo labelExpressionInfo { get; set; }

            [DataMember(Name = "labelPlacement")]
            public string labelPlacement { get; set; }

            [DataMember(Name = "useCodedValues")]
            public bool useCodedValues { get; set; }

            [DataMember(Name = "minScale")]
            public float minScale { get; set; }

            [DataMember(Name = "maxScale")]
            public float maxScale { get; set; }

            [DataMember(Name = "symbol")]
            public Renderer.Symbol symbol { get; set; }

            [DataMember(Name = "where")]
            public string where { get; set; }

            #region Sub Objects
            [DataContract]
            public class LabelExpressionInfo
            {
                [DataMember(Name = "value")]
                public string value { get; set; }
            }
            #endregion
        }

        [DataContract]
        public class Field
        {
            [DataMember(Name = "name")]
            public string name { get; set; }

            [DataMember(Name = "alias")]
            public string alias { get; set; }

            [DataMember(Name = "type")]
            public string type { get; set; }

            [DataMember(Name = "editable")]
            public bool editable { get; set; }

            [DataMember(Name = "nullable")]
            public bool nullable { get; set; }

            [DataMember(Name = "domain")]
            public Domain domain { get; set; }

            [DataContract]
            public class Domain
            {
                [DataMember(Name = "type")]
                public string type { get; set; }

                [DataMember(Name = "name")]
                public string name { get; set; }

                [DataMember(Name = "range")]
                public float[] range { get; set; }
            }
        }

        [DataContract]
        public class TypeIdType
        {
            [DataMember(Name = "id")]
            public int id { get; set; }

            [DataMember(Name = "name")]
            public string name { get; set; }

            [DataMember(Name = "domains")]
            //public Field.Domain [] domains { get; set; }
            public Field.Domain domains { get; set; }

            [DataMember(Name = "template")]
            public Template template { get; set; }

            [DataContract]
            public class Template
            {
                [DataMember(Name = "description")]
                public string description { get; set; }

                [DataMember(Name = "name")]
                public string name { get; set; }

                [DataMember(Name = "prototype")]
                public Feature prototype { get; set; }

                [DataMember(Name = "drawingTool")]
                public string drawingTool { get; set; }
            }
        }

        [DataContract]
        public class Feature
        {
            [DataMember(Name = "attributes")]
            public object attributes { get; set; }

            [DataMember(Name = "geometry")]
            public object geometry { get; set; }

            [DataMember(Name = "symbol")]
            public Renderer.Symbol symbol { get; set; }
        }
        #endregion
    }

    [DataContract]
    public class FeatureCollection
    {
        [DataMember(Name = "layers")]
        public List<Layer> layers { get; set; }

        [DataMember(Name = "showLegend")]
        public bool showLegend { get; set; }
    }

    [DataContract]
    public class LocationInfo
    {
        [DataMember(Name = "locationType")]
        public string locationType { get; set; }

        [DataMember(Name = "latitudeFieldName")]
        public string latitudeFieldName { get; set; }

        [DataMember(Name = "longitudeFieldName")]
        public string longitudeFieldName { get; set; }
    }

    [DataContract]
    public class RenderingRule
    {
        [DataMember(Name = "rasterFunction")]
        public string rasterFunction { get; set; }
    }
    #endregion

    #region BasemapLayers
    [DataContract]
    public class BaseMap
    {
        [DataMember(Name = "baseMapLayers")]
        public List<BaseMapLayer> baseMapLayers { get; set; }

        [DataMember(Name = "title")]
        public string title { get; set; }

        //For basemaps of webscenes
        [DataMember(Name = "elevationLayers")]
        public List<ElevationLayer> elevationLayers { get; set; }

        [DataContract]
        public class BaseMapLayer
        {
            [DataMember(Name = "id")]
            public string id { get; set; }

            [DataMember(Name = "maxScale")]
            public float maxScale { get; set; }

            [DataMember(Name = "minScale")]
            public float minScale { get; set; }

            [DataMember(Name = "isReference")]
            public bool isReference { get; set; }

            [DataMember(Name = "opacity")]
            public double opacity { get; set; }

            [DataMember(Name = "portalUrl")]
            public string portalUrl { get; set; }

            [DataMember(Name = "showLegend")]
            public bool showLegend { get; set; }

            [DataMember(Name = "type")]
            public string type { get; set; }

            [DataMember(Name = "layerType")]
            public string layerType { get; set; }

            [DataMember(Name = "url")]
            public string url { get; set; }

            [DataMember(Name = "visibility")]
            public bool visibility { get; set; }

            [DataMember(Name = "exclusionAreas")]
            public List<ExclusionArea> exclusionAreas { get; set; }

            public class ExclusionArea
            {
                [DataMember(Name = "minZoom")]
                public int minZoom { get; set; }

                [DataMember(Name = "maxZoom")]
                public int maxZoom { get; set; }

                [DataMember(Name = "maxScale")]
                public float maxScale { get; set; }

                [DataMember(Name = "minScale")]
                public float minScale { get; set; }

                [DataMember(Name = "geometry")]
                public Extent geometry { get; set; }
            }
        }
    }
    #endregion

    [DataContract]
    public class Bookmark
    {
        [DataMember(Name = "extent")]
        public Extent extent { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }
    }

    [DataContract]
    public class Widget
    {
    }
    #endregion

    #region Online Item DataContracts
    [DataContract]
    public class SDCItem
    {
        [DataMember(Name = "id")]
        public string id { get; set; }

        [DataMember(Name = "owner")]
        public string owner { get; set; }

        [DataMember(Name = "created")]
        public long created { get; set; }

        [DataMember(Name = "modified")]
        public long modified { get; set; }

        [DataMember(Name = "guid")]
        public string guid { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "title")]
        public string title { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "typeKeywords")]
        public List<string> typeKeywords { get; set; }

        [DataMember(Name = "description")]
        public string description { get; set; }

        [DataMember(Name = "tags")]
        public List<string> tags { get; set; }

        [DataMember(Name = "snippet")]
        public string snippet { get; set; }

        [DataMember(Name = "thumbnail")]
        public string thumbnail { get; set; }

        [DataMember(Name = "documentation")]
        public string documentation { get; set; }

        [DataMember(Name = "extent")]
        public List<List<double>> extent { get; set; }

        [DataMember(Name = "spatialReference")]
        public string spatialReference { get; set; }

        [DataMember(Name = "accessInformation")]
        public string accessInformation { get; set; }

        [DataMember(Name = "licenseInfo")]
        public string licenseInfo { get; set; }

        [DataMember(Name = "culture")]
        public string culture { get; set; }

        [DataMember(Name = "properties")]
        public string properties { get; set; }

        [DataMember(Name = "url")]
        public string url { get; set; }

        [DataMember(Name = "access")]
        public string access { get; set; }

        [DataMember(Name = "size")]
        public long size { get; set; }

        [DataMember(Name = "appCategories")]
        public List<string> appCategories { get; set; }

        [DataMember(Name = "industries")]
        public List<string> industries { get; set; }

        [DataMember(Name = "languages")]
        public List<string> languages { get; set; }

        [DataMember(Name = "largeThumbnail")]
        public string largeThumbnail { get; set; }

        [DataMember(Name = "banner")]
        public string banner { get; set; }

        [DataMember(Name = "screenshots")]
        public List<string> screenshots { get; set; }

        [DataMember(Name = "listed")]
        public bool listed { get; set; }

        [DataMember(Name = "ownerFolder")]
        public string ownerFolder { get; set; }

        [DataMember(Name = "protected")]
        public bool @protected { get; set; }

        [DataMember(Name = "numComments")]
        public long numComments { get; set; }

        [DataMember(Name = "numRatings")]
        public long numRatings { get; set; }

        [DataMember(Name = "avgRating")]
        public double avgRating { get; set; }

        [DataMember(Name = "numViews")]
        public long numViews { get; set; }
    }

    [DataContract]
    public class Sharing
    {
        [DataMember(Name = "access")]
        public string access { get; set; }

        [DataMember(Name = "groups")]
        public List<string> groups { get; set; }
    }

    /// <summary>
    /// DataContract to deserialize any online item. Can be used for packages, services, webmaps, webscenes etc.
    /// For webmaps, webscenes, to know the contents, use WebMapLayerInfo, WebSceneLayerInfo classes.
    /// </summary>
    [DataContract]
    public class OnlineItem
    {
        [DataMember(Name = "item")]
        public SDCItem item { get; set; }

        [DataMember(Name = "sharing")]
        public Sharing sharing { get; set; }
    }
    #endregion

    #region Search result DataContracts
    [DataContract]
    public class SearchResult
    {
        [DataMember(Name = "query")]
        public string query { get; set; }

        [DataMember(Name = "total")]
        public int total { get; set; }

        [DataMember(Name = "start")]
        public int start { get; set; }

        [DataMember(Name = "num")]
        public int num { get; set; }

        [DataMember(Name = "nextStart")]
        public int nextStart { get; set; }

        [DataMember(Name = "results")]
        public List<SharingDataContracts.SDCItem> results { get; set; }
    }

    //[DataContract]
    //public class Result: SharingDataContracts.Item
    //{

    //}
    #endregion

    #region esriTransportTypeUrl DataContracts
    [DataContract]
    public class esriTransportTypeUrl
    {
        [DataMember(Name = "transportType")]
        public string transportType { get; set; }

        [DataMember(Name = "responseType")]
        public string responseType { get; set; }

        [DataMember(Name = "URL")]
        public string URL { get; set; }
    }
    #endregion

    #region Replica DataContracts
    [DataContract]
    public class Replica
    {
        [DataMember(Name = "replicaName")]
        public string replicaName { get; set; }

        [DataMember(Name = "replicaID")]
        public string replicaID { get; set; }
    }
    #endregion

    #region ExportMap Response DataContracts
    [DataContract]
    public class MapResponse
    {
        [DataMember(Name = "href")]
        public string href { get; set; }

        [DataMember(Name = "width")]
        public int width { get; set; }

        [DataMember(Name = "height")]
        public int height { get; set; }

        [DataMember(Name = "extent")]
        public InitialExtent extent { get; set; }

        [DataMember(Name = "scale")]
        public double scale { get; set; }

    }

    #endregion

    #region SearchGroup result DataContracts
    /// <summary>
    /// Search Group result is similar to searched item result but the results have slightly different KVP
    /// Update: not inheriting SearchResult class. Not efficient but deserializer ends up with ambiguity.
    /// </summary>
    [DataContract]
    public class SearchGroupResult
    {
        [DataMember(Name = "query")]
        public string query { get; set; }

        [DataMember(Name = "total")]
        public int total { get; set; }

        [DataMember(Name = "start")]
        public int start { get; set; }

        [DataMember(Name = "num")]
        public int num { get; set; }

        [DataMember(Name = "nextStart")]
        public int nextStart { get; set; }

        [DataMember(Name = "results")]
        public List<SearchGroupItem> results { get; set; }
    }

    /// <summary>
    /// SearchGroupItem contails all of UserGroups object and more KVPs
    /// </summary>
    [DataContract]
    public class SearchGroupItem : UserGroups
    {
        [DataMember(Name = "tags")]
        public List<string> tags { get; set; }

        [DataMember(Name = "phone")]
        public object phone { get; set; }

        [DataMember(Name = "sortField")]
        public string sortField { get; set; }

        [DataMember(Name = "sortOrder")]
        public string sortOrder { get; set; }

        [DataMember(Name = "isFav")]
        public bool isFav { get; set; }

        [DataMember(Name = "created")]
        public new long created { get; set; }

        [DataMember(Name = "modified")]
        public long modified { get; set; }

        [DataMember(Name = "provider")]
        public object provider { get; set; }

        [DataMember(Name = "providerGroupName")]
        public object providerGroupName { get; set; }

        [DataMember(Name = "isReadOnly")]
        public bool isReadOnly { get; set; }

        [DataMember(Name = "access")]
        public string access { get; set; }
    }
    #endregion

    #region TestForDeepCompare DataContracts
    [DataContract]
    public class AllDataTypes
    {
        [DataMember]
        public int Integer { get; set; }

        [DataMember]
        public float FloatingPoint { get; set; }

        [DataMember]
        public string Stringer { get; set; }

        [DataMember]
        public List<int> intList { get; set; }

        [DataMember]
        public List<string> stringList { get; set; }

        [DataMember]
        public ContainedClass cClass { get; set; }

        [DataMember]
        public int[] intArray { get; set; }

        [DataMember]
        public string[] stringArray { get; set; }

        [DataContract]
        public class ContainedClass
        {
            [DataMember]
            public int Integer { get; set; }

            [DataMember]
            public List<string> stringList { get; set; }
        }
    }

    [DataContract]
    public class RecursiveCollections
    {
        [DataMember]
        public List<List<int>> twoLevelIntList { get; set; }

        [DataMember]
        public List<AllDataTypes.ContainedClass> classList { get; set; }
    }
    #endregion
}
