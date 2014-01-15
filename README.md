ovhapicil
=========

.NET Client for OVH API ( More info about API can be found at [https://api.ovh.com/](https://api.ovh.com/))

The API is automatically generated from [OVH documentation](https://api.ovh.com/console/) using CodeDom and T4 templates

How to use the API
==================


1. Create your Application keys and tokens
---------------------------------------

To create an application key and password, visit the [OVH page to create application](https://eu.api.ovh.com/createApp/)

Once the application key and secret are created, you must retrieve a consumer key that is linked to an OVH Account. The only parameter of RequestRredential method is an array of AccessRule specifying the access rights required by the application.

 ```csharp
   OvhApiClient api = new OvhApiClient("YOUR_APPLICATION_KEY",
                                    "YOUR_APPLICATION_SECRET", OvhInfra.Europe);
   CredentialsResponse response = api.RequestCredential(new []{
                                     new AccessRule{ Method = "GET", Path = "/*"},
                                    // new AccessRule{ Method = "PUT", Path = "/*"},
                                    // new AccessRule{ Method = "POST", Path = "/*"},
                                    // new AccessRule{ Method = "DELETE", Path = "/*"},
                                  }).Result;
                        
 ```
 
 The previous code returns a *CredentialsResponse.ValidationUrl*.  **You must use this url to link your application with your OVH NickHandle**. Therefore, Copy/Paste or redirect user to that url to create the ConsumerKey.
 
2. Consume API services
-----------------------

```csharp
   OvhApiClient api = new OvhApiClient("YOUR_APPLICATION_KEY",
                                       "YOUR_APPLICATION_SECRET", 
                                       OvhInfra.Europe,
                                       "YOUR_CONSUMER_KEY");
                                       
    //now, you can use your api to access your OVH Services
```

