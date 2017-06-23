using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlStyles 
{
    public class Customer : INotifyPropertyChanged
    {
        private string _name;
        private int _id;
        private string _gender;
        private bool _ismvp;

        /// <summary>
        /// returns some sample data
        /// </summary>
        public  IList<Customer> Customers
        {
            get
            {
                return new List<Customer>()
                {
                    new Customer()
                    {
                        Name = "Charlie",
                        ID = 1,
                        Gender = "M",
                        IsMVP = true
                    },
                    new Customer()
                    {
                        Name = "Wolf",
                        ID = 2,
                        Gender = "M",
                        IsMVP = true
                    },
                    new Customer()
                    {
                        Name = "Uma",
                        ID = 3,
                        Gender = "F",
                        IsMVP = true
                    }
                };
            }
        }

        public int ID {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("ID");
            }
        }
        public string Name {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }
        public string Gender {
            get { return _gender; }
            set
            {
                _gender = value;
                NotifyPropertyChanged("Gender");
            }
        }
        public bool IsMVP {
            get { return _ismvp; }
            set
            {
                _ismvp = value;
                NotifyPropertyChanged("IsMVP");
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
