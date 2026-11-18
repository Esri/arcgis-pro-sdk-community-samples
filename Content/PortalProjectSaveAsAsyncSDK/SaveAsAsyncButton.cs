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

namespace PortalProjectSaveAsAsyncSDK
{
    internal class SaveAsAsyncButton : Button
    {
				protected override async void OnClick()
        {
			// The filter string below is: esri_browseDialogFilters_projects_computerAndOnline_create_proprojects
			// Not sure if this was included in the beta snapshot for 3.8 but will be for 3.8 release.
			var filter = BrowseProjectFilter.GetFilter(ArcGIS.Desktop.Catalog.ItemFilters.PortalProjects);
			var saveDlg = new SaveItemDialog()
			{
				Title = "Save Portal Project",
				BrowseFilter = filter,
				Filter = filter.Name
			};
			var result = saveDlg.ShowDialog();
			if (result != true)
				return;

			var savePath = saveDlg.FilePath;
			// This gives the portal folder if the user selected a folder in the SaveAsPortalProject dialog. If the user did not select a folder, this will be null.
			var portalFolder = saveDlg?.PortalFolder;
			// This gives the portal group folder if the user selected a group folder in the SaveAsPortalProject dialog. If the user did not select a group folder, this will be null.
			var groupFolder = saveDlg?.PortalGroup;

			CreatePortalProjectSettings ppSettings = new CreatePortalProjectSettings
			{
				Name = savePath,
				Summary = "This is a test project created from the SaveAsPortalProject sample",
				PortalFolderItem = portalFolder,
				ShareToOrganization = true,
				TemplateType = TemplateType.Map,
				Tags = new List<string> { "SaveAsPortalProject", "ArcGIS Pro SDK" }
				// The following are settings that can be set when creating a new portal project. If not set, the existing values will be used.
				//DefaultFolderPath = "C:\\Temp",
				//DefaultGeodatabasePath = "C:\\Temp\\Default.gdb",
				//DefaultToolboxPath = "C:\\Temp\\Default.atbx",
			};

			if (groupFolder != null)
				ppSettings.PortalGroupItems = [groupFolder];

			try
			{
				await Project.Current.SaveAsAsync(ppSettings);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.GetType().Name} {ex.Message}");
			}
		}
    }
}
