#
# File: turbine_consumer_install.ps1
#


Add-Type -Path "AWSSDK.dll"
. .\setup.ps1
$ErrorActionPreference = 'Stop'

echo "Stop AspenConsumer"
$service = Get-Service AspenConsumer
if ($service.Status -eq "Running") 
{
    $service.Stop()
    $service.Refresh()
}
while ($service.Status -ne "Stopped") 
{
    echo wait $service.Status
    sleep 1
    $service.Refresh()
}
sleep 5;

$client=[Amazon.AWSClientFactory]::CreateAmazonS3Client($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)

#$ok = read-host "Update Installer[y|n]?"
$ok = "y"
if ($ok -ne "y") 
{
	echo "return"
	return;
}

echo "Delete Turbine directory"
try 
{
    Remove-Item -Recurse -Path "\Program Files\Turbine"
} catch 
{
   echo No Turbine Dir
}

$targetFile="TurbineCloudGateway.msi"
echo $client.ListBuckets()
echo "Uploading: "  $file.FullName
$request = New-Object -TypeName Amazon.S3.Model.GetObjectRequest
[void]$request.WithBucketName($BUCKET_NAME)
[void]$request.WithKey($targetFile)
echo "GetObject"
$response = $client.GetObject($request)
echo "Response" $response
[byte[]]$buffer = New-Object byte[] 4096

$targetDir=[System.Environment]::GetFolderPath("Desktop")
$targetFilePath= Join-Path $targetDir $targetFile

echo "Uninstall"
#$job = Start-Job -ScriptBlock { msiexec /quiet /x $targetFilePath }
#echo $job.ExitCode
#Receive-Job $job

msiexec /quiet /x $targetFilePath | Out-Host

echo "Save File to $targetFilePath"
[system.io.stream]$stream = [system.io.File]::OpenWrite($targetFilePath)

try {
	do {
		[int]$bytesRead = $response.ResponseStream.Read($buffer, 0, 4096)
		#echo "Write: $bytesRead"
		$stream.Write($buffer, 0, $bytesRead)
	} while ($bytesRead -ne 0)
	$stream.Flush()
} catch 
{
	echo "ERROR"
} finally
{
	echo "Done"
	$response.ResponseStream.close()
	$stream.close()
}

echo "Get the Shared Database IP ADDR"
$request = New-Object -TypeName Amazon.S3.Model.GetObjectRequest
[void]$request.WithBucketName($BUCKET_NAME)
[void]$request.WithKey("database")
echo "GetObject Database"
$response = $client.GetObject($request)
$sr = New-Object System.IO.StreamReader($response.ResponseStream)
$databaseIP = $sr.ReadToEnd().TrimEnd()

echo "Installing MSI"
msiexec /i $targetFilePath PROP.SQLSERVER="$databaseIP" ADDLOCAL="Feature.Common,Feature.AspenConsumer" 
AWS_REGION="https://ec2.us-west-1.amazonaws.com" AWS_TERMINATE_QUEUE="https://us-west-1.queue.amazonaws.com/754323349409/AspenConsumerTerminateQueueMS11"  AWS_NOTIFICATION_QUEUE="https://us-west-1.queue.amazonaws.com/754323349409/ConsumerShutdownNotificationMS11" AWS_BUCKET="turbine-ms11" AWS_ACCESS_KEY="AKIAIQY3GMFZ5U4JBI3Q" AWS_SECRET_KEY="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" AWS_LOG_QUEUE="https://us-west-1.queue.amazonaws.com/754323349409/AspenConsumerLog" | Out-Host
    
#echo "start AspenConsumer"
#$service.Start()
#$service.Refresh()
#while ($service.Status -ne "Running") 
#{
#    echo wait $service.Status
#    sleep 1
#    $service.Refresh()
#}


$ok = read-host "[return]"