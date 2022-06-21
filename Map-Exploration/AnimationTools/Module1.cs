/*

   Copyright 2018 Esri

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
using System.Linq;
using System.Text;
using System.Windows.Input;
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
using ArcGIS.Desktop.Core.Geoprocessing;

namespace AnimationTools
{
  /// <summary>
  /// This sample illustrates how animation keyframes can be generated with camera positions which follow 3D vehicle features.  There is the option to create keyframes with durations which simulate vehicle speed, or a brief 10-second animation. The animations are ready for export to video using the standard animation tools.  
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains a dataset called 'VehicleAnimation'.  Make sure that the Sample data is unzipped under c:\data and the folder "C:\Data\VehicleAnimation\" is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open the project "VehicleAnimation.aprx" found in folder "C:\Data\VehicleAnimation\".
  /// 1. The demo dataset contains polyline route and 3D point layers for three different vehicles – a car, sailboat and helicopter.  There project’s bookmarks which will take you to the start of each vehicle route.  The project opens at the beginning of the car route. Each of the 3D point layers has a range already set so that only one vehicle point feature is visible at a time.
  /// 1. Click on the Animation Tab, and you will find two new sample groups added to the end of the tab – "Vehicle Animation Settings" and Build Animation".  
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click on the Timeline button in the Playback group to open the Timeline pane.  
  /// 1. First, create a speed animation for the car.  Enter your desired speed value in the edit box at the top of the Vehicle Animation Settings group or proceed with the default value of 55 miles per hour.
  /// 1. Make sure that "Car" is chosen in the combobox in the Vehicle Animation Settings group.
  /// 1. Press the "Speed Animation" button in the Build Animation group. You will be prompted to confirm if your choices are correct.  If so, press "OK".
  /// 1. New keyframes will be built in the animation timeline. The speed animation option builds the keyframes with a duration allowing for the simulation of the correct speed in miles per hour, for the car to travel the full route.  Using the default speed of 55 mph, the car animation duration will be just over 28 seconds.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. You will see an Info message box that informs you that you can view the timeline, and a recommended Frames Per Second setting value to use when exporting a video from the animation. Note and use that value for best results based on the full duration of the animation.
  /// 1. Play the animation and/or export a video to see the car movement and note how the keyframes have been generated with camera settings which allow close viewing of the vehicle’s movement along the route.
  /// 1. Next, create a 10-second animation for the car.  Delete the existing animation, or create a new animation, using the tools in the "Manage" group.  Reminder:  It is essential that the timeline window be empty of keyframes showing the "Create first keyframe" button before proceeding with building an animation.
  /// 1. Ensure the Car option is chosen in the settings combobox, and press the "10-Second Animation" button in the Build Animation group.  When prompted to confirm, press "OK".
  /// 1. As before, new keyframes will be built in the animation timeline. The 10-second animation option builds the keyframes for a total animation duration of 10 seconds.  You will see the Info message box again informing you to export videos this time with a 25 Frames Per Second setting, which is optimal for the 10-second duration with approximately 250 keyframes generated.
  /// ![UI](Screenshots/Screen4.png)
  /// 1. Again, play the new animation and/or export a video to see the car movement and how the keyframes have been generated with camera settings which allow close viewing of the vehicles movement along the route.
  /// 1. If you like, repeat the steps above using the sailboat and helicopter vehicles.  Use the project bookmarks to move to the beginning of their respective routes.
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AnimationTools_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            return true;
        }

        #endregion Overrides

        #region Business Logic

        public SpeedEditBox SpeedValueEditBox { get; set; }

        public VehicleListComboBox VehicleListComboBoxValue { get; set; }
        public void GenerateKeyframes(string animationType)
        {


            try
            {

                QueuedTask.Run(() =>
                {

                    if (VehicleListComboBoxValue.Text == "" || VehicleListComboBoxValue.Text == null)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No Vehicle Route selected. Exiting...", "Value Needed");
                        return;
                    }

                    string VehiclePointsLayer = VehicleListComboBoxValue.Text + "_Route";

                    // Iterate and list the layers within the group layer
                    // string pointLayerName = System.IO.Path.GetFileName(outfc);
                    var pointLayer = MapView.Active.Map.FindLayers("VehiclePoints_" + VehiclePointsLayer).FirstOrDefault() as BasicFeatureLayer;
                    if (pointLayer == null)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Can't find the layer: " + "VehiclePoints_" + VehiclePointsLayer + ". Exiting...", "Info");
                        return;
                    }

                    // Ensure there's a value in the speed edit box
                    if (SpeedValueEditBox.Text == "" && animationType == "use speed")
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("The Speed in MPH value Editbox is empty. Exiting...", "Value Needed");
                        SpeedValueEditBox.Text = "";
                        return;
                    }

                    double speedValueMPH;
                    bool isNumeric = Double.TryParse(SpeedValueEditBox.Text, out speedValueMPH);
                    // Ensure there's a value in the speed edit box
                    if (isNumeric != true && animationType == "use speed")
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("The Speed in MPH value Editbox is not invalid. Type in a new value. Exiting...", "Value Needed");
                        SpeedValueEditBox.Text = "";
                        return;
                    }

                    // Prompt for confirmation, and if answer is no, return.
                    if (animationType == "use speed")
                    {
                        var result = MessageBox.Show("Confirm building of " + VehicleListComboBoxValue.Text.ToUpper() + " animation simulating vehicle speed of " + SpeedValueEditBox.Text + " MPH?", "Build Animation", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);
                        // Return if cancel value is chosen
                        if (Convert.ToString(result) == "Cancel")
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Operation Canceled.", "Info");
                            return;
                        }
                    }
                    else if (animationType == "ten-second")
                    {
                        var result = MessageBox.Show("Confirm building of " + VehicleListComboBoxValue.Text.ToUpper() + " 10-SECOND duration animation?", "Build Animation", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);
                        // Return if cancel value is chosen
                        if (Convert.ToString(result) == "Cancel")
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Operation Canceled.", "Info");
                            return;
                        }
                    }


                    int viewDistance = 0;
                    if (VehiclePointsLayer == "Car_Route")
                    {
                        viewDistance = 100;
                    }
                    else if (VehiclePointsLayer == "Boat_Route")
                    {
                        viewDistance = 200;
                    }
                    else if (VehiclePointsLayer == "Helicopter_Route")
                    {
                        viewDistance = 300;
                    }



                    // SET UP 3D MODEL SYMBOL AND SETTINGS

                    // Use the speed value to set keyframe timespan - Calculate distance covered in seconds and milliseconds based on speed and distance between points
                    // NOTE:  Assumes map units are in feet.  This is key.
                    // Calculate the Timespan using speed:

                    int rowNumber;
                    RowCursor pointsRowcursor;
                    long numPoints;
                    // Tempororily set to empty timespans
                    TimeSpan temp_span = new TimeSpan(0, 0, 0, 0, 0);
                    TimeSpan temp_newTimeSpan = new TimeSpan(0, 0, 0, 0, 0);

                    if (animationType == "use speed")
                    {

                        // Get the value of the distance between features.
                        rowNumber = 0;
                        MapPoint mapPoint1 = null;
                        MapPoint mapPoint2 = null;
                        pointsRowcursor = pointLayer.Search(null);
                        numPoints = pointLayer.GetTable().GetCount();
                        while (rowNumber < 3)
                        {
                            rowNumber += 1;
                            using (Row currentRow = pointsRowcursor.Current)
                            {
                                if (pointsRowcursor.MoveNext())
                                {
                                    using (var pointFeature = pointsRowcursor.Current as Feature)
                                    {
                                        if (rowNumber == 1)
                                        {
                                            mapPoint1 = pointFeature.GetShape().Clone() as MapPoint;
                                        }
                                        else if (rowNumber == 2)
                                        {
                                            mapPoint2 = pointFeature.GetShape().Clone() as MapPoint;
                                        }
                                    }
                                }
                            }

                        }

                        pointsRowcursor.Dispose();

                        double dblDistBetween = GeometryEngine.Instance.Distance(mapPoint1, mapPoint2);
                        double dblFeetPerSecond = (speedValueMPH * 5280) / 3600;
                        double dblSecondsPerPoint = dblDistBetween / dblFeetPerSecond;

                        Double dblSec = Math.Truncate(dblSecondsPerPoint);
                        Double dblMilliSec = dblSecondsPerPoint - dblSec;
                        // round it
                        dblMilliSec = Math.Round(dblMilliSec, 3);
                        string strFixMilliSec = Convert.ToString(dblMilliSec);
                        strFixMilliSec = strFixMilliSec.Remove(0, 2);
                        Int32 intMilliSec = Convert.ToInt32(strFixMilliSec);
                        Int32 intSec = Convert.ToInt32(dblSec);

                        temp_span = new TimeSpan(0, 0, 0, intSec, intMilliSec);
                        temp_newTimeSpan = new TimeSpan(0, 0, 0, 0, 0);

                    }

                    else if (animationType == "ten-second")
                    {

                        temp_span = new TimeSpan(0, 0, 0, 0, 40);
                        temp_newTimeSpan = new TimeSpan(0, 0, 0, 0, 0);

                    }

                    TimeSpan span = temp_span;
                    TimeSpan newTimeSpan = temp_newTimeSpan;

                    var currentmapview = MapView.Active;
                    var mapAnimation = currentmapview.Map.Animation;
                    var currentcamera = currentmapview.Camera;
                    var newCamera = currentcamera;

                    // Use GetDisconnectedTracks
                    List<Track> tracks = mapAnimation.GetDisconnectedTracks();
                    var cameraTrack = tracks.OfType<CameraTrack>().First();
                    var rangeTrack = tracks.OfType<RangeTrack>().First();

                    int cameraJumpVal = 0;
                    rowNumber = 0;
                    string cameraSetting = "perspective";
                    Feature prevFeature = null;
                    MapPoint prevPoint = null;
                    pointsRowcursor = pointLayer.Search(null);
                    numPoints = pointLayer.GetTable().GetCount();
                    while (pointsRowcursor.MoveNext())
                    {
                        rowNumber += 1;
                        using (var pointFeature = pointsRowcursor.Current as Feature)
                        {
                            var mapPoint = pointFeature.GetShape().Clone() as MapPoint;
                            // if prevPoint == null, skip creation of keyframe 
                            if (prevFeature != null)
                            {
                                double bearingRadians = 0;
                                string bearingValForKeyframe = Convert.ToString(pointFeature["BEARING"]);

                                if (rowNumber == 2)  // View from behind vehicle to begin animation
                                {
                                    double bearingVal = Convert.ToDouble(pointFeature["BEARING"]) + 180;
                                    bearingRadians = (Math.PI / 180) * bearingVal;
                                    cameraJumpVal = 12;
                                }
                                else if (rowNumber == numPoints)  // Last point, build keyframe with camera facing the front of the vehicle:
                                {
                                    double bearingVal = Convert.ToDouble(pointFeature["BEARING"]) + 0;
                                    bearingRadians = (Math.PI / 180) * bearingVal;
                                    cameraJumpVal = 12;

                                }
                                else  // View from the side
                                {
                                    double bearingVal = Convert.ToDouble(pointFeature["BEARING"]) + 270;
                                    bearingRadians = (Math.PI / 270) * bearingVal;

                                }

                                double xNew = mapPoint.X + viewDistance * Math.Cos(Math.PI / 2 - bearingRadians);
                                double yNew = mapPoint.Y + viewDistance * Math.Sin(Math.PI / 2 - bearingRadians);

                                MapPoint newprevPoint = MapPointBuilderEx.CreateMapPoint(xNew, yNew);
                                double headingCalc = CalculateHeading(newprevPoint, mapPoint);
                                newCamera.Heading = headingCalc;

                                newCamera.X = xNew;
                                newCamera.Y = yNew;

                                if (VehiclePointsLayer == "Helicopter_Route")
                                {
                                    newCamera.Z = mapPoint.Z + 60;
                                }
                                else
                                {
                                    newCamera.Z = 60;
                                }

                                newCamera.Pitch = -5;
                                newCamera.Viewpoint = CameraViewpoint.LookAt;

                                double sequenceVal = Convert.ToDouble(pointFeature["Sequence"]);
                                ArcGIS.Desktop.Mapping.Range newRange = new ArcGIS.Desktop.Mapping.Range
                                {
                                    Min = sequenceVal,
                                    Max = sequenceVal
                                };

                                // Create and edit the keyframes
                                rangeTrack.CreateKeyframe(newRange, newTimeSpan, AnimationTransition.Linear);
                                //newTimeSpan = newTimeSpan.Add(span);

                                if (cameraSetting == "perspective" && cameraJumpVal == 12)
                                {
                                    // rangeTrack.CreateKeyframe(newRange, newTimeSpan, AnimationTransition.Linear);
                                    // Set up side / angle perspective for the camera and create keyframe
                                    cameraTrack.CreateKeyframe(newCamera, newTimeSpan, AnimationTransition.Linear);
                                    cameraJumpVal = 0;
                                }
                                newTimeSpan = newTimeSpan.Add(span);
                            }

                            prevFeature = pointFeature;
                            prevPoint = prevFeature.GetShape() as MapPoint;
                        }
                        cameraJumpVal += 1;
                    }


                    // Wrap up the animation
                    mapAnimation.Tracks = tracks;

                    // Final calculation for the recommendation on keyframes for video export:
                    // Get decimal seconds of newTimeSpan and use numPoints for number of keyframes
                    double keyframesTotal = Convert.ToDouble(numPoints);
                    double seconds = newTimeSpan.TotalSeconds;
                    double kpsValue = keyframesTotal / seconds;
                    double reccomendedKPS;
                    if (kpsValue > 50 )
                    {
                        var kpsDivision = kpsValue / 25;
                        int kpsDivider = Convert.ToInt16(Math.Truncate(kpsDivision));
                        reccomendedKPS = kpsValue / kpsDivider;
                    }
                    else
                    {
                        reccomendedKPS = kpsValue;
                    }

                    string strReccKPS = Convert.ToString(Math.Round(reccomendedKPS, 0));

                    MessageBox.Show("Animation built.  View timeline and/or export movie. \r\n" + 
                        "NOTE:  Recommended Frames Per Second setting for Export: \r\n" +
                        strReccKPS + " Frames Per Second", "Info");

                });

            }

            catch (Exception ex)
            {
                MessageBox.Show("Error in GenerateKeyframes:  " + ex.ToString(), "Error");
            }


        }   // End of GenerateKeyframes()


        public static double CustomPitch = 0;
        private static double Z_CONVERSION_FACTOR = 1;

        // Map Authoring Snippet
        private Task ApplySymbolToFeatureLayerAsync(FeatureLayer featureLayer, string symbolName)
        {

            return QueuedTask.Run(async () =>
            {
                //Get the ArcGIS 2D System style from the Project
                var arcGIS3DStyle = Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "Sample 3D Models");

                //Search for the symbolName style items within the ArcGIS 2D style project item.
                var items = await QueuedTask.Run(() => arcGIS3DStyle.SearchSymbols(StyleItemType.PointSymbol, symbolName));

                //Gets the CIMSymbol
                CIMSymbol symbol = items.FirstOrDefault().Symbol;

                //Get the renderer of the point feature layer
                CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

                //Set symbol's real world setting to be the same as that of the feature layer
                symbol.SetRealWorldUnits(featureLayer.UsesRealWorldSymbolSizes);

                //Apply the symbol to the feature layer's current renderer
                renderer.Symbol = symbol.MakeSymbolReference();
                try
                {
                    //Appy the renderer to the feature layer
                    featureLayer.SetRenderer(renderer);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in ApplySymbolToFeatureLayerAsync:  " + ex.ToString(), "Error");
                }
            });
        }

        // Set range for feature layer
        // Map Exploration Team Routine
        private Task SetRangeProperties(FeatureLayer ftrLyr, string fieldName, bool hasCustomRange = false, double customMin = 0, double customMax = 1, bool inSingleValueMode = false, int rangeStepCount = 0)
        {
            return QueuedTask.Run(() =>
            {
                //Get min/max values for the field
                double minValue = 0;
                double maxValue = 0;

                //Calculate min/max if custom range is not defined
                if (!hasCustomRange)
                {
                    int ftrCount = 0;
                    RowCursor rows = ftrLyr.Search(null);
                    while (rows.MoveNext()) ftrCount++;
                    double[] fieldVals = new double[ftrCount];
                    rows = ftrLyr.Search(null);
                    //Looping through to count
                    int i = 0;
                    while (rows.MoveNext())
                    {
                        using (var currentRow = rows.Current)
                        {
                            object origVal = currentRow.GetOriginalValue(rows.FindField(fieldName));
                            if (!(origVal is DBNull))
                                fieldVals[i] = System.Convert.ToDouble(origVal);
                            i++;
                        }
                    }
                    if (fieldVals.Count() > 0)
                    {
                        minValue = fieldVals.Min();
                        maxValue = fieldVals.Max();
                    }
                }
                else
                {
                    minValue = customMin;
                    maxValue = customMax;
                }

                CIMBasicFeatureLayer baseLyr = (CIMBasicFeatureLayer)ftrLyr.GetDefinition();
                CIMFeatureTable ftrTable = baseLyr.FeatureTable;

                //CIMRange theRange = new CIMRange();
                CIMRangeDefinition[] rangeDefn = new CIMRangeDefinition[1];
                rangeDefn[0] = new CIMRangeDefinition();
                rangeDefn[0].FieldName = fieldName;
                rangeDefn[0].Name = fieldName;
                rangeDefn[0].CustomFullExtent = new CIMRange();
                rangeDefn[0].CustomFullExtent.Min = minValue;
                rangeDefn[0].CustomFullExtent.Max = maxValue;

                //Set current range with either step count == 1 OR for some cases with step count == 0
                rangeDefn[0].CurrentRange = new CIMRange();
                rangeDefn[0].CurrentRange.Min = minValue;
                //rangeDefn[0].CurrentRange.Max = (rangeStepCount == 1) ? minValue + 1 : minValue;

                //set range step to 0 if in single value mode and to rangeStepCount otherwise
                rangeDefn[0].CurrentRange.Max = (inSingleValueMode) ? minValue : minValue + rangeStepCount;
                //rangeDefn[0].CurrentRange.Max = maxValue;

                ftrTable.RangeDefinitions = rangeDefn;
                ftrTable.ActiveRangeName = fieldName;
                baseLyr.FeatureTable = ftrTable;
                ftrLyr.SetDefinition(baseLyr);
            });
        }



        private static double CalculateHeading(MapPoint startPt, MapPoint endPt)
        {
            string SelectedCameraView = "Top down";
            double dx, dy, dz, angle, heading;

            if (SelectedCameraView == "Top down - face north")
            {
                heading = 0;
            }
            else
            {
                dx = endPt.X - startPt.X;
                dy = endPt.Y - startPt.Y;

                //need to apply z-conversion factor to target Z
                dz = (SelectedCameraView == "Face target") ? endPt.Z * Z_CONVERSION_FACTOR - startPt.Z : endPt.Z - startPt.Z;

                angle = Math.Atan2(dy, dx);
                heading = 180 + (90 + angle * 180 / Math.PI);

                if (SelectedCameraView == "Face backward") { heading = heading - 180; }
            }

            return heading;
        }



        #endregion





    }
}
