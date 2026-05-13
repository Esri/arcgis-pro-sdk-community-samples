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
using ArcGIS.Core.Data.DDL;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ImportCsv
{
  internal class Import2FileGDBViewModel : DockPane
  {
    private const string _dockPaneID = "ImportCsv_Import2FileGDB";

    protected Import2FileGDBViewModel() { }

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

    #region Properties

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Import CSV into File Geodatabase";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private string _CsvFilePath = Path.Combine(GetAddinAssemblyLocation(), "EarthquakeDamage.csv");
    
    /// <summary>
    /// Csv file path to be processed
    /// </summary>
    public string CsvFilePath
    {
      get => _CsvFilePath;
      set => SetProperty(ref _CsvFilePath, value);
    }

    private string _GDBPath = Project.Current.DefaultGeodatabasePath;

    /// <summary>
    /// Path to the File Geodatabase where the CSV data will be imported.
    /// </summary>
    public string GDBPath
    {
      get => _GDBPath;
      set => SetProperty(ref _GDBPath, value);
    }


    private string _NewFCName = "CsvTest";

    /// <summary>
    /// Name of the new feature class to be created in the File Geodatabase.
    /// </summary>
    public string NewFCName
    {
      get => _NewFCName;
      set => SetProperty(ref _NewFCName, value);
    }

    /// <summary>
    /// Image source for the button to find CSV file
    /// </summary>
    public ImageSource CmdFindCsvImageSrc {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["SaveMetadataCSV16"] as ImageSource;
        return imageSource;
      }
    }

    /// <summary>
    /// Image source for the button to open File Geodatabase
    /// </summary>
    public ImageSource CmdGDBPathImageSrc
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["GeodatabaseAdd16"] as ImageSource;
        return imageSource;
      }
    }

    /// <summary>
    /// Image source for the button to create a new feature class from the CSV file
    /// </summary>
    public ImageSource CmdCreateNewFeatureClassImageSrc
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["GeodatabaseAdd16"] as ImageSource;
        return imageSource;
      }
    }

    #endregion Properties

    #region Commands

    /// <summary>
    /// Command to find the CSV file to import into the File Geodatabase.
    /// </summary>
    public ICommand CmdFindCsv
    {
      get
      {
        return new RelayCommand((args) =>
        {
          try
          {
            var dlg = new OpenItemDialog
            {
              Title = "Find CSV file to import",
              InitialLocation = Path.GetDirectoryName(CsvFilePath),
              AlwaysUseInitialLocation = true,
              Filter = "esri_browseDialogFilters_textFiles_csv"
            };

            if (!(dlg.ShowDialog() ?? false)) return;
            var item = dlg.Items.FirstOrDefault();
            if (item == null) return;

            CsvFilePath = item.Path;
            if (!File.Exists(CsvFilePath))
            {
              MessageBox.Show($@"CSV file doesn't exist: {CsvFilePath}");
              return;
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Error in open project: {ex}");
          }
        }, () => true);
      }
    }


    /// <summary>
    /// Command to set the File Geodatabase path where the CSV data will be imported.
    /// </summary>
    public ICommand CmdGDBPath
    {
      get
      {
        return new RelayCommand((args) =>
        {
          try
          {
            var dlg = new OpenItemDialog
            {
              Title = "Geodatabase to store newly created F/C for CSV import",
              InitialLocation = GDBPath,
              AlwaysUseInitialLocation = true,
              Filter = ItemFilters.Geodatabases
            };

            if (!(dlg.ShowDialog() ?? false)) return;
            var item = dlg.Items.FirstOrDefault();
            if (item == null) return;

            GDBPath = item.Path;
            if (!Directory.Exists (GDBPath))
            {
              MessageBox.Show($@"Geodatabase folder doesn't exist: {GDBPath}");
              return;
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Error in open project: {ex}");
          }
        }, () => true);
      }
    }

    public ICommand CmdCreateNewFeatureClass
    {
      get
      {
        return new RelayCommand(async (args) =>
        {
          try
          {
            if (!File.Exists(CsvFilePath))
            {
              MessageBox.Show($@"CSV file doesn't exist: {CsvFilePath}");
              return;
            }
            if (!Directory.Exists(GDBPath))
            {
              MessageBox.Show($@"Geodatabase folder doesn't exist: {GDBPath}");
              return;
            }
            if (await FeatureClassExistsAsync(GDBPath, NewFCName))
              throw new Exception($@"The Importer wants to create a new {NewFCName} feature class.  Use a new F/C name.");
            var resultCreate = await CreateFcWithAttributesAsync(GDBPath, NewFCName);
            if (!resultCreate)
            {
              MessageBox.Show($@"Failed to create feature class {NewFCName} in {GDBPath}");
              return;
            }
            // Copy the data from the CSV
            await QueuedTask.Run(() =>
            {
              using Geodatabase geodatabase = new(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath)));
              using FeatureClass newFc = geodatabase.OpenDataset<FeatureClass>(NewFCName);
              using FeatureClassDefinition newFcDefinition = newFc.GetDefinition();
              var spatialRef = newFcDefinition.GetSpatialReference();
              geodatabase.ApplyEdits(() =>
              {
                // copy the data using geodatabase f/c only

                // Read all lines from the file
                var lines = File.ReadLines(CsvFilePath);
                int lineNo = 1;

                // Skip the header line and process the rest
                foreach (var line in lines.Skip(1))
                {
                  var values = line.Split(',');
                  if (values.Length < 3)
                  {
                    throw new Exception($@"Invalid line [{lineNo + 1}] in CSV [{Path.GetFileName(CsvFilePath)}]: {line}");
                  }
                  MapPoint pointWGS84 = MapPointBuilderEx.CreateMapPoint(Convert.ToDouble(values[0]),
                    Convert.ToDouble(values[1]),
                    SpatialReferences.WGS84);
                  MapPoint geom = GeometryEngine.Instance.Project(
                    pointWGS84, spatialRef) as MapPoint;
                  // copy the data
                  using var newRowBuffer = newFc.CreateRowBuffer();
                  newRowBuffer["Shape"] = geom;
                  newRowBuffer["LineNo"] = lineNo;
                  newRowBuffer["Name"] = values[2];
                  using Row newRow = newFc.CreateRow(newRowBuffer);
                  lineNo++;
                }
              });
            });
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Error creating feature class: {ex}");
          }
        }, () => true);
      }
    }

    #endregion Commands

    #region Helpers

    private Task<bool> CreateFcWithAttributesAsync(string gDBPath, string newFCName)
    {
      return QueuedTask.Run<bool>(() =>
      {
        try
        {
          var spatialRef = MapView.Active?.Map.SpatialReference ?? SpatialReferences.WebMercator;
          using Geodatabase projectGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gDBPath)));
          SchemaBuilder schemaBuilder = new SchemaBuilder(projectGDB);

          // Creating a 'point' feature class with a name fields
          ArcGIS.Core.Data.DDL.FieldDescription lineNo = new("LineNo", FieldType.Integer);
          ArcGIS.Core.Data.DDL.FieldDescription name = new("Name", FieldType.String);

          FeatureClassDescription featureClassDescription = new(newFCName, 
            new List<ArcGIS.Core.Data.DDL.FieldDescription> { lineNo, name },
            new ShapeDescription(GeometryType.Point, spatialRef)
            {
              HasM = false,
              HasZ = false 
            });

          schemaBuilder.Create(featureClassDescription);
          return schemaBuilder.Build();
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($@"FeatureClassExists Error: {ex.ToString()}");
          return false;
        }
      });
    }

    private static string GetAddinAssemblyLocation()
    {
      var asm = System.Reflection.Assembly.GetExecutingAssembly();
      return System.IO.Path.GetDirectoryName(
                        Uri.UnescapeDataString(
                                new Uri(asm.Location).LocalPath));
    }

    private static Task<bool> FeatureClassExistsAsync(string gdbPath, string fcName)
    {
      return QueuedTask.Run(() =>
      {
        try
        {
          using Geodatabase projectGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath)));
          using FeatureClass fc = projectGDB.OpenDataset<FeatureClass>(fcName);
          return fc != null;
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($@"FeatureClassExists Error: {ex.ToString()}");
          return false;
        }
      });
    }

    #endregion Helpers
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class Import2FileGDB_ShowButton : Button
  {
    protected override void OnClick()
    {
      Import2FileGDBViewModel.Show();
    }
  }
}
