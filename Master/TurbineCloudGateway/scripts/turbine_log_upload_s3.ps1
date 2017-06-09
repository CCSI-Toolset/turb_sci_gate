#
# File: turbine_log_upload_s3.ps1
#    Uploads logs in Turbine directory to S3 turbine-downloads bucket
#
#  TODO: Configure via installer

$ErrorActionPreference='Stop'

Function upload_logs($LOG_FILE, $NAME, $TimeStamp)
{
    $InstanceID=(New-Object System.Net.WebClient).DownloadString("http://169.254.169.254/latest/meta-data/instance-id")
    $BUCKET="turbine-logs"


    $PROCESS = Get-Process -Name foqus -ErrorAction "Ignore"
    $KEY="Logs/ec2_$($InstanceID)_$($NAME)"
	$KEY="Logs/ec2_$($InstanceID)/$($TimeStamp)/ec2_$($InstanceID)_$($NAME)"
    if ( $PROCESS -ne $null )
    {
        Write-Host "PROCESS $($PROCESS.Id)"
        $KEY="Logs/ec2_$($InstanceID)/ec2_$($InstanceID)_$($PROCESS.ProcessName)_$($PROCESS.Id)/$($TimeStamp)/ec2_$($InstanceID)_$($PROCESS.ProcessName)_$($PROCESS.Id)_$($NAME)"
    }
    
    Write-Host $LOG_FILE
    Copy-Item $LOG_FILE "temp.log"
    if ( Test-Path $LOG_FILE ) 
    {
    	Write-S3Object -BucketName $BUCKET -File "temp.log" -Key $KEY
    }
}

$TimeStamp = Get-Date -Format s

$LOG_FILE='C:\Users\Administrator\Desktop\FOQUS-Service\logs\foqus.log'
upload_logs $LOG_FILE 'foqus-service.out.log' $TimeStamp
#$LOG_FILE='C:\Program Files (x86)\Turbine\Lite\Data\TurbineCompactDatabase.sdf'
#upload_logs $LOG_FILE 'TurbineCompactDatabase.sdf' $TimeStamp
$LOG_FILE='Z:\HydroLog.txt'
upload_logs $LOG_FILE 'HydroLog.log' $TimeStamp
$LOG_FILE='Z:\AspenSinterConsumerConsole.log'
upload_logs $LOG_FILE 'AspenSinterConsumerConsole.log' $TimeStamp


Function consumer_logs()
{

    # NOTE: Problem Windows wont open this file for reading with Stream for some reason...
    $LOG_FILE='C:\Program Files (x86)\Turbine\Gateway\Logs\AspenSinterConsumerWindowsServiceLog.txt.bk'
    Copy-Item 'C:\Program Files (x86)\Turbine\Gateway\Logs\AspenSinterConsumerWindowsServiceLog.txt' $LOG_FILE

    $DBNAME="Turbine"
    $SQL_USER="uturbineX"
    $SQL_PASSWORD="X3n0F0b3"
    $DBHOST="localhost"
    $PROCESSID=$ASPEN.Id
    $conn = new-object system.data.SqlClient.SQLConnection("Data Source=$DBHOST;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
    $QUERY="SELECT Id FROM dbo.JobConsumers WHERE instance='$instanceID' and status='up' and processId='$PROCESSID'"
    echo $QUERY
    $cmd = new-object system.data.sqlclient.sqlcommand($QUERY, $conn);
    $conn.Open();
    $reader = $cmd.ExecuteReader()
    $consumerID=""
    $inc=0

    if($reader.Read())
    {
        $consumerID=$reader[0]
        if ( Test-Path $LOG_FILE ) 
        {
            $KEY="Logs/Consumer/$instanceID/$consumerID" + ".txt"
	        Write-S3Object -BucketName $BUCKET -File $LOG_FILE -Key $KEY
        }
    }
    else 
    {
        echo "No Consumers Running on instance"
    }

    Remove-Item $LOG_FILE

    while($reader.Read()) 
    {
        $consumerID=$reader[0]
        $KEY="Logs/Consumer/error/$consumerID" + ".txt"
	    Write-S3Object -BucketName $BUCKET -Content "Duplicate Consumer in status up and processId $ASPEN.Id for instance $instanceID" -Key $KEY
    }


    $conn.Close()
}

