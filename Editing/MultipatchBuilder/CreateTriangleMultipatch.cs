/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Resources;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilder
{
  /// <summary>
  /// Creates a new triangle multipatch and stores it in a feature class. The multipatch has no materials and no textures. 
  /// </summary>
  internal class CreateTriangleMultipatch : Button
  {
    protected override async void OnClick()
    {
      #region Initialization
      // set up a set of TextureCoordinates - these determine how the texture is draped over a face
      //  In this scenario we will use the same textureCoordinates for each face
      var textureCoords = new List<Coordinate2D>()
              {
                new Coordinate2D(4.67909908294678,-2.89953231811523),
                new Coordinate2D(-3.7085223197937,-2.89953231811523),
                new Coordinate2D(-3.6790623664856,1.89953279495239),
                new Coordinate2D(4.67909908294678,-2.89953231811523),
                new Coordinate2D(-3.6790623664856,1.89953279495239),
                new Coordinate2D(4.7085223197937,1.89953327178955)
              };

      esriTextureCompressionType compressionType = esriTextureCompressionType.CompressionJPEG;
      byte[] glassImageBuffer = GetBufferImage("pack://application:,,,/MultipatchBuilder;component/Textures/Glass.jpg", compressionType);
      var glassTextureResource = new TextureResource(new JPEGTexture(glassImageBuffer));
      byte[] roofImageBuffer = GetBufferImage("pack://application:,,,/MultipatchBuilder;component/Textures/Roof.jpg", compressionType);
      var roofTextureResource = new TextureResource(new JPEGTexture(roofImageBuffer));

      var materialGray = new BasicMaterial
      {
        Color = System.Windows.Media.Colors.Gray
      };
      #endregion

      if (MapView.Active?.Map == null)
        return;

      // find footprint layer
      var footPrintLyr = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "BuildingFootprints") as FeatureLayer;
      if (footPrintLyr == null)
      {
        MessageBox.Show("Can't find layer: BuildingFootprint");
        return;
      }

      var buildingLyr = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "BuildingStructure") as FeatureLayer;
      if (buildingLyr == null)
      {
        MessageBox.Show("Can't find layer: BuildingStructure");
        return;
      }

      // create the multipatch
      var mpb = await QueuedTask.Run<MultipatchBuilderEx>(() =>
      {
        // get all selected lines and use them as the building footprint
        var footPrintSelection = footPrintLyr.GetSelection();
        Polygon footPrint = null;
        int floorLevels = 1;
        #region Get Footprint and Floor levels
        foreach (var footprintOid in footPrintSelection.GetObjectIDs())
        {
          // get the multipatch shape using the Inspector
          var insp = new Inspector();
          insp.Load(footPrintLyr, footprintOid);
          footPrint = GeometryEngine.Instance.ReverseOrientation(insp.Shape as Multipart) as Polygon;
          floorLevels = (int)insp["Floors"];
        }
        if (footPrint == null)
        {
          MessageBox.Show("No selected building footprint found");
          return null;
        }
        #endregion
        // Create the MultipatchBuilder using the building footprints and the floorlevels as height
        return MyMultipatchBuilder.CreateTriangleMultipatchBuilder(footPrint, floorLevels);
      });

      // apply texture or material
      // create a builder to work on the multipatch geometry
      switch (Module1.SelectedTexture)
      {
        case "Glass":
          // create the textures for walls and roof
          BasicMaterial glassMaterialTexture = new BasicMaterial
          {
            TextureResource = glassTextureResource
          };
          BasicMaterial roofMaterialTexture = new BasicMaterial
          {
            TextureResource = roofTextureResource
          };

          // apply the texture materials to the patches
          var patches = mpb.Patches;
          for (var iPatch = 0; iPatch < patches.Count; iPatch++)
          {
            if (iPatch == patches.Count - 1)
            {
              // roof
              patches[iPatch].Material = roofMaterialTexture;
              patches[iPatch].TextureCoords2D = textureCoords;
            }
            else
            {
              // walls
              patches[iPatch].Material = glassMaterialTexture;
              patches[iPatch].TextureCoords2D = textureCoords;
            }
          }
          break;
        case "Red-solid":
          // create some materials
          var materialRed = new BasicMaterial
          {
            Color = System.Windows.Media.Colors.Brown
          };
          // apply the materials to the patches
          for (var iPatch = 0; iPatch < mpb.Patches.Count; iPatch++)
          {
            if (iPatch == mpb.Patches.Count - 1)
            {
              // roof
              mpb.Patches[iPatch].Material = materialGray;
            }
            else
            {
              // walls
              mpb.Patches[iPatch].Material = materialRed;
            }
          }
          break;
        case "Gray-solid":
          // create some materials
          var materialSilver = new BasicMaterial
          {
            Color = System.Windows.Media.Colors.Silver
          };
          // apply the materials to the patches
          for (var iPatch = 0; iPatch < mpb.Patches.Count; iPatch++)
          {
            if (iPatch == mpb.Patches.Count - 1)
            {
              // roof
              mpb.Patches[iPatch].Material = materialGray;
            }
            else
            {
              // walls
              mpb.Patches[iPatch].Material = materialSilver;
            }
          }
          break;
      }

      // create a new feature using the multipatch
      bool result = await QueuedTask.Run(() =>
      {
        // track the newly created objectID
        long newObjectID = -1;
        var op = new EditOperation
        {
          Name = "Create multipatch feature",
          SelectNewFeatures = false
        };
        Module1.NewMultipatch = mpb.ToGeometry() as Multipatch;
        op.Create(buildingLyr, Module1.NewMultipatch, oid => newObjectID = oid);
        if (op.Execute())
        {
          // save the oid in the module for other commands to use
          Module1.NewMultipatchOID = newObjectID;
          return true;
        }
        var msg = op.ErrorMessage;
        return false;
      });
    }

    // sUri of the form  "pack://application:,,,/myPack;component/Images/image.jpg"
    private byte[] GetBufferImage(string sUri, esriTextureCompressionType compressionType)
    {
      System.Drawing.Imaging.ImageFormat format = (compressionType == esriTextureCompressionType.CompressionJPEG) ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Bmp;

      Uri uri = new Uri(sUri, UriKind.RelativeOrAbsolute);
      StreamResourceInfo info = System.Windows.Application.GetResourceStream(uri);
      System.Drawing.Image image = System.Drawing.Image.FromStream(info.Stream);

      MemoryStream memoryStream = new MemoryStream();

      image.Save(memoryStream, format);
      byte[] imageBuffer = memoryStream.ToArray();

      return imageBuffer;
    }
  }
}
