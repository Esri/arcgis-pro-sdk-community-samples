/*

   Copyright 2026 Esri

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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DDLCreateFeatureClassCHUI
{
  public class PropertyChangedBase : INotifyPropertyChanged
  {
    //
    // Summary:
    //     Occurs when a property value changes.
    public event PropertyChangedEventHandler PropertyChanged;

    //
    // Summary:
    //     Sets a property value and calls NotifyPropertyChanged when the new value differs
    //     from the current value.
    //
    // Returns:
    //     Returns true if the property changes.

    public bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string name = "")
    {
      bool num = !EqualityComparer<T>.Default.Equals(backingField, value);
      if (num)
      {
        backingField = value;
        NotifyPropertyChanged(name);
      }

      return num;
    }

    //
    // Summary:
    //     Raises the PropertyChanged event for the specified property.
    protected void NotifyPropertyChanged<T>(Expression<Func<T>> property)
    {
      NotifyPropertyChanged(GetName(property));
    }

    //
    // Summary:
    //     Raises the PropertyChanged event for the specified property.
    protected void NotifyPropertyChanged([CallerMemberName] string name = "")
    {
      NotifyPropertyChanged(new PropertyChangedEventArgs(name));
    }

    //
    // Summary:
    //     Raises the PropertyChanged event for the specified property.
    protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
    {
      this.PropertyChanged?.Invoke(this, args);
    }

    private static string GetName<T>(Expression<Func<T>> property)
    {
      MemberExpression memberExpression = (property.Body is not UnaryExpression) ? ((MemberExpression)property.Body) : ((MemberExpression)((UnaryExpression)property.Body).Operand);
      return memberExpression.Member.Name;
    }
  }

  public static class ListExtensions
  {
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
    {
      ArgumentNullException.ThrowIfNull(list);

      ArgumentNullException.ThrowIfNull(collection);

      if (list is List<T> list2)
      {
        list2.AddRange(collection);
        return;
      }

      foreach (T item in collection)
      {
        list.Add(item);
      }
    }
  }

  // Simple RelayCommand implementation
  public class RelayCommand : ICommand
  {
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
      _execute = execute;
      _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object parameter) => _execute(parameter);
    public event System.EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }
  }

}
