# Installation
First, download the files from the [Releases](../../../releases).

## Dynamics 365
1. Go to Settings > Solutions, click Import.
 
2. Follow the prompts to import the `ReportRenderer_X_X_X_X_managed.zip` file.

3. After the solution is imported, open it up.

4. Go to the Configuration page, set the username/password, and click Submit.

   ![](./ReportRenderer_Solution_Config.png "New Registration")

5. Open the `Render` process and activate it.

## Azure AD
1. Open Azure Active Directory from the [Azure Portal](https://portal.azure.com).

2. Open App registrations.

3. Click the link to add a new application registration.

   ![](./ReportRenderer_Azure_Create.png "New Registration")

4. After the application registration is created, copy the Application ID.  You will use this as the Client ID.

5. Click Settings.

6. Click Reply URLs, add the appropriate one for your region, then click Save.

   ![](./ReportRenderer_Azure_ReplyURLs.png "Reply URL")

   | Region         | Reply URL                                             |
   | :------------- |:----------------------------------------------------- |
   | United States  | https://msmanaged-na.consent.azure-apim.net/redirect  |
   | Europe         | https://europe-001.consent.azure-apim.net             |
   | Asia           | https://asia-001.consent.azure-apim.net               |
   | Australia      | https://australia-001.consent.azure-apim.net          |
   | India          | https://india-001.consent.azure-apim.net              |
   | Japan          | https://japan-001.consent.azure-apim.net              |
   | Canada         | https://canada-001.consent.azure-apim.net             |
   | Brazil         | https://brazil-001.consent.azure-apim.net             |
   | United Kingdom | https://uk-001.consent.azure-apim.net                 |

   

7. Add a required permission for Dynamics CRM Online.  After choosing the delegated permission, click Select, and then click Done.

   ![](./ReportRenderer_Azure_Permissions.png "New Registration")

8. Add a new key. After saving, copy the generated Value.  You will use this as the Client Secret below.

   ![](./ReportRenderer_Azure_Keys.png "Keys")


## Flow
1. Create the connector by importing the OpenAPI file.

   ![](./ReportRenderer_Connector_Import.png "Import Connector")
   ![](./ReportRenderer_Connector_Create.png "Create Connector")

2. After the import, replace the Host with your Dynamics 365 host.

   ![](./ReportRenderer_Connector_General.png "General")

3. Next, configure Security.  Use the Client ID/Secret from the app registration above.  Also replace the Resource URL.

   ![](./ReportRenderer_Connector_Security.png "Security")

4. Click on the Test link, then click Create Connector.

5. Create a new connection.

   ![](./ReportRenderer_Connector_NewConnection.png "New Connection")

6. After logging in, you will be asked to consent to the permissions.

   ![](./ReportRenderer_Connector_Consent.png "Grant Consent")

7. Finally, test the API call.  
   _You may get a 404 error.  Wait approximately 30 minutes and try again._