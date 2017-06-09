Add-Type -Path "C:\Program Files (x86)\AWS Tools\PowerShell\AWSPowerShell\AWSSDK.dll"
#Set-PSDebug -Trace 2
$DebugPreference = "continue"
$ErrorActionPreference='Stop'
Start-Transcript -Path "C:\Program Files (x86)\Turbine\Gateway\Logs\turbine_database_monitor.log"
. .\setup.ps1

#Write-Debug "start debugging"
$InstanceID=(New-Object System.Net.WebClient).DownloadString("http://169.254.169.254/latest/meta-data/instance-id")
$region = "us-west-1"
$namespace = "TurbineScienceGateway"

Write-Host "turbine_database_monitor $InstanceID`r`n"

$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("select convert(decimal(12,2),round(a.size/128.000,2)) as FileSizeMB from dbo.sysfiles a where fileid = 1; ", $conn)
$conn.Open();
$reader = $cmd.ExecuteReader()
if ($reader.Read()) 
{
    $filesizeMB = $reader.GetValue(0);

    $dat = New-Object Amazon.CloudWatch.Model.MetricDatum;
    $dat.Timestamp = (Get-Date).ToUniversalTime() 
    $dat.MetricName = "filesize"
    $dat.Unit = "Megabytes"
    $dat.Value = $filesizeMB;

    $d1 = New-Object Amazon.CloudWatch.Model.Dimension;
    $d1.Name  = "milestone"
    $d1.Value = 16;
    $d2 = New-Object Amazon.CloudWatch.Model.Dimension;
    $d2.Name  = "gateway"
    $d2.Value = "UQ";


    $l = New-Object 'System.Collections.Generic.List[Amazon.CloudWatch.Model.Dimension]'
    $l.Add($d1);
    $l.Add($d2);

    $dat.Dimensions = $l;

    Write-CWMetricData -Region $region -Namespace $namespace -MetricData $dat
}


$reader.Close();
$conn.Close();


$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("select convert(decimal(12,2),round((a.size-fileproperty(a.name,'SpaceUsed'))/128.000,2)) as FreeSpaceMB from dbo.sysfiles a where fileid = 1; ", $conn)
$conn.Open();
$reader = $cmd.ExecuteReader()
$reader.Read()
$freespaceMB = $reader.GetValue(0);
$reader.Read()
$reader.Close();
$conn.Close();

$dat = New-Object Amazon.CloudWatch.Model.MetricDatum;
$dat.Timestamp = (Get-Date).ToUniversalTime() 
$dat.MetricName = "freespace"
$dat.Unit = "Megabytes"
$dat.Value = $freespaceMB;

$d1 = New-Object Amazon.CloudWatch.Model.Dimension;
$d1.Name  = "milestone"
$d1.Value = 16;
$d2 = New-Object Amazon.CloudWatch.Model.Dimension;
$d2.Name  = "gateway"
$d2.Value = "UQ";


$l = New-Object 'System.Collections.Generic.List[Amazon.CloudWatch.Model.Dimension]'
$l.Add($d1);
$l.Add($d2);

$dat.Dimensions = $l;

Write-CWMetricData -Region $region -Namespace $namespace -MetricData $dat




$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("select convert(decimal(12,2),round(fileproperty(a.name,'SpaceUsed')/128.000,2)) as SpaceUsedMB from dbo.sysfiles a where fileid = 1; ", $conn)
$conn.Open();
$reader = $cmd.ExecuteReader()
$reader.Read()
$spaceusedMB = $reader.GetValue(0);
$reader.Read()
$reader.Close();
$conn.Close();


$dat = New-Object Amazon.CloudWatch.Model.MetricDatum;
$dat.Timestamp = (Get-Date).ToUniversalTime() 
$dat.MetricName = "spaceused"
$dat.Unit = "Megabytes"
$dat.Value = $spaceusedMB;

$d1 = New-Object Amazon.CloudWatch.Model.Dimension;
$d1.Name  = "milestone"
$d1.Value = 16;
$d2 = New-Object Amazon.CloudWatch.Model.Dimension;
$d2.Name  = "gateway"
$d2.Value = "UQ";


$l = New-Object 'System.Collections.Generic.List[Amazon.CloudWatch.Model.Dimension]'
$l.Add($d1);
$l.Add($d2);

$dat.Dimensions = $l;

Write-CWMetricData -Region $region -Namespace $namespace -MetricData $dat






Write-Host "FileSizeMB = $filesizeMB`r`n"
Write-Host "FreespaceMB = $freespaceMB`r`n"
Write-Host "spaceusedMB = $spaceusedMB`r`n"


# CONSUMERS
$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT COUNT(*) FROM dbo.JobConsumers  WHERE Status='up'", $conn)
$conn.Open();
$reader = $cmd.ExecuteReader()
$count = $reader.FieldCount


$total = 0;
if ($reader.Read())
{
	$total = $reader.GetValue(0);
}


$dat = New-Object Amazon.CloudWatch.Model.MetricDatum;
$dat.Timestamp = (Get-Date).ToUniversalTime() 
$dat.MetricName = "Consumers"
$dat.Unit = "Count"
$dat.Value = $total;

$d1 = New-Object Amazon.CloudWatch.Model.Dimension;
$d1.Name  = "milestone"
$d1.Value = 16;
$d2 = New-Object Amazon.CloudWatch.Model.Dimension;
$d2.Name  = "gateway"
$d2.Value = "UQ";


$l = New-Object 'System.Collections.Generic.List[Amazon.CloudWatch.Model.Dimension]'
$l.Add($d1);
$l.Add($d2);

$dat.Dimensions = $l;

Write-CWMetricData -Region $region -Namespace $namespace -MetricData $dat


# ERRORS, SUCCESS, ETC
foreach ($jobstate in @("success","error","submit", "running", "setup") )
{
    $query = "SELECT COUNT(*) FROM dbo.Jobs  WHERE State='$jobstate'";
    Write-Host "QUERY: $query`r`n"
    $conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
    $cmd = new-object system.data.sqlclient.sqlcommand($query, $conn)
    $conn.Open();
    $reader = $cmd.ExecuteReader()
    $count = $reader.FieldCount
    $consumer_list = New-Object “System.Collections.Generic.List[String]”


    $total = 0;
    if ($reader.Read())
    {
	    $total = $reader.GetValue(0);
    }
    else 
    {
        Write-Host "Reader read failed for $jobstate`r`n"
        continue;
    }


    $dat = New-Object Amazon.CloudWatch.Model.MetricDatum;
    $dat.Timestamp = (Get-Date).ToUniversalTime() 
    $dat.MetricName = $jobstate;
    $dat.Unit = "Count"
    $dat.Value = $total;

    $d1 = New-Object Amazon.CloudWatch.Model.Dimension;
    $d1.Name  = "milestone"
    $d1.Value = 16;
    $d2 = New-Object Amazon.CloudWatch.Model.Dimension;
    $d2.Name  = "gateway"
    $d2.Value = "UQ";


    $l = New-Object 'System.Collections.Generic.List[Amazon.CloudWatch.Model.Dimension]'
    $l.Add($d1);
    $l.Add($d2);

    $dat.Dimensions = $l;
    Write-Host "Write-CWMetricData: $jobstate $total`r`n"
    Write-CWMetricData -Region $region -Namespace $namespace -MetricData $dat
}

