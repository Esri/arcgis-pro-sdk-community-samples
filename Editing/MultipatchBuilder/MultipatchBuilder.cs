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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilder
{
    public class MyMultipatchBuilder
    {
        // lower left, upper left, lower right, upper right
        private static int ll = 0, ul = 1, lr = 2, ur = 3;

        /// <summary>
        /// Creates the structure (multipatch) from a given footprint and the number of floors
        /// building a multipatch geometry using the new 2.5 MultipatchBuilderEx class
        /// Must be called from MCT !!!
        /// </summary>
        /// <param name="floorFootPrint">footprint line features of wall</param>
        /// <param name="floorLevels">Number of floors (4 meters/floor)</param>
        /// <returns></returns>
        public static MultipatchBuilderEx CreateTriangleMultipatchBuilder(Polygon floorFootPrint, 
            int floorLevels)
        {
            var heightInMeters = floorLevels * 7;
            // create the multipatchBuilderEx object
            var mpb = new ArcGIS.Core.Geometry.MultipatchBuilderEx();
            // create a list of patch objects
            var patches = new List<Patch>();
            var coords = new List<Coordinate3D>();
            // each line in the polygon makes up one 'face' (patch) for the multipatch
            // the floor footprint is the baseline 
            // heightInMeters is the elevation
            coords.AddRange (CoordsWithHeight(floorFootPrint, heightInMeters));
            // to drape the faces with textures we have to using triangle patches
            // with one patch for each wall and each wall with the following coordinates
            // making two triangles per wall:
            // upper right, upper left, lower left
            // upper right, lower left, lower right
            // the coords list contains for each wall:
            // lower left, upper left, lower right, upper right
            // process each wall:
            for (var idx = 0; idx < coords.Count-3; idx += 2)
            {
                var patch = mpb.MakePatch(PatchType.Triangles);
                var wallCoords = GetWallCoordinates(coords, idx);
                patch.Coords = wallCoords;
                // if (idx == 0) ShowPatchLabels(patch);
                patches.Add(patch);
            }
            // add the roof
            var roofPatch = mpb.MakePatch(PatchType.TriangleFan);
            var roofCoords = GetRoofCoordinates(coords);
            roofPatch.Coords = roofCoords;
            patches.Add(roofPatch);

            // assign the patches to the multipatchBuilder
            mpb.Patches = patches;

            // call ToGeometry to get the multipatch
            return mpb;
        }

        private static int iNode = 0;

        private static List<Coordinate3D> CoordsWithHeight(Polygon polygon, int height)
        {
            List<Coordinate3D> coords = new List<Coordinate3D>();
            System.Diagnostics.Debug.WriteLine("new face");
            foreach (var pnt in polygon.Points)
            {
                var c = new Coordinate3D(pnt);
                coords.Add(c);
                var up = new Coordinate3D(pnt);
                up.Z += Convert.ToDouble(height);
                coords.Add(up);
                System.Diagnostics.Debug.WriteLine($@"Pnt: {iNode++} {c.X} {c.Y} {c.Z} ");
                System.Diagnostics.Debug.WriteLine($@"Pnt: {iNode++} {up.X} {up.Y} {up.Z} ");
            }
            return coords;
        }

        private static List<Coordinate3D> GetWallCoordinates(List<Coordinate3D> structureCoords, int iStart)
        {
            List<Coordinate3D> coords = new List<Coordinate3D>();
            // upper right, upper left, lower left
            coords.Add(structureCoords[iStart + ur]);
            coords.Add(structureCoords[iStart + ul]);
            coords.Add(structureCoords[iStart + ll]);
            // upper right, lower left, lower right
            coords.Add(structureCoords[iStart + ur]);
            coords.Add(structureCoords[iStart + ll]);
            coords.Add(structureCoords[iStart + lr]);
            return coords;
        }

        private static List<Coordinate3D> GetRoofCoordinates(List<Coordinate3D> structureCoords)
        {
            List<Coordinate3D> coords = new List<Coordinate3D>();
            for (int idx = 1; idx < structureCoords.Count; idx+=2)
            {
                coords.Add(structureCoords[idx]);
            }
            return coords;
        }

        private static List<IDisposable> CimGraphics = new List<IDisposable>();

        public static void ShowPatchLabels (Patch patch)
        {
            foreach (var cg in CimGraphics) cg.Dispose();
            CimGraphics.Clear();
            int nodeNumber = 0;
            int maxCoordCount = patch.Coords.Count;
            int coordCount = 0;
            foreach (var coord in patch.Coords)
            {
                //MapView.Active.AddOverlay()
                var textGraphic = new CIMTextGraphic
                {
                    Symbol = SymbolFactory.Instance.ConstructTextSymbol(
                  ColorFactory.Instance.BlackRGB, 12, "Times New Roman", "Regular").MakeSymbolReference()
                };

                //make the callout for the circle
                var callOut = new CIMPointSymbolCallout
                {
                    PointSymbol = new CIMPointSymbol()
                };
                //Circle outline - 40
                var circle_outline = SymbolFactory.Instance.ConstructMarker(40, "ESRI Default Marker") as CIMCharacterMarker;
                //Square - 41
                //Triangle - 42
                //Pentagon - 43
                //Hexagon - 44
                //Octagon - 45
                circle_outline.Size = 40;
                //eliminate the outline
                foreach (var layer in circle_outline.Symbol.SymbolLayers)
                {
                    if (layer is CIMSolidStroke)
                    {
                        ((CIMSolidStroke)layer).Width = 0;
                    }
                }

                //Circle fill - 33
                var circle_fill = SymbolFactory.Instance.ConstructMarker(33, "ESRI Default Marker") as CIMCharacterMarker;
                //Square - 34
                //Triangle - 35
                //Pentagon - 36
                //Hexagon - 37
                //Octagon - 38
                circle_fill.Size = 40;
                //eliminate the outline, make sure the fill is white
                foreach (var layer in circle_fill.Symbol.SymbolLayers)
                {
                    if (layer is CIMSolidFill)
                    {
                        ((CIMSolidFill)layer).Color = ColorFactory.Instance.WhiteRGB;
                    }
                    else if (layer is CIMSolidStroke)
                    {
                        ((CIMSolidStroke)layer).Width = 0;
                    }
                }

                var calloutLayers = new List<CIMSymbolLayer>
                {
                    circle_outline,
                    circle_fill
                };
                //set the layers on the callout
                callOut.PointSymbol.SymbolLayers = calloutLayers.ToArray();

                //set the callout on the text symbol
                var textSym = textGraphic.Symbol.Symbol as CIMTextSymbol;
                textSym.Callout = callOut;
                textSym.Height = 12;//adjust as needed

                //now set the text
                textGraphic.Text = $@"{nodeNumber++}";
                // reposition the last two labels
                // 2 meters up
                System.Diagnostics.Debug.WriteLine($@"Pnt: {coordCount} {coord.X} {coord.Y} {coord.Z} ");
                var labelPnt = MapPointBuilderEx.CreateMapPoint (new Coordinate3D (coord.X, coord.Y, coord.Z+4*coordCount++)); 
                textGraphic.Shape = labelPnt;

                CimGraphics.Add(MapView.Active.AddOverlay(textGraphic));
            }
        }
    }
}
