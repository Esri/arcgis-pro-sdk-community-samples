## Create and Delete FeatureClass with Subtypes

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to use the DDL APIs to add and remove fields in a feature class.
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Geodatabase
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  02/20/2023
ArcGIS Pro:            3.0
Visual Studio:         2023
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)


## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Open this solution in Visual Studio.  
1. Click the build menu and select Build Solution.  
1. Click the Start button to open ArcGIS Pro. ArcGIS Pro will open.    
1. Open any project. 
1. Click on the Add-in tab and verify that a "Add/Remove Fields" group was added.  
1. Notice the buttons in the "Add/Remove Fields" group.
1. Tap the "Create Emtpy Geodatabase" button.
![UI](Screenshots/Screen0.png)
1. Add the new Database located in the "C:\temp\mySampleGeoDatabase.gdb" directory into the Catalog pane. 
1. Add a Feature Class to the Geodatabase and name it "Parcels".
![UI](Screenshots/Screen1.png)
1. Tap the finish button to finish adding the new Feature Class
![UI](Screenshots/Screen2.png)
1. Tap the "Add Fields in Feature Class" button.
![UI](Screenshots/Screen3.png)
1. Open the Table for the Parcels Feature Class.
1. Notice the newly added "Tax_Code" , "Parcel_ID" , "Global_ID" and "Parcel_Address" fields.
![UI](Screenshots/Screen4.png)
1. Tap the "Remove Field Table"
![UI](Screenshots/Screen8.png)
1. Open the Table for the Parcels Feature Class.
1. Notice the "Parcel_Address" Field has been deleted.
![UI](Screenshots/Screen5.png) 
1. Add a new Feature Class named "Pipes",
1. Tap the "Add Field with Domain" button.
![UI](Screenshots/Screen6.png) 
1. Open the table for the "Pipes" Feature Class.
![UI](Screenshots/Screen7.png) 
1. Notice the domains of the "Pipe Type" field. 


  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
