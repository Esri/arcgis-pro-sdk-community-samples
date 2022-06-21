/*

   Copyright 2019 Esri

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
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilderEx
{
    /// <summary>
    /// Applies textures to the newly created multipatch feature
    /// </summary>
    internal class ApplyTextures : Button
    {
        protected override async void OnClick()
        {
            // make sure there's an OID from a created feature
            if (Module1.MultipatchOID == -1)
                return;

            if (MapView.Active?.Map == null)
                return;

            // find layer
            var member = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "MultipatchWithTextureSimple") as FeatureLayer;
            if (member == null)
                return;


            // create the textures
            TextureCompressionType compressionType = TextureCompressionType.CompressionJPEG;
            byte[] brickImageBuffer = GetBufferImage("pack://application:,,,/MultipatchBuilderEx;component/Textures/Brick.jpg", compressionType);
            var brickTextureResource = new TextureResource(new JPEGTexture(brickImageBuffer));

            BasicMaterial brickMaterialTexture = new BasicMaterial();
            brickMaterialTexture.TextureResource = brickTextureResource;

            byte[] blocksImageBuffer = GetBufferImage("pack://application:,,,/MultipatchBuilderEx;component/Textures/Retaining_Blocks.jpg", compressionType);
            var blocksTextureResource = new TextureResource(new JPEGTexture(blocksImageBuffer));

            BasicMaterial blockskMaterialTexture = new BasicMaterial();
            blockskMaterialTexture.TextureResource = blocksTextureResource;

            byte[] waterImageBuffer = GetBufferImage("pack://application:,,,/MultipatchBuilderEx;component/Textures/water.jpg", compressionType);
            var waterTextureResource = new TextureResource(new JPEGTexture(waterImageBuffer));

            BasicMaterial waterMaterialTexture = new BasicMaterial();
            waterMaterialTexture.TextureResource = waterTextureResource;

            // set up a set of TextureCoordinates - these determine how the texture is draped over a face
            //  In this scenario we will use the same textureCoordinates for each face
            var textureCoords = new List<Coordinate2D>()
                {
                new Coordinate2D(0,0),
                new Coordinate2D(1,0),
                new Coordinate2D(1,-1),
                new Coordinate2D(0,0),
                new Coordinate2D(1,-1),
                new Coordinate2D(0,-1),
                };

            bool result = await QueuedTask.Run(() =>
            {
                // get the multipatch shape using the Inspector
                var insp = new Inspector();
                insp.Load(member, Module1.MultipatchOID);
                var origMultipatch = insp.Shape as Multipatch;

                // create a builder
                var mpb = new ArcGIS.Core.Geometry.MultipatchBuilderEx(origMultipatch);

                // apply the texture materials to the patches
                var patches = mpb.Patches;

                patches[0].Material = brickMaterialTexture;
                patches[0].TextureCoords2D = textureCoords;

                patches[1].Material = blockskMaterialTexture;
                patches[1].TextureCoords2D = textureCoords;

                patches[2].Material = brickMaterialTexture;
                patches[2].TextureCoords2D = textureCoords;

                patches[3].Material = waterMaterialTexture;
                patches[3].TextureCoords2D = textureCoords;

                patches[4].Material = blockskMaterialTexture;
                patches[4].TextureCoords2D = textureCoords;

                patches[5].Material = waterMaterialTexture;
                patches[5].TextureCoords2D = textureCoords;

                // use this method to determine patches which contain the specified texture
                //var texture = mpb.QueryPatchIndicesWithTexture(brickTextureResource);

                // get the modified multipatch geometry
                var newMultipatch = mpb.ToGeometry() as Multipatch;

                // modify operation
                var modifyOp = new EditOperation();
                modifyOp.Name = "Apply textures to multipatch";
                modifyOp.Modify(member, Module1.MultipatchOID, newMultipatch);

                if (modifyOp.Execute())
                    return true;

                return false;
            });
        }

        // sUri of the form  "pack://application:,,,/myPack;component/Images/image.jpg"
        private byte[] GetBufferImage(string sUri, TextureCompressionType compressionType)
        {
            System.Drawing.Imaging.ImageFormat format = (compressionType == TextureCompressionType.CompressionJPEG) ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Bmp;

            Uri uri = new Uri(sUri, UriKind.RelativeOrAbsolute);
            StreamResourceInfo info = Application.GetResourceStream(uri);
            System.Drawing.Image image = System.Drawing.Image.FromStream(info.Stream);

            MemoryStream memoryStream = new MemoryStream();

            image.Save(memoryStream, format);
            byte[] imageBuffer = memoryStream.ToArray();

            return imageBuffer;
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
