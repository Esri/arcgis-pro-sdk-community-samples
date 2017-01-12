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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using Field = ArcGIS.Core.Data.Field;

namespace LayerPopups.Helpers {

    /// <summary>
    /// Enumeration used for the NormalizeField in ChartMediaInfo
    /// </summary>
    public enum RelateFieldStatistic {
        Count = 0,
        Minimum,
        Maximum,
        Sum,
        Mean,
        StandardDeviation
    }

    /// <summary>
    /// Types supported by ChartMediaInfos
    /// </summary>
    public enum ChartMediaType {
        Column = 0,
        Line,
        Pie,
        Bar
    }

    /// <summary>
    /// Definition for a layer popup. Includes the various media types that can be added to a popup
    /// </summary>
    /// <remarks>The CIM model allows for an arbitrary mix of media elements to be added to a popup
    /// but in order to be compatible with Online, the first two elements are always Text and Table (if
    /// they are present).
    /// Text that is added to the CIM should be formatted as XHTML. Some default format strings are
    /// provided in this class that mimic the ones Pro uses but feel free to experiment</remarks>
    public class PopupDefinition {

        internal static readonly string DefaultFormat =
            "<div><p><span style=\"font-weight:normal;text-decoration:none;\">{0}</span></p></div>";

        internal static readonly string DefaultTitleFormat =
            "<div><p><span style=\"color:black;font-weight:bold;text-decoration:none;\">{0}</span></p></div>";

        internal static readonly string DefaultURLFormat =
            "<a href=\"{0}\" target=\"_blank\" style=\"color:#6D6D6D;text-decoration:underline;\">{1}</a>";

        private List<BaseMediaInfo> _mediaInfos = new List<BaseMediaInfo>();
        private string _title = "";

        /// <summary>
        /// Popup title. Shows along the popup window frame (not in the popup content itself).
        /// </summary>
        public string Title {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// Text media. Always shows first if it has been added
        /// </summary>
        public TextMediaInfo TextMediaInfo { get; set; }

        /// <summary>
        /// Table media. Always shows first or second - second if a
        /// Text media has been added to the popup
        /// </summary>
        public TableMediaInfo TableMediaInfo { get; set; }

        /// <summary>
        /// A mix of charts, images, attachements - in any order. These
        /// are added to the popup carousel ( with "next" and "previous" navigation)
        /// </summary>
        /// <remarks>If a text or table media is added it is ignored</remarks>
        public IList<BaseMediaInfo> OtherMediaInfos => _mediaInfos;

        public static string FormatTitle(string title) {
            return String.Format(DefaultTitleFormat, title);
        }

        /// <summary>
        /// Format a caption for a popup
        /// </summary>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static string FormatCaption(string caption) {
            return String.Format(DefaultTitleFormat, caption);
        }
        /// <summary>
        /// Format a plain text string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatText(string text) {
            return String.Format(DefaultFormat, text);
        }

        /// <summary>
        /// Format a string as a field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string FormatFieldName(string fieldName) {
            return String.Format("{0}{1}{2}", "{", fieldName, "}");
        }

        /// <summary>
        /// Format a URI as a string for the popup
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string FormatUrl(Uri uri) {
            return uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString;
        }

        /// <summary>
        /// Field names can also be added as URLs to the popup (eg for an image media)
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string FormatUrl(string fieldName) {
            return FormatFieldName(fieldName);
        }

        /// <summary>
        /// When adding charts, related fields can be used either as values for the
        /// ChartMedia NormalizeField property. Specify the name of the relate (as read from the
        /// GDB), the name of the field in the related table, and the statistic to use when
        /// Normalizing.
        /// </summary>
        /// <param name="relateName"></param>
        /// <param name="fieldName"></param>
        /// <param name="statistic"></param>
        /// <returns></returns>
        public static string FormatRelatedFieldName(string relateName, string fieldName, RelateFieldStatistic statistic) {
            string format = "{0}\\{1}:";
            switch (statistic) {
                case RelateFieldStatistic.Count:
                    format += "COUNT";
                    break;
                case RelateFieldStatistic.Minimum:
                    format += "MIN";
                    break;
                case RelateFieldStatistic.Maximum:
                    format += "MAX";
                    break;
                case RelateFieldStatistic.Sum:
                    format += "SUM";
                    break;
                case RelateFieldStatistic.Mean:
                    format += "MEAN";
                    break;
                case RelateFieldStatistic.StandardDeviation:
                    format += "STDDEV";
                    break;
                default:
                    throw new ArgumentException(String.Format("{0} is not a valid statistic value", statistic));
            }
            return String.Format(format, relateName, fieldName);
        }

        /// <summary>
        /// Generate the CIMPopupInfo for the layer from the current media infos
        /// </summary>
        /// <returns></returns>
        public CIMPopupInfo CreatePopupInfo() {
            CIMPopupInfo popupInfo = new CIMPopupInfo();
            popupInfo.Title = this.Title;
            
            List<CIMMediaInfo> mediaInfos = new List<CIMMediaInfo>();
            
            if (this.TextMediaInfo != null) {
                var cimText = new CIMTextMediaInfo {
                    Row = 1,
                    Column = 1,
                    Text = this.TextMediaInfo.Text
                };
                mediaInfos.Add(cimText);
            }
            if (this.TableMediaInfo != null) {
                var cimTable = new CIMTableMediaInfo {
                    Row = mediaInfos.Count + 1,
                    Column = 1,
                    Fields = this.TableMediaInfo.FieldNames.ToArray()
                };

                mediaInfos.Add(cimTable);
            }
            foreach (var cimMediaInfo in OtherMediaInfos) {
                if (cimMediaInfo is TextMediaInfo || cimMediaInfo is TableMediaInfo)
                    continue;
                if (cimMediaInfo is AttachmentsMediaInfo) {
                    var mediaInfo = CreateAttachmentMediaInfo((AttachmentsMediaInfo)cimMediaInfo);
                    mediaInfo.Row = mediaInfos.Count + 1;
                    mediaInfo.Column = 1;
                    mediaInfos.Add(mediaInfo);
                }
                else if (cimMediaInfo is ImageMediaInfo) {
                    var mediaInfo = CreateImageMediaInfo((ImageMediaInfo)cimMediaInfo);
                    mediaInfo.Row = mediaInfos.Count + 1;
                    mediaInfo.Column = 1;
                    mediaInfos.Add(mediaInfo);
                }
                else if (cimMediaInfo is ChartMediaInfo) {
                    var mediaInfo = CreateChartMediaInfo((ChartMediaInfo)cimMediaInfo);
                    mediaInfo.Row = mediaInfos.Count + 1;
                    mediaInfo.Column = 1;
                    mediaInfos.Add(mediaInfo);
                }
             
            }
            if (mediaInfos.Count > 0) {
                popupInfo.MediaInfos = mediaInfos.ToArray();
            }
            return popupInfo;;
        }

        //Convert the AttachmentsMediaInfo to its CIM equivalent
        internal CIMAttachmentsMediaInfo CreateAttachmentMediaInfo(AttachmentsMediaInfo attachment) {
            CIMAttachmentsMediaInfo mediaInfo = new CIMAttachmentsMediaInfo();
            mediaInfo.Title = attachment.Title;
            mediaInfo.Caption = attachment.Caption;
            mediaInfo.ContentType = attachment.MimeContentType;
            mediaInfo.DisplayType = attachment.AttachmentDisplayType;
            return mediaInfo;
        }

        //Convert the ImageMediaInfo to its CIM equivalent
        internal CIMImageMediaInfo CreateImageMediaInfo(ImageMediaInfo image) {
            CIMImageMediaInfo mediaInfo = new CIMImageMediaInfo();
            mediaInfo.Title = image.Title;
            mediaInfo.Caption = image.Caption;
            mediaInfo.SourceURL = image.SourceURL;
            mediaInfo.LinkURL = image.LinkURL;
            return mediaInfo;
        }

        //Convert the ChartMediaInfo to its CIM equivalent
        internal CIMChartMediaInfo CreateChartMediaInfo(ChartMediaInfo chart) {
            CIMChartMediaInfo mediaInfo = null;
            if (chart.ChartMediaType == ChartMediaType.Column) {
                mediaInfo = new CIMColumnChartMediaInfo();
            }
            else if (chart.ChartMediaType == ChartMediaType.Bar) {
                mediaInfo = new CIMBarChartMediaInfo();
            }
            else if (chart.ChartMediaType == ChartMediaType.Line) {
                mediaInfo = new CIMLineChartMediaInfo();
            }
            else {
                mediaInfo = new CIMPieChartMediaInfo();
            }
            mediaInfo.Title = chart.Title;
            mediaInfo.Caption = chart.Caption;
            mediaInfo.NormalizeField = chart.NormalizeFieldName;
            mediaInfo.Fields = chart.FieldNames.ToArray();
            return mediaInfo;
        }
    }

    /// <summary>
    /// Base class for the MediaInfos. The MediaInfos wrap the underlying CIM definitions
    /// that the CIMPopupInfo uses
    /// </summary>
    public abstract class BaseMediaInfo {
    }

    /// <summary>
    ///  Define a Text media element for the popup
    /// </summary>
    public class TextMediaInfo : BaseMediaInfo {
        private string _formattedText = "";

        public TextMediaInfo() {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public TextMediaInfo(TextMediaInfo copy) {
            this._formattedText = copy._formattedText;
        }
        /// <summary>
        /// The actual text content to show on the popup
        /// </summary>
        public string Text
        {
            get { return _formattedText; }
            set { _formattedText = value; }
        }
    }

    /// <summary>
    /// Define a Table media element for the popup
    /// </summary>
    public class TableMediaInfo : BaseMediaInfo {

        private List<string> _fieldNames = new List<string>();

        public TableMediaInfo() {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public TableMediaInfo(TableMediaInfo copy) {
            this._fieldNames.AddRange(copy._fieldNames);
        }

        public TableMediaInfo(IEnumerable<Field> fields) : base() {
            _fieldNames.AddRange(fields.Select((f) => f.Name).ToArray());
        }

        public TableMediaInfo(IEnumerable<string> fieldNames) : base() {
            _fieldNames.AddRange(fieldNames.ToArray());
        }

        /// <summary>
        /// The list of field names to show in the Table media element
        /// </summary>
        public IList<string> FieldNames => _fieldNames;
    }

    /// <summary>
    /// Base class for all the other media types each of which have a
    /// Caption and Title field
    /// </summary>
    public abstract class BaseTitleAndCaptionMediaInfo : BaseMediaInfo {
        protected string _formattedTitle = "";
        protected string _formattedCaption = "";

        /// <summary>
        /// Title shows at the top of the media element (above the
        /// line demarcating the carousel region of the popup)
        /// </summary>
        public string Title
        {
            get { return _formattedTitle; }
            set { _formattedTitle = value; }
        }

        /// <summary>
        /// Caption shows directly above the media element on the
        /// carousel (beneath the Title and line at the top of the carousel)
        /// </summary>
        public string Caption
        {
            get { return _formattedCaption; }
            set { _formattedCaption = value; }
        }

    }

    /// <summary>
    /// If your feature class has attachments you can use an Attachment media element
    /// </summary>
    /// <remarks>You do NOT have to specify the attachment field. The popup is smart
    /// enough to know which field is the attachement field (based on the attachment
    /// relationship in the GDB).</remarks>
    public class AttachmentsMediaInfo : BaseTitleAndCaptionMediaInfo {
        
        
        private string _contentType = "";

        private AttachmentDisplayType _displayType = AttachmentDisplayType.PreviewAll;

        public AttachmentsMediaInfo() {
        }

        /// <summary>
        /// Copyy constructor
        /// </summary>
        /// <param name="copy"></param>
        public AttachmentsMediaInfo(AttachmentsMediaInfo copy) {
            this._formattedTitle = copy._formattedTitle;
            this._formattedCaption = copy._formattedCaption;
            this._contentType = copy._contentType;
            this._displayType = copy._displayType;
        }

        //Must be one of these: http://www.iana.org/assignments/media-types/media-types.xhtml
        public string MimeContentType {
            get { return _contentType; }
            set { _contentType = value; }
        }

        public AttachmentDisplayType AttachmentDisplayType {
            get { return _displayType;}
            set { _displayType = value;}
        }
    }

    /// <summary>
    /// Display an image or a link to an image
    /// </summary>
    public class ImageMediaInfo : BaseTitleAndCaptionMediaInfo {
        private string _sourceUri;
        private string _linkUri;

        public ImageMediaInfo() {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public ImageMediaInfo(ImageMediaInfo copy) {
            this._formattedTitle = copy._formattedTitle;
            this._formattedCaption = copy._formattedCaption;
            this._sourceUri = copy._sourceUri;
            this._linkUri = copy._linkUri;
        }

        /// <summary>
        /// Note: This can be a field name if it has a URL to an image as
        /// an attribute
        /// </summary>
        public string SourceURL {
            get {
                return _sourceUri;
            }
            set {
                _sourceUri = value;
            }
        }

        /// <summary>
        /// Note: This can be a field name if it has a URL to an image as
        /// an attribute
        /// </summary>
        /// <remarks>Shows a link to an image rather than an image</remarks>
        public string LinkURL {
            get {
                return _linkUri;
            }
            set {
                _linkUri = value;
            }
        }
    }

    /// <summary>
    /// Display a chart. There are four types - Column, Bar, Pie, Line
    /// </summary>
    /// <remarks>They all follow the same configuration. Specify the field or
    /// fields to plot, any normalization (in Normalize field) as well as a Title
    /// and Caption.
    /// You can also use a related field for the normalization but you must specify the
    /// name of the relate as well as the related field.</remarks>
    public class ChartMediaInfo : BaseTitleAndCaptionMediaInfo {

        private List<string> _fieldNames = new List<string>();
        private string _normalizeFieldName = "";
        private ChartMediaType _chartType = default(ChartMediaType);

        public ChartMediaInfo() {
        }

        public ChartMediaInfo(ChartMediaInfo copy) {
            if (this._fieldNames.Count > 0)
                this._fieldNames.AddRange(copy._fieldNames);
            this.Title = copy.Title;
            this.Caption = copy.Caption;
            this._normalizeFieldName = copy._normalizeFieldName;
            this._chartType = copy._chartType;
        }

        /// <summary>
        /// Pick the chart type
        /// </summary>
        public ChartMediaType ChartMediaType
        {
            get { return _chartType; }
            set { _chartType = value; }
        }

        public string NormalizeFieldName
        {
            get { return _normalizeFieldName; }
            set { _normalizeFieldName = value; }
        }

        public IList<string> FieldNames => _fieldNames;
    }
}
