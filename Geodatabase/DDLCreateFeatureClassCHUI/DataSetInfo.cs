//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Encapsulates the meta data definition for a Dataset
/// </summary>
namespace DDLCreateFeatureClassCHUI
{

  /// <summary>
  /// Represents information about a dataset, including its name and associated definition.
  /// </summary>
  /// <remarks>
  /// This class provides a way to encapsulate metadata about a dataset, such as its name and  the
  /// definition that describes its structure or configuration.</remarks>
  public class DatasetInfo
  {
    /// <summary>
    /// Dataset name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Definition for a given dataset
    /// </summary>
    public Definition DatasetDefinition { get; set; }
  }
}