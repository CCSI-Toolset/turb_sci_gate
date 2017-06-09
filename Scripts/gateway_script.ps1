
$ErrorActionPreference = "Stop"
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
cd C:\Users\Administrator\Desktop;
Start-Transcript -Path "C:\Users\Administrator\Desktop\turbine_EC2Config.log"
$QUEUE_URL = "https://sqs.us-west-2.amazonaws.com/754323349409/DeploymentUpdate";
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Starting' -Region us-west-2;
[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo");
[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.SqlWmiManagement");
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force
function InstallStuff([string]$ArgumentList) {
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Installing' -Region us-west-2;
    Write-Host "==== InstallStuff"
    $exit_code = (Start-Process -FilePath "msiexec.exe" -ArgumentList $ArgumentList -Wait -Passthru).ExitCode;
    if ($exit_code -ne 0) {
        Write-Host $exit_code Installation Failed for $ArgumentList;
        Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody "$exit_code Installation Failed for $ArgumentList" -Region us-west-2;
        exit $exit_code
    }
}

Function Install-Python-Env() {
    Write-Host "==== Installing Python"
    Read-S3Object -BucketName turbine-downloads -Key python-2.7.9.amd64.msi -File python-2.7.9.amd64.msi;
    $ArgumentList = "/lvx python.log /qn /i python-2.7.9.amd64.msi ADDLOCAL=ALL ALLUSERS=1 TARGETDIR=C:\Python27 ";
    InstallStuff $ArgumentList;
    C:\Python27\Scripts\pip.exe install redis
    C:\Python27\Scripts\pip.exe install adodbapi
    C:\Python27\Scripts\pip.exe install logstash_formatter
    C:\Python27\Scripts\pip.exe install loggly-python-handler
}

Function Bind-TurbineSecureWebSite()
{
    Write-Host "==== Bind-TurbineSecureWebSite"
    import-module webadministration
    [System.Security.Cryptography.X509Certificates.X509Store]$store = get-item cert:\LocalMachine\MY
    $store.Open("ReadOnly")
    if ($store.Certificates.Count -ne 1) {
        Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Certificate Error: Expecting 1' -Region us-west-2;
        Write-Error "Certificate Count is not one"
        exit 1;
    }
    $cert = $store.Certificates[0]
    $thumbprint = $cert.Thumbprint;
    $guid = [guid]::NewGuid().ToString("B")
    Write-Host "Thumbprint: $thumbprint"
    netsh http add sslcert ipport=0.0.0.0:8080 certhash="$thumbprint" certstorename=MY appid="$guid"
    Set-ItemProperty 'IIS:\Sites\Turbine Secure Web Site' -name logfile.directory Z:\IISLogs
    iisreset
}
Function Initialize-TurbineSecureWebSite()
{
    Write-Host "==== Initialize-TurbineSecureWebSite"
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    $auth = 'Basic ' + [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes('nouser:password'))
    $uri = 'https://localhost:8080/Turbine/application/'
    $req = New-Object System.Net.WebClient
    $req.Headers.Add('Authorization', $auth )
    $req.DownloadString($uri)
}
Function Install-Certificate()
{
    Write-Host "==== Install-Certificate"
    Read-S3Object -Region us-east-1 -BucketName turbine-downloads -Key turbine.pkcs12 -File C:\Users\Administrator\Desktop\turbine.pkcs12;
    Read-S3Object -Region us-east-1 -BucketName turbine-downloads -Key ca.pem -File C:\Users\Administrator\Desktop\ca.pem;
    Certutil.exe -addstore "Root" C:\Users\Administrator\Desktop\ca.pem
    Certutil.exe -p TXmiCAor2000 -importpfx C:\Users\Administrator\Desktop\turbine.pkcs12
}
Function Install-WebServer()
{
    Write-Host "==== Install-WebServer"
    Import-Module servermanager;
    Add-WindowsFeature Web-Server;
    Add-WindowsFeature Web-App-Dev;
    C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe /i;
    iisreset
}
Function Init-TurbineClusterDatabase()
{
    Write-Host "==== Init-TurbineClusterDatabase"
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    $encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes('user:pass'))
    $basicAuthValue = "Basic $encodedCreds"
    $Headers = @{ Authorization = $basicAuthValue }
    try {
         $result = Invoke-WebRequest -Uri "https://localhost:8080/Turbine/applications" -Headers $Headers
    } catch{
         Write-Host 'Caught the exception';
         Write-Host $Error[0].Exception;
    }
}
Function Install-CredentialManager()
{
    Write-Host "==== Install-CredentialManager"
    New-Item -ErrorAction Ignore -ItemType directory -Path 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator\Tasks';
    SCHTASKS /CREATE /RU Administrator /RP XR80yz125  /xml 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator\Tasks\turbine_credential_manager.xml' /TN 'TurbineCredentialManager';
    #$exit_code = (Start-Process -FilePath "C:\Program Files (x86)\Turbine\Gateway\Orchestrator\Tasks\turbine_auth_update_users.ps1" -ArgumentList "init" -Wait -Passthru).ExitCode;
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Rebooting; Credential Manager exit_code $exit_code' -Region us-west-2;
}
Function Install-Redis-Connector()
{
    Write-Host "==== Install-Redis-Connector"
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Create RedisConnector' -Region us-west-2;
    & 'C:\Program Files\TortoiseSVN\bin\svn.exe' 'co' 'https://svn.acceleratecarboncapture.org/svn/projects/turb_hydro/trunk/' 'RedisConnector'  '--username' 'aaelbashandy' '--password' '#Berkeley456123#' 
    SCHTASKS /CREATE /RU Administrator /RP XR80yz125  /xml 'C:\Users\Administrator\Desktop\RedisConnector\Hydro\turbine_cluster_task.xml' /TN 'HydroTurbineCluster';
}
Function Install-Loggly-Collector()
{
    Write-Host "==== Install-Loggly-Collector"
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Install Loggly Collector' -Region us-west-2;
    wmic computersystem where name="%COMPUTERNAME%"  call rename name='turbine-gateway-dev'
    Read-S3Object -BucketName turbine-downloads -Prefix 'loggly' -Directory loggly
    $ArgumentList = "/lvx nxlog.log /qn /i nxlog-ce-2.9.1347.msi";
    InstallStuff $ArgumentList;
    Copy-Item loggly\nxlog.conf  C:\Program Files (x86)\nxlog\conf
}
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Check SQLServer Named Pipes and TCP' -Region us-west-2;
$smo = 'Microsoft.SqlServer.Management.Smo.'
$wmi = new-object ($smo + 'Wmi.ManagedComputer')
$server = $env:computername
$singleWmi = $wmi | where {$_.Name -eq $server}
$instance = "MSSQLSERVER"
$protocol = 'NP'
$protocol = $singleWmi.GetSmoObject("ManagedComputer[@Name='$server']/ServerInstance[@Name='$instance']/ServerProtocol[@Name='$protocol']");
if (!$protocol.IsEnabled) {
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Enable Named Pipes' -Region us-west-2;
    $protocol.IsEnabled = $True;
    $protocol.Alter()
}
$protocol = 'TCP'
$protocol = $singleWmi.GetSmoObject("ManagedComputer[@Name='$server']/ServerInstance[@Name='$instance']/ServerProtocol[@Name='$protocol']");
if (!$protocol.IsEnabled) {
    Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Enable TCP' -Region us-west-2;
    $protocol.IsEnabled = $True;
    $protocol.Alter()
}
NET STOP $instance /y
NET START $instance /y
Install-WebServer
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'Downloading' -Region us-west-2;
Read-S3Object -BucketName turbine-downloads -Key SSCERuntime_x64-ENU.exe -File SSCERuntime_x64-ENU.exe;
Read-S3Object -BucketName turbine-downloads -Key jenkins/FOQUS_bundle.exe -File FOQUS_bundle.exe;
Read-S3Object -BucketName turbine-downloads -Key TortoiseSVN-1.8.6.25419-x64-svn-1.8.8.msi -File TortoiseSVN-1.8.6.25419-x64-svn-1.8.8.msi;
Read-S3Object -BucketName turbine-downloads -Key applications.sql -File applications.sql;
Read-S3Object -BucketName turbine-downloads -Key redis-2.4.6-setup-64-bit.exe -File redis-2.4.6-setup-64-bit.exe;
Read-S3Object -BucketName turbine-downloads -Key applications.sql -File applications.sql;
$ArgumentList = "/lvx TortoiseSVN.log /qn /i TortoiseSVN-1.8.6.25419-x64-svn-1.8.8.msi ADDLOCAL=ALL";
InstallStuff $ArgumentList;
(Start-Process -FilePath 'C:\Users\Administrator\Desktop\SSCERuntime_x64-ENU.exe' -ArgumentList '/i /qn /l Sql_server_compact_4.0.log' -Wait -Passthru).ExitCode;
(Start-Process -FilePath 'C:\Users\Administrator\Desktop\FOQUS_bundle.exe' -ArgumentList '-q -s -l FOQUS_bundle.txt ADDLOCAL="Feature.Web,Feature.Database,Feature.Admin.Utilities" PROP.SQL_LOGIN=uturbineX PROP.SQL_PASSWORD=X3n0F0b3 PROP.TURBINE_USER=test PROP.TURBINE_PASSWORD=jd8fae34JRB' -Wait -Passthru).ExitCode;
New-Item -Path 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator' -Name 'Tasks' -ItemType 'directory';
Read-S3Object -BucketName turbine-downloads -Prefix 'Orchestrator/UQ' -Directory 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator\Tasks'
NET STOP $instance /y
NET START $instance /y
Install-Python-Env;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody 'turn off firewall' -Region us-west-2;
netsh advfirewall set allprofiles state off;
Send-SQSMessage -QueueUrl $QUEUE_URL -MessageBody  'Binding $binding' -Region us-west-2;
Install-Certificate
Bind-TurbineSecureWebSite
Init-TurbineClusterDatabase
sqlcmd -d Turbine -i applications.sql
Install-Redis-Connector
Install-CredentialManager
SCHTASKS /CREATE /RU Administrator /RP XR80yz125  /xml 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator\Tasks\turbine_database_monitor.xml' /TN 'turbine_database_monitor';
shutdown -t 10 -r -f;
