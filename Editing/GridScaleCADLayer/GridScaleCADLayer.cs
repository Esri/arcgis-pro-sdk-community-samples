//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace GridScaleCADLayer
{
  internal class GridScaleCADLayer : Button
  {
    // A reference point near the center of the CAD layer is defined
    // A second point is defined deltaX=100 and deltaY=100 units away from the first reference point
    // These 2 points are used as the From Points for 2 vectors.
    // The coordinates of these are multiplied by the scale factor entered by the user 
    // The resulting scaled coordinates are used to define the To points
    // of the vectors.
    // These 2 vectors are written to the world file.
    // The world file is placed at the same location on disk as the CAD file
    // The CAD Layer is removed and re-added to the same map so that the display transformation 
    // can be pulled from the newly created world file and is used to update the CAD file 
    // based on the entered Grid Scale.

    private string sTargetFile = "temp.wld";

    protected async override void OnClick()
    {
      // get the feature layer that's selected in the table of contents
      var selectedLayers = MapView.Active.GetSelectedLayers();
      var CADLayer = selectedLayers.OfType<FeatureLayer>().FirstOrDefault() as FeatureLayer;
      if (CADLayer == null)
      {
        System.Windows.MessageBox.Show("Please select the CAD layer in the table of contents", "Layer Grid Scale");
        return;
      }
      double dMetersPerUnit = 1;
      string CadFeatClassNameForSelectedLayer = "";
      //Run on MCT
      bool isValid = await QueuedTask.Run(() =>
      {
        var CADFeatClass = CADLayer.GetFeatureClass();

        var FilePathConnX = CADFeatClass.GetDatastore();
        string sCADFilePath = FilePathConnX.GetConnectionString();
        sCADFilePath = sCADFilePath.Replace("DATABASE=", "");

        var FeatDS = CADFeatClass.GetFeatureDataset();
        string CadFileName = System.IO.Path.Combine(sCADFilePath, FeatDS.GetName());
        string fileExtension = System.IO.Path.GetExtension(CadFileName);
        fileExtension = fileExtension.ToLower();

        string sTargetFileName = System.IO.Path.GetFileNameWithoutExtension(CadFileName);
        sTargetFile = System.IO.Path.Combine(sCADFilePath, sTargetFileName + ".wld");

        //get name for layer
        string FCName = CADFeatClass.GetName();
        CadFeatClassNameForSelectedLayer = System.IO.Path.Combine(CadFileName, FCName);

        FeatureClassDefinition CADFeatClassDef = CADFeatClass.GetDefinition();
        var fcSR = CADFeatClassDef.GetSpatialReference();

        if (fcSR.IsProjected)
          dMetersPerUnit = fcSR.Unit.ConversionFactor; //meters per unit
        
        bool bIsCAD = (fileExtension == ".dwg" || fileExtension == ".dgn" || fileExtension == ".dxf");

        // zoom to the layer
        MapView.Active.ZoomTo(CADLayer); //Layer's ZoomTo extent is used to get a good reference point later

        return bIsCAD & !fcSR.IsGeographic; //The addin requires that the CAD data is not in geographic coordinates
      });

      // if not a valid CAD file
      if (!isValid)
      {
        System.Windows.MessageBox.Show("Please select the CAD layer in the table of contents." + Environment.NewLine +
          "CAD data in a geographic coordinate system is not supported.", "Scale CAD Layer");
        return;
      }

      // get the scale from the user
      double dScaleFactor = 1.0000;
      var sf = new ScaleFactorInput();
      sf.Owner = FrameworkApplication.Current.MainWindow;
      try
      {
        if (sf.ShowDialog() == true)
        {
          string sScaleFactor = sf.ScaleFactor.Text;
          if (!Double.TryParse(sScaleFactor, out dScaleFactor))
          {
            MessageBox.Show("Please type a number", "Grid Scale");
            return;
          }
        }
        else
        {
          // cancelled from dialog
          return;
        }
      }
      finally
      {
        sf = null;
      }

      //use the layer's ZoomTo extent to get the approximate location of the CAD file for the reference point
      var dataReferencePoint = (Coordinate2D)MapView.Active.Extent.Center; //this is sufficient for the reference point
      var secondPoint = new Coordinate2D(dataReferencePoint.X + (100 / dMetersPerUnit), dataReferencePoint.Y + (100 / dMetersPerUnit));

      var dataReferencePointScaled = new Coordinate2D(dataReferencePoint.X * dScaleFactor, dataReferencePoint.Y * dScaleFactor);
      var SecondPointScaled = new Coordinate2D(secondPoint.X * dScaleFactor, secondPoint.Y * dScaleFactor);

      if (WriteWorldFile(sTargetFile, dataReferencePoint, secondPoint, dataReferencePointScaled, SecondPointScaled))
      {
        await QueuedTask.Run(() =>
        {
          //Remove and then re-add layer
          //remove layer
          var map = MapView.Active.Map;
          map.RemoveLayer(CADLayer.Parent as GroupLayer);

          var featureClassUri = new Uri(CadFeatClassNameForSelectedLayer);
          //Define the Feature Layer's parameters.
          var layerParams = new FeatureLayerCreationParams(featureClassUri)
          {
            //Set visibility
            IsVisible = true,
          };
          var createdCadLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(layerParams, MapView.Active.Map);
          MapView.Active.ZoomTo(CADLayer); //ZoomTo the updated extent 
        });
      }
      else
      {
        MessageBox.Show("The world file could not be created.", "Grid Scale");
        return;
      }
    }

    private bool WriteWorldFile(string sTargetFile, Coordinate2D dataReferencePoint, Coordinate2D secondPoint, Coordinate2D dataReferencePointScaled, Coordinate2D secondPointScaled)
    {
      string sX1From = dataReferencePoint.X.ToString("#.###");
      string sY1From = dataReferencePoint.Y.ToString("#.###");
      string sX2From = secondPoint.X.ToString("#.###");
      string sY2From = secondPoint.Y.ToString("#.###");

      string sX1To = dataReferencePointScaled.X.ToString("#.###");
      string sY1To = dataReferencePointScaled.Y.ToString("#.###");
      string sX2To = secondPointScaled.X.ToString("#.###");
      string sY2To = secondPointScaled.Y.ToString("#.###");

      //Write the world file
      try
      {
        StreamWriter sw = new StreamWriter(sTargetFile);
        string Line1 = sX1From + "," + sY1From + " " + sX1To + "," + sY1To;
        string Line2 = sX2From + "," + sY2From + " " + sX2To + "," + sY2To;
        sw.WriteLine(Line1);
        sw.WriteLine(Line2);
        sw.Close();

        return true;
      }
      catch // streamwriter can throw if no write permissions
      {
        // ignore exception but could throw a message box if you want.  
      }

      return false;
    }
  }
}
