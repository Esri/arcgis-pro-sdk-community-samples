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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace ProjectCustomItemEarthQuake
{
    internal class QuakeFilterDialog : Button
    {
        protected override void OnClick()
        {
            var bf = new BrowseProjectFilter();
            //This allows us to view the .quake custom item (the "container")
            bf.AddCanBeTypeId("acme_quake_handler"); //TypeID for the ".quake" custom project item 
            //This allows the .quake item to be browsable to access the events inside
            bf.AddDoBrowseIntoTypeId("acme_quake_handler");
            //This allows us to view the quake events contained in the .quake item
            bf.AddCanBeTypeId("acme_quake_event"); //TypeID for the quake events contained in the .quake item
            bf.Name = "Quake Item";

            var openItemDialog = new OpenItemDialog
            {
                Title = "Open Quake Item",
                InitialLocation = @"E:\Data\CustomItem\QuakeCustomItem",
                BrowseFilter = bf
            };
            bool? ok = openItemDialog.ShowDialog();
        }
    }
}
