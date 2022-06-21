/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;


namespace ConstructToolWithOptions
{
	internal class ConstructFacilitiesToolToolOptionsViewModel : ToolOptionsEmbeddableControl
	{
		internal const bool DefaultSaveLastSubtypeChoiceToDefaults = true;
		internal const bool DefaultUseSubtypeChoiceOverride = true;

		internal const string SaveLastSubtypeChoiceToDefaultsName = "SaveLastSubtypeChoiceToDefaults";
		internal const string UseSubtypeChoiceOverrideName = "UseSubtypeChoiceOverride";

		public ConstructFacilitiesToolToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }


		#region Properties

		private bool _saveLastSubtypeChoiceToDefaults;
		public bool SaveLastSubtypeChoiceToDefaults
		{
			get => _saveLastSubtypeChoiceToDefaults;
			set
			{
				IsDirty = true;
				SetToolOption(SaveLastSubtypeChoiceToDefaultsName, value);
				SetProperty(ref _saveLastSubtypeChoiceToDefaults, value);
			}
		}

		private bool _useSubtypeChoiceOverride;
		public bool UseSubtypeChoiceOverride
		{
			get => _useSubtypeChoiceOverride;
			set
			{
				IsDirty = true;
				SetToolOption(UseSubtypeChoiceOverrideName, value);
				SetProperty(ref _useSubtypeChoiceOverride, value);
			}
		}

		public int SelectedSubtypeChoiceIndex
		{
			get => Module1.Current.SelectedSubtypeChoiceIndex;
			set => Module1.Current.SelectedSubtypeChoiceIndex = value;
		}

		public List<SubtypeChoice> SubtypeChoices => Module1.Current.SubtypeChoices;

		#endregion

		public override ImageSource SelectorIcon => System.Windows.Application.Current.Resources["BexDog32"] as ImageSource;

		public override bool IsAutoOpen(string toolID) => true;

		protected override Task LoadFromToolOptions()
		{
			SaveLastSubtypeChoiceToDefaults = GetToolOption<bool>(SaveLastSubtypeChoiceToDefaultsName, DefaultSaveLastSubtypeChoiceToDefaults);
			UseSubtypeChoiceOverride = GetToolOption<bool>(UseSubtypeChoiceOverrideName, DefaultUseSubtypeChoiceOverride);
			return Task.CompletedTask;
		}
	}
}
