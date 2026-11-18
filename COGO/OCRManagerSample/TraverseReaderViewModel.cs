/*

   Copyright 2026 Esri

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

using ArcGIS.Core.Geometry;
using ArcGIS.Core.OCR;
using ArcGIS.Desktop.Editing.COGO;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using OCRManagerSample.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OCRManagerSample
{
  internal class TraverseReaderViewModel : DockPane
  {
    private const string _dockPaneID = "OCRManagerSample_TraverseReader";

    protected TraverseReaderViewModel() { }

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

    public ImageSource MoveFolderImage => ProImageProvider.Instance["MoveFromFolder16"];
    public RelayCommand BrowseDocumentCmd { get { return new RelayCommand(() => this.InvokeBrowseDocumentCmd(), true); } }
    public RelayCommand ClearCmd { get { return new RelayCommand(() => this.InvokeClearCmd(), true); } }
    public RelayCommand AddToMapCmd { get { return new RelayCommand(() => this.InvokeAddToMapCmd(), CanAddToMap()); } }

    /// <summary>
    /// Document to be scanned.
    /// </summary>
    private string _documentName = string.Empty;
    public string DocumentName
    {
      get => _documentName;
      set => SetProperty(ref _documentName, value);
    }

    /// <summary>
    /// Extracted text
    /// </summary>
    private string _text = string.Empty;
    public string Text
    {
      get => _text;
      set => SetProperty(ref _text, value);
    }

    private void InvokeBrowseDocumentCmd()
    {
      var dlg = new Microsoft.Win32.OpenFileDialog()
      {
        Title = "OpenDeed",
        Filter = OCRManager.Instance.SupportedFileFilter,
        Multiselect = false
      };

      if (dlg.ShowDialog() == true)
      {
        // clear existing
        InvokeClearCmd();

        // process the new
        DocumentName = dlg.FileName;
        ExtractFromDocument();
      }
    }

    private void InvokeClearCmd()
    {
      // clean up all internal variables
      DocumentName = "";
      Text = "";
      _traverses = null;
      Traverse = null;

      NotifyPropertyChanged(nameof(TraverseDataVisibility));

      // remove any overlay graphics
      Module1.Current.RemoveOverlays();
    }

    private IReadOnlyList<Traverse> _traverses = null;

    private async Task ExtractFromDocument()
    {
      var canceled = false;

      _traverses = null;

      var sr = MapView.Active?.Map?.SpatialReference;
      if (sr == null)
        return;

      using (CancelableProgressorSource cps = new CancelableProgressorSource("Reading document", "Canceled"))
      {
        await QueuedTask.Run(async () =>
        {
          try
          {
            // extract traverse from all pages
            _traverses = await ArcGIS.Core.OCR.OCRManager.Instance.ExtractTraverseFromDeedAsync(DocumentName, sr, cps.Progressor.CancellationToken);

            // or extract from a specific page number

            //int pageIdx = 0;
            //traverses = await ArcGIS.Core.OCR.OCRManager.Instance.ExtractTraverseFromDeedAsync(fileName, pageIdx, cps.Progressor.CancellationToken);
          }
          catch (OperationCanceledException cex)
          {
            canceled = true;
          }
          catch (Exception ex)
          {
          }
        }, cps.Progressor);
      }

      // if no cancel occurred, process the data
      if (!canceled)
        ProcessTraverses();
    }

    private async Task ProcessTraverses()
    {
      if (TraverseCount > 0)
      {
        InvokeMoveToBeginningCmd();
      }
      else
      {
        _currentTraverseIndex = -1;
        Traverse = null;
      }

      NotifyPropertyChanged(nameof(TraverseDataVisibility));
      NotifyPropertyChanged(nameof(RecordCountText));
    }


    #region Traverse 

    public Visibility TraverseDataVisibility => TraverseCount > 0 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Count of traverses extracted
    /// </summary>
    private int TraverseCount => _traverses?.Count() ?? 0;
    public string RecordCountText => (TraverseCount > 0) ? (_currentTraverseIndex + 1).ToString() + " of " + TraverseCount.ToString() + " parcels" : "";

    /// <summary>
    /// Index of current traverse displayed 
    /// </summary>
    private int _currentTraverseIndex = -1;

    private Traverse _traverse = null;
    /// <summary>
    /// Current traverse displayed 
    /// </summary>
    public Traverse Traverse
    {
      get => _traverse;
      set => SetProperty(ref _traverse, value);
    }

    #region Traverse Navigation

    public ImageSource MoveToBeginningImage => ProImageProvider.Instance["SliderStepBackward16"];
    public ImageSource MoveToPreviousImage => ProImageProvider.Instance["SliderStepBack16"];
    public ImageSource MoveToNextImage => ProImageProvider.Instance["SliderStepNext16"];
    public ImageSource MoveToEndImage => ProImageProvider.Instance["SliderStepForward16"];

    public RelayCommand MoveToBeginningCmd => new RelayCommand(() => this.InvokeMoveToBeginningCmd(), CanMove());
    public RelayCommand MoveToPreviousCmd => new RelayCommand(() => this.InvokeMoveToPreviousCmd(), CanMove());
    public RelayCommand MoveToNextCmd => new RelayCommand(() => this.InvokeMoveToNextCmd(), CanMove());
    public RelayCommand MoveToEndCmd => new RelayCommand(() => this.InvokeMoveToLastCmd(), CanMove());

    public bool CanMove() => TraverseCount > 0;

    private void InvokeMoveToBeginningCmd()
    {
      _currentTraverseIndex = 0;
      DisplayCurrentTraverse();
    }

    private void InvokeMoveToPreviousCmd()
    {
      if (_currentTraverseIndex > 0)
      {
        _currentTraverseIndex--;
        DisplayCurrentTraverse();
      }
    }

    private void InvokeMoveToNextCmd()
    {
      if (_currentTraverseIndex < TraverseCount - 1)
      {
        _currentTraverseIndex++;
        DisplayCurrentTraverse();
      }
    }

    private void InvokeMoveToLastCmd()
    {
      _currentTraverseIndex = TraverseCount - 1;
      DisplayCurrentTraverse();
    }

    private async Task DisplayCurrentTraverse()
    {
      Traverse = _traverses[_currentTraverseIndex];

      NotifyPropertyChanged(nameof(RecordCountText));
      NotifyPropertyChanged(nameof(CanAddToMap));

      string text = "";
      text += "Traverse \r\n";

      text += await ExportTraverse(Traverse) + "\r\n\r\n";

      Text = text;
    }

    #endregion

    #endregion

    // export a traverse to a temporary file and obtain the text
    private async Task<string> ExportTraverse(Traverse traverse)
    {
      var text = "";

      var tmpFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      tmpFilePath += System.IO.Path.DirectorySeparatorChar.ToString() + "tmp.txt";
      try
      {
        var options = new TraverseExportOptions(tmpFilePath);
        await traverse.ExportAsync(options);

        text = File.ReadAllText(tmpFilePath);
      }
      finally
      {
        if (File.Exists(tmpFilePath))
          File.Delete(tmpFilePath);
      }

      return text;
    }

    internal bool CanAddToMap()
    {
      return _traverses != null && Traverse != null;
    }

    internal async Task InvokeAddToMapCmd()
    {
      if (_traverses is null)
          return;

      await DisplayTraverse(MapView.Active);
    }

    internal Task DisplayTraverse(MapView mv)
    {
      if (Traverse is null)
        return Task.CompletedTask;

      Envelope extent = null;

      return QueuedTask.Run(() =>
      {
        // set up colors
        var red = ColorFactory.Instance.CreateRGBColor(255, 0, 0);
        var blue = ColorFactory.Instance.CreateRGBColor(0, 0, 255);

        // set up symbols
        var lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(red);
        var polygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(blue, SimpleFillStyle.Horizontal);

        var lineSymbolRef = lineSymbol.MakeSymbolReference();
        var polySymbolRef = polygonSymbol.MakeSymbolReference();

        var centerPt = mv.Extent.Center;

        // check for start point
        var startPoint = Traverse.StartPoint;
        if ((startPoint.X == 0) && (startPoint.Y == 0))
        Traverse.StartPoint = centerPt;

        // get the polylines for the traverse courses
        var polylines = Traverse.GetCourseGeometries();

        foreach (var polyline in polylines)
        {
          if (extent == null)
            extent = polyline.Extent;
          else
            extent = extent.Union(polyline.Extent);
        }

        // add to overlay
        foreach (var polyline in polylines)
        {
          var disp = mv.AddOverlay(polyline, lineSymbolRef);
          Module1.Current.Overlays.Add(disp);
        }

        // zoom
        if ((extent != null) || (!extent.IsEmpty))
          mv.ZoomTo(extent);
      });
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class TraverseReader_ShowButton : Button
  {
    protected override void OnClick()
    {
      TraverseReaderViewModel.Show();
    }
  }
}
