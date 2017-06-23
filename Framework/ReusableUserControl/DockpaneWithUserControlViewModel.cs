/*

   Copyright 2017 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ReusableUserControl.Model;
using System.Windows.Threading;
using System.Windows.Input;

namespace ReusableUserControl
{
	internal class DockpaneWithUserControlViewModel : DockPane
	{
		private const string _dockPaneID = "ReusableUserControl_DockpaneWithUserControl";
		private DispatcherTimer _timer = null;

		protected DockpaneWithUserControlViewModel()
    {
      // program a timer to update the age field asynchronously
      _timer = new DispatcherTimer();
			_timer.Interval = TimeSpan.FromSeconds(4);
			_timer.Tick += Timer_Tick;
			_timer.Start();
			Person = new PersonModel()
			{
				Age = 1,
				Name = "Tester",
				Email = "Tester@acme.com"
			};
		}

		void Timer_Tick(object sender, EventArgs e)
		{
			if (Person.Age > 60) Person.Age = 0;
			Person.Age++;
		}

		private ICommand _saveInfo = null;

		public ICommand SaveInfo
		{
			get
			{
				if (_saveInfo == null)
        {
          _saveInfo = new RelayCommand(() => 
          {
            // save the info from the current user control
            _timer.IsEnabled = false;
            var output = $@"Name: {Person.Name}, {Person.Age} years old, Email: {Person.Email}";
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(output);
						_timer.IsEnabled = true;
					}, () => true);
				}
				return _saveInfo;
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

		public PersonModel Person { get; set; }

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "My DockPane";
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
	internal class DockpaneWithUserControl_ShowButton : Button
	{
		protected override void OnClick()
		{
			DockpaneWithUserControlViewModel.Show();
		}
	}
}
