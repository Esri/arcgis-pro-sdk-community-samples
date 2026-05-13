## Connectivity

<!-- TODO: Write a brief abstract explaining this sample -->
This sample is used to demonstrate how to use the ArcGIS.Core.SystemCore.Connectivity class to determine the connectivity status of the machine running ArcGIS Pro. It also demonstrates how to listen for connectivity changes by subscribing to the ConnectivityChanged event on the Connectivity class.  
The results are displayed in a DockPane, which is opened by clicking the "Connectivity" button on the "Add-In" tab of the ArcGIS Pro ribbon. The DockPane displays whether the machine is a mobile device or desktop, whether it is in airplane mode, and the internet connection status. The icons and text update when connectivity changes are detected.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Connectivity
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  2/16/2026
ArcGIS Pro:            3.7
Visual Studio:         2026
Target Framework:      net10.0-windows7.0
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio, click the Build menu, then select Build Solution to build the solution and make sure there are no errors.
2. Start debugging by clicking the Start button or pressing F5. This will open ArcGIS Pro.  
3. In ArcGIS Pro, open the "Add-In" tab on the ribbon and click the "Connectivity" button to open the DockPane.  
4. Observe the connectivity status displayed in the DockPane. Try changing your connectivity status (e.g., turn on airplane mode, disconnect from the internet) and observe how the DockPane updates to reflect the changes.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
