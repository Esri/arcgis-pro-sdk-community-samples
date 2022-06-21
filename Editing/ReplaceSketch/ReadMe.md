## ReplaceSketch

<!-- TODO: Write a brief abstract explaining this sample -->
This sample adds the ReplaceSketch command to the sketch context menu.   
This allows you to add the shape of a line or polygon to the edit sketch by right clicking on the feature and choosing this command.  
It is equivalent to the ArcMap editing sketch context menu item.  
The replace sketch functionality is useful when you want to create a sketch from an underlying feature.  
For example you may want to split a polygon with an underlying road or stream.   
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
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
To install this add-in:  
  
1. In Visual Studio click the Build menu. Then select Build Solution.  
2. Select a polygon to split.  
3. Activate the editor split tool.  
![UI](Screenshots/Screenshot1.png)    
4. Right-click over a whole line feature that passes through the polygon  
![UI](Screenshots/Screenshot2.png)    
5. And select ReplaceSketch.  
![UI](Screenshots/Screenshot3.png)    
6. Continue or adjust the sketch as necessary then finish the sketch to use it as the splitting line.  
![UI](Screenshots/Screenshot4.png)    
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
