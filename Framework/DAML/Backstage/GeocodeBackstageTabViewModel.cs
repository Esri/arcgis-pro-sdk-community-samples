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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;


namespace DAML.Backstage
{
    internal class GeocodeBackstageTabViewModel : BackstageTab
    {
        /// <summary>
        /// Called when the backstage tab is selected.
        /// </summary>
        protected override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }

        /// <summary>
        /// Called when the backstage tab is unselected.
        /// </summary>
        protected override Task UninitializeAsync()
        {
            return base.UninitializeAsync();
        }

        private string _tabHeading = "Tab Title";
        public string TabHeading
        {
            get
            {
                return _tabHeading;
            }
            set
            {
                SetProperty(ref _tabHeading, value, () => TabHeading);
            }
        }
    }
}
