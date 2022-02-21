## About 

uSync Triggers adds an endpoint to your site that you can then call
without having to login to the umbraco back end.

---

note: you need to enable uSync.Triggers in your appsettings.json before 
it will work. 

For v9 we currently also only support HMAC auth 

```
"uSync" : {
	"Triggers": {
		"Enabled" : true,
		"Key" : "HMAC_KEY_HERE"
	}
}
```

to generate a HMAC key use the uSyncTriggerCli, 

```
dotnet run uSyncTrigger seed
```

---

Once installed you can then trigger imports to and from your site

---