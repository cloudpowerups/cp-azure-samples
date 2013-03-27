This is a sample of Multi Component Role deployable using Cloud Power-Ups packaging solution.
---------------
Introduction
---------------
Application shown here is a scalable file download service which consists of 3 components:
1. Web Api, which is used to queue the download.
2. Engine (Windows Service), which actually downloads files and saves them into storage
3. Frontend, a UI to display latest download activity

---------------
Publishing
---------------
In order to publish this application to Azure, perform following steps:
1. Execute Publish.ps1
2. Edit MultiComponentRole.cscfg file in "deploy" folder:
	2.1. Enter value for "StorageConnectionString"
		2.1.1. Note: this is a connection string to your Azure storage account in format: DefaultEndpointsProtocol=http;AccountName=[AccountName];AccountKey=[AccountKey]
	2.2. Set "instanceCount" to "2" (or whatever you prefer)
3. Go to Windows Azure Portal
4. Create a new Cloud Service called "File Download Service"
5. Deploy package to Production
6. Done!

---------------
Testing
---------------
1. Navigate to <service_name>.cloudapps.net:8088
2. Enter file url to download
3. Click "Download it!"
	3.1. Alternatively you can send a POST to <service_name>.cloudapps.net:80/api/download/ with "=<file_uri>" body.
4. Navigate to <service_name>.cloudapps.net
5. Now you can see files uploaded to your storage

---------------
Manual package creation details
---------------
1. A package has been created using Cloud Power-Ups service with these parameters:
	1.1. Service Name: File Download Service
	1.2. Role Name: Multi Component Role
	1.3. Size: Extra Small
	1.4. Settings: add "StorageConnectionString"
	1.5. Endpoints: open 2 public HTTP ports -> 80 & 8088
	1.6. Websites:
		1.6.1. WebApi: port 8088
		1.6.2. Frontend: port 80
		1.6.3. Note: if you are using Try account you will not be able to host both WebApi & Frontend. You can still merge them together if needed or deploy separately.
	1.7. Windows Services:
		1.7.1. Name: ProcessingEngine
		1.7.2. Executable: CP.Azure.Samples.MultiComponentRole.Engine.exe
2. Download the package, unzip it and copy next to Publish.ps1 script