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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveAddins
{
    internal static class Utils
    {
        /// <summary>
        /// Gets the well-known Add-in folders on the machine
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAddInFolders()
        {

            List<string> myAddInPathKeys = new List<string>();

            string regPath = string.Format(@"Software\ESRI\ArcGISPro\Settings\Add-In Folders");
            //string path = "";
            string err1 = "This is an error";
            try
            {
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey esriKey = localKey.OpenSubKey(regPath);

                if (esriKey == null)
                {
                    localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64);
                    esriKey = localKey.OpenSubKey(regPath);
                }

                if (esriKey != null)
                    myAddInPathKeys.AddRange(esriKey.GetValueNames().Select(key => key.ToString()));

            }
            catch (InvalidOperationException ie)
            {
                //this is ours
                throw ie;
            }
            catch (Exception ex)
            {
                throw new System.Exception(err1, ex);
            }

            return myAddInPathKeys;

        }
    }
}
