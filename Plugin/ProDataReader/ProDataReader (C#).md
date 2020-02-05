## ProDataReader

<!-- TODO: Write a brief abstract explaining this sample -->
ProDataReader implements two plugin datasources to allowing Pro viewing of the following formats:  
- Jpg photos with GPS metadata: smart phone and digital cameras have the option to capture GPS information when a photo is taken.  ProJpgPluginDatasource allows to access these GPS enable photos as a read-only feature class.  
- Gpx data: GPX (the GPS eXchange Format) is a data format for exchanging GPS data between programs and implemented by many GPS tracking devices. ProGpxPluginDatasource allows to access Gpx files as a read-only feature class.   
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  7/17/2019
ArcGIS Pro:            2.5
Visual Studio:         2019
.NET Target Framework: 4.8
```

## Resources

* [API Reference online](https://pro.arcgis.com/en/pro-app/sdk/api-reference)
* <a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">ArcGIS Pro SDK for .NET (pro.arcgis.com)</a>
* [arcgis-pro-sdk-community-samples](https://github.com/Esri/arcgis-pro-sdk-community-samples)
* [ArcGIS Pro DAML ID Reference](https://github.com/Esri/arcgis-pro-sdk/wiki/ArcGIS-Pro-DAML-ID-Reference)
* [FAQ](https://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)
* [ArcGIS Pro SDK icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/2.4.0.19948)

![ArcGIS Pro SDK for .NET Icons](https://Esri.github.io/arcgis-pro-sdk/images/Home/Image-of-icons.png  "ArcGIS Pro SDK Icons")

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [repo releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)  
1. Make sure that the Sample data is unzipped in c:\data   
1. The data used in this sample is located in this folder 'C:\Data\PluginData' and 'C:\Data\PluginDataLinkTo'  
1. In Visual Studio click the Build menu. Then select Build Solution.  
1. Click Start button to open ArcGIS Pro.  
1. In ArcGIS Pro open this project: 'C:\Data\PluginDataLinkTo\TestPlugin\TestPlugin.aprx'  
1. Open the Catalog Dockpane and open Folder Connection to drill down to this folder: 'PluginDataLinkTo'  
![UI](Screenshots/Screen1.png)    
  
1. The folder 'PluginDataLinkTo' is linked to 'C:\Data\PluginDataLinkTo' this folder contains a file called 'LinkToCustomPluginData.xlnk' with a file extension '.xlnk' that triggers the 'ProDataProjectItem' Custom Project Item class. 'LinkToCustomPluginData.xlnk' in turn contains all the folder paths which are scanned and processed by the Custom Project Item class.  
1. Open the 'LinkToCustomPluginData.xlnk' folder to find various GPX and JPG file data sources.   
![UI](Screenshots/Screen2.png)    
  
1. In the source code look at the ProMdbProjectItem class, which is used to implement the LinkToCustomPluginData.xlnk node in the Catalog dockpane.  
![UI](Screenshots/Screen3.png)    
  
1. Under the 'LinkToCustomPluginData.xlnk' node you can see '2019-March-31-Hike' which represents a line feature class (GPS Track) and 'Berlin-Devsummit-17' which represents a point feature datasets containing the point location where the photo was taken.  In source code, the ProDataSubItem class is used to prepresent each node in the catalog browser.  
1. Right clicking on any of the feature classes or the 'MiscPictures Jpeg Images' image folder node allows the feature class(es) to be added to the current map.  In the source code this is done in the AddToCurrentMap button class.  
1. After you add an item to the current map, the ProPluginDatasource plug-in is used to convert the source data to a feature class that can be added to a map and displayed as an attribute table.  
![UI](Screenshots/Screen5.png)   
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
