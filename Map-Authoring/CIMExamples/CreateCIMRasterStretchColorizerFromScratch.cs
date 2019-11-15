
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CIMExamples
{
    /// <summary>
    /// Provide sample to create CIMRasterStretchColorizer from scratch
    /// </summary>
    internal class CreateCIMRasterStretchColorizerFromScratch : Button
    {

        protected override async void OnClick()
        {

            var rasterLayer =
                MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault() as RasterLayer;
            if (rasterLayer == null)
            {
                MessageBox.Show("Please add a raster as layer 0", "Cannot find layer");
                return;
            }

            await QueuedTask.Run(async () =>
            {
                var renderer = rasterLayer.GetColorizer();

                var parametersMin = Geoprocessing.MakeValueArray(rasterLayer, "MINIMUM");
                var parametersMax = Geoprocessing.MakeValueArray(rasterLayer, "MAXIMUM");
                var minRes = await Geoprocessing.ExecuteToolAsync("GetRasterProperties_management", parametersMin);
                var maxRes = await Geoprocessing.ExecuteToolAsync("GetRasterProperties_management", parametersMax);

                var min = Convert.ToDouble(minRes.Values[0]);
                var max = Convert.ToDouble(maxRes.Values[0]);

                var layerCimRenderer = CreateStretchRendererFromScratch(min, max);

                //For examination in the debugger..
                string xmlDef = renderer.ToXml();
                string xmlDef2 = layerCimRenderer.ToXml();

                rasterLayer.SetColorizer(layerCimRenderer);

            });

        }

        /// <summary>
        /// Warning! You must call this method on the MCT!
        /// </summary>
        /// <returns></returns>
        private CIMRasterStretchColorizer CreateStretchRendererFromScratch(double min, double max)
        {
            //All of these methods have to be called on the MCT
            if (Module1.OnUIThread)
                throw new CalledOnWrongThreadException();

            var colors = getColors();

            var multiPartRamp = new CIMMultipartColorRamp
            {
                Weights = new double[colors.GetLength(0)]
            };
            CIMColorRamp[] rampValues = new CIMColorRamp[colors.GetLength(0)];
            for (int i = 0; i < colors.GetLength(0) - 1; i++)
            {
                var ramp = new CIMPolarContinuousColorRamp();
                var r = colors[i, 0];
                var g = colors[i, 1];
                var b = colors[i, 2];
                ramp.FromColor = new CIMRGBColor() { R = r, G = g, B = b };
                r = colors[i + 1, 0];
                g = colors[i + 1, 1];
                b = colors[i + 1, 2];
                ramp.ToColor = new CIMRGBColor() { R = r, G = g, B = b };
                ramp.PolarDirection = PolarDirection.Clockwise;
                rampValues[i] = ramp;
                multiPartRamp.Weights[i] = 1;
            }
            multiPartRamp.Weights[colors.GetLength(0) - 1] = 1;

            multiPartRamp.ColorRamps = rampValues;

            var colorizer = new CIMRasterStretchColorizer
            {
                ColorRamp = multiPartRamp,
                StretchType = RasterStretchType.StandardDeviations,
                StandardDeviationParam = 2,
                // in order to see the label on the layer we must add at least three values
                StretchClasses = new CIMRasterStretchClass[3]
            };
            colorizer.StretchClasses[0] = new CIMRasterStretchClass() { Value = min, Label = min.ToString() };
            colorizer.StretchClasses[1] = new CIMRasterStretchClass() { Value = max / 2 };
            colorizer.StretchClasses[2] = new CIMRasterStretchClass() { Value = max, Label = max.ToString() };

            return colorizer;
        }

        private int[,] getColors()
        {
            return new int[,] {{161,255,255,255},
                                {73,116,220,255},
                                {164,20,0,255},
                                {255,255,0,255}};
        }
    }
}
