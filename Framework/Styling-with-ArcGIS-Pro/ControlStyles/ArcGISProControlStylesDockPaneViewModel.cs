//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace ControlStyles
{
    internal class ArcGISProControlStylesDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "ControlStyles_ArcGISProControlStylesDockPane";

        protected ArcGISProControlStylesDockPaneViewModel() 
        {
            _controlTypes = null;
            LoadControls();
            SelectedControl = _controlTypes[0];
            _copyXamlCmd = new RelayCommand(() => Utils.CopyStringToClipboard(ControlXaml));
            _copyDictionaryCmd = new RelayCommand(() => Utils.CopyStringToClipboard(ReferenceDictionary));
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }
        #region Properties
        
        private string _heading = "ArcGIS Pro Control Styles";
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
        private string _controlXaml;
        public string ControlXaml
        {
            get { return _controlXaml; }
            set
            {
                SetProperty(ref _controlXaml, value, () => ControlXaml);
            }
        }

        private ObservableCollection<string> _controlTypes;

        /// <summary>
        /// List of controls to view the Styles for
        /// </summary>
        public IList<string> ControlTypes
        {
            get { return _controlTypes; }
        }

        private string _selectedControl;
        private StyleViewModelBase _vm = null;

        /// <summary>
        /// The Selected Control.
        /// </summary>
        public string SelectedControl
        {
            get { return _selectedControl; }
            set
            {
                _selectedControl = value;
                SetProperty(ref _selectedControl, value, () => SelectedControl);

                //cleanup first
                if (_vm != null)
                {
                    _vm.StyleXaml = "";
                    _vm.PropertyChanged -= vm_PropertyChanged;
                    _vm = null;
                }
                switch (value)
                {
                    case "Button":
                        _vm = new ButtonStyleViewModel();
                        break;
                    case "CheckBox":
                        _vm = new CheckBoxStyleViewModel();
                        break;
                    case "DataGrid":
                        _vm = new DataGridStyleViewModel();
                        break;
                    case "Expanders":
                        _vm = new ExpanderStyleViewModel();
                        break;
                    case "ListBox":
                        _vm = new ListBoxStyleViewModel();
                        break;
                    case "RadioButton":
                        _vm = new RadioButtonStyleViewModel();
                        break;
                   
                }
                UserControl view = null;
                if (_vm != null)
                {
                    view = Locate<UserControl>(_vm);
                    _vm.PropertyChanged += vm_PropertyChanged;
                    _vm.Initialize();
                }
                if (view != null)
                    view.DataContext = _vm;

                EmbeddableView = view;

            }
        }

        private UserControl _embeddableView = null;
        public UserControl EmbeddableView
        {
            get { return _embeddableView; }
            internal set { SetProperty(ref _embeddableView, value, () => EmbeddableView); }
        }


        #endregion

        private void LoadControls()
        {
            if (_controlTypes == null)
            {
                _controlTypes = new ObservableCollection<string>();
                foreach (string ctrl in ControlNames)
                {
                    _controlTypes.Add(ctrl);
                }
                
                this.NotifyPropertyChanged(new PropertyChangedEventArgs("ControlTypes"));
            }
        }

        private static readonly string[] ControlNames = { "Button",                                             
                                             "CheckBox","DataGrid", "Expanders","ListBox","RadioButton"
                                             };

        private string _refDict;
        public string ReferenceDictionary
        {
            get 
            {
                _refDict = string.Format("<ResourceDictionary.MergedDictionaries><extensions:DesignOnlyResourceDictionary Source=\"pack://application:,,,/ArcGIS.Desktop.Framework;component\\Themes\\Default.xaml\"/></ResourceDictionary.MergedDictionaries>");
                return _refDict;       
            }

            set
            {
                SetProperty(ref _refDict, value, () => ReferenceDictionary);
            }
        }

        void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StyleXaml")
            {
               // StyleViewModelBase vm = EmbeddableView.DataContext as StyleViewModelBase;
                if (_vm != null)
                    ControlXaml = _vm.StyleXaml;
            }
        }

        public static T Locate<T>(ViewModelBase viewModel) where T : FrameworkElement
        {
            if (viewModel == null)
                return null;
            var type = LocateType(viewModel);
            if (type == null)
                return null;

            var view = Activator.CreateInstance(type) as T;
            if (view != null)
                view.DataContext = viewModel;
            return view;
        }

        public static Type LocateType(ViewModelBase viewModel)
        {
            var type = viewModel.GetType();
            var fullNameStem = StripTrailingText(type.FullName, "ViewModel");
            return type.Assembly.GetType(fullNameStem + "View");
        }
        /// <summary>
        /// strips the pattern from the trailing end of given text, which must be at the trailing end of the text.
        /// This is needed to avoid problems if we ever create a namespace that includes "ViewModel"
        /// </summary>
        private static string StripTrailingText(string text, string pattern)
        {
            // ReSharper disable StringLastIndexOfIsCultureSpecific.1
            var index = text.LastIndexOf(pattern);
            // ReSharper restore StringLastIndexOfIsCultureSpecific.1
            if (index < 0 || index != (text.Length - pattern.Length))
                return text;
            return text.Substring(0, index);
        }
        private RelayCommand _copyXamlCmd;
        private RelayCommand _copyDictionaryCmd;

        /// <summary>
        /// Represents the Copy to Clipboard command
        /// </summary>
        public ICommand CopyControlXamlCmd
        {
            get { return _copyXamlCmd; }
        }

        /// <summary>
        /// Represents the Copy to Clipboard command
        /// </summary>
        public ICommand CopyDictionaryCmd
        {
            get { return _copyDictionaryCmd; }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class ArcGISProControlStylesDockPane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            ArcGISProControlStylesDockPaneViewModel.Show();
        }
    }
}
