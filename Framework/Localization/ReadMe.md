<!-- TODO: Write a brief abstract explaining this sample -->
  
This localization sample shows how to 'Globalize' an add-in by providing an add-in with   
text translated into different languages and showing support for different regions (locale).  
A ProTutorial based on this sample is also available.  
  

<a href="http://pro.arcgis.com/en/pro-app/beta/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:      C#
Subject:       Framework
Contracts:     Button, Dock Pane
Contributor:   Wolfgang
Organization:  Acme
Date:          1/13/2015 1:31:18 PM, 2015
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
3. Open the "Add-ins tab" on the ArcGIS Pro ribbon to see the sample button and dockpane in English.  Close ArcGIS Pro.  
  
__4. Testing other languages__: in order to test other languages you need to install the proper language pack in ArcGIS Pro first.  Once you have additional languages installed you can change ArcGIS Pro's default language in the ArcGIS Pro Options dialog:   
![package](Images/Localization/ArcGISoptions.png)  
  
In order to view this sample you have to install the German language pack for ArcGIS Pro and Windows.  Change the language option to German and restart ArcGIS Pro.  Open the 'Registrierkarte Add-in' tab and verify that the language of you add-in is correct.  
![package](Images/Localization/Test4.png)  
  
Please note that the date and the currency field does not reflect he language change.  The reason is that their settings are not defined by the language but instead by the region.  
  
__5. Testing other region settings__: in order to change your region please use the Windows control panel's Region settings dialog and change the format to 'German' (please note that you might have to install a language pack for this to work):    
![package](Images/Localization/Region.png)  
  
After you change your region setting, debug your add-in (with the language setting for German still in place) and verify that the date and currency columns are now defined consistently with the region settings of Windows.   
![package](Images/Localization/Test5.png)  
  

[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​

<p align = center><img src="https://github.com/Esri/arcgis-pro-sdk/wiki/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.1 SDK for Microsoft .NET Framework (Beta)</b>
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/beta/sdk/api-reference/" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#system-requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#download) |  <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
