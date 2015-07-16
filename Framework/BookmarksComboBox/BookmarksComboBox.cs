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

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_comboBox_solution
{
    /// <summary>
    /// Represents the comboBox that holds the collection of Bookmarks in the project.
    /// </summary>

    internal class BookmarksComboBox : ComboBox
    {

        /// <summary>
        /// Constructor to initialize the ComboBox        
        /// </summary>
        public BookmarksComboBox()
        {
           UpdateCombo();
        }

        private ReadOnlyObservableCollection<Bookmark> _bmks = null; //Collection holding the bookmarks in the project

        /// <summary>
        /// Updates the combo box with all the maps in the current project.
        /// </summary>
        /// 
        private async Task UpdateCombo()
        {
            Task<ReadOnlyObservableCollection<Bookmark>> tbkmrks = GetActiveMapBookmarks(); //set task to get bookmarks

            _bmks = await tbkmrks; //get all the bookmarks in the current project.

            if (_bmks == null)
            {
                Add(new ComboBoxItem(String.Empty));
                return;
            }
            foreach (var bmk in _bmks) //iterate through the bookmark collection to get all the bookmarks, add to the combo box.
            {
                if (bmk != null)
                    Add(new ComboBoxItem(bmk.Name));
                else
                    Add(new ComboBoxItem(String.Empty));
            }

            Enabled = true; //enables the ComboBox
            SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
        }

        private Task<ReadOnlyObservableCollection<Bookmark>> GetActiveMapBookmarks()
        {
            return QueuedTask.Run(() =>
            {
                //Get the active map view.
                var mapView = MapView.Active;
                if (mapView == null)
                    return null;

                //Return the collection of bookmarks for the map.
                return mapView.Map.GetBookmarks();
            });
        }

        /// <summary>
        /// The on comboBox selection change event. Zoom to the bookmark selected.
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override async void OnSelectionChange(ComboBoxItem item)
        {
            if (_bmks == null)
                return;

            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;

            Bookmark bookmark = _bmks.FirstOrDefault(bk => bk.Name == item.Text); //get the bookmark from the name in the combo box
            if (bookmark != null)
                await MapView.Active.ZoomToAsync(bookmark);     //Zoom to the bookmark selected  
            
        }

    }
}
