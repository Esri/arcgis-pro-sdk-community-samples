/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RubiksCube
{
  internal class ApplyTextures : Button
  {
    protected override async void OnClick()
    {
      // make sure there's an OID from a created feature
      if (Module1.CubeMultipatchObjectID == -1)
        return;

      //get the multipatch layer from the map
      var localSceneLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                          .FirstOrDefault(l => l.Name == "Cube" && l.ShapeType == esriGeometryType.esriGeometryMultiPatch);
      if (localSceneLayer == null)
        return;

      // get the multipatch shape using the Inspector
      var insp = new Inspector();
      await insp.LoadAsync(localSceneLayer, Module1.CubeMultipatchObjectID);
      var multipatchFromScene = insp.Shape as Multipatch;

      // apply textures to the multipatch
      var multipatchWithTextures = ApplyTexturesToMultipatch(multipatchFromScene);


      // modify the multipatch geometry
      string msg = await QueuedTask.Run(() =>
      {
        var op = new EditOperation();
        op.Name = "Apply textures to multipatch";

        // queue feature modification
        op.Modify(localSceneLayer, Module1.CubeMultipatchObjectID, multipatchWithTextures);
        // execute
        bool result = op.Execute();
        // if successful
        if (result)
        {
          return "";
        }

        // not successful, return any error message from the EditOperation
        return op.ErrorMessage;
      });

      // if there's an error, show it
      if (!string.IsNullOrEmpty(msg))
        MessageBox.Show($@"Multipatch update failed: " + msg);
    }


    private Multipatch ApplyTexturesToMultipatch(Multipatch source)
    {
      // read jpeg file into a buffer.  Create a JPEGTexture
      // "C:\Data\xxx\Rubik_cube.jpg
      byte[] imageBuffer = GetBufferFromImageFile(@"C:\Data\MultipatchBuilderEx\Textures\Rubik_cube.jpg", TextureCompressionType.CompressionJPEG);
      JPEGTexture rubiksCubeTexture = new JPEGTexture(imageBuffer);

      // create a material
      BasicMaterial textureMaterial = new BasicMaterial();

      // create a TextureResource from the JPEGTexture and assign 
      textureMaterial.TextureResource = new TextureResource(rubiksCubeTexture);


      // create a builder using the source multipatch
      var cubeMultipatchBuilderExWithTextures = new MultipatchBuilderEx(source);

      // assign texture material to all the patches
      foreach (Patch p in cubeMultipatchBuilderExWithTextures.Patches)
      {
        p.Material = textureMaterial;
      }

      // assign texture coordinate to patches

      // assign texture coordinates to the top face patch
      Patch topFacePatch = cubeMultipatchBuilderExWithTextures.Patches[0];
      topFacePatch.TextureCoords2D = new List<Coordinate2D>
      {
        new Coordinate2D(0.25, 0.33),
        new Coordinate2D(0.25, 0),
        new Coordinate2D(0.5, 0),
        new Coordinate2D(0.5, 0.33),
        new Coordinate2D(0.25, 0.33)
      };

      // assign texture coordinates to the bottom face patch
      Patch bottomFacePatch = cubeMultipatchBuilderExWithTextures.Patches[1];
      bottomFacePatch.TextureCoords2D = new List<Coordinate2D>
      {
        new Coordinate2D(0.25, 1),
        new Coordinate2D(0.25, 0.66),
        new Coordinate2D(0.5, 0.66),
        new Coordinate2D(0.5, 1),
        new Coordinate2D(0.25, 1)
      };

      // assign texture coordinates to the sides face patch
      Patch sidesFacePatch = cubeMultipatchBuilderExWithTextures.Patches[2];
      sidesFacePatch.TextureCoords2D = new List<Coordinate2D>
      {
        new Coordinate2D(0, 0.66),
        new Coordinate2D(0, 0.33),
        new Coordinate2D(0.25, 0.66),
        new Coordinate2D(0.25, 0.33),
        new Coordinate2D(0.5, 0.66),
        new Coordinate2D(0.5, 0.33),
        new Coordinate2D(0.75, 0.66),
        new Coordinate2D(0.75, 0.33),
        new Coordinate2D(1.0, 0.66),
        new Coordinate2D(1.0, 0.33)
      };

      // create the geometry
      Multipatch cubeMultipatchWithTextures = cubeMultipatchBuilderExWithTextures.ToGeometry() as Multipatch;
      return cubeMultipatchWithTextures;
    }

    // fileName of the form  "d:\Temp\Image.jpg"
    private byte[] GetBufferFromImageFile(string fileName, TextureCompressionType compressionType)
    {
      System.Drawing.Image image = System.Drawing.Image.FromFile(fileName);
      MemoryStream memoryStream = new MemoryStream();

      System.Drawing.Imaging.ImageFormat format = compressionType == TextureCompressionType.CompressionJPEG ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Bmp;
      image.Save(memoryStream, format);
      byte[] imageBuffer = memoryStream.ToArray();

      return imageBuffer;
    }
  }
}
