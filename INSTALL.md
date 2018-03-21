# turb_sci_gate
Turbine Science Gateway

## StandAlone Deployment:   Installing TurbineLite with FOQUS

### Install GIT:  
Open anaconda terminal and type
```
conda install git
```
### Install turb_client
```
  >pip install git+https://github.com/CCSI-Toolset/turb_client@master
  >pip install git+https://$USERNAME:$PASSWORD@github.com/CCSI-Toolset/foqus@master
```
## TurbineLite Dependencies
### Install SQL Compact 4.0 x64 ( MUST INSTALL 64 bit on x64 platform )
### Install SimSinterInstaller.msi
### Install TurbineLite.msi

## Cluster deployment
### Configure TurbineLite to run FOQUS Flowsheets
Run FOQUS to setup working directory, etc
```
(base) C:\Users\Administrator>\ProgramData\Anaconda2\python.exe \ProgramData\Anaconda2\Scripts\foqus.py --nogui —addTurbineApp focus
```
