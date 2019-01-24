//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadReportSample
{
  /// <summary>
  /// Represents the result of the load report
  /// </summary>
  /// <remarks>
  /// This class is used to pass back information from the worker thread back to the Button (which executes on a UI thread).  
  /// The Message property is used to return error messages.
  /// </remarks>
  /// 
  class LoadTraceResults
  {
    public bool Success { get; set; }
    public int NumberServicePointsA { get; set; }
    public int NumberServicePointsB { get; set; }
    public int NumberServicePointsC { get; set; }
    public double TotalLoadA { get; set; }
    public double TotalLoadB { get; set; }
    public double TotalLoadC { get; set; }
    public string Message { get; set; }
  }
}
