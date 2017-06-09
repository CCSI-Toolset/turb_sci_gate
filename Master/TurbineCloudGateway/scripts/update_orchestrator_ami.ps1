# Required initial setup
# Downloads MSI from S3 Bucket
Add-Type -Path "AWSSDK.dll"
. .\setup.ps1
echo "Stop AspenConsumer"
$service = Get-Service TurbineOrchestrator
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
echo $client.ListBuckets()

[byte[]]$buffer = New-Object byte[] 4096

echo "Get the Orchestrator AMI"
$request = New-Object -TypeName Amazon.S3.Model.GetObjectRequest
[void]$request.WithBucketName("turbine-ms11")
[void]$request.WithKey("ami")
echo "GetObject Database"
$response = $client.GetObject($request)
$sr = New-Object System.IO.StreamReader($response.ResponseStream)
$myAMI = $sr.ReadToEnd().TrimEnd()
echo "Update AMI: $myAMI"

$f=Get-Item "C:\Program Files (x86)\Turbine\Gateway\Bin\EC2AutoScalingWindowsService.exe.config"
Copy-Item $f "EC2AutoScalingWindowsService.exe.config.bk"
$xml=[xml](Get-Content $f)
# NOTE HACK ASSUMED ORDER
$node=$xml.GetElementsByTagName("property").ItemOf(0)
echo "PROPERTY $node.value"
$node.value=$myAMI
echo "Update PROPERTY $node.value"
$xml.save("C:\Program Files (x86)\Turbine\Gateway\Bin\EC2AutoScalingWindowsService.exe.config")

echo "start TurbineOrchestrator"
$service.Start()
$service.Refresh()
while ($service.Status -ne "Running") 
{
    echo wait $service.Status
    sleep 1
    $service.Refresh()
}
$ok = read-host "[return]"