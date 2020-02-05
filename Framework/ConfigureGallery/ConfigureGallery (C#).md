## ConfigureGallery

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to use custom categories to configure buttons and tools into a gallery on Pro's ribbon.  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  7/17/2019
ArcGIS Pro:            2.5
Visual Studio:         2017, 2019
.NET Target Framework: 4.8
```

## Resources

* [API Reference online](https://pro.arcgis.com/en/pro-app/sdk/api-reference)
* <a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">ArcGIS Pro SDK for .NET (pro.arcgis.com)</a>
* [arcgis-pro-sdk-community-samples](https://github.com/Esri/arcgis-pro-sdk-community-samples)
* [ArcGIS Pro DAML ID Reference](https://github.com/Esri/arcgis-pro-sdk/wiki/ArcGIS-Pro-DAML-ID-Reference)
* [FAQ](https://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)
* [ArcGIS Pro SDK icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/2.4.0.19948)

![ArcGIS Pro SDK for .NET Icons](https://Esri.github.io/arcgis-pro-sdk/images/Home/Image-of-icons.png  "ArcGIS Pro SDK Icons")

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [repo releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu. Then select Build Solution.  
1. Click Start button to open ArcGIS Pro.  
1. ArcGIS Pro will open.   
1. Open any project. Click on the Configure Gallery tab on the ribbon.  
1. Within this tab there is an inline gallery that hosts a collection of controls. Controls that are registered within the AcmeCustom_AnalysisTools custom category are displayed in this gallery.  
Controls are registered to a category using DAML. In Config.daml, here is a code snippet that registers a control with the AcmeCustom_AnalysisTools category:  
```xml
 <button id="ConfigureGallery_Buttons_AcmeCommand1" caption="Command 1" 
           categoryRefID="AcmeCustom_AnalysisTools" 
           className="ConfigureGallery.Buttons.AcmeCommand1" ...>
     <tooltip heading = "Tooltip Heading" >
      Command 1<disabledText />
     </tooltip>
     <content version = "1.0" group="Group A" />
 </button>
```
6. This tab also contains other controls that are placed directly on the ribbon, outside the gallery. These controls are not registered with the category.  
7. New custom categories are defined in DAML. Notice the following code snippet in the config.daml that defines a new category called AcmeCustom_AnalysisTools:  
```xml
<categories>
 <!--Step 1-->
 <!--Create a new category to hold new commands in a a Gallery-->
 <insertCategory id = "AcmeCustom_AnalysisTools" ></ insertCategory >
</ categories >
```
![UI](screenshots/configuregallery.png)  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
