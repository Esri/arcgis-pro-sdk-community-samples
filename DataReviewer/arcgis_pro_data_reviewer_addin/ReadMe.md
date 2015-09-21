##arcgis_pro_data_reviewer_samples

<!-- TODO: Write a brief abstract explaining this sample -->
The add-in contains basic and advanced examples to demonstrate how to add and remove Data Reviewer project items.    
The basic example focuses solely on Data Reviewer interfaces and objects.  
The advanced example demonstrates how you can use Data Reviewer interfaces and objects with other interfaces in ArcGIS Pro  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:      C#
Subject:       $subject$
Contributor:   ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:  Esri, http://www.esri.com
Date:          9/21/2015
ArcGIS Pro:    1.1
Visual Studio: 2013, 2015
```

##Resources

* [API Reference online](http://pro.arcgis.com/en/pro-app/sdk/api-reference)
* <a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">ArcGIS Pro SDK for .NET (pro.arcgis.com)</a>
* [arcgis-pro-sdk-community-samples](http://github.com/Esri/arcgis-pro-sdk-community-samples)
* [FAQ](http://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)
* [ArcGIS Pro SDK icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/1.1.0.3308)
* [ProConcepts: ArcGIS Pro Add in Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProConcepts-ArcGIS-Pro-Add-in-Samples)
* [Sample data for ArcGIS Pro SDK Community Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases)

##How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data containing a Reviewer workspace and Reviewer batch Jobs is packaged as part of the add-in.  Make sure that the Sample data is unzipped in c:\data and c:\data\DataReviewer is available.  
2. You can modify AddReviewerResults_Basic() method to update the path of the Reviewer Workspace.  
3. You can modify AddBatchJobs_Basic() to update the path of the Reviewer Batch job.  
4. In Visual Studio click the Build menu. Then select Build Solution.  
5. Click Start button to open ArcGIS Pro.  
6. ArcGIS Pro will open.   
7. Open any project file. Click the Reviewer Sample - Basic Tab to use basic samples.  
7.a Make sure that Project pane is open.  
7.b Click Add Reviewer Results button. The Reviewer Results item will be added to the Project pane.  
7.c Click Add Sessions button. All the sessions that are in the Reviewer Dataset will be added to the Project pane as child items to Reviewer Results.  
7.d Click Mark Default button. The first session will be marked as default and its icon will be updated with a home icon in the Project pane.  
7.e Click Remove Session button. The first session will be removed from the Project pane.  
7.f Click Remove Reviewer Results button. The Reviewer Results item will be removed from the Project pane.  
7.g Click Add Reviewer Batch Jobs button. All Reviewer batch jobs that are in the sample data will be added to the Project pane.  
7.h Click Remove Reviewer Batch Job button. The first Batch Job will be removed from the Project pane.  
8. Click the Reviewer Sample - Advanced Tab to use advanced samples.  
8.a Make sure that Project pane is open.  
8.b Click Add Reviewer Results button. A browse dialog will open and you can select a Reviewer workspace to add it to the Project pane.  
8.c Click Add Sessions button. All the sessions that are in the Reviewer Dataset will be added to the Project pane as child items to Reviewer Results. These sessions will also be added to the Reviewer Sessions gallery.  
8.d Right-click a session in the Reviewer Sessions gallery and click Mark Default. This session will be marked as default and its icon will be updated with a home icon in the Project pane.  
8.e Right-click a session in the Reviewer Sessions gallery and click Remove. This session will be removed from the Project pane.  
8.f Click Remove Reviewer Results button. The Reviewer Results item will be removed from the Project pane.  
8.g Click Add Reviewer Batch Jobs button. A browse dialog will open and you can select one or more Reviewer Batch Jobs to add to the Project pane. These Batch Jobs will also be added to the Reviewer Batch Jobs gallery.  
8.h Right-click a Batch Job in Reviewer Batch Jobs gallery and click Remove. This Batch Job will be removed from the Project Pane.  
![UI](Screenshots/Screen.png)  
  


[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​


<p align = center><img src="http://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.1 SDK for Microsoft .NET Framework</b>
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](#requirements) | [Download](#download) |  <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
