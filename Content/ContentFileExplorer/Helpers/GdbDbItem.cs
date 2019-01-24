/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
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
