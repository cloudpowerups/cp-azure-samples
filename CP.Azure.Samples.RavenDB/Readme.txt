---------------
Disclaimer
---------------
RavenDB (http://ravendb.net/) is a property Hibernating Rhinos. You can find details about licensing here: http://ravendb.net/licensing

---------------
Introduction
---------------
This is a sample RavenDB hosting solution in "master"-"slave reads" configuration prepared with Cloud Power-Ups service (http://cloudpowerups.com).
It consists of following components :
1. Raven-Master, which is a RavenDB web server running on a single Cloud Service instance (it can't be load-balanced with Azure's round-robin approach). Data is persisted in Azure drive.
2. Raven-Slave, an automatically configured read replicas of master database. They are maintained on instance local disk, so not persisted.

---------------
Publishing
---------------
In order to publish this application to Azure, perform following steps:
1. Execute Publish.ps1
2. Edit RavenMasterSlaveReads.Cloud.Template.cscfg file and copy it into "deploy" folder:
	2.1. Modify value of "DriveStorageConnection_RavenDrive"
		2.1.1. Note: this is a connection string to your Azure storage account 
	2.2. Modify "ReadsDBConnectionString" and "WritesDBConnectionString" by specifying cloud service name as it will be created in Azure portal.
3. Go to Windows Azure Portal
4. Create a new Cloud Service called "Raven Master Slave Reads"
5. Deploy package to Production
6. Done!

---------------
Testing
---------------
1. Navigate to <service_name>.cloudapps.net
2. Switch to "WritesDB"
3. Create new document
4. Navigate to <service_name>.cloudapps.net:8080
5. Switch to "ReadsDB"
6. Verify that newly created document is present there

---------------
Raven database template details
---------------
1. MasterDatabase.zip:
	1.1. Added "WritesKey" API Key and allowed Admin access to both <system> and all DBs
	1.2. Created "WritesDB" with Replication bundle enabled
2. SlaveDatabase.zip:
	1.1. Added "ReadsKey" API Key and allowed Admin access to both <system> and all DBs
	1.2. Created "ReadsDB" with Replication bundle enabled

---------------
Manual package creation details
---------------
1. A package has been created using Cloud Power-Ups service with these parameters:
	1.1. Service Name: RavenMasterSlaveReads
	1.2. Role Name: RavenDB-Master
		1.2.1. Size: Extra Small
		1.2.2. Tasks: add "Bootstrap.cmd"
		1.2.3. Endpoints: add "RavenIn" port 80, HTTP, Public
		1.2.4. Websites:
			1.2.4.1. RavenWeb: mapped to "RavenIn" endpoint
		1.2.5. Cloud Drives:
			1.2.5.1. Name: RavenDrive
			1.2.5.2. Size: 5120 MB
			1.2.5.3. File name: raven-master.vhd
	1.3. Role Name: RavenDB-Slave
		1.3.1. Size: Extra Small
		1.3.2. Settings: add "WritesDBConnectionString" and "ReadsDBConnectionString" 
		1.3.3. Tasks: add "Bootstrap.cmd"
		1.3.4 Endpoints: 
			1.3.4.1. Add "RavenIn" port 80, HTTP, Public
			1.3.4.2. Add "RavenReplication" port 9000, HTTP, Internal
		1.3.5. Websites:
			1.3.5.1 RavenWeb: mapped to both "RavenIn" & "RavenReplication" endpoints
		1.3.6. Local Resources:
			1.3.6.1. Name: RavenDrive
			1.3.6.2. Size: 5120 MB
2. Download the package, unzip it and copy next to Publish.ps1 script