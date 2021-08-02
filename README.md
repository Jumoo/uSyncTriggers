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
## Basic Auth
The default setup requires you supply a valid Umbraco username and password via basic authentication. This umbraco user must have access to the Settings section of the site in-order for the commands to succeed. 

## HMAC Auth 
Using the Command line tool , you can use HMAC signiture authentication to trigger the tool (and then you don't have to use a username/password from your setup if you don't want to.)

```
usynctrigger seed 
```

Will generate a hmac key that you can use in the below setup:

in the site's web.config : 

```
    <add key="uSync.Triggers" value="True"/>
    <add key="uSync.TriggerScheme" value="Hmac"/>
    <add key="uSync.TriggerHmacKey" value="[HMACKEY]"/>
```

You can then use the CLI tool to call the end point and have it use a generate and use a HMAC signature: 

```
usynctrigger import https://mysite-url/umbraco -h [HMACKEY]
```

HMAC auth method will use the default Umbraco user for all operations

-- 


n.b - you can only run uSync.triggers in basic OR HMAC mode not both at the same time. 

