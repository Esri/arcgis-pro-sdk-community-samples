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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using System.Windows.Input;
using System.Windows;
using ArcGIS.Desktop.Editing;
using System.Windows.Media;

namespace CrowdPlannerTool
{

    #region Showbutton
    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CPDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CPDockpaneViewModel.Show();
        }
    }
    #endregion

    #region DockPane Activate
    internal class CPDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "CrowdPlannerTool_CPDockpane";
        private KeyValuePair<string, int>[] _piechartResult;
        private KeyValuePair<string, int>[] _piechartResultMedium;
        private KeyValuePair<string, int>[] _piechartResultLow;
        private string textboxBackground;

        /// <summary>
        /// Show the DockPane
        /// </summary>
        internal static void Show()
        {
            CPDockpaneViewModel pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as CPDockpaneViewModel;
            if (pane == null)
                return;

            pane.Activate();

        }


        protected CPDockpaneViewModel()
        {
            _updateSettingsCmd = new RelayCommand(() => GetTotalValues(), () => true);

            _updateSettingsOnlyCmd = new RelayCommand(() => EnableSettings(), () => true);

            _updateSettingsAllCurrentCmd = new RelayCommand(() => ResetValues("current"), () => true);

            _updateSettingsAllDefaultCmd = new RelayCommand(() => ResetValues("default"), () => true);

            _setDefaultSettingsCmd = new RelayCommand(() => SetDefaultSettings(), () => true);

            //string textboxBackground;
            if (FrameworkApplication.ApplicationTheme == ApplicationTheme.Dark ||
                FrameworkApplication.ApplicationTheme == ApplicationTheme.HighContrast)
                textboxBackground = "#323232";
            else textboxBackground = "#FFFFFF";
            //set background value for textboxes
            _highBackground = textboxBackground;
            _mediumBackground = textboxBackground;
            _lowBackground = textboxBackground;

        }

        #endregion

        #region Commands
        // ** COMMANDS **
        private RelayCommand _updateSettingsCmd;
        public ICommand UpdateSettingsCmd
        {
            get
            {
                return _updateSettingsCmd;
            }
        }


        private RelayCommand _updateSettingsOnlyCmd;
        public ICommand UpdateSettingsOnlyCmd
        {
            get
            {
                return _updateSettingsOnlyCmd;
            }
        }

        private RelayCommand _updateSettingsAllCurrentCmd;
        public ICommand UpdateSettingsAllCurrentCmd
        {
            get
            {
                return _updateSettingsAllCurrentCmd;
            }
        }

        private RelayCommand _updateSettingsAllDefaultCmd;
        public ICommand UpdateSettingsAllDefaultCmd
        {
            get
            {
                return _updateSettingsAllDefaultCmd;
            }
        }

        private RelayCommand _setDefaultSettingsCmd;
        public ICommand SetDefaultSettingsCmd
        {
            get
            {
                return _setDefaultSettingsCmd;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        //private string _heading = "Crowd Estimate Summary";
        //public string Heading
        //{
        //    get { return _heading; }
        //    set
        //    {
        //        SetProperty(ref _heading, value, () => Heading);
        //    }
        //}


        private long _totalHigh = 0;
        public long TotalHigh
        {
            get
            {
                return _totalHigh;
            }
            set
            {
                _totalHigh = value;
                NotifyPropertyChanged();
            }
        }

        private double _highSetting = 0;
        public double HighSetting
        {
            get
            {
                return _highSetting;
            }
            set
            {
                _highSetting = value;
                NotifyPropertyChanged();
            }
        }

        private double _mediumSetting = 0;
        public double MediumSetting
        {
            get
            {
                return _mediumSetting;
            }
            set
            {
                _mediumSetting = value;
                NotifyPropertyChanged();
            }
        }

        private double _lowSetting = 0;
        public double LowSetting
        {
            get
            {
                return _lowSetting;
            }
            set
            {
                _lowSetting = value;
                NotifyPropertyChanged();
            }
        }

        private long _targetSetting = 0;
        public long TargetSetting
        {
            get
            {
                return _targetSetting;
            }
            set
            {
                _targetSetting = value;
                NotifyPropertyChanged();
            }
        }

        private long _totalMedium = 0;
        public long TotalMedium
        {
            get
            {
                return _totalMedium;
            }
            set
            {
                _totalMedium = value;
                NotifyPropertyChanged();
            }
        }

        private long _totalLow = 0;
        public long TotalLow
        {
            get
            {
                return _totalLow;
            }
            set
            {
                _totalLow = value;
                NotifyPropertyChanged();
            }
        }

        // High total background
        private string _highBackground;   // "#FFFFFF";
        public string HighBackground
        {
            get
            {
                return _highBackground;
            }
            set
            {
                _highBackground = value;
                NotifyPropertyChanged("ColorBrush");
            }
        }

        public Brush ColorBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(_highBackground));

        // Medium total background
        private string _mediumBackground;   // = "#FFFFFF";
        public string MediumBackground
        {
            get
            {
                return _mediumBackground;
            }
            set
            {
                _mediumBackground = value;
                NotifyPropertyChanged("ColorBrushMedium");
            }
        }
        public Brush ColorBrushMedium => new SolidColorBrush((Color)ColorConverter.ConvertFromString(_mediumBackground));

        // Low total background
        private string _lowBackground;   // = "#FFFFFF";
        public string LowBackground
        {
            get
            {
                return _lowBackground;
            }
            set
            {
                _lowBackground = value;
                NotifyPropertyChanged("ColorBrushLow");
            }
        }
        public Brush ColorBrushLow => new SolidColorBrush((Color)ColorConverter.ConvertFromString(_lowBackground));

        // High Estimate Chart
        public KeyValuePair<string, int>[] PieChartResult
        {
            get
            {
                return _piechartResult;
            }
            set
            {
                SetProperty(ref _piechartResult, value, () => PieChartResult);
            }
        }

        // Medium Estimate Chart
        public KeyValuePair<string, int>[] PieChartResultMedium
        {
            get
            {
                return _piechartResultMedium;
            }
            set
            {
                SetProperty(ref _piechartResultMedium, value, () => PieChartResultMedium);
            }
        }

        // Low Estimate Chart
        public KeyValuePair<string, int>[] PieChartResultLow
        {
            get
            {
                return _piechartResultLow;
            }
            set
            {
                SetProperty(ref _piechartResultLow, value, () => PieChartResultLow);
            }
        }



        #endregion

        #region EnableSettings

        private bool _canEdit = false;
        public void EnableSettings()
        {
            _canEdit = !_canEdit;
            NotifyPropertyChanged("CanEdit");

        }

        public bool CanEdit => _canEdit;

        #endregion

        #region SetDefaultSettings
        public void SetDefaultSettings()
        {

            HighSetting = 2.5;
            MediumSetting = 4.5;
            LowSetting = 10;

            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Default settings set for current editing only.  Use Reset to Defaults to reset all polygons.", "Settings");

        }


        #endregion

        #region ResetValues
        // Reset Settings Based on Current Settings
        public async void ResetValues(string settingsValue)
        {
            if (MapView.Active == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("CrowdPlanning Layer is not found. Please open the map view of the Crowd Planner data project.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            // Check for CrowdPlanning layer, exit if not present in current Map.
            var CrowdLayerTest = MapView.Active.Map.FindLayers("CrowdPlanning").FirstOrDefault() as BasicFeatureLayer;
            if (CrowdLayerTest == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("CrowdPlanning Layer is not found. Please use with the Crowd Planner data project.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // ***  Check to ensure the densitysettings are set.  If not, show a warning and deactivate tool.
            if (HighSetting == 0 || MediumSetting == 0 || LowSetting == 0 || TargetSetting == 0)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("An empty setting value exists. All settings are required to use this tool.", "Warning!");
                // ** End if there are not complete settings
                return;
            }
            // else proceed with confirming a reset is desired.
            else
            {
                if (settingsValue == "current")
                {
                    // Prompt for confirmation, and if answer is no, return.
                    var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Reset all values to CURRENT settings?", "RESET VALUES", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                    // Return if cancel value is chosen
                    if (Convert.ToString(result) == "Cancel")
                        return;
                }

                else if (settingsValue == "default")
                {
                    // Prompt for confirmation, and if answer is no, return.
                    var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Reset all values to DEFAULT settings?", "RESET VALUES", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                    // Return if cancel value is chosen
                    if (Convert.ToString(result) == "Cancel")
                        return;
                }
            }

            FeatureLayer CrowdLayer = MapView.Active.Map.Layers.First(layer => layer.Name.Equals("CrowdPlanning")) as FeatureLayer;
            if (CrowdLayer == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("CrowdPlanning Layer is not found");
                return;
            }
            ArcGIS.Core.Data.Table CrowdTable = await QueuedTask.Run(() => CrowdLayer.GetTable());

            await QueuedTask.Run(() =>
            {

                var editOperation = new EditOperation();
                editOperation.Callback(context =>
                {

                    QueryFilter QF = new QueryFilter
                    {

                        WhereClause = "LOW > 0"
                    };

                    RowCursor CrowdRow = CrowdTable.Search(QF, false);

                    while (CrowdRow.MoveNext())
                    {

                        using (Row currentRow = CrowdRow.Current)
                        {

                            var squarefeetValue = currentRow["Shape_Area"];
                            long squarefeetValueLong;
                            squarefeetValueLong = Convert.ToInt64(squarefeetValue);

                            if (settingsValue == "current")
                            {
                                currentRow["High"] = (squarefeetValueLong / HighSetting);
                                currentRow["Medium"] = (squarefeetValueLong / MediumSetting);
                                currentRow["Low"] = (squarefeetValueLong / LowSetting);
                                currentRow["TargetSetting"] = TargetSetting;
                                currentRow["HighSetting"] = HighSetting;
                                currentRow["MediumSetting"] = MediumSetting;
                                currentRow["LowSetting"] = LowSetting;
                            }

                            else if (settingsValue == "default")
                            {
                                currentRow["High"] = squarefeetValueLong / 2.5;
                                currentRow["Medium"] = squarefeetValueLong / 4.5;
                                currentRow["Low"] = squarefeetValueLong / 10;
                                currentRow["TargetSetting"] = TargetSetting;
                                currentRow["HighSetting"] = 2.5;
                                currentRow["MediumSetting"] = 4.5;
                                currentRow["LowSetting"] = 10;
                            }

                        // Store the values
                        currentRow.Store();

                        // Has to be called after the store too.
                        context.Invalidate(currentRow);

                        }
                    }
                    CrowdTable.Dispose();
                // close the editOperation.Callback(context
            }, CrowdTable);

                editOperation.ExecuteAsync();

            }); // new closing QueuedTask

            GetTotalValues();

        }

        #endregion

        #region GetTotalValues
        //public async void GetTotalValues()
        public async void GetTotalValues()
        {

            long TotalValueLow = 0;
            long TotalValueMedium = 0;
            long TotalValueHigh = 0;
            long targetSettingValue = 0;
            double lowSettingValue = 0;
            double mediumSettingValue = 0;
            double highSettingValue = 0;
            double targetRemaining = 0;

            // Check for CrowdPlanning layer, exit if not present in current Map.
            if (MapView.Active == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("CrowdPlanning Layer is not found. Please open the map view of the Crowd Planner data project.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            var CrowdLayerTest = MapView.Active.Map.FindLayers("CrowdPlanning").FirstOrDefault() as BasicFeatureLayer;
            if (CrowdLayerTest == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("CrowdPlanning Layer is not found. Please use with the Crowd Planner data project.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            await QueuedTask.Run(() =>
            {
                // FeatureLayer CrowdLayer = MapView.Active.Map.FindLayer("CrowdPlanning") as FeatureLayer;
                FeatureLayer CrowdLayer = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(layer => layer.Name.Equals("CrowdPlanning")) as FeatureLayer;

                //CrowdLayer.Search(null)
                ArcGIS.Core.Data.Table CrowdTable = CrowdLayer.GetTable();
                QueryFilter QF = new QueryFilter
                {
                    WhereClause = "LOW > 0"
                };

                RowCursor CrowdRow = CrowdTable.Search(QF, false);

                long lowValue;
                long mediumValue;
                long highValue;


                while (CrowdRow.MoveNext())
                {
                    lowValue = 0;
                    mediumValue = 0;
                    highValue = 0;
                    using (Row currentRow = CrowdRow.Current)
                    {
                        lowValue = Convert.ToInt32(currentRow["Low"]);
                        TotalValueLow = TotalValueLow + lowValue;

                        mediumValue = Convert.ToInt32(currentRow["Medium"]);
                        TotalValueMedium = TotalValueMedium + mediumValue;

                        highValue = Convert.ToInt32(currentRow["High"]);
                        TotalValueHigh = TotalValueHigh + highValue;

                        targetSettingValue = Convert.ToInt32(currentRow["TargetSetting"]);
                        lowSettingValue = Convert.ToDouble(currentRow["LowSetting"]);
                        mediumSettingValue = Convert.ToDouble(currentRow["MediumSetting"]);
                        highSettingValue = Convert.ToDouble(currentRow["HighSetting"]);

                    }

                }

            });

            // Assign total values
            TotalHigh = TotalValueHigh;
            TotalMedium = TotalValueMedium;
            TotalLow = TotalValueLow;
            // Assign setting values
            TargetSetting = targetSettingValue;
            LowSetting = lowSettingValue;
            MediumSetting = mediumSettingValue;
            HighSetting = highSettingValue;


            // UPDATE PIE CHARTS
            // Chart 1 - High Estimate
            if (TotalValueHigh > TargetSetting)
            {
                targetRemaining = 0;
                _highBackground = "#9BBB59";
                NotifyPropertyChanged(() => ColorBrush);

            }
            else
            {
                targetRemaining = (TargetSetting - TotalValueHigh);
                _highBackground = textboxBackground;
                NotifyPropertyChanged(() => ColorBrush);
            }
            KeyValuePair<string, int>[] myKeyValuePairHigh = new KeyValuePair<string, int>[]
                    {
                    new KeyValuePair<string,int>("High Estimate Allocated", Convert.ToInt32(TotalValueHigh)),
                    new KeyValuePair<string,int>("Remaining to Estimate", Convert.ToInt32(targetRemaining))
                    };
            PieChartResult = myKeyValuePairHigh;
            NotifyPropertyChanged(() => PieChartResult);

            // Chart 2 - Medium Estimate
            if (TotalValueMedium > TargetSetting)
            {
                targetRemaining = 0;
                _mediumBackground = "#9BBB59";
                NotifyPropertyChanged(() => ColorBrushMedium);
            }
            else
            {
                targetRemaining = (TargetSetting - TotalValueMedium);
                _mediumBackground = textboxBackground;
                NotifyPropertyChanged(() => ColorBrushMedium);
            }

            KeyValuePair<string, int>[] myKeyValuePairMedium = new KeyValuePair<string, int>[]
                    {
                    new KeyValuePair<string,int>("Medium Estimate Allocated", Convert.ToInt32(TotalValueMedium)),
                    new KeyValuePair<string,int>("Remaining to Estimate", Convert.ToInt32(targetRemaining))
                    };
            PieChartResultMedium = myKeyValuePairMedium;
            NotifyPropertyChanged(() => PieChartResultMedium);

            // Chart 3 - Low Estimate
            if (TotalValueLow > TargetSetting)
            {
                targetRemaining = 0;
                _lowBackground = "#9BBB59";
                NotifyPropertyChanged(() => ColorBrushLow);
            }
            else
            {
                targetRemaining = (TargetSetting - TotalValueLow);
                _lowBackground = textboxBackground;
                NotifyPropertyChanged(() => ColorBrushLow);
            }

            KeyValuePair<string, int>[] myKeyValuePairLow = new KeyValuePair<string, int>[]
                    {
                    new KeyValuePair<string,int>("Low Estimate Allocated", Convert.ToInt32(TotalValueLow)),
                    new KeyValuePair<string,int>("Remaining to Estimate", Convert.ToInt32(targetRemaining))
                    };
            PieChartResultLow = myKeyValuePairLow;
            NotifyPropertyChanged(() => PieChartResultLow);

            //end of GetTotalValues
        }

        #endregion


    }

}
