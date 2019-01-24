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

using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreRel_UndoRedo
{
  internal static class PreRel_UndoRedo
  {
    internal static string Category = "SampleCategory";
  }

  /// <summary>
  /// An empty sample undo/redo operation used for illustrating how to add to an OperationManager.
  /// </summary>
  internal class MySampleOperation : Operation
  {
    public MySampleOperation()
    {
      // TODO - do init stuff here.  Pass parameters as necessary
    }

    /// <summary>
    /// Gets the name of the operation
    /// </summary>
    public override string Name
    {
      get { return "Sample Operation"; }
    }

    /// <summary>
    /// Gets the category of the operation
    /// </summary>
    public override string Category
    {
      get { return PreRel_UndoRedo.Category; }
    }

    /// <summary>
    /// Performs the operation
    /// </summary>
    /// <returns>A Task to the DoAsync method</returns>
    protected override Task DoAsync()
    {
      // TODO - do something here 

      return Task.FromResult(0);
    }

    /// <summary>
    /// Repeats the operation
    /// </summary>
    /// <returns>A Task to the RedoAsync method</returns>
    protected override Task RedoAsync()
    {
      return DoAsync();
    }

    /// <summary>
    /// Undo the operation to reset the state
    /// </summary>
    /// <returns>A Task to the UndoAsync method</returns>
    protected override Task UndoAsync()
    {
      // TODO - undo the something here

      return Task.FromResult(0);
    }

  }
}
