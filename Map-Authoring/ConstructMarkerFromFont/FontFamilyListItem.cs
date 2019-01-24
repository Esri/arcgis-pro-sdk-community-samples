/*

   Copyright 2019 Esri

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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ConstructMarkerFromFont
{
         internal class FontFamilyListItem : TextBlock, IComparable
        {
            private string _displayName;

            public FontFamilyListItem(FontFamily fontFamily)
            {
                _displayName = GetDisplayName(fontFamily);
                _fontFamily = fontFamily;
                //this.FontFamily = fontFamily;
                this.Text = _displayName;
                this.ToolTip = _displayName;

                // In the case of symbol font, apply the default message font to the text so it can be read.
                if (IsSymbolFont(fontFamily))
                {
                    TextRange range = new TextRange(this.ContentStart, this.ContentEnd);
                    range.ApplyPropertyValue(TextBlock.FontFamilyProperty, SystemFonts.MessageFontFamily);
                }
            }

            public override string ToString()
            {
                return _displayName;
            }

            private FontFamily _fontFamily;

            public FontFamily Font => _fontFamily;

            int IComparable.CompareTo(object obj)
            {
                return string.Compare(_displayName, obj.ToString(), true, CultureInfo.CurrentCulture);
            }

            internal static bool IsSymbolFont(FontFamily fontFamily)
            {
                foreach (Typeface typeface in fontFamily.GetTypefaces())
                {
                    GlyphTypeface face;
                    if (typeface.TryGetGlyphTypeface(out face))
                    {
                        return face.Symbol;
                    }
                }
                return false;
            }

            internal static string GetDisplayName(FontFamily family)
            {
                return NameDictionaryHelper.GetDisplayName(family.FamilyNames);
            }
        }
    
}
