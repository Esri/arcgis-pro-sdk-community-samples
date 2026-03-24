# Copilot Custom Instructions for ArcGIS Pro SDK Add-in Development

I am developing ArcGIS Pro Add-ins using the ArcGIS Pro SDK.

- Use the authoritative ArcGIS Pro SDK API documentation as the primary reference: https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference and https://github.com/Esri/arcgis-pro-sdk/tree/master/References/ArcGIS%20Pro%20API
- When providing code examples, follow the patterns and best practices from the official ArcGIS Pro SDK documentation and samples.
- Prefer C# for all code snippets and explanations.
- Reference and leverage the code snippets available at: https://github.com/Esri/arcgis-pro-sdk-snippets and at: https://github.com/Esri/arcgis-pro-sdk
- Assume the development environment is Visual Studio 2022 or later.
- Use a tab size as defined in the Visual Studio settings (usually 2 spaces).
- To open ArcGIS Pro items, use the OpenItemDialog rather than the standard OpenFileDialog.
- To display a message box, use the ArcGIS.Desktop.Framework.Dialogs.MessageBox class.
- Use modern .NET and ArcGIS Pro SDK conventions.
- Use the https://github.com/Esri/arcgis-pro-sdk/blob/master/References/ArcGIS.Desktop.Framework.xsd xml schema when making changes to any config.daml desktop add-in markup language file.
- For UI development, use the MVVM (Model-View-ViewModel) programming pattern as recommended by the ArcGIS Pro SDK.
- When possible, provide concise explanations and relevant links to documentation.
- If a task involves UI, follow the ArcGIS Pro Add-in UI guidelines.
- CIMSymbol has an extension method called MakeSymbolReference.  Use MakeSymbolReference to convert any CIMSymbol to a CIMSymbolReference.

Always ensure that code suggestions are compatible with the latest supported version of ArGIS Pro SDK.
