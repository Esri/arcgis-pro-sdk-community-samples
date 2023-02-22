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
using ArcGIS.Core.Data;
using ArcGIS.Core.Hosting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISProConfigCoreHost
{
  /// <summary>
  /// This sample shows how to remote start and remote control ArcGIS Pro from an ArcGIS Corehost application.
  /// This sample is comprised of two samples:
  /// - Corehost **server application** that starts and controls the ArcGIS Pro client: ArcGISProConfigCoreHost
  /// - ArcGIS Pro **client application** that is started and controlled: ArcGISProConfig
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureTest.aprx" is available.
  /// 1. Open both solutions in Visual Studio: ArcGISProConfigCoreHost and ArcGISProConfig
  /// 1. Rebuild both solutions.
  /// 1. Make sure this project is available: "C:\Data\FeatureTest\FeatureTest.aprx" or change the path in the Corehost application.
  /// 1. Run the client application: ArcGISProConfigCoreHost
  /// 1. Note that the AppDomain is modified on startup to resolve the Assembly Paths for ArcGIS.Core.dll and ArcGIS.CoreHost.dll by using the ArcGIS Pro installation location.
  /// 1. The ArcGISProConfigCoreHost console application starts ArcGIS Pro with the "ArcGISProConfig" Managed Configuration
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Once ArcGISProConfig has started and is ready, ArcGISProConfigCoreHost sends the project path: "C:\Data\FeatureTest\FeatureTest.aprx" to be opened
  /// 1. ArcGISProConfig opens the project
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Once the output stops press any key to close the application.  
  /// 1. Close the "ArcGISProConfig" client session (ArcGIS Pro) using the User Interface.  Note that closing can be automated as well.
  /// 1. The "ArcGISProConfigCoreHost" Corehost application terminates after ArcGIS Pro has been closed.
  /// ![UI](Screenshots/Screen3.png)
  /// </remarks>
  internal class Program
  {
    private static string _arcgisProPath = "";

    private static bool _processExited = false;

    private static string _project = @"C:\Data\FeatureTest\FeatureTest.aprx";

    //[STAThread] must be present on the Application entry point
    [STAThread]
    static void Main(string[] args)
    {            //Get path of Pro installation from registry 
      _arcgisProPath = GetInstallDirAndVersionFromReg().path;
      if (string.IsNullOrEmpty(_arcgisProPath)) throw new InvalidOperationException("Can't find Pro installation");

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
      try
      {
        //Call Host.Initialize before constructing any objects from ArcGIS.Core
        Host.Initialize();
        //TODO: Add your business logic here.   
        Process processArcGISConfig = new Process();
        processArcGISConfig.StartInfo.FileName = "ArcGISPro.exe";
        processArcGISConfig.StartInfo.Arguments = @"/config:ArcGISProConfig";
        processArcGISConfig.StartInfo.UseShellExecute = false;
        processArcGISConfig.EnableRaisingEvents = true;
        processArcGISConfig.Exited += ProcessArcGISConfig_Exited;
        // comment this line out if you are running the configuriaton in a debugger
        // and set started to true
        var started = processArcGISConfig.Start();
        //var started = true;
        Console.WriteLine("Started ArcGIS Pro");
        if (started)
        {
          // talk to Pro
          using (NamedPipeServerStream namedPipeServer = new NamedPipeServerStream("ArcGISProCom"))
          {
            namedPipeServer.WaitForConnection();
            //namedPipeServer.BeginWaitForConnection(ConnectionCallback, namedPipeServer);
            using (StreamWriter sw = new StreamWriter(namedPipeServer))
            {
              sw.AutoFlush = true;
              // Send a 'sync message' and wait for client to receive it.
              sw.WriteLine("SYNC");
              namedPipeServer.WaitForPipeDrain();
              // Send the 'open' project command to client
              sw.WriteLine($@"Open Project: '{_project}'");
            }
          }
          // wait until Pro has shut down
          while (!_processExited)
          {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Pro is still running");
          }
          Console.WriteLine("ArcGIS Pro stopped running");
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(ex.Message);
      }
    }


    private static void ProcessArcGISConfig_Exited(object sender, EventArgs e)
    {
      _processExited = true;
    }

    #region Utility functions to set path to Pro Assemblies

    /// <summary>
    /// Resolves the ArcGIS Pro Assembly Path.  Called when loading of an assembly fails.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <returns>programmatically loaded assembly in the pro /bin path</returns>
    static Assembly ResolveProAssemblyPath(object sender, ResolveEventArgs args)
    {
      string assemblyPath = Path.Combine(_arcgisProPath, "bin", new AssemblyName(args.Name).Name + ".dll");
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

    #endregion
  }
}
