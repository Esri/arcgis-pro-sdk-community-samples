## ScientificDataStatisticalAnalysis

<!-- TODO: Write a brief abstract explaining this sample -->
This sample demonstrates how to leverage the raster function template to simplify the statistical analysis workflows for multidimensional data.  
The sample includes these functions:  
  
1. Group raster items in an image service layer using a SQL expression.  
2. Perform statistical calculations on the grouped items from a list of operations.  
Supported operations: Majority, Maximum, Mean, Median, Minimum, Minority, Range, StandardDeviation, Sum, and Variety.  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Raster
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
2. Create a new ArcGIS Pro project. Insert a new map if one does not exist.  
3. Import the Scientific_data_calculation template to the Project category's Project1 sub-category on Raster Functions pane.  
   (Note: The Scientific_data_calculation template is provided with the add-in located in the Visual Studio project folder.)   
   1) Click Raster Functions button on Imagery tab to open the Raster Functions pane.   
   2) On Raster Functions pane, click the Project category.   
   3) Click the Import functions button on the right of the Project1 sub-cagetory (the down arrow button), browse to the template file and add it.   
   4) Save ArcGIS Pro project.  
4. Click on the Add-In tab.  
5. Add a scientific multidimensional image service to the map view.   
   By default, the Scientific_data_calculation template performs calculation on selected variable within a certain time period,     therefore Variable and time (i.e.,StdTime) fields in the attribute table are required.     
   Edit the template properties to fit your own data format.  
6. Enter your definition query SQL expression, then click "Enter" on the key board to confirm your editing.  
   Note: You can build your query or write a SQL expression through the layer properties definition query tab,     then copy the SQL expression and paste it to the definition query textbox.   
   Example: Variable = 'Water Temperature' And StdZ = 0 And StdTime between date '2018-1-1 0:0:0' And date '2018-3-6 0:0:0'.  
7. Click the drop down arrow on the right of the "Operations" combo box to show the list of supported operations.  
8. Select an operation from the list. This will apply the operation on the image service layer.  
9. To visualize the results better, make sure the image service layer is selected in the Contents pane.  
10. Click the DRA (Dynamic range adjustment) button on the Appearance tab.  
11. Selecting different operations in the dropdown will apply those operations on the image service layer.  
12. To change your query, edit your definition query SQL expression in the textbox, and click "Enter",      this will apply the new definition query and selected operation to the layer.  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="http://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
