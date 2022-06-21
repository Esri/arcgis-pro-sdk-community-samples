/*

   Copyright 2020 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
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
using ArcGIS.Desktop.Mapping;
using System.Windows.Markup;

namespace OpenItemDialogBrowseFilter
{
    internal class FilterGallery : Gallery
    {
        private bool _isInitialized;
        private object _smallGalleryItemTemplate;
        private object _largeGalleryItemTemplate;
        private bool _isSmallGalleryItemTemplate;
        public FilterGallery()
        {
            _isSmallGalleryItemTemplate = true;
            CacheGalleryTemplates();
            LoadGalleryTemplate(true);
            Module1.SetToggleFiltersGalleryView(() => ToggleGalleryTemplate());
            Initialize();
            this.AlwaysFireOnClick = true;
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;
            _isInitialized = true;

            //Get our components from the category
            // Get all the button/tool components registered in our category
            foreach (var component in Categories.GetComponentElements("Filter_Options_Category"))
            {
                try
                {
                    var content = component.GetContent();
                    var group = component.ReadAttribute("group") ?? "";

                    //check we get a plugin
                    var plugin = FrameworkApplication.GetPlugInWrapper(component.ID);
                    if (plugin != null)
                    {
                        Add(new FilterGalleryItem(component.ID, group, plugin));
                    }
                }
                catch (Exception e)
                {
                    string x = e.Message;
                }
            }

        }

        protected override void OnClick(object item)
        {
            var filterItem = item as FilterGalleryItem;
            filterItem.Execute();
        }

        private void CacheGalleryTemplates()
        {
            var resourceDir = GetResourceDictionary("pack://application:,,,/OpenItemDialogBrowseFilter;component/FilterGalleryTemplate.xaml");
            if (resourceDir != null)
            {
                _smallGalleryItemTemplate = resourceDir["FilterGallerySmallImageItemTemplate"];
                _largeGalleryItemTemplate = resourceDir["FilterGalleryLargeImageItemTemplate"];
            }
        }

        private void LoadGalleryTemplate(bool smallGallerySize)
        {            
            this.ItemTemplate = smallGallerySize ? _largeGalleryItemTemplate : _smallGalleryItemTemplate ;
            
        }
        public void ToggleGalleryTemplate()
        {
            _isSmallGalleryItemTemplate = !_isSmallGalleryItemTemplate;
            LoadGalleryTemplate(_isSmallGalleryItemTemplate);
        }

        private static ResourceDictionary GetResourceDictionary(string templateFile)
        {
            ResourceDictionary resourceDir = null;
            var streamInfo = System.Windows.Application.GetResourceStream(new Uri(templateFile));
            if ((streamInfo != null) && (streamInfo.Stream != null))
            {
                using (System.IO.StreamReader reader = new(streamInfo.Stream))
                {
                    resourceDir = XamlReader.Load(reader.BaseStream) as ResourceDictionary;
                }
            }
            streamInfo = null;
            return resourceDir;
        }
    }

    internal class ToggleGallerySizeButton :Button
    {
        protected override void OnClick()
        {
            Module1.ToggleFiltersGalleryView();
        }
    }
}
