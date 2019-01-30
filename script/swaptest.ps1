<#
Test SWAP
#>
$apiurl = "http://ip/api/values"

Invoke-WebRequest -Uri $apiurl | Write-Host

while ($true)
{
    $r = Invoke-WebRequest -Uri $apiurl

    Write-Host "$(Get-Date) : $($r.content)"

    Start-Sleep -s 1

}
