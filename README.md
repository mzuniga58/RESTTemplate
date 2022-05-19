# RESTTemplate #
REST Service Visual Studio Extension

The REST Service Visual Studio Extension aids the user in the creation of REST Services. The REST Service generated comes with these features automaitcally included.

- Api Versioning using headers versioning
- Serilog logging - can log to any sink supported by Serilog
- Correlation ID support
- CORS support
- Swagger support
- Built-in health checks

In addition, the user can opt to include these features in the REST Service

- HAL, Hateoas support
- RQL support
- Choice of database technologies (currently, only SQL Server is supported)
- OAuth2 / OpenId-Connect support

## Api Versioning ##
The Api Versioning accepts these media types in the accept header:

- application/json - the default version of the endpoint is called.
- application/vnd.vnnn+json - version nnn is called. When the service is first created, only version 1 exists.

In addition, if the user chooses to include Hal, Hateoas support, the service will also support

- application/hal - the default version of the endpoint is called.
- application/hal.vnnn+json - version nnn is called.

## OAuth / OpenId-Connect Support ##
In the appSettings.json configuration files contain an OAuth section.

```json
  "OAuth2": {
    "Policies": [
      {
        "Policy": "policy",
        "Scopes": [ "scope" ]
      }
    ]
  }
```

The user can define any many policies as desired. Each policy supports a list of scopes. Only users or principals with one of those scopes can call the endpoint protected by a policy. To protect and endpoint, use the following attribute 

'[Authorize(Policy="name")]'

on the endpoint. Alternatively, you can use

'[AllowAnonymous]'

to allow any user to use that endpoint.

## HAL,Hateoas Support ##
If included, HAL responses will only be included for configured endpoints, and then only if either the **application/hal**
or **application/hal.v1+json** media type was sent in the accept header.

## RQL Support ##
RQL, if included, is supported on GET and PUT endpoints. To include RQL support in swagger, include the

'[SupportRQL]'

attribute at the endpoint. 

> Note: In order to support RQL in swagger, we internally generate an RQL = parameter. This parameter will unfortunately show up in the curl like this:
>
>.```
>curl -X 'GET' \
>  'https://localhost:50499/apiScopes?RQL=select(id,name)' \
>  -H 'accept: application/hal+json'
>```
>
>However, the RQL= paramter is not necessary. It is only there because of a limitation in swagger. This url call
>
>'https://localhost:50499/apiScopes?select(id,name)' 
>
>would produce the exact same result.