//Copyright 2015 Esri

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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ArcGIS.Core.Data;

namespace LayersPane.Extensions
{
    /// <summary>
    /// From <a href="http://social.technet.microsoft.com/wiki/contents/articles/17635.the-equivalent-to-autogeneratecolumns-for-a-listviewgridview.aspx"/>
    /// </summary>
    public class ExtendListView
    {

        private static ObjectConverter _forBlob = new ObjectConverter() { TheFieldType = FieldType.Blob };
        private static ObjectConverter _forRaster = new ObjectConverter() { TheFieldType = FieldType.Raster };
        private static ObjectConverter _forGeom = new ObjectConverter() { TheFieldType = FieldType.Geometry };

        public static List<Field> Columns { get; set; }


        public static readonly DependencyProperty GenerateColumnsProperty =
            DependencyProperty.RegisterAttached("GenerateColumns",
            typeof(bool?), typeof(ExtendListView),
            new FrameworkPropertyMetadata(null, GenerateColumnsChanged));

        public static bool GetGenerateColumns(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(GenerateColumnsProperty);
        }

        public static void SetGenerateColumns(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(GenerateColumnsProperty, value);
        }

        public static void GenerateColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //register for the itemssourcechanged on the ListView
            ListView lv = (ListView)obj;
            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(ListView.ItemsSourceProperty, typeof(ListView));
            descriptor.AddValueChanged(lv, new EventHandler(ItemsSourceChanged));
        }

        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            IEnumerable its = lv.ItemsSource;
            if (null != its)
            {
                IEnumerator itsEnumerator = its.GetEnumerator();

                bool hasItems = itsEnumerator.MoveNext();
                if (hasItems)
                {
                    SetUpTheColumns(lv);
                }
            }
        }

        private static void SetUpTheColumns(ListView theListView)
        {

            GridView gv = (GridView)theListView.View;
            gv.Columns.Clear();

            foreach (var field in Columns)
            {
                //foreach (var field in ((RowData)theListView.ItemsSource).Columns) {
                string columnName = FieldName(field);
                //Binding bnd = new Binding(string.Format("[{0}]", col++));
                Binding bnd = new Binding(columnName);
                GridViewColumn grv = new GridViewColumn { Header = columnName };

                switch (field.FieldType)
                {
                    case FieldType.Blob:
                        bnd.Converter = _forBlob;
                        break;
                    case FieldType.Raster:
                        bnd.Converter = _forRaster;
                        break;
                    case FieldType.Geometry:
                        _forGeom.Init();
                        bnd.Converter = _forGeom;
                        break;
                    case FieldType.Date:
                        bnd.StringFormat = @"mm/dd/yyyy";
                        break;
                    case FieldType.Double:
                        bnd.StringFormat = "0,0.0##";
                        break;
                    case FieldType.Integer:
                    case FieldType.OID:
                    case FieldType.Single:
                    case FieldType.SmallInteger:
                        bnd.StringFormat = "0,0";
                        break;
                    case FieldType.GlobalID:
                    case FieldType.GUID:
                    case FieldType.String:
                    case FieldType.XML:
                    default:
                        break;
                }
                //BindingOperations.SetBinding(grv, TextBlock.TextProperty, bnd);
                grv.DisplayMemberBinding = bnd;
                gv.Columns.Add(grv);

            }
        }

        private static string FieldName(Field fld)
        {
            return fld.AliasName != null && fld.AliasName.Length > 0 ? fld.AliasName : fld.Name;
        }
    }

    public class ObjectConverter : IValueConverter
    {

        public FieldType TheFieldType { get; set; }
        private string _shapeType { get; set; }

        public void Init()
        {
            _shapeType = "";//reset
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DBNull)
                return null;
            if (TheFieldType == FieldType.Blob)
                return "Blob";
            else if (TheFieldType == FieldType.Raster)
                return "Raster";
            else if (TheFieldType == FieldType.Geometry)
            {
                //if (_shapeType.Length == 0) {
                //    Geometry shape = Interop.CreateGeometryWrapperOnHandle(new IntPtr((int)value));
                //    _shapeType = shape.GeometryType.ToString();
                //}
                //return _shapeType;
                //return shape.GeometryType.ToString();
                return "Shape";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
