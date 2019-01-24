/*

   Copyright 2018 Esri

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
using CustomCategoriesExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraReport1
{
  /// <summary>
  /// A custom component that implements the IAcmeCustomReport contract
  /// for the AcmeCustom_Reports category.
  /// </summary>
  /// <remarks>The component must register in its config.daml in
  /// order to be loaded by the cateogry "owner"</remarks>
  internal class ExtraReport1 : IAcmeCustomReport
  {
    public string Label => "Extra Report1";

    public string Title => Label;

    public string Details => "Details details details";

    public Task RunAsync()
    {
      return Task.Delay(2000);
    }
  }
}
