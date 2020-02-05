/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Mapping.Controls;
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

namespace WorkingWithQueryDefinitionFilters
{
    /// <summary>
    /// Interaction logic for QueryBuilderWindow.xaml
    /// </summary>
    public partial class QueryBuilderWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public QueryBuilderWindowViewModel QueryBuilderWindowVM = null;
        public string QueryExpressionVM;
        public QueryBuilderWindow(DefinitionFilterItem defItem, QueryBuilderControlProperties queryBuilderControlProperties)
        {            
            InitializeComponent();
            // trap for Expression changes
            this.QueryBuilderControl.ExpressionChanged += QueryBuilderControl_ExpressionChanged;
            QueryBuilderWindowVM = new QueryBuilderWindowViewModel(queryBuilderControlProperties);
            // set the datacontext to our ViewModel
            this.DataContext = QueryBuilderWindowVM;
            QueryBuilderWindowVM.ThisQueryBuilderWindow = this;
            QueryBuilderWindowVM.ThisDefinitionFilterItem = defItem;
        }

        // Update the ViewModel's Expression when the expression changes in the QueryBuilderControl.
        private void QueryBuilderControl_ExpressionChanged(object sender, ArcGIS.Desktop.Mapping.Controls.ExpressionChangedEventArgs args)
        {
            if (QueryBuilderWindowVM != null)
            {
                QueryBuilderWindowVM.Expression = args.Expression;

                // if interested in validating via API
                //bool result = await QueryBuilderControl.ValidateExpression(false);
                //var msg = QueryBuilderControl.Message;
            }
        }
    }
}
