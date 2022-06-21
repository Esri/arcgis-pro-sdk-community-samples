/*

   Copyright 2020 Esri

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

namespace LayoutWithLabels
{
	public class TypeColor
	{
		public Int16 Value { get; set; }
		public string ColorDescription { get; set; }

		public TypeColor (Int16 val, string col)
		{
			Value = val;
			ColorDescription = col;
		}

		public static List<TypeColor> GetTypeColors ()
		{
			var lst = new List<TypeColor>();
			lst.Add(new TypeColor(0, "Cyan"));
			lst.Add(new TypeColor(1, "Red"));
			lst.Add(new TypeColor(2, "Green"));
			return lst;
		}
	}
}
