//Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using CoreHostGDB.UI;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace CoreHostGDB {
    /// <summary>
    /// WPF application that implements a generic File GDB reader
    /// </summary>
    /// <remarks>
    /// 1. Open this solution in Visual Studio 
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to run the WPF app.  
    /// 1. Specify a valid path to a file geodatabase path in the 'Open a GDB' input field and click the 'Open' button.  
    /// 1. The 'Open a Dataset' dropdown is filled with all available datasets.  
    /// 1. Select a dataset on the 'Open a Dataset' dropdown and click the 'Read' button.
    /// 1. View the table showing the dataset's content.
    /// ![UI](Screenshots/Screen.png)
    /// </remarks>
    class Program {

        static Window w = null;

        [STAThread]
        static void Main(string[] args) 
        {
            //Resolve ArcGIS Pro assemblies.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(ResolveProAssemblyPath);

            //Perform CoreHost task
            try
            {
                PerformCoreHostTask(args);
            }
            catch (Exception e)
            {
                // Error (missing installation, no license, 64 bit mismatch, etc.)
                Console.WriteLine(string.Format("Initialization failed: {0}", e.Message));
                return;
            }
        }
        private static void PerformCoreHostTask(string[] args)
        {
                //Initialize CoreHost (we can do it in Window Initialize but this way
                //we don't even pop the form if Initialize fails

                ArcGIS.Core.Hosting.Host.Initialize();//this will throw!

            Application app = new Application();
            w = new Window
            {
                Height = 500,
                Width = 700
            };
            Grid g = new Grid();
            g.Children.Add(new GDBGrid());
            w.Content = g;
            w.Title = "ArcGIS Pro CoreHost GDB Sample: File GDB Reader";
            app.Run(w);            
        }


        /// <summary>
        /// Resolves the ArcGIS Pro Assembly Path.  Called when loading of an assembly fails.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>programmatically loaded assembly in the pro /bin path</returns>
        static Assembly ResolveProAssemblyPath(object sender, ResolveEventArgs args)
        {
            //Get path of Pro installation from registry 
            string assemblyPath = Path.Combine(GetInstallDirAndVersionFromReg().path, "bin", new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        /// <summary>
        /// Gets the ArcGIS Pro install location, major version, and build number from the registry.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">InvalidOperationException</exception>
        internal static (string path, string version, string buildNo) GetInstallDirAndVersionFromReg()
        {
            string regKeyName = "ArcGISPro";
            string regPath = $@"SOFTWARE\ESRI\{regKeyName}";

            string err1 = $@"Install location of ArcGIS Pro cannot be found. Please check your registry for HKLM\{regPath}\InstallDir";
            string err2 = $@"Version of ArcGIS Pro cannot be determined. Please check your registry for HKLM\{regPath}\Version";
            string err3 = $@"Build Number of ArcGIS Pro cannot be determined. Please check your registry for HKLM\{regPath}\BuildNumber";
            string path;
            string version;
            string buildNo;
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
                path = esriKey.GetValue("InstallDir") as string;
                if (path == null || path == string.Empty)
                    //this is an error
                    throw new InvalidOperationException(err1);

                version = esriKey.GetValue("Version") as string;
                if (version == null || version == string.Empty)
                    //this is an error
                    throw new InvalidOperationException(err2);

                buildNo = esriKey.GetValue("BuildNumber") as string;
                if (buildNo == null || buildNo == string.Empty)
                    //this is an error
                    throw new InvalidOperationException(err3);
            }
            catch (InvalidOperationException ie)
            {
                //this is ours
                throw ie;
            }
            catch (Exception ex)
            {
                throw new Exception(err1, ex);
            }
            return (path, version, buildNo);
        }
    }
}
