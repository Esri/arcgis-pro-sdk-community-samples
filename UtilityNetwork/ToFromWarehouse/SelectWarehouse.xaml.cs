using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToFromWarehouse
{
  //   Copyright 2019 Esri
  //   Licensed under the Apache License, Version 2.0 (the "License");
  //   you may not use this file except in compliance with the License.
  //   You may obtain a copy of the License at

  //       https://www.apache.org/licenses/LICENSE-2.0

  //   Unless required by applicable law or agreed to in writing, software
  //   distributed under the License is distributed on an "AS IS" BASIS,
  //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  //   See the License for the specific language governing permissions and
  //   limitations under the License.

  /// <summary>
  /// Interaction logic for SelectWarehouse.xaml
  /// </summary>
  public partial class SelectWarehouse : ArcGIS.Desktop.Framework.Controls.ProWindow
  {
    private ObservableCollection<string> _selectedWarehouse;

    public SelectWarehouse(Dictionary<string, string> warehouseNames)
    {
      _selectedWarehouse = new ObservableCollection<string>();
      InitializeComponent();
      cboWarehouseNames.ItemsSource = warehouseNames.Keys;
      DataContext = this;
    }


    private RelayCommand _moveCommand = null;
    public ICommand MoveCommand => _moveCommand ??= new RelayCommand(() => Done());

    private void Done()
    {
      ObservableCollection<string> result = new()
            {
              cboWarehouseNames.SelectedItem.ToString()
            };
      ChosenWarehouse = result;
      Close();
    }

    private void CheckSelection()
    {
      if (cboWarehouseNames.SelectedIndex != -1)
      {
        btnDone.IsEnabled = true;
      }
    }

    private void cboWarehouseNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CheckSelection();
    }


    public ObservableCollection<string> ChosenWarehouse
    {
      get => _selectedWarehouse;
      set => _selectedWarehouse = value;
    }
  }
}
