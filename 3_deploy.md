# Deploy app to VMSS

You can deploy application to VMSS using custom VM images or platform image with VM extension.

Deploying with custom VM image is faster than using platform image with VM extension. However, if you are using platform image you could have a benefit of [automatic OS uprade](https://docs.microsoft.com/en-us/azure/virtual-machine-scale-sets/virtual-machine-scale-sets-automatic-upgrade).

## Azure Template

Deployment template includes following features.

- Set admin password using Key Vault
- Install certificate from Key Vault
- Setup WinRm to remote access
- Enable IIS to communicate https

### Secure admin password

Instead of using plain text, use secret from Key Vault.

```
    "adminPassword": {
        "reference": {
            "keyVault": {
                "id": "/subscriptions/XXXX-XXXX/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kvname"
            },
            "secretName": "adminpassword"
            }
    },
```

### Install certificate for HTTPS and WinRM

Set `secrets` and `WindowsConfiguration` properties.

> Note that you can generate self-signed certificate or import certficate from Key Vault. This example is using import from Key Vault.

```
    "osProfile": {
        "computerNamePrefix": "[variables('namingInfix')]",
        "adminUsername": "[parameters('adminUsername')]",
        "adminPassword": "[parameters('adminPassword')]",
        "secrets": [
            {
                "sourceVault": {
                    "id": "[parameters('vaultResourceId')]"
                },
                "vaultCertificates": [
                    {
                        "certificateUrl": "[parameters('certificateUrl')]",
                        "certificateStore": "My"
                    }
                ]
            }
        ],
        "windowsConfiguration": {
            "provisionVMAgent": true,
            "winRM": {
                "listeners": [
                    {
                        "protocol": "Http"
                    },
                    {
                        "protocol": "Https",
                        "certificateUrl": "[parameters('certificateUrl')]"
                    }
                ]
            },
            "enableAutomaticUpdates": false
        }
    },
```

### Enable IIS to communicate https

You'll setup binding SSL certificate to IIS for HTTPS communication. You can do this by running custom script using VM extension like below.

```
    "extensionProfile": {
        "extensions": [
            {
                "name": "CustomScriptExtension",
                "properties": {
                    "publisher": "Microsoft.Compute",
                    "type": "CustomScriptExtension",
                    "typeHandlerVersion": "1.9",
                    "autoUpgradeMinorVersion": true,
                    "settings": {
                        "fileUris": [
                            "[parameters('scriptUrl')]"
                        ],
                        "commandToExecute": "powershell -ExecutionPolicy Unrestricted -File iis_script.ps1"
                    }
                }
            }
        ]
    }
```

## Deploy using custom VM Image

Update thumbprint of certificate in [iis_script.ps1](./asset/iis_script.ps1) and upload this file to blob storage. Use this file url as`scriptUrl` parameter.

Update parameters `adminPassword`, `imageId`, `vaultResourceId`, and `certificateUrl` and `scriptUrl` in [vmss_win.parameter.json](./template/vmss_win.parameter.json).

Run deployment command.

```
az group deployment create -g $rg -n $vmssprod --template-file vmss_win.json --parameters @vmss_win.parameters.json

```

If you want to apply rolling policy to VMSS use [vmss_win_rolling.json](./template/vmss_win_rolling.json) template.

```
az group deployment create -g $rg -n $vmssstag --template-file vmss_win_rolling.json --parameters @vmss_win.parameters.json
```

## Deploy using patform image + VM Extension

Update thumbprint of certificate in [install_script.ps1](./asset/install_script.ps1) and upload this script file and `apiapp.zip` file to blob storage. Use this file urls as `scriptUrl` and `packageUrl` parameters.

Update parameters `adminPassword`, `imageId`, `vaultResourceId`, `certificateUrl`, `scriptUrl`, `packageUrl` and `packageName` in [vmss_win_ext.parameter.json](./template/vmss_win_ext.parameter.json)

Azure template for this deployment is below.

```
    "extensionProfile": {
        "extensions": [
            {
                "name": "CustomScriptExtension",
                "properties": {
                    "publisher": "Microsoft.Compute",
                    "type": "CustomScriptExtension",
                    "typeHandlerVersion": "1.9",
                    "autoUpgradeMinorVersion": true,
                    "settings": {
                        "fileUris": [
                            "[parameters('scriptUrl')]",
                            "[parameters('packageUrl')]"
                        ],
                        "commandToExecute": "[concat('powershell -ExecutionPolicy Unrestricted -File install_script.ps1 ', variables('packageName')]"
                    }
                }
            }
        ]
    }
```

Run deployment command.

```
az group deployment create -g $rg -n $vmssstag --template-file vmss_win_ext.json --parameters @vmss_win_ext.parameters.json
```

> This deployment method takes longer than deployment by custom image.

For troubleshooting, location of download files and log are located in `C:\Packages\Plugins\Microsoft.Compute.CustomScriptExtension\<version>\Downloads\1`

## Test HTTPS endpoint

```
curl -k https://<ip>/api/values

["value1","value2","Banana"]
```


## References

- [Custom Script Extension for Windows](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/custom-script-windows)
Reference for config HTTPS:
- [Secure a web server on a Windows on Azure](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/tutorial-secure-web-server)
- [Secure a web server on a Linux on Azure](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/tutorial-secure-web-server)