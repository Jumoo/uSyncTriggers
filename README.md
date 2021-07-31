# uSync Triggers

uSync triggers is an add on package for uSync that allows you to remotely trigger usync import or exports. The intention is this will help with CI/CD workflows - where you can trigger the import 
once your site is up, or after a warm up/slot swap for example. 

## Installation 

```
install-package uSync.Triggers -pre
```


## uSyncTriggerCLI

The triggers package exposes an end point that you can call with CURL commands, to trigger the processes, but for neatness and for nicer formatted results you can use the uSyncTriggerCLI which is a .net tool that you can install (standalone or as part of a build/release script).

```
dotnet tool install uSyncTriggerCLI 
```

You can then trigger an import/export from the command line 

e.g 
```
uSyncTrigger import {siteurl}/umbraco -u user -p password 
```
