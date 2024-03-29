<h1>REST Service Wizard Tutorial</h1>
<p>Eventually, I will place this template on the Microsoft Store, so that it can be downloaded directly in Visual Studio. However, until then, you will need to download this repository and build the project. I suggest you build it in release mode, but either release or debug will work. Once compiled, using the standard windows explorer, navigate to <i>&period;\RESTTemplate\RESTInstaller\bin\Release</i>. In that folder you will find the file <b>RESTInstaller.vsix</b>. Double click on that file to install the Visual Studio extension. Note, you should shut down all instances of Visual Studio before installing.</p>

<h2>Creating a REST Service</h2>
<p>Once installed, open Visual Studio and select <b>Create a new project</b> from the initial popup window. When the <i>Create a new project</i> dialog appears, select <b>WebAPI</b> in the project types dropdown on the top right side of the dialog. When you do, you will see an entry for <b>REST Service (.Net Core)</b> option, with the blue and white MZ logo next to it.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateAService.png"
     alt="Create a new Project"
     style="float: center; margin-right: 10px;" /></p>
<p>Selet that entry and press <b>next</b>. When you do, the standard Visual Studio <i>Create a project</i> dialog appears. We're going to create a bookstore service that will list books and their authors, so in the Project name field, enter <b>Bookstore</b> and press <b>create</b>. When you do, you will see the REST Service Wizard dialog.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/RESTServiceWizard.png"
     alt="REST Service Wizard dialog"
     style="float: center; margin-right: 10px;" /></p>
<p>When you first open this dialog, the <i>Your name</i>, <i>Email</i> and <i>Project Url</i> fields will be blank. Once you fill them in, they will be pre-populated the next time you run this wizard. All three fields contain information that will be placed in the Swagger document of your project, so do fill them in with meaningful information. In my case, I have filled them in with my name, my email address, and the Url to my GitHub home page.</p>
<p>Next you can choose the .NET Version you wish to build your service in. At present, the only option is .NET 6.0. In the near future, I will be adding support for .NET 7.0 and as time goes on, support for any newer versions that Microsoft produces. You can also choose the database technology that your service will use. There are four options:</p>
<ul>
<li>None</li>
<li>SQL Server</li>
<li>Postgresql</li>
<li>My SQL</li>
</ul>
<p>However, at present, I only have support for SQL Server. You have three other options, all of which are checked by default.</p>
<ul>
<li><b>Use OAuth Authentication</b> - this choice allows your sevice to be protected by OAuth. You will need an OAuth identity provider to take advantage of this option. In this case, I will leave it checked, but we won't really be using it. Nevertheless, you will see how it could be used. Feel free to use it if you do have access to an identity provider.</li>
<li><b>Incorporate RQL</b> - <b>Resource Query Language (RQL)</b> is a query language designed for use in URIs with object style data structures. The language provides powerful filtering capabilities to your endpoints.</li>
<li><b>Incorporate HAL</b> - <b>Hypertext Application Language (HAL)</b> is a simple format that gives a consistent and easy way to hyperlink between resources in your API.</li>
</ul>
<p>Now, press Ok to generate your REST Service. Once it is generated, you can compile it and run it.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/StarterService.png"
     alt="Starter Service"
     style="float: center; margin-right: 10px;" /></p>
<p>It doesn't look like much yet, as we nave not yet defined any resources or endpoints. Nevertheless, you can see the information about yourself and the project in the top-left corner under the title, and users can click on your name to send you email, or click on the website link to visit your website. You can also see the authorize button. Clicking on this button will require you to enter an access token that you get from the identity provider to authorize the user to hit the various endpoints that require it. At this time, of course, we don't have any endpoints that reqire it, so you can igore that button for the time being.</p>
<p>There are some settings you will want to set at this point. In the <b>program.cs</b> file, on line 115, you will see this code:</p>
<pre><code>Description = "&lt;description here&gt;",
</code></pre>
<p>You should replace the "&lt;description here&gt;" with a detailed description of your service. The description can include HTML code and inline styles, so you can make it look very professional. A good description will make your service easier to use for your customers.</p>
<p>Also, you will notices several appSettings.json files, one for each environment your service will run in. The default implementation includes settings files for five environments:</p>
<ul>
<li><b>Development</b> - despite the name, this is the local environment, the one running on your computer.</li>
<li><b>Dev</b> - a specialized development environment</li>
<li><b>QA</b> - the QA environment</li>
<li><b>Staging</b> - a Staging environment</li>
<li><b>Production</b> - the Production environment</li>
</ul>
<p>If your setup doesn't include one of these environments, you can simply delete the appSettings files that don't apply, or you could add others that do apply but aren't included in the default implementation. One of the most important settings is the ConnectionStrings setting found in all the environment specific settings files.</p>
<details>
<summary>Instructions for setting the Default Conntection String</summary>
<p>You need to change the <b>DefaultConnection</b> string in the appSettings&lt;environment&gt;.json file to the connection string appropriate to that environment.<p>
<pre><code>  "ConnectionStrings": {
    //	To do: Replace the following with the database connection string suited to your
    //		   Database server in your QA environment.
    "DefaultConnection": "Server=localdb;Database=master;Trusted_Connection=True;"
  },
</code></pre>
</details>
<p>Collections are returned wrapped in a <b>PagedSet<></b> class. That <b>PagedSet</b> limits the number of records that can be returned in a single call. By default, that limit is set to the <b>BatchLimit</b> of 100 records. You can change this value to whatever you feel is appropriate to your environment. And lastly, the <b>Timeout</b> value, encoded as a <i>TimeSpan</i>, informs the service how long it will wait for a request to be fulfilled. If a request takes longer than this time limit, the request is canceled, and a timeout error is returned. The default is 5 seconds, but you can change that to whatever you feel is appropriate.</p>
<details>
<summary>Instructions for setting the <b>batchLimit</b> and <b>Timeout</b> values</summary>
<p>These are also found in the appSettings&lt;environment&gt;.json file for each environment.</p>
<pre><code>  "ServiceSettings": {
    "BatchLimit": 100,
    "Timeout": "00:00:05"
  }
</code></pre>
</details>
<p>Before we expand our service, let's take a minute to go over some of the features. This REST service is created with a layered architecture. The three main layers are the Presentation Layer (also called the Resource Layer), the Orchestration Layer and the Repository Layer.</p>
<h2>Presentation Layer</h2>
<p>At this point, the presentation layer doesn't exist, aside from the swagger. Soon we will be adding some controllers, and it is the collection of controllers and the endpoints that they define, along with the HAL configurations, that constitute the presentation layer. This is the layer that your users see and interact with. This is the layer they use to obtain and manage the resources the service controls, hence, that is why it is also sometimes called the resource layer. Later in this tutorial, we will show how to create a controller and how to populate the HAL configurations.</p>
<h2>Orchestration Layer</h2>
<p>When the user calls an endpoint in your presentation layer, under the covers, the presentation layer calls the orchestration layer to accomplish the actual work. If the user asks for a parcitular resource, i.e., /books/1234, the preentation layer calls the orchestration layer to get the book whose Id is 1234. Likewise, if the user calls books/name/The%20Wrath%20of%20Isis, the user is calling the service to obtain the book called "The Wrath of Isis" (a wonderfully written book, by the way). The orchestration layer will return the desired result, and in turn, the presentation layer will pass back the result to the caller.</p>
<p>The orchestration layer resides in the Orchestration folder. There are two files there, <b>IOrchestrator</b> and <b>Orchestrator</b>. The <b>IOrchestrator</b> defines, and the <b>Orchestrator</b> implements a set of generic CRUD (Create, Read, Update, Delete) functions that have been pre-defined for you. These generic operations are:</p>
<ul>
<li><b>GetSingleResourceAsync</b> - retrieves a single resource of type T, using an RQL Statement to futher refine the resource.</li>
<li><b>GetResourceCollectionAsync</b> - retrieves a collection of resources of type T, wrapped inside a <b>PagedSet</b>, using an RQL Statement to filter and refine the set.</li>
<li><b>AddResourceAsync</b> - adds a resource of type T to the datastore.</li>
<li><b>UpdateResourceAsync</b> - updates a resource of type T in the datastore, using an RQL Statment to further refine the update.</li>
<li><b>DeleteResourceAsync</b> - deletea a resource or set of resources of type T from the datastore, using an RQL Statement to def.ine which resources are to be deleted.</li>
</ul>
<p>These generic functions are sufficient for simple resources. However, for more complex resources (such as those that contai<b>RqlNode</b>n embedded child resources), you will need to create your own functions. To do so, simply include that function in the <b>IOrchestrator</b> interface and implement it in the <b>Orchestrator</b> object. In your new function, you can often combine (or "orchestrate") the generic functions to accomplish your task. I'll be giving an example of that later in this tutorial.</p>
<p>One other thing about the orchestration layer is its use of RQL. The <b>RqlNode</b> is a compiled version of an RQL Statement. The RQL Statement can be provided by the user in the query portion of the Url (basically, everything after the ?), or it can be hardcoded in the presentation layer by the programmer. Here are two examples:</p>
<pre><code>var node = RqlNode.Parse(Request.QueryString.Value);<br>
var node = RqlNode.Parse($"Author={authorId});
</code></pre>
<p>The first example compiles the query string of the requested Url into an <b>RqlNode</b> object. The second takes the RQL Statement "Author=nnn", and compiles it into an <b>RqlNode</b> object. The <b>RqlNode</b> object contains a structured representation of the statement, or a statement of NOOP if there is no statement. RqlNode.Parse("") will return an <b>RqlNode</b> of NOOP, for example. RQL statments can be simple, like the one shown above, or they can become quite complex.</p>
<pre><code>(in(AuthorId,{auth1},{auth2},{auth3})&category=Scifi)|(like(AuthorName,T*)&category=Fantasy)&select(bookTitle,pubishDate,AuthorName)&limit(1,20)
</code></pre>
<p>This RQL statement will return the collection of books who where written by either *auth1*, *auth2* or *auth3* (these are Author Ids) and whose category is Scifi, OR any Fantasy book written by an auther whose name beings with 'T' (i.e., Thomson, Tiller, Tanner, etc.). It limits the results to include only the book title, the publish date of the book and the authorname, and it gives you only the first 20 results.</p>
<p>For more information on RQL syntax, look here: enter link here</p>
<h2>Repsoitory Layer</h2>
<p>Under the covers, to do its work, the orchestration layer calls the repository layer to obtain and manipulate data in the underlying datastore. The repositoy layer resides under the repositories folder in the <b>IRepository</b> and <b>Repository</b> files. Likw the orchestraton layer, the repository layer comes with pre-configured generic CRUD functions. One thing to notice between the repository layer and the orchestration layer is that the orchestration layer accepts <i>Resource Models</i>, while the repository layer accepts <i>Entity Models</i>. When you think about it it makes sense. The orchestration layer is responsible for "orchestrating" bits and pieces of data (that come in the form of entity models) into a cohesive response in the form of a resource model.</p>
<p>The entity models are essentially a one-to-one mapping between the model and the underlying datasource. They also include information about the data (which members are primary key, which are foreign keys, whether the member is nullable, etc.).</p>
<p>Consequently, because we have resource models and entity mModels, we need a way to translate between the two. The translations are accomplished by AutoMapper profiles residing in the Mapping folder.</p>
<p>It is worth noting that a repository layer need not referene a database. Sometimes, a repository layer will be used to interact with a foreign service, or even interact with a file system or any other type of device. At present, the REST Service Wizard only provides support for database oriented repositories, but that doesn't mean you can't include other types of repositories in your service. Neither does it mean that the repository is limited to just the pre-configured generic functions. The author can add new functions to the repository as needed to support custom requirements.</p>
<h2>Extending our Service</h2>
<p>Before we begin to extend our service, we will need a database to hold all of our book and authors information. Since, at this time, we only support SQL Server, we have a database definition, located at <a href="https://github.com/mzuniga58/RESTTemplate/blob/main/Scripts/Bookstore.sql">Boookstore.sql</a>.</p>
<p>Open Microsoft SQL Server Management Studio and create a database called Bookstore. Then, open the above file in Microsoft SQL Server Management Studio while connected to that database, and run it. It will create the Bookstore database we will be using in this tutorial.</p>
<h3>Adding Entity Models</h3>
<p>Okay, now that we have some data, we're going to want to add our first endpoints to our service to manipulate that data. Before we can create that endpoint, we need to do a few things. First, we need to create an entity model of the data we want to manipulate. The first bit of data we want to define is the categories table. The categories table is the list of categories that a book can belong to, such as Science Fiction or Romance.</p> 
<p>Typically, we create entity model/resource model pairs. These pairs will allow us to write the functions to manipulate the data, i.e., add new items, delete or update existing items, etc. But our categories table is a bit different. It doesn't change. Categories, commonly called Literary genres, are formed by shared literary conventions. Although they do change over time, as new genres emerge and others fade, the change is usually measured in decades, and sometimes centuries.</p>
<p>We could create an entity model/resource model pair for our categories, but it doens't really make that muuch sense. In the C# world, the categories are better represented by an <b>enum</b>. Fortunately, the REST Service makes that easy.</p>
<p>With your Bookstore service open in Visual Studio, expand the Models folder. Under the Models folder you will see two child folders, <b>EntityModels</b> and <b>ResourceModels</b>. We want to create an entity model for the Categories table, but we want to create it as an enum, not a class model. To do that, right-click on the <b>EntityModels</b> folder. When you do, a pop-up menu will appear. On that menu, click on Add REST Entity Model... It should be 3rd on the menu, with the Blue and White MZ logo next to it.</p>
<blockquote><p>What are you talking about? I don't see any menu item called "Add REST Entity Model..." with a blue and white MZ logo?</p>
<p>Did you install the Wizard, by clicking on the RESTInstaller.vsix file as described at the beginning of this tutorial? And if so, did it run to completion?</p>
<p>If you did that, and the menu item still isn't showing, that can sometimes happen if Visual Studio is running a bit slow. Try closing Visual Studio and running it again.</p>
<p>If after all that, it still isn't showing, try the second method.</p>
<blockquote>
<p>Right click on the EntityModels menu and select "Add -> New Item...", or press Shift+Ctrl+A. On the resulting dialog, on the left-hand side, navigate to Visual C# &rArr; ASP .NET Core &rArr; Web &rArr; REST Services. There you should see a number of items, all with the blue and white MZ logo. REST Entity Model should be one of those options. Click on that.<p></blockquote></blockquote>
<p>Alright, now you should have a dialog asking for the name of your new class.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/ChooseName.png"
     alt="Entity Model Generator"
     style="float: left; margin-right: 10px;" /></p>
<p>A good standard is to name models in singular form, so call your class <b>Category</b>. Normally, we prepend a prefix to entity models. I use the letter E. The E stands for "entity". So, normally we would call this the <b>ECategory</b> class. An alternative is <b>Entity_Category</b>, or <b>EntityCategory</b>. It doesn't really matter, we just need a name to differentiate it from the resource model class that we will be making later. However, in this case, we will be creating an enum, and there is no reason to create a resource model (it would simply be exactly the same as the entity model, just with a different class name). So, in this case, I will forego the "E" prefix, and just call it <b>Category.cs</b>. (P.S., if you forgot to add the trailing .cs, don't worry, Visual Studio will add it for you.)</p>
<p>Now you will be presented with the Entity Model Generator dialog.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateEntityModel.png"
     alt="Entity Model Generator"
     style="float: left; margin-right: 10px;" /></p>
<p>The first time you try to create an entity model, the wizard doesn't know anything about your databases, so the top part of the dialog is empty. Click on the Add New Server button.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/AddNewServer.png"
     alt="Add New Server"
     style="float: left; margin-right: 10px;" /></p>
<p>Select the database technology you want to connect to. Remember, we created our database in SQL Server, so choose SQL Server here. Then type in the name of your server, and select either Windows Authority or SQL Server Authority, whichever is appropriate to your installation. If you select SQL Server Authority, you will also need to enter your username and password. Once you have it all complete, click on the "check" button to ensure the wizard can talk to your database. Once you have established a connection, hit OK.</p>
<p>Now, your database is shown at the top of the Entity Model Generator dialog, and the list of databases on that server are shown in the left-hand list box. Select the Bookstore database in that list. When you do, the list of tables for the Bookstore database should appear in the right-hand list. One of those tables should be the Categories table. Select that table.</p>
<p>When you select that table, the "Render as Enum" checkbox becomes enabled. If you click around on the other tables, you will notice that the "Render as Enum" checkbox becomes diabled, and you can't change it. Only certain tables are candidates for enum. If the table contains a single primary key of a numeric type, and contains a sinle string member, then that table is a candidate for enum. Not all tables of this nature are good representations of an enum, but this one is, as it as an int as the primary key and a single name field. It is essentially a key/value pair table, and it has a limited number of rows. Such tables are usually good candidates for an enum. You will have to know your data and choose appropriately.</p>
<p>We know we want Category to be an enum, so select that table, check the "Render as Enum" checkbox and hit OK.</p>
<p>The generator will now generate an enum entity model for you.</p>
<details>
<summary>The generated Category enum</Summary>
<pre><code>using System;<br>
using System.Collections.Generic;<br>
using Tense;<br>
<br>
namespace Bookstore.Models.EntityModels<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Enumerates a list of Categories<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;[Table("Categories", Schema = "dbo", DBType = "SQLSERVER")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;public enum Category : int<br>
&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;ActionAndAdventure<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ActionAndAdventure = 1,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Classics<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Classics = 2,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;ComicBooksOrGraphicNovels<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ComicBooksOrGraphicNovels = 3,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;DetectiveAndMystery<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DetectiveAndMystery = 4,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Fantasy<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Fantasy = 5,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;	HistoricalFiction<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HistoricalFiction = 6,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Horror<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Horror = 7,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;LiteraryFiction<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;LiteraryFiction = 8,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Romance<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Romance = 9,<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;ScienceFiction<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ScienceFiction = 10<br>
&nbsp;&nbsp;&nbsp;&nbsp;}<br>
}</code></pre></details>
<p>You notice that the generator has added some annotations to further describe the table. The <b>Table</b> attribute tells us that this model is for the Categories table under the dbo schema on a SQL Server. That's the only annotation you will get for an enum table. Notice it is also using the <b>Tense</b> namespace. <b>Tense</b> is a nuget package that contains the definition for the <b>Table</b> attribute, and the <b>Member</b> attribute we will use later. That nuget package was already included for you when you first created the REST Service project.</p>
<p>Okay, so now we have the <b>Category</b> enumerator defined. We needed to do that one first, because it will be used in our next set of classes. So, let's create something a bit more interesting. Let's create an entity/resource model pair for some data we do wish to manipulate. Let's create an <b>EBook</b> entity model based off the <b>Books</b> database table.</p>
<p>Once again, right-click on the <b>EntityModels</b> folder, select <i>Add REST Entity Model</i>, enter <b>EBook</b> as the name of the class. Then, in the Entity Model Generator dialog (this time, your SQL Server you used last time is already pre-populated and selected), choose the <b>Bookstore</b> database and select the <b>Books</b> table. We don't want an enum this time, so we want to leave the "Render as Enum" checkbox blank. In this case, you couldn't select it if you tried, because the <b>Books</b> table doesn't have the structure suitable for an enum. That "Render as Enum" check box will be unchecked, and it will be disabled.</p>
<p>Hit OK to render the new class. </p>
<details>
<summary>The generated EBook class</Summary>
<pre><code>using System;<br>
using System.Collections.Generic;<br>
using Tense;<br>
<br>
namespace Bookstore.Models.EntityModels<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;EBook<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;[Table("Books", Schema = "dbo", DBType = "SQLSERVER")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;public class EBook<br>
&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;BookId<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Member(IsPrimaryKey = true, IsIdentity = true, AutoField = true, IsIndexed = true, IsNullable = false, NativeDataType="int")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public int BookId { get; set; }<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Title<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Member(IsNullable = false, Length = 50, IsFixed = false, NativeDataType="varchar")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public string Title { get; set; } = string.Empty;<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;PublishDate<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Member(IsNullable = false, NativeDataType="datetime")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public DateTime PublishDate { get; set; } = DateTime.UtcNow;<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;CategoryId<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Member(IsIndexed = true, IsForeignKey = true, ForeignTableName="Categories", IsNullable = false, NativeDataType="int")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public int CategoryId { get; set; }<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Synopsis<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Member(IsNullable = true, IsFixed = false, NativeDataType="varchar")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public string? Synopsis { get; set; }<br>
&nbsp;&nbsp;&nbsp;&nbsp;}<br>
}
</code></pre></details>
<p>As you can see, it is a one-to-one mapping to the database table with annotactions. We have the <b>Table</b> annotation as we did with the <b>Category</b> enum. We also have <b>Member</b> annotations on each member, telling us if the member represents a primary key, or a foreign key. It also tells us if the member can be null, what database data type it is, and so forth.</p>
<h3>Adding a Resource Model</h3>
<p>Having an entity model is all well and fine, but users don't see entity models. They see resource models. So, let's do that again, this time creating a resource model for the Books table. Go back to the models folder, but this time, right-click on the <b>ResourceModels</b> folder. Once again, there should be an Add REST Resource Model... menu item. Click on that. A dialog appears where you enter the class name. Enter <b>Book</b> this time. <b>Book</b> is the resource model, and <b>EBook</b> is the entity model. This naming convention makes it easy to find the corresponding entity model or resource model, as the case may be. Press Ok to get the Resource Model Generator.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateResourceModel.png"
     alt="Add New Resource"
     style="float: left; margin-right: 10px;" /></p>
<p>This time, a list of entity models appears. Notice that the <b>Category</b> class is conspicously absent from the list. There is never a good reason to create a reosurce model from an enum. They'd just be the same structure with a different class name. Select the entity model you wish to make a Resource model for. That's pretty easy at this point, since we only have one entity model defined. Select <b>EBook</b>, and press OK.</p>
<details>
<summary>The generated Resource model</Summary>
<pre><code>using Tense;<br>
using Tense.Rql;<br>
using Microsoft.AspNetCore.Mvc.ModelBinding;<br>
using Bookstore.Orchestration;<br>
using System.Collections.Generic;<br>
using System.ComponentModel.DataAnnotations;<br>
using Bookstore.Models.EntityModels;<br>
<br>
namespace Bookstore.Models.ResourceModels<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Book<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;[Entity(typeof(EBook))]<br>
&nbsp;&nbsp;&nbsp;&nbsp;public class Book<br>
&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;BookId<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public int BookId { get; set; }<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Title<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public string Title { get; set; } = string.Empty;<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;PublishDate<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public DateTimeOffset PublishDate { get; set; } = DateTimeOffset.UtcNow.ToLocalTime();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;CategoryId<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public Category CategoryId { get; set; }<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Synopsis<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public string? Synopsis { get; set; }<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Checks the resource to see if it is in a valid state to update.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="orchestrator"&gt;The &lt;see cref="IOrchestrator"/&gt; used to orchestrate operations.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="node"&gt;The &lt;see cref="RqlNode"/&gt; that restricts the update.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="errors"&gt;The &lt;see cref="ModelStateDictionary"/&gt; that will contain errors from failed validations.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;returns&gt;&lt;see langword="true"/&gt; if the resource can be updated; &lt;see langword="false"/&gt; otherwise&lt;/returns&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;bool&gt; CanUpdateAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.Clear();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var existingValues = await orchestrator.GetResourceCollectionAsync&lt;Book&gt;(node);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (existingValues.Count == 0)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError("Search", "No matching Book was found.");<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var selectNode = node.ExtractSelectClause();<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (selectNode is null || (selectNode is not null && selectNode.SelectContains(nameof(Title))))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (string.IsNullOrWhiteSpace(Title))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError(nameof(Title), "Title cannot be blank or null.");<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (Title is not null && Title.Length > 50)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError(nameof(Title), "Title cannot exceed 50 characters.");<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (selectNode is null || (selectNode is not null && selectNode.SelectContains(nameof(Synopsis))))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return errors.IsValid;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Checks the resource to see if it is in a valid state to add.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="orchestrator"&gt;The &lt;see cref="IOrchestrator"/&gt; used to orchestrate operations.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="errors"&gt;The &lt;see cref="ModelStateDictionary"/&gt; that will contain errors from failed validations.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;returns&gt;&lt;see langword="true"/&gt; if the resource can be updated; &lt;see langword="false"/&gt; otherwise&lt;/returns&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;bool&gt; CanAddAsync(IOrchestrator orchestrator, ModelStateDictionary errors)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.Clear();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (string.IsNullOrWhiteSpace(Title))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError(nameof(Title), "Title cannot be blank or null.");<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (Title is not null && Title.Length > 50)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError(nameof(Title), "Title cannot exceed 50 characters.");<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;await Task.CompletedTask;<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return errors.IsValid;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Checks the resource to see if it is in a valid state to delete.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="orchestrator"&gt;The &lt;see cref="IOrchestrator"/&gt; used to orchestrate operations.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="node"&gt;The &lt;see cref="RqlNode"/&gt; that restricts the update.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="errors"&gt;The &lt;see cref="ModelStateDictionary"/&gt; that will contain errors from failed validations.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;returns&gt;&lt;see langword="true"/&gt; if the resource can be updated; &lt;see langword="false"/&gt; otherwise&lt;/returns&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public static async Task&lt;bool&gt; CanDeleteAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.Clear();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var existingValues = await orchestrator.GetResourceCollectionAsync&lt;Book&gt;(node);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (existingValues.Count == 0)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError("Search", "No matching Book was found.");<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return errors.IsValid;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;}<br>
}
</code></pre>
</details>
<p>Notice that the new resource model looks pretty much like we'd expect, it has members for each column in the database. However, it has a member called CategoryId, and that member matches the CategoryId in the entity model. However, instead of an int, the CategoryId in our resource model is defined as a Category enum. And that's pretty much what we want, except for one little thing.</p>
<p>Inside our database model books are grouped by category, but in the real world, the world our customers live in, they prefer to think of this grouping as genres. So, to make our customers happy, let's change the name of this member from CategoryId to Genre.</p>
<p>Change the line of code from</p>
<pre><code>public Category CategoryId { get; set; }
</code></pre>
<p>to</p>
<pre><code>public Category Genre { get; set; }
</code></pre>
<p>There, now we are using our Category enum to represent the genre for the book. It's worth noting that this is not an uncommon exercise. Don't take the REST Wizard's word for what any resource model column should be named. Don't take the word of the database either. Make the names meaningful to your customer. Little things like this often make the difference between good software and great software.</p>
<p>Also notice that at the bottom we have three pre-defined methods for validating a book model.</p>
<ul>
<li><b>CanUpdateAsync</b> - this method will be called just before we attempt to update a book in the datastore.</li>
<li><b>CanAddAsync</b> - this method will be called just before we attempt to add a new book to the datastore.</li>
<li><b>CanDeleteAsync</b> - this method will be called just before we attempt to delete a book from the datastore.</li>
</ul>
<p>Let's look at each of these a bit more closely.</p>
<p>In the <b>CanUpdateAsync</b> function, we have a reference to the <b>IOrchestrator</b> interface, an <b>RqlNode</b> and a <b>ModelStateDictionary</b> list of errors. We begin by clearing the list of errors. It should be empty anyway, but it's alwasy a good practice to make sure. During the validation process, if we find anything amiss, we will add the error to the list of errors. If, at the end of the validation process, there are any errors present in our list, then the update will be abandoned and the service will return a <b>BadRequest</b>, listing all the errors we found.</p>
<p>In RQL, the <b>RqlNode</b> is going to contain the information needed to create the WHERE clause in the SQL Statement that will eventually be generated. In other words, the <b>RqlNode</b> tells us which book, or books, are to be updated. The first question we have in our update validation is, does this <b>RqlNode</b> actually specify any books to be updated?</p>
<p>To answer this question, we make this call</p>
<pre><code>var existingValues = await orchestrator.GetResourceCollectionAsync<Books>(node);
</code></pre>
<p>This call tells the orchestrator to get the collection of books that matches the <b>RqlNode</b> specification. If no books are returned, then there are no books that match the specification, and therefore, there is nothing to update. If the <b>Count</b> property is zero, then there are no books to update, and we record that as an error. It is a <b>BadRequest</b>, because the user has asked us to update books that don't exist.</p>
<p>In RQL, an update does not have to be limited to one single resource. The update can update many resources at once. But when you update many resources, you don't want them all to be the same. You typically only want one or two columns to be the same. Now, the book design we have doesn't really lend itself to mass updates, but there are database schemas that do. We can however, for the sake of understanding the concept, conjure up a scenario where we would want to do multiple updates, albiet, not a very realistic one for books.</p>
<p>In the <b>Books</b> table, the synopsis can be null. So, given our list of books, we might want to update all books with a null synopsis, whose publish date was before 1950, and make the synopsis say "classic literature". Not very realistic, I know. Not all books written prior to 1950 are classics. In fact, most of them are not. But we're only doing this for illustration purposes, so, as they say in the literary world, enhance you willing suspension of disbelief, and just go with it.</p>
<p>To do this, we would first have to generate an RQL statement to select such books:</p>
<pre><code>PublishDate&lt;01/01/1950&amp;Synopsis=null
</code></pre>
<p>This RQL statement will select all the books whose publish date was before January 1, 1950 (PublishDate&lt;01/01/1950), and (&amp;) whose Synopsis is null (Synopsis=null). In the incoming model, we would have set the Synopsis value to "classic literature".</p>
<p>But what about the title? We only care to update the synopsis, so the title in our model is likely to be null. But whatever value it is, we don't want to set the title of every book published before 1950 with a null synopsis to that value. We want to leave the title value alone. Likewise, we don't want to change the publish date or the category either. To accomplish our task, we add a select statement to the RQL.</p>
<pre><code>PublishDate&lt;01/01/1950&amp;Synopsis=null&amp;select(Synopsis)
</code></pre>
<p>The select statement, in the case of an update, tells us we only want to update the values included in the select statement (in this case, we only update the synopsis column). All the other columns are to be left unchanged.</p>
<p>In the end, this is the SQL statement that will be generated from this RQL statement:</p>
<pre><code>UPDATE [dbo].[Books]<br>
&nbsp;&nbsp;SET Synopsis = @P0<br>
 WHERE PublishDate<@P1<br>
&nbsp;&nbsp;AND Synopsis IS NULL</code></pre>
<p>Where the @P0 and @P1 represent SQL parameters, where the value of @P0 is 'classic literature' and the value of @P1 is '1950-01-01T00:00:00.000-0500'.</p>
<p>What this means for our validation routine is we don't want to inspect the values of columns that are not going to be included in the update statement. We don't care, for example, what the value of Title is in our incoming model, because in this case, the Title value will never be used and won't have any effect on the operation.</p>
<p>So, the next thing we do in our validation code is to extract the select clause from the RQL statement.</p>
<pre><code>var selectNode = node.ExtractSelectClause();
</code></pre>
<p>There may not be a select clause in the statement, so the returned select clause may be null.</p>
<p>Now, it time to check if the Title value is valid.</p>
<pre><code>if (selectNode is null || (selectNode is not null &amp;&amp; selectNode.SelectContains(nameof(Title))))<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;if (string.IsNullOrWhiteSpace(Title))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError(nameof(Title), "Title cannot be blank or null.");<br>
&nbsp;&nbsp;&nbsp;&nbsp;if (Title is not null && Title.Length > 50)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;errors.AddModelError(nameof(Title), "Title cannot exceed 50 characters.");<br>
}</code></pre>
<p>If the select statement is null then all columns in the table will be updated, and so we do have to check the validity of the Title member. If the select clause is not null, then we only have to check the validity of the Title member if the Title member is included in the select clause.</p>
<p>Finally, if we do have to check the validity of the Title, we do so in the enclosed code. We verify that the Title is not null or composed entirely of whitespace. A book must have a title. Blank titles are not allowed. Finally, we only have room for 50 characters in the title column, so the title the user gives us must be 50 characters or less.</p>
<p>The validation routine is not intended to be considered complete. You can, and should, add your own business logic to it. For example, one bit of logic we may wish to add is to ensure that the new Title (it may, or may not have changed) does not conflict with any other books. We could implement a unique constraint on the book title member in our SQL definition, or we could ensure that uniqueness here with code. However you want to do it is up to you. In our design, the size of the synopsis is unlimited (well, limited to the maximum text size that SQL Server supports, which is 8,000 characters.) You might decide to limit it to something smaller, 2,000 characters say.</p>
<p>Notice that the select clause logic is missing from the <b>CanAddAsync</b> function. That is because the add function does not recognize RQL. You can put an RQL statement in there if you wish, but it will be ignored.</p>
<p>Likewise, in the delete validation, we do use the RQL statement to generate the WHERE clause, but the select statement is ignored. When deleting, we don't care about individual columns, we're going to delete them anyway. We just want to know which records to delete.</p>
<p>The validation routines aren't limited to just the object being validated. You can also include dependency validations. For example, you may not wish to delete any books if there are existing reviews assigned to them. You may require the user to first delete all reviews associated with a book before you delete the book. Or, in your orchestration, you can delete all reviews assigned to a book before you delete the book itself. It's up to you how you want to design your system.</p>
<h3>Mapping Between Resource and Entity</h3>
<p>When we eventually get to writing our controller, the user is going to give us a resource model. But, the repository doesn't understand resource models. It understands entity models. It goes without saying then, that we need a method to translate between entity and resource models. We need a Resource &rArr; Entity transformation, and we need an Entity &rArr; Resource translation.</p>
<p>To do this, we use Automapper. Let's create the translation routines for Books.</p>
<p>Right-click on the Mapping folder. When you do, you will see an entry called Add REST Mapping... Choose that entry. You will be given a dialog to enter the new class name. Call it BooksProfile. Next you'll be presented with a dialog that contains a dropdown list of all the resource models. Select <b>Books</b> and press OK.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateMapping.png"
     alt="Create Mapping"
     style="float: left; margin-right: 10px;" /></p>
<details>
<summary>The generated Mapping code</Summary>
<pre><code>using System;<br>
using System.Linq;<br>
using Microsoft.Extensions.Configuration;<br>
using System.Collections.Generic;<br>
using Bookstore.Models.EntityModels;<br>
using Bookstore.Models.ResourceModels;<br>
using AutoMapper;<br>
<br>
namespace Bookstore.Mapping<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Book Profile for AutoMapper<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;public class BookProfile : Profile<br>
&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;the Book Profile<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public BookProfile()<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;//&nbsp;Creates a mapping to transform a Book model instance (the source)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;//&nbsp;into a EBook model instance (the destination).<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CreateMap<Book, EBook>()<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.BookId, opts =&gt; opts.MapFrom(source =&gt; source.BookId))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.Title, opts =&gt; opts.MapFrom(source =&gt; source.Title))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.PublishDate, opts =&gt; opts.MapFrom(source =&gt; source.PublishDate.UtcDateTime))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.CategoryId, opts =&gt; opts.MapFrom(source =&gt;(int) source.Genre))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.Synopsis, opts =&gt; opts.MapFrom(source =&gt; source.Synopsis));<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;//&nbsp;Creates a mapping to transform a EBook model instance (the source)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;//&nbsp;into a Book model instance (the destination).<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CreateMap<EBook, Book>()<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.BookId, opts =&gt; opts.MapFrom(source =&gt; source.BookId))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.Title, opts =&gt; opts.MapFrom(source =&gt; source.Title))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.PublishDate, opts =&gt; opts.MapFrom(source =&gt; new DateTimeOffset(source.PublishDate).ToLocalTime()))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.Genre, opts =&gt; opts.MapFrom(source =&gt; (Category) source.CategoryId))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.ForMember(destination =&gt; destination.Synopsis, opts =&gt; opts.MapFrom(source =&gt; source.Synopsis));<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;}<br>
}</code></pre>
</details>
<p>This is a standard Automapper mapping. The CreateMap&lt;source,destination&gt; function translates the source type to the destination type. The first translations translates a <b>Book</b> resource model to an <b>EBook</b> entity Model. The second translations does the opposite, translating an <b>EBook</b> entity model to a <b>Book</b> resource model. Notice that the <b>CategoryId</b> is mapped to the <b>Genre</b> column in both transformations.</p>
<p>The REST Service Mapping Generator typically does a pretty good job at creating the mapping, but there are scenarios where it cannot interpret the programmer's intent. For example, suppose, for some reason, you decided to change the name of the Synopsis column to Description. Now, our entity model has a Synopsis column, and our resource model has a column called Description. There is nothing that ties these two columns together, and so the resulting mapping model will not be able to generate the mapping between these two columns. Instead, it will have a list of TO DO comments, telling you that the Synopsis column is left unmapped on one direction, and the Description member is left unmapped in the other. It is now up to the programmer to resolve those unmapped columns.</p>
<p>Now that we have our models, and our translations, we can finally create some endpoints.</p>
<h3>Creating a Controller</h3>
<p>Endpoints live in controllers, and the standard naming convention for a controller is resourcesController. That is to say, the plural name of the resource followed by "Controller." We have our book models so now we need to create the <b>BooksController</b>.</p>
<p>Right-click on the <b>Controllers</b> folder, and select "Add REST Controller...". For the class name, enter <b>BooksController</b> and press OK. The Controller Generator dialog appears.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/CreateController.png"
     alt="Create Controller"
     style="float: left; margin-right: 10px;" /></p>
<p>The top dropdown box contains the list of resource models. Select <b>Book</b>. The second dropdown lists the set of OAuth policies that you have defined for your service. These policies are defined in the appSettings.json file.</p>
<pre><code>"OAuth2": {
	"Policies": [
		{
		"Policy": "policy",
		"Scopes": [ "scope" ]
		}
	]
}</code></pre>
<p>The "Policy" entry defines the name of the policy. It is these names you see in the dropdown. You can create as many different policies as you need. The "Scopes" entry defines the list of scopes that this policy supports. Given an access token (which you obtain from your identity provider) that contains at least one of these scopes, then this policy will allow you to access the function. If your access token does not contain any of these scopes, you will not be allowed to access the function, and the service will return <b>Unauthorized</b>.</p>
<p>When this wizard creates the controller, all of the endpoints will be protected with the policy you choose. Or, you can choose the default value of <b>anonymous</b>. The <b>anonymous</b> policy allows anyone to hit your endpoint.</p>
<p>Not all endpoints in a controller must have the same policy. You can pick and choose. For example, you might set your GET functions to <b>anonymous</b>, allowing anyone to read data from your server, while setting the PUT, POST and DELETE funtions to some other policy you define. That means, anyone can read the data, but they will need a specific access token to manipulate the data.</p>
<p>For now, let's just leave the policy at <b>anonymous</b>, letting anyone use our service.</p>
<p>Press OK to generate the controller.</p>
<details>
<summary>The generated Controller code</Summary>
<pre><code>using System;<br>
using System.Collections.Generic;<br>
using System.Linq;<br>
using System.Net;<br>
using System.Net.Mime;<br>
using System.Security.Claims;<br>
using System.Text.Json;<br>
using System.Threading.Tasks;<br>
using Microsoft.AspNetCore.Authorization;<br>
using Microsoft.AspNetCore.Mvc;<br>
using Microsoft.AspNetCore.Mvc.ModelBinding;<br>
using Microsoft.Extensions.Logging;<br>
using Microsoft.Extensions.DependencyInjection;<br>
using Bookstore.Models.ResourceModels;<br>
using Bookstore.Orchestration;<br>
using Tense;<br>
using Tense.Rql;<br>
using Serilog.Context;<br>
using Swashbuckle.AspNetCore.Annotations;<br>
<br>
namespace Bookstore.Controllers<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Book Controller<br>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;[ApiVersion("1.0")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;[ApiController]<br>
&nbsp;&nbsp;&nbsp;&nbsp;public class BooksController : ControllerBase<br>
&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;value&gt;A generic interface for logging where the category name is derrived from<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;the specified &lt;see cref="BooksController"/&gt; type name.&lt;/value&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;private readonly ILogger&lt;BooksController&gt; _logger;<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;value&gt;The interface to the orchestration layer.&lt;/value&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;private readonly IOrchestrator _orchestrator;<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Instantiates a BooksController<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="logger"&gt;A generic interface for logging where the category name is derrived from<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;the specified &lt;see cref="BooksController"/&gt; type name. The logger is activated from dependency injection.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="orchestrator"&gt;The &lt;see cref="IOrchestrator"/&gt; interface for the Orchestration layer. The orchestrator is activated from dependency injection.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public BooksController(ILogger&lt;BooksController&gt; logger, IOrchestrator orchestrator)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger = logger;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_orchestrator = orchestrator;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Returns a collection of Books<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="200"&gt;A collection of Books&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="400"&gt;The RQL query was malformed.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="401"&gt;The user is not authorized to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="403"&gt;The user is not allowed to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[HttpGet]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Route("books")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[AllowAnonymous]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[SupportRQL]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedSet&lt;Book&gt;))]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; GetBooksAsync()<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var node = RqlNode.Parse(Request.QueryString.Value);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var errors = new ModelStateDictionary();<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (!node.ValidateMembers&lt;Book&gt;(errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var resourceCollection = await _orchestrator.GetResourceCollectionAsync&lt;Book&gt;(node);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return Ok(resourceCollection);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Returns a Book<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="bookId" example="123">The BookId of the Book.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="400"&gt;The RQL query was malformed.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="401"&gt;The user is not authorized to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="403"&gt;The user is not allowed to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="404"&gt;The requested resource was not found.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;[HttpGet]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;[Route("books/{bookId}")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;[AllowAnonymous]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;[SupportRQL]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Book))]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; GetBookAsync(int bookId)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var node = RqlNode.Parse($"BookId={bookId}")<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.Merge(RqlNode.Parse(Request.QueryString.Value));<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var errors = new ModelStateDictionary();<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (!node.ValidateMembers&lt;Book&gt;(errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var resource = await _orchestrator.GetSingleResourceAsync&lt;Book&gt;(node);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (resource is null)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return NotFound();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return Ok(resource);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&lt;Adds a Book
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/remarks&gt;Add a Book to the datastore.&lt;/remarks&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/response code="201"&gt;The new Book was successfully added.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/response code="400"&gt;The request failed one or more validations.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/response code="401"&gt;The user is not authorized to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/response code="403"&gt;The user is not allowed to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[HttpPost]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Route("books")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[AllowAnonymous]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.Created, Type = typeof(Book))]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Consumes("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; AddBookAsync([FromBody] Book resource)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ModelStateDictionary errors = new();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (await resource.CanAddAsync(_orchestrator, errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;resource = await _orchestrator.AddResourceAsync&lt;Book&gt;(resource);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return Created($"{Request.Scheme}://{Request.Host}/books/{resource.BookId}", resource);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;else<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Update a Book<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;remarks&gt;Update a Book in the datastore.&lt;/remarks&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="204"&gt;The Book was successfully updated in the datastore.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="400"&gt;The request failed one or more validations.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="401"&gt;The user is not authorized to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="403"&gt;The user is not allowed to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[HttpPut]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Route("books")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[AllowAnonymous]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.NoContent)]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Consumes("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; UpdateBookAsync([FromBody] Book resource)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var node = RqlNode.Parse($"BookId={resource.BookId}")<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.Merge(RqlNode.Parse(Request.QueryString.Value));<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ModelStateDictionary errors = new();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (node.ValidateMembers&lt;Book&gt;(errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (await resource.CanUpdateAsync(_orchestrator, node, errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;await _orchestrator.UpdateResourceAsync&lt;Book&gt;<br>(resource, node);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return NoContent();<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;<br><br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp; a Book<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;param name="bookId" example="123"&gt;The BookId of the Book.&lt;/param&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;remarks&gt;Deletes a Book in the datastore.&lt;/remarks&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="204"&gt;The Book was successfully deleted.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="400"&gt;The request failed one or more validations.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="401"&gt;The user is not authorized to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="403"&gt;The user is not allowed to acquire this resource.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="405"&gt;The resource could not be deleted.&lt;/response&gt;<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[HttpDelete]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Route("books/{bookId}")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[AllowAnonymous]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.NoContent)]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Consumes("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; DeleteBookAsync(int bookId)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var node = RqlNode.Parse($"&BookId={bookId}");<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var errors = new ModelStateDictionary();<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (!node.ValidateMembers&lt;Book&gt;(errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (await Book.CanDeleteAsync(_orchestrator, node, errors))<br>
&nbsp;&nbsp;&nbsp;&nbsp;{<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;await _orchestrator.DeleteResourceAsync&lt;Book&gt;(node);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return NoContent();<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return StatusCode((int)HttpStatusCode.MethodNotAllowed, errors);<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}<br>
&nbsp;&nbsp;&nbsp;&nbsp;}<br>
}</code></pre></details>
<p>Compile and run your new service<p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/Website1.png"
     alt="Running WebSite"
     style="float: left; margin-right: 10px;" /></p>
<p>Let's take a look at what our service can do. You can see we now have five new endpoints. There are two GET endpoints, one for retrieving a single book and one for retrieving a collection of books. The collection endpoints looks like this:</p>
<pre><code>/books
</code></pre>
<p>That endpoint is deceptively simple. Just execute it from swagger, and you will get all the books in the collection. The result is wrapped inside of a <b>PagedSet&lt;&gt;</b> class. What that means is, the set is paged, and it has a limit on how many resources it will deliver in a single call. The limit is configurable. It's set as the batch limit in appSettings.json for each environment.</p>
<p>Let's look at the annotations for that endpoint:</p>
<pre><code>&nbsp;&nbsp;&nbsp;&nbsp;[HttpGet]<br>
&nbsp;&nbsp;&nbsp;&nbsp;[Route("books")]<br>
&nbsp;&nbsp;&nbsp;&nbsp;[AllowAnonymous]<br>
&nbsp;&nbsp;&nbsp;&nbsp;[SupportRQL]<br>
&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedSet&lt;Book&gt;))]<br>
&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]<br>
</code></pre>
<p>The endpoint responds to the GET Verb and is located at /books. It has the <i>AllowAnonymous</i> attribute, so anyone can call this endpoint. It supports RQL and returns a <b>PagedSet&lt;Book&gt;></b> response. It can take <i>application/hal+json</i>, <i>application/hal.v1+json</i>, <i>application/json</i> or <i>application/vnd.v1+json</i> in the accept header. If the user specifies either of the 'hal' media types, the response will include HAL syntax.</p>
<blockquote>
Note: If you try it out right now, you won't see any HAL syntax, or only very limited HAL in collection responses. This is because we haven't configured the HAL responses yet. Once they are configured, you'll be able to see the HAL responses.
</blockquote>
<p>Let's go ahead and try it out. Just click on the Blue GET button to expand it, and the click on the "Try it out" button to enable the endpoint in swagger. Press the blue Execute button to call the endpoint.</p>
<p>Here is the response:</p>
<pre><code>{
  "count": 20,
  "start": 1,
  "pageSize": 20,
  "_embedded": {
    "items": [
      {
        "bookId": 1,
        "title": "The Wrath of Isis",
        "publishDate": "2019-01-15T18:00:00-06:00",
        "categoryId": "HistoricalFiction",
        "synopsis": "A mysterious device from the future sends Alister and Allison back to the past, where they appear before the ancient Roman Emperor Constantine. Mistaking the young couple as the Goddess Isis and the God Osiris and fearing their wrath, Constantine abandons his plans to promote Christianity as the official state religion of Rome. Upon returning to the present, the intrepid time travelers discover that they have inadvertently altered history. Desperate to reintroduce Christianity to the world, Alister uses the power of the mysterious device to propel himself to religious stardom. But in this new reality, his sister Amanda, gifted haruspex and a devout follower of the Goddess Isis, views the reinstatement of Christianity as nothing less than the end of her world. Amanda uses the device in an attempt to thwart Alister’s goals. Will the faith of the Goddess prevail, allowing Amanda to save her world? A thought-provoking alternate history science fiction adventure from Michael Zuniga."
      },
      {
        "bookId": 2,
        "title": "The Last Wish (The Witcher, 1)",
        "publishDate": "1993-12-02T18:00:00-06:00",
        "categoryId": "Fantasy",
        "synopsis": "Geralt of Rivia is a witcher. A cunning sorcerer. A merciless assassin.\r\nAnd a cold-blooded killer. \r\nHis sole purpose: to destroy the monsters that plague the world. But not everything monstrous-looking is evil, and not everything fair is good...and in every fairy tale there is a grain of truth."
      }
    ]
  }
}</code></pre>
<p>The actual response is bigger, and includes all the books in the table. We're only showing the first two here to conserve space. You will notice the "Count" field. The Count field tells you how many total resources are in the result set. If there were 10,000 books in our database, this number would be 10000. The next number tells you where in the set the first record resides. In this case, the start value is 1, so the first value in the collection is the first book in the entire set. The next value, pageSize, tells you how many books are included in this page.</p>
<p>As it happens, there are only 20 books in our example database, and since 20 is less than the maximum batch size of 100, you get the entire set.</p>
<p>But we can alter that by using some RQL. Let's try it again, only this time, in the RQL parameter, enter:</p>
<pre><code>limit(1,5)
</code></pre>
<p>The limit clause of RQL has the syntax Limit(&lt;start&gt;,&lt;pagesize&gt;). This statement informs the service that you only want to return 5 books, starting with the first book. Run that, and you will see that the pagesize is now 5, and only the first 5 books were returned. To see the next 5 books, enter</p>
<pre><code>limit(5,5)
</code></pre>
<p>We can also do some other things. Suppose we want the list of books that were published prior to 1960. To do that, enter the following RQL statement:</p>
<pre><code>publishDate&lt;1/1/1960
</code></pre>
<p>Now, the returned value shows only those books that were published before 1960. How does this happen, you ask? Well, let's take a closer look. Here is the endpoint for getting a collection of books.</p>
<pre></code>
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Returns a collection of Books
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="200"&gt;A collection of Books&lt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="400"&gt;The RQL query was malformed.&lt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="401"&gt;The user is not authorized to acquire this resource.&lt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="403"&gt;The user is not allowed to acquire this resource.&lt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;[HttpGet]
&nbsp;&nbsp;&nbsp;&nbsp;[Route("books")]
&nbsp;&nbsp;&nbsp;&nbsp;[SupportRQL]
&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedSet&lt;Book&gt;))]
&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]
&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; GetBooksAsync()
&nbsp;&nbsp;&nbsp;&nbsp;{
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var node = RqlNode.Parse(Request.QueryString.Value);

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var errors = new ModelStateDictionary();
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (!node.ValidateMembers&lt;Book&gt;(errors))
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var resourceCollection = await _orchestrator.GetResourceCollectionAsync&lt;Book&gt;(node);
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return Ok(resourceCollection);
&nbsp;&nbsp;&nbsp;&nbsp;}</code></pre>
<p>When we make our call, swagger composes the Url like so:</p>
<pre></code>https://localhost:19704/books?publishDate&lt;1/1/1960
</code></pre>
<p>So, that is the Url we get in our request. First, we compile the query into an <b>RqlNode</b> object. The RQL is in the Url that the user sent. It's everything after the questoin mark.</p>
<p>Now that we have an <b>RqlNode</b> representation of the RQL Statement, we want to validate it against our model. The <b>RqlNode</b>.Parse function produces an <b>RqlNode</b> that is model agnostic. For example, we could write this RQL Statement:</p>
<pre></code>Status=Active
</code></pre>
<p>That is a perfectly valid RQL statement. The problem is, there is no such member as "Status" in our <b>Book</b> model, making that RQL Statement invalid for our purposes. So, to take care of that, we first create an empty <b>ModelStateDictionary</b>. The <b>ModelStateDictionary</b> will hold the collection of errors we discover during any validation. If there are any errors, we simply return <b>BadRequest</b> with the collection of errors we found and return that to the user.</p>
<p>To see if all the members included in our <b>RqlNode</b> pertain to our model, we simply call the <b>ValidateMember&lt;T&gt;</b> function on the node. This function inspects all the PROPERTY nodes in the <b>RqlNode</b> and verifies that they are valid members of the \<T\> (in this case, \<<b>Book</b>\>) type. The function will return *true* if all the members it contains are valid members of the type; otherwise, it will return <i>false</i>. If it does return <i>false</i>, we simply return <b>BadRequest</b> with those errors.</p>
<p>If the <b>RqlNode</b> is valid, then we call the orchestrator to do our work for us. We call the generic <b>GetResourceCollectionAsync</b> function, passing the &lt;Book&gt; type, and passing the compiled <b>RqlNode</b>. That function returns our desired collection, which we simply pass back to the user with the OK (200) HTTP status code.</p>
<h3>HAL Configuration</h3>
<p>We have our endpoints, but HAL is not yet configured. Try out the second GET function, the one that returns a single book, at the /books/{bookid} route. Enter 1 for the book id. Notice that you get back this book:</p>
<pre><code>{
  "bookId": 1,
  "title": "The Wrath of Isis",
  "publishDate": "2019-01-15T18:00:00-06:00",
  "genre": "HistoricalFiction",
  "synopsis": "A mysterious device from the future sends Alister and Allison back to the past, where they appear before the ancient Roman Emperor Constantine. Mistaking the young couple as the Goddess Isis and the God Osiris and fearing their wrath, Constantine abandons his plans to promote Christianity as the official state religion of Rome. Upon returning to the present, the intrepid time travelers discover that they have inadvertently altered history. Desperate to reintroduce Christianity to the world, Alister uses the power of the mysterious device to propel himself to religious stardom. But in this new reality, his sister Amanda, gifted haruspex and a devout follower of the Goddess Isis, views the reinstatement of Christianity as nothing less than the end of her world. Amanda uses the device in an attempt to thwart Alister’s goals. Will the faith of the Goddess prevail, allowing Amanda to save her world? A thought-provoking alternate history science fiction adventure from Michael Zuniga."
}</code></pre>
<p>This, by the way, is a wonderful book, without a doubt destined to go down in history as one of the greatest American Literary classics. You should do yourself a favor and buy 20 copies, give them to your friends. You can purchase the <a href="https://www.amazon.com/Wrath-Isis-Chronicles-Temporal-Lens/dp/B09H8Z65P3/ref=sr_1_1?crid=1BJIVJHK6JRP9&keywords=wrath+of+isis+hardcover&qid=1653601816&s=books&sprefix=wrath+of+isis+hard+cover%2Cstripbooks%2C97&sr=1-1">Hard Cover copy here</a> or the <a href="https://www.amazon.com/gp/product/1792984006/ref=dbs_a_def_rwt_bibl_vppi_i1">Paperback edition here</a>. Truely one of the greatest books ever written (I should know, I wrote it).</p>
<p>Okay, enough shamelss self-promotion. Aside from the obvious - that it really does look like a great book - you also may notice that there is no HAL information in the result set. In order to correct that, we need to configure HAL for this controller. To do that, right-click on the <b>Configuration</b> folder and select Add Hal Configuration. Call it BooksConfiguration when the name dialog appears, and press OK.</p>
<p><img src="https://github.com/mzuniga58/RESTTemplate/blob/main/Images/AddConfiguration.png"
     alt="Add Configuration"
     style="float: left; margin-right: 10px;" /></p>
<p>When the HAL Configuration Generator dialog appears, select Book for the resource and select BooksController for the controller. Press OK.</p>
<p>Now, re-compile, and go and select that Wonderful Book, The Wrath of Isis, BookId 1 once again. This time, you can see the Hal information.</p>
<p>It isn't much yet, because it's still a relativly simple endpoint. Let's complicate it a bit. Books have reviews. Let's add them to our service.<p>
<ul>
<li><b>EBookReview</b> - add an entitiy model, call it EBookReview. Model it from the <b>Reviews</b> database table.</li>
<li><b>BookReview</b> - add a resource model, call it BookReview. Model it from the <b>EBookReview</b> entity Model.</li>
<li><b>BookReviewProfile</b> - create a mapping between the <b>BookReview</b> and <b>EBookReview</b> models. Call it <b>BookReviewProfile</b></li>
<li><b>BookReviewsConroller</b> - create a controller for the Book Reviews.</li>
<li><b>BookReviewsConfiguration</b> - add the HAL Configuration for the BookReviews.</li>
</ul>
<p>Now we have endpoints that allow us to get and manipulate book reviews. One very nice feature would be an endpoint that allows us to retrive all the book reviews for a particular book. The endpoint would look like this:</p>
<pre><code>/books/{bookid}/reviews
</code></pre>
<p>WIth RQL, that becomes very simple. The endpoint should reside in the BooksController. The code to get all reviews already exists in the BookReviewsController. However, becuase that same endpoint supports RQL, we already have the code that return all the reviews for a particular book. Run the /bookReviews endpoint from swagger. You notice that it returns all the reviews for all the books. You also notice that one of the members of a book review is BookId. So, we an use this RQL statement to limit the returned reviews to the reviews for a single book...</p>
<pre><code>/bookReviews?BookId=1
</code></pre>
<p>That call returns all the reviews for just The Wrath of Isis. By the way, on the topic of Book Reviews, if by chance you do buy The Wrath of Isis, don't forget to go back to Amazon and write a short review. It's doesn't have to be long. You could just write "Great book. Highly recommended." or even "What trash!! Don't waste your time or money on this piece of garbage." Honestly, as an Author, I'd rather get a review like that last one than getting no review at all. You see, the more reviews, the higher the book gets in Amazon searches, and the more people will see it. So, even a horribly bad review is good for a book, provided there are counterbalancing good reviews to go with it. So, anytime you buy a book, any book, leave a review.</p>
<p>Okay, so now all we need to do is to copy that code into a new endpoint in the Books controller.</p>
<pre><code>&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;summary&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;Returns a collection of BookReviews
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;/summary&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="200"&gt;A collection of BookReviews&gt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="400"&gt;The RQL query was malformed.&gt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="401"&gt;The user is not authorized to acquire this resource.&gt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;///&nbsp;&lt;response code="403"&gt;The user is not allowed to acquire this resource.&gt;/response&gt;
&nbsp;&nbsp;&nbsp;&nbsp;[HttpGet]
&nbsp;&nbsp;&nbsp;&nbsp;[Route("books/{bookid}/reviews")]
&nbsp;&nbsp;&nbsp;&nbsp;[AllowAnonymous]
&nbsp;&nbsp;&nbsp;&nbsp;[SupportRQL]
&nbsp;&nbsp;&nbsp;&nbsp;[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedSet<BookReview>))]
&nbsp;&nbsp;&nbsp;&nbsp;[Produces("application/hal+json", "application/hal.v1+json", MediaTypeNames.Application.Json, "application/vnd.v1+json")]
&nbsp;&nbsp;&nbsp;&nbsp;public async Task&lt;IActionResult&gt; GetBookReviewsAsync(int bookid)
&nbsp;&nbsp;&nbsp;&nbsp;{
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var node = RqlNode.Parse($"BookId={bookid}")
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.Merge(RqlNode.Parse(Request.QueryString.Value));

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;_logger.LogInformation("{s1} {s2}", Request.Method, Request.Path);

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var errors = new ModelStateDictionary();
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if (!node.ValidateMembers<Book>(errors))
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return BadRequest(errors);

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var resourceCollection = await _orchestrator.GetResourceCollectionAsync<BookReview>(node);
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;return Ok(resourceCollection);
&nbsp;&nbsp;&nbsp;&nbsp;}</code></pre>
<p>All we did here is copied the first endpoint from the BookReviewsController into the BooksController. We changed the route from <i>bookReviews</i> to <i>books/{bookid}/reviews</i>.  Then, we changed the construction of the RQL node. In this case, we want only the reviews that partain to a particular book (bookid). We've already seen that the RQL statement that accomplishes this is BookId={bookid}. So, all we need to do is to hardcode that RQL statement in our code. That's what this line does.</p>
<pre><code>var node = RqlNode.Parse($"BookId={bookid}")
</code></pre>
<p>Now the next line is optional. Do you want to give your called the ability to further refine the reviews? Perhaps the caller only wants the reviews for this book that have been written this year. Or maybe they only want to see the 5 star reviews, or maybe just the 1 star reviews. They can write an RQL statment to do that, if you allow them to. To allow it, we leave the [SupportRQL] annotation on the endpoint, and we take what the user gives us (<b>Request.QueryString.Value</b>), compile that into an <b>RqlNode</b>, and merge that node into the node we've already created. The combined node will be the node we use to obtain our list. It limits the list to only the reviews for this book (that's the part we hardcoded) and whatever other filters the user added, if any. The rest of the code remains the same.</p>
<p>One last thing. Because we changed our controller, the HAL configuration is no longer up to date. You can either manually fix it, or just delete the HAL Configuration for the BooksController and then regenerate it. I'm lazy, so I choose the former.</p>
<p>Okay, let's try it out. Compile and run that endpoint. If you selected 1 for the bookid, you'll find that the Wrath of Isis has only one review. Also, if you run the /books/{bookid} endpoint once again, you'll see more HAL links. In particular, you'll see a currie for the /books/{bookid}/reviews link.</p>