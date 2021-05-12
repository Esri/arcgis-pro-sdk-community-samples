## DeviceTracker

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates functionality associated with GNSS devices and snapshot location data. In order to use this sample you must either have a GNSS device or have configured an appropriate simulator.   
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  5/12/2021
ArcGIS Pro:            2.8
Visual Studio:         2019
.NET Target Framework: 4.8
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Start your GNSS device or simulator.   
1. In Visual Studio, build the solution.  
1. Click Start button to open ArcGIS Pro.  
1. In Pro start with a new map.  Add a graphics layer to your map. (Map tab, Add Graphics Layer button).   
1. Open the Add-In tab. And click the "Show GNSS Properties" button. The GNSS Properties dockpane appears.   
1. If your device is not currently connected, the Current Properties section should be blank.   
1. Under "Input Parameters", enter the communicating comPort for your device. The comPort is the mandatory property required to connect. Other parameters such as baud rate, antenna height, accuracy are optional.    
1. Click the "Open" button. The "Current Parameters" should be populated and your device should be connected.  
1. You can update your connection parameters (such as baud rate or antenna height) by entering values in the Input Properties and clicking the "Update" button.  
1. You can close the connection by clicking the 'Close' button.   
1. With your device connected, enable the device by clicking the "Enable/Disable DeviceSource" button.   
1. Move to the GNSS Location tab.   
1. Click the "Zoom/Pan to Location" button.  You should zoom to where your device is tracking.   
1. Click "Show Location" and other Location Options then "Update Options" button to configure the device location map options.   
1. To add the most recent location to the graphics layer, click the "Add Last Location" button.   
1. To connect to the snapshot events, click the "Connect" button under Snapshot Events.  The event and location data will be logged to the dockPane.   
1. Wait until a number of snapshot events have been received.  Click the "Add Polyline" button.  Recent snapshot data will be added to the graphics layer as a polyline.   
1. Disconnect from the snapshot events by clicking the "Disconnect" button.   
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
