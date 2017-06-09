# Required initial setup
# Uploads MSI to S3 Bucket
Add-Type -Path "AWSSDK.dll"
. .\setup.ps1

$client=[Amazon.AWSClientFactory]::CreateAmazonS3Client($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)


echo $client.ListBuckets()
$log_dir = Get-Item "C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\Log"
$BUCKET_NAME="sqlserver-logs"

foreach ($file in $log_dir.GetFiles("*")) 
{
	echo "Uploading: "  $file.FullName
	$request = New-Object -TypeName Amazon.S3.Model.PutObjectRequest
	[void]$request.WithFilePath($file.fullname)
    [void]$request.WithBucketName($BUCKET_NAME)
    [void]$request.WithKey($file.name)
    [void]$client.PutObject($request)
}


$ok = read-host "Done  [return]"

