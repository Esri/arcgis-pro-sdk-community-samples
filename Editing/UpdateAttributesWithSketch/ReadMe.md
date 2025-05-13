## UpdateAttributesWithSketch

<!-- TODO: Write a brief abstract explaining this sample -->
This sample creates a sketch tool that can be used to update the attributes of features that it intersects. It shows an example of creating a sketch tool, finding intersecting features, updating attributes and performing the edit through an edit operation.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
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
1. Examine the code within AttributeWithSketch.cs.
2. This code finds a field named "PARCEL_ID" and replaces the value with "42".  
3. Build or debug start the sample through Visual Studio.  
4. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'  
5. Select the layer in the table of contents who's attributes you wish to update with this tool. Create a field called "PARCEL_ID" in this layer.  
6. Select the "AttributeWithSketch" tool in the Add-In Tab.  
7. Sketch a line across features in the selected layer.  
8. Features that intersect the sketch will have their attributes in the PARCEL_ID field updated.  
![UI](Screenshots/Screen.png)      
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
