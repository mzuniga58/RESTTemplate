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

It doesn't look like much yet, as we nave not yet defined any resources or endpoints. Nevertheless, you can see the information about yourself and the project in the top left corner under the title, and users can click on your name to send you email, or click on the website link to visit your website. You can also see the authorize button. Clicking on this button will require you to enter an access token that you get from the identity provider to authorize the user to hit the various endpoints that require it. At this time, of course, we don't have any endpoints that reqire it, so you can igore that button for the time being.

Before we expand our service, let's take a minute to go over some of the features. This REST service is created with a layered architecture. The three main layers are the presentation layer (also called the Resource layer), the orchestration layer and the repository layer.

## Presentation Layer ##
At this point, the presentation layer doesn't exist, aside from the swagger. Soon we will be adding some controllers, and it is the collection of controllers and the endpoints that they define, along with the HAL configurations, that constitute the presentation layer. This is the layer that your users see and interact with. This is the layer they use to obtain and manage the resources the service controls, hence, that is why it is also sometimes called the resource layer. Later in this tutorial, we will show how to create a controller and how to populate the HAL configurations.

## Orchestration Layer ##
When the user calls an endpoint in your presentation layer, under the covers, the presentation layer calls the orchestration layer to accomplish the actual work. If the user asks for a parcitular resource, i.e., /books/1234, the preentation layer calls the orchestration layer to get the book whose Id is 1234. Likewise, if the user calls books/name/The%20Wrath%20of%20Isis, the user is calling the service to obtain the book called "The Wrath of Isis" (a wonderfully written book, by the way), then the presentation layer will call the orchestration layer to get the book whose name is "The Wrath of Isis". The orchestration layer will return the desired result, and in turn, the presentation layer will pass back the result to the caller.

The orchestration layer resides in the Orchestration folder. There are two files there, IOrchestrator and Orchestrator (which supports the IOrchestrator interface). The IOrchestrator defines, and the Orchestrator implements a number of geneic functions, a set of generice CRUD (Create, Read, Update, Delete) functions, that have been pre-defined for you. These generic operations are:

- **GetSingleResourceAsync** - retrieves a single resource of type T, using an RQL Statement to futher refine the resource.
- **GetResourceCollectionAsync** - retrieves a collection of resources of type T, returned in a PagedSet, using an RQL Statement to filter and refine the set.
- **AddResourceAsync** - adds a resource of type T to the datastore
- **UpdateResourceAsync** - updates a resource of type T in the datastore, using an RQL Statment to further refine the update
- **DeleteResourceAsync** - deletea a resource or set of resources of type T from the datastore, using an RQL Statement to define which resources are to be deleted.

These generic functions are sufficient for simple resources. However, for more complex resources (such as those that contain embedded child resources), you will need to create your own functions. To do so, simply include that function in the IOrchestrator interface and implement it in the Orchestrator object. In your new function, you can often combine (or "orchestrate") the generic functions to accomplish your task. I'll be giving an example of that later in this tutorial.

One other thing about the orchestration layer is its use of RQL. The **RqlNode** is a compiled version of an RQL Statement. The RQL Statement can be provided by the user in the query portion of the URL (basically, everything after the ?), or it can be hardcoded in the presentation layer by the programmer. Here are two examples:

```
var node = RqlNode.Parse(Request.QueryString.Value);
var node = RqlNode.Parse($"Author={authorId});
```

The first example compiles the query string of the requested Url into an RqlNode object. The second takes the RQL Statement "Author=nnn", and compiles it into an RqlNode object. The RqlNode object contains a structured representation of the statement, or a statement of NOOP if there is no statement. RqlNode.Parse("") will return an RqlNode of NOOP, for example. RQL statments can be simple, like the one shown above, or they can become quite complex. 

```
(in(AuthorId,{auth1},{auth2},{auth3})&category=Sifi)|(like(AuthorName,T*)&category=Fantasy)&select(bookTitle,pubishDate,AuthorName)&limit(1,20)
```

This RQL statement will return the collection of books who where written by either auth1, auth2 or auth3 (these are Author Ids) and whose category is Sifi, OR any Fantasy book written by an auther whose name beings with 'T' (i.e., Thomson, Tiller, Tanner, etc.). It limits the results to include only the book title, the publish date of the book and the authorname, and it gives you only the first 20 results.

For more information on RQL syntax, look here: enter link here

## Repsoitory Layer ##
Under the covers, to do its work, the orchestration layer calls the repository layer to obtain and manipulate data in the underlying datastore. The repositoy layer resides under the repositories folder in the IRepository and Repository files. Likw the orchestraton layer, the repository layer comes with pre-configured generic CRUD functions. One thing to notice between the repository layer and the orchestration layer is that the orchestration layer accepts Resource models, while the repository layer accepts entity models. When you think about it it make sense. The orchestration layer is responsible for "orchestrating" bits and pieces of data (that come in the form of entity models) into a cohesive response in the form of a Resource model. 

The entity models are essentially a one-to-one mapping between the model and the underlying datasource. They also include information about the data (which members are primary key, which are foreign keys, whether the member is nullable, etc.).

Consequently, because we have Resource Models and Entity Models, we need a way to translate between the two. The translations are accomplished by AutoMapper profiles residing in the Mapping folder.

It is worth noting that a repository layer need not referene a database. Sometimes, a repository layer will be used to interact with a foreign service, or even interact with a file system or any other type of device. At present, the REST Service Wizard only provides support for database oriented repositories, but that doesn't mean you can't include other types of repositories in your service. Neither does it mean that the repository is limited to just the pre-configured generic functions. The author can add new functions to the repository as needed to support custom requirements. 

## Extending our Service ##
Before we begin to extend our service, we will need a database to hold all of our book and authors information. Since, at this time, we only support SQL Server, we have a database definition, located at [Bookstore.sql](https://github.com/mzuniga58/RESTTemplate/blob/main/Scripts/Bookstore.sql). 

Open Microsoft SQL Server Management Studio and create a database called Bookstore. Then, open the above file in Microsoft SQL Server Management Studio while connected to that database, and run it. It will create the Bookstore database we will be using in this tutorial.

### Adding Entity Models ###
Okay, now that we have some data, we're going to want to add our first endpoints to our service to manimulate that data. Before we can create that endpoint, we need to do a few things. First, we need to create an entity model of the data we want to manipulate. The first bit of data we want to manipulate is the categories table. The categories table is the list of categories that a book can belong to, such as Science Fiction or Romance. 

To do that, with your Bookstore service open in Visual Studio, expand the Models folder. Under the Models folder you will see two child folders, EntityModels and ResourceModels. We want to create an entity model for the Categories table, so right-click on the EntityModels folder. When you do, a pop-up menu will appear. On that menu, click on Add REST Entity Model... It should be 3rd on the menu, with the Blue and White MZ logo next to it.

>What are you talking about? I don't see any menu item called "Add REST Entity Model..." with a blue and white MZ logo?
>
>Did you install the Wizard, by clicking on the RESTInstaller.vsix file as described at the beginning of this tutorial? And if so, did it run to completion?
>If you did that, and the menu item still isn't showing, that can sometimes happen if Visual Studio is running a bit slow. Try closing Visual Studio and running it again.
>If after all that, it still isn't showing, try the second method.
>
>Right click on the EntityModels menu and select "Add -> New Item...", or press Shift+Ctrl+A. 
>On the resulting dialog, on the left-hand side, navigate to Visual C# / ASP .NET Core / Web / REST Services.
>There you should see a number of items, all with the blue and white MZ logo. REST Entity Model should be one of those options. Click on that.

Alright, now you should have a dialog asking for the name of your new class. A good standard is to name models in singular form, so call your class **ECategory**. The E stands for "entity". An alternative is **Entity_Category**, or **EntityCategory**. It doesn't really matter, we just need a name to differentiate it from the Resource Category class that we will be making later. I'm going to call mine **ECategory.cs**. (P.S., if you forgot to add the trailing .cs, don't worry, Visual Studio will add it for you.)






