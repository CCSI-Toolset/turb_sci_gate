#
# File:  job_sweeper.ps1
#    Finds jobs in database that are being processed by JobConsumers that ARE NOT in status 'up', 
#    and moves them back to 'submit'.
# Author: Joshua Boverhof
# See Copyright

Add-Type -Path "AWSSDK.dll"

$ErrorActionPreference='Stop'
. .\setup.ps1

$config = New-Object -TypeName Amazon.EC2.AmazonEC2Config

$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT Id,hostname,AMI,instance,status FROM dbo.JobConsumers  WHERE status='up'", $conn)
$conn.Open();
$reader = $cmd.ExecuteReader()
$count = $reader.FieldCount
$consumer_list = New-Object “System.Collections.Generic.List[String]”


while ($reader.Read())
{
	#for ($i = 0; $i -lt $count; $i++) {
	#	$key = $reader.GetName($i);
	#	$value = $reader.GetValue($i);
	#	@{ $reader.GetName($i) = $reader.GetValue($i); }	
	#}
	if ([string]::Compare($reader.GetName(0), "Id", $True)) 
	{ 
		"Unexpected Value"
		exit -1;
	}
	
	$consumer_list.Add($reader.GetValue(0));
}

$reader.Close();
echo "Running Consumers: $consumer_list"
$cmd_str = "SELECT Id,ConsumerId FROM ( SELECT *, ROW_NUMBER() OVER (ORDER BY Id) as row FROM dbo.Jobs  WHERE state='setup') a WHERE row > 0 and row <= 100"
$cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
$reader = $cmd.ExecuteReader()
$count = $reader.FieldCount

$row_count = 0;

while ($reader.Read())
{
	if ([string]::Compare($reader.GetName(0), "Id", $True)) 
	{ 
		"Unexpected Value"
		exit -1;
	}
	for ($i = 0; $i -lt $count; $i++) {
		$key = $reader.GetName($i);
		$value = $reader.GetValue($i);
		@{ $reader.GetName($i) = $reader.GetValue($i); }	
	}
	if ($consumer_list.Contains($reader.GetValue(1)) )
	{
		$msg="CONSUMER RUNNING SKIP {0} " -f $reader.GetValue(1)
		write-host $msg
		continue;
	}
	
	$cmd_str = "UPDATE dbo.Jobs SET state='submit',Setup='' WHERE Id='{0}'" -f $reader.GetValue(0)
	echo $cmd_str
	
	$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
	$update_reader = $update_cmd.ExecuteReader()
	$update_count = $update_reader.FieldCount
	
	while ($update_reader.Read())
	{
		for ($i = 0; $i -lt $count; $i++) {
			@{ $update_reader.GetName($i) = $update_reader.GetValue($i); }
		}
	}
	$update_reader.Close();
	$row_count += 1;
	
	$cmd_str = "INSERT INTO dbo.Messages (Id,Value,JobId) VALUES ('{0}','{1}','{2}')" -f ([guid]::NewGuid(), "JobSweeper Reset From Setup, detected Consumer down", $reader.GetValue(0))
	echo $cmd_str
	$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
	$update_reader = $update_cmd.ExecuteReader();
	
}

$msg="Moved to submit:  {0}" -f $row_count
echo $msg

