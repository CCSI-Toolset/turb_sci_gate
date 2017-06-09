MD 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator\'
cd 'C:\Program Files (x86)\Turbine\Gateway\Orchestrator'
svn checkout https://svn.acceleratecarboncapture.org/svn/projects/turb_sci_gate/trunk/Master/TurbineCloudGateway/tasks/
svn checkout https://svn.acceleratecarboncapture.org/svn/projects/turb_sci_gate/trunk/Master/TurbineCloudGateway/scripts/
cd .\Tasks
schtasks /Create /XML .\turbine_log_upload_s3.xml /TN "HydroUploadLogs"