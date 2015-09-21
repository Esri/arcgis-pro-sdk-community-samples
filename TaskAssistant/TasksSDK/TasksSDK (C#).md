##TasksSDK

<!-- TODO: Write a brief abstract explaining this sample -->
 This sample illustrates the methods available in the Tasks API.  The methods provide the following functionality  
   1. Open a task file (.esriTasks)  
   2. Close a specified task item  
   3. Export a specified task item to an .esriTasks file  
   


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:      C#
Subject:       Framework
Contributor:   ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:  Esri, http://www.esri.com
Date:          9/21/2015
ArcGIS Pro:    1.1
Visual Studio: 2013, 2015
```

##Resources

* [API Reference online](http://pro.arcgis.com/en/pro-app/sdk/api-reference)
* <a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">ArcGIS Pro SDK for .NET (pro.arcgis.com)</a>
* [arcgis-pro-sdk-community-samples](http://github.com/Esri/arcgis-pro-sdk-community-samples)
* [FAQ](http://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)
* [ArcGIS Pro SDK icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/1.1.0.3308)
* [ProConcepts: ArcGIS Pro Add in Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProConcepts-ArcGIS-Pro-Add-in-Samples)
* [Sample data for ArcGIS Pro SDK Community Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases)

##How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Open this solution in Visual Studio 2013.    
2. Save the Project Exploration Tasks.esriTasks file in this solution to a location on your disk.  
3. Open the OpenTask.cs file and change the parameter of the TaskAssistantModule.OpenTaskAsync method to be the  location where you saved the esriTasks file.   
4. Open the ExportTask.cs file and modify the path parameter in the TaskAssistantModule.ExportTaskAsync method  if you wish the task item to be exported to a different location.  
5. Click the build menu and select Build Solution.  
6. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.  
7. Open any project - it can be an existing project containing data or a new empty project.  
8. Click on the Add-in tab and see that 3 buttons are added to a Tasks group.  
9. The Open Task button will open the .esriTasks file, add it to the project and load it into the Tasks pane.   
The Close Task button will close the task item loaded by the Open Task button.  It will also remove it  from the project.   The Export Task button will export a project task item to an .esriTasks file.   
   


[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​


<p align = center><img src="http://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.1 SDK for Microsoft .NET Framework</b>
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](#requirements) | [Download](#download) |  <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
