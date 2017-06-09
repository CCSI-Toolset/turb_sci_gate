#
# File:  autoscale_sqs_orchestrator.ps1
#    Searches for all jobs in setup,running,submit state, adds entries to SQS queue.
#    Simple tie-in for autoscaling for shared-database architectures.
#
# Author: Joshua Boverhof
# See Copyright
#
Add-Type -Path "AWSSDK.dll"
$ErrorActionPreference='Stop'
. .\setup.ps1

$client=[Amazon.AWSClientFactory]::CreateAmazonSQSClient()

$conn = new-object system.data.SqlClient.SQLConnection("Data Source=localhost;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$cmd = new-object system.data.sqlclient.sqlcommand("SELECT COUNT(*) FROM dbo.Jobs WHERE state='submit' OR state='setup' OR state='running'", $conn);
$conn.Open();
$reader = $cmd.ExecuteReader()
$count = 0;
$queue_count = 0;


# Call Read before accessing data.
while ($reader.Read())
{
	$count=$reader[0]
	$request = New-Object -TypeName Amazon.SQS.Model.GetQueueAttributesRequest
	$request.WithAttributeName(@("ApproximateNumberOfMessages"))
	$request.QueueUrl = $SUBMIT_QUEUE_URL
	$response = $client.GetQueueAttributes($request)
	$queue_count = $response.GetQueueAttributesResult.ApproximateNumberOfMessages
	echo "RESPONSE: " $queue_count
	echo "COUNT: " $count
	$change = $count - $queue_count
	echo "DIFF: " $change
	break;
}
$reader.Close();

$myid = 1
if ($change -lt 0) 
{
	while ($change -lt 0) 
	{
		$delete_num = $change * -1
		echo "POP SUBMIT QUEUE: " $delete_num
		$request = New-Object -TypeName Amazon.SQS.Model.ReceiveMessageRequest
		$request.VisibilityTimeout = 60
		$request.maxNumberOfMessages = ($delete_num % 10)
		$request.QueueUrl = $SUBMIT_QUEUE_URL
		if ($delete_num -ge 10)
		{
			$request.maxNumberOfMessages = 10
		}
		$response = $client.ReceiveMessage($request)
		if ($response.IsSetReceiveMessageResult()) 
		{
			if($response.ReceiveMessageResult.IsSetMessage()) 
			{
				$request =  New-Object -TypeName Amazon.SQS.Model.DeleteMessageBatchRequest
				$request.WithQueueUrl($SUBMIT_QUEUE_URL)
				$entry_list = New-Object -TypeName System.Collections.Generic.List[Amazon.SQS.Model.DeleteMessageBatchRequestEntry]
				foreach ($message in $response.ReceiveMessageResult.Message)
				{
					if ($message.IsSetReceiptHandle())
					{
						$entry = New-Object -TypeName Amazon.SQS.Model.DeleteMessageBatchRequestEntry
						$handle = $message.ReceiptHandle
						$entry.WithReceiptHandle($handle)
						$entry.WithId("deletemsg{0}" -f $myid)
						$entry_list.Add($entry)
						$myid += 1
					}
				}
				$request.WithEntries($entry_list.ToArray())
				$response = $client.DeleteMessageBatch($request)
				if ($response.IsSetDeleteMessageBatchResult())
				{
					if ($response.DeleteMessageBatchResult.IsSetDeleteMessageBatchResultEntry())
					{
						echo "DELETE COUNT: " $response.DeleteMessageBatchResult.DeleteMessageBatchResultEntry.Count
						$change += $response.DeleteMessageBatchResult.DeleteMessageBatchResultEntry.Count
						echo "CHANGE: " $change
					}
				}
			}
			
		}
	}
}
elseif ($change -gt 0)
{
	echo "PUSH SUBMIT QUEUE: " $change
	while ($change -gt 0) 
	{
		$request =  New-Object -TypeName Amazon.SQS.Model.SendMessageBatchRequest
		$request.WithQueueUrl($SUBMIT_QUEUE_URL)
		$entry_list = New-Object -TypeName System.Collections.Generic.List[Amazon.SQS.Model.SendMessageBatchRequestEntry]
		$cmp_val = $change - 10
		if ($cmp_val -lt 0) 
		{
			$cmp_val = 0
		}
		
		do {
			$entry = New-Object -TypeName Amazon.SQS.Model.SendMessageBatchRequestEntry
			$entry.WithId("MSG{0}{1}" -f ($change, [guid]::NewGuid()))
			$entry.WithMessageBody("autoscale MSG{0}" -f $change)
			$entry_list.Add($entry)
			$cmp_val += 1
		} while ($change -gt $cmp_val) ;
		
		$request.WithEntries($entry_list.ToArray())
		$response = $client.SendMessageBatch($request)	
		if ($response.IsSetSendMessageBatchResult())
		{
			if ($response.SendMessageBatchResult.IsSetSendMessageBatchResultEntry())
			{
				echo "SEND COUNT: " $response.SendMessageBatchResult.SendMessageBatchResultEntry.Count
				$change -= $response.SendMessageBatchResult.SendMessageBatchResultEntry.Count
				#echo "CHANGE: " $change
			}
		}
	}
}
else
{
	echo "NOTHING TO DO"
}