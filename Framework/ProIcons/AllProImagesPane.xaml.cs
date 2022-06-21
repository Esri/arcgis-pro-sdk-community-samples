/*

   Copyright 2020 Esri

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
using System.Windows.Controls;
using System.Windows.Input;


namespace ProIcons
{
  /// <summary>
  /// Interaction logic for AllProImagesPaneView.xaml
  /// </summary>
  internal partial class AllProImagesPaneView : UserControl
  {
    private string _currentIconName;

    public AllProImagesPaneView()
    {
      InitializeComponent();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
      {
        AllProImagesPaneViewModel vm = this.DataContext as AllProImagesPaneViewModel;
        vm.InvokeSearchCommand();
      }
    }

    private void Image_ToolTipOpening(object sender, ToolTipEventArgs e)
    {
      GetImageName(sender as Image);
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      _currentIconName = GetImageName(sender as Image);
      System.Windows.Clipboard.SetText(_currentIconName);
      if (showPack.IsChecked.HasValue && showPack.IsChecked.Value == true)
      {
        string pack = @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/";
        IconText.Text = pack + _currentIconName + ".png";
      }
      else
      {
        IconText.Text = _currentIconName;
      }

    }

    private string GetImageName(Image img)
    {
      if (img == null || img.DataContext == null)
        return string.Empty;
      ProImage proImage = img.DataContext as ProImage;
      img.ToolTip = proImage.Name;
      return proImage.Name;
    }

    private void showPack_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      if (showPack.IsChecked.HasValue && showPack.IsChecked.Value == true)
      {
        if (!string.IsNullOrEmpty(IconText.Text))
        {
          string pack = @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/";
          IconText.Text = pack + _currentIconName + ".png";
        }
        IconText.Width = 550;
      }
      else
      {
        if (!string.IsNullOrEmpty(IconText.Text))
          IconText.Text = _currentIconName;
        IconText.Width = 275;
      }
    }
  }
}
