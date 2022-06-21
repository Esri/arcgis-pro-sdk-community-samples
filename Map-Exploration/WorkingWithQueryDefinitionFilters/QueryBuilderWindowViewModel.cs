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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WorkingWithQueryDefinitionFilters
{
    public class QueryBuilderWindowViewModel : PropertyChangedBase
    {
        public QueryBuilderWindowViewModel(QueryBuilderControlProperties queryBuilderControlProperties)
        {
            MapMemberName = queryBuilderControlProperties.MapMember.Name;
            Expression = queryBuilderControlProperties.Expression;
            ControlProps = queryBuilderControlProperties;
            _origExpression = Expression;
        }
        public QueryBuilderWindow ThisQueryBuilderWindow;

        private DefinitionFilterItem _thisDefinitionFilterItem;
        public DefinitionFilterItem ThisDefinitionFilterItem
        {
            get { return _thisDefinitionFilterItem; }
            set { SetProperty(ref _thisDefinitionFilterItem, value, () => ThisDefinitionFilterItem);
            }
        }
        private string _mapMemberName;
        public string MapMemberName
        {
            get { return _mapMemberName; }
            set {
                SetProperty(ref _mapMemberName, value, () => MapMemberName);
            }
        }
        private string _expression;
        public string Expression
        {
            get { return _expression; }
            set {
                //SetProperty(ref _expression, value, () => Expression);
                _expression = value;
            }
        }
        private QueryBuilderControlProperties _controlProps = null;
        private string _origExpression;

        public QueryBuilderControlProperties ControlProps
        {
            get { return _controlProps; }
            set {
                SetProperty(ref _controlProps, value);
                Expression = ControlProps.Expression;
                _origExpression = ControlProps.Expression;

            }
        }
        /// <summary>
        /// Has the current expression been altered?  
        /// </summary>
        /// <returns>true if the current expression has been altered. False otherwise.</returns>
        private bool CanSaveChanges()
        {
            string newExpression = Expression ?? "";
            var isNewExpession = string.Compare(_origExpression, newExpression);
            if (isNewExpession == 0)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Gets the Apply command to write query definition to mapMember.
        /// </summary>
        private RelayCommand _applyCommand;
        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                    _applyCommand = new RelayCommand(() => SaveChanges(), CanSaveChanges);

                return _applyCommand;
            }
        }
        private void SaveChanges()
        {
            
            // get the new expression
            string newExpression = Expression ?? "";

            // is it different?
            if (string.Compare(_origExpression, newExpression) != 0)
            {
                if (ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Expression has changed. Do you wish to save it?", "Definition Query", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                {
                    // update internal var
                    _origExpression = newExpression;
                    ControlProps.Expression = newExpression;
                    ThisDefinitionFilterItem.QueryExpression = ControlProps.Expression;
                    ThisDefinitionFilterItem.CurrentDefinitionQuery = new DefinitionQuery {WhereClause = ControlProps.Expression};
                    Module1.Current.DefFilterVM.SelectedDefinitionFilter = ThisDefinitionFilterItem;
                    ApplyFilterChangesToLayer(ThisDefinitionFilterItem.ItemMapMember);

                    ThisQueryBuilderWindow.Close();
                }
            }          
        }

        private void ApplyFilterChangesToLayer(MapMember mapMember)
        {
            QueuedTask.Run(() => {
                //Apply these filters to the Selected Map Member
                if (mapMember is BasicFeatureLayer)
                {
                    var selectedMapMemberAsLayer = mapMember as BasicFeatureLayer;
                    selectedMapMemberAsLayer.InsertDefinitionQuery(new DefinitionQuery { WhereClause = ThisDefinitionFilterItem.QueryExpression});
              }
                if (mapMember is StandaloneTable)
                {
                    var selectedMapMemberAsTable = mapMember as StandaloneTable;
                    selectedMapMemberAsTable.InsertDefinitionQuery(new DefinitionQuery { WhereClause = ThisDefinitionFilterItem.QueryExpression });
                }
            });
        }
    }
}

