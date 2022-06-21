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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CreateDiagramWithACustomLayout.CreateDiagramWithACustomLayoutModule;


namespace CreateDiagramWithACustomLayout
{
  internal class GenerateEnclosure : Button
  {
    DiagramManager m_DiagramManager;
    DiagramTemplate m_Template;

    /// <summary>
    /// Create a  diagram based on the Enclosure Diagram template from the current selection set and apply the EnclosureLayout custom telco layout to its content
    /// </summary>
    protected override async void OnClick()
    {
      if (MapView.Active == null || MapView.Active.Map == null)
        return;

      // If you run this in the DEBUGGER you will NOT see the dialog progressor
      var pd = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("Generate Enclosure Diagram - Cancelable", "Canceled", 6, false);

      string returnedValue = await RunCancelableEnclosure(new CancelableProgressorSource(pd));

      if (!string.IsNullOrEmpty(returnedValue))
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(returnedValue);
      }
    }

    /// <summary>
    /// Generate a diagram and apply the EnclosureLayout custom telco layout on its content
    /// </summary>
    /// <param name="cps">Cancelable Progressor Source to show the progression</param>
    /// <returns>An error comment if needed, empty of no error</returns>
    private async Task<string> RunCancelableEnclosure(CancelableProgressorSource cps)
    {
      string status = "";
      List<Guid> listIds = null;
      NetworkDiagram myDiagram = null;

      await QueuedTask.Run(() =>
      {
        cps.Progressor.Max = 3;

        cps.Progressor.Value += 0;
        cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
        cps.Progressor.Message = "Step 1 – Get Selected Guids";

        try
        {
          if (m_DiagramManager == null)
          {
            UtilityNetwork un = GetUtilityNetworkFromActiveMap();
            if (un == null)
              return;

            m_DiagramManager = un.GetDiagramManager();

            if (m_DiagramManager == null)
              return;
          }

          if (m_Template == null)
          {
            m_Template = m_DiagramManager.GetDiagramTemplate(csTemplateName);
            if (m_Template == null)
              return;
          }

          listIds = GetSelectedGuidFromActiveMap();
          if (listIds.Count == 0)
            return;
        }
        catch (Exception ex)
        {
          status = string.Format("Selected guids\n{0}", ExceptionFormat(ex));
        }
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        cps.Progressor.Value += 1;
        cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
        cps.Progressor.Message = string.Format("Step 2 – Generate a diagram based on the '{0}' template", csTemplateName);

        try
        {
          // generate a diagram
          myDiagram = m_DiagramManager.CreateNetworkDiagram(diagramTemplate: m_Template, globalIDs: listIds);
        }
        catch (Exception ex)
        {
          if (string.IsNullOrEmpty(status))
            status = string.Format("Generate diagram\n{0}", ExceptionFormat(ex));
          else
            status = string.Format("Generate diagram\n{0}", ExceptionFormat(ex));
        }
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        cps.Progressor.Value += 1;
        cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
        cps.Progressor.Message = "Step 3 – Apply the EnclosureLayout custom telco layout";

        try
        {
          // apply the telco custom layout 
          EnclosureLayout myLayout = new EnclosureLayout();
          myLayout.Execute(myDiagram);

          ShowDiagram(myDiagram);
        }
        catch (Exception ex)
        {
          if (string.IsNullOrEmpty(status))
            status = string.Format("Apply layout\n{0}", ExceptionFormat(ex));
          else
            status = string.Format("Apply layout\n{0}", ExceptionFormat(ex));
        }
      });

      return status;
    }
  }
}
