
$ErrorActionPreference = "Stop"
Start-Transcript -Path "C:\Users\Administrator\Desktop\turbine_EC2Config.log"
function InstallStuff([string]$ArgumentList) {
    $exit_code = (Start-Process -FilePath "msiexec.exe" -ArgumentList $ArgumentList -Wait -Passthru).ExitCode;
    if ($exit_code -ne 0) {
        Write-Host $exit_code Installation Failed for $ArgumentList;
        Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'FAILED Install' -Region us-west-2;
        exit $exit_code
    }
}

Function Install-RedisConnector() {
    $ArgumentList = "/lvx python.log /qn /i python-2.7.9.amd64.msi ADDLOCAL=ALL ALLUSERS=1 TARGETDIR=C:\Python27 ";
    InstallStuff $ArgumentList;
    $ArgumentList = "/lvx tortoise_svn.log /qn /i TortoiseSVN-1.8.6.25419-x64-svn-1.8.8.msi ADDLOCAL=ALL ALLUSERS=1 ";
    InstallStuff $ArgumentList;
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Starting TurbineWebAPIService' -Region us-west-2;
    Start-Service  -Name TurbineWebAPIService;
    $f = Get-ChildItem "C:\Program Files (x86)\foqus\foqus*\dist\foqus.exe"
    cd C:\Users\Administrator\Desktop;
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'foqus-service: configure xml' -Region us-west-2;
    Write-Host "TODO: Configure FOQUS-Service with parameter RedisIPAddr"
    [xml]$myXML = Get-Content FOQUS-Service\foqus-service.xml
    $myXML.service.executable = $f.FullName
    $myXML.Save("C:\Users\Administrator\Desktop\FOQUS-Service\foqus-service.xml")
    cd C:\Users\Administrator\Desktop\;
    Read-S3Object -BucketName turbine-downloads -Key logstash_formatter-0.5.14.zip -File logstash_formatter-0.5.14.zip
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Unzip logstash_formatter' -Region us-west-2;
    $shell_app=new-object -com shell.application
    $zip_file = $shell_app.namespace((Get-Location).Path + "\logstash_formatter-0.5.14.zip")
    $destination = $shell_app.namespace((Get-Location).Path)
    $destination.Copyhere($zip_file.items())
    cd C:\Users\Administrator\Desktop\logstash_formatter-0.5.14;
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'install logstash_formatter' -Region us-west-2;
    C:\Python27\python.exe setup.py install;
    C:\Python27\Scripts\pip.exe install adodbapi
    C:\Python27\Scripts\pip.exe install loggly-python-handler
}
Function Configure-TurbineLite-Logging() {
    $f = Get-ChildItem "C:\Program Files (x86)\Turbine\Lite\Clients\AspenSinterConsumerConsole.exe.config"
    [xml]$myXML = Get-Content $f.FullName
    $myXML.configuration.'system.diagnostics'.trace.listeners.add.name = "EC2LogListener"
    $myXML.configuration.'system.diagnostics'.trace.listeners.add.type = "System.Diagnostics.TextWriterTraceListener"
    $attr = $myXML.CreateAttribute("initializeData")
    $attr.Value = "Z:\AspenSinterConsumerConsole.log"
    $myXML.configuration.'system.diagnostics'.trace.listeners.add.Attributes.append($attr);
    $myXML.Save($f.FullName)
}
cd C:\Users\Administrator\Desktop;
$QUEUE_URL = "https://sqs.us-west-2.amazonaws.com/754323349409/DeploymentUpdate";
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Starting' -Region us-west-2;
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Firewall off' -Region us-west-2;
netsh advfirewall set allprofiles state off;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Downloading' -Region us-west-2;
Read-S3Object -BucketName turbine-downloads -Key SSCERuntime_x64-ENU.exe -File SSCERuntime_x64-ENU.exe;
Read-S3Object -BucketName turbine-downloads -Key python-2.7.9.amd64.msi -File python-2.7.9.amd64.msi;
Read-S3Object -BucketName turbine-downloads -Key TortoiseSVN-1.8.6.25419-x64-svn-1.8.8.msi -File TortoiseSVN-1.8.6.25419-x64-svn-1.8.8.msi;
Read-S3Object -BucketName turbine-downloads -Key jenkins/TurbineLite.msi -File TurbineLite.msi;
Read-S3Object -BucketName turbine-downloads -Key jenkins/SimSinterInstaller.msi -File SimSinterInstaller.msi;
Read-S3Object -BucketName turbine-downloads -Key jenkins/foqus_installer.msi -File foqus_installer.msi;
Read-S3Object -BucketName turbine-downloads -Key Puppet/puppet-3.8.4-x64.msi -File puppet-3.8.4-x64.msi;
Read-S3Object -BucketName turbine-downloads -KeyPrefix FOQUS-Service -Folder FOQUS-Service;
Read-S3Object -BucketName turbine-downloads -KeyPrefix Turbine-Lite-Service -Folder Turbine-Lite-Service;
Read-S3Object -BucketName turbine-downloads -Key Consumer/turbine_consumer_foqus_service_cleaner.xml -File C:\FOQUS-Service\turbine_consumer_foqus_service_cleaner.xml;
Read-S3Object -BucketName turbine-downloads -Key Consumer/turbine_consumer_foqus_service_cleaner.ps1 -File C:\FOQUS-Service\turbine_consumer_foqus_service_cleaner.ps1;
(Start-Process -FilePath "FOQUS-Service\nssm.exe" -ArgumentList "install foqus C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe .\foqus-service.ps1" -Wait -Passthru).ExitCode;
(Start-Process -FilePath "FOQUS-Service\nssm.exe" -ArgumentList "set foqus AppDirectory C:\Users\Administrator\Desktop\FOQUS-Service" -Wait -Passthru).ExitCode;
(Start-Process -FilePath "FOQUS-Service\nssm.exe" -ArgumentList "start foqus" -Wait -Passthru).ExitCode;
(Start-Process -FilePath "FOQUS-Service\nssm.exe" -ArgumentList "install TurbineLiteService C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe .\turbine-lite-service.ps1" -Wait -Passthru).ExitCode;
(Start-Process -FilePath "FOQUS-Service\nssm.exe" -ArgumentList "set TurbineLiteService AppDirectory C:\Users\Administrator\Desktop\Turbine-Lite-Service" -Wait -Passthru).ExitCode;
(Start-Process -FilePath "FOQUS-Service\nssm.exe" -ArgumentList "start TurbineLiteService" -Wait -Passthru).ExitCode;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Installing SQLCompact' -Region us-west-2;
(Start-Process -FilePath 'C:\\Users\\Administrator\\Desktop\\SSCERuntime_x64-ENU.exe' -ArgumentList '/i /qn /l Sql_server_compact_4.0.log' -Wait -Passthru).ExitCode;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Installing SimSinter' -Region us-west-2;
$ArgumentList = "/lvx SimSinterLog.txt /qn /i SimSinterInstaller.msi";
InstallStuff $ArgumentList;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Installing FOQUS' -Region us-west-2;
$ArgumentList = "/lvx FOQUS.txt /qn /i foqus_installer.msi";
InstallStuff $ArgumentList;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Installing TurbineLite' -Region us-west-2;
$ArgumentList = "/lvx TurineLiteLog.txt /qn /i TurbineLite.msi";
InstallStuff $ArgumentList;
Configure-TurbineLite-Logging;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Installing PuppetAgent' -Region us-west-2;
$InstanceID=(New-Object System.Net.WebClient).DownloadString("http://169.254.169.254/latest/meta-data/instance-id")
$ArgumentList = "/lvx PuppetAgent.txt /qn /norestart /i puppet-3.8.4-x64.msi PUPPET_MASTER_SERVER=10.0.0.27 PUPPET_AGENT_CERTNAME=$InstanceID.internal";
InstallStuff $ArgumentList;
& 'C:\Program Files\Puppet Labs\Puppet\bin\puppet.bat' agent -t --verbose --debug;
cd C:\Users\Administrator\Desktop;
Write-Host KeyName: ccsi-west2
Write-Host RedisIPAddr: 10.0.0.10
Install-RedisConnector
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Rebooting' -Region us-west-2;
Write-Host End of Script
shutdown -t 10 -r -f;
