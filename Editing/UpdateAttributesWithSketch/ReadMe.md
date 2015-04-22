<!-- TODO: Write a brief abstract explaining this sample -->
This sample creates a sketch tool that can be used to update the attributes of features that it intersects. It shows an example of creating a sketch tool,  
finding intersecting features, updating attributes and performing the edit through an edit operation.  

<a href="http://pro.arcgis.com/en/pro-app/beta/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:      C#
Subject:       Editing
Contracts:     Tool
Contributor:   ArcGIS Pro SDK Team
Organization:  Esri
Date:          4/13/2015 4:11:39 PM, 2015
ArcGIS Pro:    1.1.3059
Visual Studio: Visual Studio 12.0
```

##Resources

ArcGIS Pro SDK resources, including concepts, guides, tutorials, and snippets, will be available at ArcGIS Pro 1.1 Beta in the [arcgis-pro-sdk repository](https://github.com/esri/arcgis-pro-sdk). The [arcgis-pro-sdk-community-samples repo](https://github.com/esri/arcgis-pro-sdk-community-samples) hosts ArcGIS Pro samples that provide some guidance on how to use the new .NET API to extend ArcGIS Pro. A complete [API Reference](http://pro.arcgis.com/en/pro-app/beta/sdk/api-reference/) is also available online.

* [ProConcepts: ArcGIS Pro Add in Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProConcepts-ArcGIS-Pro-Add-in-Samples)
* [ArcGIS Pro API Reference Guide](http://pro.arcgis.com/en/pro-app/beta/sdk/api-reference/index.html)
* [arcgis-pro-sdk-community-samples](https://github.com/Esri/arcgis-pro-sdk-community-samples)
* <a href="http://pro.arcgis.com/en/pro-app/beta/sdk/" target="_blank">ArcGIS Pro SDK (pro.arcgis.com)</a>
* [FAQ](https://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)  
* [ArcGIS Pro SDK Icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/1.1.0.3068_(Beta))  
![Image-of-icons.png](https://github.com/Esri/arcgis-pro-sdk/wiki/images/Home/Image-of-icons.png "ArcGIS Pro SDK Icons")  

##Download

Download the ArcGIS Pro SDK from the [ArcGIS Pro Beta Community](http://pro.arcgis.com/en/pro-app/beta/sdk).

##How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
<para>To use this sample:</para><list type="number">  
  <item>Examine the code within AttributeWithSketch.cs.</item>  
  <item>Replace the string "PARCEL_ID" with a field name from data you will add to the application.</item>  
  <item>Build or debug start the sample through Visual Studio.</item>  
  <item>Create a new map and add some data or open an existing map with known data.</item>  
  <item>Examine the ADD-IN TAB on the ribbon, the sample should be present.</item>  
  <item>Select the layer in the table of contents who's attributes you wish to update with this tool.</item>  
  <item>Select the tool in the ADD-IN TAB.</item>  
  <item>Sketch a line across features in the selected layer.</item>  
  <item>Features that intersect the sketch will have their attributes updated.</item>  
</list>  

[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​

<p align = center><img src="https://github.com/Esri/arcgis-pro-sdk/wiki/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.1 SDK for Microsoft .NET Framework (Beta)</b>
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk/api-reference/" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#system-requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#download) |  <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
