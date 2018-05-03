using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;

namespace ContentFileExplorer.Helpers
{
	public class GdbDbItem : FolderItem
	{
		public GdbDbItem(FolderItem parentFolderItem, string theFolder) : base (parentFolderItem, theFolder)
		{
			Name = Path.GetFileNameWithoutExtension(theFolder);
		}
		
		public override void LoadChildren()
		{
			var addChildrenTask = QueuedTask.Run(() =>
			{
				var lstChildren = new List<GdbItem>();
				// use the geodatabase to get all layers
				var fGdbPath = new FileGeodatabaseConnectionPath(new Uri(Folder, UriKind.Absolute));
				using (var gdb = new Geodatabase(fGdbPath))
				{
					IReadOnlyList<Definition> fcList = gdb.GetDefinitions<FeatureClassDefinition>();
					//Feature class
					foreach (FeatureClassDefinition fcDef in fcList)
					{
						GdbItem gdbItem = new GdbItem(this, fcDef.GetName());
						lstChildren.Add(gdbItem);
					}
				}
				return lstChildren;
			});
			addChildrenTask.Wait();
			foreach (var gdbItem in addChildrenTask.Result)
			{
				Children.Add(gdbItem);
			}
		}

	}
}
