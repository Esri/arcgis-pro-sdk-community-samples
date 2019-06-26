// Copyright 2019 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using System;
using System.Collections.Generic;
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


namespace ConstructionToolWithOptions
{
  /// <summary>
  /// Interaction logic for CircleToolOptionsView.xaml
  /// </summary>
  public partial class CircleToolOptionsView : UserControl
  {
    public CircleToolOptionsView()
    {
      InitializeComponent();
    }

    internal CircleToolOptionsViewModel ViewModel => DataContext as CircleToolOptionsViewModel;

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      string decimalPt = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

      string text = (sender as TextBox).Text;

      // no - for negative numbers allowed
      if (text.Contains("-"))
        e.Handled = true;
      // only allow one decimal point
      else if (text.Contains(decimalPt) && decimalPt.Equals(e.Text))
        e.Handled = true;
      else
        e.Handled = !e.Text.Any(x => Char.IsDigit(x) || decimalPt.Equals(e.Text));
    }

    private void TextBox_PastingHandler(object sender, DataObjectPastingEventArgs e)
    {
      if (e.DataObject.GetDataPresent(typeof(string)))
      {
        string text = (string)e.DataObject.GetData(typeof(string));
        if (IsPastedTextNotNumeric(text))
          e.CancelCommand();
      }
      else
        e.CancelCommand();
    }

    public static bool IsPastedTextNotNumeric(string str)
    {
      return IsTextNotNumeric(str, true);
    }

    private static bool IsTextNotNumeric(string str, bool needAll)
    {
      string decimalPt = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
      string groupSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;

      if (str.Equals(decimalPt) || str.Equals(groupSep))
        return false;

      int len = 0;
      foreach (Char c in str)
      {
        if (c == '-' && len > 0)  // allow negative symbol only as the first char.
          return true;

        len++;

        if (c == ' ')
          return true;

        if (Char.IsDigit(c) || c == '-' || c.ToString().Equals(decimalPt) || c.ToString().Equals(groupSep))
        {
          if (needAll)
            continue;
          else
            return false;
        }
        if (needAll)
          return true;
      }

      return !needAll;
    }

    // This routine is only applicable in template properties when multiple templates are selected and you have different values in tool options
    // for each.
    // The user is attempting to edit to set the radius value when different values exist 
    private void OnDifferentLabelPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var vm = ViewModel;
      if (vm == null)
        return;

      // change the flag to show the textBox
      vm.IsDifferentValue = false;
      // initialize text box value
      textBox.Text = "";
      // set Valid flag on VM (dont use IsValid) to disable OK button on template properties dialog
      vm.SetValidIfNotEditingDifferentValues(false);
      e.Handled = true;
    }

    private void textBox_KeyUp(object sender, KeyEventArgs e)
    {
      string text = (sender as TextBox).Text;
      if (double.TryParse(text, out double val))
      {
        // set Valid flag on VM (dont use IsValid) to enable OK button on template properties dialog
        ViewModel.SetValidIfNotEditingDifferentValues(true);
      }
    }
  }
}
