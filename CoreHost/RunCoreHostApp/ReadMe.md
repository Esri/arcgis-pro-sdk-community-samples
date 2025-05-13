## RunCoreHostApp

<!-- TODO: Write a brief abstract explaining this sample -->
This application can be used to run a CoreHost standalone CoreHost application on ArcGIS Pro 3.3 or later, even so the CoreHost App is build for 3.0, 3.1 or 3.2.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Console
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  05/06/2025
ArcGIS Pro:            3.5
Visual Studio:         2022
.NET Target Framework: net6.0
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
If you write a CoreHost standalone app for ArcGIS Pro 3.1 you can achieve forward compatibility as well, but there are a few caveats:  
1)	Your CoreHost app is in essence a.NET console app with references to the following ArcGIS Pro assemblies: ArcGIS.Core and ArcGIS.CoreHost.You have to make sure that the “Copy Local” attribute for these references is set to “NO”.   You also have to add code in your CoreHost app to resolve the path to these assemblies (they are located in the ArcGIS Pro installation bin folder).  This ensures that your CoreHost application is actually running the assemblies that are installed with ArcGIS Pro and not a potentially outdated(or mismatched versions) assembly copy included with your CoreHost app.  
2)	Also, your CoreHost app cannot be a ‘self-contained’ .NET application, instead it has to have a ‘Target Framework’.  In order to implement this, you have to edit your.CSPROJ file and add the following setting under the property group:  
<SelfContained>false</SelfContained>  
When a console app is ‘self-contained’, the runtime for the target .NET version is included with the binary output when the console application is built.However, this feature is not desirable because this would mean that your .NET runtime version is static.  
If you follow the steps above your CoreHost standalone app can be forward compatible, but the problem is that the app will only allow the target.NET version to be loaded.  So, in our case since you built the app using the Pro SDK 3.1 this means that the CoreHost app is permanently linked to.NET 6.0 (or any minor release of .NET 6.0).  I added a small sample project called ‘CoreHostTest31Build’ to this post so you can see an implementation of a ‘forward compatible’ capable CoreHost app.  
If you look at the .json files included with the corehost app you will notice that they are ‘bound’ to a specific .NET target of .NET 6.0, which means that the CoreHost app will not work under ArcGIS Pro 3.3 since Pro 3.3 requires .NET 8.0.  You will get this error:  
> CoreHostTest31Build C:\Data\FeatureTest\FeatureTest.gdb  
Could not load file or assembly 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.The system cannot find the file specified.  
You can see that the CoreHost app is trying to load.NET 8.0 because ArcGIS Pro 3.3 requires.NET 8.0.However, the CoreHost app is bound to .NET 6.0 and hence the loading of.NET 8.0 fails.  
This problem can be fixed by updating the.NET target framework version as a parameter of the dotnet command line tool:   
For ArcGIS Pro 3.0, 3.1, 3.2 .NET 6.0 is required and the CoreHost dll can be called using the following command line:  
"C:\Program Files\dotnet\dotnet.exe" exec --fx-version "6.0.30" CoreHostTest31Build.dll and for ArcGIS Pro 3.3 and later .NET 8.0 is required and the CoreHost dll can be called using the following command line:  
"C:\Program Files\dotnet\dotnet.exe" exec --fx-version "8.0.5" CoreHostTest31Build.dll the exact version of available .NET installations has to be found because the --fx-version parameter requires an exact version of .NET.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
