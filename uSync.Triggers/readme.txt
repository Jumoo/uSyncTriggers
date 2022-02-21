
       __                   _____      _                           
 _   _/ _\_   _ _ __   ___ /__   \_ __(_) __ _  __ _  ___ _ __ ___ 
| | | \ \| | | | '_ \ / __|  / /\/ '__| |/ _` |/ _` |/ _ \ '__/ __|
| |_| |\ \ |_| | | | | (__ _/ /  | |  | | (_| | (_| |  __/ |  \__ \
 \__,_\__/\__, |_| |_|\___(_)/   |_|  |_|\__, |\__, |\___|_|  |___/
          |___/                          |___/ |___/               


  Thanks for downloading uSync.triggers. 

  This will add an endpoint to your site that you can call without
  being authenticated, that can start uSync import or export 
  processes. 

  -----

  n.b : this will not work until you have a uSync.Triggers value
  in your appsettings.json config file 
  
  "uSync" : {
	"Triggers": {
		"Enabled" : true,
		"Key" : "HMAC_KEY_HERE"
	}
  }
  
  -----

  use can the uSyncTriggerCli tool to generate a hmac key

  PS> dotnet run uSynctrigger seed 

  -----

  See the CLI tool for how to run imports/export etc...



