# Securely access Blob and Key Vault using MSI

## Setup Key Vault

To setup VMSS to access Key Vault, run following commands.

```
az vmss identity show -g $rg -n $vmss
az vmss identity assign -g $rg -n $vmss

assignee=$(az vmss identity show -g $rg -n $vmss --query "principalId" -o tsv)

role=$(az role definition list --query '[?roleName==`Key Vault Contributor`].name' -o tsv)

scope=$(az keyvault show -g $rg -n ikdnckv --query "id" -o tsv)

echo "$assignee, $role, $scope"
az role assignment create --role $role --assignee $assignee --scope $scope

az keyvault set-policy -n ikdnckv -g $rg --object-id $assignee --secret-permissions list get
```

> To access KeyVault, you need to configure additional permission setting in `Access Policies`.

To test whether MSI is correctly configured, test following commands inside Windows VM instance.

```
$response = $(curl.exe -s 'http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https%3A%2F%2Fvault.azure.net' -H "Metadata:true" | ConvertFrom-Json)

curl.exe -s "https://${kv}.vault.azure.net/secrets/testkey/?api-version=2016-10-01" -H "Authorization: Bearer $($response.access_token)" | ConvertFrom-Json | % { $_.Value}

```

Now, test keyvault endpoint.

```
curl -k https://<ip>/keyvault

Microsoft Azure
```

If you didn't set MSI correctly you'll get following error.

```
Something went wrong: Parameters: Connection String: [No connection string specified], Resource: https://vault.azure.net, Authority: https://login.windows.net/XXXX-XXXX. Exception Message: Tried the following 3 methods to get an access token, but none of them worked.
....
```

## Setup Blob

To setup VMSS to access blob storage, run following commands.

```
role=$(az role definition list --query '[?roleName==`Storage Blob Data Contributor (Preview)`].name' -o tsv)
scope=$(az storage account show -g $rg -n $blob --query "id" -o tsv)

echo "$assignee, $role, $scope"
az role assignment create --role $role --assignee $assignee --scope $scope
```

Similarly, test following commands inside Windows VM instance.

```
$response = $(curl.exe -s 'http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://storage.azure.com/' -H "Metadata:true" | ConvertFrom-Json)

curl.exe "https://${blob}.blob.core.windows.net/doc/hello.txt" -H 'x-ms-version: 2017-11-09' -H "Authorization: Bearer $($response.access_token)"
```

Now, test blob endpoint.

```
curl -k https://<ip>/blob

Hello, Microsoft Azure
```