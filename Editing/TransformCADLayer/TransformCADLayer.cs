/*

   Copyright 2023 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransformCADLayer
{
  internal class TransformCADLayerButton : Button
  {
    // 
    // Two vectors are written to a world file.
    // The world file is placed at the same location on disk as the CAD file.
    // The CAD Layer has it's projection assigned (or re-assigned) so that the
    // transformation can be pulled from the newly created world file and
    // so that its updated location is properly displayed based on the entered
    // transformation parameters.

    private string sTargetWldFile = "temp.wld3";
    protected async override void OnClick()
    {
      // get the feature layer that's selected in the table of contents
      var selectedLayers = MapView.Active.GetSelectedLayers();
      FeatureLayer CADLayer = selectedLayers.OfType<FeatureLayer>().FirstOrDefault();
      string CadFileName = "";
      SpatialReference fcSR = null;
      if (CADLayer == null)
      {
        System.Windows.MessageBox.Show("Please select the CAD layer in the table of contents", "Layer Transform");
        return;
      }
      double dMetersPerUnit = 1.0;
      string CadFeatClassNameForSelectedLayer = "";
      string message = "Please select the CAD layer in the table of contents." + Environment.NewLine +
          "CAD data in a geographic coordinate system is not supported.";
      CadDataset cadDataset = null;
      //Run on MCT
      bool isValid = await QueuedTask.Run(() =>
      {
        var CADFeatClass = CADLayer.GetFeatureClass();

        var FilePathConnX = CADFeatClass.GetDatastore();
        string sCADFilePath = FilePathConnX.GetConnectionString();
        sCADFilePath = sCADFilePath.Replace("DATABASE=", "");

        var FeatDS = CADFeatClass.GetFeatureDataset();
        CadFileName = System.IO.Path.Combine(sCADFilePath, FeatDS.GetName());
        string fileExtension = System.IO.Path.GetExtension(CadFileName);
        fileExtension = fileExtension.ToLower();
        try
        {
          var cadfileStore = new FileSystemDatastore(new FileSystemConnectionPath(new Uri(sCADFilePath), FileSystemDatastoreType.Cad));
          cadDataset = cadfileStore.OpenDataset<CadDataset>(CadFileName);
        }
        catch
        {
          message += Environment.NewLine + "BIM Data not supported.";
          return false;
        }

        string sTargetFileName = System.IO.Path.GetFileNameWithoutExtension(CadFileName);
        sTargetWldFile = System.IO.Path.Combine(sCADFilePath, sTargetFileName + ".wld3");

        //get name for layer
        string FCName = CADFeatClass.GetName();
        CadFeatClassNameForSelectedLayer = System.IO.Path.Combine(CadFileName, FCName);
        bool bIsCAD = fileExtension == ".dwg" || fileExtension == ".dgn" || fileExtension == ".dxf";
        FeatureClassDefinition CADFeatClassDef = CADFeatClass.GetDefinition();
        fcSR = CADFeatClassDef.GetSpatialReference();
        bool bResult = bIsCAD & !fcSR.IsGeographic; //The addin requires that the CAD data is not in geographic coordinates
        if (bResult)
        {
          if (fcSR.IsProjected)
            dMetersPerUnit = fcSR.Unit.ConversionFactor; //meters per unit
        }
        return bResult;
      });

      // if not a valid CAD file
      if (!isValid)
      {
        System.Windows.MessageBox.Show(message, "Transform CAD Layer");
        return;
      }

      double dOriginX = 0.0; // Origin Northing(Y) coordinate
      double dOriginY = 0.0; // Origin Northing (Y) coordinate
      double dGridX = 0.0; // Grid Easting (X) coordinate
      double dGridY = 0.0; // Grid Northing (Y) coordinate
      double dScaleFactor = 1.0000; // Scale Factor
      double dRotation = 0.0000; // Rotation
      bool bSetGroundToGrid = false;

      #region Collect parameters from dialog
      TransformationInput transformationDlg = new();
      transformationDlg.Owner = FrameworkApplication.Current.MainWindow;

      TransformationViewModel VM = new();
      transformationDlg.DataContext = VM;

      string sLastUsedParams = "";
      bool bCancel = false;

      try
      {
        if (transformationDlg.ShowDialog() == true)
        {
          string sOriginX = VM.Transformation.OriginX;
          string sOriginY = VM.Transformation.OriginY;
          string sGridX = VM.Transformation.GridX;
          string sGridY = VM.Transformation.GridY;
          string sScaleFactor = VM.Transformation.ScaleFactor;
          string sRotation = VM.Transformation.Rotation;
          string sSetGroundToGrid = VM.Transformation.UpdateGround2Grid.ToString();
          sLastUsedParams = sOriginX + "|" + sOriginY + "|" + sGridX + "|" + sGridY + "|"
            + sScaleFactor + "|" + sRotation + "|" + sSetGroundToGrid;

          bSetGroundToGrid = VM.Transformation.UpdateGround2Grid;

          if (!Double.TryParse(sOriginX, out dOriginX))
          {
            MessageBox.Show("Local Easting (X) must be a number." + Environment.NewLine
              + "Press the Transform button again to retry.", "Local Easting (X)");
            return;
          }
          if (!Double.TryParse(sOriginY, out dOriginY))
          {
            MessageBox.Show("Local Northing (Y) must be a number." + Environment.NewLine
              + "Press the Transform button again to retry.", "Local Northing (Y)");
            return;
          }
          if (!Double.TryParse(sGridX, out dGridX))
          {
            MessageBox.Show("Grid Easting (X) must be a number." + Environment.NewLine
              + "Press the Transform button again to retry.", "Grid Easting (X)");
            return;
          }
          if (!Double.TryParse(sGridY, out dGridY))
          {
            MessageBox.Show("Grid Northing (Y) must be a number." + Environment.NewLine
              + "Press the Transform button again to retry.", "Grid Northing (Y)");
            return;
          }
          if (!Double.TryParse(sScaleFactor, out dScaleFactor))
          {
            MessageBox.Show("Scale Factor must be a number." + Environment.NewLine
              + "Press the Transform button again to retry.", "Scale Factor");
            return;
          }
          if (dScaleFactor <= 0.0)
          {
            MessageBox.Show("Scale Factor must be greater than zero." + Environment.NewLine + "Press the Transform button again to retry.", "Scale Factor");
            return;
          }
          if (!Double.TryParse(sRotation, out dRotation))
          {
            MessageBox.Show("Rotation must be a number." + Environment.NewLine + "Press the Transform button again to retry.", "Rotation");
            return;
          }

          if (bSetGroundToGrid)
          {
            var mapView = MapView.Active;
            if (mapView?.Map == null)
              return;

            var cimG2G = await mapView.Map.GetGroundToGridCorrection();
            if (cimG2G == null)
              cimG2G = new CIMGroundToGridCorrection();

            cimG2G.UseScale = true;
            cimG2G.ScaleType = GroundToGridScaleType.ConstantFactor;
            cimG2G.ConstantScaleFactor = dScaleFactor;

            cimG2G.UseDirection = true; //use direction offset
            cimG2G.Direction = dRotation; //direction offset angle

            cimG2G.Enabled = true; //turn ground to grid ON
            await mapView.Map.SetGroundToGridCorrection(cimG2G);
          }
        }


        else
        {
          // Canceled from dialog
          bCancel = true;
          return;
        }
      }
      finally
      {
        transformationDlg = null;
        if (!bCancel)
        {
          TransformCADDialog.Default["LastUsedParams"] = sLastUsedParams;
          TransformCADDialog.Default.Save(); //comment out if you only want to save settings within each app session
        }
      }
      #endregion

      bool bUnknownProjection = false;
      #region define projection
      if (fcSR.IsUnknown && MapView.Active.Map.SpatialReference.IsProjected)
      {//if .prj is unknown use the active map's projection for the CAD file.
        fcSR = MapView.Active.Map.SpatialReference;
        bUnknownProjection = true;
      }

      CancelableProgressorSource cps =
              new("Define Projection", "Canceled");
      int numSecondsDelay = 5;
      bool bContinue = true;
      //The following code assigns the .wld file so
      //that the CAD layer can be redrawn in the correct location.
      //a projection is assigned if it is initially unknown
      await QueuedTask.Run(async () =>
      {
        cps.Progressor.Max = (uint)numSecondsDelay;
        //check every second
        cps.Progressor.Value += 1;
        cps.Progressor.Message = "Creating and applying world file...";

        //if the CAD file projection is unknown, use define projection to match map
        if (bUnknownProjection)
          await DefineProjectionAsync(CadFileName, fcSR, cps);

        bContinue = !cps.Progressor.CancellationToken.IsCancellationRequested;

        var pFrom1 = new Coordinate2D(dOriginX, dOriginY);
        var pTo1 = new Coordinate2D(dGridX, dGridY);
        var dDeltaX = dGridX - dOriginX;
        var dDeltaY = dGridY - dOriginY;

        var pFrom2 = new Coordinate2D(pFrom1.X + (10000 / dMetersPerUnit), pFrom1.Y + (10000 / dMetersPerUnit));

        var Rotated = GeometryEngine.Instance.Rotate(pFrom2.ToMapPoint(), pFrom1.ToMapPoint(), dRotation * Math.PI / 180);
        MapPoint RotatedAndScaled = (MapPoint)GeometryEngine.Instance.Scale(Rotated, pFrom1.ToMapPoint(), dScaleFactor, dScaleFactor);
        var RotatedScaledTranslated = new Coordinate2D(RotatedAndScaled.X + dDeltaX, RotatedAndScaled.Y + dDeltaY);

        if (!WriteWorldFile(sTargetWldFile, pFrom1, pTo1, pFrom2, RotatedScaledTranslated))
        {
          MessageBox.Show("The world file could not be created.", "Transform");
          return;
        }
        else
        {
          cadDataset.Reload();
          if (CADLayer.Parent is Layer)
          {
            MapView.Active.ZoomTo(CADLayer.Parent as GroupLayer);
            //Invalidate the layer to refresh display
            MapView.Active.Invalidate(CADLayer.Parent as GroupLayer, MapView.Active.Extent);
          }
          else
          {
            MapView.Active.ZoomTo(CADLayer as Layer);
            MapView.Active.Invalidate(CADLayer as Layer, MapView.Active.Extent);
          }
        }
      }, cps.Progressor);
      if (!bContinue)
        return;
      #endregion

    }

    private static bool WriteWorldFile(string sTargetFile, Coordinate2D FromPoint1, Coordinate2D ToPoint1, Coordinate2D FromPoint2, Coordinate2D ToPoint2)
    {
      string sX1From = FromPoint1.X.ToString("0.##########");
      string sY1From = FromPoint1.Y.ToString("0.##########");
      string sX2From = FromPoint2.X.ToString("0.##########");
      string sY2From = FromPoint2.Y.ToString("0.##########");

      string sX1To = ToPoint1.X.ToString("0.##########");
      string sY1To = ToPoint1.Y.ToString("0.##########");
      string sX2To = ToPoint2.X.ToString("0.##########");
      string sY2To = ToPoint2.Y.ToString("0.##########");

      //Write the world file
      try
      {
        StreamWriter sw = new StreamWriter(sTargetFile);
        string Line1 = sX1From + "," + sY1From + ",0 " + sX1To + "," + sY1To + ",0";
        string Line2 = sX2From + "," + sY2From + ",0 " + sX2To + "," + sY2To + ",0";
        sw.WriteLine(Line1);
        sw.WriteLine(Line2);
        sw.Close();

        return true;
      }
      catch // streamwriter can throw error if no write permissions
      {
        // ignore exception, but could show a message box if you want.  
      }

      return false;
    }

    protected static async Task DefineProjectionAsync(string CadFileName, SpatialReference SpatialRef, CancelableProgressorSource cps)
    {
      //GP Define Projection
      GPExecuteToolFlags flags = GPExecuteToolFlags.Default; // | GPExecuteToolFlags.GPThread;
      var parameters = Geoprocessing.MakeValueArray(CadFileName, SpatialRef.Wkt);
      await Geoprocessing.ExecuteToolAsync("management.DefineProjection", parameters,
          null, cps.Progressor, flags);
    }
  }
}
