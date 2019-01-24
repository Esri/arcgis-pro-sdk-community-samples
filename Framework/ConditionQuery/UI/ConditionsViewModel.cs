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
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ConditionsQuery.Data;
using ConditionsQuery.Models;

namespace ConditionQuery.UI {
    internal class ConditionsViewModel : DockPane {
        private const string _dockPaneID = "ConditionQuery_UI_Conditions";

        private Condition _selectedCondition = null;
        private EvaluateCondition _conditionLoader = null;
        private ICommand _refresh = null;

        protected ConditionsViewModel() {
            _conditionLoader = new EvaluateCondition();
            BindingOperations.EnableCollectionSynchronization(_conditionLoader.ActiveStates, EvaluateCondition.Lock);
            BindingOperations.EnableCollectionSynchronization(_conditionLoader.EnabledConditions, EvaluateCondition.Lock);
        }

        /// <summary>
        /// Called when the pane is first created to give it the opportunity to initialize itself asynchronously.
        /// </summary>
        protected override Task InitializeAsync() {
            return _conditionLoader.UpdateConditionsAndStateAsync();
        }

        /// <summary>
        /// Gets and sets the selected condition
        /// </summary>
        public Condition SelectedCondition
        {
            get
            {
                return _selectedCondition;
            }
            set
            {
                _selectedCondition = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("SelectedConditionXml");
            }
        }
        /// <summary>
        /// Gets the xml of the selected condition
        /// </summary>
        public string SelectedConditionXml => _selectedCondition != null ? _selectedCondition.Xml : "";

        /// <summary>
        /// Gets the ConditionLoader that will evaluate the current Pro conditions
        /// </summary>
        public EvaluateCondition ConditionLoader => _conditionLoader;

        /// <summary>
        /// Gets the refresh command
        /// </summary>
        public ICommand RefreshCommand
        {
            get
            {
                if (_refresh == null)
                    _refresh = new RelayCommand(() => {
                        _conditionLoader.UpdateConditionsAndStateAsync();
                    });
                return _refresh;
            }
        }

        /// <summary>
        /// Called whenever the DockPane is activated or deactivated.
        /// </summary>
        /// <param name="isActive">Has the Pane been activated or deactivated.</param>
        protected override void OnActivate(bool isActive) {
            FrameworkApplication.State.Activate("condition_query_Visible");
            base.OnActivate(isActive);
        }

        /// <summary>
        /// Called when the DockPane is completely hidden.
        /// </summary>
        /// <remarks>
        /// DockPanes are singletons and pushing their Close button really just hides them.
        /// </remarks>
        protected override void OnHidden() {
            FrameworkApplication.State.Deactivate("condition_query_Visible");
            base.OnHidden();
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Conditions_ShowButton : Button {
        protected override void OnClick() {
            ConditionsViewModel.Show();
        }
    }
}
