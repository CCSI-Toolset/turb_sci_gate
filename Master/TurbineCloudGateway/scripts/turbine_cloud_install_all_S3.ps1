#
# File:  turbine_cloud_install_all_S3.ps1
#
# Author: Joshua Boverhof
# See LICENSE.md for License and Copyright details

Add-Type -Path "AWSSDK.dll"
. .\setup.ps1

$client=[Amazon.AWSClientFactory]::CreateAmazonS3Client($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)

#$ok = read-host "Update Installer[y|n]?"
$ok = "y"
if ($ok -ne "y") 
{
	echo "return"
	return;
}
echo "Stopping IIS"
$job = Start-Job { iisreset /STOP }
Wait-Job $job
Receive-Job $job

echo "Delete Turbine directory"
Remove-Item -Recurse -Path "\Program Files\Turbine"


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
#$job = Start-Job { msiexec /x $targetFilePath }
#Wait-Job $job
#Receive-Job $job
msiexec /x $targetFilePath | Out-Host

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

echo "Get the Orchestrator AMI"
$request = New-Object -TypeName Amazon.S3.Model.GetObjectRequest
[void]$request.WithBucketName("turbine-ms11")
[void]$request.WithKey("ami")
echo "GetObject Database"
$response = $client.GetObject($request)
$sr = New-Object System.IO.StreamReader($response.ResponseStream)
$myAMI = $sr.ReadToEnd().TrimEnd()



echo "Installing MSI"
#msiexec /i $targetFilePath PROP.AMI="$myAMI" ADDLOCAL="ALL"| Out-Host
msiexec /i $targetFilePath ADDLOCAL="Feature.WebDatabase"| Out-Host

#echo "Configure Bucket on AWS S3"
#$request = New-Object -TypeName Amazon.S3.Model.PutObjectRequest
#[void]$request.WithBucketName("turbine-ms11")
#[void]$request.WithKey("config")
#[void]$request.WithContentBody("{}")
#echo "GetObject"
#$response = $client.GetObject($request)


echo "Starting IIS"
iisreset /START | Out-Host


$ok = read-host "[return]"