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
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;

namespace ConfigWithStartWizard.UI.StartPages {
    class StartPageViewModel : INotifyPropertyChanged {

        private List<StartPageViewModelBase> _pages = new List<StartPageViewModelBase>();
        private StartPageViewModelBase _currentPage = null;
        private bool _executeQuery = false;
        private bool _previousEnabled = false;
        private SignOnStatusViewModel _ssvm = null;
        private DispatcherTimer _timer = null;
        private double _progressValue = 1;
        private double _maxProgressValue = 100;

        private ICommand _next;
        private ICommand _previous;
        private ICommand _about;
        private int _index = 0;

        internal StartPageViewModel() {
            Initialize();
        }

        private void Initialize() {
            _pages.Add(new OutofBoxStartPageViewModel());
            _pages.Add(new StockStartPageViewModel());
            _pages.Add(new OnlineItemStartPageViewModel("Projects"));
            _pages.Add(new OnlineItemStartPageViewModel("Templates"));
            //_pages.Add(new OnlineItemStartPageViewModel("Map Packages"));
            _pages.Add(new OnlineItemStartPageViewModel("Web Maps"));
            _pages.Add(new OnlineItemStartPageViewModel("Layers"));
            _pages.Add(new CaliforniaStartPageViewModel());
            _currentPage = _pages[0];
            _index = 0;
            _ssvm = new SignOnStatusViewModel();

            //init timer
            _timer = new DispatcherTimer() {
                Interval = TimeSpan.FromMilliseconds(25d),
                IsEnabled = false
            };
            _timer.Tick += (o, e) => {
                //update the progress bar
                _progressValue += 1.0;
                if (_progressValue > _maxProgressValue)
                    _progressValue = 1.0;
                FrameworkApplication.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("ProgressValue"));
            };
        }

        public StartPageViewModelBase Page => _currentPage;

        public string Title => _currentPage.Title;

        public bool IsExecutingQuery => _executeQuery;

        public SignOnStatusViewModel SignOnStatusViewModel => _ssvm;

        public string ApplicationName => FrameworkApplication.Name;

        public double ProgressValue => _progressValue;

        public double MaxProgressValue => _maxProgressValue;


        #region Page Handling

        bool MoveToNextPage() {
            _index++;
            if (_index < _pages.Count) {
                _currentPage = _pages[_index];
            }
            else {
                _index = 0;
                _currentPage = _pages[_index];
            }
            NotifyPropertyChanged("Page");
            NotifyPropertyChanged("Title");
            _executeQuery = true;
            NotifyPropertyChanged("IsExecutingQuery");
            _timer.Start();
            _currentPage.InitializeAsync().ContinueWith(t => {
                if (_timer.IsEnabled) _timer.Stop();
                _executeQuery = false;
                NotifyPropertyChanged("IsExecutingQuery");
            });
            return _index != 0;
        }

        bool MoveToPreviousPage() {
            _index--;
            if (_index >= 0) {
                _currentPage = _pages[_index];
            }
            else {
                _index = _pages.Count - 1;
                _currentPage = _pages[_index];
            }
            NotifyPropertyChanged("Page");
            NotifyPropertyChanged("Title");
            return _index != 0;
        }

        public ICommand NextCommand
        {
            get
            {
                if (_next == null)
                    _next = new RelayCommand(() => PreviousEnabled = MoveToNextPage());
                return _next;
            }
        }

        public bool PreviousEnabled
        {
            get
            {
                return _previousEnabled;
            }
            set
            {
                _previousEnabled = value;
                NotifyPropertyChanged("PreviousEnabled");
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                if (_previous == null)
                    _previous = new RelayCommand(() => PreviousEnabled = MoveToPreviousPage());
                return _previous;
            }
        }

        public ICommand AboutArcGISProCommand
        {
            get
            {
                if (_about == null)
                    _about = new RelayCommand(() => FrameworkApplication.OpenBackstage());
                return _about;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { }; 

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

    }
    
}
