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

namespace COGOLineFeatures
{
  /// <summary>
  /// Interaction logic for COGOAttributesInput.xaml
  /// </summary>
  public partial class COGOLineInput : ArcGIS.Desktop.Framework.Controls.ProWindow
  {
    public COGOLineInput()
    {
      InitializeComponent();
      Direction.Focus();
    }
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      TextBox textBox = (TextBox)sender;
      textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
      e.Handled = true;
    }
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        Distance.Focus();
        e.Handled = true;
      }
    }
  }
}
