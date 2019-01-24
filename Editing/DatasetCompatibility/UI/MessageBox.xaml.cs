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
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace DatasetCompatibility.UI
{
  /// <summary>
  /// Interaction logic for MessageBox.xaml
  /// </summary>
  public partial class MessageBox : Window, INotifyPropertyChanged
  {
    private string message;
    public string Message
    {
      get { return message; }
      set { message = value; RaisePropertyChanged("Message"); }
    }

    public MessageBox()
    {
      InitializeComponent();
      DataContext = this;
    }

    public static void Show(string message, string title = "")
    {
      var mbox = new MessageBox();
      mbox.Title = title;
      mbox.Message = message;
      mbox.ShowDialog();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    }
  }
}
