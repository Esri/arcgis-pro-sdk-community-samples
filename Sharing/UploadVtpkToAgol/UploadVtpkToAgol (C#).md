## UploadVtpkToAgol

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to add a vector tile package file to a map both from disk and ArcGIS Online.  The sample also has code to 'upload' a vtpk file to ArcGIS Online.    
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Sharing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  05/06/2025
ArcGIS Pro:            3.5
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Make sure that the Sample data is unzipped in c:\data
2. The data used for this sample is 'C:\Data\VectorTileDemos\AlaskaGeology.vtpk'      
3. In Visual Studio click the Build menu. Then select Build Solution.    
4. Click Start button to open ArcGIS Pro.    
5. ArcGIS Pro will open.     
6. Open a new map project.     
7. Click on the *Upload to ArcGIS Online* tab on the ribbon.    
8. Within this tab there is a *Show & Upload Vector Tile* button.  Click the button to display the *Show & Upload Vector Tile* dockpane.    
9. On the *Show & Upload Vector Tile* dockpane click the open file button and open *C:\Data\VectorTileDemos\AlaskaGeology.vtpk*.    
10. Click the *Add To Map* button to add the vector tile package to the current map.    
![UI](Screenshots/Screenshot1.png)    
11. Click the *Upload to ArcGIS Online (AGOL)* button to upload the vector tile package (referenced under 'Select a VTPK File').    
12. The upload status will be updated once the upload completes.    
13. Click the "Query AGOL' button to download the previously uploaded VTPK vector tile service that was created by AGOL using the uploaded vector tile package.    
14. Look in the code to see the query conditions.    
![UI](Screenshots/Screenshot2.png)     
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
