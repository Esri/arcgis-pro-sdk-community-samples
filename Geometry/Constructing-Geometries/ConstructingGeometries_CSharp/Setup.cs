using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ConstructingGeometries
{
    /// <summary>
    /// Ensure that the prerequisites for the SDK sample are met and then enable subsequently enabled the buttons to construct the geometries.
    /// </summary>
    internal class Setup : Button
    {
        protected override void OnClick()
        {
            ConstructingGeometriesModule.PrepareTheSample();

            // activate the prerequisite state that the expected layers exist
            FrameworkApplication.State.Activate("layers_exists");
        }
    }
}
