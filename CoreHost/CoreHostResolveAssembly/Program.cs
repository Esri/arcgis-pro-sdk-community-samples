using ArcGIS.Core.Data;
using ArcGIS.Core.Hosting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreHostResolveAssembly
{
  /// <summary>
  /// This sample illustrates how to deploy a standalone CoreHost application that resolves the ArcGIS Pro location in order to load the dependent assemblies such as ArcGIS.Core.dll and ArcGIS.CoreHost.dll from the ArcGIS Pro install location.
  /// Except for the dynamic assembly loading logic this sample is identical to the "CoreHostSample". 
  /// The references to ArcGIS.Core.dll and ArcGIS.CoreHost.dll in Visual Studio have the "Copy Local" property set to "false". 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "FeatureTest.aprx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\SDK\SDK.gdb" is available.
  /// 1. Open this solution in Visual Studio 
  /// 1. Click the build menu and select Build Solution.
  /// 1. Specify a valid path to a file geodatabase as your debug command line parameter 
  /// 1. Click the Start button to run the console app.
  /// 1. Note that the AppDomain is modified on startup to resolve the Assembly Paths for ArcGIS.Core.dll and ArcGIS.CoreHost.dll by using the ArcGIS Pro installation location.
  /// 1. View the table definition in your file geodatabase
  /// ![UI](Screenshots/ConsoleWindow.png)
  /// 1. Once the output stops press any key to close the application.  
  /// </remarks>
  class Program
  {

    private static string _arcgisProPath = "";
    //[STAThread] must be present on the Application entry point
    [STAThread]
    static void Main(string[] args)
    {
      //Get path of Pro installation from registry 
      _arcgisProPath = GetInstallDirAndVersionFromReg().Item1;
      string ProNotFound = Properties.Resource1.ProNotFound;
      if (string.IsNullOrEmpty(_arcgisProPath)) throw new InvalidOperationException(ProNotFound);

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
      //Call Host.Initialize before constructing any objects from ArcGIS.Core
      try
      {
        Host.Initialize();
      }
      catch (Exception e)
      {
        // Error (missing installation, no license, 64 bit mismatch, etc.)
        Console.WriteLine(string.Format("Initialization failed: {0}", e.Message));
        return;
      }
      var gdbPath = @"C:\Data\SDK\SDK.gdb";
      if (args.Count() > 0) gdbPath = args[0];
      try
      {
        //if we are here, ArcGIS.Core is successfully initialized
        using (var gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath, UriKind.Absolute))))
        {
          IReadOnlyList<TableDefinition> definitions = gdb.GetDefinitions<FeatureClassDefinition>();
          foreach (var fdsDef in definitions)
          {
            Console.WriteLine(TableString(fdsDef as TableDefinition));
          }
        }
        Console.WriteLine("Press any key to close this app.");
        Console.ReadKey();
      }
      catch (Exception e)
      {
        // Error (missing installation, no license, 64 bit mismatch, etc.)
        Console.WriteLine(string.Format("Cannot read file Geodatabase [{0}] error: {1}", gdbPath, e.Message));
        return;
      }
    }

    private static string TableString(TableDefinition table)
    {
      string alias = table.GetAliasName();
      string name = table.GetName();
      StringBuilder sb = new StringBuilder();
      sb.AppendLine(string.Format("{0} ({1})", alias.Length > 0 ? alias : name, name));
      sb.AppendLine(new string('-', 80));
      foreach (var fld in table.GetFields())
      {
        sb.Append(string.Format("{0,-20}", fld.AliasName ?? fld.Name));
        sb.Append(string.Format("{0,-14} ({1}) ", fld.FieldType.ToString(), fld.Length));
        if (fld.IsNullable || fld.IsRequired)
        {

          string sep = fld.IsNullable && fld.IsRequired ? ", " : "";
          sb.Append(string.Format("({0}{1}{2})", fld.IsNullable ? "Null" : "", sep, fld.IsRequired ? "Required" : ""));
        }
        sb.Append("\r\n");
      }
      sb.Append("\r\n");
      return sb.ToString();
    }

    static Assembly ResolveProAssemblyPath(object sender, ResolveEventArgs args)
    {

      string assemblyPath = Path.Combine(_arcgisProPath, "bin", new AssemblyName(args.Name).Name + ".dll");
      if (!File.Exists(assemblyPath)) return null;
      Assembly assembly = Assembly.LoadFrom(assemblyPath);
      return assembly;
    }
    /// <summary>
    /// Get the ArcGIS Pro install location from the registry.
    /// </summary>
    /// <returns></returns>
    private static Tuple<string, string> GetInstallDirAndVersionFromReg()
    {
      string regPath = string.Format(@"SOFTWARE\ESRI\{0}", "ArcGISPro");
      string temp = Properties.Resource1.InstallDirMissing;

      string err1 = string.Format(Properties.Resource1.InstallDirMissing, string.Format(@"HKLM\{0}\{1}", regPath, "InstallDir"));
      string err2 = string.Format(Properties.Resource1.VersionMissing, string.Format(@"HKLM\{0}\{1}", regPath, "Version"));


      string path = "";
      string version = "";
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
      return new Tuple<string, string>(path, version);
    }


  }
}
