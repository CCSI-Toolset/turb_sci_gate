# Required initial setup
. .\setup.ps1

#
# Creates 2 Databases
#
$DATABASETEST = $DATABASE + "Test"
$ALL_ARRAY = @($DATABASE,$USER,$PASS),@($DATABASETEST,$USERTEST,$PASSTEST)

foreach ($MY_ARRAY in $ALL_ARRAY)
{

	$DB = $MY_ARRAY[0]
	$USER = $MY_ARRAY[1]
	$PASS = $MY_ARRAY[2]
	
	$ok = read-host "Initialize $DB Continue [y|n]?"
	if ($ok -eq "y") 
	{
		$ok = read-host "DROP/CREATE/ALTER DATABASE $DB USER $USER[y|n]?"

		if ($ok -eq "y") 
		{
			echo "DROP $DB"
			$QUERY_STR = "DROP DATABASE $DB"
			sqlcmd  -S $SERVER -U $USER -P  $PASS -Q $QUERY_STR

			echo "CREATE $DB"

			# ADMIN USE Windows Authentication
			$QUERY_STR = "CREATE DATABASE $DB"
			sqlcmd  -S $SERVER -Q $QUERY_STR

			echo "ALTER AUTHORIZATION $DB"

			# ADMIN USE Windows Authentication
			$QUERY_STR = "ALTER AUTHORIZATION ON DATABASE::$DB TO $USER"
			sqlcmd  -S $SERVER -Q $QUERY_STR
		}
		else 
		{
			echo "DROP ALL Tables in $DB"
			$QUERY_STR = "DROP TABLE Messages, Jobs, Sessions, SinterProcesses, Consumers, Simulations, Users"
			sqlcmd  -d $DB -S $SERVER -U $USER -P  $PASS  -Q $QUERY_STR
		}
		echo "Schema: Create Tables in $DB"
		sqlcmd  -d $DB -S $SERVER -U $USER -P  $PASS -i ..\Master\Turbine.Data\TurbineDataModel.edmx.sql
	}

	$ok = read-host "[return]"
}