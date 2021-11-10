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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ColorPickerControl
{
	internal class PolygonColorPickerViewModel : DockPane
	{
		private const string _dockPaneID = "ColorPickerControl_PolygonColorPicker";
		private static PolygonColorPickerViewModel _this = null;
		private static FeatureLayer PolygonLayer = null;

		protected PolygonColorPickerViewModel()
		{
			_this = this;
			_ = ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChangedEvent);
		}

		/// <summary>
		/// Waiting for the active map view to change in order to setup event listening
		/// </summary>
		/// <param name="args"></param>
		private void OnActiveMapViewChangedEvent(ActiveMapViewChangedEventArgs args)
		{
			if (args.IncomingView != null)
			{
				SetPolygonLayer();
			}
		}

		internal async void SetPolygonLayer()
		{
			PolygonLayer = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(e => e.ShapeType == esriGeometryType.esriGeometryPolygon).FirstOrDefault(); if (PolygonLayer != null)
			{
				PolygonLayerName = PolygonLayer.Name;
				SelectedColor = await GetPolygonRendererColor(PolygonLayer);
			}
		}

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			if (_this != null)
			{
				_this.SetPolygonLayer();
			}
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
			{
				return;
			}

			pane.Activate();
		}

		/// <summary>
		/// Color selection
		/// </summary>
		private CIMColor _SelectedColor = CIMColor.CreateRGBColor(255, 0, 0);
		public CIMColor SelectedColor
		{
			get => _SelectedColor;
			set => SetProperty(ref _SelectedColor, value, () => SelectedColor);
		}

		/// <summary>
		/// Name of the Polygon layer for which to change the fill color.
		/// </summary>
		private string _polygonLayerName = "No Polygon layer found";
		public string PolygonLayerName
		{
			get => _polygonLayerName;
			set => SetProperty(ref _polygonLayerName, value, () => PolygonLayerName);
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Change Polygon Fill Color";
		public string Heading
		{
			get => _heading;
			set => SetProperty(ref _heading, value, () => Heading);
		}

		/// <summary>
		/// Change the polygon fill color
		/// </summary>
		public ICommand CmdChangeColor => new RelayCommand(() =>
		{
			if (PolygonLayer != null)
			{
				_ = SetPolygonRendererColor(PolygonLayer, SelectedColor);
			}
		});

		internal static Task SetPolygonRendererColor(FeatureLayer featureLayer, CIMColor fillColor)
		{
			return QueuedTask.Run(() =>
			{
				//Creating a polygon with a red fill and blue outline.
				CIMStroke outline = SymbolFactory.Instance.ConstructStroke(
					 ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
				CIMPolygonSymbol fillWithOutline = SymbolFactory.Instance.ConstructPolygonSymbol(
					 fillColor, SimpleFillStyle.Solid, outline);
				//Get the layer's current renderer
				CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

				//Update the symbol of the current simple renderer
				renderer.Symbol = fillWithOutline.MakeSymbolReference();

				//Update the feature layer renderer
				featureLayer.SetRenderer(renderer);
			});
		}

		internal static Task<CIMColor> GetPolygonRendererColor(FeatureLayer featureLayer)
		{
			return QueuedTask.Run<CIMColor>(() =>
			{
				if (!(featureLayer.GetRenderer() is CIMSimpleRenderer renderer)) return CIMColor.NoColor();

				return !(renderer.Symbol.Symbol is CIMPolygonSymbol polySymbol) ? CIMColor.NoColor() : polySymbol.GetColor();
			});
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class PolygonColorPicker_ShowButton : Button
	{
		protected override void OnClick()
		{
			PolygonColorPickerViewModel.Show();
		}
	}
}
