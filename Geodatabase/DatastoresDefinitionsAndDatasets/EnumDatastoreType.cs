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
namespace DatastoresDefinitionsAndDatasets
{
	/// <summary>
	/// Enums of all DatastoreTypes
	/// </summary>
	public enum EnumDatastoreType
	{
		/// <summary>
		/// Datastore is File Geodatabase
		/// </summary>
		FileGDB,
		/// <summary>
		/// Datastore is Enterprise Geodatabase
		/// </summary>
		EnterpriseGDB,
		/// <summary>
		/// Datastore is Web Geodatabase
		/// </summary>
		WebGDB,
		/// <summary>
		/// Datastore is Enterprise Database
		/// </summary>
		EnterpriseDB,
		/// <summary>
		/// Datastore is SQLITE Database
		/// </summary>
		SqliteDB,
		/// <summary>
		/// Datastore is Shape File
		/// </summary>
		ShapeFile
	}
}
