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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Desktop.Tests.APIHelpers.SharingDataContracts
{
  #region agoUser and related
  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class agoUser
#pragma warning restore IDE1006 // Naming Styles
  {
    [DataMember(Name = "username")]
#pragma warning disable IDE1006 // Naming Styles
    public string username { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "orgId")]
#pragma warning disable IDE1006 // Naming Styles
    public string orgID { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "role")]
#pragma warning disable IDE1006 // Naming Styles
    public string role { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "tags")]
    public string[] Tags { get; set; }

    [DataMember(Name = "groups")]
#pragma warning disable IDE1006 // Naming Styles
    public UserGroups[] groups { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
    public Int64 created { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "snippet")]
#pragma warning disable IDE1006 // Naming Styles
    public string snippet { get; set; }
#pragma warning restore IDE1006 // Naming Styles

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
#pragma warning disable IDE1006 // Naming Styles
    public string capabilities { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    //public string description { get; set; }
    //public string copyrightText { get; set; }

    [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
    public SpatialReference spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "initialExtent")]
#pragma warning disable IDE1006 // Naming Styles
    public InitialExtent initialExtent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "fullExtent")]
#pragma warning disable IDE1006 // Naming Styles
    public FullExtent fullExtent { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    //public bool allowGeometryUpdates { get; set; }
    //public string units { get; set; }
    //public DocumentInfo documentInfo { get; set; }

    [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Layer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "tables")]
#pragma warning disable IDE1006 // Naming Styles
    public List<object> tables { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    //public bool enableZDefaults { get; set; }
  }

  /// <summary>
  /// Datacontract class to be used with FeatureServeice class, WebMapServiceInfo class
  /// </summary>
  [DataContract]
  public class Layer
  {
      [DataMember(Name = "defaultVisibility")]
#pragma warning disable IDE1006 // Naming Styles
      public bool defaultVisibility { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "layerDefinition")]
#pragma warning disable IDE1006 // Naming Styles
      public LayerDefinition layerDefinition { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "legendUrl")]
#pragma warning disable IDE1006 // Naming Styles
      public string legendUrl { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "maxScale")]
#pragma warning disable IDE1006 // Naming Styles
      public float maxScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "minScale")]
#pragma warning disable IDE1006 // Naming Styles
      public float minScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "parentLayerId")]
#pragma warning disable IDE1006 // Naming Styles
      public string parentLayerId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "popupInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public PopupInfo popupInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "showLegend")]
#pragma warning disable IDE1006 // Naming Styles
      public bool showLegend { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  #region returned by analyze request
  [DataContract]
  public class AnalyzedService
  {
      [DataMember(Name = "filesize")]
#pragma warning disable IDE1006 // Naming Styles
      public double filesize { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "fileUrl")]
#pragma warning disable IDE1006 // Naming Styles
      public string fileUrl { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "publishParameters")]
#pragma warning disable IDE1006 // Naming Styles
      public PublishParameters publishParameters { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "records")]
#pragma warning disable IDE1006 // Naming Styles
      public List<Attributes> records { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
  public class Attributes
  {
      [DataMember(Name = "Name")]
      public string Name { get; set; }

      [DataMember(Name = "latitude")]
#pragma warning disable IDE1006 // Naming Styles
      public float latitude { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "longitude")]
#pragma warning disable IDE1006 // Naming Styles
      public float longitude { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "Address")]
      public string Address { get; set; }
  }

  [DataContract]
  public class PublishParameters
  {
      [DataMember(Name = "columnDelimiter")]
#pragma warning disable IDE1006 // Naming Styles
      public string columnDelimiter { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "editorTrackingInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public EditorTrackingInfo editorTrackingInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "latitudeFieldName")]
#pragma warning disable IDE1006 // Naming Styles
      public string latitudeFieldName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "longitudeFieldName")]
#pragma warning disable IDE1006 // Naming Styles
      public string longitudeFieldName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "layerInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public AnalyzedServiceLayerInfo layerInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "locationType")]
#pragma warning disable IDE1006 // Naming Styles
      public string locationType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "maxRecordCount")]
#pragma warning disable IDE1006 // Naming Styles
      public double maxRecordCount { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "qualifier")]
#pragma warning disable IDE1006 // Naming Styles
      public string qualifier { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
      public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "sourceSR")]
#pragma warning disable IDE1006 // Naming Styles
      public SpatialReference sourceSR { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "targetSR")]
#pragma warning disable IDE1006 // Naming Styles
      public SpatialReference targetSR { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
  public class AnalyzedServiceLayerInfo
  {
      [DataMember(Name = "advancedQueryCapabilities")]
#pragma warning disable IDE1006 // Naming Styles
      public AdvancedQueryCapabilities advancedQueryCapabilities { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "allowGeometryUpdates")]
#pragma warning disable IDE1006 // Naming Styles
      public bool allowGeometryUpdates { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
      public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "copyrightText")]
#pragma warning disable IDE1006 // Naming Styles
      public string copyrightText { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "capabilities")]
#pragma warning disable IDE1006 // Naming Styles
      public string capabilities { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "currentVersion")]
#pragma warning disable IDE1006 // Naming Styles
      public string currentVersion { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "defaultVisibility")]
#pragma warning disable IDE1006 // Naming Styles
      public bool defaultVisibility { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "drawingInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public LayerDefinition.DrawingInfo drawingInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "fields")]
#pragma warning disable IDE1006 // Naming Styles
      public List<field> fields { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "geometryType")]
#pragma warning disable IDE1006 // Naming Styles
      public string geometryType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "hasAttachments")]
#pragma warning disable IDE1006 // Naming Styles
      public bool hasAttachments { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "hasStaticData")]
#pragma warning disable IDE1006 // Naming Styles
      public bool hasStaticData { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
      public string ASLI_type { get; set; }
  }

  [DataContract]
  public class AdvancedQueryCapabilities
  {
      [DataMember(Name = "supportsDistinct")]
#pragma warning disable IDE1006 // Naming Styles
      public bool supportsDistinct { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "supportsOrderBy")]
#pragma warning disable IDE1006 // Naming Styles
      public bool supportsOrderBy { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "supportsPagination")]
#pragma warning disable IDE1006 // Naming Styles
      public bool supportsPagination { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "supportsQueryWithDistance")]
#pragma warning disable IDE1006 // Naming Styles
      public bool supportsQueryWithDistance { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "supportsReturningQueryExtent")]
#pragma warning disable IDE1006 // Naming Styles
      public bool supportsReturningQueryExtent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "supportsStatistics")]
#pragma warning disable IDE1006 // Naming Styles
      public bool supportsStatistics { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
  public class EditorTrackingInfo
  {
      [DataMember(Name = "allowOthersToDelete")]
#pragma warning disable IDE1006 // Naming Styles
      public bool allowOthersToDelete { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "allowOthersToUpdate")]
#pragma warning disable IDE1006 // Naming Styles
      public bool allowOthersToUpdate { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "enableEditorTracking")]
#pragma warning disable IDE1006 // Naming Styles
      public bool enableEditorTracking { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "enableOwnershipAccessControl")]
#pragma warning disable IDE1006 // Naming Styles
      public bool enableOwnershipAccessControl { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region returned by isServiceNameAvailable request
  [DataContract]
  public class AvailableResult
  {
      [DataMember(Name = "available")]
#pragma warning disable IDE1006 // Naming Styles
      public bool available { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region returned by publish request
  [DataContract]
  public class PublishedServices
  {
      [DataMember(Name = "services")]
#pragma warning disable IDE1006 // Naming Styles
      public List<PublishedService> services { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
  public class PublishedService
  {
      [DataMember(Name = "encodedServiceURL")]
#pragma warning disable IDE1006 // Naming Styles
      public string encodedServiceURL { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "jobId")]
#pragma warning disable IDE1006 // Naming Styles
      public string jobId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "serviceItemId")]
#pragma warning disable IDE1006 // Naming Styles
      public string serviceItemId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "serviceurl")]
#pragma warning disable IDE1006 // Naming Styles
      public string serviceurl { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "size")]
#pragma warning disable IDE1006 // Naming Styles
      public double size { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
      public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region returned by applyEdits request
  [DataContract]
  public class EditsResponse
  {
      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "addResults")]
#pragma warning disable IDE1006 // Naming Styles
      public List<SingleEditsResult> addResults { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "addResults")]
#pragma warning disable IDE1006 // Naming Styles
      public List<SingleEditsResult> updateResults { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "addResults")]
#pragma warning disable IDE1006 // Naming Styles
      public List<SingleEditsResult> deleteResults { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

    [DataContract]
  public class SingleEditsResult
  {
      [DataMember(Name = "objectId")]
#pragma warning disable IDE1006 // Naming Styles
      public int objectId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "globalId")]
#pragma warning disable IDE1006 // Naming Styles
      public int globalId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "success")]
#pragma warning disable IDE1006 // Naming Styles
      public bool success { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region returned by query request
  [DataContract]
  public class QueryLayers
  {
      [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
      public List<QueryLayer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
  public class QueryLayer
  {
      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "fields")]
#pragma warning disable IDE1006 // Naming Styles
      public List<field> fields { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "features")]
#pragma warning disable IDE1006 // Naming Styles
      public List<feature> features { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class field
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class feature
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "attributes")]
#pragma warning disable IDE1006 // Naming Styles
      public FeatureAttributes attributes { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "geometry")]
#pragma warning disable IDE1006 // Naming Styles
      public FeatureGeometry geometry { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
      public double x { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "y")]
#pragma warning disable IDE1006 // Naming Styles
      public double y { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region returned by userlicenses request
  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class userLicenses
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "signature")]
#pragma warning disable IDE1006 // Naming Styles
      public string signature { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "userEntitlementsString")]
#pragma warning disable IDE1006 // Naming Styles
      public string userEntitlementsString { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class userEntitlementsString
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "username")]
#pragma warning disable IDE1006 // Naming Styles
      public string username { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "lastLogin")]
#pragma warning disable IDE1006 // Naming Styles
      public long lastLogin { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "disconnected")]
#pragma warning disable IDE1006 // Naming Styles
      public bool disconnected { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "entitlements")]
#pragma warning disable IDE1006 // Naming Styles
      public List<string> entitlements { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "licenses")]
#pragma warning disable IDE1006 // Naming Styles
      public List<string> licenses { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "nonce")]
#pragma warning disable IDE1006 // Naming Styles
      public string nonce { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "timestamp")]
#pragma warning disable IDE1006 // Naming Styles
      public long timestamp { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region returned by portal self request
  [DataContract]
  public class PortalSelf
  {
      [DataMember(Name = "appInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public appInfo appInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "created")]
#pragma warning disable IDE1006 // Naming Styles
      public long created { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "user")]
#pragma warning disable IDE1006 // Naming Styles
      public user user { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class appInfo
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "itemId")]
#pragma warning disable IDE1006 // Naming Styles
      public string itemId { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class user
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "lastLogin")]
#pragma warning disable IDE1006 // Naming Styles
      public long lastLogin { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  [DataContract]
  public class PopupInfo
  {
    [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
    public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "fieldInfos")]
#pragma warning disable IDE1006 // Naming Styles
    public List<FieldInfo> fieldInfos { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "mediaInfos")]
#pragma warning disable IDE1006 // Naming Styles
    public List<MediaInfo> mediaInfos { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "Title")]
    public string Title { get; set; }

    [DataMember(Name = "showAttachments")]
#pragma warning disable IDE1006 // Naming Styles
    public bool showAttachments { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataContract]
    public class FieldInfo
    {
      [DataMember(Name = "fieldName")]
#pragma warning disable IDE1006 // Naming Styles
      public string fieldName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "format")]
#pragma warning disable IDE1006 // Naming Styles
      public Format format { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "isEditable")]
#pragma warning disable IDE1006 // Naming Styles
      public bool isEditable { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "label")]
#pragma warning disable IDE1006 // Naming Styles
      public string label { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "stringFieldOption")]
#pragma warning disable IDE1006 // Naming Styles
      public string stringFieldOption { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      
      [DataMember(Name = "tooltip")]
#pragma warning disable IDE1006 // Naming Styles
      public string tooltip { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "visible")]
#pragma warning disable IDE1006 // Naming Styles
      public bool visible { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataContract]
      public class Format
      {
        [DataMember(Name = "dateFormat")]
#pragma warning disable IDE1006 // Naming Styles
        public string dateFormat { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "digitSeparator")]
#pragma warning disable IDE1006 // Naming Styles
        public bool digitSeparator { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "places")]
#pragma warning disable IDE1006 // Naming Styles
        public int places { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }
    }

    [DataContract]
    public class MediaInfo
    {
      [DataMember(Name = "caption")]
#pragma warning disable IDE1006 // Naming Styles
      public string caption { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "title")]
#pragma warning disable IDE1006 // Naming Styles
      public string title { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
      public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "value")]
#pragma warning disable IDE1006 // Naming Styles
      public Value value { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataContract]
      public class Value
      {
        [DataMember(Name = "fields")]
#pragma warning disable IDE1006 // Naming Styles
        public List<string> fields { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "linkURL")]
#pragma warning disable IDE1006 // Naming Styles
        public string linkURL { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "normalizeField")]
#pragma warning disable IDE1006 // Naming Styles
        public string normalizeField { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "sourceURL")]
#pragma warning disable IDE1006 // Naming Styles
        public string sourceURL { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }
    }
  }

  [DataContract]
  public class Extent
  {
    [DataMember(Name = "xmin")]
#pragma warning disable IDE1006 // Naming Styles
    public double xmin { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "ymin")]
#pragma warning disable IDE1006 // Naming Styles
    public double ymin { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "xmax")]
#pragma warning disable IDE1006 // Naming Styles
    public double xmax { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "ymax")]
#pragma warning disable IDE1006 // Naming Styles
    public double ymax { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
    public SpatialReference spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  
  /// <summary>
  /// Datacontract class to be used with FeatureServeice class
  /// </summary>
  [DataContract]
  public class FullExtent:Extent
  {
  }

  /// <summary>
  /// Datacontract class to be used with FeatureServeice class
  /// </summary>
  [DataContract]
  public class InitialExtent:Extent
  {
  }

  /// <summary>
  /// Datacontract class to be used with FeatureServeice class
  /// </summary>
  [DataContract]
  public class SpatialReference
  {
    [DataMember(Name="wkid")]
#pragma warning disable IDE1006 // Naming Styles
    public int wkid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "wkt")]
#pragma warning disable IDE1006 // Naming Styles
    public string wkt { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="latestWkid")]
#pragma warning disable IDE1006 // Naming Styles
    public int latestWkid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "vscWkid")]
#pragma warning disable IDE1006 // Naming Styles
    public int vcsWkid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "latestVcsWkid")]
#pragma warning disable IDE1006 // Naming Styles
    public int latestVcsWkid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
    public string capabilities { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    //public string description { get; set; }
    //public string copyrightText { get; set; }

    [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
    public SpatialReference spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "initialExtent")]
#pragma warning disable IDE1006 // Naming Styles
    public InitialExtent initialExtent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "fullExtent")]
#pragma warning disable IDE1006 // Naming Styles
    public FullExtent fullExtent { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    //public bool allowGeometryUpdates { get; set; }
    //public string units { get; set; }
    //public DocumentInfo documentInfo { get; set; }

    [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Layer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "tables")]
#pragma warning disable IDE1006 // Naming Styles
    public List<object> tables { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    //public bool enableZDefaults { get; set; }

    [DataMember(Name = "lods")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Lod> lods { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "tileInfo")]
#pragma warning disable IDE1006 // Naming Styles
    public TileInfo tileInfo {get; set; }
#pragma warning restore IDE1006 // Naming Styles

    #region SubObjects
    [DataContract]
    public class TileInfo
    {
      [DataMember(Name = "rows")]
#pragma warning disable IDE1006 // Naming Styles
      public int rows { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "cols")]
#pragma warning disable IDE1006 // Naming Styles
      public int cols { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "dpi")]
#pragma warning disable IDE1006 // Naming Styles
      public int dpi { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "format")]
#pragma warning disable IDE1006 // Naming Styles
      public string format { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "compressionQuality")]
#pragma warning disable IDE1006 // Naming Styles
      public int compressionQuality { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "storageFormat")]
#pragma warning disable IDE1006 // Naming Styles
      public string storageFormat { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "Origin")]
#pragma warning disable IDE1006 // Naming Styles
      public Origin origin { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
      public SpatialReference spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "lods")]
#pragma warning disable IDE1006 // Naming Styles
      public List<Lod> lods { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    [DataContract]
    public class Origin
    {
      [DataMember(Name = "x")]
#pragma warning disable IDE1006 // Naming Styles
      public double x { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "y")]
#pragma warning disable IDE1006 // Naming Styles
      public double y { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    [DataContract]
    public class Lod
    {
      [DataMember(Name = "level")]
#pragma warning disable IDE1006 // Naming Styles
      public int level { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "resolution")]
#pragma warning disable IDE1006 // Naming Styles
      public double resolution { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "scale")]
#pragma warning disable IDE1006 // Naming Styles
      public double scale { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
    #endregion
  }

  [DataContract]
  public class MapServiceLayersResource
  {
    [DataMember(Name="layers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Layer> layers {get; set;}
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
    public string serviceName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "serviceVersion")]
#pragma warning disable IDE1006 // Naming Styles
    public string serviceVersion { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "supportedBindings")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> supportedBindings { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "supportedOperationsProfile")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> supportedOperationsProfile { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<LayerSummary> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataContract]
    public class LayerSummary
    {
      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "alias")]
#pragma warning disable IDE1006 // Naming Styles
      public string alias { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "lodType")]
#pragma warning disable IDE1006 // Naming Styles
      public string lodType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "href")]
#pragma warning disable IDE1006 // Naming Styles
      public string href { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
  }

  [DataContract]
  public class SceneServiceLayersResource
  {
    [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<SceneLayerInfo> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  #region sub objects
  [DataContract]
  public class SceneLayerInfo
  {
    [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
    public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "version")]
#pragma warning disable IDE1006 // Naming Styles
    public string version { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
    public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "alias")]
#pragma warning disable IDE1006 // Naming Styles
    public string alias { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
    public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "copyrightText")]
#pragma warning disable IDE1006 // Naming Styles
    public string copyrightText { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "capabilities")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Capabilities> capabilities { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "store")]
#pragma warning disable IDE1006 // Naming Styles
    public Store store { get; set; }
#pragma warning restore IDE1006 // Naming Styles

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
#pragma warning disable IDE1006 // Naming Styles
      public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "profile")]
#pragma warning disable IDE1006 // Naming Styles
      public string profile { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name="geometryType")]
#pragma warning disable IDE1006 // Naming Styles
      public GeometryType geometryType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "resourcePattern")]
#pragma warning disable IDE1006 // Naming Styles
      public ResourceType resourcePattern { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "rootNode")]
#pragma warning disable IDE1006 // Naming Styles
      public string rootNode { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "version")]
#pragma warning disable IDE1006 // Naming Styles
      public string version { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "extent")]
#pragma warning disable IDE1006 // Naming Styles
      public float[] extent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "indexCRS")]
#pragma warning disable IDE1006 // Naming Styles
      public string indexCRS { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "vertexCRS")]
#pragma warning disable IDE1006 // Naming Styles
      public string vertexCRS { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "nidEncoding")]
#pragma warning disable IDE1006 // Naming Styles
      public string nidEncoding { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "featureEncoding")]
#pragma warning disable IDE1006 // Naming Styles
      public string featureEncoding { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "geometryEncoding")]
#pragma warning disable IDE1006 // Naming Styles
      public string geometryEncoding { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "textureEncoding")]
#pragma warning disable IDE1006 // Naming Styles
      public string textureEncoding { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "lodType")]
#pragma warning disable IDE1006 // Naming Styles
      public LodType lodType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "indexingScheme")]
#pragma warning disable IDE1006 // Naming Styles
      public IndexingScheme indexingScheme { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "featureOrdering")]
#pragma warning disable IDE1006 // Naming Styles
      public FeatureOrdering featureOrdering { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "defaultGeomterySchema")]
#pragma warning disable IDE1006 // Naming Styles
      public GeometrySchema defaultGeometryschema { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "fields")]
#pragma warning disable IDE1006 // Naming Styles
      public List<AttributeField> fields { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      //TO-DO not added TextureDefinition and MaterialDefinition. This not written out
      //in any of the scene services published as of 3/11/2015.

      [DataContract]
      public enum ResourceType
      {
        [EnumMember(Value="3dNodeIndexDocument")]
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
#pragma warning disable IDE1006 // Naming Styles
      public List<HeaderDefinition> header { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "ordering")]
#pragma warning disable IDE1006 // Naming Styles
      public List<string> ordering { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "vertexAttributes")]
#pragma warning disable IDE1006 // Naming Styles
      public List<VertexAttribute> vertexAttributes { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "faces")]
#pragma warning disable IDE1006 // Naming Styles
      public List<Faces> faces { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "featureAttributeOrder")]
#pragma warning disable IDE1006 // Naming Styles
      public List<string> featureAttributeOrder { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name="featureAttributes")]
#pragma warning disable IDE1006 // Naming Styles
      public FeatureAttributes featureAttributes {get;set;}
#pragma warning restore IDE1006 // Naming Styles

      #region sub objects
      [DataContract]
      public class HeaderDefinition
      {
        [DataMember(Name = "property")]
#pragma warning disable IDE1006 // Naming Styles
        public string property { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
        public numericDataType type { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }

      [DataContract]
      public class VertexAttribute : GeometryAttribute { }

      [DataContract]
      public class Faces : GeometryAttribute { }

      [DataContract]
      public class FeatureAttributes
      {
        [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
        public FeatureAttributeId id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "faceRange")]
#pragma warning disable IDE1006 // Naming Styles
        public FaceRange faceRange { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public class FeatureAttributeId : GeometryAttribute.CommonGeometryAttributes { }

        public class FaceRange : GeometryAttribute.CommonGeometryAttributes { }
      }

      public class GeometryAttribute
      {
        [DataMember(Name = "position")]
#pragma warning disable IDE1006 // Naming Styles
        public Position position { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "normal")]
#pragma warning disable IDE1006 // Naming Styles
        public Normal normal { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "uv0")]
#pragma warning disable IDE1006 // Naming Styles
        public Uv0 uv0 { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "color")]
#pragma warning disable IDE1006 // Naming Styles
        public Color color { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public class CommonGeometryAttributes
        {
          [DataMember(Name = "valueType")]
#pragma warning disable IDE1006 // Naming Styles
          public numericDataType valueType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "valuesPerElement")]
#pragma warning disable IDE1006 // Naming Styles
          public int valuesPerElement { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
      public FieldType type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "alias")]
#pragma warning disable IDE1006 // Naming Styles
      public string alias { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
        
#pragma warning disable IDE1006 // Naming Styles
    public enum numericDataType
#pragma warning restore IDE1006 // Naming Styles
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
    [DataMember(Name="title")]
#pragma warning disable IDE1006 // Naming Styles
    public string title { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name="visibility")]
#pragma warning disable IDE1006 // Naming Styles
    public bool visibility { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="opacity")]
#pragma warning disable IDE1006 // Naming Styles
    public double opacity { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="url")]
#pragma warning disable IDE1006 // Naming Styles
    public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="layerType")]
#pragma warning disable IDE1006 // Naming Styles
    public string layerType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<WebSceneServiceLayer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  /// <summary>
  /// Elevation layer within a basemap layer for a webscene
  /// </summary>
  [DataContract]
  public class ElevationLayer
  {
    [DataMember(Name="url")]
#pragma warning disable IDE1006 // Naming Styles
    public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="id")]
#pragma warning disable IDE1006 // Naming Styles
    public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="layerType")]
#pragma warning disable IDE1006 // Naming Styles
    public string layerType { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  /// <summary>
  /// Root class when deserializing webscene item's data REST endpoint.
  /// </summary>
  [DataContract]
  public class WebSceneLayerInfo
  {
    [DataMember(Name="operationalLayers")]
#pragma warning disable IDE1006 // Naming Styles
    public List<WebSceneServiceLayer> operationalLayers { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="baseMap")]
#pragma warning disable IDE1006 // Naming Styles
    public BaseMap baseMap { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="version")]
#pragma warning disable IDE1006 // Naming Styles
    public string version { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="authoringApp")]
#pragma warning disable IDE1006 // Naming Styles
    public string authoringApp { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    
    [DataMember(Name="authoringAppVersion")]
#pragma warning disable IDE1006 // Naming Styles
    public string authoringAppVersion { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
    public List<WebMapServiceLayer> operationalLayers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "baseMap")]
#pragma warning disable IDE1006 // Naming Styles
    public BaseMap baseMap { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
    public SpatialReference spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "bookmarks")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Bookmark> bookmarks { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "widgets")]
#pragma warning disable IDE1006 // Naming Styles
    public List<Widget> widgets { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "applicationProperties")]
#pragma warning disable IDE1006 // Naming Styles
    public ApplicationProperties applicationProperties { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "version")]
#pragma warning disable IDE1006 // Naming Styles
    public float version { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataContract]
    public class ApplicationProperties
    {
      [DataMember(Name = "viewing")]
#pragma warning disable IDE1006 // Naming Styles
      public Viewing viewing { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "editing")]
#pragma warning disable IDE1006 // Naming Styles
      public Editing editing { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataContract]
      public class Viewing
      {
        [DataMember(Name = "routing")]
#pragma warning disable IDE1006 // Naming Styles
        public Dictionary<string, bool> routing { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "measure")]
#pragma warning disable IDE1006 // Naming Styles
        public Dictionary<string, bool> measure { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "basemapGallery")]
#pragma warning disable IDE1006 // Naming Styles
        public Dictionary<string, bool> basemapGallery { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "search")]
#pragma warning disable IDE1006 // Naming Styles
        public Search search { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataContract]
        public class Search
        {
          [DataMember(Name = "enabled")]
#pragma warning disable IDE1006 // Naming Styles
          public bool enabled { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "disablePlaceFinder")]
#pragma warning disable IDE1006 // Naming Styles
          public bool disablePlaceFinder { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "hintText")]
#pragma warning disable IDE1006 // Naming Styles
          public string hintText { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
          public List<Layer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
      }

      [DataContract]
      public class Editing
      {
        [DataMember(Name = "locationTracking")]
#pragma warning disable IDE1006 // Naming Styles
        public LocationTracking locationTracking { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataContract]
        public class LocationTracking
        {
          [DataMember(Name = "enabled")]
#pragma warning disable IDE1006 // Naming Styles
          public bool enabled { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "info")]
#pragma warning disable IDE1006 // Naming Styles
          public Info info { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        [DataContract]
        public class Info
        {
          [DataMember(Name = "layerId")]
#pragma warning disable IDE1006 // Naming Styles
          public string layerId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "updateInterval")]
#pragma warning disable IDE1006 // Naming Styles
          public double updateInterval { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
      public string capabilities { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "defaultVisibility")]
#pragma warning disable IDE1006 // Naming Styles
      public bool defaultVisibility { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "disablePopup")]
#pragma warning disable IDE1006 // Naming Styles
      public bool disablePopup { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "featureCollection")]
#pragma warning disable IDE1006 // Naming Styles
      public FeatureCollection featureCollection { get; set; } //yet to complete this object
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
      public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "itemId")]
#pragma warning disable IDE1006 // Naming Styles
      public string itemId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "layerDefinition")]
#pragma warning disable IDE1006 // Naming Styles
      public LayerDefinition layerDefinition { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
      public List<Layer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "layerType")]
#pragma warning disable IDE1006 // Naming Styles
      public string layerType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "maxScale")]
#pragma warning disable IDE1006 // Naming Styles
      public float maxScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "minScale")]
#pragma warning disable IDE1006 // Naming Styles
      public float minScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "mode")]
#pragma warning disable IDE1006 // Naming Styles
      public long mode { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "opacity")]
#pragma warning disable IDE1006 // Naming Styles
      public double opacity { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "parentLayerId")]
#pragma warning disable IDE1006 // Naming Styles
      public long parentLayerId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "popupInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public PopupInfo popupInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "refreshInterval")]
#pragma warning disable IDE1006 // Naming Styles
      public float refreshInterval { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "showLabels")]
#pragma warning disable IDE1006 // Naming Styles
      public bool showLabels { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "showLegend")]
#pragma warning disable IDE1006 // Naming Styles
      public bool showLegend { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "subLayerIds")]
#pragma warning disable IDE1006 // Naming Styles
      public List<long> subLayerIDs { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "timeAnimation")]
#pragma warning disable IDE1006 // Naming Styles
      public bool timeAnimation { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "title")]
#pragma warning disable IDE1006 // Naming Styles
      public string title { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "token")]
#pragma warning disable IDE1006 // Naming Styles
      public string secureServiceToken { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
      public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "url")]
#pragma warning disable IDE1006 // Naming Styles
      public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "visibility")]
#pragma warning disable IDE1006 // Naming Styles
      public bool visibility { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "visibleFolders")]
#pragma warning disable IDE1006 // Naming Styles
      public List<long> visibleFolders { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      //fields for services from CSV files
      [DataMember(Name = "columnDelimiter")]
#pragma warning disable IDE1006 // Naming Styles
      public string columnDelimiter { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "locationInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public LocationInfo locationInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      //fields for WMS layer types
      [DataMember(Name = "copyright")]
#pragma warning disable IDE1006 // Naming Styles
      public string copyright { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "extent")]
#pragma warning disable IDE1006 // Naming Styles
      public Extent extent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "format")]
#pragma warning disable IDE1006 // Naming Styles
      public string format { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "legendUrl")]
#pragma warning disable IDE1006 // Naming Styles
      public string legendUrl { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "maxHeight")]
#pragma warning disable IDE1006 // Naming Styles
      public double maxHeight { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "maxWidth")]
#pragma warning disable IDE1006 // Naming Styles
      public double maxWidth { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "mapUrl")]
#pragma warning disable IDE1006 // Naming Styles
      public string mapUrl { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "spatialReferences")]
      public List<int> SRwkids { get; set; }

      [DataMember(Name = "version")]
#pragma warning disable IDE1006 // Naming Styles
      public string version { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      //fields for image service layer
      [DataMember(Name = "bandIds")]
#pragma warning disable IDE1006 // Naming Styles
      public List<int> bandIds { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "compressionQuality")]
#pragma warning disable IDE1006 // Naming Styles
      public int compressionQuality { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "interpolation")]
#pragma warning disable IDE1006 // Naming Styles
      public string interpolation { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "noData")]
#pragma warning disable IDE1006 // Naming Styles
      public string noData { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "mosaicRule")]
#pragma warning disable IDE1006 // Naming Styles
      public string mosaicRule { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "noDataInterpretation")]
#pragma warning disable IDE1006 // Naming Styles
      public string noDataInterpretation { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "pixelType")]
#pragma warning disable IDE1006 // Naming Styles
      public string pixelType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "renderingRule")]
#pragma warning disable IDE1006 // Naming Styles
      public RenderingRule renderingRule { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    [DataContract]
    public class LayerDefinition
    {
      [DataMember(Name = "definitionExpression")]
#pragma warning disable IDE1006 // Naming Styles
      public string definitionExpression { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
      public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
      public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "displayField")]
#pragma warning disable IDE1006 // Naming Styles
      public string displayField { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "definitionEditor")]
#pragma warning disable IDE1006 // Naming Styles
      public DefinitionEditor definitionEditor { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "source")]
#pragma warning disable IDE1006 // Naming Styles
      public Source source { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "extent")]
#pragma warning disable IDE1006 // Naming Styles
      public Extent extent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
      public SpatialReference spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "drawingInfo")]
#pragma warning disable IDE1006 // Naming Styles
      public DrawingInfo drawingInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "hasAttachments")]
#pragma warning disable IDE1006 // Naming Styles
      public bool hasAttachments { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "objectIdField")]
#pragma warning disable IDE1006 // Naming Styles
      public string objectIdField { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "typeIdField")]
#pragma warning disable IDE1006 // Naming Styles
      public string typeIdField { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "fields")]
#pragma warning disable IDE1006 // Naming Styles
      public List<Field> fields { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "types")]
#pragma warning disable IDE1006 // Naming Styles
      public List<TypeIdType> types { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "minScale")]
#pragma warning disable IDE1006 // Naming Styles
      public float minScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "maxScale")]
#pragma warning disable IDE1006 // Naming Styles
      public float maxScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      //For feature collection and CSV types
      [DataMember(Name = "geometryType")]
#pragma warning disable IDE1006 // Naming Styles
      public string geometryType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "templates")]
#pragma warning disable IDE1006 // Naming Styles
      public List<TypeIdType.Template> templates { set; get; }
#pragma warning restore IDE1006 // Naming Styles
      
      #region subObjects
      [DataContract]
      public class DefinitionEditor
      {
        [DataMember(Name = "parameterizedExpression")]
#pragma warning disable IDE1006 // Naming Styles
        public string parameterizedExpression { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "inputs")]
#pragma warning disable IDE1006 // Naming Styles
        public List<Input> inputs { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }

      [DataContract]
      public class Input
      {
        [DataMember(Name = "hint")]
#pragma warning disable IDE1006 // Naming Styles
        public string hint { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "prompt")]
#pragma warning disable IDE1006 // Naming Styles
        public string prompt { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "parameters")]
#pragma warning disable IDE1006 // Naming Styles
        public List<Parameter> parameters { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }

      [DataContract]
      public class Parameter
      {
        [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "fieldName")]
#pragma warning disable IDE1006 // Naming Styles
        public string fieldName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "parameterId")]
#pragma warning disable IDE1006 // Naming Styles
        public int parameterId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "defaultValue")]
#pragma warning disable IDE1006 // Naming Styles
        public string defaultValue { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }

      [DataContract]
      public class Source
      {
        [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "mapLayerId")]
#pragma warning disable IDE1006 // Naming Styles
        public string mapLayerId { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "gdbVersion")]
#pragma warning disable IDE1006 // Naming Styles
        public string gdbVersion { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }

      [DataContract]
      public class DrawingInfo
      {
        [DataMember(Name = "renderer")]
#pragma warning disable IDE1006 // Naming Styles
        public Renderer renderer { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "transparency")]
#pragma warning disable IDE1006 // Naming Styles
        public int transparency {get; set;}
#pragma warning restore IDE1006 // Naming Styles

        [DataMember (Name = "labelingInfo")]
#pragma warning disable IDE1006 // Naming Styles
        public LabelingInfo labelingInfo {get; set;}
#pragma warning restore IDE1006 // Naming Styles

        [DataMember (Name = "fixedSymbols")]
#pragma warning disable IDE1006 // Naming Styles
        public bool fixedSymbols { get; set;}
#pragma warning restore IDE1006 // Naming Styles
      }

      [DataContract]
      public class Renderer
      {
        //Base and simple renderer
        [DataMember(Name = "type")]
        public string RenderType { get; set; }

        [DataMember(Name = "label")]
#pragma warning disable IDE1006 // Naming Styles
        public string label { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
        public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "symbol")]
#pragma warning disable IDE1006 // Naming Styles
        public Symbol symbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "rotationType")]
#pragma warning disable IDE1006 // Naming Styles
        public string rotationType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "rotationExpression")]
#pragma warning disable IDE1006 // Naming Styles
        public string rotationExpression { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        //Unique value renderer
        [DataMember(Name = "field1")]
#pragma warning disable IDE1006 // Naming Styles
        public string field1 { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "field2")]
#pragma warning disable IDE1006 // Naming Styles
        public string field2 { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "field3")]
#pragma warning disable IDE1006 // Naming Styles
        public string field3 { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "fieldDelimiter")]
#pragma warning disable IDE1006 // Naming Styles
        public string fieldDelimiter { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "defaultSymbol")]
#pragma warning disable IDE1006 // Naming Styles
        public Symbol defaultSymbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "uniqueValueInfo")]
#pragma warning disable IDE1006 // Naming Styles
        public List<UniqueValueInfo> uniqueValueInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        //Class break renderer
        [DataMember(Name = "field")]
#pragma warning disable IDE1006 // Naming Styles
        public string field { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "classificationMethod")]
#pragma warning disable IDE1006 // Naming Styles
        public string classificationMethod { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "normalizationType")]
#pragma warning disable IDE1006 // Naming Styles
        public string normalizationType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "normalizationField")]
#pragma warning disable IDE1006 // Naming Styles
        public string normalizationField { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "normalizationTotal")]
#pragma warning disable IDE1006 // Naming Styles
        public string normalizationTotal { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "backgroundFillSymbol")]
#pragma warning disable IDE1006 // Naming Styles
        public Symbol backgroundFillSymbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "minValue")]
#pragma warning disable IDE1006 // Naming Styles
        public float minValue { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "classBreakInfos")]
#pragma warning disable IDE1006 // Naming Styles
        public List<ClassBreakInfo> classBreakInfos { get; set; }
#pragma warning restore IDE1006 // Naming Styles

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
#pragma warning disable IDE1006 // Naming Styles
          public float size { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "angle")]
#pragma warning disable IDE1006 // Naming Styles
          public float angle { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "xoffset")]
#pragma warning disable IDE1006 // Naming Styles
          public float xOffset { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "yoffset")]
#pragma warning disable IDE1006 // Naming Styles
          public float yOffset { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          //[DataMember(Name = "outline")]
          //public Outline outline { get; set; }

          [DataMember(Name = "url")]
#pragma warning disable IDE1006 // Naming Styles
          public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "imageData")]
#pragma warning disable IDE1006 // Naming Styles
          public string imageData { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "contentType")]
#pragma warning disable IDE1006 // Naming Styles
          public string contentType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          //[DataMember(Name = "width")]
          //public float width { get; set; }

          [DataMember(Name = "height")]
#pragma warning disable IDE1006 // Naming Styles
          public float height { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "xscale")]
#pragma warning disable IDE1006 // Naming Styles
          public float xScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "yscale")]
#pragma warning disable IDE1006 // Naming Styles
          public float yScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "backgroundColor")]
#pragma warning disable IDE1006 // Naming Styles
          public int[] backgroundColor { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "borderLineSize")]
#pragma warning disable IDE1006 // Naming Styles
          public float borderLineSize { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "borderLineColor")]
#pragma warning disable IDE1006 // Naming Styles
          public int[] borderLineColor { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "font")]
#pragma warning disable IDE1006 // Naming Styles
          public Font font { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "haloColor")]
#pragma warning disable IDE1006 // Naming Styles
          public int[] haloColor { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "haloSize")]
#pragma warning disable IDE1006 // Naming Styles
          public float haloSize { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "horizontalAlignment")]
#pragma warning disable IDE1006 // Naming Styles
          public string horizontalAlignment { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "kerning")]
#pragma warning disable IDE1006 // Naming Styles
          public bool kerning { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "rightToLeft")]
#pragma warning disable IDE1006 // Naming Styles
          public bool rightToLeft { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "rotated")]
#pragma warning disable IDE1006 // Naming Styles
          public bool rotated { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "text")]
#pragma warning disable IDE1006 // Naming Styles
          public string text { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "verticalAlignment")]
#pragma warning disable IDE1006 // Naming Styles
          public string verticalAlignment { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        [DataContract]
        public class Outline
        {
          [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
          public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "style")]
#pragma warning disable IDE1006 // Naming Styles
          public string style { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "color")]
          public int[] OutlineColor { get; set; }

          [DataMember(Name = "width")]
#pragma warning disable IDE1006 // Naming Styles
          public float width { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        [DataContract]
        public class Font
        {
          [DataMember(Name = "family")]
#pragma warning disable IDE1006 // Naming Styles
          public string family { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "size")]
#pragma warning disable IDE1006 // Naming Styles
          public float size { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "style")]
#pragma warning disable IDE1006 // Naming Styles
          public string style { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "weight")]
#pragma warning disable IDE1006 // Naming Styles
          public string weight { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "decoration")]
#pragma warning disable IDE1006 // Naming Styles
          public string decoration { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        [DataContract]
        public class UniqueValueInfo
        {
          [DataMember(Name = "value")]
#pragma warning disable IDE1006 // Naming Styles
          public string value { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "label")]
#pragma warning disable IDE1006 // Naming Styles
          public string label { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
          public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "symbol")]
#pragma warning disable IDE1006 // Naming Styles
          public Symbol symbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        [DataContract]
        public class ClassBreakInfo
        {
          [DataMember(Name = "classMinValue")]
#pragma warning disable IDE1006 // Naming Styles
          public float classMinValue { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "classMaxValue")]
#pragma warning disable IDE1006 // Naming Styles
          public float classMaxValue { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "label")]
#pragma warning disable IDE1006 // Naming Styles
          public string label { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
          public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "symbol")]
#pragma warning disable IDE1006 // Naming Styles
          public Symbol symbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
        #endregion
      }

      [DataContract]
      public class LabelingInfo
      {
        [DataMember(Name = "labelExpression")]
#pragma warning disable IDE1006 // Naming Styles
        public string labelExpression { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "labelExpressionInfo")]
#pragma warning disable IDE1006 // Naming Styles
        public LabelExpressionInfo labelExpressionInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "labelPlacement")]
#pragma warning disable IDE1006 // Naming Styles
        public string labelPlacement { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "useCodedValues")]
#pragma warning disable IDE1006 // Naming Styles
        public bool useCodedValues { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "minScale")]
#pragma warning disable IDE1006 // Naming Styles
        public float minScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "maxScale")]
#pragma warning disable IDE1006 // Naming Styles
        public float maxScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "symbol")]
#pragma warning disable IDE1006 // Naming Styles
        public Renderer.Symbol symbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "where")]
#pragma warning disable IDE1006 // Naming Styles
        public string where { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        #region Sub Objects
        [DataContract]
        public class LabelExpressionInfo
        {
          [DataMember(Name = "value")]
#pragma warning disable IDE1006 // Naming Styles
          public string value { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
        #endregion
      }

      [DataContract]
      public class Field
      {
        [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
        public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "alias")]
#pragma warning disable IDE1006 // Naming Styles
        public string alias { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "editable")]
#pragma warning disable IDE1006 // Naming Styles
        public bool editable { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "nullable")]
#pragma warning disable IDE1006 // Naming Styles
        public bool nullable { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "domain")]
#pragma warning disable IDE1006 // Naming Styles
        public Domain domain { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataContract]
        public class Domain
        {
          [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
          public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
          public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "range")]
#pragma warning disable IDE1006 // Naming Styles
          public float [] range { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
      }

      [DataContract]
      public class TypeIdType
      {
        [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
        public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "domains")]
        //public Field.Domain [] domains { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Field.Domain domains { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "template")]
#pragma warning disable IDE1006 // Naming Styles
        public Template template { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataContract]
        public class Template
        {
          [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
          public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
          public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "prototype")]
#pragma warning disable IDE1006 // Naming Styles
          public Feature prototype { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "drawingTool")]
#pragma warning disable IDE1006 // Naming Styles
          public string drawingTool { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
      }

      [DataContract]
      public class Feature
      {
        [DataMember(Name = "attributes")]
#pragma warning disable IDE1006 // Naming Styles
        public object attributes { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "geometry")]
#pragma warning disable IDE1006 // Naming Styles
        public object geometry { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "symbol")]
#pragma warning disable IDE1006 // Naming Styles
        public Renderer.Symbol symbol { get; set; }
#pragma warning restore IDE1006 // Naming Styles
      }
      #endregion
    }

    [DataContract]
    public class FeatureCollection 
    {
      [DataMember(Name = "layers")]
#pragma warning disable IDE1006 // Naming Styles
      public List<Layer> layers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "showLegend")]
#pragma warning disable IDE1006 // Naming Styles
      public bool showLegend { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    [DataContract]
    public class LocationInfo
    {
      [DataMember(Name = "locationType")]
#pragma warning disable IDE1006 // Naming Styles
      public string locationType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "latitudeFieldName")]
#pragma warning disable IDE1006 // Naming Styles
      public string latitudeFieldName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "longitudeFieldName")]
#pragma warning disable IDE1006 // Naming Styles
      public string longitudeFieldName { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    [DataContract]
    public class RenderingRule
    {
      [DataMember(Name = "rasterFunction")]
#pragma warning disable IDE1006 // Naming Styles
      public string rasterFunction { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
    #endregion

    #region BasemapLayers
    [DataContract]
    public class BaseMap
    {
      [DataMember(Name = "baseMapLayers")]
#pragma warning disable IDE1006 // Naming Styles
      public List<BaseMapLayer> baseMapLayers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "title")]
#pragma warning disable IDE1006 // Naming Styles
      public string title { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      //For basemaps of webscenes
      [DataMember(Name = "elevationLayers")]
#pragma warning disable IDE1006 // Naming Styles
      public List<ElevationLayer> elevationLayers { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataContract]
      public class BaseMapLayer
      {
        [DataMember(Name = "id")]
#pragma warning disable IDE1006 // Naming Styles
        public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "maxScale")]
#pragma warning disable IDE1006 // Naming Styles
        public float maxScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "minScale")]
#pragma warning disable IDE1006 // Naming Styles
        public float minScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "isReference")]
#pragma warning disable IDE1006 // Naming Styles
        public bool isReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "opacity")]
#pragma warning disable IDE1006 // Naming Styles
        public double opacity { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "portalUrl")]
#pragma warning disable IDE1006 // Naming Styles
        public string portalUrl { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "showLegend")]
#pragma warning disable IDE1006 // Naming Styles
        public bool showLegend { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "layerType")]
#pragma warning disable IDE1006 // Naming Styles
        public string layerType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "url")]
#pragma warning disable IDE1006 // Naming Styles
        public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "visibility")]
#pragma warning disable IDE1006 // Naming Styles
        public bool visibility { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [DataMember(Name = "exclusionAreas")]
#pragma warning disable IDE1006 // Naming Styles
        public List<ExclusionArea> exclusionAreas { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public class ExclusionArea
        {
          [DataMember(Name = "minZoom")]
#pragma warning disable IDE1006 // Naming Styles
          public int minZoom { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "maxZoom")]
#pragma warning disable IDE1006 // Naming Styles
          public int maxZoom { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "maxScale")]
#pragma warning disable IDE1006 // Naming Styles
          public float maxScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "minScale")]
#pragma warning disable IDE1006 // Naming Styles
          public float minScale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

          [DataMember(Name = "geometry")]
#pragma warning disable IDE1006 // Naming Styles
          public Extent geometry { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
      }
    }
    #endregion

  [DataContract]
  public class Bookmark
  {
    [DataMember(Name = "extent")]
#pragma warning disable IDE1006 // Naming Styles
    public Extent extent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
    public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
    public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "owner")]
#pragma warning disable IDE1006 // Naming Styles
    public string owner { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "created")]
#pragma warning disable IDE1006 // Naming Styles
    public long created { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "modified")]
#pragma warning disable IDE1006 // Naming Styles
    public long modified { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "guid")]
#pragma warning disable IDE1006 // Naming Styles
    public string guid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "name")]
#pragma warning disable IDE1006 // Naming Styles
    public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "title")]
#pragma warning disable IDE1006 // Naming Styles
    public string title { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "type")]
#pragma warning disable IDE1006 // Naming Styles
    public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "typeKeywords")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> typeKeywords { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "description")]
#pragma warning disable IDE1006 // Naming Styles
    public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "tags")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> tags { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "snippet")]
#pragma warning disable IDE1006 // Naming Styles
    public string snippet { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "thumbnail")]
#pragma warning disable IDE1006 // Naming Styles
    public string thumbnail { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "documentation")]
#pragma warning disable IDE1006 // Naming Styles
    public string documentation { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "extent")]
#pragma warning disable IDE1006 // Naming Styles
    public List<List<double>> extent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "spatialReference")]
#pragma warning disable IDE1006 // Naming Styles
    public string spatialReference { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "accessInformation")]
#pragma warning disable IDE1006 // Naming Styles
    public string accessInformation { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "licenseInfo")]
#pragma warning disable IDE1006 // Naming Styles
    public string licenseInfo { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "culture")]
#pragma warning disable IDE1006 // Naming Styles
    public string culture { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "properties")]
#pragma warning disable IDE1006 // Naming Styles
    public string properties { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "url")]
#pragma warning disable IDE1006 // Naming Styles
    public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "access")]
#pragma warning disable IDE1006 // Naming Styles
    public string access { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "size")]
#pragma warning disable IDE1006 // Naming Styles
    public long size { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "appCategories")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> appCategories { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "industries")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> industries { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "languages")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> languages { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "largeThumbnail")]
#pragma warning disable IDE1006 // Naming Styles
    public string largeThumbnail { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "banner")]
#pragma warning disable IDE1006 // Naming Styles
    public string banner { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "screenshots")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> screenshots { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "listed")]
#pragma warning disable IDE1006 // Naming Styles
    public bool listed { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "ownerFolder")]
#pragma warning disable IDE1006 // Naming Styles
    public string ownerFolder { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "protected")]
#pragma warning disable IDE1006 // Naming Styles
    public bool @protected { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "numComments")]
#pragma warning disable IDE1006 // Naming Styles
    public long numComments { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "numRatings")]
#pragma warning disable IDE1006 // Naming Styles
    public long numRatings { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "avgRating")]
#pragma warning disable IDE1006 // Naming Styles
    public double avgRating { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "numViews")]
#pragma warning disable IDE1006 // Naming Styles
    public long numViews { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  [DataContract]
  public class Sharing
  {
    [DataMember(Name = "access")]
#pragma warning disable IDE1006 // Naming Styles
    public string access { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "groups")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> groups { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  /// <summary>
  /// DataContract to deserialize any online item. Can be used for packages, services, webmaps, webscenes etc.
  /// For webmaps, webscenes, to know the contents, use WebMapLayerInfo, WebSceneLayerInfo classes.
  /// </summary>
  [DataContract]
  public class OnlineItem
  {
    [DataMember(Name = "item")]
#pragma warning disable IDE1006 // Naming Styles
    public SDCItem item { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "sharing")]
#pragma warning disable IDE1006 // Naming Styles
    public Sharing sharing { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region Search result DataContracts
  [DataContract]
  public class SearchResult
  {
    [DataMember(Name = "query")]
#pragma warning disable IDE1006 // Naming Styles
    public string query { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "total")]
#pragma warning disable IDE1006 // Naming Styles
    public int total { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "start")]
#pragma warning disable IDE1006 // Naming Styles
    public int start { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "num")]
#pragma warning disable IDE1006 // Naming Styles
    public int num { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "nextStart")]
#pragma warning disable IDE1006 // Naming Styles
    public int nextStart { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "results")]
#pragma warning disable IDE1006 // Naming Styles
    public List<SharingDataContracts.SDCItem> results { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  //[DataContract]
  //public class Result: SharingDataContracts.Item
  //{

  //}
  #endregion

  #region esriTransportTypeUrl DataContracts
  [DataContract]
#pragma warning disable IDE1006 // Naming Styles
  public class esriTransportTypeUrl
#pragma warning restore IDE1006 // Naming Styles
  {
      [DataMember(Name = "transportType")]
#pragma warning disable IDE1006 // Naming Styles
      public string transportType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "responseType")]
#pragma warning disable IDE1006 // Naming Styles
      public string responseType { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "URL")]
      public string URL { get; set; }
  }
  #endregion

  #region Replica DataContracts
  [DataContract]
  public class Replica
  {
      [DataMember(Name = "replicaName")]
#pragma warning disable IDE1006 // Naming Styles
      public string replicaName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "replicaID")]
#pragma warning disable IDE1006 // Naming Styles
      public string replicaID { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion

  #region ExportMap Response DataContracts
  [DataContract]
  public class MapResponse
  {
      [DataMember(Name = "href")]
#pragma warning disable IDE1006 // Naming Styles
      public string href { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "width")]
#pragma warning disable IDE1006 // Naming Styles
      public int width { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "height")]
#pragma warning disable IDE1006 // Naming Styles
      public int height { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "extent")]
#pragma warning disable IDE1006 // Naming Styles
      public InitialExtent extent { get; set; }
#pragma warning restore IDE1006 // Naming Styles

      [DataMember(Name = "scale")]
#pragma warning disable IDE1006 // Naming Styles
      public double scale { get; set; }
#pragma warning restore IDE1006 // Naming Styles

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
#pragma warning disable IDE1006 // Naming Styles
    public string query { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "total")]
#pragma warning disable IDE1006 // Naming Styles
    public int total { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "start")]
#pragma warning disable IDE1006 // Naming Styles
    public int start { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "num")]
#pragma warning disable IDE1006 // Naming Styles
    public int num { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "nextStart")]
#pragma warning disable IDE1006 // Naming Styles
    public int nextStart { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "results")]
#pragma warning disable IDE1006 // Naming Styles
    public List<SearchGroupItem> results { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }

  /// <summary>
  /// SearchGroupItem contails all of UserGroups object and more KVPs
  /// </summary>
  [DataContract]
  public class SearchGroupItem : UserGroups
  {
    [DataMember(Name = "tags")]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> tags { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "phone")]
#pragma warning disable IDE1006 // Naming Styles
    public object phone { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "sortField")]
#pragma warning disable IDE1006 // Naming Styles
    public string sortField { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "sortOrder")]
#pragma warning disable IDE1006 // Naming Styles
    public string sortOrder { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "isFav")]
#pragma warning disable IDE1006 // Naming Styles
    public bool isFav { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "created")]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
#pragma warning disable IDE1006 // Naming Styles
    public long created { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    [DataMember(Name = "modified")]
#pragma warning disable IDE1006 // Naming Styles
    public long modified { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "provider")]
#pragma warning disable IDE1006 // Naming Styles
    public object provider { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "providerGroupName")]
#pragma warning disable IDE1006 // Naming Styles
    public object providerGroupName { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "isReadOnly")]
#pragma warning disable IDE1006 // Naming Styles
    public bool isReadOnly { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember(Name = "access")]
#pragma warning disable IDE1006 // Naming Styles
    public string access { get; set; }
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
    public List<int> intList { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember]
#pragma warning disable IDE1006 // Naming Styles
    public List<string> stringList { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember]
#pragma warning disable IDE1006 // Naming Styles
    public ContainedClass cClass { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember]
#pragma warning disable IDE1006 // Naming Styles
    public int[] intArray { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember]
#pragma warning disable IDE1006 // Naming Styles
    public string[] stringArray { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataContract]
    public class ContainedClass
    {
      [DataMember]
      public int Integer { get; set; }

      [DataMember]
#pragma warning disable IDE1006 // Naming Styles
      public List<string> stringList { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
  }

  [DataContract]
  public class RecursiveCollections
  {
    [DataMember]
#pragma warning disable IDE1006 // Naming Styles
    public List<List<int>> twoLevelIntList { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    [DataMember]
#pragma warning disable IDE1006 // Naming Styles
    public List<AllDataTypes.ContainedClass> classList { get; set; }
#pragma warning restore IDE1006 // Naming Styles
  }
  #endregion
}
