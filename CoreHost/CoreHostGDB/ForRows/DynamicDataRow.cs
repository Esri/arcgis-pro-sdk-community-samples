//Copyright 2015 Esri

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
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreHostGDB.ForRows {
    /// <summary>
    /// Inherits from <see cref="System.Dynamic.DynamicObject"/>. Implements a class
    /// that dynamically changes its properties at runtime. The set of properties is changed
    /// to match the columns of the selected GDB table.
    /// </summary>
    /// <remarks>This class is bindable</remarks>
    public class DynamicDataRow : DynamicObject, INotifyPropertyChanged {

        readonly IDictionary<string, object> data;

        public DynamicDataRow() {
            data = new Dictionary<string, object>();
        }

        public DynamicDataRow(IDictionary<string, object> source) {
            data = source;
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return data.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = this[binder.Name];

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            this[binder.Name] = value;

            return true;
        }

        public object this[string columnName] {
            get {
                if (data.ContainsKey(columnName)) {
                    return data[columnName];
                }
                return null;
            }
            set {
                if (!data.ContainsKey(columnName)) {
                    data.Add(columnName, value);

                    OnPropertyChanged(columnName);
                }
                else {
                    if (data[columnName] != value) {
                        data[columnName] = value;

                        OnPropertyChanged(columnName);
                    }
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

    }
}
