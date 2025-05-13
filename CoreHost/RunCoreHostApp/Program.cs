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
using System;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Threading.Channels;
using System.Text;

namespace RunCoreHostApp
{/// <summary>
 /// This application can be used to run a CoreHost standalone CoreHost application on ArcGIS Pro 3.3 or later, even so the CoreHost App is build for 3.0, 3.1 or 3.2.
 /// </summary>
 /// <remarks>
 /// If you write a CoreHost standalone app for ArcGIS Pro 3.1 you can achieve forward compatibility as well, but there are a few caveats:
 /// 1)	Your CoreHost app is in essence a.NET console app with references to the following ArcGIS Pro assemblies: ArcGIS.Core and ArcGIS.CoreHost.You have to make sure that the “Copy Local” attribute for these references is set to “NO”.   You also have to add code in your CoreHost app to resolve the path to these assemblies (they are located in the ArcGIS Pro installation bin folder).  This ensures that your CoreHost application is actually running the assemblies that are installed with ArcGIS Pro and not a potentially outdated(or mismatched versions) assembly copy included with your CoreHost app.
 /// 2)	Also, your CoreHost app cannot be a ‘self-contained’ .NET application, instead it has to have a ‘Target Framework’.  In order to implement this, you have to edit your.CSPROJ file and add the following setting under the property group:
 /// &lt;SelfContained&gt;false&lt;/SelfContained&gt;
 /// When a console app is ‘self-contained’, the runtime for the target .NET version is included with the binary output when the console application is built.However, this feature is not desirable because this would mean that your .NET runtime version is static.
 /// If you follow the steps above your CoreHost standalone app can be forward compatible, but the problem is that the app will only allow the target.NET version to be loaded.  So, in our case since you built the app using the Pro SDK 3.1 this means that the CoreHost app is permanently linked to.NET 6.0 (or any minor release of .NET 6.0).  I added a small sample project called ‘CoreHostTest31Build’ to this post so you can see an implementation of a ‘forward compatible’ capable CoreHost app.
 /// If you look at the .json files included with the corehost app you will notice that they are ‘bound’ to a specific .NET target of .NET 6.0, which means that the CoreHost app will not work under ArcGIS Pro 3.3 since Pro 3.3 requires .NET 8.0.  You will get this error:
 /// &gt; CoreHostTest31Build C:\Data\FeatureTest\FeatureTest.gdb
 /// Could not load file or assembly 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.The system cannot find the file specified.
 /// You can see that the CoreHost app is trying to load.NET 8.0 because ArcGIS Pro 3.3 requires.NET 8.0.However, the CoreHost app is bound to .NET 6.0 and hence the loading of.NET 8.0 fails.
 /// This problem can be fixed by updating the.NET target framework version as a parameter of the dotnet command line tool: 
 /// For ArcGIS Pro 3.0, 3.1, 3.2 .NET 6.0 is required and the CoreHost dll can be called using the following command line:
 /// "C:\Program Files\dotnet\dotnet.exe" exec --fx-version "6.0.30" CoreHostTest31Build.dll
 /// and for ArcGIS Pro 3.3 and later .NET 8.0 is required and the CoreHost dll can be called using the following command line:
 /// "C:\Program Files\dotnet\dotnet.exe" exec --fx-version "8.0.5" CoreHostTest31Build.dll
 /// the exact version of available .NET installations has to be found because the --fx-version parameter requires an exact version of .NET.
 /// </remarks>
  class Program
  {    
    static void Main(string[] args)
    {
      var coreHostAppName = string.Empty;
      var coreHostDll = string.Empty;
      var coreHostAppPath = string.Empty;
      (string Folder, string Version)? proInstallInfo = null;
      try
      {
        if (args.Length == 0)
        {
          throw new Exception("Please provide a CoreHost dll name.");
        }
        var argList = new List<string>(args);
        argList.RemoveAt(0);
        coreHostAppName = args[0];
        var coreHostAppExtension = Path.GetExtension(coreHostAppName);
        if (!string.IsNullOrEmpty(coreHostAppExtension)
          && coreHostAppExtension.ToLower().CompareTo(".dll") == 0)
        {
          coreHostAppName = Path.GetFileNameWithoutExtension(coreHostAppName);
          coreHostAppPath = AppDomain.CurrentDomain.BaseDirectory;
          coreHostDll = Path.Combine(coreHostAppPath, $"{coreHostAppName}.dll");
        }
        // check if the CoreHost dll exists
        if (!File.Exists(coreHostAppName))
        {
          // we have to all the path to the CoreHost dll
          coreHostDll = Path.Combine(coreHostAppPath, $"{coreHostAppName}.dll");
        }
        if (!File.Exists(coreHostDll))
        {
          throw new Exception($@"Error: {coreHostDll} does not exist.");
        }
        // we will  try to load the correct .NET assembly from the correct .NET version 
        // with Pro Version 3.0 - 3.2 the .NET version is 6.0
        // with Pro Version 3.3+ the .NET version is 8.0
        proInstallInfo = MSIHelper.GetInstallDirAndVersion();
        if (proInstallInfo == null)
        {
          throw new Exception("Error: Can't get ArcGIS Pro version/install location from registry.");
        }
        // get all versions of .NET installed
        DotNetHelper.InitializeDotNetVars();
        var dotNetCmdArgsPrefix = string.Empty;
        if (proInstallInfo.Value.Version.CompareTo("3.3") >= 0)
        {
          // Pro Version 3.3+ the .NET version is 8.0
          // command line to run the CoreHost app using: exec --fx-version "8.0.*" 
          foreach (var dotNetVer in DotNetHelper.DotNetVersions)
          {
            if (dotNetVer.StartsWith("8.0"))
            {
              dotNetCmdArgsPrefix = @$"exec --fx-version ""{dotNetVer}"" {MakeArguments(new List<string> { coreHostDll })}"; 
            }
          }
        }
        else
        {
          // Pro Version 3.0 - 3.2 the .NET version is 6.0
          // command line to run the CoreHost app using: exec --fx-version "6.0.*" 
          foreach (var dotNetVer in DotNetHelper.DotNetVersions)
          {
            if (dotNetVer.StartsWith("6.0"))
            {
              dotNetCmdArgsPrefix = @$"exec --fx-version ""{dotNetVer}"" {MakeArguments(new List<string> { coreHostDll })}";
            }
          }
        }
        if (string.IsNullOrEmpty(dotNetCmdArgsPrefix) )
          throw new Exception("Error: Cannot find suitable version of .NET.");
        var proProcess = new Process();
        proProcess.StartInfo.FileName = DotNetHelper.DotNetExePath;
        proProcess.StartInfo.Arguments = dotNetCmdArgsPrefix + " " + MakeArguments(argList);
        proProcess.StartInfo.UseShellExecute = false;
        proProcess.Start();
      }
      catch (Exception ex)
      {
        Console.WriteLine($@"{ex.Message}");
        var nl = Environment.NewLine;
        Console.WriteLine($@"Configuration:{nl} CoreHost App name {coreHostAppName}{nl} CoreHost DLL name {coreHostDll}{nl} CoreHost App path {coreHostAppPath}");
        if (proInstallInfo != null)
          Console.WriteLine($@"ArcGIS Pro Install Info:{nl} Folder {proInstallInfo?.Folder}{nl} Version {proInstallInfo?.Version}");
        Console.WriteLine($@"DotNet runtime path: {DotNetHelper.DotNetRuntimePath} DotNet exe path: {DotNetHelper.DotNetExePath}");
        Console.WriteLine($@"DotNet version found:");
        foreach (var dotNetVer in DotNetHelper.DotNetVersions)
        {
          Console.WriteLine($@" {dotNetVer}");
        }
        Environment.Exit(1);
      }
    }

    private static string MakeArguments(List<string> argList)
    {
      var arguments = new StringBuilder();
      foreach (var arg in argList)
      {
        if (arguments.Length > 0)
        {
          arguments.Append(" ");
        }
        if (arg.Contains(" "))
        {
          arguments.Append($"\"{arg.Replace("\"", "\"\"")}\"");
        }
        else
        {
          arguments.Append(arg);
        }
      }
      return arguments.ToString();
    }
  }
}
