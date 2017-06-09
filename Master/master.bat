call "c:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" amd64
set MSBuildEmitSolution=1
msbuild Turbine.sln /p:Configuration=Debug /p:Platform="Any CPU"
