//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ScientificDataStatisticalAnalysis
{
  class Process
  {
    /// <summary>
    /// This function is used to replace the user defined query and statistical operation in the raster function template XML file.
    /// </summary>
    /// <param name="xmlPath">The path of the raster function template XML file.</param>
    /// <param name="operation">An Enum type represents the user selected operation.</param>
    /// <param name="definitionQuery">A string representing the definition query.</param>
    /// <returns>A string representing the path to the updated xml.</returns>
    public static String CustomRFTXmlFile(string xmlPath, Enum operation, string definitionQuery)
    {
      // Gets the XML file name from XML file path.
      string xmlFileName = Path.GetFileNameWithoutExtension(xmlPath);

      // Creates a copy of the XML file at the temp path. 
      string xmlPath_Copy = Path.GetTempPath() + xmlFileName + "_copy" + ".xml";
      File.Copy(xmlPath, xmlPath_Copy, true);

      // Gets the operation id from operation enumeration.
      int operationId = Convert.ToInt32(operation);

      // Initializes a new instance of the XmlDocument class. 
      XmlDocument xmlDoc = new XmlDocument();

      // Loads the XML document from the copy XML file path.
      xmlDoc.Load(xmlPath_Copy);

      // Finds the Operation node in the XML file using its node ID.
      XmlNode node = xmlDoc.SelectSingleNode("//*[@id = 'ID8']");

      // Edits the value in the selected node with user selected operation.
      foreach (XmlNode n in node.ChildNodes)
      {
        if (n.Name == "Value")
        {
            n.InnerText = operationId.ToString();
        }
      }

      // Edits the definition section in XML doc with the string from EditBox.
      xmlDoc.SelectSingleNode("/RasterFunctionTemplate/Definition").InnerText = definitionQuery;

      // Saves the changes to the copy XML file path. 
      xmlDoc.Save(xmlPath_Copy);

      // Returns the copy XML file path.
      return xmlPath_Copy;
    }

    /// <summary>
    /// This function is used to apply a raster function template onto an image service layer.
    /// </summary>
    /// <param name="selectedLayer">The selected image service layer. </param>
    /// <param name="operation">An Enum type represents the user selected operation. </param>
    /// <param name="xmlFilePath">The XML file path.</param>
    /// <returns></returns>
    public static async Task ApplyRFTXmlFile(ImageServiceLayer selectedLayer, Enum operation, string xmlFilePath)
    {   
      // Creates a new instance of XmlDocument class. 
      XmlDocument xmlDoc = new XmlDocument();

      // Loads the RFT XML file from its file path.
      xmlDoc.Load(xmlFilePath);

      // Creates a new instance of the rendering rule.
      CIMRenderingRule setRenderingrule = new CIMRenderingRule();

      // Gets the markup containing all the nodes and their child nodes, passes to the rendering rule definition. 
      setRenderingrule.Definition = xmlDoc.OuterXml;

      // Defines the rendering rule name as the operation name. 
      setRenderingrule.Name = Convert.ToString(operation);

      await QueuedTask.Run(() =>
      {
        // Sets the new rendering rule to the selected layer. 
        selectedLayer.SetRenderingRule(setRenderingrule);

        // Gets the current rendering rule from the layer.
        CIMRenderingRule renderingRule = selectedLayer.GetRenderingRule();

        //Verifies if the current rendering rule name is correct. Else, error message shows up. 
        if (renderingRule.Name != Convert.ToString(operation))
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("The operation is not succeeded, please check...",
            "Operation unsucceeded:", MessageBoxButton.OK, MessageBoxImage.Error);
      });
    }
  }
}