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
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace SeqNum
{
  /// <summary>
  /// Interaction logic for StartValue.xaml
  /// </summary>
  public partial class StartValue : Window
  {
    //Regex NumEx = new Regex(@"^-?\d*\.?\d*$");
    Regex NumEx = new Regex(@"[0-9]$");
    public string _svalue;

    public StartValue()
    {
      InitializeComponent();
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
      textBox.Text = Module1._startValue;
      textBox.Focus();
      textBox.SelectAll();
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      //only allow numeric input
      if (sender is TextBox)
      {
        string text = (sender as TextBox).Text + e.Text;
        e.Handled = !NumEx.IsMatch(text);
      }
      else
        throw new NotImplementedException("TextBox_PreviewTextInput Can only Handle TextBoxes");
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        _svalue = textBox.Text;
        DialogResult = true;
        this.Close();
      }

      if (e.Key == Key.Escape)
        this.Close();
    }
  }
}
