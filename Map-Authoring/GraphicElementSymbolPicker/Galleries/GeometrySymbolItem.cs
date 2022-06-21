/*

   Copyright 2022 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicElementSymbolPicker
{
    /// <summary>
    /// Represents each item displayed in the symbol gallery
    /// </summary>
    public class GeometrySymbolItem
    {
        public GeometrySymbolItem(SymbolStyleItem symbolStyleItem, string group)
        {
            Icon32 = symbolStyleItem.PreviewImage;
            Name = symbolStyleItem.Name;
            Group = group;
            if (symbolStyleItem != null)
                cimSymbol = symbolStyleItem.Symbol as CIMSymbol;

        }
        public object Icon32 { get; private set; }

        public string Name { get; private set; }

        public string Group { get; private set; }
        public CIMSymbol cimSymbol
        {
            get; private set;
        }
        internal void Execute()
        {
            if (cimSymbol != null)
            {
                //QueuedTask.Run(() => Module1.SelectedSymbol = cimSymbol);
                Module1.SelectedSymbol = cimSymbol;
                Module1.SelectedSymbolName = Name;
            }
                
        }
    }
}
