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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using CommandFilter.Models;

namespace CommandFilter
{
	/// <summary>
	/// Represents the customization filter. Implement <see cref="OnExecuteCommand"/> to
	/// execute your filtering logic
	/// </summary>
	/// <remarks>Customization filters must be registered via <see cref="M:FrameworkApplication.RegisterCustomizationFilter"/></remarks>
	class CommandFilter : CustomizationFilter, IDisposable
	{

		private ObservableCollection<CommandFilterItem> _clickedCommands = null;
		private bool _popMessageBox = false;
		private bool _isRegistered = false;
		private bool _filter = false;

		public CommandFilter(ObservableCollection<CommandFilterItem> clickedCommands)
		{
			_clickedCommands = clickedCommands;
			Register();
		}

		/// <summary>
		/// Register for command filtering. Customization filters must be registered before they are
		/// called.
		/// </summary>
		public void Register()
		{
			FrameworkApplication.RegisterCustomizationFilter(this);
		}

		/// <summary>
		/// Unregister for command filtering
		/// </summary>
		public void UnRegister()
		{
			FrameworkApplication.UnregisterCustomizationFilter(this);
		}

		/// <summary>
		/// Gets or sets the 'Pop Message Box' property. True will pop a message box if the
		/// clicked command is filtered
		/// </summary>
		public bool PopMessageBox
		{
			get => _popMessageBox;
			set
			{
				_popMessageBox = value;
			}
		}

		/// <summary>
		/// Gets the flag indicating whether the filter is registered
		/// </summary>
		public bool IsRegistered => _isRegistered;

		/// <summary>
		/// Gets whether we are filtering
		/// </summary>
		public bool IsFiltering => _filter;

		/// <summary>
		/// Start filtering
		/// </summary>
		public void StartFiltering()
		{
			_filter = true;
			FrameworkApplication.AddNotification(new Notification()
			{
				Message = "Command Filtering is 'On'",
				Title = "Command Filter",
				ImageUrl = Module1.PackUriForResource("MarsCat32.png").AbsoluteUri
			});
		}

		/// <summary>
		/// Stop filtering
		/// </summary>
		public void StopFiltering()
		{
			_filter = false;
			FrameworkApplication.AddNotification(new Notification()
			{
				Message = "Command Filtering is 'Off'",
				Title = "Command Filter",
				ImageUrl = Module1.PackUriForResource("MarsCat32.png").AbsoluteUri
			});
		}

		/// <summary>
		/// Implement your custom filtering logic here. OnCommandExecute is called every time
		/// a command is clicked on the UI.
		/// </summary>
		/// <param name="ID">DAML ID of the command that is executing</param>
		/// <returns>true to filter the command; false to execute the command</returns>
		protected override bool OnExecuteCommand(string ID)
		{
			if (!_filter)
				return true;

			//have we recorded this one?
			CommandFilterItem cmdFilterItem = _clickedCommands.FirstOrDefault(cfi => cfi.Id == ID);
			if (cmdFilterItem == null)
			{
				//no, we have not recorded this
				cmdFilterItem = new CommandFilterItem();
				IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper(ID, false);
				if (wrapper.LargeImage != null)
				{
					ImageSource img = wrapper.LargeImage as ImageSource;
					cmdFilterItem.Thumbnail = img as BitmapImage;
				}
				cmdFilterItem.Caption = wrapper.Caption;
				cmdFilterItem.Tooltip = wrapper.Tooltip ?? "";
				cmdFilterItem.Id = ID;
				cmdFilterItem.ClickCount = 1;
				_clickedCommands.Add(cmdFilterItem);
			}
			else
			{
				cmdFilterItem.ClickCount += 1;
			}

			if (_popMessageBox)
			{
				MessageBox.Show(string.Format("{0} is filtered. Cannot be executed", cmdFilterItem.Caption),
"Command Filter");
			}
			return false; // returning false blocks command
		}

		/// <summary>
		/// Implements IDisposable
		/// </summary>
		public void Dispose()
		{
			_clickedCommands = null;
			UnRegister();
		}
	}
}
