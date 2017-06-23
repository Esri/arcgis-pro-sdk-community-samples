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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMapViews
{
    public class MapviewItem : PropertyChangedBase
    {
        public string MapviewName
        {
            get
            {
                return $@"{MapName}-{MapNumber:00}";
            }
        }

        private UInt16 _mapNumber;
        public UInt16 MapNumber
        {
            get
            {
                return _mapNumber;
            }
            set
            {
                SetProperty(ref _mapNumber, value, () => MapNumber);
            }
        }

        private string _mapName;
        public string MapName
        {
            get
            {
                return _mapName;
            }
            set
            {
                SetProperty(ref _mapName, value, () => MapName);
            }
        }

        private bool _hasViewPane;
        public bool HasViewPane
        {
            get
            {
                return _hasViewPane;
            }
            set
            {
                SetProperty(ref _hasViewPane, value, () => HasViewPane);
            }
        }

        private Envelope _extent;

        public Envelope Extent
        {
            get
            {
                return _extent;
            }
            set
            {
                SetProperty(ref _extent, value, () => Extent);
            }
        }


    }
}
