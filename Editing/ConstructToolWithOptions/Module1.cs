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
using System.Windows.Input;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;

namespace ConstructToolWithOptions
{

	internal class SubtypeChoice
	{
		public int SubtypefieldValue { get; set; }
		public string FeatureCodeValue { get; set; }

		public override string ToString()
		{
			return $"{this.SubtypefieldValue} - {this.FeatureCodeValue}";
		}
	}

	/// <summary>
	/// This sample illustrates how to build a point construction tool with the option to select the feature subtype when the point is created.  
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data
	/// 1. The project used for this sample is 'C:\Data\ConstructToolSample\ConstructToolSample.ppkx'
	/// 1. In Visual Studio click the Build menu.Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select the ConstructToolSample.ppkx project
	/// 1. Select the 'Edit' tab on the ArcGIS Pro ribbon and click the 'Create' button  
	/// 1. On the 'Create Features' pane select any of the point features (FacilitySitePoint... feature classes) to see the 'Construct Facilities with Subtypes Tool' tool
	/// ![UI](Screenshots/Screen1.png)      
	/// 1. Select the tool and see the Options page displaying the subtype selection
	/// ![UI](Screenshots/Screen2.png)      
	/// 1. Select a subtype to generate a new point feature using that subtype.
	/// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current
		{
			get
			{
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ConstructToolWithOptions_Module"));
			}
		}

		#region For Tool Options

		private List<SubtypeChoice> _subtypeChoices = new List<SubtypeChoice>();
		private int _currentSubtypeChoice = 0;
		private bool _saveLastSubtypeChoiceToDefaults = true;
		private bool _useSubtypeChoiceOverride = true;

		public bool SaveLastSubtypeChoiceToDefaults
		{
			get
			{
				return _saveLastSubtypeChoiceToDefaults;
			}
			set
			{
				_saveLastSubtypeChoiceToDefaults = value;
			}
		}

		public bool UseSubtypeChoiceOverride
		{
			get
			{
				return _useSubtypeChoiceOverride;
			}
			set
			{
				_useSubtypeChoiceOverride = value;
			}
		}

		public int SelectedSubtypeChoiceIndex
		{
			get
			{
				return _currentSubtypeChoice;
			}
			set
			{
				_currentSubtypeChoice = value;
			}
		}

		public SubtypeChoice SelectedSubtypeChoice => _subtypeChoices[_currentSubtypeChoice];

		public List<SubtypeChoice> SubtypeChoices => _subtypeChoices;

		#endregion

		#region Overrides

		protected override bool Initialize()
		{
			//possible choices for new buildings
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 710,
				FeatureCodeValue = "Industrial Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 720,
				FeatureCodeValue = "Commercial or Retail Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 730,
				FeatureCodeValue = "Education Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 740,
				FeatureCodeValue = "Emergency Response or Law Enforcement Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 790,
				FeatureCodeValue = "Building General"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 800,
				FeatureCodeValue = "Health or Medical Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 810,
				FeatureCodeValue = "Transportation Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 830,
				FeatureCodeValue = "Government or Military Facility"
			});
			_subtypeChoices.Add(new SubtypeChoice()
			{
				SubtypefieldValue = 880,
				FeatureCodeValue = "Information or Communication Facility"
			});

			return true;
		}

		/// <summary>
		/// Called by Framework when ArcGIS Pro is closing
		/// </summary>
		/// <returns>False to prevent Pro from closing, otherwise True</returns>
		protected override bool CanUnload()
		{
			//TODO - add your business logic
			//return false to ~cancel~ Application close
			return true;
		}

		#endregion Overrides

	}
}
