$InstanceID=(New-Object System.Net.WebClient).DownloadString("http://169.254.169.254/latest/meta-data/instance-id")

$file = Get-ChildItem "C:\\Program Files (x86)\\foqus\\foqus*\\dist\\foqus_console.exe" 

if ($file)
{
	# SEND MESSAGE TO SNS TOPIC
	#$msg = "Consumer ($($InstanceID)): Starting FOQUS"
	#Publish-SNSMessage -TopicArn arn:aws:sns:us-east-1:754323349409:FOQUS-Consumer -Message $msg -Region us-east-1

	& $file --consumer

	# Check if the last command succeeded or failed
	if ($?)
	{
		# SEND MESSAGE TO SNS TOPIC
		#$msg = "Consumer ($($InstanceID)): FOQUS failed to run"
		#Publish-SNSMessage -TopicArn arn:aws:sns:us-east-1:754323349409:FOQUS-Consumer -Message $msg -Region us-east-1
                
        	# SEND MESSAGE TO SNS TOPIC
        	$msg = "Consumer ($($InstanceID)): Stopping FOQUS"
        	Publish-SNSMessage -TopicArn arn:aws:sns:us-east-1:754323349409:FOQUS-Consumer -Message $msg -Region us-east-1
	        
                & 'C:\\Users\\Administrator\\Desktop\\FOQUS-Service\\nssm.exe' stop foqus
        }
}
