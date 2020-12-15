using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace GraphicTools
{
    internal class MapQueryTool : MapTool
    {
        private TextPaneViewModel _dockpane;
        public MapQueryTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {

                QueuedTask.Run(() =>
                {
                    try
                    {
                        var layers = MapView.Active.Map.GetLayersAsFlattenedList();
                        var fL = layers.FirstOrDefault(l => l is FeatureLayer) as FeatureLayer;
                        // if there is no featurelayer in the map then exit
                        if (fL == null)
                        {
                            return;
                        }

                        var fLayer = MapView.Active.Map.FindLayers("Overall SVI - Tracts").FirstOrDefault() as BasicFeatureLayer;
                        if (fLayer == null)
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Tracts layer is not found.", "Error");
                            return;
                        }
                        RowCursor rowCursor = null;
                        // define a spatial query filter
                        var spatialQueryFilter = new SpatialQueryFilter
                        {
                            // passing the search geometry to the spatial filter
                            FilterGeometry = geometry,
                            // define the spatial relationship between search geometry and feature class
                            SpatialRelationship = SpatialRelationship.Intersects
                        };
                        // apply the spatial filter to the feature layer in question
                        rowCursor = fLayer.Search(spatialQueryFilter);

                        RowHandle rowHandle = null;
                        var pctAge65 = string.Empty;
                        if (rowCursor.MoveNext())
                        {
                            var row = rowCursor.Current;
                            rowHandle = new RowHandle(row);
                            pctAge65 = Convert.ToString(row["EP_AGE65"]);
                        }

                        if (rowHandle != null)
                        {
                            // Get the editbox or the dockpane textbox value
                            string txtBoxString = null;
                            if (Module1.Current.blnDockpaneOpenStatus == false)
                            {
                                // Use value in the edit box
                                txtBoxString = Module1.Current.QueryValueEditBox.Text;
                                if (txtBoxString == null || txtBoxString == "")
                                {
                                    Module1.Current.QueryValueEditBox.Text = "Age > 65:  " + pctAge65 + "%";
                                }
                                else
                                {
                                    MessageBox.Show("Query value exists on Ribbon", "Query Text");
                                }
                            }
                            if (Module1.Current.blnDockpaneOpenStatus == true)
                            {
                                _dockpane = FrameworkApplication.DockPaneManager.Find("GraphicTools_TextPane") as TextPaneViewModel;
                                txtBoxString = _dockpane.QueryTxtBoxDoc;
                                if (txtBoxString == null || txtBoxString == "")
                                {
                                    _dockpane.QueryTxtBoxDoc = "Age > 65:  " + pctAge65 + "%";
                                }
                                else
                                {
                                    MessageBox.Show("Query value exists in Text pane", "Query Text");
                                }
                                
                            }

                        }
                    }
                    catch (Exception exc)
                    {
                        // Catch any exception found and display in a message box
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught: " + exc.Message);
                        return;
                    }
                });

            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
