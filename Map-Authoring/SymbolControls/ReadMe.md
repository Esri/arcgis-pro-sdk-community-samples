## SymbolControls

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrate how to work with the SymbolPicker and SymbolSearcher controls.  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Authoring
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  11/01/2021
ArcGIS Pro:            2.9
Visual Studio:         2019
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
1. Open any ArcGIS Pro project that contains feature layers and graphics layers. Activate the map view with the feature layers.  
1. In the Add-In tab, click the Show Symbols button. This will display the "View and Apply Symbols" dockpane.  
1. In the Style Item Types combo box, you can pick Points, Line, Polygon style item types to search and view.  
1. In the Symbol Searcher control, you can add Search strings such as circle, ship, etc. The style items with the search string in their names will be displayed in the Symbol Picker control below.  
1. By default, the Symbol Controls allow you to display the symbols in the entire project. Filter options available are: All Styles, Project Styles,ArcGIS 2D, ArcGIS 3D, ArcGIS Colors, etc. Using the combo box to the right of the Symbol Searcher control, you can set this filter.  
1. Using the "Pick StyleX File" button, you can add your own .stylX file to the project and view style items in it.  
1. Using this custom dockpane, you can change the symbology for any of the layers in the active map.  
#### Change Feature Layer Symbology  
  
1. In the Select Layer combo box, pick any feature layer that is rendered with a "Single Symbol".  
1. The "Style Item Type" combo box will automatically select the shape type of the selected feature layer.  The Symbol picker control will display all the symbols for that style item type.  
1. Select a symbol in the symbol picker control.  
1. Click the Apply Symbology button to apply the selected symbol to the feature layer.  
#### Change Graphic elements Symbology  
  
1. In the Select Layer combo box, pick any GraphicsLayer in the Active map.  
1. Using the Graphics selection tool, select any graphics. Note: Selected Graphics need to have the same shape type. So you have to select all point graphics, for example.  
1. The "Style Item Type" combo box will automatically select the shape type of the selected graphics in the Graphics layer.  The Symbol picker control will display all the symbols for that style item type.  
1. Select a symbol in the symbol picker control.  
1. Click the Apply Symbology button to apply the selected symbol to the selected graphics.  
![UI](screenshots/SymbolViewer.png)    
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
