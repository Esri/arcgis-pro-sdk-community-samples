## EditingTemplates

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrates creating and modifying editing templates largely using the Cartographic Information Model (CIM).   
In this example we will create and modify templates including group templates.    
We will also show how to retrieve and use templates to create features; overriding default attribute values.   
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  02/22/2023
ArcGIS Pro:            3.1
Visual Studio:         2022
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)  
1. Make sure that the Sample data is unzipped in c:\data  
1. In Visual Studio click the Build menu.Then select Build Solution.  
1. Click Start button to open ArcGIS Pro.  
1. ArcGIS Pro will open.    
1. Open the Interacting with Map project.  
1. Open the Create Features dockpane.   
1. Click on the ADD-IN TAB.    
1. Click on the *Create Template with CIM* button.     
1. A new template 'My CIM Template' will be created.  
![UI](screenshots/Templates_NewCIMTemplate.png)   
  
1. Click on the *Create Template with Extension* button.     
1. A new template 'My extension Template' will be created.  
![UI](screenshots/Templates_NewExtensionTemplate.png)   
  
1. Click on the *Modify Template with CIM* button.     
1. Activate the 'North Precinct' template and see the default construction tool is now the Right Angle Polygon tool.  
1. Click on the *Create Features* button.     
1. See 3 new Fire Station features created all with different City attribute values.  
1. Click on the *Create Group Template with CIM* button.     
1. A new template 'My Group Template' will be created.  
![UI](screenshots/Templates_GroupTemplate.png)   
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
