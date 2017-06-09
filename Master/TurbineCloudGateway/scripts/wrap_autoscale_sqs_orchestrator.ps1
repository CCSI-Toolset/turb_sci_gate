# Required initial setup
# Downloads MSI from S3 Bucket
$X=1
while (1)
{
	echo "$X) START"
	.\autoscale_sqs_orchestrator.ps1
	sleep 60
}
