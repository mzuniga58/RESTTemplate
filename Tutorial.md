# REST Service Wizard Tutorial #
Eventually, I will place this template on the Microsoft Store, so that it can be downloaded directly in Visual Studio. However, until then, you will need to download this repository and build the project. I suggest you build it in release mode, but either release or debug will work. Once compiled, using the standard windows explorer, navigate to 

```
.\RESTTemplate\RESTInstaller\bin\Release
```

In that folder you will find the file **RESTInstaller.vsix**. Double click on that file to install the Visual Studio extension. Note, you should shut down all instances of Visual Studio before installing.

## Creating a REST Service ##
Once installed, open Visual Studio and select **Create a new project** from the initial popup window. When the "Create a new project" dialog appears, select **WebAPI** in the project types dropdown on the top right side of the dialog. When you do, you will see an entry for REST Service (.Net Core) option, with the blue and white MZ logo next to it.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateAService.png "Create a new project")

Selet that entry and press **next**. When you do, the standard Visual Studio "Create a project" dialog appears. We're going to create a bookstore service that will list books and their authors, so in the Project name field, enter "Bookstore" and press **create**. When you do, you will see the REST Service Wizard dialog.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/RESTServiceWizard.png "REST Service Wizard dialog")

When you first open this dialog, the *Your name*, *Email* and *Project Url* fields will be blank. Once you fill them in, they will be pre-populated the next time you run this wizard. All three fields contain information that will be placed in the Swagger document of your project, so do fill them in with meaningful information. In my case, I have filled them in with my name, my email address, and the Url to my GitHub home page.

Next you can choose the .NET Version you wish to build your service in. At present, the only option is .NET 6.0. In the near future, I will be adding support for .NET 7.0 and as time goes on, support for any newer versions that Microsoft produces. You can also choose the database technology that your service will use. There are four options:

- None
- SQL Server
- Postgresql
- My SQL

However, at present, I only have support for SQL Server. You have three other options, all of which are checked by default.

- **Use OAuth Authentication** - this choice allows your sevice to be protected by OAuth. You will need an OAuth identity provider to take advantage of this option. In this case, I will leave it checked, but we won't really be using it. Nevertheless, you will see how it could be used. Feel free to use it if you do have access to an identity provider.
- **Incorporate RQL** - **Resource Query Language (RQL)** is a query language designed for use in URIs with object style data structures. The language provides powerful filtering capabilities to your endpoints.
- **Incorporate HAL** - **Hypertext Application Language (HAL)** is a simple format that gives a consistent and easy way to hyperlink between resources in your API.

Now, press Ok to generate your REST Service. Once it is generated, you can compile it and run it.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/StarterService.png "Starter Service")

It doesn't look like much yet, as we nave not yet defined any resources or endpoints. Nevertheless, you can see the information about yourself and the project in the top-left corner under the title, and users can click on your name to send you email, or click on the website link to visit your website. You can also see the authorize button. Clicking on this button will require you to enter an access token that you get from the identity provider to authorize the user to hit the various endpoints that require it. At this time, of course, we don't have any endpoints that reqire it, so you can igore that button for the time being.

Before we expand our service, let's take a minute to go over some of the features. This REST service is created with a layered architecture. The three main layers are the Presentation Layer (also called the Resource Layer), the Orchestration Layer and the Repository Layer.

## Presentation Layer ##
At this point, the presentation layer doesn't exist, aside from the swagger. Soon we will be adding some controllers, and it is the collection of controllers and the endpoints that they define, along with the HAL configurations, that constitute the presentation layer. This is the layer that your users see and interact with. This is the layer they use to obtain and manage the resources the service controls, hence, that is why it is also sometimes called the resource layer. Later in this tutorial, we will show how to create a controller and how to populate the HAL configurations.

## Orchestration Layer ##
When the user calls an endpoint in your presentation layer, under the covers, the presentation layer calls the orchestration layer to accomplish the actual work. If the user asks for a parcitular resource, i.e., /books/1234, the preentation layer calls the orchestration layer to get the book whose Id is 1234. Likewise, if the user calls books/name/The%20Wrath%20of%20Isis, the user is calling the service to obtain the book called "The Wrath of Isis" (a wonderfully written book, by the way). The orchestration layer will return the desired result, and in turn, the presentation layer will pass back the result to the caller.

The orchestration layer resides in the Orchestration folder. There are two files there, **IOrchestrator** and **Orchestrator** (which supports the **IOrchestrator** interface). The **IOrchestrator** defines, and the **Orchestrator** implements a set of generic CRUD (Create, Read, Update, Delete) functions, that have been pre-defined for you. These generic operations are:

- **GetSingleResourceAsync** - retrieves a single resource of type T, using an RQL Statement to futher refine the resource.
- **GetResourceCollectionAsync** - retrieves a collection of resources of type T, wrapped inside a **PagedSet**, using an RQL Statement to filter and refine the set.
- **AddResourceAsync** - adds a resource of type T to the datastore
- **UpdateResourceAsync** - updates a resource of type T in the datastore, using an RQL Statment to further refine the update
- **DeleteResourceAsync** - deletea a resource or set of resources of type T from the datastore, using an RQL Statement to define which resources are to be deleted.

These generic functions are sufficient for simple resources. However, for more complex resources (such as those that contain embedded child resources), you will need to create your own functions. To do so, simply include that function in the **IOrchestrator** interface and implement it in the **Orchestrator** object. In your new function, you can often combine (or "orchestrate") the generic functions to accomplish your task. I'll be giving an example of that later in this tutorial.

One other thing about the orchestration layer is its use of RQL. The **RqlNode** is a compiled version of an RQL Statement. The RQL Statement can be provided by the user in the query portion of the Url (basically, everything after the ?), or it can be hardcoded in the presentation layer by the programmer. Here are two examples:

```
var node = RqlNode.Parse(Request.QueryString.Value);
var node = RqlNode.Parse($"Author={authorId});
```

The first example compiles the query string of the requested Url into an **RqlNode** object. The second takes the RQL Statement "Author=nnn", and compiles it into an **RqlNode** object. The **RqlNode** object contains a structured representation of the statement, or a statement of NOOP if there is no statement. RqlNode.Parse("") will return an **RqlNode** of NOOP, for example. RQL statments can be simple, like the one shown above, or they can become quite complex. 

```
(in(AuthorId,{auth1},{auth2},{auth3})&category=Scifi)|(like(AuthorName,T*)&category=Fantasy)&select(bookTitle,pubishDate,AuthorName)&limit(1,20)
```

This RQL statement will return the collection of books who where written by either *auth1*, *auth2* or *auth3* (these are Author Ids) and whose category is Scifi, OR any Fantasy book written by an auther whose name beings with 'T' (i.e., Thomson, Tiller, Tanner, etc.). It limits the results to include only the book title, the publish date of the book and the authorname, and it gives you only the first 20 results.

For more information on RQL syntax, look here: enter link here

## Repsoitory Layer ##
Under the covers, to do its work, the orchestration layer calls the repository layer to obtain and manipulate data in the underlying datastore. The repositoy layer resides under the repositories folder in the **IRepository** and **Repository** files. Likw the orchestraton layer, the repository layer comes with pre-configured generic CRUD functions. One thing to notice between the repository layer and the orchestration layer is that the orchestration layer accepts *Resource models*, while the repository layer accepts *Entity models*. When you think about it it makes sense. The orchestration layer is responsible for "orchestrating" bits and pieces of data (that come in the form of entity models) into a cohesive response in the form of a resource model. 

The entity models are essentially a one-to-one mapping between the model and the underlying datasource. They also include information about the data (which members are primary key, which are foreign keys, whether the member is nullable, etc.).

Consequently, because we have resource models and entity mModels, we need a way to translate between the two. The translations are accomplished by AutoMapper profiles residing in the Mapping folder.

It is worth noting that a repository layer need not referene a database. Sometimes, a repository layer will be used to interact with a foreign service, or even interact with a file system or any other type of device. At present, the REST Service Wizard only provides support for database oriented repositories, but that doesn't mean you can't include other types of repositories in your service. Neither does it mean that the repository is limited to just the pre-configured generic functions. The author can add new functions to the repository as needed to support custom requirements. 

## Extending our Service ##
Before we begin to extend our service, we will need a database to hold all of our book and authors information. Since, at this time, we only support SQL Server, we have a database definition, located at [Bookstore.sql](https://github.com/mzuniga58/RESTTemplate/blob/main/Scripts/Bookstore.sql). 

Open Microsoft SQL Server Management Studio and create a database called Bookstore. Then, open the above file in Microsoft SQL Server Management Studio while connected to that database, and run it. It will create the Bookstore database we will be using in this tutorial.

### Adding Entity Models ###
Okay, now that we have some data, we're going to want to add our first endpoints to our service to manipulate that data. Before we can create that endpoint, we need to do a few things. First, we need to create an entity model of the data we want to manipulate. The first bit of data we want to define is the categories table. The categories table is the list of categories that a book can belong to, such as Science Fiction or Romance. 

Typically, we create entity model/resource model pairs. These pairs will allow us to write the functions to manipulate the data, i.e., add new items, delete or update existing items, etc. But our categories table is a bit different. It doesn't change. Categories, commonly called Literary genres, are formed by shared literary conventions. Although they do change over time, as new genres emerge and others fade, the change is usually measured in decades, and sometimes centuries.

We could create an entity model/resource model pair for our categories, but it doens't really make that muuch sense. In the C# world, the categories are better represented by an **enum**. Fortunately, the REST Service makes that easy.

With your Bookstore service open in Visual Studio, expand the Models folder. Under the Models folder you will see two child folders, **EntityModels** and **ResourceModels**. We want to create an entity model for the Categories table, but we want to create it as an enum, not a class model. To do that, right-click on the **EntityModels** folder. When you do, a pop-up menu will appear. On that menu, click on Add REST Entity Model... It should be 3rd on the menu, with the Blue and White MZ logo next to it.

>What are you talking about? I don't see any menu item called "Add REST Entity Model..." with a blue and white MZ logo?
>
>Did you install the Wizard, by clicking on the RESTInstaller.vsix file as described at the beginning of this tutorial? And if so, did it run to completion?
>If you did that, and the menu item still isn't showing, that can sometimes happen if Visual Studio is running a bit slow. Try closing Visual Studio and running it again.
>If after all that, it still isn't showing, try the second method.
>
>Right click on the EntityModels menu and select "Add -> New Item...", or press Shift+Ctrl+A. 
>On the resulting dialog, on the left-hand side, navigate to Visual C# / ASP .NET Core / Web / REST Services.
>There you should see a number of items, all with the blue and white MZ logo. REST Entity Model should be one of those options. Click on that.

Alright, now you should have a dialog asking for the name of your new class. 

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/ChooseName.png "Entity Model Generator")

A good standard is to name models in singular form, so call your class **Category**. Normally, we prepend a prefix to entity models. I use the letter E. The E stands for "entity". So, normally we would call this the **ECategory** class. An alternative is **Entity_Category**, or **EntityCategory**. It doesn't really matter, we just need a name to differentiate it from the resource model class that we will be making later. However, in this case, we will be creating an enum, and there is no reason to create a resource model (it would simply be exactly the same as the entity model, just with a different class name). So, in this case, I will forego the "E" prefix, and just call it **Category.cs**. (P.S., if you forgot to add the trailing .cs, don't worry, Visual Studio will add it for you.)

Now you will be presented with the Entity Model Generator dialog.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateEntityModel.png "Entity Model Generator")

The first time you try to create an entity model, the wizard doesn't know anything about your databases, so the top part of the dialog is empty. Click on the Add New Server button.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/AddNewServer.png "Add New Server")

Select the database technology you want to connect to. Remember, we created our database in SQL Server, so choose SQL Server here. Then type in the name of your server, and select either Windows Authority or SQL Server Authority, whichever is appropriate to your installation. If you select SQL Server Authority, you will need to enter your username and password. Once you have it all complete, click on the "check" button to ensure the wizard can talk to your database. Once you have established a connection, hit OK.

Now, your database is shown at the top of the Entity Model Generator dialog, and the list of databases on that server are shown in the left-hand list box. Select the Bookstore database in that list. When you do, the list of tables for the Bookstore database should appear in the right-hand list. One of those tables should be the Categories table. Select that table and press OK.

The generator will now generate an entity model for you. The resulting code should look like this:

```
using System;
using System.Collections.Generic;
using Tense;

namespace Bookstore.Models.EntityModels
{
	///	<summary>
	///	ECategory
	///	</summary>
	[Table("Categories", Schema = "dbo", DBType = "SQLSERVER")]
	public class ECategory
	{
		///	<summary>
		///	CategoryId
		///	</summary>
		[Member(IsPrimaryKey = true, IsIdentity = true, AutoField = true, IsIndexed = true, IsNullable = false, NativeDataType="int")]
		public int CategoryId { get; set; }

		///	<summary>
		///	Name
		///	</summary>
		[Member(IsNullable = false, Length = 50, IsFixed = false, NativeDataType="varchar")]
		public string Name { get; set; } = string.Empty;
	}
}
```

You notice that the generator has added some annotations to further describe the table. The Table attribute tells us that this model is for the Categories table under the dbo schema on a SQL Server. The Member attribute gives us information about each member. The CategoryId member is a primary key and it is an identity field. It is not nullable, and the native SQL Server data type is int. Likewise, the Name field is not nullable, and it has a native SQL Server data type of varchar, and is limited to 50 characters.

### Adding a Resource Model ###
But, users don't see entity models, they see resource models. So, let's do that again, this time creating a resource model for the Categories table. Go back to the models folder, but this time, right-click on the ResourceModels folder. Once again, there should be an Add REST Resource Model... menu item. Click on that. A dialog appears where you enter the class name. Enter Category this time. Category is the resource model, and ECategory is the entity model. This naming convention makes it easy to find the corresponding entity model or resource model, as the case may be. Press Ok to get the Resource Model Generator.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateResourceModel.png "Add New Resource")

This time, a list of entity models appears. Select the entity model you wish to make a Resource model for. That's pretty easy at this point, since we only have one entity model defined. Select ECategory.

Now, we could create a resource model for Category that matches the database table. But the category of books don't channge very often. In fact, there hasn't been a new category of books in years. So, instead of creating a table look-up every time we want to know which cateogory a book belongs to, it makes more sense to create an enum. You will notice there is a render as enum check box on the form. Check it and press OK.

The resulting code should look like this:

```
using System;
using Tense;
using Tense.Rql;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bookstore.Orchestration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Bookstore.Models.EntityModels;

namespace Bookstore.Models.ResourceModels
{
	///	<summary>
	///	Category
	///	</summary>
	[Entity(typeof(ECategory))]
	public enum Category : int
	{
		///	<summary>
		///	Action and Adventure
		///	</summary>
		ActionandAdventure = 1,

		///	<summary>
		///	Classics
		///	</summary>
		Classics = 2,

		///	<summary>
		///	Comic Books or Graphic Novels
		///	</summary>
		ComicBooksorGraphicNovels = 3,

		///	<summary>
		///	Detective and Mystery
		///	</summary>
		DetectiveandMystery = 4,

		///	<summary>
		///	Fantasy
		///	</summary>
		Fantasy = 5,

		///	<summary>
		///	Historical Fiction
		///	</summary>
		HistoricalFiction = 6,

		///	<summary>
		///	Horror
		///	</summary>
		Horror = 7,

		///	<summary>
		///	Literary Fiction
		///	</summary>
		LiteraryFiction = 8,

		///	<summary>
		///	Romance
		///	</summary>
		Romance = 9,

		///	<summary>
		///	Science Fiction
		///	</summary>
		ScienceFiction = 10
	}
}
```

Now, we can't manipulate the categories table with an enum. If we wanted to manipulate that table, we'd need to create an additional resource model that is not an enum. But that isn't very interesting, as this table isn't likely to change in the near future. We can always do that later, if the need arises. For now, we're going to use the enum version and treat categories as if it is carved in stone. 

So, that's it for categories. Now, let's do something a bit more interesting. Let's create an entity model and resource model for something that does change. Go back to the EntityModles folder and create an entity model for books (remember to call it EBooks). Then create a Resource Model for Books.

Your code should look like this:

```
using System;
using System.Collections.Generic;
using Tense;

namespace Bookstore.Models.EntityModels
{
	///	<summary>
	///	EBooks
	///	</summary>
	[Table("Books", Schema = "dbo", DBType = "SQLSERVER")]
	public class EBooks
	{
		///	<summary>
		///	BookId
		///	</summary>
		[Member(IsPrimaryKey = true, IsIdentity = true, AutoField = true, IsIndexed = true, IsNullable = false, NativeDataType="int")]
		public int BookId { get; set; }

		///	<summary>
		///	Title
		///	</summary>
		[Member(IsNullable = false, Length = 50, IsFixed = false, NativeDataType="varchar")]
		public string Title { get; set; } = string.Empty;

		///	<summary>
		///	PublishDate
		///	</summary>
		[Member(IsNullable = false, NativeDataType="datetime")]
		public DateTime PublishDate { get; set; } = DateTime.UtcNow;

		///	<summary>
		///	CategoryId
		///	</summary>
		[Member(IsIndexed = true, IsForeignKey = true, ForeignTableName="Categories", IsNullable = false, NativeDataType="int")]
		public int CategoryId { get; set; }

		///	<summary>
		///	Synopsis
		///	</summary>
		[Member(IsNullable = true, IsFixed = false, NativeDataType="varchar")]
		public string? Synopsis { get; set; }
	}
}
```

```
using System;
using Tense;
using Tense.Rql;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bookstore.Orchestration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Bookstore.Models.EntityModels;

namespace Bookstore.Models.ResourceModels
{
	///	<summary>
	///	Books
	///	</summary>
	[Entity(typeof(EBooks))]
	public class Books
	{
		///	<summary>
		///	BookId
		///	</summary>
		public int BookId { get; set; }

		///	<summary>
		///	Title
		///	</summary>
		public string Title { get; set; } = string.Empty;

		///	<summary>
		///	PublishDate
		///	</summary>
		public DateTimeOffset PublishDate { get; set; } = DateTimeOffset.UtcNow.ToLocalTime();

		///	<summary>
		///	CategoryId
		///	</summary>
		public int CategoryId { get; set; }

		///	<summary>
		///	Synopsis
		///	</summary>
		public string? Synopsis { get; set; }

		///	<summary>
		///	Checks the resource to see if it is in a valid state to update.
		///	</summary>
		///	<param name="orchestrator">The <see cref="IOrchestrator"/> used to orchestrate operations.</param>
		///	<param name="node">The <see cref="RqlNode"/> that restricts the update.</param>
		///	<param name="errors">The <see cref="ModelStateDictionary"/> that will contain errors from failed validations.</param>
		///	<returns><see langword="true"/> if the resource can be updated; <see langword="false"/> otherwise</returns>
		public async Task<bool> CanUpdateAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)
		{
			errors.Clear();

			var existingValues = await orchestrator.GetResourceCollectionAsync<Books>(node);

			if (existingValues.Count == 0)
			{
				errors.AddModelError("Search", "No matching Books was found.");
			}

			var selectNode = node.ExtractSelectClause();
			if (selectNode is null || (selectNode is not null && selectNode.SelectContains(nameof(Title))))
			{
				if (string.IsNullOrWhiteSpace(Title))
					errors.AddModelError(nameof(Title), "Title cannot be blank or null.");
				if (Title is not null && Title.Length > 50)
					errors.AddModelError(nameof(Title), "Title cannot exceed 50 characters.");
			}
			if (selectNode is null || (selectNode is not null && selectNode.SelectContains(nameof(Synopsis))))
			{
			}
			return errors.IsValid;
		}

		///	<summary>
		///	Checks the resource to see if it is in a valid state to add.
		///	</summary>
		/// <param name="orchestrator">The <see cref="IOrchestrator"/> used to orchestrate operations.</param>
		/// <param name="errors">The <see cref="ModelStateDictionary"/> that will contain errors from failed validations.</param>
		/// <returns><see langword="true"/> if the resource can be updated; <see langword="false"/> otherwise</returns>
		public async Task<bool> CanAddAsync(IOrchestrator orchestrator, ModelStateDictionary errors)
		{
			errors.Clear();

			if (string.IsNullOrWhiteSpace(Title))
				errors.AddModelError(nameof(Title), "Title cannot be blank or null.");
			if (Title is not null && Title.Length > 50)
				errors.AddModelError(nameof(Title), "Title cannot exceed 50 characters.");

			await Task.CompletedTask;

			return errors.IsValid;
		}

		///	<summary>
		///	Checks the resource to see if it is in a valid state to delete.
		///	</summary>
		///	<param name="orchestrator">The <see cref="IOrchestrator"/> used to orchestrate operations.</param>
		///	<param name="node">The <see cref="RqlNode"/> that restricts the update.</param>
		///	<param name="errors">The <see cref="ModelStateDictionary"/> that will contain errors from failed validations.</param>
		///	<returns><see langword="true"/> if the resource can be updated; <see langword="false"/> otherwise</returns>
		public static async Task<bool> CanDeleteAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)
		{
			errors.Clear();

			var existingValues = await orchestrator.GetResourceCollectionAsync<Books>(node);

			if (existingValues.Count == 0)
			{
				errors.AddModelError("Search", "No matching Books was found.");
			}

			return errors.IsValid;
		}
	}
}
```

Notice that the new resource model looks pretty much like we'd expect, it has members for each column in the database. However, it has a member called CategoryId, and that member matches the CategoryId in the entity model. But that isn't what we want. We took the trouble to create an enum to represent categories in our resource models, and that is what we want to use here.

Change the line of code from

```
		public int CategoryId { get; set; }
```

to

```
		public Category Category { get; set; }
```

There, now we are using our category enum to represent the category for the book.

Also notice, however, that at the bottom we have three pre-defined method for validating a book model.

- **CanUpdateAsync** - this method will be called just before we attempt to update a book in the datastore.
- **CanAddAsync** - this method will be called just before we attempt to add a new book to the datastore.
- **CanDeleteAsync** - this method will be called just before we attempt to delete a book from the datastore.

Let's look at each of these a bit more closely.

In the CanUpdateAsync function, we have a reference to the **IOrchestrator** interface, an **RqlNode** and a **ModelStateDictionary** list of errors. We begin by clearing the list of errors. It should be empty anyway, but it's alwasy a good practice to make sure. During the validation process, if we find anything amiss, we will add the error to the list of errors. If at the end of the validation process, there are any errors present in our list, then the update will be abandoned, and the service will return a BadRequest, listing all the errors we found.

In Rql, the Rql node is going to contain the information needed to create the WHERE clause in the SQL that will eventually be generated. In other words, the Rql node tells us which book, or books, are to be updated. The first question we have in our update validation is, does this Rql node actually specify any books to be updated?

To answer this question, we make this call

```
			var existingValues = await orchestrator.GetResourceCollectionAsync<Books>(node);
```

This call tells the orchestrator to get the collection of books that matches the Rql node specification. If no books are returned, then there are no books that match the specification, and therefore, there is nothing to update. IF the **Count** property is zero, then there are no books to update, and we record that as an error. It is a BadRequest, because the user has asked us to update books that don't exist.

In Rql, an update does not have to be limited to one single resource. The update can update many resources at once. But when you update many resources, you don't want them all to be the same, you typicallyl just want one or two columns to be the same. Now, the book design we have doesn't really lend itself to mass updates, but there are database schemas that do. We can however, conjure up a scenario where we would want to do multiple updates, albiet, not a very realistic one for books.

In books, the synopsis can be null. So, given our list of books, we might want to update all books with a null synopsis, whose publish date was before 1950, and make the synopsis say "classic literature".

To do this, we would first have to generate an RQL statement to select such books:

```
PublishDate<01/01/1950&Synopsis=null
```

This Rql statement will select all the books whose Synopsis is null, and whose publish date was before January 1, 1950. In the incoming model, we would have set the Synopsis value to "classic literature". 

But what about the title? We only care to update the synopsis, so the title in our model is likely to be null. But whatever value it is, we don't want to set the title of every book published before 1950 with a null synopsis to that value. We want to leave the value alone. Likewise, we don't want to change the publish date or the category either. To accomplish our task, we add a select statement to the Rql.

```
PublishDate<01/01/1950&Synopsis=null&select(Synopsis)
```

The select statement, in the case of an update, tells us we only want to update the values included in the select statement (in this case, we only update the synopsis column). All the other columns are to be left unchanged.

In the end, this is the SQL that will be generated for this Rql statement:

```
UPDATE [dbo].[Books]
   SET Synopsis = @P0
 WHERE PublishDate<@P1
   AND Synopsis IS NULL
```

Where the @P0 and @P1 represent SQL parameters, where the value of @P0 is 'classic literature' and the value of @P1 is '01/01/1950T00:00:00.000-0500'.

What this means for our validation routine, is we don't want to inspect the values of columns that are not going to be included in the update statement. We don't care, for example, what the value of Title is in our incoming model, because in this case, the Title value will never be used and won't have any effect on the operation.

So, the next thing we do in our validation code is to extract the select clause from the Rql statement. There may not be a select clause in the statement, so the returned select clause may be null.

Now, it time to check if the Title value is valid.

```
			if (selectNode is null || (selectNode is not null && selectNode.SelectContains(nameof(Title))))
			{
				if (string.IsNullOrWhiteSpace(Title))
					errors.AddModelError(nameof(Title), "Title cannot be blank or null.");
				if (Title is not null && Title.Length > 50)
					errors.AddModelError(nameof(Title), "Title cannot exceed 50 characters.");
			}
```

If the select statement is null then all columns in the table will be updated, and so we do have to check the validity of the Title member. If the select clause is not null, then we only have to check the validity of the Title member if the Title member is included in the select clause.

Finally, if we do have to check the validity of the Title, we do so in the enclose code. We verify that the Title is not null or composed entirely of whitespace. A book must have a title. Blank titles are not allowed. Finally, we only have room for 50 characters in the title column, so the title the user gives us must be 50 characters or less.

The validation routine is not intended to be considered complete. You can, and should, add your own business logic to it. For example, one bit of logic we may wish to add is to ensure that the new Title (it may, or may not have changed) does not conflict with any other books. We could implement a unique constraint on the book title member in our SQL definition, or we could ensure that uniqueness here with code. However you want to do it is up to you. In our design, the size of the synopsis is unlimited (well, limited to the maximum text size that SQL Server supports, which is 8000 characters.) You might decide to limit it to something smaller, 2000 characters say. 

Notice that the select clause logic is missing from the CanAddAsync function. That is because the add function does not recognize Rql. You can put an Rql statement in there if you wish, but it will be ignored.

Likewise, in the delete validation, we do use the Rql statement to generate the WHERE clause, but the select statement is ignored. When deleting, we don't care about individual columns, we're going to delete them anyway. We just want to know which records to delete.

The validation routines aren't limited to just the object being validated. You can also include dependency validations. For example, you may not wish to delete any books if there are existing reviews assigned to them. You may require the user to first delete all reviews associated with a book before you delete the book. Or, in your orchestration, you can delete all reviews assigned to a book before you delete the book. It's up to you how you want to design your system.

### Mapping Between Resource and Entity ###
When we eventually get to writing our controller, the user is going to give us a resource model. But, the repository doesn't understand resource models. It understands Entity models. It goes without saying then, that we need a method to translate between Entity and Resource models. We need a Resource -> Entity transformation, and we need an Entity -> Resource translation.

To do this, we use Automapper. Let's create the translation routines for Books.

Right-click on the Mapping folder. When you do, you will see an entry called Add REST Mapping... Chose that entry. You will be given a dialog to enter the new class name. Call it BooksProfile.

![alt text](https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateMapping.png "Create Mapping")


