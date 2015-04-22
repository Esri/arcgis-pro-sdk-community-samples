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
            _colorKeyXAML = string.Format("\"{{StaticResource {0}}}\"", _name);
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
