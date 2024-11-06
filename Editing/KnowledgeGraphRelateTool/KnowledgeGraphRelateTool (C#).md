## KnowledgeGraphRelateTool

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates how to build a set of KnowledgeGraph relate records using the EditOperation object.   
The sketch tool displays a UI on the overlay using the OverlayControlID property of the MapTool.   
The UI requires the user to specify the source and destination entity types along with the relate type and  the relate direction.   
The tool uses the SketchTip property to direct the user to identify a single source entity. Once a source entity has been found, the SketchTip is updated to direct the user to identify destination entities.   
Once destination entities have been found, the edit is performed for each source and destination entity pair.   
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  11/04/2024
ArcGIS Pro:            3.4
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu.Then select Build Solution.
2. Launch the debugger to open ArcGIS Pro.  
3. ArcGIS Pro will open, select a project containing a link chart with a KnowledgeGraph.   
4. Select the 'Add-In' tab on the ArcGIS Pro ribbon and activate the 'Create Relate records' tool.  
![UI](Screenshots/relateTool.png)        
5. In the UI, choose a source entity type, destination entity type, relate type from the dropdowns and  specify the relate direction.   
6. Click and drag a rectangle to identify a single source entity.   
7. Click and drag a rectangle to identify destination entities.   
8. The application will create a relate record for each of the destination entities.   
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
