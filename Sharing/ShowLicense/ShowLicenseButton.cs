//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using Microsoft.Win32;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Core.Licensing;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Tests.APIHelpers.SharingDataContracts;

namespace ShowLicense
{
    /// <summary>
    /// This sample provides a new tab and controls that shows the licensing info
    ///  and expiration dates in a message box.
    /// </summary>
    internal class ShowLicenseButton : Button
    {
        /// <summary>
        /// enumerations of entitlements (licensing level and extensions)
        /// </summary>
        enum Entitlements
        {
            None = 0,
            desktopBasicN = 100,
            desktopStdN = 200,
            desktopAdvN = 300,
            geostatAnalystN = 1000,
            networkAnalystN = 2000,
            spatialAnalystN = 3000,
            _3DAnalystN = 4000,
            dataReviewerN = 5000,
            workflowMgrN = 6000,
        }

        /// <summary>
        /// On user click returns message box displaying licensing info
        /// </summary>
        protected override void OnClick()
        {
            Tuple<bool, string> res = TestGetLicense();
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(res.Item2, "Pro License Details", MessageBoxButton.OK);
        }

        /// <summary>
        /// Get the licensing portal from registry
        /// </summary>
        /// <returns></returns>
        Tuple<bool, string> GetLicensingPortalFReg()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\ESRI\\ArcGIS Online For Pro\\Signin", false);
                if (rk.GetValue("AuthorizationPortal") != null)
                {
                    string rk_val = rk.GetValue("AuthorizationPortal").ToString();
                    return new Tuple<bool, string>(true, rk_val);
                }
                else
                {
                    return new Tuple<bool, string>(false, "https://www.arcgis.com/");
                }
            }
            catch (System.IO.IOException e)
            {
                return new Tuple<bool, string>(false, "Reg key {Software\\ESRI\\ArcGIS Online For Pro\\Signin} does not exist: " + e.ToString());
            }
        }

        /// <summary>
        /// Print the license codes and expiration dates in string
        /// </summary>
        /// <returns></returns>
        string printLicenseCodes()
        {
            string printLine = "";
            foreach (LicenseCodes lc in Enum.GetValues(typeof(LicenseCodes)))
            {
                bool avail = LicenseInformation.IsAvailable(lc);
                if (avail)
                {
                    printLine += lc.ToString() + "\t" + LicenseInformation.GetExpirationDate(lc);
                    DateTime expDate = LicenseInformation.GetExpirationDate(lc) ?? DateTime.Now;
                    TimeSpan remain = expDate.Subtract(DateTime.Now);
                    printLine += "\t\tTime remaining till expiration: " + remain.Days + " day(s), " + remain.Hours + " hrs(s), " + remain.Minutes + " min(s)\n";
                }
                else
                {
                    printLine += lc.ToString() + "\t" + "License not available\n";
                }
            }
            return printLine;
        }

        /// <summary>
        /// This method uses EsriHttpClient.Get method to fetch licensing info from licensing portal
        /// </summary>
        /// <returns></returns>
        Tuple<bool, string> TestGetLicense()
        {
            string printMore = "";
            LicenseLevels ll = LicenseInformation.Level;
            string ll_str = Enum.GetName(typeof(LicenseLevels), ll);
            printMore += "License level: " + ll_str + "\n";

            Tuple<bool, string> regPortal = GetLicensingPortalFReg();
            //if (regPortal.Item1 == false)
            //Assert.Inconclusive(regPortal.Item2 + " [CR310474]");
            string portalUrl = regPortal.Item2;
            printMore += "Licensing portal: " + portalUrl + "\n";
            EsriHttpClient myClient = new EsriHttpClient();

            #region REST call to get appInfo.Item.id and user.lastLogin of the licensing portal
            string selfUri = @"/sharing/rest/portals/self?f=json";
            var selfResponse = myClient.Get(portalUrl + selfUri);
            if (selfResponse == null)
                return new Tuple<bool, string>(false, "HTTP response is null");

            if (selfResponse.StatusCode != System.Net.HttpStatusCode.OK)
                return new Tuple<bool, string>(false, "Licensing portal is not set");

            string outStr = selfResponse.Content.ReadAsStringAsync().Result;

            //Deserialize the response in JSON into a usable object. 
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            PortalSelf self_obj = (PortalSelf)serializer.Deserialize(outStr, typeof(PortalSelf));
            if ((self_obj == null) || (self_obj.appInfo == null) || (self_obj.user == null))
            {
                return new Tuple<bool, string>(true, printLicenseCodes() + "\nPro is licensed offline.");
            }
            #endregion

            #region REST call to get the userLicenses
            string layerUri = @"/sharing/rest/content/listings/" + self_obj.appInfo.itemId + "/userLicenses?f=json&nonce=007&timestamp=" + self_obj.user.lastLogin;
            var response = myClient.Get(portalUrl + layerUri);
            if (response == null)
                return new Tuple<bool, string>(false, "HTTP response is null");

            string outStr2 = response.Content.ReadAsStringAsync().Result;

            //Deserialize the response in JSON into a usable object. 
            userLicenses obj = (userLicenses)serializer.Deserialize(outStr2, typeof(userLicenses));
            if (obj == null || obj.userEntitlementsString == null)
            {
                return new Tuple<bool, string>(true, printLicenseCodes() + "\nPro is licensed offline, and signed in with a different account.");
            }
            userEntitlementsString entitlement = (userEntitlementsString)serializer.Deserialize(obj.userEntitlementsString, typeof(userEntitlementsString));
            if (entitlement == null)
                return new Tuple<bool, string>(false, "Failed to fetch valid entitlements.");
            else
            {
                printMore += printLicenseCodes() + "Entitlements returned by GET request:";
                foreach (string e in entitlement.entitlements)
                    printMore += " " + e;
                printMore += "\nLicenses returned by GET request:";
                foreach (string l in entitlement.licenses)
                    printMore += " " + l;
                return new Tuple<bool, string>(true, printMore);
            }
            #endregion
        }
    }
}
