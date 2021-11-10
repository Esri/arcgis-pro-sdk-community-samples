using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
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

namespace AttributeCustomDockpane
{
	internal class ShowAttributeViewModel : DockPane
	{
		private const string _dockPaneID = "AttributeCustomDockpane_ShowAttribute";
		private EmbeddableControl _inspectorViewModel = null;
        private System.Windows.Controls.UserControl _inspectorView = null;
        private Inspector _attributeInspector;
		private Geometry _geometry;

		protected ShowAttributeViewModel() 
		{
			// create a new instance for the inspector
			_attributeInspector = new Inspector();

			// Tell the singleton module class
			Module1.AttributeInspector = AttributeInspector;
			Module1.AttributeViewModel = this;

			// create an embeddable control from the inspector class to display on the pane
			var icontrol = _attributeInspector.CreateEmbeddableControl();

			// get viewmodel and view for the inspector control
			InspectorViewModel = icontrol.Item1;
			InspectorView = icontrol.Item2;
		}

		#region Properties

		/// <summary>
		/// Property containing an instance for the inspector.
		/// </summary>
		public Inspector AttributeInspector
		{
			get
			{
				return _attributeInspector;
			}
		}

		/// <summary>
		/// Property for the inspector UI.
		/// </summary>
		public System.Windows.Controls.UserControl InspectorView
		{
			get { return _inspectorView; }
			set { SetProperty(ref _inspectorView, value, () => InspectorView); }
		}

		/// <summary>
		/// Access to the view model of the inspector
		/// </summary>
		public EmbeddableControl InspectorViewModel
		{
			get { return _inspectorViewModel; }
			set
			{
				if (value != null)
				{
					_inspectorViewModel = value;
					_inspectorViewModel.OpenAsync();
				}
				else if (_inspectorViewModel != null)
				{
					_inspectorViewModel.CloseAsync();
					_inspectorViewModel = value;
				}
				NotifyPropertyChanged(() => InspectorViewModel);
			}
		}

		public Geometry Geometry
		{
			get => _geometry;
			set => SetProperty(ref _geometry, value);
		}

		#endregion

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
		private string _heading = "Custom Attribute Dockpane";
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
	internal class ShowAttribute_ShowButton : Button
	{
		protected override void OnClick()
		{
			ShowAttributeViewModel.Show();
		}
	}
}
