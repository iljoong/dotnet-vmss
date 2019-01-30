# Prepare App and Build VM Images

## Prepare .NET Core App

You can develop your own .NET Core application or use [sample app](./src). If you develop your own application than please refer below sections.

### create a webapi project

```
dotnet new webapi -o dncbench
```

### add nuget package

```
dotnet add package Microsoft.Azure.KeyVault
dotnet add package Microsoft.Azure.Services.AppAuthentication
dotnet add package WindowsAzure.Storage
```

### update api endpoints

Update code. You need update default Get function and add additional 3 endpoints, `/healthcheck`, `/keyvault` and `/blob`.

See [./src/dncbench/Controllers/ValuesController.cs](./src/dncbench/Controllers/ValuesController.cs) for more detail.

### publish and package(zip) app

Run publish project

```
cd ./src
dotnet publish -c Release -o ..\out dncbench

Compress-Archive -Path .\out\* -DestinationPath apiapp.zip -Force
```

After package into zip file, copy this package to [asset](./asset) folder. 

## Build custom VM images using Packer

You could build custom VM image manually or build using automation tool like [packer](https://packer.io). Install `packer` on your PC if you haven't installed.

You also need to create an [Azure AD service principal](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) before update packer template file. If you don't have a service principal then create it.

```
az account list -o table

export SUBSCRIPTION_ID=<subs id>

az ad sp create-for-rbac --role="Contributor" --scopes="/subscriptions/${SUBSCRIPTION_ID}"
```

Output of service principal information is following,

```
{
  "appId": "0000000000-0000-0000-0000-0000000000",
  "displayName": "azure-cli-2018-11-15-07-19-22",
  "name": "http://azure-cli-2018-11-15-07-19-22",
  "password": "xxxxxxxxxx-xxxx-xxxx-xxxxxxxxxxx",
  "tenant": "11111111-1111-1111-11111-11111111111"
}
```

> note that `appId` is also called `client_id` and `password` is also called `client_secret`.

Update variables like `rgname`, `imagename` and etc in packer template file before run this script.

Run packer to build two images.

```
packer build ./packer/winvm_packer_1.json

packer build ./packer/winvm_packer_2.json
```

## References

- [Azure Storage APIs for .NET](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage?view=azure-dotnet)
- [Azure Storage samples using .NET](https://docs.microsoft.com/en-us/azure/storage/common/storage-samples-dotnet)
- [Azure Key Vault sample](https://azure.microsoft.com/en-us/resources/samples/app-service-msi-keyvault-dotnet/)