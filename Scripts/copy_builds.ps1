#
[void][reflection.assembly]::LoadFile("C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Client\System.dll")
[void][reflection.assembly]::LoadFile("C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Client\WindowsBase.dll")


. .\setup.ps1

#$ok = read-host "Zip and copy out builds for MS6[y|n]?"
$ok = 'y'
if ($ok -ne "y") 
{
	echo "return"
	return;
}

$consumer_dir = "C:\Users\boverhof\Documents\Visual Studio 2010\Projects\turbine_ms7\Master\AspenSinterConsumerWindowsService\bin\Debug"
$zipfilename = "C:\inetpub\www-turbine\Builds\MS7\AspenSinterConsumerWindowsService.zip"
#$files = Get-ChildItem -Path $consumer_dir
$files = Resolve-Path $consumer_dir/*

$zippackage = [System.IO.Packaging.ZipPackage]::Open($zipfilename,
		[System.IO.FileMode]"OpenOrCreate", [System.IO.FileAccess]"ReadWrite")
		
foreach ($file in $files) 
{
	echo $file.FullName
	$partUri = New-Object System.Uri($file.Path, [System.UriKind]"Relative")
	echo partUri
	$partName = $PackUriHelper.CreatePartUri($partUri)
	#$part=$ZipPackage.CreatePart($partName, "application/zip", [System.IO.Packaging.CompressionOption]"Maximum")
	#$bytes=[System.IO.File]::ReadAllBytes($file)
	#$stream=$part.GetStream()
	#$stream.Write($bytes, 0, $bytes.Length)
	#$stream.Close()
	break;
}		

		
$zippackage.Close()
