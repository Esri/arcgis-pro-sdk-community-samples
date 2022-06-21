## CustomAnimation

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to create custom animations such as flying along a 3D line feature and rotating around a point of interest.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
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
1. In Visual Studio click the Build menu. Then select Build Solution.  
2. Launch the debugger to open ArcGIS Pro.  
4. With a 3D scene view active go to the Add-In tab.  
5. Select a 3D line features in the scene.  
6. Click the Follow Path button. This will create new keyframes that you can use to animate along the path.   
7. On the Animation tab click the Timeline button to see the animation timeline.  Click the Play button in the Animation tab to fly along the line.   
8. Additional options are available in the Path group on the Add-In tab to configure how the keyframes are created.  Experiment by  clicking the Clear Animation button on the Add-In tab to remove the existing keyframes, change a few of the options and click the Follow Path button to create a different set of keyframes. Play these animations to see the differences.   
9. Click the Center At tool and with the tool active click a point of interest in the view.  
10. New keyframes will be constructed that will fly around the point keeping the point you clicked at the center of the view.  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
