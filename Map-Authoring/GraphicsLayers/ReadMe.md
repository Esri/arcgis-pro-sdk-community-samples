## GraphicsLayers

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrates working with graphics layers and graphic elements in a map.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Authoring
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  02/22/2023
ArcGIS Pro:            3.1
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
1. Launch the debugger to open ArcGIS Pro.   
1. Open any project that contains a Map.  
1. You will need a Graphics Layer in the map to work with this sample.  If the map doesn't have a Graphics Layer, click the Map tab and use the Add Graphics layer button to insert a new layer in the map.  
1. Notice the Graphics Example tab that appears on the Pro ribbon when a Graphics Layer exists in the map.  
![UI](screenshots/GraphicsExample.png)  
  
1. This tab has various controls that demonstrates common Graphic Element functionalities such as:  
     * Create Graphics layer  
     * Create Graphic elements (Text, point, line, polygon and image)  
     * Clipboard  
     * Modify graphic element symbology  
     * Select graphic elements  
     * Group, Order and Align graphic elements.  
     * Move Graphic elements  
  
1. **Create Graphics Layer:** To create a new graphics layer, click the New graphics layer button. A new graphics layer will appear on the TOC.  
1. **Create Graphic elements:** The Create Graphic elements gallery has a collection of tools to add Text,   
Shape and Image elements to your map. In order to add an element, select a graphic layer in the map TOC.   
The graphic element gets added to the selected layer in the Map TOC.  
  
1. **Clipboard:** The **Copy** button copies the selected elements to the clipboard.  The **Paste** button pastes the elements in the clipboard to the selected Graphics Layer in the TOC. The **Paste into Group** button allows you to paste the selected graphics into a Group element.  
1. **Select graphic elements:** The Select tool palette provides you with tools to select graphic elements.   
You can select using a rectangle and a lasso. There is also a tool that allows you select only text graphic elements that lie within a selection rectangle. Buttons to Select all graphics, Clear selection and Delete Graphics are also provided.  
  
1. **Modify graphic element symbology:** You can change the selected Text element's font, size, etc using the text symbol properties controls. Similarly, you can change the selected point, line or polygon graphic elements symbology.  
1. **Group, Order and Align graphic elements:** When you select multiple symbols from the same graphics layer, you can group them using the Group button. Grouped graphics can be ungrouped using the Un-group button. The "Select graphics to group" tool allows you to select graphics using a rectangle and then groups them.   
You can change the Z-Order of graphics using the Bring to Front and Send Back buttons.   
There are two alignment tools that allows you to select graphics to align them to the left or to the top.  
  
1. **Move Graphics:** Right click the selected Graphic element. In the context menu, click the Move Graphic option. This will move the anchor point of the selected graphic.  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
