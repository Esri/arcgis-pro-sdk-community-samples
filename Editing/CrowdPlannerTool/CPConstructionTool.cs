using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace CrowdPlannerTool
{
    internal class CPConstructionTool : MapTool
    {
        public CPConstructionTool()
        {
            IsSketchTool = true;
            UseSnapping = true;
            SketchType = SketchGeometryType.Polygon;
            // NOTE:  Use for "freehand tool", SketchType = SketchGeometryType.Lasso;
        }

        protected override Task OnToolActivateAsync(bool hasMapViewChanged)
        {
            // Update the dockpane with values
            if (_dockpane == null)
            {
                _dockpane = FrameworkApplication.DockPaneManager.Find("CrowdPlannerTool_CPDockpane") as CPDockpaneViewModel;
            }

            // ***  Check to ensure the densitysettings are set.  If not, show a warning and deactivate tool.
            if (_dockpane.HighSetting == 0 || _dockpane.MediumSetting == 0 || _dockpane.LowSetting == 0 || _dockpane.TargetSetting == 0)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("An empty setting value exists. All settings are required to use this tool.", "Warning!");
                // Activate alternate tool to reset values check
                FrameworkApplication.SetCurrentToolAsync("esri_editing_SketchPolygonTool");
                return Task.FromResult(0);
            }


            return base.OnToolActivateAsync(hasMapViewChanged);
        }

        private CPDockpaneViewModel _dockpane;

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
        protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {

            if (CurrentTemplate == null || geometry == null)
                //return Task.FromResult(false);
                return false;

            await QueuedTask.Run(async () =>
            {
                // Create an edit operation
                var createOperation = new EditOperation();
                createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
                createOperation.SelectNewFeatures = true;
                createOperation.Create(CurrentTemplate, geometry);
                await createOperation.ExecuteAsync();
                var selectedFeatures = ArcGIS.Desktop.Mapping.MapView.Active.Map.GetSelection();
                // get the first layer and its corresponding selected feature OIDs
                var firstSelectionSet = selectedFeatures.First();
                // create an instance of the inspector class
                var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                // load the selected features into the inspector using a list of object IDs
                inspector.Load(firstSelectionSet.Key, firstSelectionSet.Value);
                var squarefeetValue = inspector["Shape_Area"];
                long squarefeetValueLong;
                squarefeetValueLong = Convert.ToInt64(squarefeetValue);
                inspector["High"] = (squarefeetValueLong / _dockpane.HighSetting);
                inspector["Medium"] = (squarefeetValueLong / _dockpane.MediumSetting);
                inspector["Low"] = (squarefeetValueLong / _dockpane.LowSetting);
                inspector["HighSetting"] = _dockpane.HighSetting;
                inspector["MediumSetting"] = _dockpane.MediumSetting;
                inspector["LowSetting"] = _dockpane.LowSetting;
                inspector["TargetSetting"] = _dockpane.TargetSetting;
                await inspector.ApplyAsync();
                _dockpane.GetTotalValues();

            });

            return true;

        }
    }
}
