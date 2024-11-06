/*

   Copyright 2024 Esri

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
using ActiproSoftware.Windows.Extensions;
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VersionUpdatePerformance
{
  internal class VersionUpdateViewModel : DockPane
  {
    private const string _dockPaneID = "VersionUpdatePerformance_VersionUpdate";
    private string _nl = Environment.NewLine;

    private (bool IsSDE, bool IsFileGdb) SourceGdbType;
    private (bool IsSDE, bool IsFileGdb) DestGdbType;

    protected VersionUpdateViewModel() 
    {
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_versions, _lock);
    }

    #region Feature Class input/output

    private string _fcSource;
    public string FcSource
    {
      get { return _fcSource; }
      set
      {
        SetProperty(ref _fcSource, value, () => FcSource);
      }
    }

    private string _fcDestination;
    public string FcDestination
    {
      get { return _fcDestination; }
      set
      {
        SetProperty(ref _fcDestination, value, () => FcDestination);
      }
    }

    private string _status;
    public string Status
    {
      get { return _status; }
      set
      {
        SetProperty(ref _status, value, () => Status);
      }
    }

    private ObservableCollection<string> _versions = [];
    private object _lock = new();
    public ObservableCollection<string> Versions
    {
      get => _versions;
    }

    private string _selectedVersion;
    public string SelectedVersion
    {
      get => _selectedVersion;
      set
      {
        SetProperty(ref _selectedVersion, value);
      }
    }

    public ICommand CmdUpdateDestinationFc
    {
      get
      {
        return new RelayCommand(() => {
          OpenItemDialog openFc = new()
          {
            Title = "Select Destination Feature Class",
            InitialLocation = @"C:\Data",
            MultiSelect = false,
            //Use BrowseFilter property to specify the filter 
            //you want the dialog to display
            BrowseFilter = BrowseProjectFilter.GetFilter("esri_browseDialogFilters_geodatabaseItems_featureClasses")
          };
          bool? ok = openFc.ShowDialog();
          if (ok.HasValue && ok.Value && openFc.Items.Count > 0)
          {
            foreach (Item itm in openFc.Items)
            {
              FcDestination = itm.Path;
              break;
            }
          }
        }, true);
      }
    }

    public ICommand CmdUpdateSourceFc
    {
      get
      {
        return new RelayCommand(() => {
          OpenItemDialog openFc = new ()
          {
            Title = "Select Source Feature Class",
            InitialLocation = @"C:\Data",
            MultiSelect = false,
            //Use BrowseFilter property to specify the filter 
            //you want the dialog to display
            BrowseFilter = BrowseProjectFilter.GetFilter("esri_browseDialogFilters_geodatabaseItems_featureClasses")
          };
          bool? ok = openFc.ShowDialog();
          if (ok.HasValue && ok.Value && openFc.Items.Count() > 0)
          {
            foreach (Item itm in openFc.Items)
            {
              FcSource = itm.Path;
              break;
            }
          }
        }, true);
      }
    }

    public ICommand CmdRefreshDestinationFc
    {
      get
      {
        return new RelayCommand(async () =>
          {
            try
            {
              StringBuilder sb = new();
              Versions.Clear();
              // step 1: parse input values: FcSource and FcDest
              var sourceGdbPath = string.Empty;
              var sourceFcName = string.Empty;
              var destGdbPath = string.Empty;
              var destFcName = string.Empty;
              var singleGDB = false;
              sb.Append(ParseInput(out singleGDB,
                out sourceGdbPath, out sourceFcName,
                out destGdbPath, out destFcName));
              Status = sb.ToString();
              if (DestGdbType.IsSDE)
              {
                var versionLst = await QueuedTask.Run<IReadOnlyList<string>>(() =>
                {
                  // open the source feature class
                  using Geodatabase geodatabase = new(new DatabaseConnectionFile(new Uri(destGdbPath, UriKind.Absolute)));
                  return GetVersions(geodatabase);
                });
                Versions.AddRange(versionLst);
                if (Versions.Count > 0)
                {
                  SelectedVersion = Versions[0];
                }
              }
            }
            catch (Exception ex)
            {
              MessageBox.Show($@"Error trying to get versions: {ex}");
            }
          });
      }
    }

    public ICommand CmdCopy
    {
      get
      {
        return new RelayCommand(async () =>
        {
          StringBuilder sb = new();
          try
          {
            // step 1: parse input values: FcSource and FcDest
            var sourceGdbPath = string.Empty;
            var sourceFcName = string.Empty;
            var destGdbPath = string.Empty;
            var destFcName = string.Empty;
            var createNewFeatureClass = false;
            sb.Append(ParseInput(out createNewFeatureClass,
              out sourceGdbPath, out sourceFcName,
              out destGdbPath, out destFcName));
            Status = sb.ToString();
            // Step 2: Check is Gdbs existing and create destination F/C if it doesn't exist
            var prepResult = await QueuedTask.Run<(bool isOk, string msg)>(() =>
            {
              FeatureClass sourceFc = null;
              FeatureClass destFc = null;
              // open the source feature class
              using Geodatabase geodatabase = SourceGdbType.IsFileGdb ?
                  new(new FileGeodatabaseConnectionPath(new Uri(sourceGdbPath, UriKind.Absolute)))
                : new(new DatabaseConnectionFile(new Uri(sourceGdbPath, UriKind.Absolute)));
              sourceFc = geodatabase.OpenDataset<FeatureClass>(sourceFcName);
              // open the Dest feature class
              // open a file geodatabase
              using Geodatabase destGeodatabase = DestGdbType.IsFileGdb ?
                  new(new FileGeodatabaseConnectionPath(new Uri(destGdbPath, UriKind.Absolute)))
                  : new(new DatabaseConnectionFile(new Uri(destGdbPath, UriKind.Absolute)));
              try
              {
                destFc = destGeodatabase.OpenDataset<FeatureClass>(destFcName);
                createNewFeatureClass = destFc == null;
              }
              catch
              {
                createNewFeatureClass = true;
              }
              if (createNewFeatureClass)
              {
                var newFcName = destFcName;
                sb.AppendLine($@"Created new F/C: {newFcName}");
                // create the destination GDB F/C
                destFc = null;
                FeatureClassDefinition fcDef = sourceFc.GetDefinition();
                List<ArcGIS.Core.Data.DDL.FieldDescription> fieldDescriptions = new();
                foreach (var field in fcDef.GetFields())
                {
                  if (field.FieldType != FieldType.Geometry
                    && !field.ModelName.StartsWith("Shape."))
                    fieldDescriptions.Add(new ArcGIS.Core.Data.DDL.FieldDescription(field));
                }
                // Create a TableDescription object to describe the table to create
                FeatureClassDescription featureClassDescription = new(newFcName,
                              fieldDescriptions,
                              new ShapeDescription(fcDef));
                // Create a SchemaBuilder object
                SchemaBuilder schemaBuilder = new(destGeodatabase);
                // Add the creation of PoleInspection to our list of DDL tasks
                schemaBuilder.Create(featureClassDescription);
                // Execute the DDL
                bool success = schemaBuilder.Build();
                return (success, string.Join(",", schemaBuilder.ErrorMessages));
              }
              return (true, string.Empty);
            });
            Status = sb.ToString();
            if (!prepResult.isOk)
            {
              MessageBox.Show($@"Failed to create {destFcName} {prepResult.msg} ");
              return;
            }
            // Step 3: now copy the data to the correct version
            // if the version doesn't exist then the version is created
            await QueuedTask.Run(() =>
            {
              using Geodatabase sourceGdb = SourceGdbType.IsFileGdb ?
                    new(new FileGeodatabaseConnectionPath(new Uri(sourceGdbPath, UriKind.Absolute)))
                  : new(new DatabaseConnectionFile(new Uri(sourceGdbPath, UriKind.Absolute)));
              var sourceFc = sourceGdb.OpenDataset<FeatureClass>(sourceFcName);
              Geodatabase destGdb = DestGdbType.IsFileGdb ?
                    new(new FileGeodatabaseConnectionPath(new Uri(destGdbPath, UriKind.Absolute)))
                  : new(new DatabaseConnectionFile(new Uri(destGdbPath, UriKind.Absolute)));
              if (DestGdbType.IsSDE)
              {
                var versionGdb = ConnectToVersion(destGdb, SelectedVersion);
                if (versionGdb == null)
                {
                  // create the version
                  var newVersion = CreateVersion(destGdb, SelectedVersion, $@"Perf test: {DateTime.Now.ToShortTimeString}", VersionAccessType.Public);
                  destGdb = ConnectToVersion(destGdb, SelectedVersion);
                }
                else
                {
                  // use the verisonGdb
                  destGdb = versionGdb;
                }
              }
              var destFc = destGdb.OpenDataset<FeatureClass>(destFcName);

              FieldMap fieldMap = new(sourceFc, destFc);

              var timer = new Stopwatch();
              timer.Start();
              using var rowCursor = sourceFc.Search();
              timer.Stop();
              TimeSpan timeTaken = timer.Elapsed;
              sb.AppendLine($@"Search: {timeTaken}");
              timer.Restart();
              uint recordCnt = 0;
              destGdb.ApplyEdits(() =>
              {
                while (rowCursor.MoveNext())
                {
                  using var feature = rowCursor.Current as Feature;
                  using RowBuffer rowBuffer = destFc.CreateRowBuffer();
                  // use the fieldMap to copy attributes in addition to Geometry
                  rowBuffer[fieldMap.ToShapeName] = feature.GetShape().Clone();
                  foreach (var fldMap in fieldMap.FromToMap)
                  {
                    rowBuffer[fldMap.ToFieldName] = feature[fldMap.FromFieldName];
                  }
                  destFc.CreateRow(rowBuffer);
                  recordCnt++;
                }
              });
              timer.Stop();
              timeTaken = timer.Elapsed;
              sb.AppendLine($@"Cursor: {timeTaken.TotalMilliseconds} msec");
              sb.AppendLine($@"Records: {recordCnt}");
              if (recordCnt > 0)
              {
                var dblMs = Convert.ToDouble(timeTaken.TotalMilliseconds);
                var dblCnt = Convert.ToDouble(recordCnt);
                sb.AppendLine($@"Per record: {dblMs / dblCnt} msec");
              }
            });
            Status = sb.ToString();
            // Step 4: add the new FeatureClass to the map
            var newLyr = await QueuedTask.Run(() =>
            {
              var lyrName = $@"New: {destFcName}";
              // does the layer exist?
              if (MapView.Active != null 
                  && MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals(lyrName)) == null)
              {
                Geodatabase destGdb = DestGdbType.IsFileGdb ?
                              new(new FileGeodatabaseConnectionPath(new Uri(destGdbPath, UriKind.Absolute)))
                            : new(new DatabaseConnectionFile(new Uri(destGdbPath, UriKind.Absolute)));
                if (DestGdbType.IsSDE)
                {
                  var versionGdb = ConnectToVersion(destGdb, SelectedVersion);
                  if (versionGdb == null)
                  {
                    // create the version
                    var newVersion = CreateVersion(destGdb, SelectedVersion, $@"Perf test: {DateTime.Now.ToShortTimeString}", VersionAccessType.Public);
                    destGdb = ConnectToVersion(destGdb, SelectedVersion);
                  }
                  else
                  {
                    // use the verisonGdb
                    destGdb = versionGdb;
                  }
                }
                var newFc = destGdb.OpenDataset<FeatureClass>(destFcName);
                return LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(newFc) { Name = lyrName }, MapView.Active.Map);
              }
              return null;
            });
          }
          catch (Exception ex)
          {
            sb.AppendLine(ex.ToString());
          }
          Status = sb.ToString();
        }, true);
      }
    }

    private string ParseInput(
      out bool singleGDB,
      out string sourceGdbPath,
      out string sourceFcName,
      out string destGdbPath, 
      out string destFcName)
    {
      singleGDB = false;
      StringBuilder sb = new();
      SourceGdbType = ParseGeoDatabasePath(FcSource, out sourceGdbPath, out sourceFcName);
      if (!SourceGdbType.IsFileGdb && !SourceGdbType.IsSDE)
      {
        sb.AppendLine($@"Unable to parse source Path:{_nl}{FcSource}");
      }
      else
      {
        sb.AppendLine($@"Source Gdb:{_nl}{sourceGdbPath}{_nl}{sourceFcName}");
      }
      destGdbPath = string.Empty;
      destFcName = string.Empty;
      DestGdbType = ParseGeoDatabasePath(FcDestination, out destGdbPath, out destFcName);
      if (!DestGdbType.IsSDE && !DestGdbType.IsFileGdb)
      {
        var newDest = $@"{sourceGdbPath}\{FcDestination}";
        DestGdbType = ParseGeoDatabasePath(newDest, out destGdbPath, out destFcName);
        if (!DestGdbType.IsSDE && !DestGdbType.IsFileGdb)
        {
          sb.AppendLine($@"Unable to parse destination Path:{_nl}{FcDestination}");
          destFcName = FcDestination;
          sb.AppendLine($@"Create destination F/C:{_nl}{FcDestination}");
        }
        else
        {
          destGdbPath = sourceGdbPath;
          singleGDB = true;
          sb.AppendLine($@"Destination Gdb:{_nl}{destGdbPath}{_nl}{destFcName}");
        }
      }
      else
      {
        sb.AppendLine($@"Destination Gdb:{_nl}{destGdbPath}{_nl}{destFcName}");
      }
      return sb.ToString();
    }

    #endregion Feature Class input/output

    #region Button Images loaded from Pro

    public System.Windows.Media.ImageSource SourceImg
    {
      get { return System.Windows.Application.Current.Resources["AddBathymetryDataset32"] as System.Windows.Media.ImageSource; }
    }

    public System.Windows.Media.ImageSource DestinationImg
    {
      get { return System.Windows.Application.Current.Resources["SaveMetadataGDB32"] as System.Windows.Media.ImageSource; }
    }

    public System.Windows.Media.ImageSource CopyImg
    {
      get { return System.Windows.Application.Current.Resources["SelectionCopySelectedFeatures32"] as System.Windows.Media.ImageSource; }
    }

    public System.Windows.Media.ImageSource RefreshDestinationImg
    {
      get { return System.Windows.Application.Current.Resources["MapSeriesRefresh16"] as System.Windows.Media.ImageSource; }
    }

    #endregion

    /// <summary>
    /// Parse a geodatabase path into GeoDatabase, GeoDatabase Type, and FeatureClass Name
    /// i.e.
    /// C:\Users\wolf2125\Documents\ArcGIS\Projects\MyProject\SQLServer-localhost-PerformanceTest.sde\PerformanceTest.DBO.ReadTest
    /// </summary>
    /// <param name="geodatabasePath"></param>
    /// <param name="gdb"></param>
    /// <param name="FcName"></param>
    private (bool IsSDE, bool IsFileGdb) ParseGeoDatabasePath(string geodatabasePath, out string gdb, out string FcName)
    {
      gdb = null;
      FcName = null;
      var isGdb = false;
      var isSde = geodatabasePath.Contains(@".sde\", StringComparison.CurrentCultureIgnoreCase);
      if (isSde)
      {
        var parts = geodatabasePath.ToLower().Split(@".sde\");
        gdb = parts[0] + @".sde";
        FcName = parts[1];
        if (!string.IsNullOrEmpty(FcName) && FcName.Contains(@"."))
        {
          parts = FcName.Split(@".");
          FcName = parts[^1];
        }
      }
      else
      {
        isGdb = geodatabasePath.Contains(@".gdb\", StringComparison.CurrentCultureIgnoreCase);
        if (isGdb)
        {
          var parts = geodatabasePath.ToLower().Split(@".gdb\");
          gdb = parts[0] + @".gdb";
          FcName = parts[1];
        }
      }
      if (!string.IsNullOrEmpty(FcName) && FcName.Contains(@"\"))
      {
        var parts = FcName.Split(@"\");
        FcName = parts[^1];
      }
      return (isSde, isGdb);
    }

    public ArcGIS.Core.Data.Version CreateVersion(Geodatabase geodatabase, string versionName, string description, VersionAccessType versionAccessType)
    {
      if (!geodatabase.IsVersioningSupported()) return null;

      using (VersionManager versionManager = geodatabase.GetVersionManager())
      {
        VersionDescription versionDescription = new VersionDescription(versionName, description, versionAccessType);
        return versionManager.CreateVersion(versionDescription);
      }
    }

    public Geodatabase ConnectToVersion(Geodatabase geodatabase, string versionName)
    {
      Geodatabase connectedVersion = null;
      try
      {
        if (geodatabase.IsVersioningSupported())
        {
          using VersionManager versionManager = geodatabase.GetVersionManager();
          using ArcGIS.Core.Data.Version version = versionManager.GetVersion(versionName);
          connectedVersion = version.Connect();
        }
      }
      catch { }
      return connectedVersion;
    }

    public IReadOnlyList<string> GetVersions(Geodatabase geodatabase)
    {
      if (geodatabase.IsVersioningSupported())
      {
        using VersionManager versionManager = geodatabase.GetVersionManager();
        IReadOnlyList<string> versions = versionManager.GetVersionNames();
        return versions;
      }
      return null;
    }

    private static Task<bool> FeatureClassExistsAsync(Geodatabase geoDatabase, string fcName)
    {
      return QueuedTask.Run(() =>
      {
        try
        {
          using FeatureClass fc = geoDatabase.OpenDataset<FeatureClass>(fcName);
          return fc != null;
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($@"FeatureClassExists Error: {ex.ToString()}");
          return false;
        }
      });
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
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class VersionUpdate_ShowButton : Button
  {
    protected override void OnClick()
    {
      VersionUpdateViewModel.Show();
    }
  }
}
