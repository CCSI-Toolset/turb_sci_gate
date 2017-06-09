#
# File:  job_SimpleDB_archiver.ps1
#    Finds jobs in database that are being processed by JobConsumers that ARE NOT in status 'up', 
#    and moves them back to 'submit'.
# Author: Joshua Boverhof
# See Copyright
Add-Type -Path "AWSSDK.dll"

$ErrorActionPreference='Stop'
. .\setup.ps1

$domains = New-Object “System.Collections.ArrayList”;

Function CreateDomain($client, $targetDomain) 
{
	$request = New-Object -TypeName Amazon.SimpleDB.Model.ListDomainsRequest
	$response = $client.ListDomains($request);
	$targetDomainExists = $FALSE;
	Write-Host("List Domains '{0}'" -f $response.ListDomainsResult.IsSetDomainName())
	if ($response.ListDomainsResult.IsSetDomainName() -eq $TRUE) 
	{
		$domains.Clear();
		foreach ($domain in $response.ListDomainsResult.DomainName) 
		{
			Write-Host("DOMAIN: '{0}'" -f $domain);
			$targetDomainExists = $domain -eq $targetDomain
			$domains.Add($domain);
		}
	}

	# Gets and sets the DomainName property. The name of the domain to create. 
	# The name can range between 3 and 255 characters and can contain the 
	# following characters: a-z, A-Z, 0-9, '_', '-', and '.'. 

	if ($targetDomainExists -eq $FALSE)
	{
		Write-Host("Create Domain {0}" -f $targetDomain);
		$request = New-Object -TypeName Amazon.SimpleDB.Model.CreateDomainRequest;
		$request.DomainName = $targetDomain;
		$response = $client.CreateDomain($request);
	}
}

Function GetJobs($delete_jobs, $sessionID)
{
	$conn = new-object system.data.SqlClient.SQLConnection("Data Source=184.169.156.70;Initial Catalog=turbineMS12;User ID=uturbine;Password=X3n0F0b3;Persist Security Info=True;MultipleActiveResultSets=True;");
	$conn.Open();

	# flatten jobs
	Write-Host("Grab Jobs for session: {0}" -f $sessionID);
	$cmd_str = "SELECT job.Id,job.State,job.[Create],job.Submit,job.Setup,job.Running,job.Finished,job.Initialize,job.guid,job.UserName,job.ConsumerId,job.Reset,job.SimulationName,sim.ApplicationName,pro.Input,pro.Output,pro.Status FROM dbo.Jobs job,dbo.Simulations sim,dbo.SinterProcesses pro WHERE job.SessionId='{0}' AND job.SimulationName=sim.Name AND job.Process_Id=pro.Id" -f ($sessionID)
	$cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
	$reader = $cmd.ExecuteReader()
	$count = $reader.FieldCount
	$job_list = New-Object “System.Collections.ArrayList”;

	while ($reader.Read())
	{
		$job_dict = @{};
		for ($i = 0; $i -lt $count; $i++) {
			$job_dict.Add($reader.GetName($i),$reader.GetValue($i));	
		}
		$job_list.Add($job_dict);
		
		if ($delete_jobs) {
			$cmd_str = "DELETE FROM dbo.Messages WHERE JobId='{0}'" -f $reader.GetValue(0)
			Write-Host($cmd_str)
			$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
			$update_reader = $update_cmd.ExecuteReader()
			$update_count = $update_reader.FieldCount
			$update_reader.Close();
			
			$cmd_str = "DELETE FROM dbo.StagedInputFiles WHERE JobId='{0}'" -f $reader.GetValue(0)
			Write-Host($cmd_str)
			$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
			$update_reader = $update_cmd.ExecuteReader()
			$update_count = $update_reader.FieldCount
			$update_reader.Close();	
			
			$cmd_str = "DELETE FROM dbo.StatgedOutputFiles WHERE JobId='{0}'" -f $reader.GetValue(0)
			Write-Host($cmd_str)
			$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
			$update_reader = $update_cmd.ExecuteReader()
			$update_count = $update_reader.FieldCount
			$update_reader.Close();		
		}
	}
	if ($delete_jobs) 
	{
		$cmd_str = "DELETE FROM dbo.Jobs WHERE SessionId='{0}'" -f ($sessionID)
		Write-Host($cmd_str)
		$update_cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
		$update_reader = $update_cmd.ExecuteReader()
		$update_count = $update_reader.FieldCount
		$update_reader.Close();	
	}
	return $job_list;
}

$client=[Amazon.AWSClientFactory]::CreateAmazonSimpleDBClient($SECRET_ACCESSKEY_ID,$SECRET_KEY_ID)
#$sessionID = '6e7a4734-173e-4547-9945-a25e19a231fa';
$sessionID = '83ff15e2-77e1-45cb-9222-2a94bd5599e1';
$job_list = GetJobs $FALSE $sessionID
Write-Host($job_list);
echo $job_list
foreach( $job in $job_list) 
{
	echo $job.ApplicationName
	$domain = "{0}_{1}" -f  ($job.ApplicationName,$job.SimulationName)
	echo DOMAIN: $domain
	if ($domains.Contains($domain) -eq $FALSE) 
	{
		CreateDomain $client $domain
	}
}

