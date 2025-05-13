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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPToolInspector.TreeHelpers
{
  /// <summary>
  /// Interaction logic for SearchWithDropDownView.xaml
  /// </summary>
  public partial class SearchWithDropDownView : UserControl, INotifyPropertyChanged
  {
    /// <summary>
    /// Ctor
    /// </summary>
    public SearchWithDropDownView()
    {
      InitializeComponent();
      //this.searchTextBox.DataContext = this;
    }

    #region INotifyPropertyChanged implementation

    SynchronizationContext sc = SynchronizationContext.Current;
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler CanExecuteChanged;
    private void OnPropertyChanged([CallerMemberName] string propName = "") =>
      sc.Post(new SendOrPostCallback((p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName))), null);

    #endregion // INotifyPropertyChanged implementation

    public ImageSource SearchBackgroundImgSource => Application.Current.Resources["Search16"] as ImageSource;

    /// <summary>
    /// Searchtext property
    /// </summary>
    public string Searchtext
    {
      get { return (string)GetValue(SearchtextProperty); }
      set { 
        SetValue(SearchtextProperty, value);
        OnPropertyChanged();
      }
    }

    /// <summary>
    /// Searchtext dependency property definition: registers the property with the framework
    /// </summary>
    public static readonly DependencyProperty SearchtextProperty =
        DependencyProperty.Register("Searchtext", typeof(string), typeof(SearchWithDropDownView));

    /// <summary>
    /// Dependency property for complete searchable text list to be used for binding
    /// </summary>
    public static readonly DependencyProperty SearchtextListProperty =
                           DependencyProperty.RegisterAttached("SearchtextList", typeof(ObservableCollection<string>),
                           typeof(SearchWithDropDownView), new UIPropertyMetadata(null,
                           SearchtextListPropertyChanged));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static ObservableCollection<string> GetSearchtextList(DependencyObject obj)
    {
      return (ObservableCollection<string>)obj.GetValue(SearchtextListProperty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetSearchtextList(DependencyObject obj, string value)
    {
      obj.SetValue(SearchtextListProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    public static void SearchtextListPropertyChanged(DependencyObject o,
                                                 DependencyPropertyChangedEventArgs e)
    {
      ObservableCollection<string> newList = (ObservableCollection<string>)e.NewValue;
      System.Diagnostics.Trace.WriteLine("SearchtextListPropertyChanged: " + newList.Count);
    }


    /// <summary>
    /// Dependency property for complete searchable text list to be used for binding
    /// </summary>
    public static readonly DependencyProperty SelectedSearchTextProperty =
                           DependencyProperty.RegisterAttached("SelectedSearchText", typeof(string),
                           typeof(SearchWithDropDownView), new UIPropertyMetadata(null,
                           SelectedSearchTextPropertyChanged));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetSelectedSearchText(DependencyObject obj)
    {
      return (string)obj.GetValue(SelectedSearchTextProperty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetSelectedSearchText(DependencyObject obj, string value)
    {
      obj.SetValue(SelectedSearchTextProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    public static void SelectedSearchTextPropertyChanged(DependencyObject o,
                                                 DependencyPropertyChangedEventArgs e)
    {
      // _webBrowser = (WebBrowser)o;
      string location = (string)e.NewValue;
      if (string.IsNullOrEmpty(location))
      {
        location = @"<html><body>Nothing selected</body></html>";
      }
      //_webBrowser.NavigateToString(location);
    }
  }
}
