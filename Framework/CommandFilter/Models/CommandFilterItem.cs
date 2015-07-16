using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CommandFilter.Models {
    class CommandFilterItem : INotifyPropertyChanged {
        private int _count = 0;

        public string Id { get; set; }
        public string Caption { get; set; }
        public string Tooltip { get; set; }

        /// <summary>
        /// Gets and sets the number of times a command has been clicked during
        /// filtering.
        /// </summary>
        public int ClickCount {
            get {
                return _count;
            }
            set {
                _count = value;
                OnPropertyChanged();
            }
        }


        public BitmapImage Thumbnail { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() {
            return Caption;
        }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
