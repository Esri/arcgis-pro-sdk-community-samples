## FilteredFindPathAnalysis

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrates how to use the Knowledge Graph API to do Filtered Find Path Analysis.  
  


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
Target Framework:      net10.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
**Sample Data:**  
This sample uses the freely available[Esri Developer BumbleBees Knowledge Graph] (https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/) dataset included with the[Query Knowledge Graphs](https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/) javascript sample.  
  
  
1. Open ArcGIS Pro and add a portal connection to the [sampleserver7 arcgisonline portal](https://sampleserver7.arcgisonline.com/portal) using the following URL `https://sampleserver7.arcgisonline.com/portal`. At the time of this writing, the username and pwd are:
User: viewer01  
Pwd:  I68VGU
nMurF  
If the password has changed, check the[Query Knowledge Graphs] (https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/) esri developer's page for the current user/pwd for the sample dataset.  
2. In Visual Studio click the Build menu.Then select Build Solution.  
3. Launch the debugger to open ArcGIS Pro.  
4. Create a new blank Map project.  
5. Set your active portal to be the sampleserver7.arcgisonline portal.In the Catalog dockpane,  
6. select the "Portal" tab and then the "My Organization" secondary tab. Scroll down to find the  
7. `BumbleBees` knowledge graph. Right click on BumbleBees and execute the "Add to New Investigation"  
8. context menu item.  
![UI] (screenshots/Catalog_view.png)  
9. To create a link chart to use with the sample, either create one from the Investigation UI  
10. using the Pro UI or simply execute the "CreateLinkChart" button on the sample "FFP Analysis Samples"  
11. tab on the ribbon.  
![UI] (screenshots/ffp_analysis_sample_ribbon.png)  
12. If you choose to run the "CreateLinkChart" button, it will create a link chart that looks like  
13. this:  
![UI] (screenshots/default_link_chart.png)  
14. You can now run the sample analysis options. There is one button on the Tab for each of the  
15. supported KG FFP Analysis:  
 * FFP  
 * Expand  
 * Filtered Expand  
 * Connect  
 * Find Between  
 Each of these options conforms to built-in options of the same name on the Pro "Link Chart" tab.  Expand, Filtered Expand, and Find Between each require that at least one enity, sometimes two, be selected.  
 Consult the [ArcGIS Pro help](https://pro.arcgis.com/en/pro-app/latest/help/data/knowledge/add-relationships-missing-between-entities-in-the-link-chart.htm) for detailed explanations of these options  
16. Note: Expand, Filtered Expand, and Find Between require, minimum, one or two entities to be   
17. selected on the view otherwise these options will be unavailable/disabled on the tab.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
