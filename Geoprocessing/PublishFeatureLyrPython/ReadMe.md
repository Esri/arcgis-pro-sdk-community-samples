## PublishFeatureLyrPython

<!-- TODO: Write a brief abstract explaining this sample -->
Create an ArcGIS Pro SDK Add-in that has Python scripts embedded.  This adds your Add-in’s Python script toolbox to Pro’s GP toolboxes.  
Once the Add-in is registered in ArcGIS Pro, you can run the Add-in’s Python scripts through ArcGIS Pro’s ‘GP Toolboxes’.  
This sample also demonstrates how to call the embedded Python script programmatically with parameters  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
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
1. This solution file includes an example python script named MySharing.pyt which is included as 'Content' (Build action) in the add-in.
2. The python script is stored in the .\Toolboxes\toolboxes folder and when this add-in is loaded in ArcGIS Pro the python script is available a script tool under the ArcGIS Pro Geoprocessing toolbox.  
![UI](Screenshots/Screen1.png)  
3. You can run the python script from the Geoprocessing toolbox as shown here.  
![UI](Screenshots/Screen2.png)  
![UI](Screenshots/Screen3.png)  
![UI](Screenshots/Screen4.png)  
![UI](Screenshots/Screen5.png)  
![UI](Screenshots/Screen6.png)  
4. You can also run the python script from code as implemented in the 'Publish as Web Layer' button under the 'Custom Publishing' tab (RunPyScriptButton.cs).  
![UI](Screenshots/Screen7.png)  
![UI](Screenshots/Screen8.png)  
![UI](Screenshots/Screen9.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
