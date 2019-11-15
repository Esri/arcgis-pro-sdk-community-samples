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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace DuplicateLayout
{
    internal class Clone : Button
    {
        protected async override void OnClick()
        {
            var catalogPane = Project.GetCatalogPane();
            var items = catalogPane.SelectedItems;
            var layoutItems = items.OfType<LayoutProjectItem>();
            foreach (var item in layoutItems)
            {
                LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(itm => itm.Name.Equals(item.Name));
                try
                {
                    var layout = await layoutItem.GetLayoutAsync();
                    if (layout != null)
                    {
                        await layout.Duplicate();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
    }

    public static class LayoutExtensions
    {
        /// <summary>
        /// Duplicate a layout - run from GUi Thread
        /// </summary>
        /// <param name="layout">Layout to duplicated</param>
        /// <returns></returns>
        public static async Task Duplicate(this Layout layout)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout), "layout cannot be null");
            var layout_clone = await layout.CloneAsync();
            await ProApp.Panes.CreateLayoutPaneAsync(layout_clone);
        }

        public static Task<Layout> CloneAsync(this Layout layout)
        {
            return QueuedTask.Run(() => {
                return Clone(layout);
            });
        }

        public static Task<Layout> GetLayoutAsync(this LayoutProjectItem layout)
        {
            return QueuedTask.Run(() => {
                return layout.GetLayout();
            });
        }

        //Must be called on the QueuedTask
        public static Layout Clone(this Layout layout)
        {
            var layout_dup = LayoutFactory.Instance.CreateLayout();
            var metadata_uri = layout_dup.GetDefinition().MetadataURI;

            var layout_def = layout.GetDefinition();
            var layout_def_clone = layout_def.Clone() as CIMLayout;
            layout_def_clone.URI = layout_dup.URI;
            layout_def_clone.MetadataURI = metadata_uri;

            layout_dup.SetDefinition(layout_def_clone);
            return layout_dup;
        }
    }

    public static class CIMExtensions
    {
        public static CIMObject Clone(this CIMObject cimObject)
        {
            var clone = System.Activator.CreateInstance("ArcGIS.Core", cimObject.GetType().ToString()).Unwrap() as CIMObject;
            var stringReader = new StringReader(cimObject.ToXml());
            var xmlReader = new XmlTextReader(stringReader);
            //xmlReader.MoveToContent();
            clone.ReadXml(xmlReader);
            xmlReader.Dispose();
            return clone;
        }
    }
}
