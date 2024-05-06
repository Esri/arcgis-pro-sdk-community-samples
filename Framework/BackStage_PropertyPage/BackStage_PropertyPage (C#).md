## BackStage_PropertyPage

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates how to   
  
1. add a new backstage item    
2. add property sheet items into the Options property pages  
3. save and restore project settings  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  04/04/2024
ArcGIS Pro:            3.3
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
Backstage items can be either a tab or a button. As per other controls they have a reference in the config.daml file.  However they are different from other controls  in that they are not children of the module tag - they are children of the backstage tag. This sample shows how to add a new tab following the MVVM pattern.   
The tab can be positioned using the "insert" and "placeWith" attributes in the config.daml.  The SampleBackstageTabView xaml file uses ArcGIS Pro styles to  allow the custom tab to look those those in the existing application.  
  
Property sheets are used to capture settings. They can be either project or application settings. You can insert your custom property sheets into the existing Options property sheets which are displayed from the backstage Options tab.  This is achieved in the config.daml by using the updateSheet xml tag and specifying the  esri_core_optionsPropertySheet id.  Use the group attribute on the insertPage tag to specify whether your view/viewmodel represents project or application settings.   
This sample has an example of both project and application settings, including illustrating how these settings can be saved.   
  
Modules can write out their own set of properties when a project is saved. Correspondingly, modules can read their own settings when a project is opened.  The module contains two methods OnReadStateAsync and OnWriteStateAsync which should be overriden to read and write module specific settings or properties.   
  
  
1. Open this solution in Visual Studio.
2. Click the build menu and select Build Solution.  
3. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.  
4. Open any project - it can be an existing project containing data or a new empty project.  
5. Click the project tab.  See that there is a new Sample Tab item in the backstage.  Select it and it will show the new backstage tab.  
6. Click the project tab and select the Options backstage item.  The options property page will display.  
7. See that there is a Sample Project Settings under Project and a Sample App Settings under Application.   
8. Change the project settings and application settings.   
9. Save the project.  
10. Open another project (or create new); return to the Project|Options|Sample Project Settings and see that the settings have been reset.  
11. Open the project from step4; return to the Project|Options|Sample Project Settings and see that the settings have been restored.  
![UI](Screenshots/Screen.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
