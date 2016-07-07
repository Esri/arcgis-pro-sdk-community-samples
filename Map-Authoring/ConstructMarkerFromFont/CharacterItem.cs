using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ConstructMarkerFromFont
{
    internal class CharacterItem
    {

        public CharacterItem(char character)
        {
            _character = character;
        }

        private char _character;

        public char Character
        {
            get { return _character; }
            set { _character = value; }
        }       

    }
}
