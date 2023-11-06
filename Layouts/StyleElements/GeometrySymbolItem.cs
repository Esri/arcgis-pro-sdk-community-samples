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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Drawing;
//using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static ArcGIS.Desktop.Core.ItemFactory;
using System.Xml.Linq;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Layouts;
using System.Windows;
using System.Windows.Controls;

namespace StyleElements
{
  /// <summary>
  /// Represents a custom object that holds the SymbolStyle Item in the dockpane
  /// </summary>
  public class GeometrySymbolItem
  {
    public GeometrySymbolItem(SymbolStyleItem symbolStyleItem, StyleItemType styleItemType)
    {


      //int patchHeight ;
      //int patchWidth;
      //patchHeight = size.Height;
      //patchWidth = size.Width;
      //Different symbol types need different image sizes
      var size = GetImageSize(styleItemType, 32, false);

      symbolStyleItem.PatchHeight = size.Height;
      symbolStyleItem.PatchWidth = size.Width;
      Icon32 = symbolStyleItem.PreviewImage;

      //Get the pretty images for Legend  and Legend items. 
      if (styleItemType is StyleItemType.Legend)
      {
        var legend_sym = symbolStyleItem.GetObject() as CIMLegend;
        switch(legend_sym.FittingStrategy)
        {
          case LegendFittingStrategy.AdjustSize:
            Icon32 = FrameworkApplication.Current.Resources["AdjustFontSize"] as ImageSource;
            break;
          case LegendFittingStrategy.AdjustColumns:
            Icon32 = FrameworkApplication.Current.Resources["AdjustColumns"] as ImageSource;
            break;
          case LegendFittingStrategy.AdjustColumnsAndSize:
            Icon32 = FrameworkApplication.Current.Resources["AdjustColumnsAndFont"] as ImageSource;
            break;
          case LegendFittingStrategy.AdjustFrame:
            Icon32 = FrameworkApplication.Current.Resources["AdjustFrame"] as ImageSource;
            break;
          case LegendFittingStrategy.ManualColumns:
            Icon32 = FrameworkApplication.Current.Resources["ManualColumns"] as ImageSource;
            break;
        }
      }
      else if (styleItemType is StyleItemType.LegendItem)
      {
        var legend_item = symbolStyleItem.GetObject() as CIMLegendItem;
        switch(legend_item)
        {
          case CIMHorizontalLegendItem:
            Icon32 =  FrameworkApplication.Current.Resources["PatchDefault"] as ImageSource;
            break;
          case CIMHorizontalBarLegendItem:
            Icon32 = FrameworkApplication.Current.Resources["ColorRampHorizontalBar"] as ImageSource;
            break;
          case CIMNestedLegendItem:
            Icon32 = FrameworkApplication.Current.Resources["GraduatedNested"] as ImageSource;
            break;
          case CIMVerticalLegendItem:
            Icon32 = FrameworkApplication.Current.Resources["PatchVertical"] as ImageSource;
            break;
          default:
            Icon32 = FrameworkApplication.Current.Resources["LegendItemHorizontal32"] as ImageSource;
            break;
        }
      }

      //Other style item info 
      Name = symbolStyleItem.Name;
      Group = symbolStyleItem.Category;
      StylePath = symbolStyleItem.StylePath;
      Tags = symbolStyleItem.Tags;
      SymbolStyleItem = symbolStyleItem;
      StyleItemType = styleItemType;
      if (symbolStyleItem != null)
        cimSymbol = symbolStyleItem.Symbol as CIMSymbol;

    }
    public ImageSource Icon32 { get; private set; }

    public StyleItemType StyleItemType { get; private set; }

    public string Name { get; private set; }

    public string Group { get; private set; }
    public string StylePath { get; private set; }
    public string Tags { get; private set; }

    public SymbolStyleItem SymbolStyleItem { get; private set; }
    public CIMSymbol cimSymbol
    {
      get; private set;
    }

    //Apply the style to the element
    internal void Execute(List<ArcGIS.Desktop.Layouts.Element> elements)
    {
      var firstElement = elements.FirstOrDefault();

      QueuedTask.Run(() =>
      {
        #region Grid Style application. Use this until ApplyStyle can accept a Grid style item
        if (firstElement is MapFrame mapFrame)
        {
          var symbolItemName = SymbolStyleItem.Name;
          //var girdGraticuleObject = SymbolStyleItem.GetObject();

          var cmf = mapFrame.GetDefinition() as CIMMapFrame;
          //note, if page units are _not_ inches then grid's gridline
          //lengths and offsets would need to be converted to the page units
          var mapGrids = new List<CIMMapGrid>();
          if (cmf.Grids != null)
            mapGrids.AddRange(cmf.Grids);

          var girdGraticuleObject = SymbolStyleItem.GetObject() as CIMMapGrid;

          switch (girdGraticuleObject)
          {
            case CIMGraticule:
              var gridGraticule = girdGraticuleObject as CIMGraticule;
              gridGraticule.Name = symbolItemName;
              gridGraticule.SetGeographicCoordinateSystem(mapFrame.Map.SpatialReference);
              //assign grid to the frame             
              mapGrids.Add(gridGraticule);

              break;
              case CIMMeasuredGrid:
              var gridMeasure = girdGraticuleObject as CIMMeasuredGrid;
              gridMeasure.Name = symbolItemName;
              gridMeasure.SetProjectedCoordinateSystem(mapFrame.Map.SpatialReference);
              //assign grid to the frame
              mapGrids.Add(gridMeasure);  
 
              break;
              case CIMReferenceGrid:
              var gridReference = girdGraticuleObject as CIMReferenceGrid;
              gridReference.Name = symbolItemName;
              //assign grid to the frame
              mapGrids.Add(gridReference);
              break;
          }

          cmf.Grids = mapGrids.ToArray();
          mapFrame.SetDefinition(cmf);
          return; // no need to apply any more styles
        }
        #endregion
        //Apply the style for all elements other than Grids and Graticules.
        foreach (var element in elements)
        {
          if (element.CanApplyStyle(SymbolStyleItem))
          {
            if (element is GraphicElement ge)
            {
              //The magic happens here
              ge.ApplyStyle(SymbolStyleItem, true); //for Graphic Elements such as Point, Lines, Polys, text, preserve size.
            }
            else
              element.ApplyStyle(SymbolStyleItem);
          }
        }
        
      });
    }

    private static bool SaveBitmapSourceToFile(BitmapSource screenShot, string filePath)
    {
      //var screenShot = Clipboard.GetImage();
      using (var fileStream = new FileStream(filePath, FileMode.Create))
      {
        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(screenShot));
        encoder.Save(fileStream);
      }
      return File.Exists(filePath);
    }

    public static System.Drawing.Size GetImageSize(StyleItemType itemType, int patchSize = 32, bool useSimplifiedImage = true)
    {
      int width = patchSize;
      int height = patchSize;
      switch (itemType)
      {
        case StyleItemType.ColorRamp: width = 150; break;
        case StyleItemType.DimensionStyle: width = Math.Max(192, patchSize); height = Math.Max(32, patchSize); break;
        // Layout
        case StyleItemType.NorthArrow: width = 64; height = 64; break;
        case StyleItemType.ScaleBar: width = 150; height = 30; break;
        case StyleItemType.Legend:
          if (useSimplifiedImage)
          {
            width = 64;
            height = 64;
          }
          else
          {
            width = 160;
            height = 160;
          }
          break;

        case StyleItemType.LegendItem: width = 160; height = 160; break;
        case StyleItemType.TableFrame: width = 160; height = 160; break;
        case StyleItemType.TableFrameField: width = 80; height = 160; break;
        case StyleItemType.Grid: width = 160; height = 160; break;
      }
      return new System.Drawing.Size(width, height);
    }       
  }
}
