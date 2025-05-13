using ArcGIS.Core.Data.DDL;
using System;
using System.Collections.Generic;
using ArcGIS.Core.Data.Mapping;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;

namespace DDLCreateDeleteDimensionFeatureClass
{
  public static class Operations
  {

    public static void CreateDimensionFeatureClass(SchemaBuilder schemaBuilder)
    {
      DimensionFeatureClassDescription dimensionFeatureClassDescription = new DimensionFeatureClassDescription(Module1.dimensionFeautreClassName,
        GetDimensionFieldDescriptions(), new ShapeDescription(GeometryType.Polygon, SpatialReferences.WGS84), GetCIMDimensionStyles());

      DimensionFeatureClassToken dimensionFeatureClassToken = schemaBuilder.Create(dimensionFeatureClassDescription);
      bool status = schemaBuilder.Build();
      IReadOnlyList<string> errors = schemaBuilder.ErrorMessages;
    }

    public static void DeleteDimensionFeatureClass(SchemaBuilder schemaBuilder, DimensionFeatureClassDefinition dimensionFeatureClassDefinition)
    {
      schemaBuilder.Delete(new DimensionFeatureClassDescription(dimensionFeatureClassDefinition));
      schemaBuilder.Build();
    }

    private static List<FieldDescription> GetDimensionFieldDescriptions()
    {
      FieldDescription distanceFieldDescription = new FieldDescription("DistanceMeasure", FieldType.Double);
      FieldDescription measurementDate = new FieldDescription("MeasurementDate", FieldType.Date);

      return [distanceFieldDescription, measurementDate];
    }

    #region Private helpers

    private static List<CIMDimensionStyle> GetCIMDimensionStyles()
    {
      CIMColor color = CIMColor.CreateRGBColor(10, 20, 30);
      CIMPointSymbol beginMarkerSymbol = GetMarkerSymbol(color);
      CIMPointSymbol endMarkerSymbol = GetMarkerSymbol(color);
      CIMLineSymbol dimensionLineSymbol = GetLineSymbol(color, 0.5);
      CIMLineSymbol extensionLineSymbol = GetLineSymbol(color, 0.5);
      CIMTextSymbol textSymbol = GetDefaultTextSymbol(10);
      textSymbol.Symbol = GetPolygonSymbol(color, color, 0);
      textSymbol.HorizontalAlignment = HorizontalAlignment.Center;

      CIMDimensionStyle cimDimensionStyle = new CIMDimensionStyle()
      {
        Name = "Style 1",

        Align = true,
        DisplayUnits = null,
        DisplayPrecision = 0,

        BeginMarkerSymbol = beginMarkerSymbol,
        EndMarkerSymbol = endMarkerSymbol,
        MarkerOption = DimensionPartOptions.Both,
        MarkerFit = DimensionMarkerFit.Text,

        DimensionLineSymbol = dimensionLineSymbol,
        DimensionLineOption = DimensionPartOptions.Both,
        DrawLineOnFit = false,
        ExtendLineOnFit = true,

        BaselineHeight = 0.0,
        ExtensionLineSymbol = extensionLineSymbol,
        ExtensionLineOption = DimensionPartOptions.Both,
        ExtensionLineOvershot = 0.0,
        ExtensionLineOffset = 0.0,

        Expression = "",
        ExpressionParserName = "Arcade",

        TextSymbol = textSymbol,
        TextOption = DimensionTextOption.Only,
        TextFit = DimensionTextFit.MoveBegin,
      };

      List<CIMDimensionStyle> cimDimensionStyles = [cimDimensionStyle];

      return cimDimensionStyles;
    }

    private static CIMFill DefaultFill(CIMColor color)
    {
      return new CIMSolidFill()
      {
        Enable = true,
        Name = "Fill_" + Guid.NewGuid().ToString(),
        ColorLocked = false,
        Color = color
      };
    }

    private static CIMStroke DefaultStroke(CIMColor color, double width = 1)
    {
      return new CIMSolidStroke()
      {
        Color = color,
        Name = "Stroke_" + Guid.NewGuid().ToString(),
        CapStyle = LineCapStyle.Round,
        JoinStyle = LineJoinStyle.Round,
        CloseCaps3D = false,
        LineStyle3D = Simple3DLineStyle.Strip,
        MiterLimit = 4,
        Width = width,

        ColorLocked = false,
        Enable = true,
      };
    }

    private static CIMPolygonSymbol GetPolygonSymbol(CIMColor fillColor, CIMColor outlineColor, double outlineWidth)
    {
      if ((outlineColor == null) || (outlineWidth <= 0))
        return new CIMPolygonSymbol() { SymbolLayers = new CIMSymbolLayer[1] { DefaultFill(fillColor) } };
      return new CIMPolygonSymbol() { SymbolLayers = new CIMSymbolLayer[2] { DefaultStroke(outlineColor, outlineWidth), DefaultFill(fillColor) } };
    }

    private static CIMLineSymbol GetLineSymbol(CIMColor color, double width)
    {
      var stroke = DefaultStroke(color, width);

      return new CIMLineSymbol() { SymbolLayers = new CIMSymbolLayer[1] { stroke } };
    }

    private static Polygon CreateArrowGeometry()
    {
      double x = 2.39;
      double y = 1.20;
      double xMod = 0.3;

      var pt1 = new Coordinate2D() { X = -x + xMod, Y = -y };
      var pt2 = new Coordinate2D() { X = x + xMod, Y = 0 };
      var pt3 = new Coordinate2D() { X = -x + xMod, Y = y };
      var coords = new List<Coordinate2D>() { pt1, pt2, pt3 };

      var polygon = PolygonBuilderEx.CreatePolygon(coords);
      return polygon;
    }

    private static CIMPointSymbol GetMarkerSymbol(CIMColor color)
    {
      CIMMarkerGraphic graphic = new CIMMarkerGraphic()
      {
        Geometry = CreateArrowGeometry(),
        Symbol = new CIMPolygonSymbol()
        {
          SymbolLayers = new CIMSymbolLayer[1]
          {
        new CIMSolidFill()
        {
          Enable = true,
          Color = color,
        }
          },
          UseRealWorldSymbolSizes = false
        }
      };

      CIMSymbolLayer symbolLayer = new CIMVectorMarker()
      {
        Enable = true,
        Size = 12,
        Frame = new EnvelopeBuilderEx() { XMin = -5, YMin = -3, XMax = 5, YMax = 3 }.ToGeometry() as Envelope,
        MarkerGraphics = new CIMMarkerGraphic[1] { graphic },
        ScaleSymbolsProportionally = true,
        RespectFrame = true
      };

      return new CIMPointSymbol()
      {
        SymbolLayers = new CIMSymbolLayer[1] { symbolLayer },
        HaloSize = 1,
        ScaleX = 1,
        AngleAlignment = AngleAlignment.Map,
      };
    }

    private static CIMTextSymbol GetDefaultTextSymbol(double height)
    {
      return new CIMTextSymbol()
      {
        DrawGlyphsAsGeometry = false,
        DrawSoftHyphen = false,
        ExtrapolateBaselines = true,
        FlipAngle = 0.0,
        IndentAfter = 0.0,
        IndentBefore = 0.0,
        IndentFirstLine = 0.0,
        Kerning = true,
        LetterSpacing = 0.0,
        LetterWidth = 100.0,
        Ligatures = true,
        LineGap = 0.0,
        LineGapType = LineGapType.ExtraLeading,
        Underline = false,
        Strikethrough = false,
        OffsetX = 0.0,
        OffsetY = 0.0,
        FontFamilyName = "Tahoma",
        FontStyleName = "Regular",
        WordSpacing = 100.0,
        Height = height
      };
    }
    #endregion
  }
}
