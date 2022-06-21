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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace WorkingWithQueryDefinitionFilters
{
    public class DefinitionFilterItem : PropertyChangedBase
    {
        private string _queryExpression;
        private string _expressionName;
        private MapMember _mapMember;
        private DefinitionQuery _definitionFilter;
        private bool _isActiveFilter;

        public DefinitionFilterItem(MapMember mapMember, DefinitionQuery definitionQuery)
        {
            //if (definitionFilter == null) return;
            _mapMember = mapMember;
            _queryExpression = definitionQuery?.WhereClause;
            _expressionName = definitionQuery?.Name;
            _definitionFilter = definitionQuery;
            var queryBuilderControlProps = new QueryBuilderControlProperties
            {
                Expression = CurrentDefinitionQuery?.WhereClause,
                EditClauseMode = true,
                MapMember = ItemMapMember,
                AutoValidate = true
            };
            ControlProperties = queryBuilderControlProps;
            _isActiveFilter = IsDefinitionFilterActive();
        }

        /// <summary>
        /// The Expression in the Query Definition to use for binding in the Data Template
        /// </summary>
        public string QueryExpression
        {
            get { return _queryExpression; }
            set
            {
                SetProperty(ref _queryExpression, value, () => QueryExpression);
            }
        }
        /// <summary>
        /// The name of the Expression to use for binding in the Data template
        /// </summary>
        public string ExpressionName
        {
            get { return _expressionName; }
            set
            {
                SetProperty(ref _expressionName, value, () => ExpressionName);
            }
        }

        public MapMember ItemMapMember
        {
            get { return _mapMember; }
            set { SetProperty(ref _mapMember, value, () => ItemMapMember); }
        }

        public bool IsActiveFilter
        {
            get { return _isActiveFilter; }
            set { SetProperty(ref _isActiveFilter, value, () => IsActiveFilter); }
        }

        private ICommand _editExpressionCommand;
        public ICommand EditExpressionCommand
        {
            get
            {
                _editExpressionCommand = new RelayCommand(() => EditExpression(), () => { return !(string.IsNullOrEmpty(CurrentDefinitionQuery.WhereClause)); });
                return _editExpressionCommand;
            }
        }

        private ICommand _deleteFilterCommand;
        public ICommand DeleteFilterCommand
        {
            get
            {
                _deleteFilterCommand = new RelayCommand(() => DeleteFilter());
                return _deleteFilterCommand;
            }
        }

        private ICommand _activateFilterCommand;
        public ICommand ActivateFilterCommand
        {
            get
            {
                _activateFilterCommand = new RelayCommand(() => ActivateFilter());
                return _activateFilterCommand;
            }
        }
        private QueryBuilderControlProperties _controlProps;
        public QueryBuilderControlProperties ControlProperties
        {
            get { return _controlProps; }
            set { SetProperty(ref _controlProps, value, () => ControlProperties); }
        }

        public DefinitionQuery CurrentDefinitionQuery
        {
            get { return _definitionFilter; }
            set {
                SetProperty(ref _definitionFilter, value, () => CurrentDefinitionQuery);
            }
        }
        private void EditExpression()
        {
            Module1.Current.DefFilterVM.SelectedDefinitionFilter = this;
            var querybuilderwindow = new QueryBuilderWindow(this, ControlProperties);
            querybuilderwindow.Owner = FrameworkApplication.Current.MainWindow;
            querybuilderwindow.Closed += (o, e) => { querybuilderwindow = null; };
            querybuilderwindow.ShowDialog();
        }

        private void DeleteFilter()
        {
            if (this != null)
            {
                if (ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Do you wish to remove {ExpressionName} expression?", $"Remove {ExpressionName} Definition Query", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                {
                    //Module1.Current.DefFilterVM.DefinitionFilters.Remove(this);

                    if (_mapMember is BasicFeatureLayer)
                    {
                        var selectedMapMemberAsLayer = _mapMember as BasicFeatureLayer;
                        int indexOfQueryToDelete =  selectedMapMemberAsLayer.DefinitionQueries.ToList().FindIndex(a => a.WhereClause == QueryExpression);
                        QueuedTask.Run(() => selectedMapMemberAsLayer.RemoveDefinitionQuery(indexOfQueryToDelete));
                    }
                    if (_mapMember is StandaloneTable)
                    {
                        var selectedMapMemberAsTable = _mapMember as StandaloneTable;
                        int indexOfQueryToDelete = selectedMapMemberAsTable.DefinitionQueries.ToList().FindIndex(a => a.WhereClause == QueryExpression);
                        QueuedTask.Run(() => selectedMapMemberAsTable.RemoveDefinitionQuery(indexOfQueryToDelete));
                    }
                }
            }               
        }

        private void ActivateFilter()
        {
            if (this != null)
            {
                Module1.Current.ActiveFilterExists = false;
                //Apply these filters to the Selected Map Member
                if (_mapMember is BasicFeatureLayer)
                {
                    var selectedMapMemberAsLayer = _mapMember as BasicFeatureLayer;
                    QueuedTask.Run(() => selectedMapMemberAsLayer.SetActiveDefinitionQuery(this.CurrentDefinitionQuery.Name));
                }
                if (_mapMember is StandaloneTable)
                {
                    var selectedMapMemberAsTable = _mapMember as StandaloneTable;
                    QueuedTask.Run(() => selectedMapMemberAsTable.SetActiveDefinitionQuery(this.CurrentDefinitionQuery.Name));
                }
            }
        }

        private bool IsDefinitionFilterActive()
        {
            if ((Module1.Current.ActiveFilterExists)) //multiple filters with same expression and name can exist.
                return false;
            bool isActiveFilter;
            string activeQuery = null;
            if (this._mapMember == null) return false;

            if (_mapMember is BasicFeatureLayer)
            {
                if ((this._mapMember as BasicFeatureLayer).ActiveDefinitionQuery == null) return false;
                activeQuery = (this._mapMember as BasicFeatureLayer).ActiveDefinitionQuery.Name;
                
            }
            if (this._mapMember is StandaloneTable)
            {
                if ((this._mapMember as StandaloneTable).ActiveDefinitionQuery == null) return false;
               activeQuery = (this._mapMember as StandaloneTable).ActiveDefinitionQuery.Name;
            }
            if (activeQuery == null) return false;
            isActiveFilter = ( (activeQuery == this.CurrentDefinitionQuery?.Name) && (activeQuery == this.CurrentDefinitionQuery?.WhereClause) )? true : false;
            if (isActiveFilter)
                Module1.Current.ActiveFilterExists = true; //set this to true for the first pass
            return isActiveFilter;
        }
    }
}
