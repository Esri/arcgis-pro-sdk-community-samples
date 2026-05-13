using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace QueryBuilderControl
{
  internal static class Utilities
  {
    public static bool IsOnUIThread
    {
      get { return Application.Current?.Dispatcher.CheckAccess() == true; }
    }
    public static void RunOnUIThread(Action action)
    {
      if (IsOnUIThread)
        action();
      else if (Application.Current != null)
        Application.Current.Dispatcher.Invoke(action);
    }
  }
}
