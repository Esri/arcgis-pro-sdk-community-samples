## JobManagement

<!-- TODO: Write a brief abstract explaining this sample -->
This Sample provides a Dockable Pane that allows the user to manipulate workflow manager databases while focusing only on one Job Type  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Workflow
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  11/01/2021
ArcGIS Pro:            2.9
Visual Studio:         2017, 2019
.NET Target Framework: 4.8
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
      
1  Must have a Workflow manager database set up and accessible  
1.a. This is done with the combination of database management and our Workflow manager software.  
1.b. please refer to setting up a Workflow manager database before using this sample.  
2. must designate job type to focus on prior to compiling code  
2.a. Open project in Visual Studio. Locate JobManagementModule.cs. Find JobManagementModule. designate the Job Type ID.  
2.b  Use 2 as your ID if you aren't certain and you just used the quick configuration, in your post install.  
2.c  If you have any problems consult your workflow database administrator  
2.d. currently configured to work with a Job Type called 'Work Order' some names maybe needed to change in the xaml and viewmodel  
3. must have a valid workflow database connection active in project before using  
3.a. To acquire a valid worflow database connection just use the add workflow connection under the connections menu on the project tab   
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
