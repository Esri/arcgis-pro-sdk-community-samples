<!-- TODO: Write a brief abstract explaining this sample -->
  
This sample demonstrates how to add operations onto the undo/redo stack.    ArcGIS Pro contains multiple undo/redo stacks.  Generally one exists for each dockpane.  
The undo/redo stack is exposed using the OperationManager property on the Dockpane.  Custom operations (inheriting from   
ArcGIS.Desktop.Framework.Contracts.Operations) are pushed onto the OperationManager.  
  

<a href="http://pro.arcgis.com/en/pro-app/beta/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:      C#
Subject:       $subject$
Contracts:     Button, Dock Pane
Contributor:   ArcGIS Pro SDK Team, arcgisprosdk@esri.com
Organization:  esri
Date:          10/22/2014 5:34:03 AM, 2014
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
4. Navigate to the Add-Ins tab  
5. Click the Show Sample DockPane button in the Undo_Redo group.  The Sample dockpane should be displayed  
6. Use the Fixed Zoom In and Fixed Zoom Out buttons to see zoom in and zoom out operations added to the undo stack for the sample dockpane.  
7. Use the Undo/Redo/Remove Operation/Clear All Operations buttons to manipulate the undo stack.  
  

[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​

<p align = center><img src="https://github.com/Esri/arcgis-pro-sdk/wiki/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.1 SDK for Microsoft .NET Framework (Beta)</b>
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk/api-reference/" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#system-requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#download) |  <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
