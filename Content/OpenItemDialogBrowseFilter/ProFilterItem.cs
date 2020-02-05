/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenItemDialogBrowseFilter
{
    public class ProFilterItem : PropertyChangedBase
    {
        private string _filterID;
        private string _filterName;
        public ProFilterItem(string filterID, string filterName)
        {
            _filterID = filterID;
            _filterName = filterName;
            _openItemBrowseFilter = new BrowseProjectFilter(filterID);
        }

        public string FilterID
        {
            get { return _filterID; }
            set
            {
                SetProperty(ref _filterID, value, () => FilterID);
            }
        }

        public string FilterName
        {
            get { return _filterName; }
            set
            {
                SetProperty(ref _filterName, value, () => FilterName);
            }
        }
        private BrowseProjectFilter _openItemBrowseFilter;
        public BrowseProjectFilter OpenItemBrowseFilter
        {
            get { return _openItemBrowseFilter; }
            set
            { SetProperty( ref _openItemBrowseFilter, value, () => OpenItemBrowseFilter); }
        }
    }
}
