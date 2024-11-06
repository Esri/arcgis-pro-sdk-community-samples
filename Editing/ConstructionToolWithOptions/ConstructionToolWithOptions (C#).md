## ConstructionToolWithOptions

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates how to build a construction tool with options allowing users to provide parameters at run-time.    
Two samples are included.  
The first is the BufferedLineTool.  The line sketch geoemtry is buffered by a user defined distance to create a polygon feature.  
The second sample is the CircleTool.  A user defined radius is used to create a circular arc with the point sketch geometry as the centroid.  This tool is registered with both the esri_editing_construction_polyline and esri_editing_construction_polygon categories allowing both polyline and polygon features to be created.  
The CircleTool options follows the multiple tool options pattern (implementing ToolOptionsEmbeddableControl) allowing the users to select multiple templates in the manage templates dialog and change tool option values for those templates. The BufferedLineTool sample follows the single use pattern.  
  


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
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
2. Make sure that the Sample data is unzipped in c:\data  
3. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'  
4. In Visual Studio click the Build menu.Then select Build Solution.  
5. Launch the debugger to open ArcGIS Pro.  
6. ArcGIS Pro will open, select the FeatureTest.aprx project  
7. Select the 'Edit' tab on the ArcGIS Pro ribbon and 'Create' new features  
8. On the 'Create Features' pane select the test polygon feature layer to see the 'Buffered Line' tool  
![UI](Screenshots/ConstructionToolOptions_1.png)        
9. Select the tool and see the Options page displaying the buffer distance  
![UI](Screenshots/ConstructionToolOptions_2.png)        
10. Enter a buffer distance and sketch a line.See a buffer of the sketched line used to generate a new polygon feature.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
