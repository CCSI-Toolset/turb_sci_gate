#
# File:  consumer_sweeper.ps1
#    Searches for running instances in specified security group, takes all those instanceIDs and compares them
#	 against all JobConsumers table rows.  Moves to 'down' all consumers whose instanceID is not a running instance.
#
#  NOTE:  Make "group-name" and value configurable.  Use "instance.group-id" for VPC and group.id "sg-XXXXXX"
#   $CONSUMER_SECURITY_GROUP=sg-xxxxxx
#   $SECURITY_GROUP_FILTER="instance.group-id"
#
# Author: Joshua Boverhof
# See Copyright
#
Add-Type -Path "AWSSDK.dll"

$ErrorActionPreference='Stop'
. .\setup.ps1

$config = New-Object -TypeName Amazon.EC2.AmazonEC2Config
$config.WithServiceURL($REGION)
$client=[Amazon.AWSClientFactory]::CreateAmazonEC2Client($config);

$request = New-Object -TypeName Amazon.EC2.Model.DescribeInstancesRequest
$f1 = New-Object -TypeName Amazon.EC2.Model.Filter
$f1.WithName($SECURITY_GROUP_FILTER)
$f1.WithValue($CONSUMER_SECURITY_GROUP)
$f2 = New-Object -TypeName Amazon.EC2.Model.Filter
$f2.WithName("instance-state-name")
$f2.WithValue("running")

$filter_list = $f1,$f2
$request.WithFilter($filter_list);
$response = $client.DescribeInstances($request)
$instances = New-Object System.Collections.ArrayList

foreach ($reservation in $response.DescribeInstancesResult.Reservation)
{  
    foreach($instance in $reservation.RunningInstance)
    {
		echo $instance.InstanceId
        [void]$instances.Add($instance.InstanceId)
    }
}

if ($response.IsSetDescribeInstancesResult() -eq 0) 
{
	echo exit
	exit -1;
}

$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT Id,hostname,AMI,instance,status FROM ( SELECT *, ROW_NUMBER() OVER (ORDER BY Id) as row FROM dbo.JobConsumers  WHERE status='up') a WHERE row > 0", $conn)
$conn.Open();
$reader = $cmd.ExecuteReader()
$count = $reader.FieldCount
$consumer_list = New-Object “System.Collections.Generic.List[String]”


while ($reader.Read())
{
	for ($i = 0; $i -lt $count; $i++) {
		$key = $reader.GetName($i);
		$value = $reader.GetValue($i);
		@{ $reader.GetName($i) = $reader.GetValue($i); }
		
	}
	$consumerID = $reader.GetValue(0);
	$instanceID = $reader.GetValue(3);
	if ([string]::Compare($reader.GetName(3), "instance", $True)) 
	{ 
		"Unexpected Value"
		exit -1;
	}
	
	if($instances.Contains($instanceID)) 
	{
		$msg= "SKIP INSTANCE RUNNING {0}" -f $reader.GetValue(0)
		echo $msg
		continue
	}
	$consumerID = $reader.GetValue(0);
	echo "Turn Off Consumer: $consumerID"
	$consumer_list.Add($consumerID);
}

$reader.Close();
echo "Move to Down: $consumer_list"

foreach ( $i in $consumer_list) 
{
    #$cmd_str = "SELECT * FROM dbo.JobConsumers WHERE Id='{0}'" -f $i
	$cmd_str = "UPDATE dbo.JobConsumers SET status='down' WHERE Id='{0}'" -f $i
	echo $cmd_str
	$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
	$update_reader = $update_cmd.ExecuteReader();
	$update_count = $update_reader.FieldCount
	
	while ($update_reader.Read())
	{
		for ($i = 0; $i -lt $count; $i++) {
			@{ $update_reader.GetName($i) = $update_reader.GetValue($i); }
		}
	}
	$update_reader.Close();
}

$msg="Moved to Down:  {0}" -f $consumer_list.Count
echo $msg

