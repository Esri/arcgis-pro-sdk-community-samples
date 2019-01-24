/*

   Copyright 2018 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CustomCategoriesExample.UI
{
  /// <summary>
  /// ProWindow to show the custom reports (loaded by the Module
  /// using the AcmeCustom_Reports category)
  /// </summary>
  public partial class ReportsWindow : ProWindow, INotifyPropertyChanged
  {

    private bool _isRunningReport = false;
    private static readonly object _lock = new object();
    private ICommand _runCommand = null;
    private ICommand _closeCommand = null;
    private IAcmeCustomReport _selReport = null;

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public ReportsWindow()
    {
      InitializeComponent();
      DataContext = this;
      this.Closing += ReportsWindow_Closing;
    }

    public List<IAcmeCustomReport> CustomReports => Module1.Current.CustomReports;

    public IAcmeCustomReport SelectedReport
    {
      get { return _selReport; }

      set
      {
        if (_selReport == null || _selReport != value)
        {
          _selReport = value;
          NotifyPropertyChanged("Details");
        }
      }
    }

    public string Details
    {
      get
      {
        if (_selReport == null) return "";

        return $"Title: {_selReport.Title}\r\nDetails: {_selReport.Details}";
      }
    }


    public ICommand CloseCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new RelayCommand(() => this.Close());
        }
        return _closeCommand;
      }
      
    }

    public ICommand RunCommand
    {
      get
      {
        if (_runCommand == null)
        {
          _runCommand = new RelayCommand(async () =>
          {
            _isRunningReport = true;
            //Show progress
            var progDialog = new ProgressDialog($"Running {SelectedReport.Title}...");
            progDialog.Show();
            //run the chosen report
            await SelectedReport.RunAsync();
            
            progDialog.Hide();
            _isRunningReport = false;
          }, () => !_isRunningReport && SelectedReport != null);
        }
        return _runCommand;
      }

    }

    private void ReportsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      e.Cancel = _isRunningReport;
    }

    private void NotifyPropertyChanged([CallerMemberName] string propName = "")
    {
      PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
  }
}
