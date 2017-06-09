# Required initial setup
# Uploads MSI to S3 Bucket
Add-Type -Path "AWSSDK.dll"
. .\setup.ps1

$client=[Amazon.AWSClientFactory]::CreateAmazonS3Client($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)


echo $client.ListBuckets()
$msi_dir = Get-Item .

foreach ($file in $msi_dir.GetFiles("*.zip")) 
{
	echo "Uploading: "  $file.FullName
	$request = New-Object -TypeName Amazon.S3.Model.PutObjectRequest
	[void]$request.WithFilePath($file.fullname)
    [void]$request.WithBucketName($BUCKET_NAME)
    [void]$request.WithKey($file.name)
    [void]$client.PutObject($request)
}


$ok = read-host "Done  [return]"

