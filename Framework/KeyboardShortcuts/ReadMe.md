## Keyboard Shortcuts

<!-- TODO: Write a brief abstract explaining this sample -->
  This sample provides an overview of the Keyboard Shortcuts framework. Examples include: Global Accelerator, keyUp shortcut, conditional shortcut, 
  Pane and DockPane shortcut via keyCommand.
   


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
 1. Open a map view. 
 1. Launch the Keyboard Shortcuts Dialog with F12. Take a moment to inspect the Shortcut Tables. 
 1. Expand the "Global" group. Scroll down to "SampleButton." This is a read-only accelerator added by this AddIn.
 1. Uncheck the "Show currently active shortcuts" checkbox at the top of the dialog.
 1. Expand the Custom group. This is an example of a Custom category added in this sample.
 1. Close the Keyboard Shortcuts Dialog.
 1. Press `h` to invoke the Accelerator command. A message will appear.
 1. Press `k` to invoke a shortcut targeting the map view.
 1. With the map view still activated, press `Shift + j` to trigger a shortcut that opens a Sample DockPane.
 1. With the DockPane activated, press `a` - this will trigger a keyCommanmd shortcut on the DockPane.
 1. Click on the Add-In ribbon tab and then click on the "Open SamplePane" button in the Shortcuts Sample group. This will open a sample Pane.
 1. With the SamplePane activated, click the "StateAButton" in the Shortcuts Sample group. This will satisfy ConditionA which will allow invocation of a conditional shortcut.
 1. With the SamplePane activated, press `l` - this will invoke the conditional shortcut.
 1. Dismiss the message and, with the SamplePane still activated, press `r` - this will trigger a keyCommanmd shortcut on the Pane.
 1. Activate the map view - this will enable the "Open Sample Tool" button.
 1. Click the button to simulate tool activation. Press `n` to invoke a shortcut targeted at the tool.
 ![UI](Screenshots/ShortcutDialog.png)  
![UI](Screenshots/ConditionShortcut.png)  


   


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>