## DuplicateAndSelect

<!-- TODO: Write a brief abstract explaining this sample -->
This sample provides a set of controls which guide the user through a data quality assurance(QA) review workflow, with tools for visually reviewing and notating datasets based on their accuracy.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  04/04/2024
ArcGIS Pro:            3.3
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Open this solution in Visual Studio.
2. Click the build menu and select Build Solution.  
3. Click the Start button to run the solution.  ArcGIS Pro will open.  
4. Open an existing Project the contains a Map showing a FeatureLayer and a StandAloneTable.  
5. Click on the "Add-in" tab and note the "Feature Duplicate and Select" and "Row Duplicate and Select" button groups.  
![UI](Screenshot/Screenshot1.png)  
6. Click the "Attributes" button to bring up the "Attributes" dockpane and make sure that the "selection" tab is selected.  
7. Select one Feature and one Row (in the Standalone Table).  
![UI](Screenshot/Screenshot2.png)  
8. Click the "Duplicate Add Selection" button to create a duplicate of the first selected feature and add the newly created feature to the existing selection.  
![UI](Screenshot/Screenshot3.png)  
9. Next to perform the same operation on a Standalone Table  
![UI](Screenshot/Screenshot4.png)  
10. Click the "Table Dupl. Add Selection" button to duplicate the first selected row and add the newly created row to the selected set.  
![UI](Screenshot/Screenshot5.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
