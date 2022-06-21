/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentFileExplorer.Helpers
{
	public class FolderItem : FileItemBase
	{
		private FolderItem _parentFolderItem = null;

		public FolderItem(string theFolder)
		{
			Folder = theFolder;
		}

		public FolderItem(FolderItem parentFolderItem, string theFolder)
		{
			_parentFolderItem = parentFolderItem;
			Folder = theFolder;
		}

		private string _folder = string.Empty;

		public string Folder
		{
			get { return _folder; }
			set
			{
				SetProperty(ref _folder, value, () => Folder);
				base.Name = Path.GetFileName(_folder);
			}
		}

		public override void LoadChildren()
		{
			// use this directory as parent and first find all child directories
			var directories = Directory.GetDirectories(_folder);
			foreach (string directory in directories)
			{
				if (Path.GetExtension(directory).ToLower().EndsWith("gdb"))
				{
					// is geodatabase
					Children.Add(new GdbDbItem(this, directory));
				}
				else
				{
					// is folder
					Children.Add(new FolderItem(this, directory));
				}
			}
			// also we need to check for shape files
			var shapeFiles = Directory.GetFiles(_folder, "*.shp");
			foreach (string shapeFile in shapeFiles)
			{
				Children.Add(new GdbItem(this, Path.GetFileName(shapeFile)));
			}
		}
	}
}
