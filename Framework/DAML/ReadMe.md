## DAML

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrates how to modify the Pro UI using DAML  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  06/10/2022
ArcGIS Pro:            3.0
Visual Studio:         2022
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu. Then select Build Solution.    
1. Click Start button to open ArcGIS Pro.  
1. ArcGIS Pro will open.  
1. Open any project with at least one feature layer.  
1. Notice the following customizations made to the Pro UI:  
* The Bookmarks button has been removed in the Navigate group on the Map tab  
![RemoveButton](screenshots/DeleteCoreButton.png)   
* A new button has been inserted into the Navigate group on the Map Tab  
![NewButton](screenshots/NewButtonCoreGroup.png)   
* With any Map view active, right click on a feature layer in the TOC. Notice the New Button context menu item added.  
![New Menu](screenshots/NewMenuItemInContextMenu.png)  
* Click the Project tab to access Pro's backstage.  Notice the missing Open and New project tabs.  A new tab called "Geocode" has been inserted above the Save project button.  
![New Backstage tab](screenshots/BackstageDeleteExistingTabsInsertNewTabs.png)  
* Click the Project tab to access Pro's backstage. Click the Options tab to display the Options Property Sheet.  Notice the new "Sample Project Settings" property page inserted within the Project group.  
![New Backstage tab](screenshots/PropertySheetOptionsDialog.png)  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
