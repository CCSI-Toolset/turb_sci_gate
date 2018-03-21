# Turbine Science Gateway
This is a meta-project from which several Turbine related Windows projects are built to be used with the [CCSI foqus framework](https://github.com/CCSI-Toolset/foqus).

Turbine is batching execution environment and web API to run scientific software, for example Aspen, ACM and AspenPlus.

This repository contains the code and Visual Studio solutions for building the following targets:
* TurbineLite: Windows service Turbine Web API and file database
* SharedDataArchetecture: IIS Turbine Web API and SQL server database.

## Getting Started
For users of foqus, download the [latest release of TurbineLite.msi](../../releases/latest) and follow the [installation instructions](./INSTALL.md).

For developers or contributors, follow the below Development Practices.

## Development Practices
* Code development will be performed in a forked copy of the repo. Commits will not be 
  made directly to the repo. Developers will submit a pull request that is then merged
  by another team member, if another team member is available.
* Each pull request should contain only related modifications to a feature or bug fix.  
* Sensitive information (secret keys, usernames etc) and configuration data 
  (e.g database host port) should not be checked in to the repo.
* A practice of rebasing with the main repo should be used rather that merge commmits.

## Authors
* Josh Boverhof 

See also the list of [contributors](../../contributors) who participated in this project.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, 
see the [releases](../../releases) or [tags](../../tags) for this repository. 

## License & Copyright
See [LICENSE.md](LICENSE.md) file for details
