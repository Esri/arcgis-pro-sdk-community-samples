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

namespace ProWindowMVVM
{
  /// <summary>
  /// Interaction logic for ProWindowDialog.xaml
  /// </summary>
  public partial class ProWindowDialog : ArcGIS.Desktop.Framework.Controls.ProWindow
  {
    ProWindowDialogVM ProWindowVM = new ProWindowDialogVM();

    public ProWindowDialog()
    {
      InitializeComponent();
      // TODO: set data context
      this.DataContext = ProWindowVM;
    }
  }
}
