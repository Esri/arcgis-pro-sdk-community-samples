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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace OCRManagerSample
{
  internal class ExtractTSV : Button
  {
    protected override async void OnClick()
    {
      var mv = MapView.Active;
      if (mv == null)
        return;

      string fileName = "";
      var dlg = new Microsoft.Win32.OpenFileDialog()
      {
        Title = "OpenDeed",
        Filter = OCRManager.Instance.SupportedFileFilter, 
        Multiselect = false
      };

      if (dlg.ShowDialog() == true)
        fileName = dlg.FileName;


      if (string.IsNullOrEmpty(fileName))
        return;

      var text = await QueuedTask.Run<string>(async () =>
      {
        string ocrText = "";
        try
        {
          //  use the options to extract from all pages

          var ocrOptions = new ArcGIS.Core.OCR.OCROptions(fileName);
          ocrOptions.OutputType = ArcGIS.Core.OCR.OCROutputType.TSV;


          var ocrOutput = await ArcGIS.Core.OCR.OCRManager.Instance.ExtractFromDocumentAsync(ocrOptions);
          var ocrTSVutput = ocrOutput as OCRTSVOutput;

          if (ocrTSVutput != null)
          {
            var flowDocument = CreateFlowDocument(ocrTSVutput);
            ocrText = GetText(flowDocument); 
          }
        }
        catch (Exception ex)
        {
        }
        return ocrText;
      });

      // do something with text
    }

    internal FlowDocument CreateFlowDocument(OCRTSVOutput tsvOutput)
    {
      var flowDoc = new FlowDocument();

      if (tsvOutput == null)
        return flowDoc;

      foreach (var page in tsvOutput.Pages)
      {
        List<Paragraph> pageParagraphs = new();

        foreach (var block in page.Blocks)
          foreach (var paragraph in block.Paragraphs)
            foreach (var line in paragraph.Lines)
            {
              string lineWords = string.Join(" ", line.Words.Select(w => w.Content));
              pageParagraphs.Add(new Paragraph(new Span(new Run(lineWords))));
            }

        flowDoc.Blocks.AddRange(pageParagraphs);
        flowDoc.Blocks.Add(new Paragraph(new Span(new Run(Environment.NewLine))));
      }

      return flowDoc;
    }

    internal string GetText(FlowDocument flowDocument)
    {
      var startPointer = flowDocument?.ContentStart;
      var endPointer = flowDocument?.ContentEnd;

      if (startPointer == null || endPointer == null)
        return "";

      return new TextRange(startPointer, endPointer).Text;
    }

  }
}
