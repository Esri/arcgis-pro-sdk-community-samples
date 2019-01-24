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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.ObjectModel;
using System.IO;

namespace AddInInfoManager
{
	internal class AddInInfoManagerViewModel : DockPane
	{
		private const string _dockPaneID = "AddInInfoManager_AddInInfoManager";

		protected AddInInfoManagerViewModel() {
			LstAddIn = new ObservableCollection<AddIn>();
			foreach (var addIn in AddIn.GetAddIns())
				LstAddIn.Add(addIn);
		}

		public ObservableCollection<AddIn> LstAddIn { get; set; }

		private AddIn _addinSelected;
		public AddIn AddInSelected
		{
			get { return _addinSelected; }
			set
			{
				SetProperty(ref _addinSelected, value, () => AddInSelected);
				if (_addinSelected == null) return;
				// delete the add-in
				File.Delete(_addinSelected.AddInPath);
				LstAddIn.Remove(_addinSelected);
			}
		}

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
		private string _heading = "AddIn List";
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
	internal class AddInInfoManager_ShowButton : Button
	{
		protected override void OnClick()
		{
			AddInInfoManagerViewModel.Show();
		}
	}
}
