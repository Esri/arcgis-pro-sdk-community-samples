## TasksSDK

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates the methods available in the Tasks API.  The methods provide the following functionality  
  
1. Open a task file (.esriTasks)  
1. Close a specified task item  
1. Export a specified task item to an .esriTasks file  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  6/27/2019
ArcGIS Pro:            2.4
Visual Studio:         2017, 2019
.NET Target Framework: 4.6.1
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
1. Open this solution in Visual Studio  
1. Save the Project Exploration Tasks.esriTasks file in this solution to a location on your disk.  
1. Open the OpenTask.cs file and change the taskFile variable to be the location where you saved the esriTasks file.  
1. Open the ExportTask.cs file and modify the exportFolder variable if you wish the task item to be exported to a different location.    
1. Open the GetTaskItemInfo.cs file and modify the taskFile variable to be the location where you saved the esriTasks file.  
1. Click the build menu and select Build Solution.    
1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.    
1. Open any project - it can be an existing project containing data or a new empty project.  
1. Click on the Add-in tab and see that 4 buttons are added to a Tasks group.  
1. The Open Task button will open the.esriTasks file, add it to the project and load it into the Tasks pane.     
The Export Task button will export a project task item to an .esriTasks file.    
The Close Task button will close the task item loaded by the Open Task button.It will also remove it from the project.      
The Task Info button will retrieve the task information from either a project task item or an.esriTasks file.  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
