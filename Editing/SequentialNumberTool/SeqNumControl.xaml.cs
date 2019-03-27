//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


namespace SeqNum
{
  /// <summary>
  /// Interaction logic for SeqNumControlView.xaml
  /// </summary>
  public partial class SeqNumControlView : UserControl
  {
    public SeqNumControlView()
    {
      InitializeComponent();
    }

    //Regex NumEx = new Regex(@"^-?\d*\.?\d*$");
    Regex NumEx = new Regex(@"[0-9]$");

    private void txtStart_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      if (sender is TextBox)
      {
        string text = (sender as TextBox).Text + e.Text;
        e.Handled = !NumEx.IsMatch(text);
      }
      else
        throw new NotImplementedException("TextBox_PreviewTextInput Can only Handle TextBoxes");
    }

    private void txtIncrement_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      if (sender is TextBox)
      {
        string text = (sender as TextBox).Text + e.Text;
        e.Handled = !NumEx.IsMatch(text);
      }
      else
        throw new NotImplementedException("TextBox_PreviewTextInput Can only Handle TextBoxes");
    }
  }
}
