# Setup environment

You first create resource group, VNET, Key Vault and Blob.

```
az group create -n rgname -l koreacentral
az network ...
```

## Setup Key Vault

Create a certificate by importing certificate. If you don't have a certificate then choose `Generate` method to create a self-signed certificate. Copy certificate url and thumbprint after the certificated is enabled.

https://docs.microsoft.com/en-us/azure/virtual-machines/linux/tutorial-secure-web-server#generate-a-certificate-and-store-in-key-vault

CLI to create a user generated certificate.

```
az keyvault certificate create \
    --vault-name ikdnckv \
    --name mycert \
    --policy "$(az keyvault certificate get-default-policy)"
```

> Note that use `Secret Identifier` as a certificate url, not `Certificate Identifier`. 

Create a secret `adminpassword` and put your admin password. You also create a secret `testkey` and put any secret string such as `Microsoft Azure`.

Lastly, you need to enable access to ARM template deployment for this sample. 

![advanced access policies](https://docs.microsoft.com/en-us/azure/azure-resource-manager/media/resource-manager-tutorial-use-key-vault/resource-manager-tutorial-key-vault-access-policies.png)

## Setup Blob

Create a blob container (private) `doc` and copy [hello.txt](./asset/hello.txt) to this container location. You will access this file using MSI.

You can also enable [VNet service endpoint](https://docs.microsoft.com/en-us/azure/virtual-network/virtual-network-service-endpoints-overview) for your blob account and Key Vault.

## Setup App

Update properties of Key Vault and Blob account in two appsetting files ([appsettings_1.json](./asset/appsettings_1.json) and [appsettings_2.json](./asset/appsettings_2.json))
