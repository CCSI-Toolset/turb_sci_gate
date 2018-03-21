#
# File:  ec2-describe-instances.ps1
# Author: Joshua Boverhof
# See LICENSE.md for License and Copyright details

Add-Type -Path "AWSSDK.dll"

$ErrorActionPreference='Stop'
. .\setup.ps1

$config = New-Object -TypeName Amazon.EC2.AmazonEC2Config
$config.WithServiceURL($REGION)
$client=[Amazon.AWSClientFactory]::CreateAmazonEC2Client($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID,$config);

$request = New-Object -TypeName Amazon.EC2.Model.DescribeInstancesRequest
$response = $client.DescribeInstances($request)
$instances = New-Object System.Collections.ArrayList

echo "====================="
foreach ($reservation in $response.DescribeInstancesResult.Reservation)
{ 
    foreach($instance in $reservation.RunningInstance)
    {
		echo $instance
        [void]$instances.Add($instance)
    }
}



$fields = @{Expression={$_.InstanceId};Label="Instance ID";width=25}, `
@{Expression={$_.ImageId};Label="Image ID";width=15}, `
@{Expression={$_.InstanceState.Name};Label="Status";width=40}
 
 
$instances | format-table $fields