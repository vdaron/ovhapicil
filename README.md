ovhapicil
=========

.NET Client for OVH API ( More info about API can be found at [https://api.ovh.com/](https://api.ovh.com/))


Create your Application keys and tokens
---------------------------------------

First, you have to create an application key and password [here](https://eu.api.ovh.com/createApp/)

Once the application key and secret are created, you must retrieve a consumer key that is linked to an OVH Account. You can filter access rights on the api.

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
 
 The previous code returns a CredentialsResponse.ValidationUrl. Copy/Paste that url in your browser to create the ConsumerKey
 
How to use the API
------------------

```csharp
   OvhApiClient api = new OvhApiClient("YOUR_APPLICATION_KEY",
                                       "YOUR_APPLICATION_SECRET", 
                                       OvhInfra.Europe,
                                       "YOUR_CONSUMER_KEY");
                                       
    //now, you can use your api to access your OVH Services
```

