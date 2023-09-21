## Shortcuts

<!-- TODO: Write a brief abstract explaining this sample -->
 This sample shows how to display a dynamic context menu. The sample also have an embeddabel control on the map that allows you pick the layer you want to select.     
 When you select features with this tool, the oids of the features will be displayed in a dynamic context menu.   
 When you click one of the OIDs in this context menu, that feature will flash on the Map View and display a pop-up.   
   


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  09/21/2023
ArcGIS Pro:            3.2
Visual Studio:         2022
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
 1. In Visual Studio click the Build menu. Then select Build Solution.  
 1. Click Start button to open ArcGIS Pro.  
 1. ArcGIS Pro will open.  
 1. Press "h" to invoke the Accelerator.A Message will appear.
 1. Open a map view. 
 1. Press "k" to invoke a shortcut targeting the map view.
 1. With the map view still activated, press "Shift + j" to trigger a shortcut that opens a Sample DockPane.
 1. With the DockPane activated, press "a" - this will trigger a keyCommanmd shortcut on the DockPane.
 1. Click on the Add-In ribbon tab and then click on the "Open SamplePane" button in the Shortcuts Sample group. This will open a sample Pane.
 1. With the SamplePane activated, click the "StateAButton" in the Shortcuts Sample group. This will satisfy ConditionA which will allow invocation of a conditional shortcut.
 1. With the SamplePane activated, press "l" - this will invoke the conditional shortcut.
 1. Dismiss the message and, with the SamplePane still activated, press "r" - this will trigger a keyCommanmd shortcut on the Pane.
 1. Activate the map view - this will enable the "Open Sample Tool" button.
 1. Click the button to simulate tool activation. Press "n" to invoke a shortcut targeted at the tool.
![condition_shortcut](https://github.com/medina-e/arcgis-pro-sdk-community-samples/assets/126819295/66dc7277-3243-4d4b-9a4b-509a0b2125f2)


   


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
