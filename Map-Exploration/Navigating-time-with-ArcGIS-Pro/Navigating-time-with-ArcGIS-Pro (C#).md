<!-- TODO: Write a brief abstract explaining this sample -->
  
This sample provides a new tab and controls that allow you to set the time in the map view, step through time, and navigate between time enabled bookmarks in the map.  
  

<a href="http://pro.arcgis.com/en/pro-app/beta/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:      C#
Subject:       Map-Exploration
Contracts:     Button, Gallery
Contributor:   ArcGIS Pro SDK Team, arcgisprosdk@esri.com
Organization:  >Esri, http://www.esri.com
Date:          9/3/2014 4:45:39 PM, 2014
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
  
1. In Visual Studio click the Build menu. Then select Build Solution.  
2. Click Start button to open ArcGIS Pro.  
3. ArcGIS Pro will open.   
4. Open a map view that contains time aware data. Click on the new Navigation tab within the Time tab group on the ribbon.    
![UI](screenshots/UICommands.png)    
5. Within this tab there are 3 groups that provide functionality to navigate through time.  
6. The Map Time group provides two date picker controls to set the start and end time in the map.  
7. The Time Step group provides two combo boxes to set the time step interval. The previous and next button can be used to offset the map time forward or back by the specified time step interval.  
8. The Bookmarks group provides a gallery of time enabled bookmarks for the map. Clicking a bookmark in the gallery will zoom the map to that location and time.   
It also provides play, previous and next buttons that can be used to navigate between the time enabled bookmarks.   
These commands are only enabled when there are at least 2 bookmarks in the map. Finally it provides a slider that can be used to set how quickly to move between bookmarks during playback.  
  

[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​

<p align = center><img src="https://github.com/Esri/arcgis-pro-sdk/wiki/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.1 SDK for Microsoft .NET Framework (Beta)</b>
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk/api-reference/" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#system-requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#download) |  <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
