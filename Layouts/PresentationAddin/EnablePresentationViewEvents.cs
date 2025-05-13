/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Presentations;
using ArcGIS.Desktop.Presentations.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationsAddin
{
  internal class EnablePresentationViewEvents : Button
  {
    protected override void OnClick()
    {
      ArcGIS.Desktop.Presentations.Events.PresentationViewEvent.Subscribe((args) =>
      {
        //get the affected view and presentation
        var view = args.PresentationView;
        var presentation = args.PresentationView?.Presentation;
        if (presentation == null)
        {
          MessageBox.Show("No active presentation view found.");
          return;
        }
        //Check what triggered the event and take appropriate action
        switch (args.Hint)
        {
          case PresentationViewEventHint.Activated:
            MessageBox.Show("Presentation view activated");
            break;
          case PresentationViewEventHint.Opened:
            MessageBox.Show("A PresentationView has been initialized and opened");
            break;
          case PresentationViewEventHint.Deactivated:
            MessageBox.Show("Presentation view deactivated");
            break;
          case PresentationViewEventHint.Closing:
            MessageBox.Show("Set args.Cancel = true to prevent closing");
            break;
          case PresentationViewEventHint.ExtentChanged:
            MessageBox.Show("Presentation view extent has changed");
            break;
          case PresentationViewEventHint.DrawingComplete:
            MessageBox.Show("Presentation view drawing complete");
            break;
          case PresentationViewEventHint.PauseDrawingChanged:
            MessageBox.Show("Presentation pause drawing change");
            break;
        }
      });
    }
  }
}
