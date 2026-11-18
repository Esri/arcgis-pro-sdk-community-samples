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

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.OCR;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using OCRManagerSample.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OCRManagerSample
{
  internal class DocReaderViewModel : DockPane
  {
    private const string _dockPaneID = "OCRManagerSample_DocReader";

    protected DocReaderViewModel() { }

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

        // process new
        DocumentName = dlg.FileName;
        ExtractFromDocument();
      }
    }

    private void InvokeClearCmd()
    {
      DocumentName = "";
      Text = "";
    }

    private async Task ExtractFromDocument()
    {
      var canceled = false;
      var ocrText = "";

      using (CancelableProgressorSource cps = new CancelableProgressorSource("Reading document", "Canceled"))
      {
        await QueuedTask.Run(async () =>
        {
          try
          {
            // extract text from all pages
            ocrText = await ArcGIS.Core.OCR.OCRManager.Instance.ExtractTextFromDocumentAsync(DocumentName, cps.Progressor.CancellationToken);

            // or extract from a specific page number

            //int pageIdx = 0;
            //ocrText = await ArcGIS.Core.OCR.OCRManager.Instance.ExtractTextFromDocumentAsync(fileName, pageIdx, cps.Progressor.CancellationToken);

            // or use the options to extract text from all pages

            //var ocrOptions = new ArcGIS.Core.OCR.OCROptions(fileName);
            //ocrOptions.OutputType = ArcGIS.Core.OCR.OCROutputType.Text;

            //var ocrOutput = await ArcGIS.Core.OCR.OCRManager.Instance.ExtractFromDocumentAsync(ocrOptions, cps.Progressor.CancellationToken);
            //var ocrTextOutput = ocrOutput as OCRTextOutput;
            //ocrText = ocrTextOutput?.Text;
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

      if (!canceled)
        Text = ocrText;
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DocReader_ShowButton : Button
  {
    protected override void OnClick()
    {
      DocReaderViewModel.Show();
    }
  }
}
