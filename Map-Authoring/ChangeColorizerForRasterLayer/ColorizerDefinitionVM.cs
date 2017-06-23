//   Copyright 2017 Esri
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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChangeColorizerForRasterLayer
{
  class ColorizerDefinitionVM
  {

    /// <summary>
    /// Applies the RGB colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToRGBColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Defines parameters in colorizer definition.
      int bandIndexR = 2;
      int bandIndexG = 1;
      int bandIndexB = 0;
      RasterStretchType stretchType = RasterStretchType.MinimumMaximum;
      double gammaR = 2.0;
      double gammaG = 2.0;
      double gammaB = 2.0;

      await QueuedTask.Run(async () =>
      {
        // Creates a new RGB Colorizer Definition with defined parameters.
        RGBColorizerDefinition rgbColorizerDef = new RGBColorizerDefinition(bandIndexR, bandIndexG, bandIndexB, stretchType, gammaR, gammaG, gammaB);

        // Creates a new RGB colorizer using the colorizer definition created above.
        CIMRasterRGBColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(rgbColorizerDef) as CIMRasterRGBColorizer;

        // Sets the newly created colorizer on the layer.
        basicRasterLayer.SetColorizer(newColorizer);
      });
    }

    /// <summary>
    /// Applies the Stretch colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToStretchColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Defines parameters in colorizer definition.
      int bandIndex = 0;
      RasterStretchType stretchType = RasterStretchType.MinimumMaximum;
      double gamma = 2.0;
      string colorRampStyle = "ArcGIS Colors";
      string colorRampName = "Aspect";

      await QueuedTask.Run(async () =>
      {
        // Gets a color ramp from a style.
        IList<ColorRampStyleItem> rampList = GetColorRampsFromStyleAsync(Project.Current, colorRampStyle, colorRampName);
        CIMColorRamp colorRamp = rampList[0].ColorRamp;

        // Creates a new Stretch Colorizer Definition with defined parameters.
        StretchColorizerDefinition stretchColorizerDef = new StretchColorizerDefinition(bandIndex, stretchType, gamma, colorRamp);

        // Creates a new stretch colorizer using the colorizer definition created above.
        CIMRasterStretchColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(stretchColorizerDef) as CIMRasterStretchColorizer;

        // Sets the newly created colorizer on the layer.
        basicRasterLayer.SetColorizer(newColorizer);

      });

    }

    /// <summary>
    /// Applies the Discrete Color colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToDiscreteColorColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Defines values for parameters in colorizer definition.
      int numColors = 50;
      string colorRampName = "Aspect";
      string colorRampStyle = "ArcGIS Colors";

      await QueuedTask.Run(async () =>
      {
        //Get a color ramp from a style
        IList<ColorRampStyleItem> rampList = GetColorRampsFromStyleAsync(Project.Current, colorRampStyle, colorRampName);
        CIMColorRamp colorRamp = rampList[0].ColorRamp as CIMColorRamp;

        // Creates a new Discrete Colorizer Definition using the default constructor.
        DiscreteColorizerDefinition discreteColorizerDef = new DiscreteColorizerDefinition(numColors, colorRamp);

        // Creates a new Discrete colorizer using the colorizer definition created above.
        CIMRasterDiscreteColorColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(discreteColorizerDef) as CIMRasterDiscreteColorColorizer;

        // Sets the newly created colorizer on the layer.
        basicRasterLayer.SetColorizer(newColorizer);
      });

    }

    /// <summary>
    /// Applies the ColorMap colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToColorMapColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Creates a new Colormap Colorizer Definition using the default constructor.
      ColormapColorizerDefinition colorMapColorizerDef = new ColormapColorizerDefinition();

      // Creates a new Colormap colorizer using the colorizer definition created above.
      CIMRasterColorMapColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(colorMapColorizerDef) as CIMRasterColorMapColorizer;

      // Sets the newly created colorizer on the layer.
      await QueuedTask.Run(() =>
      {

        basicRasterLayer.SetColorizer(newColorizer);
      });
    }

    /// <summary>
    /// Applies the Classify colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToClassifyColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Defines values for parameters in colorizer definition.
      string fieldName = "Value";
      ClassificationMethod classificationMethod = ClassificationMethod.NaturalBreaks;
      int numberofClasses = 7;
      string colorRampStyle = "ArcGIS Colors";
      string colorRampName = "Aspect";

      await QueuedTask.Run(async () =>
      {
        // Gets a color ramp from a style.
        IList<ColorRampStyleItem> rampList = GetColorRampsFromStyleAsync(Project.Current, colorRampStyle, colorRampName);
        CIMColorRamp colorRamp = rampList[0].ColorRamp;

        // Creates a new Classify Colorizer Definition using defined parameters.
        ClassifyColorizerDefinition classifyColorizerDef = new ClassifyColorizerDefinition(fieldName, numberofClasses, classificationMethod, colorRamp);

        // Creates a new Classify colorizer using the colorizer definition created above.
        CIMRasterClassifyColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(classifyColorizerDef) as CIMRasterClassifyColorizer;

        // Sets the newly created colorizer on the layer.
        basicRasterLayer.SetColorizer(newColorizer);
      });
    }

    /// <summary>
    /// Applies the Unique Value colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToUniqueValueColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Creates a new UV Colorizer Definition using the default constructor.
      UniqueValueColorizerDefinition UVColorizerDef = new UniqueValueColorizerDefinition();

      // Creates a new UV colorizer using the colorizer definition created above.
      CIMRasterUniqueValueColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(UVColorizerDef) as CIMRasterUniqueValueColorizer;

      // Sets the newly created colorizer on the layer.
      await QueuedTask.Run(() =>
      {
        basicRasterLayer.SetColorizer(newColorizer);
      });
    }

    /// <summary>
    /// Applies the Vector Field colorizer to the basic raster layer.
    /// </summary>
    /// <param name="basicRasterLayer">The basic raster layer is either a raster or image service layer, or the image sub-layer of the mosaic layer.</param>
    /// <returns></returns>
    public static async Task SetToVectorFieldColorizer(BasicRasterLayer basicRasterLayer)
    {
      // Defines values for parameters in colorizer definition.
      bool isUVComponents = false;
      int magnitudeBandIndex = 0;
      int directionBandIndex = 1;
      SymbolizationType symbolizationType = SymbolizationType.SingleArrow;

      await QueuedTask.Run(async () =>
      {

        // Creates a new Vector Field Colorizer Definition using defined parameters.
        VectorFieldColorizerDefinition vectorFieldColorizerDef = new VectorFieldColorizerDefinition(magnitudeBandIndex, directionBandIndex, isUVComponents, symbolizationType);

        // Creates a new Vector Field colorizer using the colorizer definition created above.
        CIMRasterVectorFieldColorizer newColorizer = await basicRasterLayer.CreateColorizerAsync(vectorFieldColorizerDef) as CIMRasterVectorFieldColorizer;

        // Sets the newly created colorizer on the layer.
        basicRasterLayer.SetColorizer(newColorizer);
      });
    }

    /// <summary>
    /// Gets the color ramp from the color ramp style.
    /// </summary>
    /// <param name="project">The current project.</param>
    /// <param name="styleName">A string represents the color ramp style name. For example, "ArcGIS Colors".</param>
    /// <param name="rampName">A string represents the color ramp name. For example, "Aspect".</param>
    /// <returns></returns>
    private static IList<ColorRampStyleItem> GetColorRampsFromStyleAsync(Project project, string styleName, string rampName)
    {
      try
      {
        // Gets a collection of style items in project that matches the given style name.
        StyleProjectItem styleItem =
        project.GetItems<StyleProjectItem>().FirstOrDefault(style => (style.Name.Equals(styleName, StringComparison.CurrentCultureIgnoreCase)));

        if (styleItem == null)
          throw new System.ArgumentNullException();

        //Search for color ramp by name in the collection of style items.
        return styleItem.SearchColorRamps(rampName);

      }
      catch (Exception ex)
      {

        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(null, $@"Error in GetColorRampsFromStyleAsync: {ex.Message}", MessageBoxButton.OK, MessageBoxImage.Error);

        return null;
      }
    }

  }
}