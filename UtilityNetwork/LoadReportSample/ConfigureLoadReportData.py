# ConfigureLoadReportData.py
#
# This script should be run against a database created by the ArcGIS for Electric electric distribution asset package
# It configures the database and utility network to have the necessary schema to run the Create Load Report SDK sample
#
# 1. Before running this script, make sure to shut off all services that use this database.
# 2. Run this script when connected via client-server

import arcpy

# Parameters

utilityNetwork = arcpy.GetParameterAsText(0)
electricDistributionDeviceFeatureClass = arcpy.GetParameterAsText(1)

# Constants

DomainNetwork = "ElectricDistribution"
ServicePointCategoryName = "ServicePoint"
LoadField = "SERVICE_LOAD"
LoadAttribute = "Customer Load"
LoadAttribute2 = "Power Rating"
# Variables

needToAddCategory = False
needToAddNetworkAttribute = False

# Determine what has to be done
describe = arcpy.Describe(utilityNetwork)
networkAttributes = {na.name.lower() for na in describe.networkAttributes}
print("Network attributes = {0}\n".format(networkAttributes))
networkCategories = {cat.name.lower() for cat in describe.categories}
print("Categories = {0}\n".format(networkCategories))

# This sample will work with data that uses a network attribute called "Customer Load" (Pro 2.1 Naperville Electric Data)
# 	or "Power Rating" (Pro 2.2 Naperville Electric Data)
if LoadAttribute.lower() not in networkAttributes:
	if LoadAttribute2.lower() not in networkAttributes:
		needToAddNetworkAttribute = True
	
if ServicePointCategoryName.lower() not in networkCategories:
	needToAddCategory = True
	

if needToAddCategory == True or needToAddNetworkAttribute == True:

	# Disable the Utility Network topology
	arcpy.AddMessage("Disabling Utility Network Topology")
	arcpy.un.DisableNetworkTopology(utilityNetwork)

	if needToAddNetworkAttribute == True:
		# Create a Network Attribute to represent load
		arcpy.AddMessage("Creating Customer Load Network Attribute")
		arcpy.un.AddNetworkAttribute(utilityNetwork, LoadAttribute, "Long", False, None, False)
		
		# Assign the network attribute to the SERVICE_LOAD field in the ElectricDistributionDevice table
		arcpy.AddMessage("Assigning Customer Load Network Attribute to Field")
		arcpy.un.SetNetworkAttribute(utilityNetwork, LoadAttribute, DomainNetwork, electricDistributionDeviceFeatureClass, LoadField)
		
	if needToAddCategory == True:
		# Add a ServicePoint category
		arcpy.AddMessage("Adding ServicePoint Category")
		arcpy.un.AddNetworkCategory(utilityNetwork, ServicePointCategoryName)

		# Assign the ServicePoint category to the asset types in the ServicePoint asset group
		arcpy.AddMessage("Assigning ServicePoint Category")
		arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Primary Meter", ServicePointCategoryName)
		arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Single Phase Low Voltage Meter", ServicePointCategoryName)
		arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Three Phase Low Voltage Meter", ServicePointCategoryName)

	# Re-enable the utility network topology
	arcpy.AddMessage("Enabling Utility Network Topology")
	arcpy.un.EnableNetworkTopology(utilityNetwork)
	arcpy.AddMessage("Schema modified to work with Load Report sample")
	
else:
	arcpy.AddMessage("Schema OK")

