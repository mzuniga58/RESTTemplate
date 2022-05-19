# REST Service Wizard Tutorial #
Eventually, I will place this template on the Microsoft Store, so that it can be downloaded directly in Visual Studio. However, until then, you will need to download this repository and build the project. I suggest you build it in release mode, but either release or debug will work. Once compiled, using the standard windows explorer, navigate to 

.\RESTTemplate\RESTInstaller\bin\Release

In that folder you will find the file *RESTInstaller.vsix*. Double click on that file to install the Visual Studio extension. Note, you should shut down all instances of Visual Studio before installing.

## Creating a REST Service ##
Once installed, open Visual Studio and select **Create a new project** from the initial popup window. When the Create a new project dialog appears, select WebAPI in the project types dropdown on the top right side of the dialog. When you do, you will see an entry for REST Service (.Net Core) option, with the blue MZ logo next to it.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateAService.png "Create a new project")

Selet that entry and press **next**. When you do, the standard Visual Studio create a project dialog appears. We're going to create a bookstore service that will list books and their authors, so in the Project name field, enter Bookstore and press **create**. When you do, you will see the REST Service Wizard dialog.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/RESTServiceWizard.png "REST Service Wizard dialog")

When you first open this dialog, the Your name, email and project url fields will be blank. Once you fill them in, they will be pre-populated the next time you run this wizard. All three fields contain information that will be placed in the Swagger document of your project, so do fill them in with meaningful information. In my case, I have filled them in with my name, my email address, and the URL to my GitHub home page.

Next you can choose the .NET Version you wish to build your service in. At present, the only option is .NET 6.0. In the near future, I will be adding support for .NET 7.0 and as time goes on, support for any newer versions that Microsoft produces. You can also choose the database technology that your service will use. There are three options:

- SQL Server
- Postgresql
- My SQL

However, at present, I only have support for SQL Server. You have three other options, all of which are checked by default.

- Use OAuth Authentication - this choice allows your sevice to be protected by OAuth. You will need an OAuth identity provided to take advantage of this option. In this case, I will leave it checked, but we won't really be using it. Nevertheless, you will see how it could be used. Feel free to use it if you do have access to an identity provider.
- Incorporate RQL - **Resource Query Language (RQL)** is a query language designed for use in URIs with object style data structures. The language provides powerful filtering capabilities to your endpoints.
- Incorporate HAL - **Hypertext Application Language (HAL)** is a simple format that gives a consistent and easy way to hyperlink between resources in your API.

Now, press Ok to generate your REST Service. Once it is generated, you can compile it and run it.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/StarterService.png "Starter Service")

It doesn't look like much yet, as we nave not yet defined any resources or endpoints. Nevertheless, you can see the information about yourself and the project in the top left corner under the title, and users can click on your name to send you email, or to visit your website. 

Before we expand our service, let's take a minute to go over some of the features. This REST service is created with a layered architecture. The three main layers are the presentation layer, the orchestration layer and the repository layer.

At this point, the presentation layer doesn't exist, aside from the swagger. Soon we will be adding some controllers, and it is the collection of controllers and the endpoints that they define that constitute the presentation layer. This is the layer that your users see and interact with. 