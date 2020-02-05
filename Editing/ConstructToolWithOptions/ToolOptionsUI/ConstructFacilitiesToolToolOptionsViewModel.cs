/*

   Copyright 2020 Esri

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
using System.Windows.Media;
using System.Xml.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace ConstructToolWithOptions.ToolOptionsUI
{
	internal class ConstructFacilitiesToolToolOptionsViewModel : EmbeddableControl, IEditingCreateToolControl
	{
		public ConstructFacilitiesToolToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }

		private ToolOptions _toolOptions;
		private bool _dirty = false;

		#region Properties

		public bool SaveLastSubtypeChoiceToDefaults
		{
			get => Module1.Current.SaveLastSubtypeChoiceToDefaults;
			set
			{
				Module1.Current.SaveLastSubtypeChoiceToDefaults = value;
				//update tool options
				if (_toolOptions != null)
				{
					_dirty = true;
					_toolOptions["SaveLastSubtypeChoiceToDefaults"] = value;
				}

				NotifyPropertyChanged();
			}
		}

		public bool UseSubtypeChoiceOverride
		{
			get => Module1.Current.UseSubtypeChoiceOverride;
			set
			{
				Module1.Current.UseSubtypeChoiceOverride = value;
				//update tool options
				if (_toolOptions != null)
				{
					_dirty = true;
					_toolOptions["UseSubtypeChoiceOverride"] = value;
				}

				NotifyPropertyChanged();
			}
		}

		public int SelectedSubtypeChoiceIndex
		{
			get => Module1.Current.SelectedSubtypeChoiceIndex;
			set => Module1.Current.SelectedSubtypeChoiceIndex = value;
		}

		public List<SubtypeChoice> SubtypeChoices => Module1.Current.SubtypeChoices;

		#endregion

		#region IEditingCreateToolControl

		//These are for the Active Template Pane
		public ImageSource ActiveTemplateSelectorIcon =>
			System.Windows.Application.Current.Resources["BexDog32"] as ImageSource;

		public bool AutoOpenActiveTemplatePane(string toolID) => true;

		//We are being activated on the Active Template Pane
		//We are the active tool.
		public bool InitializeForActiveTemplate(ToolOptions options)
		{
			SetCheckboxes(options);
			return true;
		}

		//These are for the Template Properties Dialog
		//We are being activated on the Template Properties Dialog
		public bool InitializeForTemplateProperties(ToolOptions options)
		{
			SetCheckboxes(options);
			_toolOptions = options;
			return true;
		}

		public bool IsValid => true;
		public bool IsDirty => _dirty;//True indicates ToolOptions have been changed

		#endregion

		private void SetCheckboxes(ToolOptions options)
		{
			if (options.ContainsKey("SaveLastSubtypeChoiceToDefaults"))
				SaveLastSubtypeChoiceToDefaults = (bool)options["SaveLastSubtypeChoiceToDefaults"];

			if (options.ContainsKey("UseSubtypeChoiceOverride"))
				UseSubtypeChoiceOverride = (bool)options["UseSubtypeChoiceOverride"];
		}
	}
}