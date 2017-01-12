/*

   Copyright 2017 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ControlStyles.Colors
{
    public class ColorEntry : IComparable<ColorEntry>
    {
        public ColorEntry(string EsriName, string Hex)
        {
            _name = EsriName;
            _hexVal = Hex;
            _brush = MakeBrush(_hexVal);
            _colorKeyXAML = string.Format("\"{{DynamicResource {0}}}\"", _name);
        }

        #region properties

        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private string _hexVal = "";
        public string HexValue
        {
            get { return _hexVal; }
        }

        private string _colorKeyXAML = "";

        public string ColorKeyXAML
        {
            get { return _colorKeyXAML; }
        }

        private SolidColorBrush _brush;
        public SolidColorBrush Brush
        {
            get
            {
                return _brush;
            }
        }

        #endregion

        public override string ToString()
        {
            return _name;
        }

        private static SolidColorBrush MakeBrush(string HexString)
        {
            SolidColorBrush myBrush = new SolidColorBrush();
            myBrush = (SolidColorBrush)(new BrushConverter().ConvertFromString(HexString));
            return myBrush;
        }

        private static int LookUpColorScore(string colorName)
        {
            string colrTemp = colorName.ToLower();
            int scoreTemp = 0;
            int counter = 0;
            foreach (string s in ColorNameList)
            {
                if (colrTemp.Contains(s))
                {
                    scoreTemp = ColorNameScore[counter];
                }
                counter++;
            }
            return scoreTemp;        
        }

        private static readonly string[] ColorNameList = new string[] {"gray", "white", "brush","blue", "yellow", "orange", "green", "red", "purple", "brown"};
        private static readonly int[] ColorNameScore = new int[]  {10000, 9000, 8000, 7000, 6000, 5000, 4000, 3000, 2000, 1000};

        public int CompareTo(ColorEntry ce)
        {
            // Sort on Hex value and a score pattern
            int Score = LookUpColorScore(ce.Name);
            int Score2 = LookUpColorScore(this.Name);
            return( (this.Name.CompareTo(ce.Name)) + (Score - Score2));
        }
    }
}
