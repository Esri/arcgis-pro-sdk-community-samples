//   Copyright 2018 Esri
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
using System.ComponentModel;

namespace Localization
{
    /// <summary>
    /// Sample data is stored in a 'Sample record'
    /// </summary>
    public class SampleRecord : INotifyPropertyChanged
    {
        /// <summary>
        /// returns some sample data
        /// </summary>
        public static IList<SampleRecord> SampleRecords
        {
            get
            {
                return new List<SampleRecord>()
                {
                    new SampleRecord()
                    {
                        Name = "Joe",
                        BirthDate = new DateTime(1990, 2, 1),
                        PaymentAmount = 1000,
                        Location = "Los Angeles, CA, USA"
                    },
                    new SampleRecord()
                    {
                        Name = "Gunter",
                        BirthDate = new DateTime(1990, 5, 1),
                        PaymentAmount = 2000,
                        Location = "New York, NY, USA"
                    },
                    new SampleRecord()
                    {
                        Name = "Günter",
                        BirthDate = new DateTime(1990, 11, 1),
                        PaymentAmount = 1500,
                        Location = "Stuttgart, Germany"
                    }
                };
            }
        }

        private string _name;
        private DateTime _birthDate;
        private int _paymentAmount;
        private string _image;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }
        public DateTime BirthDate
        {
            get { return _birthDate; }
            set
            {
                _birthDate = value;
                NotifyPropertyChanged("BirthDate");
            }
        }
        public int PaymentAmount
        {
            get { return _paymentAmount; }
            set
            {
                _paymentAmount = value;
                NotifyPropertyChanged("PaymentAmount");
            }
        }
        public string Location
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyPropertyChanged("Location");
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Helpers
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
