## ConstructMarkerFromFont

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrate how to create a CIMMarker from a font file and use it to render a point feature layer.  
The marker is then added to a personal style item in the current project.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Authoring
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
1. In Visual Studio click the Build menu. Then select Build Solution.
2. This solution is using the **DotNetProjects.Extended.Wpf.Toolkit**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package DotNetProjects.Extended.Wpf.Toolkit".  
3. Launch the debugger to open ArcGIS Pro.  
4. Open any project file that contains a point feature layer.   
5. Right click on the point feature layer to bring up the context menu.  
6. Select the "Construct Marker from Fonts" menu item.  
7. This will open the "Construct Marker from Fonts" dockpane.  
8. Select a font, style and size and click Apply to render the point feature layer with the selected character.  
9. If you check the "Add selected marker to a personal FontMarker style" option, the selected marker will be added to a FontMarker style in the project.  
![UI](ScreenShots/ConstructMarkerFromFonts.png)    
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
