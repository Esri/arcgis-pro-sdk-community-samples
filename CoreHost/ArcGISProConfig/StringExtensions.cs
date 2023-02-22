/*

   Copyright 2023 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

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

namespace ArcGISProConfig
{
  public static class StringExtensions
  {
    /// <summary>
    /// Extract a substring between a start marker character and end marker character
    /// </summary>
    /// <param name="input"></param>
    /// <param name="charStartMarker"></param>
    /// <param name="charEndMarker"></param>
    /// <returns>if start/end markers are not found returns empty string</returns>
    public static string SubstringBetweenCharacters(this string input, char charStartMarker, char charEndMarker)
    {
      int posFrom = input.IndexOf(charStartMarker);
      if (posFrom != -1) //if found char
      {
        int posTo = input.IndexOf(charEndMarker, posFrom + 1);
        if (posTo != -1) //if found char
        {
          return input.Substring(posFrom + 1, posTo - posFrom - 1);
        }
      }
      return string.Empty;
    }
  }
}
