# uSync Triggers

uSync triggers is an add on package for uSync that allows you to remotely trigger usync import or exports. The intention is this will help with CI/CD workflows - where you can trigger the import 
once your site is up, or after a warm up/slot swap for example. 

## Installation 

```
dotnet add package uSync.Triggers
```

## uSyncTriggerCLI

The triggers package exposes an end point that you can call with CURL commands, to trigger the processes, but for neatness and for nicer formatted results you can use the uSyncTriggerCLI which is a .net tool that you can install (standalone or as part of a build/release script).

```
dotnet tool install uSyncTriggerCLI 
```

You can then trigger an import/export from the command line 

# Note : v9/v10 only suppports HMAC auth (at the moment)

## HMAC Auth 
Using the Command line tool , you can use HMAC signiture authentication to trigger the tool (and then you don't have to use a username/password from your setup if you don't want to.)

```
usynctrigger seed 
```

Will generate a hmac key that you can use in the below setup:

in the site's appsettings.json : 

```json
  "uSync": {
    "Triggers": {
      "Enabled": true,
      "Key": "YOUR_HMAC_KEY"
    }
  }
```

You can then use the CLI tool to call the end point and have it use a generate and use a HMAC signature: 

```
usynctrigger import https://mysite-url/umbraco -h [HMACKEY]
```

HMAC auth method will use the default Umbraco user for all operations

-- 


n.b - you can only run uSync.triggers in basic OR HMAC mode not both at the same time. 

