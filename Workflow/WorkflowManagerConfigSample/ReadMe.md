## WorkflowManagerConfigSample

<!-- TODO: Write a brief abstract explaining this sample -->
This sample allows users to only interact with the Workflow Pane in ArcGIS Pro, and hides the Workflow View, the Job view and the Workflow ribbon on map from the user. This configuration allows organizations to deploy a slimmed down version of Workflow Manager for ArcGIS Pro that focusses on work completion. Users see only their jobs or jobs in their groups, and is for users who don’t have the requirement to edit job properties, search for jobs and see a workflow diagram. This creates a simpler UI   
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Workflow
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
In order to use this sample you must have a Workflow manager database set up and accessible    
Please refer to setting up a Workflow manager database before using this sample   
Configurations are built using the configuration project template provided in the Pro SDK templates in Visual Studio.  Once selected, the configuration template creates the necessary project components which developers can use. Please refer to ArcGIS Pro SDK Configurations for more information   
  
1. In Visual Studio click the Build menu. Then select Build Solution.
2. Click Start button to open ArcGIS Pro. You can also install the configuration  'WorkflowManagerConfigSample.proconfigx' and then deploy the sample using the following command line: C:[file location]\ArcGISPro.exe /config:[name of config]   
3. The ArcGIS Pro Project must have a valid Workflow Manager database connection prior to using   
4. To establish a valid Workflow Manager database connection just use the ‘Add Workflow Connection’ option under the connections menu on the project tab. Save the project and reopen it again to view all open jobs assigned to the current user or their groups. Users can then execute and finish jobs using only the workflow pane.   
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
