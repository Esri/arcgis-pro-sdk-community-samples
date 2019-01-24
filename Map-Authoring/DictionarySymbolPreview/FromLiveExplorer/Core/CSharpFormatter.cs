/*

   Copyright 2019 Esri

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
/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2015 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Documents;
using Xceed.Wpf.Toolkit.LiveExplorer.Core.CodeFormatting;
using Xceed.Wpf.Toolkit;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Core
{
  /// <summary>
  /// Formats the RichTextBox text as colored C#
  /// </summary>
  public class CSharpFormatter : ITextFormatter
  {
    public readonly static CSharpFormatter Instance = new CSharpFormatter();

    public string GetText( FlowDocument document )
    {
      return new TextRange( document.ContentStart, document.ContentEnd ).Text;
    }

    public void SetText( FlowDocument document, string text )
    {
      document.Blocks.Clear();
      document.PageWidth = 2500;

      CSharpFormat cSharpFormat = new CSharpFormat();
      Paragraph p = new Paragraph();
      p = cSharpFormat.FormatCode( text );
      document.Blocks.Add( p );
    }
  }
}
