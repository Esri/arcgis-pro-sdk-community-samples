/*

   Copyright 2018 Esri

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ContentFileExplorer.Helpers;

namespace ContentFileExplorer
{
	internal class FileExplorerViewModel : DockPane
	{
		private const string _dockPaneID = "ContentFileExplorer_FileExplorer";

		protected FileExplorerViewModel() {
			DataPath = @"c:\data";
		}

		#region Properties

		private string _dataPath;
		public string DataPath
		{
			get { return _dataPath; }
			set
			{
				SetProperty(ref _dataPath, value, () => DataPath);
			}
		}

		private List<FileItemBase> _fileItems ;
		public List<FileItemBase> FileItems
		{
			get { return _fileItems; }
			set
			{
				SetProperty(ref _fileItems, value, () => FileItems);
			}
		}


		#endregion Properties

		#region Commands

		public ICommand CmdRefresh {
			get {
				return new RelayCommand(() => {
					var newFileItems = new List<FileItemBase>();
					if (Directory.Exists(DataPath))
					{
						newFileItems.Add(new FolderItem(DataPath));
					}
					FileItems = newFileItems;
				}, true);
			}
		}

		#endregion Commands

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Explore file content";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class FileExplorer_ShowButton : Button
	{
		protected override void OnClick()
		{
			FileExplorerViewModel.Show();
		}
	}
}
