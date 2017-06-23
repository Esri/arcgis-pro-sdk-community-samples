//Copyright 2014 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.using System;

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.DataReviewer;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.GeoProcessing;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.DataReviewer.Models;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;

namespace DataReviewerProSDKSamples
{
    /// <summary>
    /// This class is used to create GalleryItems from BatchJob and Session project items
    /// GalleryItems are added to the gallery
    /// </summary>
    internal class GalleryItem : ViewModelBase
    {
        public GalleryItem(string typeId,string path, string name)
        {
            TypeId = typeId;
            Path = path;
            Name = name;
        }
        public string TypeId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }

        public ContextMenu GalleryItemMenu 
        {
            get
            {
                if (TypeId == "file_datareviewer_batchjob")
                    return FrameworkApplication.CreateContextMenu("Reviewer_BatchJob_GalleryItem_Menu");
                else if (TypeId == "SessionResources")
                    return FrameworkApplication.CreateContextMenu("Reviewer_Session_GalleryItem_Menu");
                else
                    return null;
            }
        }
    }

    /// <summary>
    /// This class is for creating the events for gallery items
    /// </summary>
    internal sealed class GalleryItemsChangedEvent : CompositePresentationEvent<GalleryItemsChangedEvent.Args>
    {
        public static SubscriptionToken Subscribe(Action<GalleryItemsChangedEvent.Args> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<GalleryItemsChangedEvent>().Register(action, keepSubscriberAlive);
        }

        public static void Unsubscribe(Action<GalleryItemsChangedEvent.Args> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<GalleryItemsChangedEvent>().Unregister(action);
        }

        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<GalleryItemsChangedEvent>().Unregister(token);
        }

        internal static void Publish(GalleryItemsChangedEvent.Args galleryItemsChangedEventArgs)
        {
            FrameworkApplication.EventAggregator.GetEvent<GalleryItemsChangedEvent>().Broadcast(galleryItemsChangedEventArgs);
        }

        static public void Publish(GalleryItem galleryItem, bool adding) 
        {
            GalleryItemsChangedEvent.Publish(new Args(galleryItem, adding)); 
        }

        internal sealed class Args : EventArgs
        {
            // <param name="adding"> true == add operation, false == remove operation</param>
            public Args(GalleryItem galleryItem, bool adding) { GalleryItem = galleryItem; Adding = adding; }
            public GalleryItem GalleryItem { get; private set; }
            public bool Adding { get; private set; }
        }
       
    }

    /// <summary>
    /// This is the ViewModel of the Gallery View
    /// </summary>
    internal class GalleryItemsViewModel : Gallery
    {
        /// <summary>
        /// subscribe to gallery item events
        /// </summary>
        public GalleryItemsViewModel()
        {
            GalleryItemsChangedEvent.Subscribe(OnChanged);
        }
       
        /// <summary>
        /// OnChange event is fired when an item is added or removed from the gallery
        /// </summary>
        /// <param name="arg"></param>
        private void OnChanged(GalleryItemsChangedEvent.Args arg)
        {
            
            if (arg.GalleryItem == null)
                return;
            else
            {
                if ((this.ID == "Reviewer_BatchJob_Gallery_Advanced" && arg.GalleryItem.TypeId == "file_datareviewer_batchjob") ||
                    (this.ID == "Reviewer_Sessions_Gallery_Advanced" && arg.GalleryItem.TypeId == "SessionResources"))
                {
                    if (arg.Adding)
                    {
                        if (!ItemCollection.Any(p => (p as GalleryItem).Path == arg.GalleryItem.Path))
                            Add(arg.GalleryItem);
                    }
                    else
                    {
                        GalleryItem itemToRemove = ItemCollection.Where(p => (p as GalleryItem).Path == arg.GalleryItem.Path).FirstOrDefault() as GalleryItem;
                        if (null != itemToRemove)
                            Remove(itemToRemove);
                    }
                }
            }
        }
        
        /// <summary>
        /// Unsubscribe gallery item events
        /// </summary>
        protected override void Uninitialize()
        {
            base.Uninitialize();
            GalleryItemsChangedEvent.Unsubscribe(OnChanged);
        }

       
    }
}
