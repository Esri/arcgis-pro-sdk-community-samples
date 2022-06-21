/*

   Copyright 2019 Esri

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
using System.Windows.Media;

namespace SLR_Analyst
{
	public class KeyValueWithTooltip
	{
		private static readonly Random rand = new Random();

		public string Key { get; set; }
		public int Value { get; set; }
		public string Code { get; set; }
		public Brush ChartColor => new SolidColorBrush(Color.FromArgb(255, (byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256)));

		public string Tooltip
		{
			get
			{
				return $@"Code: {Code} count: {Value}";
			}
		}

		public KeyValueWithTooltip Clone ()
		{
			return new KeyValueWithTooltip()
			{
				Key = this.Key,
				Value = this.Value,
				Code = this.Code
			};
		}

	}
}
