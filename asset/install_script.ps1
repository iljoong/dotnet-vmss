param (
    [string]$packagename,
    [string]$thumbprint
)

function logging($output)
{
    $time = Get-Date
    Write-Output "$time - $output" >> customscript.log
}

logging("start logging")

logging("installing IIS and .NET framework")

# Install IIS
Install-WindowsFeature Web-Server,Web-Asp-Net45,NET-Framework-Features

logging("installing .NET Core runtime")

# Install runtime: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2
curl.exe https://download.visualstudio.microsoft.com/download/pr/5ee633f2-bf6d-49bd-8fb6-80c861c36d54/caa93641707e1fd5b8273ada22009246/dotnet-hosting-2.2.1-win.exe -o $env:temp\dotnet-hosting-2.2.1-win.exe 
Start-Process $env:temp\dotnet-hosting-2.2.1-win.exe  -ArgumentList '/quiet' -Wait

logging("installing application package")

Expand-Archive $packagename -DestinationPath C:\inetpub\wwwroot -Force

logging("enabling IIS SSL")

# https://docs.microsoft.com/en-us/azure/virtual-machines/windows/tutorial-secure-web-server#configure-iis-to-use-the-certificate
New-WebBinding -Name "Default Web Site" -Protocol https -Port 443

if (Test-Path IIS:\SslBindings\0.0.0.0!443) {Remove-Item -Path IIS:\SslBindings\0.0.0.0!443}
Get-ChildItem cert:\LocalMachine\My\$thumbprint | New-Item -Path IIS:\SslBindings\!443

logging("restart IIS")

# Restart the web server so that system PATH updates take effect
net stop was /y
net start w3svc

logging("end logging")