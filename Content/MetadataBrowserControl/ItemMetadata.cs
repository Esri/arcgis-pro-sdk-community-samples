/*
   Copyright 2019 Esri
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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace MetadataBrowserControl
{
    /// <summary>
    /// Represents the metadata of the Item selected
    /// </summary>
    public class ItemMetadata : INotifyPropertyChanged
    {
        private Item _item = null;
        private string _xml = "";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes the ItemMetadata
        /// </summary>
        /// <param name="item"></param>
        /// <param name="xml"></param>
        public ItemMetadata(Item item, string xml)
        {
            Init(item, xml);
        }

        private void Init(Item item, string xml)
        {
            _item = item;
            _xml = xml;
        }

        public string XML => _xml;

        /// <summary>
        /// Obtains the XML of the project item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static Task<string> GetXML(Item item)
        {
            return QueuedTask.Run<string>(() =>
            {
                return item.GetXml() ?? "<metadata>no metadata</metadata>";
            });
        }

        /// <summary>
        /// Applies the transform to view the metadata in a browser control
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xsltPath"></param>
        /// <returns></returns>
        public static string TransformXML(string xml, string xsltPath)
        {
            if (string.IsNullOrEmpty(xml))
                return "";
            if (!File.Exists(xsltPath))
                return "";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            XslCompiledTransform xslt = new XslCompiledTransform(true);
            StringWriter writer = new StringWriter();
            xslt.Load(xsltPath);
            xslt.Transform(doc.CreateNavigator(), null, writer);
            return writer.ToString();
        }
    }
}
