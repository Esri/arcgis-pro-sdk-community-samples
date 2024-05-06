## SymbolControls

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrate how to work with the SymbolPicker and SymbolSearcher controls.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Authoring
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  04/04/2024
ArcGIS Pro:            3.3
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
2. Launch the debugger to open ArcGIS Pro.  
3. Open any ArcGIS Pro project that contains feature layers and graphics layers. Activate the map view with the feature layers.  
4. In the Add-In tab, click the Show Symbols button. This will display the "View and Apply Symbols" dockpane.  
5. In the Style Item Types combo box, you can pick Points, Line, Polygon style item types to search and view.  
6. In the Symbol Searcher control, you can add Search strings such as circle, ship, etc. The style items with the search string in their names will be displayed in the Symbol Picker control below.  
7. By default, the Symbol Controls allow you to display the symbols in the entire project. Filter options available are: All Styles, Project Styles,ArcGIS 2D, ArcGIS 3D, ArcGIS Colors, etc. Using the combo box to the right of the Symbol Searcher control, you can set this filter.  
8. Using the "Pick StyleX File" button, you can add your own .stylX file to the project and view style items in it.  
9. Using this custom dockpane, you can change the symbology for any of the layers in the active map.  
#### Change Feature Layer Symbology  
10. In the Select Layer combo box, pick any feature layer that is rendered with a "Single Symbol".  
11. The "Style Item Type" combo box will automatically select the shape type of the selected feature layer.  The Symbol picker control will display all the symbols for that style item type.  
12. Select a symbol in the symbol picker control.  
13. Click the Apply Symbology button to apply the selected symbol to the feature layer.  
#### Change Graphic elements Symbology  
14. In the Select Layer combo box, pick any GraphicsLayer in the Active map.  
15. Using the Graphics selection tool, select any graphics. Note: Selected Graphics need to have the same shape type. So you have to select all point graphics, for example.  
16. The "Style Item Type" combo box will automatically select the shape type of the selected graphics in the Graphics layer.  The Symbol picker control will display all the symbols for that style item type.  
17. Select a symbol in the symbol picker control.  
18. Click the Apply Symbology button to apply the selected symbol to the selected graphics.  
![UI](screenshots/SymbolViewer.png)    
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
