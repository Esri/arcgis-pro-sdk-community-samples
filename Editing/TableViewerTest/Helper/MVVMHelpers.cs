/*

   Copyright 2023 Esri

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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace TableViewerTest.Helper
{

  public class RichTextBoxHelper : DependencyObject
  {
    private static HashSet<Thread> _recursionProtection = new HashSet<Thread>();

    public static string GetDocumentXaml(DependencyObject obj)
    {
      return (string)obj.GetValue(DocumentXamlProperty);
    }

    public static void SetDocumentXaml(DependencyObject obj, string value)
    {
      _recursionProtection.Add(Thread.CurrentThread);
      obj.SetValue(DocumentXamlProperty, value);
      _recursionProtection.Remove(Thread.CurrentThread);
    }

    public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached(
        "DocumentXaml",
        typeof(string),
        typeof(RichTextBoxHelper),
        new FrameworkPropertyMetadata(
            "",
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (obj, e) =>
            {
              if (_recursionProtection.Contains(Thread.CurrentThread))
                return;

              var richTextBox = (RichTextBox)obj;
              try
              {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(GetDocumentXaml(richTextBox)));
                var doc = (FlowDocument)XamlReader.Load(stream);

                richTextBox.Document = doc;
              }
              catch (Exception)
              {
                richTextBox.Document = new FlowDocument();
              }

              richTextBox.TextChanged += (obj2, e2) =>
              {
                RichTextBox richTextBox2 = obj2 as RichTextBox;
                if (richTextBox2 != null)
                {
                  SetDocumentXaml(richTextBox, XamlWriter.Save(richTextBox2.Document));
                }
              };
            }
        )
    );
  }

}
