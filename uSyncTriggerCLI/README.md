# uSyncTriggerCLI 

Command line tool trigger for remote uSync import/exports

This is a dotnet tool that can be used in tandem with the uSync.Triggers project

**the uSync.Triggers package need to be installed and enabled on an umbraco site for the command line tool to work.**

You can trigger actions directy via CURL commands - so technically you don't need this tool, but it makes the syntax 
slightly easier to manage, and it pritty prints the output from the tiggers, so you get nicer outputs in your logs. 

## Installing CLI tool (or in deplotment YAML)

```
dotnet tool install uSyncTriggerCLI 
```

you can also install globally with the `--global` or `-g` switch.

## Running command 

There are two core commands `Import` and `Export` both take the same suite of options, but they can be called simply with no options other than the username and password ofand url umbraco site you wish to acces.

e.g 
```
uSyncTrigger import {siteurl}/umbraco -u user -p password 
```
_The commands take the path to the /umbraco folder and add the rest of the URL to this so you don't need to remember it._


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
