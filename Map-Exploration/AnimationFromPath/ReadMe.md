## AnimationFromPath

<!-- TODO: Write a brief abstract explaining this sample -->
This sample allows creation of animation from path. A couple of different options have been proovided including View Along, Top-Down, Face-Target etc.  
The add-in also allows for setting Z-Offset, Duration and Custom Pitch.  
The Set Target tool can be used to specify a target that the camera will always face in the created animation. Set-target tool is used with the Face-Target view mode.  
In addition to the view you would like, the add-in also provides three different options for creating the keyframes. You can choose to:  
- Keyframes along path - creates fewer keyframes but tries to keep you on the path    while avoiding sharp turns at corners  
- Keyframes every N seconds - creates a keyframe at the time-spacing specified  
- Keyframes only at vertices - creates a keyframe at each line vertex  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  11/01/2021
ArcGIS Pro:            2.9
Visual Studio:         2017, 2019
.NET Target Framework: 4.8
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
1. Open a scene or map view and a line feature class  
1. Select a line feature  
1. On the ADD-IN tab choose options under Animation from Path group and create keyframes  
NOTE - the selected line geometry is used for creating the keyframes. This means that  for a 2D line feature, the keyframes will be created at zero height + any Z-Offset you  specified in the options on the ADD-IN tab  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
