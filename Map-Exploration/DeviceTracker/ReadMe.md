## DeviceTracker

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates functionality associated with GNSS devices and snapshot location data. In order to use this sample you must either have a GNSS device or have configured an appropriate simulator.   
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  11/04/2024
ArcGIS Pro:            3.4
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Start your GNSS device or simulator.
2. In Visual Studio, build the solution.  
3. Launch the debugger to open ArcGIS Pro.  
4. In Pro start with a new map.  Add a graphics layer to your map. (Map tab, Add Graphics Layer button).   
5. Open the Add-In tab. And click the "Show GNSS Properties" button. The GNSS Properties dockpane appears.   
6. If your device is not currently connected, the Current Properties section should be blank.   
7. Under "Input Parameters", enter the communicating comPort for your device. The comPort is the mandatory property required to connect. Other parameters such as baud rate, antenna height, accuracy are optional.    
8. Click the "Open" button. The "Current Parameters" should be populated and your device should be connected.  
9. You can update your connection parameters (such as baud rate or antenna height) by entering values in the Input Properties and clicking the "Update" button.  
10. You can close the connection by clicking the 'Close' button.   
11. With your device connected, enable the device by clicking the "Enable/Disable DeviceSource" button.   
12. Move to the GNSS Location tab.   
13. Click the "Zoom/Pan to Location" button.  You should zoom to where your device is tracking.   
14. Click "Show Location" and other Location Options then "Update Options" button to configure the device location map options.   
15. To add the most recent location to the graphics layer, click the "Add Last Location" button.   
16. To connect to the snapshot events, click the "Connect" button under Snapshot Events.  The event and location data will be logged to the dockPane.   
17. Wait until a number of snapshot events have been received.  Click the "Add Polyline" button.  Recent snapshot data will be added to the graphics layer as a polyline.   
18. Disconnect from the snapshot events by clicking the "Disconnect" button.   
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
