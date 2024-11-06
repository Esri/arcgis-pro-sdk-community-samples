/*

   Copyright 2024 Esri

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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RunCoreHostApp
{
  internal class MSIHelper
  {
    [DllImport("msi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern UInt32 MsiEnumRelatedProducts(string strUpgradeCode, int reserved, int iIndex, StringBuilder productCode);

    [DllImport("msi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern Int32 MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref int len);

    /// <summary>
    /// This identifier should never change for the lifetime of Pro
    /// </summary>
    public static readonly string ArcGISProUpgradeId = "{353C8253-C237-46A7-9124-88EFBF4A0C3E}";

    /// <summary>
    /// This identifier should never change for the lifetime of Pro
    /// </summary>
    public static readonly string ArcGISAllSourceUpgradeId = "{AABEECF2-1C0F-474B-BFB8-A9A9A9D9BC4F}";

    /// <summary>
    /// Used if the Msi fails on a release install
    /// </summary>
    public static readonly string RegistryKeyUseForFallback = "ArcGISPro";

    /// <summary>
    /// Used if the Msi fails on a release install
    /// </summary>
    public static readonly string RegistryKeyAllSourceUseForFallback = "ArcGISAllSource";

    public static (string Folder, string Version)? GetInstallDirAndVersion()
    {
      return GetInstallDirAndVersion(string.Empty);
    }
    /// <summary>
    /// Get the install directory and version using the MSI Upgrade code. If that doesn't work, fallback on the registry.
    /// </summary>
    /// <param name="productName"></param>
    /// <returns>empty strings if no information found. Otherwise return directory and version.</returns>
    public static (string Folder, string Version)? GetInstallDirAndVersion(string productName)
    {
      string upgradeCode = string.Empty;
      string productNameRegKey = string.Empty;
      if (productName == "ArcGISPro" || string.IsNullOrEmpty(productName))
      {
        upgradeCode = ArcGISProUpgradeId;
        productNameRegKey = RegistryKeyUseForFallback;
      }

      else if (productName == "AllSource")
      {
        upgradeCode = ArcGISAllSourceUpgradeId;
        productNameRegKey = RegistryKeyAllSourceUseForFallback;
      }
      else
      {
        return null;
      }
      StringBuilder productCode = new(255);

      if (MsiEnumRelatedProducts(upgradeCode, 0, 0, productCode) != 0)
      {
        //Houston, we have a problem
        Debug.Assert(false, "DEBUG: DEBUG: The Msi could not resolve the current installed version of Pro!");
        //release - fallback on the hardcoded reg key
        return GetInstallDirAndVersionFromReg(productName);
      }
      string[] properties = new string[] { "InstallLocation", "VersionString" };
      string[] results = new string[properties.Length];
      int size = 1024;
      StringBuilder buffer = new(size);
      int i = 0;
      foreach (string prop in properties)
      {
        if (MsiGetProductInfo(productCode.ToString(), prop, buffer, ref size) != 0)
        {
          //Houston, we have a problem
          Debug.Assert(false, string.Format("DEBUG: DEBUG: The Msi could not resolve {0} for Pro!", prop));
          return GetInstallDirAndVersionFromReg(productNameRegKey);
        }
        results[i++] = buffer.ToString();
        buffer.Clear();
        size = buffer.Capacity;//reset size
      }
      return (results[0], results[1]);
    }

    /// <summary>
    /// Get the install information from the registry
    /// </summary>
    /// <param name="productNameRegKey"></param>
    /// <returns>Returns empty strings if no registry is found. Otherwise returns installDir, Version</returns>
    /// <exception cref="System.Exception"></exception>
    private static (string Folder, string Version)? GetInstallDirAndVersionFromReg(string productNameRegKey)
    {
      string regKeyName = productNameRegKey;
      string regPath = string.Format(@"SOFTWARE\ESRI\{0}", regKeyName);
      string regPathInstallDir = string.Format(@"HKLM\{0}\{1}", regPath, "InstallDir");
      string regPathVersion = string.Format(@"HKLM\{0}\{1}", regPath, "Version");
      var productDesignation = productNameRegKey == "ArcGISPro" ? "Pro" : "AllSource";

      object? path;
      object? version;
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
          return (string.Empty, string.Empty);
        }
        path = esriKey.GetValue("InstallDir");
        if (path == null)
        {
          //this is an error
          return (string.Empty, string.Empty);
        }

        version = esriKey.GetValue("Version");
        if (version == null)
        {
          //this is an error
          return (string.Empty, string.Empty);
        }
      }
      catch (InvalidOperationException ie)
      {
        //this is ours
        throw;
      }
      catch (Exception ex)
      {
        throw;
      }
      if (path == null || version == null) return null;

      return (path.ToString(), version.ToString());
    }
  }
}
