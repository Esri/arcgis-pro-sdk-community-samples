/*

   Copyright 2019 Esri

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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateReport
{
    /// <summary>
    /// Represents the report field
    /// </summary>
    public class ReportField : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _displayName;
        private string _name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
                OnFieldSelectionChanged(new FieldSelectionChangedEventArgs {ChangedReportField = this });
            }
        }
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyPropertyChanged("DisplayName");
            }
        }

        public string Name { get => _name; set { _name = value; NotifyPropertyChanged("Name"); } }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Helpers
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected virtual void OnFieldSelectionChanged(FieldSelectionChangedEventArgs e)
        {
            FieldSelectionChanged?.Invoke(this, e);
        }

        public event EventHandler<FieldSelectionChangedEventArgs> FieldSelectionChanged;
    }

    public class FieldSelectionChangedEventArgs : EventArgs
    {
        public ReportField ChangedReportField { get; set; }

    }
}

