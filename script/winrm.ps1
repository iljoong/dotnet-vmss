$hostName='ip'
$winrmPort = '23001'

# Get the credentials of the machine
$cred = Get-Credential

# Connect to the machine
$soptions = New-PSSessionOption -SkipCACheck -SkipCNCheck
Enter-PSSession -ComputerName $hostName -Port $winrmPort -Credential $cred -SessionOption $soptions -UseSSL


# Restart the web server so that system PATH updates take effect
net stop was /y
net start w3svc

# copy local file to remote dir
$session= New-PSSession -ComputerName $hostName -Port $winrmPort -Credential $cred -SessionOption $soptions -UseSSL
Copy-Item –Path C:\Users\ilkim\Downloads\work\dncbench\src\apiapp_httpsys.zip –Destination 'C:\Users\iljoong' –ToSession $session

# winrm then unzip
Expand-Archive apiapp.zip -DestinationPath C:\inetpub\wwwroot -Force

# edit remote file
psedit filename

netsh advfirewall firewall add rule name="APSNETCORE" dir=in action=allow protocol=TCP localport=5000

# .net core SDK 
curl.exe -s https://download.visualstudio.microsoft.com/download/pr/d4592a50-b583-434a-bcda-529e506a7e0d/b1fee3bb02e4d5b831bd6057af67a91b/dotnet-sdk-2.2.101-win-x64.exe -o $env:temp\dotnet-sdk-2.2.101-win-x64.exe
Start-Process $env:temp\dotnet-sdk-2.2.101-win-x64.exe -ArgumentList '/quiet' -Wait

# runtime
curl.exe -s https://download.visualstudio.microsoft.com/download/pr/48adfc75-bce7-4621-ae7a-5f3c4cf4fc1f/9a8e07173697581a6ada4bf04c845a05/dotnet-hosting-2.2.0-win.exe -o $env:temp\dotnet-hosting-2.2.0-win.exe 
Start-Process $env:temp\dotnet-hosting-2.2.0-win.exe  -ArgumentList '/quiet' -Wait