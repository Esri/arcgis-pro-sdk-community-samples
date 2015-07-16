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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ArcGIS.Desktop.Framework.Contracts;

namespace InsertIntoContextMenu
{
    public class CIMViewerViewModel : ViewModelBase
    {
        private string _xml;
        public string Xml
        {
            get { return FormatXml(_xml); }
            set { SetProperty(ref _xml, value, () => Xml); }
        }

        private string FormatXml(string xml)
        {

            var stringBuilder = new StringBuilder();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                var xmlWriterSettings = new XmlWriterSettings {Indent = true, OmitXmlDeclaration = true};
                doc.Save(XmlWriter.Create(stringBuilder, xmlWriterSettings));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: {0}", ex.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
