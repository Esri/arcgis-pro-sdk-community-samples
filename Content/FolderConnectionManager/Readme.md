## FolderConnectionManager

<!-- TODO: Write a brief abstract explaining this sample -->
Allows saving and loading folder connections to a Project showing how to manage folder connections in ArcGIS Pro from within an Add-in.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Content
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
3. Click the Start button to open ArCGIS Pro.  
4. Open any project either a new or existing Project.  
5. Click on the Add-in tab and see that the "Folder Connection Manager" group appears on the "add-in" tab.  
6. Create a new Folder Connection in the Project window as shown below.    
![UI](Screenshots/Screen1.png)    
7. On the Add-in tab click the "Display Connections" button in the "Folder Connection Manager" Add-In group to see the list of all current folder connection path strings.  
![UI](Screenshots/FolderConnect.png)    
8. On the Add-in tab click the "Save Connections" button in the "Folder Connection Manager" Add-In group.The browser pop up appears in which create a txt file on the home folder under projects to save the folderpath.  
![UI](Screenshots/RemoveFolder1.png)  
9. Remove the Folder Connections you just added by right-clicking on folder name in the Project window and selecting "Remove".  
![UI](Screenshots/RemoveFolder.png)  
10. Load your saved Folder Connection by clicking the "Load Connection" button in the "Folder Connection Manager" Add-In group. Select the text file you saved in the previous step.  
11. Verify that your Folder Connections have been programmatically added back under 'Folders'.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
