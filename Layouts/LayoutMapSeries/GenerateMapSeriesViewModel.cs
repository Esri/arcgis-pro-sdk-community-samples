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
using System.Collections.ObjectModel;
using System.Windows.Input;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using LayoutMapSeries.LayoutSettings;
using System.Windows.Media;
using ArcGIS.Core.Data;
using System.Windows.Data;
using LayoutMapSeries.Helpers;
using System.Windows;
using System.Windows.Threading;
using ArcGIS.Desktop.Catalog;

namespace LayoutMapSeries
{
  internal class GenerateMapSeriesViewModel : DockPane
  {
    private const string _dockPaneID = "LayoutMapSeries_GenerateMapSeries";
    private SetPages _setPages = null;
    private Layout _layout = null;
    private MapSeriesDefinition _mapSeriesDefinition = new MapSeriesDefinition();
    private static object _lock = new object();
    private MapSeriesHelper _msHelper = new Helpers.MapSeriesHelper();
    private ObservableCollection<SetPage> _mapPageLayouts = new ObservableCollection<SetPage>();
    private ObservableCollection<MapSeriesItem> _mapSeriesItems = new ObservableCollection<MapSeriesItem>();

    protected GenerateMapSeriesViewModel()
    {
      _setPages = new SetPages();
      BindingOperations.EnableCollectionSynchronization(_mapSeriesItems, _lock);
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

    
    public IReadOnlyCollection<SetPage> PageLayouts
    {
      get
      {
        if (_mapPageLayouts.Count == 0)
        {
          foreach (var setPg in _setPages.SetPageList)
          {
            _mapPageLayouts.Add(setPg);
          }
        }
        return _mapPageLayouts;
      }
    }

    private SetPage _myPageLyt = null;

    public SetPage SelectedPageLayout
    {
      get { return _myPageLyt; }
      set { SetProperty(ref _myPageLyt, value, () => SelectedPageLayout); }
    }

    public IReadOnlyCollection<MapSeriesItem> MapSeriesItems
    {
      get
      {
        return _mapSeriesItems;
      }
    }

    private MapSeriesItem _myMapSeriesItem = null;

    public MapSeriesItem SelectedMapSeriesItem
    {
      get { return _myMapSeriesItem; }
      set
      {
        SetProperty(ref _myMapSeriesItem, value, () => SelectedMapSeriesItem);
        try
        {
          if (_myMapSeriesItem != null)
          {
            _msHelper.SetCurrentPageAsync(_layout, _myMapSeriesItem.Oid, _myMapSeriesItem.Id);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show($@"Error in SelectedMapSeriesItem: {ex}");
        }
      }
    }

    public ICommand ExportMapSeriesItem
    {
      get
      {
        return new RelayCommand(async () =>
        {
          try
          {
            // Reference a layoutitem in a project by name
            LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals(SelectedMapSeriesItem?.LayoutName));
            if (layoutItem != null)
            {
              // Create the log file and write the current Folder-Connection's to it
              SaveItemDialog saveDialog = new SaveItemDialog();
              saveDialog.Title = "Export the current selected map series item";
              saveDialog.OverwritePrompt = true;
              saveDialog.DefaultExt = "pdf";

              // If the save dialog was not dismissed, create the file
              if (saveDialog.ShowDialog() == true)
              {
                await QueuedTask.Run(() =>
                {
                  Layout layout = layoutItem.GetLayout();
                  if (layout == null)
                    return;
                  // Create PDF format with appropriate settings
                  PDFFormat PDF = new PDFFormat()
                  {
                    Resolution = 300,
                    OutputFileName = saveDialog.FilePath
                  };
                  if (PDF.ValidateOutputFilePath())
                  {
                    layout.Export(PDF);
                  }
                });

              }
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Error in create layout: {ex}");
          }
        }, () => SelectedMapSeriesItem != null);
      }
    }

    public ICommand GenerateMapSeries
    {
      get
      {
        return new RelayCommand(async () =>
        {
          try
          {
            _layout = await QueuedTask.Run<Layout>(() =>
            {
              //Set up a page
              CIMPage newPage = new CIMPage
              {
                //required
                Width = SelectedPageLayout.Width,
                Height = SelectedPageLayout.Height,
                Units = SelectedPageLayout.LinearUnit
              };
              Layout layout = LayoutFactory.Instance.CreateLayout(newPage);
              layout.SetName(SelectedPageLayout.LayoutName);

              //Add Map Frame
              Coordinate2D llMap = new Coordinate2D(SelectedPageLayout.MarginLayout, SelectedPageLayout.MarginLayout);
              Coordinate2D urMAP = new Coordinate2D(SelectedPageLayout.WidthMap, SelectedPageLayout.Height - SelectedPageLayout.MarginLayout);
              Envelope envMap = EnvelopeBuilder.CreateEnvelope(llMap, urMAP);

              //Reference map, create Map Frame and add to layout
              MapProjectItem mapPrjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("Map"));
              Map theMap = mapPrjItem.GetMap();
              MapFrame mfElm = LayoutElementFactory.Instance.CreateMapFrame(layout, envMap, theMap);
              mfElm.SetName(SelectedPageLayout.MapFrameName);

              //Scale bar
              Coordinate2D llScalebar = new Coordinate2D(2 * SelectedPageLayout.MarginLayout, 2 * SelectedPageLayout.MarginLayout);
              LayoutElementFactory.Instance.CreateScaleBar(layout, llScalebar, mfElm);

              //NorthArrow
              Coordinate2D llNorthArrow = new Coordinate2D(SelectedPageLayout.WidthMap - 2 * SelectedPageLayout.MarginLayout, 2 * SelectedPageLayout.MarginLayout);
              var northArrow = LayoutElementFactory.Instance.CreateNorthArrow(layout, llNorthArrow, mfElm);
              northArrow.SetAnchor(Anchor.CenterPoint);
              northArrow.SetLockedAspectRatio(true);
              northArrow.SetWidth(2 * SelectedPageLayout.MarginLayout);

              // Title: dynamic text: <dyn type="page" property="name"/>
              var title = @"<dyn type = ""page"" property = ""name"" />";
              Coordinate2D llTitle = new Coordinate2D(SelectedPageLayout.XOffsetMapMarginalia, SelectedPageLayout.Height - 2 * SelectedPageLayout.MarginLayout);
              var titleGraphics = LayoutElementFactory.Instance.CreatePointTextGraphicElement(layout, llTitle, null) as TextElement;
              titleGraphics.SetTextProperties(new TextProperties(title, "Arial", 16, "Bold"));

              // Table 1
              AddTableToLayout(layout, theMap, mfElm, "Inspection Locations", SelectedPageLayout, 3 * SelectedPageLayout.HeightPartsMarginalia);

              // Table 2
              AddTableToLayout(layout, theMap, mfElm, "Service Locations", SelectedPageLayout, 2 * SelectedPageLayout.HeightPartsMarginalia);

              // legend
              Coordinate2D llLegend = new Coordinate2D(SelectedPageLayout.XOffsetMapMarginalia, SelectedPageLayout.MarginLayout);
              Coordinate2D urLegend = new Coordinate2D(SelectedPageLayout.XOffsetMapMarginalia + SelectedPageLayout.XWidthMapMarginalia, SelectedPageLayout.HeightPartsMarginalia);
              System.Diagnostics.Debug.WriteLine(mfElm);
              LayoutElementFactory.Instance.CreateLegend(layout, EnvelopeBuilder.CreateEnvelope(llLegend, urLegend), mfElm);
							
							// Defince the CIM MapSeries
							var CimSpatialMapSeries = new CIMSpatialMapSeries()
							{
								Enabled = true,
								MapFrameName = SelectedPageLayout.MapFrameName,
								StartingPageNumber = 1,
								CurrentPageID = 1,
								IndexLayerURI = "CIMPATH=map/railroadmaps.xml",
								NameField = "ServiceAreaName",
								SortField = "SeqId",
								RotationField = "Angle",
								SortAscending = true,
								ScaleRounding = 1000,
								ExtentOptions = ExtentFitType.BestFit,
								MarginType = ArcGIS.Core.CIM.UnitType.Percent,
								Margin = 2
							};
							CIMLayout layCIM = layout.GetDefinition();
							layCIM.MapSeries = CimSpatialMapSeries;
							layout.SetDefinition(layCIM);

							return layout;
            });

            //CREATE, OPEN LAYOUT VIEW (must be in the GUI thread)
            ILayoutPane layoutPane = await LayoutFrameworkExtender.CreateLayoutPaneAsync(ProApp.Panes,_layout);
            var cimLayout = await QueuedTask.Run<CIMLayout>(() =>
            {
              return _layout.GetDefinition();
            });
            // refresh the UI with the map series records
            _msHelper = null;
            {
              MapProjectItem mapPrjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("Map"));
              MapSeriesDefinition ms = await QueuedTask.Run<MapSeriesDefinition>(() =>
              {
                var theMap = mapPrjItem.GetMap();
                var fc = GetFeatureClassByName(theMap, "RailroadMaps");
                var newMs = new MapSeriesDefinition { FeatureClassName = fc.GetName() };

                newMs.LoadFromFeatureClass(layoutPane.LayoutView.Layout.Name, fc, "ID,ServiceAreaName");
                _msHelper = new Helpers.MapSeriesHelper();
                _msHelper.Initialize(_layout, GetFeatureLayerFromMap (theMap, "RailroadMaps"));
                return newMs;
              });
              lock (_lock)
              {
                _mapSeriesItems.Clear();
              }
              var setSelection = true;
              foreach (var msItem in ms.MapSeriesItems)
              {
                var newMsItem = new MapSeriesItem { Id = msItem.Id, Name = msItem.Name, Oid = msItem.Oid, LayoutName = msItem.LayoutName };
                lock (_lock) _mapSeriesItems.Add(newMsItem);
                if (setSelection)
                {
                  setSelection = false;
                  await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                      SelectedMapSeriesItem = newMsItem;
                    }));
                }
              }
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Error in create layout: {ex}");
          }
        }, () => SelectedPageLayout != null);
      }
    }

    private CIMPointSymbol GetPointSymbolFromLayer(Layer layer)
    {
      if (!(layer is FeatureLayer)) return null;
      var fLyr = layer as FeatureLayer;
      var renderer = fLyr.GetRenderer() as CIMSimpleRenderer;
      if (renderer == null || renderer.Symbol == null) return null;
      return renderer.Symbol.Symbol as CIMPointSymbol;
    }

    private FeatureClass GetFeatureClassByName(Map theMap, string fcName)
    {
      var layers = theMap.FindLayers(fcName, true);
      if (layers.Count > 0)
      {
        Layer lyr = layers[0];
        if (lyr is FeatureLayer) return (lyr as FeatureLayer).GetFeatureClass();
      }
      return null;
    }

    private FeatureLayer GetFeatureLayerFromMap(Map theMap, string featureLayerName)
    {
      var lyrs = theMap.FindLayers(featureLayerName, true);
      if (lyrs.Count > 0)
      {
        return lyrs[0] as FeatureLayer;
      }
      return null;
    }

    private void AddTableToLayout(Layout layout, Map theMap, MapFrame mfElm, string layerName, SetPage setPage, double yOffset)
    {
      var lyrs = theMap.FindLayers(layerName, true);
      if (lyrs.Count > 0)
      {
        Layer lyr = lyrs[0];
        var ptSymbol = GetPointSymbolFromLayer(lyr);
        if (ptSymbol != null)
        {
          Coordinate2D llSym = new Coordinate2D(setPage.XOffsetMapMarginalia, setPage.YOffsetSymbol + yOffset);
          var sym = LayoutElementFactory.Instance.CreatePointGraphicElement(layout, llSym, ptSymbol);

          Coordinate2D llText = new Coordinate2D(setPage.XOffsetMapMarginalia + sym.GetWidth(), setPage.YOffsetSymbol + yOffset - sym.GetHeight()/2);
          var text = LayoutElementFactory.Instance.CreatePointTextGraphicElement(layout, llText, lyr.Name);
          text.SetAnchor(Anchor.CenterPoint);
          text.SetHeight(text.GetHeight());
          if (text.GetHeight() > sym.GetHeight())
          {
            sym.SetLockedAspectRatio(true);
            sym.SetHeight(text.GetHeight());
          }
          else
          {
            text.SetLockedAspectRatio(true);
            text.SetHeight(sym.GetHeight());
          }
        }
        Coordinate2D llTab1 = new Coordinate2D(setPage.XOffsetMapMarginalia, yOffset - setPage.HeightPartsMarginalia);
        Coordinate2D urTab1 = new Coordinate2D(setPage.XOffsetMapMarginalia + setPage.XWidthMapMarginalia, yOffset);
        var table1 = LayoutElementFactory.Instance.CreateTableFrame(layout, EnvelopeBuilder.CreateEnvelope(llTab1, urTab1), mfElm, lyr, new string[] { "No", "Type", "Description" });
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class GenerateMapSeries_ShowButton : Button
  {
    protected override void OnClick()
    {
      GenerateMapSeriesViewModel.Show();
    }
  }
}
