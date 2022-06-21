/*
   Copyright 2019 Esri
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       https://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace MetadataViewer
{
    /// <summary>
    /// Represents the metadata of the Item selected
    /// </summary>
    public class ItemInfo : INotifyPropertyChanged
    {
        private Item _item = null;
        private string _xml = "";
        private ImageSource _bmp = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="xml"></param>
        public ItemInfo(Item item, string xml)
        {
            Init(item, xml);
        }
        /// <summary>
        /// Intializes the ItemInfo
        /// </summary>
        /// <param name="item"></param>
        /// <param name="xml"></param>
        private void Init(Item item, string xml)
        {
            _item = item;
            _xml = FormatXml(xml);
            _bmp = ProcessBitmap(xml);
        }

        public Item ProPrjItem => _item;
        public string Name => _item?.Name ?? "No name";

        public string Type => _item?.Type ?? "No type";

        public string Location => _item?.Path ?? "No path";

        public string Tags => _item?.Tags ?? "No tags";

        public string XML => _xml;

        public ImageSource Xml_image => _bmp;

        internal static Task<string> GetXML(Item item)
        {
            return QueuedTask.Run<string>(() =>
            {
                return item.GetXml() ?? "<metadata>no metadata</metadata>";
            });
        }

        private static string FormatXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return "";
            var doc = new XmlDocument();
            var sb = new StringBuilder();
            try
            {
                doc.LoadXml(xml);
                var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
                doc.Save(XmlWriter.Create(sb, xmlWriterSettings));
            }
            catch (XmlException xmle)
            {
                System.Diagnostics.Debug.WriteLine("FormatXml Exception: {0}", xmle.ToString());
                sb.Append(xml);
            }
            return sb.ToString();
        }

        private static string GetThumbnailStringFromXml(string xml)
        {
            var thumbnailString = "";
            if (!string.IsNullOrEmpty(xml))
            {
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(xml);
                    var nodes = doc.DocumentElement.SelectNodes("//Binary");
                    if (nodes.Count > 0)
                        thumbnailString = nodes[0].InnerText;
                }
                catch (XmlException xmle)
                {
                    System.Diagnostics.Debug.WriteLine("FormatXml Exception: {0}", xmle.ToString());
                }
            }
            return thumbnailString;
        }

        private ImageSource ProcessBitmap(string xml)
        {
            var imageString = GetThumbnailStringFromXml(xml);
            ImageSource bmp = null;
            if (!string.IsNullOrEmpty(imageString))
            {
                try
                {
                    var imageByteArray = Convert.FromBase64String(imageString);
                    bmp = ByteToBitmapImage(imageByteArray);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ByteToBitmapImage Exception: {0}", e.ToString());
                }
            }
            if (bmp == null)
            {
                //string bitmapPath = @"pack://application:,,,/MetadataViewer;component/Images/DefaultThumbnail.bmp";
                string bitmapPath = @"pack://application:,,,/MetadataViewer;component/Images/DefaultImage.png";
                bmp = new BitmapImage(new Uri(bitmapPath, UriKind.Absolute));
            }
            return bmp;
        }

        private BitmapImage ByteToBitmapImage(byte[] image)
        {
            BitmapImage imageSource = new BitmapImage();
            try
            {
                using (MemoryStream ms = new MemoryStream(image))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    imageSource.BeginInit();
                    imageSource.StreamSource = ms;
                    imageSource.CacheOption = BitmapCacheOption.OnLoad;
                    imageSource.EndInit();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("imageSource Exception: {0}", e.ToString());
                throw;
            }
            return imageSource;
        }

    }
}
