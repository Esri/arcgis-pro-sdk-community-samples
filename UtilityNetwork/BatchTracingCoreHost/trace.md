# Batch Tracer
This analysis will run a named trace configuration against a specific asset group in the network and persist the aggregated geometries for each trace along with a unique identifier from the starting feature. It is particularly useful when you have an area of your network you are interested in analyzing, but it isn't signficant or permanent enough to justify managing subnetworks.

The following graphic shows an example of the batch trace used to identify the features downstream of a particular type of devie.

 ![Batch trace devices](Graphics/Batch%20B.png "Batch tracing devices, producing multiple results.")
 
 The following graphic shows an example of the batch trace used to identify the features downstream of certain sensors. Note that the results can overlap, meaning one feature can be reported multiple times.

 ![Batch trace sensors](Graphics/Batch%20A.png "Batch tracing sensors, producing overlapping output")


## Use Cases
Common use cases for this analysis include:
- Identifying all the customers downstream of a Transformer ([Electric](JSON%20Configurations/Trace_Electric_Customers.json)), District Meter (Water or Gas)
- Identifying the sewershed upstream of a Lift Station or Smart Manhole ([Wastewater](JSON%20Configurations/Trace_Sewer_LiftZone.json))
- Identifying the total drainage upstream of an outfall that drains into public waterways ([Stormwater](JSON%20Configurations/Trace_Storm_Upstream.json))
- Identifying the local drainage for each Best Management Practice (BMP) area in the network ([Stormwater](JSON%20Configurations/Trace_Storm_BMP.json))

## Parameters
These additional parameters are avaialble when running this analysis:
- Analysis
  - type: Type of analysis to perform
  - analysisName: name of the analysis
  - networkSourceName: The name of the class to use as start locations
  - assetGroupCode: The asset group code (subtype) to use as start locations
  - terminalName (optional): The terminal to start the analysis with
  - namedTraceConfigurtion: The named trace configuration to use for analysis
  - definitionQuery (optional): The definition query to be applied to start locations
- Input
  - inputWorkspace: Location of the utility network used for analysis
  - portalUrl (optional): Url of the portal to authenticate with. Only required when the input workspace is a feature service.
  - portalUser (optional): User name for the portal. Only required when establishing the credential for the application.
  - portalPassword (optional): Password for the portal. Only required when establishing the credential for the application for the first time. Remove once the   application is authenticated.
  - sourceUtilityNetwork: Name of the utility network
- Output
  - outputWorkspace: Location of the geodatabase to store the output
  - [outputPoints](ReadMe.md#aggregated-geometry-point-line-polygon) (optional): Name of the Aggregated Point output class
  - [outputPolylines](ReadMe.md#aggregated-geometry-point-line-polygon) (optional): Name of the Aggregated Line output class
  - [outputPolygons](ReadMe.md#aggregated-geometry-point-line-polygon) (optional): Name of the Aggregated Polygon output class
  - [outputTable](ReadMe.md#output-table) (optional): Name of the table to persist the results of the analysis
  - sourceResultField (optional): The field name on the source feature that contains the result name that will be stored in the output table
  - outputFunctionCount (optional): How many output fields should be added to the output feature classes to store Function results from the named trace configuration. Aliases must be applied manually once the table is created.

## Usage Notes
The tool will override the Result Types in your named trace configuration to only include Aggregated Geometry. However, you can use the Output Asset Types and Output Conditions in your Named Trace Configuration to control which features are output by the trace.