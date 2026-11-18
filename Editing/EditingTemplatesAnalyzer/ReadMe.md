## EditingTemplatesAnalyzer

<!-- TODO: Write a brief abstract explaining this sample -->
Analyzes group editing templates for warnings and errors, lists problematic templates in a dockpane, and removes selected templates.  
  
This add-in can be used to identify group templates with warnings and errors.The add-in will report the following:  
* Group template is missing the primary feature template, or one or more secondary feature template  
* Group template may create non-visible features due to a definition query  
* Missing feature builder  
* Missing a tag or a description  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  11/01/2026
ArcGIS Pro:            3.8
Visual Studio:         2026, VS Code
Target Framework:      net10.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu. Then select Build Solution.
2. Launch the debugger to open ArcGIS Pro.  
3. Open any project that has a map with multiple line, point and polygon layers that have group templates.   
4. Open the Group Templates Inspector pane from the Add-In tab on the ribbon.  
5. Click the "Find templates" button to find all group templates in the map that are missing primary templates, secondary templates, builder, or have definition queries, or have preset builder child templates missing features, or are missing tags or description.  
6. Choose a category from "Filter results by" to see the templates that are broken for that reason.  
7. Select a template from the list, click Delete, and then confirm to delete the template. Use Undo button if you want to restore the deleted template.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
