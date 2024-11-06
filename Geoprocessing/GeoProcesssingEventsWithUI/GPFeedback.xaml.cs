/*

   Copyright 2023 Esri

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


namespace GeoProcesssingEventsWithUI
{
  /// <summary>  
  /// Interaction logic for GPFeedbackView.xaml  
  /// </summary>  
  public partial class GPFeedbackView : UserControl
  {
    /// <summary>  
    /// Initializes a new instance of the <see cref="GPFeedbackView"/> class.  
    /// </summary>  
    public GPFeedbackView()
    {
      InitializeComponent();
      TxtGPStatus.TextChanged += TxtGPStatus_TextChanged;
    }

    private void TxtGPStatus_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (sender is TextBox tb)
      {
        tb.CaretIndex = tb.Text.Length;
        tb.ScrollToEnd();
      }
    }
  }
}
