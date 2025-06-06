## QAReviewTool

<!-- TODO: Write a brief abstract explaining this sample -->
This sample is a simplified data quality assurance(QA) review workflow.  It provides a set of controls guiding an operator through a data quality assurance review workflow to visually inspecting each feature and optionally add a QA notation to the record.  
  


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
1. Download the Community Sample data(see under the 'Resources' section for downloading sample data). Make sure that the Sample dataset is unzipped under c:\data and that the sample data contains the folder C:\Data\QAReviewTool containing a map package named "QA_Review_Tool.ppkx" which is required for this sample.
2. Open this solution in Visual Studio.  
3. Click the build menu and select Build Solution.  
4. Click the Start button to run the solution.  ArcGIS Pro will open.  
5. Open the map package "QA_Review_Tool.ppkx" located in the "C:\Data\QAReviewTool" folder.  This project contains all required data.  Be sure the Topographic basemap layer is displayed.  
6. Click on the "Review" tab provided by the add-in.  In the "Review Layer Selection" group, click the dropdown button for the "Layer" combobox and select "La_Jolla_Roads".  This is the feature class to be reviewed.  Press the "Start Feature Review" button, which enables the comboboxes in the controls in the "Feature Review Field Settings" group.  You can also click the "Open Attribute Table" button to view the selected table's attribute table.  
![UI](Screenshot/Screenshot1.png)  
7. In the "Feature Review Field Settings" group, click on the dropdown button for the "Value Field" and scroll to the bottom of the list and choose the field "review_code".  This will populate the "Value" field combobox with the unique data values from the review_code field.  It will then zoom to the feature containing the value "1" in the "Value" field.  The forward and backward navigation buttons are now enabled, and you can click on these buttons to zoom to the different records containing other review_code values.  Click on "Show Selected Records" in the attribute table to just show the selected record(s).  
![UI](Screenshot/Screenshot2.png)  
8. In the "Note Field" combobox, choose the "QA_NOTE" from the list of available field names to store your QA notes.  If the entered field name doesn't exist, the add-in prompts before adding the field to your table.  If you chose to add the QA_NOTE field to your schema check the attribute table to verify the addition.  The field , and is also listed in the Note Field combobox.  This step will enable the rest of the controls on the Review tab.  
![UI](Screenshot/Screenshot3.png)  
9. You are now able to use the enabled Note Shortcuts to apply a brief description to any selected records.Use the "Forward" button to navigate to review_code value 3 and you will see three road features selected.  As the Topographic basemap layer does not show a street for these features, change the basemap to Imagery to see verify the data.  You will see the features represent the entrance driveway to a building and its parking lot.  Press the "Correct" note shortcut button to set the QA review value.  
![UI](Screenshot/Screenshot4.png)  
10. Next, you can try applying custom notes and managing your edits using the controls in the "Custom Notes" group.You can load a set of custom notes from a textfile and use these in place of the note shortcuts.Press the "Load Note File" button and navigate to the C:\Data\QAReviewTool folder.Click on the file "Custom Notes.txt" and press OK.  
![UI](Screenshot/Screenshot5.png)  
11. The values will be loaded into the "Note Text" combobox.You can then select from these values and apply them to your current selected records using the "Save Notation" button.You can also add new values to the Note Text combobox and save the values to a new note file.Finally, you can use the Undo, Save and Discard buttons to manage your edits.  
![UI](Screenshot/Screenshot6.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
