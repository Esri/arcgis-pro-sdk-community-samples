## QueryGraphicBoundary

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrates the use of the BasicFeatureLayer QueryDrawingOutline(...) API methods  
Various cartographic workflows require updating and replacing existing feature masks to accomodate edits to the underlying (masked) features <br/>The <b>UpdateOutline</b> tool uses QueryDrawingOutline to generate the feature outline geometry. The "PostProcess" logic in the Module1 class replicates the post process logic of the Geoprocessing   
<a href="https://pro.arcgis.com/en/pro-app/tool-reference/cartography/feature-outline-masks.htm"> Feature Outline Masks (Cartography)</a> tool. The sample works with any point, line, polygon, or annotation feature dataset  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Geometry
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  05/06/2025
ArcGIS Pro:            3.5
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
4. In Visual Studio click the Build menu. Then select Build Solution.  
5. Launch the debugger to open ArcGIS Pro.  
6. ArcGIS Pro will open, select the FeatureTest.aprx project  
7. Click on the 'Test Mask' tab.    
![UI](Screenshots/Screen1.png)  
8. Change the type of Mask and a Margin before clicking on the 'Update Outline Tool' button.  
9. Click on a map feature to select that map feature for the Outline drawing.  
![UI](Screenshots/Screen2.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
