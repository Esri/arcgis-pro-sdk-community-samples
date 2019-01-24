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
using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatastoresDefinitionsAndDatasets
{
	/// <summary>
	/// Used to encapsulates Dataset Type Categories
	/// </summary>
	public class DatasetTypeCategory
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="datasetType"></param>
		public DatasetTypeCategory(string name, DatasetType datasetType)
		{
			Name = name;
			DatasetType = datasetType;
		}

		/// <summary>
		/// Datastore Category description
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// DatasetType
		/// </summary>
		public DatasetType DatasetType { get; set; }
	}
}
