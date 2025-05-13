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
using ArcGIS.Desktop.Internal.Mapping.TOC;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenChartPane
{
  internal class OpenChartInTOC : Button
  {

    protected override void OnClick()
    {
      try
      {
        var command = FrameworkApplication.GetPlugInWrapper("esri_charts_OpenChartButton") as ICommand;
        if (command.CanExecute(null))
          command.Execute(null);

        var mapMembers = MapView.Active?.Map?.GetMapMembersAsFlattenedList();
        if (mapMembers == null || mapMembers.Count == 0)
          return;
        foreach (var mapMember in mapMembers)
        {
          System.Diagnostics.Trace.WriteLine($@"{mapMember.Name} Type: {mapMember.GetType()}");
        }


        //  ChartTOCViewModel chartTOCViewModel = null;
        //var toc = ChartsModule.MappingInternal?.ActiveTOC as IInternalMapTOC;
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }

    //static async internal void OpenChart(Chart chart = null)
    //{
    //  MapMember mapMember = null;
    //  if (chart == null)
    //  {
    //    var toc = ChartsModule.MappingInternal?.ActiveTOC as IInternalMapTOC;
    //    ChartTOCViewModel chartVM = toc?.TOC?.ItemSelection?.SelectedItems?.FirstOrDefault() as ChartTOCViewModel;
    //    if (chartVM == null)
    //      return;

    //    TOCMapMemberViewModel mapMemberVM = chartVM.Parent as TOCMapMemberViewModel;
    //    mapMember = mapMemberVM?.Model as MapMember;
    //    if (mapMember == null)
    //      return;
    //    chart = new Chart(mapMember.URI, chartVM.ChartName);
    //  }
    //  else
    //    mapMember = await ChartsModule.MappingInternal.GetMapMemberAsync(chart.LayerURI);

    //  string[] seriesTypes = await chart.getSeriesTypesAsync();
    //  if (seriesTypes == null || seriesTypes.Length == 0)
    //    return;

    //  if (!CreateChartPropertiesPane(chart.ChartName, seriesTypes[0]))
    //    return;

    //  // find out if charts pane is opened
    //  bool bPaneFound = false;
    //  int nPanes = FrameworkApplication.Panes.Count;
    //  for (int i = 0; i < nPanes; i++)
    //  {
    //    var pane = FrameworkApplication.Panes[i];
    //    var chartPane = pane as ChartPaneViewModel;
    //    if (chartPane == null)
    //      continue;

    //    if (chartPane.ChartParentLayer == chart.LayerURI && chartPane.ChartName == chart.ChartName)
    //    {
    //      pane.Activate();
    //      var project = Project.Current;
    //      if (project != null)
    //        project.SetDirty();
    //      bPaneFound = true;
    //      break;
    //    }
    //  }

    //  if (!bPaneFound)
    //    CreateChartViewPane(mapMember, chart.ChartName, seriesTypes[0]);
    //}
  }
}
