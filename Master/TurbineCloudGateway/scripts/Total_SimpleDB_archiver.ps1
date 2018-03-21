#
# File:  Total_SimpleDB_archiver.ps1
# Author: Joshua Boverhof
# See LICENSE.md for License and Copyright details

Add-Type -Path "AWSSDK.dll"

$ErrorActionPreference='Stop'
. .\setup.ps1

$domains = New-Object “System.Collections.ArrayList”;

Function CreateDomain
{
	Param(
		[Amazon.SimpleDB.AmazonSimpleDB]$client, 
		[string]$targetDomain
	)
	$request = New-Object -TypeName Amazon.SimpleDB.Model.ListDomainsRequest
	$response = $client.ListDomains($request);
	$targetDomainExists = $FALSE;
	Write-Host("List Domains '{0}'" -f $response.ListDomainsResult.IsSetDomainName())
	if ($response.ListDomainsResult.IsSetDomainName() -eq $TRUE) 
	{
		$domains.Clear();
		foreach ($domain in $response.ListDomainsResult.DomainName) 
		{
			Write-Host("LIST DOMAIN: '{0}'" -f $domain);
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


Function PutDataDomain
{
	Param(
		[Amazon.SimpleDB.AmazonSimpleDB]$client, 
		[system.data.sqlclient.sqlcommand]$cmd, 
		[string]$domain
	)
	$batch_size = 25;
	$reader = $cmd.ExecuteReader() # Enumerators that don't return Tokens can't be used in a Powershell function call
	$count = $reader.FieldCount
	[bool]$suc = $reader.Read();

	#Write-Host("WriteDataToDomain ==> Count '{0}':" -f $reader.Count)	  BUG: THIS LINE SCREWS READER UP
	Write-Host("WriteDataToDomain ==> FieldCount {0}" -f $count);
	Write-Host("WriteDataToDomain ==> Read {0}" -f $suc);
	$rows = 0;
    [IList]$rp_items = (new-object System.Collections.ArrayList)
    $cc = 0;
	while ($suc)
	{
		$rows++;
        $key = $reader.GetName(0);
        $val = $reader.GetValue(0);
        
		$item = (new-object Amazon.SimpleDB.Model.ReplaceableItem).WithItemName($val);
		Write-Host("WriteDataToDomain ==> NAME '{0}':" -f $val)
		[IList]$attributes = (new-object System.Collections.ArrayList)
		for ($i = 1; $i -lt $count; $i++) 
		{
			$attributes.Add((new-object Amazon.SimpleDB.Model.ReplaceableAttribute).`
				WithName($reader.GetName($i)).WithValue($reader.GetValue($i)));
		}
        # NOTE: ADD HARDCODED ATTRIBUTES, ADD TO CONSUMER TABLE
        $attributes.Add((new-object Amazon.SimpleDB.Model.ReplaceableAttribute).`
				WithName("instanceType").WithValue("m1.medium"));
        $attributes.Add((new-object Amazon.SimpleDB.Model.ReplaceableAttribute).`
				WithName("ConsumerAccount").WithValue("Administrator"));

		$item.WithAttribute($attributes);
		$rp_items.Add($item);
		$suc = $reader.Read();
		if ($rows -eq $batch_size -or (-not $suc))
		{
			Write-Host("WriteDataToDomain ==> SEND BATCH OFF TO AWS");
			$request = (new-object Amazon.SimpleDB.Model.BatchPutAttributesRequest).WithDomainName($domain);
			$request.WithItem($rp_items);
			$response = $client.BatchPutAttributes($request);
			$rp_items.Clear();
			$rows = 0;
            if ($cc -eq 1) {
    			break; # just once time through
            }
            break;
            $cc++;
		}
	}
	Write-Host("WriteDataToDomain ==> RETURN");
	$reader.Close()
}

Function SaveQueryToSimpleDB
{
	Param(
		[Amazon.SimpleDB.AmazonSimpleDB]$client, 
		[system.data.SqlClient.SQLConnection]$conn, 
		[string]$domain, 
		[string]$cmd_str
	)
	Write-Host("Grab Jobs");
	$cmd = new-object system.data.sqlclient.sqlcommand($cmd_str, $conn)
	Write-Host("DOMAIN '{0}':" -f $domain)	
	if ($domains.Contains($domain) -eq $FALSE) 
	{
		CreateDomain $client $domain
	}	
	PutDataDomain $client $cmd $domain
}

Write-Host("Start")	
$XSECRET_ACCESSKEY_ID = "AKIAJYEGFRZTDU77PYOQ"
$XSECRET_KEY_ID = "qF6wTe4iPsmJDJD/EBGyu2VpVfS9oaOVmRTfA7l6"
$client=[Amazon.AWSClientFactory]::CreateAmazonSimpleDBClient($XSECRET_ACCESSKEY_ID, $XSECRET_KEY_ID, [Amazon.RegionEndpoint]::USWest1)
$conn = new-object system.data.SqlClient.SQLConnection("Data Source=184.169.156.70;Initial Catalog=Turbine;User ID=uturbine;Password=X3n0F0b3;Persist Security Info=True;MultipleActiveResultSets=True;");
$conn.Open();
$domain = "TurbineMS14_Optimization_1-25-13_Jobs";
$cmd_str = "SELECT job.Id,job.guid,job.State,job.[Create],job.Submit,job.Setup,job.Running,job.Finished,job.Initialize,job.UserName,job.ConsumerId,consumer.instance,consumer.AMI,job.SessionId,job.Reset,job.SimulationName,app.Name AS ApplicationName FROM dbo.Jobs job, dbo.Applications app, dbo.JobConsumers as consumer WHERE job.ConsumerId=consumer.Id"
SaveQueryToSimpleDB $client $conn $domain $cmd_str

$conn.Close();