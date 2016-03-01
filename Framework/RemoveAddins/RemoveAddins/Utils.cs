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
                if (esriKey == null)
                {
                    //this is an error
                    throw new System.InvalidOperationException(err1);
                }

                foreach (var key in esriKey.GetValueNames())
                {
                    myAddInPathKeys.Add(key.ToString());
                }
                
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
