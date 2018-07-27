## SOEWithPropertiesTest

<!-- TODO: Write a brief abstract explaining this sample -->
You can extend ArcGIS Server map and image services (including map and image service extensions, such as feature services) with custom logic that can be executed in ArcGIS clients. There are two ways to extend these service types: Server object extensions (SOEs) and Server object interceptors (SOIs).  
SOEs are appropriate if you want to create new service operations to extend the base functionality of map and image services(including map and image service extensions, such as feature services).   
For example, while publishing a map image layer to federated servers, the checkboxes left for clients to pick among Feature Access, WMS, or network/utilities as extensions, are the built-in SOEs.This SDK sample would add to the diversity and flexibility of the SOE capabilities, and allow clients to customize their own SOEs.  
    
In summary, the sample would support these following features – a. Add a customized SOE to Pro(when Pro detects the target server has customized extensions, display these SOEs at the configuration pane) b. Allow the customized SOE to analyze the publishing content, and decide if the SOE should be enabled or not c. Allow the customized SOE to provide custom UI that will be integrated into the sharing UI so that individual, SOE specific parameters can be set.  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Sharing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  6/11/2018
ArcGIS Pro:            2.2
Visual Studio:         2017
.NET Target Framework: 4.6.1
```

## Resources

* [API Reference online](http://pro.arcgis.com/en/pro-app/sdk/api-reference)
* <a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">ArcGIS Pro SDK for .NET (pro.arcgis.com)</a>
* [arcgis-pro-sdk-community-samples](http://github.com/Esri/arcgis-pro-sdk-community-samples)
* [ArcGISPro Registry Keys](http://github.com/Esri/arcgis-pro-sdk/wiki/ArcGIS-Pro-Registry-Keys)
* [FAQ](http://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)
* [ArcGIS Pro SDK icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/1.4.0.7198)
* [ProConcepts: ArcGIS Pro Add in Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProConcepts-ArcGIS-Pro-Add-in-Samples)

![ArcGIS Pro SDK for .NET Icons](https://esri.github.io/arcgis-pro-sdk/images/Home/Image-of-icons.png "ArcGIS Pro SDK Icons")

* [ProSnippets: 2.0 Migration](http://github.com/Esri/arcgis-pro-sdk/wiki/ProSnippets-Migrating-to-2.0)  
* [ProSnippets: 2.0 Migration Samples](http://github.com/Esri/arcgis-pro-sdk/wiki/ProSnippets-2.0-Migration-Samples)  
* [ProConcepts: 2.0 Migration](http://github.com/Esri/arcgis-pro-sdk/wiki/ProConcepts-2.0-Migration-Guide)  

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [repo releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu. Then select Build Solution.  
1. Click Start button to open ArcGIS Pro.  
1. ArcGIS Pro will open.   
1. Open a map inside an existing project, and get connected to a 10.6.1 portal (sign in and set as active)  
1. On the sharing ribbon, in the “Share As” group, select “Publish layer”   
1. When the sharing pane pops out, fill out the required information for the service, and select the “Layer Type” as Map Image Layer;  
1. Switch from General tab to configuration tab.If your federated server has already installed the customized SOEs, and you have installed the Add-in successfully, then you will see customized extensions being display as in Fig 2. If not, you might only see Fig 1.  
1. Check on the SOE(or SOIs) you would like to enable, then click the pencil icon to edit its properties.See in Fig 3 if you enabled the SpatialQueryREST SOE, and clicked the pencil button for editing.  
1. After enabling desired SOEs, click on the back arrow, to go back the general tab, and click “Publish” to start sharing process.  
1. If successfully published, you will be able to see the properties page on the server e.g. \<server adaptor url\>/rest/services/\<service name\>/MapServer/exts/\<SOE name\>    
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="http://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
