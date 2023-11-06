## AnimationFromPath

<!-- TODO: Write a brief abstract explaining this sample -->
This sample allows creation of animation from path. A couple of different options have been proovided including View Along, Top-Down, Face-Target etc.  
The add-in also allows for setting Z-Offset, Duration and Custom Pitch.  
The Set Target tool can be used to specify a target that the camera will always face in the created animation. Set-target tool is used with the Face-Target view mode.  
In addition to the view you would like, the add-in also provides three different options for creating the keyframes. You can choose to:  
- Keyframes along path - creates fewer keyframes but tries to keep you on the path    while avoiding sharp turns at corners  
- Keyframes every N seconds - creates a keyframe at the time-spacing specified  
- Keyframes only at vertices - creates a keyframe at each line vertex  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  10/01/2023
ArcGIS Pro:            3.2
Visual Studio:         2022
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu. Then select Build Solution.
2. Launch the debuuger to open ArcGIS Pro.  
3. Open a scene or map view and a line feature class  
4. Select a line feature  
5. On the Add-In tab choose options under Animation from Path group and create keyframes  
Note: The selected line geometry is used for creating the keyframes. This means that  for a 2D line feature, the keyframes will be created at zero height + any Z-Offset you  specified in the options on the Add-In tab  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
