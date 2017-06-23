using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;


namespace ControlStyles.Backstage
{
    internal class BackstageStylingViewModel : BackstageTab
    {
        /// <summary>
        /// Called when the backstage tab is selected.
        /// </summary>
        protected override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }

        /// <summary>
        /// Called when the backstage tab is unselected.
        /// </summary>
        protected override Task UninitializeAsync()
        {
            return base.UninitializeAsync();
        }

        private string _tabHeading = "Tab Title";
        public string TabHeading
        {
            get
            {
                return _tabHeading;
            }
            set
            {
                SetProperty(ref _tabHeading, value, () => TabHeading);
            }
        }
    }
}
