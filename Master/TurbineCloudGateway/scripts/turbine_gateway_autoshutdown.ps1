#
# File: turbine_gateway_autoshutdown.ps1
#    Checks queues and ec2 instances for activity, if none it calls shutdown
#
#  TODO: Configure via installer
Add-Type -Path "C:\Program Files (x86)\AWS Tools\PowerShell\AWSPowerShell\AWSSDK.dll"
$ErrorActionPreference='Stop'
. .\setup.ps1
$KEEPALIVE_QUEUE_URL = "https://sqs.us-west-1.amazonaws.com/754323349409/UQGatewayKeepAlive"

$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT COUNT(*) FROM dbo.Jobs WHERE state='submit' OR state='setup' OR state='running'", $conn);
$conn.Open();
$reader = $cmd.ExecuteReader()
$count = 0;
$queue_count = 0;
$localID = Invoke-WebRequest http://169.254.169.254/latest/meta-data/instance-id

$client=[Amazon.AWSClientFactory]::CreateAmazonSQSClient()


# Call Read before accessing data.
while ($reader.Read())
{
	$count=$reader[0]
	$request = New-Object -TypeName Amazon.SQS.Model.GetQueueAttributesRequest
	$request.WithAttributeName(@("ApproximateNumberOfMessages"))
	$request.QueueUrl = $SUBMIT_QUEUE_URL
	$response = $client.GetQueueAttributes($request)
	$queue_count = $response.GetQueueAttributesResult.ApproximateNumberOfMessages
	echo "QUEUE LENGTH: " $queue_count
	echo "JOB COUNT: " $count
	break;
}
$reader.Close();

if ($queue_count -gt 0 -or $count -gt 0) 
{
    echo "Queue is not empty and/or Jobs available"
    return;
}

# Check if Consumers are up besides local
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT COUNT(*) FROM dbo.JobConsumers WHERE status='up' and instance!='$localID'", $conn);
$reader = $cmd.ExecuteReader()
$count = 0;
while ($reader.Read())
{
	$count=$reader[0]
	echo "CONSUMER COUNT: " $count
	break;
}
$reader.Close();

# Queue is 1 
if ($count -gt 0)
{
    echo "Queue is not empty and/or Jobs available"
    return;
}

echo "No Remote Consumers ( not $localID )"

# Check KeepAlive
$request = New-Object -TypeName Amazon.SQS.Model.GetQueueAttributesRequest
$request.WithAttributeName(@("ApproximateNumberOfMessages"))
$request.QueueUrl = $KEEPALIVE_QUEUE_URL
$response = $client.GetQueueAttributes($request)
$queue_count = $response.GetQueueAttributesResult.ApproximateNumberOfMessages

if ($queue_count -eq 0)
{
    echo "Shutdown Gateway"
    stop-computer -Force
    return;
}

echo "KeepAlive (count: $queue_count)"