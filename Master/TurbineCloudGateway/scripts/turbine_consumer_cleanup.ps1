#
# File:  turbine_consumer_cleanup.ps1
#    cleans up old directories to avoid disk, and checks processID of AspenSinterConsumerWindowsService
#    against database dbo.JobConsumer table rows where status is 'up'.  Turns any JobConsumers to 'down'
#    that don't match discovered processId.
# Author: Joshua Boverhof
# See LICENSE.md for License and Copyright details

#$ErrorActionPreference='Stop'
$ErrorActionPreference='SilentlyContinue'
. .\setup.ps1


$skip=10; 
$dirs = Get-ChildItem '\Program Files (x86)\Turbine\Gateway\Data\'
echo "Clean up Turbine Gateway Data"

$dirs | 
sort-object -property LastWriteTime -Descending | 
Where-Object {$_.Name -match "^\d+"} | 
Foreach-Object { 
    if($skip -le 0 ) {
       echo "DELETE: " $_.FullName; Remove-Item -Recurse $_.FullName 
    } 
    #else { 
    #   echo "SKIP $_" 
    #} 
    $skip--
}
echo "DONE"
#$ok = read-host "[return]"

echo "Check that Only running process has status up"
$ASPEN=Get-Process -Name AspenSinterConsumerWindowsService
$PROCESSID=$ASPEN.Id
$InstanceID=(New-Object System.Net.WebClient).DownloadString("http://169.254.169.254/latest/meta-data/instance-id")
$conn = new-object system.data.SqlClient.SQLConnection("Data Source=$DBHOST;Initial Catalog=$DBNAME;User ID=$SQL_USER;Password=$SQL_PASSWORD;Persist Security Info=True;MultipleActiveResultSets=True;");
$QUERY="UPDATE dbo.JobConsumers SET status='down' WHERE instance='$instanceID' and status='up' and processId <> '$PROCESSID'"
echo $QUERY
$cmd = new-object system.data.sqlclient.sqlcommand($QUERY, $conn);
$conn.Open();
$reader = $cmd.ExecuteReader()
$conn.Close()
