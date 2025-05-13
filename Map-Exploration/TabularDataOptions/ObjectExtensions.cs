/*

   Copyright 2025 Esri

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
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TabularDataOptions
{
  /// <summary>
  /// Take a Dictionary (string, object) and converts it into a dynamic object
  /// that allows access via a property instead of a dictionary key:
  /// i.e. dictionary: "Prop1", val can now be accessed val = dynObject.Prop1;
  /// </summary>
  public class ConvertToDynamic
  {
    /// <summary>
    /// Convenience method
    /// </summary>
    /// <param name="theDictionary"></param>
    /// <returns></returns>
    public static dynamic ToDynamic(Dictionary<string, object> theDictionary)
    {
      return new DynamicFromDictionary(theDictionary);
    }
  }

  public sealed class DynamicFromDictionary : DynamicObject
  {
    private readonly Dictionary<string, object> _theDictionary;

    /// <summary>
    /// CTor
    /// </summary>
    /// <param name="theDictionary">dictionary to initilize instance with</param>
    public DynamicFromDictionary(Dictionary<string, object> theDictionary)
    {
      _theDictionary = theDictionary;
    }

    /// <summary>
    /// Get dynamic Member names
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<string> GetDynamicMemberNames()
    {
      return _theDictionary.Keys;
    }

    /// <summary>
    /// Override to get member from dictionary property
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      if (_theDictionary.ContainsKey(binder.Name))
      {
        result = _theDictionary[binder.Name];
        return true;
      }
      else
      {
        result = null;
        return false;
      }
    }

    /// <summary>
    /// Override to set member using dictionary property
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      if (_theDictionary.ContainsKey(binder.Name))
      {
        _theDictionary[binder.Name] = value;
        return true;
      }
      else
      {
        return false;
      }
    }
  }

}
