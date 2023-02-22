/*

   Copyright 2023 Esri

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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVChargers
{
  internal static class Defaults
  {
    public static IDictionary<string, string> ChargerFieldNameTypeMapping = new Dictionary<string, string>
    {
      { "Level 1 ( 3 to 5 mph)", "USER_EV_Level1_EVSE_Num" },
      { "Level 2 (12 to 80 mph)", "USER_EV_Level2_EVSE_Num"},
      { "DC Fast (3 to 20 mpm)", "USER_EV_DCFast_Count"},
      { "All", ""}
    };
    /// <summary>
    /// Dictionary: Key is the UI Value seen int he drop dropdown.  The Value is the EV Connector Type field value for the connector type.
    /// </summary>
    public static IDictionary<string, string> ConnectorValueTypesMapping = new Dictionary<string, string>
    {
      { "J1772",  "J1772" },
      {"CHAdeMO", "CHADEMO" }, 
      { "CCS (DC)", "J1772COMBO" },
      { "Tesla", "TESLA" },
      { "All", "" }
    };

    //public static List<string> AddressFields = new List<string>
    //{
    //  "USER_Street_Address",
    //  "USER_City",
    //  "USER_State",
    //  "USER_ZIP",
    //  "Country"
    //};
  }
}
