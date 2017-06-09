#
# File:  push_submit_queue.ps1
# Author: Joshua Boverhof
# See Copyright

Add-Type -Path "AWSSDK.dll"
. .\setup.ps1



$client=[Amazon.AWSClientFactory]::CreateAmazonSQSClient($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)



$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=turbineMS11;User ID=uturbine;Password=X3n0F0b3;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT * FROM dbo.Jobs WHERE state='submit'", $conn);
$conn.Open();
$reader = $cmd.ExecuteReader()

# Call Read before accessing data.
while ($reader.Read())
{
	$jobID=$reader[0]
	$state=$reader[1]
	$request = New-Object -TypeName Amazon.SQS.Model.SendMessageRequest
	$request.MessageBody = $jobID
	$request.QueueUrl = "https://us-west-1.queue.amazonaws.com/754323349409/SubmitQueueRequest"
	$request.QueueUrl = "https://us-west-1.queue.amazonaws.com/754323349409/SubmitRequestQueue"
	$response = $client.SendMessage($request)
	echo "RESPONSE: " $response.SendMessageResult
}

$reader.Close();


#echo "Configure Bucket on AWS S3"
#$request = New-Object -TypeName Amazon.S3.Model.PutObjectRequest
#[void]$request.WithBucketName("turbine-ms11")
#[void]$request.WithKey("config")
#[void]$request.WithContentBody("{}")
#echo "GetObject"
#$response = $client.GetObject($request)



$ok = read-host "[return]"