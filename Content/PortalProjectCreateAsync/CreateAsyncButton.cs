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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProjectCreateAsync
{
  internal class CreateAsyncButton : Button
  {
    /// <summary>
    /// Creates a new portal project with the configured sample settings.
    /// </summary>
    protected override void OnClick()
    {
      var ppSettings = new CreatePortalProjectSettings() {
        Name = "PortalProjectName" + Guid.NewGuid().ToString("N"),
        Summary = "This is a sample portal project created asynchronously.",
        ShareToOrganization = true,
        Tags = new List<string> { "Sample", "Portal", "Project" },
        TemplateType = TemplateType.Map,
        // If the following default paths do not exist, and exception will be thrown.
        // Set the default home folder path for this portal project.
        DefaultFolderPath = @"C:\MyPortalProjects",
        // Set the default geodatabase and toolbox paths for this portal project.
        DefaultGeodatabasePath = @"C:\MyPortalProjects\MyPortalProject.gdb",
        DefaultToolboxPath = @"C:\MyPortalProjects\MyPortalProject.atbx",
      };
      try
      {
        var savePortalProjectResult = Project.CreateAsync(ppSettings);
        MessageBox.Show($"Portal project '{ppSettings.Name}' created successfully.");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"{ex.GetType().Name} {ex.Message}");
      }
    }
  }
}
