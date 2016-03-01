using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;

namespace BasicMapTool {
    internal class BasicEmbeddableControlViewModel : EmbeddableControl {
        public BasicEmbeddableControlViewModel(XElement options) : base(options) { }

        /// <summary>
        /// Text shown in the control.
        /// </summary>
        private string _text = "Embeddable Control";
        public string Text {
            get { return _text; }
            set {
                SetProperty(ref _text, value, () => Text);
            }
        }
    }
}
