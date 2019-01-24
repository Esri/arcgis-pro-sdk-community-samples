/*

   Copyright 2019 Esri

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


namespace QueryBuilderControl
{
  /// <summary>
  /// Interaction logic for DefinitionQueryDockPaneView.xaml
  /// </summary>
  public partial class DefinitionQueryDockPaneView : UserControl
  {
    public DefinitionQueryDockPaneView()
    {
      InitializeComponent();

      // trap for Expression changes
      this.QueryBuilderControl.ExpressionChanged += QueryBuilderControl_ExpressionChanged;

    }

    private DefinitionQueryDockPaneViewModel ViewModel => DataContext as DefinitionQueryDockPaneViewModel;

    // Update the ViewModel's Expression when the expression changes in the QueryBuilderControl.
    private void QueryBuilderControl_ExpressionChanged(object sender, ArcGIS.Desktop.Mapping.Controls.ExpressionChangedEventArgs args)
    {
      if (ViewModel != null)
      {
        ViewModel.Expression = args.Expression;

        // if interested in validating via API
        //bool result = await QueryBuilderControl.ValidateExpression(false);
        //var msg = QueryBuilderControl.Message;
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (!QueryBuilderControl.IsEditing)
        QueryBuilderControl.ClearExpression();
    }
  }
}
