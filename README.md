Command line trigger for remote uSync import/exports

This is the command line tool to be used in tandem with the uSync.Triggers project.

the uSync.Triggers package need to be installed and enabled on an umbraco site for the command line tool to work.

## Installing (of in deplotment YAML)

```
dotnet tool install uSyncTriggerCLI 
```

## Running command 

There are two core commands `Import` and `Export` both take the same options

e.g 
```
uSyncTrigger import {siteurl}/umbraco -u user -p password 
```

### Options

<pre>
import
  Run an uSync import on the specified Umbraco instance

Usage:
  uSyncTrigger [options] import <url>

Arguments:
  <url>  Umbraco url

Options:
  -u, --username <username> (REQUIRED)  umbraco username
  -p, --password <password> (REQUIRED)  umbraco password
  -f, --folder <folder>                 Folder to run import against
  -g, --group <group>                   Handler group (e.g settings, content)
  -s, --set <set>                      Handler set (e.g default)
  -x, --force                          force import (items imported even if there is no change)
  -v, --verbose                        verbose output
  -?, -h, --help                       Show help and usage information# uSyncTriggerCLI
</pre>
