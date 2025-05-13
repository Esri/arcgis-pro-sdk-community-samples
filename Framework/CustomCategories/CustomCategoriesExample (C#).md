## CustomCategoriesExample

<!-- TODO: Write a brief abstract explaining this sample -->
An example of implementing a custom category. In this case we declare a custom category **AcmeCustom_Reports** and a contract:  
**IAcmeCustomReport**  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
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
CustomCategoriesExample add-in declares the category AcmeCustom_Reports.  
It also provides a default component, **CustomCategoriesExample.Report.DefaultReport** that implements it via the **IAcmeCustomReport** contract. DefaultReport registers in the AcmeCustom_Reports category in the Config.daml.    
    
CustomCategoriesExtraReport add-in also creates a component for the AcmeCustom_Reports category and likewise registers it in the category within its config.daml.   
CustomCategoriesExtraReport.ExtraReport class implements the IAcmeCustomReport contract (as required by the category creator - CustomCategoriesExample add-in in this case).  
    
When the CustomCategoriesExample Module is initialized, it reads its   
AcmeCustom_Reports category via **Categories.GetComponentElements** and tests each one for the presence of the IAcmeCustomReport contract. Any component that registers in the category but does not implement the contract is skipped.  
The rest are instantiated and loaded into the ReportsWindow dialog for selection.  
![UI](Screenshots/screen1.png)  
    
Try making additional add-ins that implement the IAcmeCustomReport contract and register their component in the AcmeCustom_Reports category. Depending on the number of components loaded that implement the AcmeCustom_Reports category, the list of available reports in the ReportsWindow will increase or decrease respectively.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
