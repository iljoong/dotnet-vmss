function logging($output)
{
    $time = Get-Date
    Write-Output "$time - $output" >> customscript.log
}

logging("start logging")
logging("enabling IIS SSL")

# https://docs.microsoft.com/en-us/azure/virtual-machines/windows/tutorial-secure-web-server#configure-iis-to-use-the-certificate
New-WebBinding -Name "Default Web Site" -Protocol https -Port 443
Get-ChildItem cert:\LocalMachine\My\_THUMBPRINT_ | New-Item -Path IIS:\SslBindings\!443

logging("end logging")