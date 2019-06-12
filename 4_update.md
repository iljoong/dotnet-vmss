# Update/upgrade app

## Rolling Update

By default, upgrade policy of VMSS is manual. You can change the policy to `rolling`. To enable rolling update you first need to setup healthprobe to VM instances.

```
az vmss show -g $rg -n $vmssprod --query upgradePolicy 

az vmss update -g $rg -n $vmssprod --query virtualMachineProfile.networkProfile.healthProbe --set "virtualMachineProfile.networkProfile.healthProbe.id=/subscriptions/XXXX-XXXX/resourceGroups/${rg}/providers/Microsoft.Network/loadBalancers/${elb}/probes/httpProbe"

az vmss update-instances -g $rg -n $vmssprod --instance-ids "*"

az vmss update -g $rg -n $vmssprod --query upgradePolicy --set upgradePolicy.mode="Rolling"
```

## Update new certificate without downtime

With rolling update, you can update new certificate without downtime. You can execute following CLI.

```
az vmss update -g test-dnc -n otnapi \
--set virtualMachineProfile.osProfile.secrets[0].vaultCertificates[0].certificateUrl="https://kv.vault.azure.net/secrets/certificateurl" \
--set virtualMachineProfile.extensionProfile.extensions[0].settings='{"fileUris": ["https://xxxx.blob.core.windows.net/package/script.ps1", "https://xxxx.blob.core.windows.net/package/appsettings.json"],"commandToExecute": "powershell -ExecutionPolicy Unrestricted -File script.ps1 -thumbprint _NEW_THUMBPRINT_"}'
```

### Troubleshooting

Login to one of VMSS instances and do following verifications

- To verify whether the new certifcate is installed correctly, run `dir Cert:\LocalMachine\My` to see new thumbprint
- To verify whether the new extension is executed correctly, go to `C:\Packages\Plugins\Microsoft.Compute.CustomScriptExtension\1.9.3\Downloads` and check log file in newly created folder


## VIP Swap

VMSS does not provide _VIP swap_ but you could manually swap VIP of VMSS LB. Following script swap IPs of VMSS LBs.

```
az network public-ip create -g $rg -n tempip --sku Standard 

tempip_id=$(az network public-ip show -g $rg -n tempip --query "id" -o tsv)
lb1_id=$(az network lb show -g $rg -n ${vmssprod}Elb --query "frontendIpConfigurations[].publicIpAddress.id" -o tsv)
lb2_id=$(az network lb show -g $rg -n ${vmssstag}Elb --query "frontendIpConfigurations[].publicIpAddress.id" -o tsv)

az network lb update -g $rg -n ${vmssprod}Elb --set "frontendIpConfigurations[0].publicIpAddress.id=$tempip_id"
az network lb update -g $rg -n ${vmssstag}Elb --set "frontendIpConfigurations[0].publicIpAddress.id=$lb1_id"
az network lb update -g $rg -n ${vmssprod}Elb --set "frontendIpConfigurations[0].publicIpAddress.id=$lb2_id"

az network public-ip delete -g $rg -n tempip
```

> Note that VIP swap takes >1 min and this causes >30 sec downtime.

Run [swaptest.ps1](./script/swaptest.ps1) while executing VIP Swap command.

As you can see, there is ~20 sec downtime wihle swapping.

```
01/30/2019 11:35:52 : ["value1","value2","Ice"]
01/30/2019 11:35:53 : ["value1","value2","Cat"]
01/30/2019 11:35:54 : ["value1","value2","Apple"]
01/30/2019 11:36:14 : ["value1","value2","Seven"]
01/30/2019 11:36:15 : ["value1","value2","One"]
01/30/2019 11:36:16 : ["value1","value2","Ten"]
```