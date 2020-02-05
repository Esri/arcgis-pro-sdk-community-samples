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
        private CIMDefinitionFilter _definitionFilter;
        private bool _isActiveFilter;

        public DefinitionFilterItem(MapMember mapMember, CIMDefinitionFilter definitionFilter)
        {
            //if (definitionFilter == null) return;
            _mapMember = mapMember;
            _queryExpression = definitionFilter?.DefinitionExpression;
            _expressionName = definitionFilter?.Name;
            _definitionFilter = definitionFilter;
            var queryBuilderControlProps = new QueryBuilderControlProperties
            {
                Expression = DefinitionFilter?.DefinitionExpression,
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
                _editExpressionCommand = new RelayCommand(() => EditExpression(), () => { return !(string.IsNullOrEmpty(DefinitionFilter.DefinitionExpression)); });
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

        public CIMDefinitionFilter DefinitionFilter
        {
            get { return _definitionFilter; }
            set {
                SetProperty(ref _definitionFilter, value, () => DefinitionFilter);
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
                    Module1.Current.DefFilterVM.DefinitionFilters.Remove(this);

                    //Apply these filters to the Selected Map Member
                    if (_mapMember is BasicFeatureLayer)
                    {
                        var selectedMapMemberAsLayer = _mapMember as BasicFeatureLayer;
                        QueuedTask.Run(() => selectedMapMemberAsLayer.RemoveDefinitionFilter(DefinitionFilter.Name));
                    }
                    if (_mapMember is StandaloneTable)
                    {
                        var selectedMapMemberAsTable = _mapMember as StandaloneTable;
                        QueuedTask.Run(() => selectedMapMemberAsTable.RemoveDefinitionFilter(DefinitionFilter.Name));
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
                    QueuedTask.Run(() => selectedMapMemberAsLayer.SetDefinitionFilter(DefinitionFilter));
                }
                if (_mapMember is StandaloneTable)
                {
                    var selectedMapMemberAsTable = _mapMember as StandaloneTable;
                    QueuedTask.Run(() => selectedMapMemberAsTable.SetDefinitionFilter(DefinitionFilter));
                }
            }
        }

        private bool IsDefinitionFilterActive()
        {
            if ((Module1.Current.ActiveFilterExists)) //multiple filters with same expression and name can exist.
                return false;
            bool isActiveFilter;
            CIMDefinitionFilter activeQueryFilter = null;
            if (this._mapMember == null) return false;

            if (_mapMember is BasicFeatureLayer)
            {
                activeQueryFilter = (this._mapMember as BasicFeatureLayer).DefinitionFilter;
                
            }
            if (this._mapMember is StandaloneTable)
            {
                activeQueryFilter = (this._mapMember as StandaloneTable).DefinitionFilter;
            }
            if (activeQueryFilter == null) return false;
            isActiveFilter = ( (activeQueryFilter.Name == this.DefinitionFilter?.Name) && (activeQueryFilter.DefinitionExpression == this.DefinitionFilter?.DefinitionExpression) )? true : false;
            if (isActiveFilter)
                Module1.Current.ActiveFilterExists = true; //set this to true for the first pass
            return isActiveFilter;
        }
    }
}
