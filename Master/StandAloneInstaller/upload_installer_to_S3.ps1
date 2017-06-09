# Required initial setup
# Uploads MSI to S3 Bucket
Add-Type -Path "C:\Program Files (x86)\AWS SDK for .NET\bin\AWSSDK.dll"
. .\setup.ps1

$client=[Amazon.AWSClientFactory]::CreateAmazonS3Client($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)

#$ok = read-host "Update Installer[y|n]?"
$ok = "y"
if ($ok -ne "y") 
{
	echo "return"
	return;
}

$msi_dir = Get-Item "bin\Debug\"

#$files = Resolve-Path $consumer_dir/*
#$files = Get-ChildItem -Path "*.msi"

echo $client.ListBuckets()

foreach ($file in $msi_dir.GetFiles("*.msi")) 
{
	echo "Uploading: "  $file.FullName
	$request = New-Object -TypeName Amazon.S3.Model.PutObjectRequest
	[void]$request.WithFilePath($file.fullname)
    [void]$request.WithBucketName($BUCKET_NAME)
    [void]$request.WithKey($file.name)
    [void]$client.PutObject($request)
}

$ok = read-host "Done  [return]"