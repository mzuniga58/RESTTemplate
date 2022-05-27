
USE [Bookstore]

IF OBJECT_ID (N'BooksByAuthor', N'V') IS NOT NULL 
BEGIN
	DROP VIEW BooksByAuthor
END

IF OBJECT_ID (N'AuthorsByBook', N'V') IS NOT NULL 
BEGIN
	DROP VIEW AuthorsByBook
END

IF EXISTS (SELECT * FROM sys.foreign_keys 
           WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reviews_Books]') 
             AND parent_object_id = OBJECT_ID(N'[dbo].[Books]'))
BEGIN
	ALTER TABLE Books
	DROP CONSTRAINT FK_Reviews_Books
END


IF EXISTS (SELECT * FROM sys.foreign_keys 
           WHERE object_id = OBJECT_ID(N'[dbo].[FK_AuthorBooks_Books]') 
             AND parent_object_id = OBJECT_ID(N'[dbo].[Books]'))
BEGIN
	ALTER TABLE Books
	DROP CONSTRAINT FK_AuthorBooks_Books
END

IF EXISTS (SELECT * FROM sys.foreign_keys 
           WHERE object_id = OBJECT_ID(N'[dbo].[FK_AuthorBooks_Authors]') 
             AND parent_object_id = OBJECT_ID(N'[dbo].[Authors]'))
BEGIN
	ALTER TABLE Authors
	DROP CONSTRAINT FK_AuthorBooks_Books
END


IF EXISTS (SELECT * FROM sys.foreign_keys 
           WHERE object_id = OBJECT_ID(N'[dbo].[FK_Books_Categories]') 
             AND parent_object_id = OBJECT_ID(N'[dbo].[Categories]'))
BEGIN
	ALTER TABLE Categories
	DROP CONSTRAINT FK_AuthorBooks_Books
END

IF OBJECT_ID (N'Reviews', N'U') IS NOT NULL 
  DROP Table [dbo].[Reviews]


IF OBJECT_ID (N'AuthorBooks', N'U') IS NOT NULL 
  DROP Table [dbo].[AuthorBooks]
  
  IF OBJECT_ID (N'Books', N'U') IS NOT NULL 
  DROP Table [dbo].[Books]

IF OBJECT_ID (N'Categories', N'U') IS NOT NULL 
  DROP Table [dbo].[Categories]

IF OBJECT_ID (N'Authors', N'U') IS NOT NULL 
  DROP Table [dbo].[Authors]

GO
/****** Object:  Table [dbo].[AuthorBooks]    Script Date: 5/19/2022 11:39:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuthorBooks](
	[AuthorId] [int] NOT NULL,
	[BookId] [int] NOT NULL,
 CONSTRAINT [PK_AuthorBooks] PRIMARY KEY CLUSTERED 
(
	[AuthorId] ASC,
	[BookId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Authors]    Script Date: 5/19/2022 11:39:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Authors](
	[AuthorId] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](50) NOT NULL,
	[LastName] [varchar](50) NOT NULL,
	[Website] [varchar](max) NULL,
 CONSTRAINT [PK_Authors] PRIMARY KEY CLUSTERED 
(
	[AuthorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Books]    Script Date: 5/19/2022 11:39:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Books](
	[BookId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[PublishDate] [datetime] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[Synopsis] [varchar](max) NULL,
 CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED 
(
	[BookId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 5/19/2022 11:39:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 5/19/2022 11:39:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewId] [int] IDENTITY(1,1) NOT NULL,
	[BookId] [int] NOT NULL,
	[ReviewAuthor] [varchar](200) NULL,
	[Review] [varchar](max) NOT NULL,
	[ReviewDate] [datetimeoffset](7) NOT NULL,
	[Stars] [decimal](18, 0) NOT NULL,
 CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED 
(
	[ReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[AuthorBooks]  WITH CHECK ADD  CONSTRAINT [FK_AuthorBooks_Authors] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[Authors] ([AuthorId])
GO
ALTER TABLE [dbo].[AuthorBooks] CHECK CONSTRAINT [FK_AuthorBooks_Authors]
GO
ALTER TABLE [dbo].[AuthorBooks]  WITH CHECK ADD  CONSTRAINT [FK_AuthorBooks_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([BookId])
GO
ALTER TABLE [dbo].[AuthorBooks] CHECK CONSTRAINT [FK_AuthorBooks_Books]
GO
ALTER TABLE [dbo].[Books]  WITH CHECK ADD  CONSTRAINT [FK_Books_Categories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([CategoryId])
GO
ALTER TABLE [dbo].[Books] CHECK CONSTRAINT [FK_Books_Categories]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([BookId])
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Books]
GO
USE [Bookstore]
GO

/****** Object:  View [dbo].[AuthorsByBook]    Script Date: 5/27/2022 8:59:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[AuthorsByBook]
AS
SELECT        dbo.Authors.AuthorId, dbo.Authors.FirstName, dbo.Authors.LastName, dbo.Authors.Website, dbo.AuthorBooks.BookId
FROM            dbo.AuthorBooks INNER JOIN
                         dbo.Authors ON dbo.AuthorBooks.AuthorId = dbo.Authors.AuthorId;
GO

USE [Bookstore]
GO

/****** Object:  View [dbo].[BooksByAuthor]    Script Date: 5/27/2022 9:00:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[BooksByAuthor]
AS
SELECT        dbo.AuthorBooks.AuthorId, dbo.Books.BookId, dbo.Books.Title, dbo.Books.PublishDate, dbo.Books.CategoryId, dbo.Books.Synopsis
FROM            dbo.AuthorBooks INNER JOIN
                         dbo.Books ON dbo.AuthorBooks.BookId = dbo.Books.BookId;
GO


INSERT INTO [dbo].[Categories] ( Name ) values
 ( 'Action and Adventure' ),
 ( 'Classics' ),
 ( 'Comic Books or Graphic Novels' ),
 ( 'Detective and Mystery' ),
 ( 'Fantasy' ),
 ( 'Historical Fiction' ),
 ( 'Horror' ),
 ( 'Literary Fiction' ),
 ( 'Romance' ),
 ( 'Science Fiction' )

GO

declare @AuthorId int
declare @BookId int

INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Michael', 'Zuniga', 'https://www.amazon.com/Wrath-Isis-Chronicles-Temporal-Lens/dp/1792984006/ref=sr_1_2?crid=175SGRKIHZCJN&keywords=The+Wrath+of+Isis&qid=1652980005&s=books&sprefix=the+wrath+of+isis%2Cstripbooks%2C92&sr=1-2' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'The Wrath of Isis', '01/16/2019', 6, 'A mysterious device from the future sends Alister and Allison back to the past, where they appear before the ancient Roman Emperor Constantine. Mistaking the young couple as the Goddess Isis and the God Osiris and fearing their wrath, Constantine abandons his plans to promote Christianity as the official state religion of Rome. Upon returning to the present, the intrepid time travelers discover that they have inadvertently altered history. Desperate to reintroduce Christianity to the world, Alister uses the power of the mysterious device to propel himself to religious stardom. But in this new reality, his sister Amanda, gifted haruspex and a devout follower of the Goddess Isis, views the reinstatement of Christianity as nothing less than the end of her world. Amanda uses the device in an attempt to thwart Alister’s goals. Will the faith of the Goddess prevail, allowing Amanda to save her world? A thought-provoking alternate history science fiction adventure from Michael Zuniga.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'KDubya', '05/16/2019', 5, 'I finally got around to finishing this. I must say, for a first book from this author, it was an enjoyable read. When reading books with temporal story lines, it can be easy to confuse a reader, but the use was justified and flowed logically. Good job Z, I am looking forward to your next book.')




INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Andrzej', 'Sapkowski', 'https://www.amazon.com/Andrzej-Sapkowski/e/B001ICAMAW/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'The Last Wish (The Witcher, 1)', '12/03/1993', 5, 'Geralt of Rivia is a witcher. A cunning sorcerer. A merciless assassin.
And a cold-blooded killer. 
His sole purpose: to destroy the monsters that plague the world. But not everything monstrous-looking is evil, and not everything fair is good...and in every fairy tale there is a grain of truth.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Metz', '11/29/2018', 3, 'I wanted to like this more. My friend highly recommended it as a great dark fantasy story with swords and sorcery, dungeons and dragons.
It has those things, but it doesn''t come off as very exciting or enchanting. Rather, we see the world through a tired grey lens, where humans are often worse than monsters, and the monsters are rarely evil incarnate. Rather they''re more just hungry like animals. And Geralt seems sick and tired of hunting them.
More broadly, the tales in this first anthology are a mix of twists on the old fairy tales, maybe mixed up with Eastern European folklore I''m less familiar with. The twists are mostly deconstructive, yet often end up less dark and gritty than the originals from the Grimms or from Hans Christian Anderson. They''re not uproariously funny enough to be parodies, either, though a few elements were worth a chuckle.
So, what does the author do well here? The way he weaves in and out from an overarching story down to the short stories he wants to tell is clever and interesting. The dialogues back and forth between Geralt and the other characters in the world are full of double entendre and puns (albeit some is lost in translation), but really help to build the world and the stories in a far more entertaining way than any of the actions and events that take place within the story itself.
So if your favorite part of fantasy or D&D is the part where you banter with the NPCs about quests and listen to spoony bards weave ballads out of half-truths and grog in the tavern, this series is definitely for you.')

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'S E Lindberg', '06/20/2016', 5, 'Andrzej Sapkowski’s Geralt of Rivia is a “Witcher,” a superhuman trained to defeat monsters. After hundreds of years killing creatures, there are fewer threats and witchers. Actually there is less hunting monsters than Geralt sleuthing mysterious altercations. Sapkowski’s stories have conflicts that are not lone-Witcher-in-the-wild vs. monster conflict; they are more humans/vs strange forces in which Geralt referees (and usually kills). His investigative methods are a bit rougher than Sherlock Holmes. Each story was as if Conan was dumped into the Grimm''s Fairy tales. But all is not grim. Lots of humor present is reminiscent of Fritz Leiber’s Fafhrd and the Gray Mouser series. Humans tend to persecute or shun the weird witchers; sustaining future witchers is addressed as the seeds of an apprenticeship are sown.
Geralt has dialogue with antagonists often. Lengthy interrogations are common. This approach allows for funny banter, philosophizing, and entertaining information-dumps. This makes for a fast, entertaining read. Sapkowski stands out as a leading non-English writer. No map, table of contents (TOC), or glossary were featured in the paperback translation. I provide the TOC below. The structure reveals the over-arching narrative of “the Voice of Reason” which attempts to connect all the others. This works pretty well, but is not always smooth. This was designed as an introduction to the series. I was impressed enough to order the Sword of Destiny when I was only half way through. It is not until the third book does a dedicated novel emerge. The series and the games continue to this day with books 7 and 8 awaiting English translation (as of 2016).')


 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Sword of Destiny (The Witcher, 2)', '04/12/1998', 5, 'Geralt is a Witcher, a man whose magic powers, enhanced by long training and a mysterious elixir, have made him a brilliant fighter and a merciless hunter. Yet he is no ordinary killer: his targets are the multifarious monsters and vile fiends that ravage the land and attack the innocent.
Sword of Destiny is the follow up to The Last Wish, and together they are the perfect introduction to a one of a kind fantasy world.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Bryan Desmond', '01/04/2020', 3, 'I think I am landing on four stars for Sword of Destiny, the second collection of Witcher stories. Of the six stories I''d give four of them four stars, and two of them five. So four seems fair.
The first thing I noted is that Sword of Destiny has a different translator than The Last Wish. David French in this case, rather than Danusia Stok. I noticed a little stiltedness and awkwardness in some of the writing in the beginning stories, and I wanted to attribute this to the new translator, however I am wondering if this was not more of a placebo effect. Because the first time I read The Last Wish I had similar ''issues'' with the translation, but the second time I didn''t have any at all. Nor did I have any issues with, say, the back half of this book. So I think it may just be a mood thing, or a matter of getting used to the writing/translation. In any case, I have never really felt my enjoyment of Sapkowski''s stories lessened by the fact that they are not in the original language.')

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'CT', '06/20/2016', 5, '"Not this war, Geralt. After this war, no-one returns. There will be nothing to return to. Nilfgaard leaves behind it only rubble; its armies advance like lava from which no-one escapes. The roads are strewn, for miles, with gallows and pyres; the sky is cut with columns of smoke as long as the horizon. Since the beginning of the world, in fact, nothing of this sort has happened before. Since the world is our world... You must understand that the Nilfgaardians have descended from their mountains to destroy this world."
The Sword of Destiny is the sequel to the Witcher''s first collection, The Last Wish, picking up where the previous book left off. The continuity is surprisingly fluid with the stories being surprisingly interlinked and best read in the order that they are published. The Sword of Destiny is also absolutely essential to understanding the later novels in the series, which is unusual when dealing with short stories.')

INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values  
( 'Blood of Elves (The Witcher, 3)', '12/07/2021', 5, 'For over a century, humans, dwarves, gnomes, and elves have lived together in relative peace. But times have changed, the uneasy peace is over, and now the races are fighting once again. The only good elf, it seems, is a dead elf. Geralt of Rivia, the cunning hunter known as the Witcher, has been waiting for the birth of a prophesied child. This child has the power to change the world—for good or for evil As the threat of war hangs over the land and the child is hunted for her extraordinary powers, it will become Geralt''s responsibility to protect them all. And the Witcher never accepts defeat.' )
 SELECT @BookId = Scope_Identity()
 
INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Corrie', '07/16/2016', 5, 'This story is excellent and if you have played The Witcher videogames then you will appreciate it all the more (and vice versa). I''m really just writing this review so people don''t fall into the same trap that I did and skip over Swords of Destiny before reading this one. Amazon has this labelled as Book 2 and has Swords of Destiny labelled as Book 4. That is because Swords of Destiny was published later (the English version at least), but in terms of the story this is the order I would recommend reading them in: Last Wish, Swords of Destiny, Blood of Elves, Time of Contempt, Tower of Swallows, and Lady of the Lake (coming out in 2017). The Last Wish and Swords of Destiny are collections of short stories, but the rest all follow a specific story arc so it is really better to at least read the rest in the order that I mentioned. Hope that helps!')

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'tqwert1', '02/25/2019', 5, 'Delightful, intense, irreverent, and compelling....you have to read The Witcher books because they are rife with all of the elements that make you love fiction, and especially fantasy, in the first place....In a word, The Witcher delivers.')




INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'George', 'Orwell', 'https://www.amazon.com/George-Orwell/e/B000AQ0KKY/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 

INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values  
( '1984', '5/12/1948', 5, 'Written more than 70 years ago, 1984 was George Orwell’s chilling prophecy about the future. And while 1984 has come and gone, his dystopian vision of a government that will do anything to control the narrative is timelier than ever...' )
 SELECT @BookId = Scope_Identity()
 
INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Truth Seeker', '06/17/2018', 5, 'This is one of the first books I have read more than once. I first read "1984" in 1985 and now for the second time in 2018. The book has remained the same, but both the world and I have not. I cannot begin to convey how genuinely frightening this book is. I am a lover of popular science fiction and am astounded by Orwell''s ability to be more compelling, entertaining and engrossing than authors with the benefit of light sabers, phasers and teleportation.
To every young person who has been assigned this book, know that you are reading a literary work of art. Many of you will understand and appreciate it, but if you love literature, please make a mental note to read this again when you are older. Youth brings with it eternal hope, boundless optimism and of course, hormones, so you will find yourself rebelling against the pessimism of the book itself - you will effectively be Winston raging against the machine, hoping, searching, questing for a way out. In short, you will cheat.
But when you get older, have a family, lose loved ones and see some of your dreams unfulfilled - when you witness entire nations and races of peoples born, live and die in brutal squalor - when you reflect on the technological advances made over the decades and gaze, with mouth agape, at how a people can be less advanced, less informed and less enlightened, not despite these innovations, but BECAUSE of them, then you will read 1984 as it was meant to be read...not as a dark, dystopian world you enter when you open the book, but a beautifully brutal warning that, even as you read it, is prophetically coming true around you.')

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Liz Benn', '07/04/2020', 5, 'Over 70 years ago Orwell predicted exactly what is happening in the USA today. His brilliant instincts for our future were uncanny. Our country is under assault right now (& has been) by “Big Brother” - ie. communism. Every thought is controlled from all media to removal of our history & heritage to absolute destruction of our laws & erasing of our real history. This was required reading when I was in HS in 1968 & it should be again today. Do yourself a favor & read this before Amazon takes it off their list of books. I doubt they’ll publish this review. Let’s see.')


INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values  
( 'Animal Farm', '4/18/1949', 5, 'George Orwell''s timeless and timely allegorical novel—a scathing satire on a downtrodden society’s blind march towards totalitarianism.' )
 SELECT @BookId = Scope_Identity()
 
INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Sara "Cristia" H. J.', '03/28/2018', 5, 'In the 21st century, when we believe that everything is evolving around us and that all countries are moving forward, we realize that there is still a parasite which it is difficult to get rid of.
Countries that had the opportunity to evolve, have had to pass a difficult test of not falling into totalitarianism and ambition. Such was the case of some countries of the Soviet Union that achieved liberation, but still others continue to fall into the same abyss from which they can''t rise, or don''t want to, since that parasite has crawled in the mind of their crowd, as did happen in North Korea, China, etc.
Animal Farm shows the perfect example of how the unhealthy idea of ​​a cheap Socialism began to take root to become a dictatorial Communism, as it happens in Venezuela today. Its strange end leaves a bitter taste that perhaps the writer did on purpose to open the consciousness of future generations. An open ending that forces the reader to ask himself: what is the solution? And how will it end?
Through human experiences of the animals of this farm, we can identify this truth that still lingers in some shady societies of the present. The solution is in our hands. It will depend on the degree of preparation, culture, moral values, determination, and courage people have to free their homeland and achieve a better future. Remember governments must fear the people and not the opposite.
After that, I summarize my point of view about the strongest references dealt with through the characters in this book (that can be easily identify and distinguished when you start to read the story) in the following sections:
1) Leaders full of charisma who manage to enter the hearts of the crowd by their power of conviction. They choose the most insecure sectors and people to whom they inject large doses of false trust and dependence, and then use them in the propagation of their miserable revolution.
2) From the beginning, they call a supposed self-identification and self-recognition through rhythmic and flattering slogans. They remember again and again their few and poor achievements that remain in the distant past. Then, they impose a barrier of differences between them and the supposed enemy. In this way, the people is infused with a nationalism that is based on ignorance, fear, and blind reverence, forcing them to repeat proverbs and apply reforms without understanding the true meaning or purpose, thus beginning to resemble a herd of sheep, marching pleased towards the slaughterhouse.
3) They make the crowd believe that they have the final decision and, for the common good, unconsciously follow the rules and imposed parameters. In addition, some extra benefits are allowed to those who follow and protect the regime indulgently. This is how they teach the majority that it is better to be corrupt, dishonest, and negligent, in order to achieve higher ranks.
4) The regime feel entitled to legalize and abolish what suits it, ordering the people what to eat, how to dress, greet and live, and what to learn, while they live freely at the expense of the efforts of others and of the injustices committed, trampling the honor of an entire country and their own Machiavellian socialist laws.
5) What seemed a worthy plan for community, social, intellectual, and economic development, now shows the true intention that tries to kill the spirit of solidarity to impose the dictatorial and even genocidal plan, if the regressive revolution warrants it.
6) Everyone, even the majority of the crowd, realize that revolutionary projects are a total failure when they find themselves amidst of aberrant poverty.
7) When they want to discredit an opponent or other progressive ideas, they use their famous method of defamation with lies, intimidation, and any other means. For them, the aim (maintain / save the revolution) justifies the means (spreading false rumors, prosecutions, torture, hunger, espionage), importing in the least the opinion of others, since their own people live in ignorance, cowardice and/or conformism.
8) To finally protect their interests and ideals, communists surround themselves with and associate with allies of their own class: corrupt, traffickers, murderers and terrorists, and expand their power further through the destruction of every vital block of a society , from its financial structure to public sectors, such as health, without caring about the misery that people live. To rule the ignorant and negligent is much easier.
9) There comes a time when the revolutionary-communist doctrine is so deeply rooted in the consciences, that the people forget how well they lived before. The most outrageous thing is that there are still people who support such regimes and whose can mental programming is so easily influenced on behalf the sadistic needs and convenience of these cunning and malevolent rulers.
Times before the Rebellion are being left in the past, where the memories struggle to keep them safe to share them with others')

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'ES', '08/27/2018', 1, 'This edition is really messed up. It ends on page 70 and starts back up on a page 103. Coincidentally because that page doesn’t end the sentence and page 103 doesn’t start a sentence, my middle school age son, continue to read it without noticing. When he went to take the comprehension test he had to stop the test in the middle because it was asking questions about things he never read. When he went through the book carefully we realized it’s completely missed printed and is missing about 30 pages!')



INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Ray', 'Bradbury', 'https://www.amazon.com/Ray-Bradbury/e/B000AQ1HW4/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Fahrenheit 451', '01/16/1984', 6, 'Guy Montag is a fireman. His job is to destroy the most illegal of commodities, the printed book, along with the houses in which they are hidden. Montag never questions the destruction and ruin his actions produce, returning each day to his bland life and wife, Mildred, who spends all day with her television “family.” But when he meets an eccentric young neighbor, Clarisse, who introduces him to a past where people didn’t live in fear and to a present where one sees the world through the ideas in books instead of the mindless chatter of television, Montag begins to question everything he has ever known.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'The Expert', '05/16/2019', 5, 'This is a must read book!! But I will say that I have a totally different point of view to the story than what most, in fact, all the reviews and editorials I have seen. I am not a bookworm and so the idea that books are gone is not an apocalyptic idea. The book was written before the internet and the information age. It is WHY the books are burned and WHAT the books represent that should open your eyes and minds while reading this book.
If all you get out of this book is the "removal" of books from society to become more connected to our electronic devices I feel so bad for you.
The point of burning the books is explained. I might give just a couple of spoilers, but everyone knows the premise of 1984 and this book is similar. It is so much more than about books.
It is about censorship and the people wanting it. The government has banned all printed material except for comic books, 3D pornographic magazines, "good old confessions" and trade journals. All other printed material is deemed too offensive to someone. So much in-fighting in society because everyone claiming something offends them. So to make everyone happy, the offensive materials are removed. Because of the year this was written (1953) Ray Bradbury could have not envisioned the internet. If he had, it would have been heavily censored also. In 1953 ideas and knowledge were shared through print as they had been for hundreds of years.
According to the book, the people wanted the offensive materials removed. Because everyone is offended by something then everything is offensive, it must all be destroyed.
For me the novel rings true about how easily people are offended by another person''s ideas, thoughts, actions, beliefs. In the story those things are still allowed (they can''t control what you think), but without being able to write them down ideas and thoughts die pretty fast.
Ultimately the story is about freedom and not being so judgmental of others lest ye be judged. If you look around today, 11/4/2017, this story has never been more relevant. We have protests and attacks in the streets daily based on ideals and beliefs that clash with others. These clashes occur, rather than people going their separate ways and understanding that the beliefs and ideals of others are just as legitimate as their own. Some groups would rather have a scorched earth policy and destroy everything they hold dear, as long as the other side loses everything as well.')



INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Upton', 'Sinclair', 'https://www.amazon.com/Upton-Sinclair/e/B000AQ0450/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'The Jungle', '08/09/1943', 6, 'In this powerful book we enter the world of Jurgis Rudkus, a young Lithuanian immigrant who arrives  in America fired with dreams of wealth, freedom,  and opportunity. And we discover, with him, the  astonishing truth about "packingtown," the  busy, flourishing, filthy Chicago stockyards, where  new world visions perish in a jungle of human  suffering. Upton Sinclair, master of the  "muckraking" novel, here explores the workingman''s  lot at the turn of the century: the backbreaking  labor, the injustices of "wage-slavery,"  the bewildering chaos of urban life. The  Jungle, a story so shocking that it  launched a government investigation, recreates this  startling chapter if our history in unflinching  detail. Always a vigorous champion on political reform,  Sinclair is also a gripping storyteller, and his  1906 novel stands as one of the most important --  and moving -- works in the literature of social  change.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Anne Davis', '12/09/2019', 5, 'When asked that question, Donald Trump said that one of those times was the onset of the 20th century with military & industrial expansion. This was when Sinclair wrote this book portraying the harsh conditions & exploited lives of immigrants here. What is great for the wealthy is not necessarily great for the rest of us. This is a powerful story of man''s inhumanity to man. No country is great until we deal successfully with that.')



INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Mary', 'Shelley', null )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Frankenstein the Original 1818 Text', '08/09/1818', 2, 'The idea of a reanimated corpse was famously conceived by an 18 year old Mary Shelley on holiday with her future husband Percy Bysshe Shelley and Lord Byron near Lake Geneva, Switzerland. The three were tasked with writing a ghost story, which resulted in one of the most famous novels to come from the 19th century. Published anonymously in a three volume series, Frankenstein instantly set the standard for a true literary horror and its themes led many to believe it was the first true science fiction novel. In 1831 and after much pressure, Mary Shelley revised the text to be more fitting to contemporary standards. Presented here by Reader''s Library Classics is the original 1818 text of Frankenstein.
Young scientist Victor Frankenstein, grief-stricken over the death of his mother, sets out in a series of laboratory experiments testing the ability to create life from non-living matter. Soon, his experiments progress further until he creates a humanoid creature eight feet tall. But as Frankenstein soon discovers, a successful experiment does not always equal a positive outcome.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'ESS', '11/19/2021', 1, 'I was confused when halfway through the novel the story stopped and then started again in a completely non-continuous way. Turns out this is because a whole section was missing. I then realized that the last several chapters were also missing. What kind of terrible money-seeking miscreants would perpetrate this kind of fraud? I just worry that so many readers may think that this is actually the book as Mary Shelley wrote it. And to be crystal clear, this is not a question of the earlier version of the novel versus the later one. This is at best incredible negligence.')



INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Brandon', 'Morris', 'https://www.amazon.com/Brandon-Q-Morris/e/B01N7UFRX4/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'The Disturbance', '08/09/2015', 10, 'No one has ever ventured deeper into space than the four astronauts of Shepherd-1. The aim of their mission: to witness the creation of the cosmos. Using the sun as a lens, they are to align a flock of probes in such a way that the moment of the Big Bang becomes visible.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Clare Anderson (CHG Librarian)', '05/14/2022', 5, 'I love the way this author explains the science behind his stories. While this one may seem more improbable than some of his other stories, I contribute that to the complexity of quantum physics. Maybe it is not so much complexity as it is to accept some of the properties of quantum physics given it seems to go against common since.
Regardless of your knowledge of quantum physics, the characters are well developed and quickly I became invested in their well being. The story took turns I did not expect. Other items that I question as I observed interactions between the crew and earth were all answered and made sense. If you have liked other hard science stories, you should enjoy this one too.')


declare @AuthorId2 int

INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'David', 'Beers', 'https://www.amazon.com/David-Beers/e/B009PQKKT6/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Michael', 'Anderle', 'https://www.amazon.com/Michael-Anderle/e/B017J2WANQ/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId2 = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Warlord Born (The Great Insurrection Book 1)', '04/22/2020', 10, 'Known as Odin to the masses, Alistair is the last line of defense in the unending war against the Subversives who want to overthrow the Commonwealth.
The legend he’s built over two decades of unquestioning service has earned him the trust of the Imperial Ascendant and the privilege of living a good life with the wife he loves.
Alistair’s mission is simple: catch and kill two Subversives.
One act of mercy will change his life forever.
Alexander’s family have ruled the Commonwealth for ten generations by crushing dissent from the AllMother’s Subversives.
His 30 years as Imperial Ascendant have been peaceful, but now the most celebrated Titan who ever lived has betrayed him. Alexander is thrown into a race against time and fate as the odds stack up against him.
Separated from his wife, Alistair takes a stand against the might of the Commonwealth. He does not stand alone. The mysterious AllMother has been waiting for her champion to lead humanity to freedom from the tyranny of the Ascendant.
Odin will die, and Prometheus will rise from the ashes.
Can Alistair survive long enough to save his love from Alexander’s clutches? Or will Earth’s greatest warrior fall to its greatest ruler?' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)
INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId2, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Rob Ballon', '04/10/2021', 5, 'This has to be one of the best Modern SCI-Fi Military books I’ve read in a long time....I don’t like to do spoilers but in a nutshell we have a future where the solar system is under oppression of a very similar Empire to Ancient Rome with so many historical references woven throughout the tale...from Achilles Mermadons to the Praetorian Guards of old where the entire solar system is under the boot...there’s genetic engineering, interdimensional travel, great battles, both small and planet wide, great references to the Norse gods and culture, then the theme of a mythical hero who must battle his own demons to rise...you really don’t want to miss this one!!')




INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'D.M.', 'Pruden', 'https://www.amazon.com/D-M-Pruden/e/B00VKVA2U6/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Kaine''s Sanction (Shattered Empire Book 1)', '01/16/2019', 10, 'When the UEF Scimitar is dispatched to retrieve an archaeologist from an isolated and abandoned Terran colony, young officer Hayden Kaine thinks it is a routine mission. But when a mysterious accident traps the ship in the quarantined star system, survival soon becomes everyone''s only concern.
Attacked by an unknown alien species, the ship is damaged and half of the crew is killed or injured, including the captain. The only surviving bridge officer, Kaine is thrust into the command seat; a role for which he is neither experienced nor prepared, and which most of the surviving crew do not easily accept.
While the enemy amasses an overwhelming invasion fleet capable of overwhelming the unsuspecting confederation, Kaine must stave off a mutiny long enough to find a way to warn Earth of the danger. If he fails, the empire which has survived for five hundred years will fall and humanity will be doomed to extinction by a foe whose existence seems to defy the laws of physics.
If you love the science fiction works of Heinlein, Clarke or Asimov, try D.M. Pruden''s rousing military space opera adventure, Kaine''s Sanction, the first book in the Shattered Empire series.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'David E.', '02/17/2019', 5, 'This unputdownable novel features an Officer cadet, Hayden Kaine, who is close to graduating. Kaine already has a family plan to enter the Diplomatic Core and beyond. However The Admiral graduates him early and assigns him as 2nd lieutenant to an old delapidated ship on a secret mission. The mission involves retrieving a scientist from an abandoned outpost and the events that transpire lead Kaine to mature as an Officer and find some meaning and a little romance. "First contact" is made with highly advanced races resulting in potential (and real) conflict....
The open-ended ending leaves the reader wanting to know what happens next and i for one can''t wait for the next book.
If you are a fan of Mel Destin and the Mars Ascendant series from Doug Pruden, then you will love this story too - which I essentially read in one sitting. I''d throughly recommend this book.')


 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Armstrong Station (Requiem''s Run Book 1)', '01/16/2005', 10, 'In space, helping a stranger can get you killed, or worse…
Armstrong Station is the busiest spaceport in the system where you can buy almost anything. Even a runaway slave.
Something unexpected happens on a routine stop at Luna’s Armstrong Station which threatens to upend Melanie Destin’s life and put her and the crew of the Requiem in mortal peril.
When she chooses to help a stowaway, Mel discovers that the young woman has a secret; one that will endanger anyone who encounters her.
On the run from a corrupt police inspector, and unable to trust any of her underworld contacts, Mel must navigate the dangerous criminal underbelly of Lunar society in search of a way to get them both safely off world. 
Roaming across the Solar System, a reluctant and unlikely heroine sets herself against overwhelming odds, and she’s not going to take crap from anyone who stands in her way.
Can Mel get herself out of this mess without someone dying?' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Jeff', '02/14/2021', 3, 'The story is done well. But as in said this barely is a science fiction story. Take out the space ship and put in a tramp steamer, add the Barbie coast instead of the moon. This could be any most any kidnapping story from the turn of the century.')

 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Phobos Station (Requiem''s Run Book 2)', '03/27/2008', 10, 'Mars is a nice place…when it’s not trying to kill you.
Most spacefarers avoid Phobos Station. It is hard to get to and the locals would rather shoot you than be helpful.
Melanie Destin has agreed to a simple task: Identify the person who receives a container the Requiem delivers to Phobos Station.
When she learns the identity of the intended recipient, things becomes far more deadly.  As the body count rises, Mel must find a way to protect her shipmates from an old nemesis with vengeance on his mind. 
Mel Destin has a knack for getting out of the trouble that finds her, but has her luck finally run out on Phobos Station?' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'docbob', '04/17/2020', 5, 'Phobos Station was provided to me by the author. What follows is an original opinion.
This book picks up where Armstrong Station leaves off. Having read that book will help, but everything you need to know is provided in the story.
In our future, man has gone to the stars, but he has not changed. While many are team players, there are many who would not think twice about injecting you with nanites that need to be fed to keep them from killing you, all so that you can be controlled. The good people seem to be better, and the evil people worse.
Melanie Destin comes into the situation trying to save a young woman who has the nanites in her, ones she was injected with so that she would be forced into prostitution. She will come face to face with the person behind this, one of the most evil men she has ever met. She will also learn what it means to be a team player, and what family means.')

 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Rhea''s Vault (Requiem''s Run Book 3)', '03/27/2010', 10, 'Saturn’s moon, Rhea, hides many secrets…
The Vault is the most secure archive in the Solar System, or so everyone believes.  Melanie Destin desperately needs to get inside and knows someone who can help her. 
But when she gains access to the facility, what she discovers should not be possible.
Something has destroyed the archive, and now it hunts her.
With her ship sabotaged, Mel must survive long enough to escape Rhea with the secret she’s uncovered.
If she fails, a force will be unleashed that will kill millions, and forever change the balance of power in the Solar System.
Mel Destin will need to use every trick at her disposal for any hope to get out of this one, but will it be enough?' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'docbob', '6/1/2020', 5, 'Rhea''s Vault is the 3rd in a series that tells the story of Doctor Melanie Destin. We know of the doctor from the Mars Ascendant series where she gains the nickname Mother of Mars. That is probably my biggest nit to pick in reading this series. I know Mel has to live to take on another day (unless Mr. Pruden has a clone of her in the wings), so I know she has to survive the death traps she is in, it is just how she is going to do it.
Dr. Mel is the ships doctor on the Requiem space freighter. Should be a quiet and simple job. In other books in the series she takes on a run away and tries to help her of some nanites that have taken over her system who later gets a more lethal concoction. Now she is working for an agent of the Mars secret police who says he knows where the cure is, it is Rhea''s Vault, a vault held by the Jovian collective, a group of 5 families who rule a great deal of space.
Her Mars connection insist the cure is there, but the vault is so secure that a small piece of space dust will be burned up before it gets close. What chance will Mel and here associates have to get in.
That is where the excitement comes in.
A word of warning. Mel has a potty mouth. I understand the need to have a character act like that however I do feel the author uses the F-bomb as a shock value. For me, it is not shocking anymore and gets in the way of a good story that it has to be used. Also there is an incident of sexual encounter off screen that gets referenced throughout the rest of the book. For that reason if you are a parent of a YA reader you might want to check the book out before you let your child read it.
Good story, good action, kept my interest, I recommend it.
I did receive an ARC of this book but this is an honest review.')

 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Ganymede Station (Requiem''s Run Book 4)', '03/27/2011', 10, 'Millions will die, and it''s all her fault...
Mel Destin is desperate. Try as she might, a cure for the deadly nano-virus infecting Chloe Cabot remains elusive. With time running out and assassins hunting her across the solar system, she is forced into an unholy alliance with the only person who can help — the man who murdered her childhood friend.
What she seeks is hidden on Ganymede, but while searching for it, Mel makes a terrible discovery: one that could drive a wedge between her and her friends aboard Requiem. Worse still, by accessing the cure, she will unleash a more terrible fate on millions.
How will Mel get herself out of this mess?' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Marc Yergin', '11/19/2020', 5, 'Doctor Mel Destin has been chasing a cure for Chloe Cabot who has been infected with nanites that are slowly killing her. Their effect has been slowed down by placing her in cryo chamber while Mel works to find a cure but Chloe can’t stay there forever. Mel and the crew of the Requiem under the command of Roy Chambers have to find a cure or her father, Anthony Cabot, will take drastic action. They have chased the cure from Earth’s satellite to Mar’s Phobos and finally to the Galilean satellite, Ganymede. Along the way she and the crew of the Requiem have had to fight off spies and agents of Anthony Cabot.
This is a continuation of the (mis)adventures of Melanie. Sometimes it seems like she and those around her just can’t get a break,
If you like fast-paced space adventures people with believable characters who manage to overcome obstacles thrown in their ways then this is one read you shouldn’t miss. And as with most well-written series you don’t have to have read the preceding books but it does help to explain characters’ motivation. Either way, this is a really good read.')



INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'M.R.', 'Forbes', 'https://www.amazon.com/M-R-Forbes/e/B00BBX2184/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Starship For Sale', '03/28/2015', 10, 'When Ben Murdock receives a text message offering a fully operational starship for sale, he’s certain it has to be a joke.
Already trapped in the worst day of his life and desperate for a way out, he decides to play along. Except there is no joke. The starship is real. And Ben’s life is going to change in ways he never dreamed possible.
All he has to do is sign the contract.
Joined by his streetwise best friend and a bizarre tenant with an unseverable lease, he’ll soon discover that the universe is more volatile, treacherous, and awesome than he ever imagined.
And the only thing harder than owning a starship is staying alive.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Franklin Hadick', '04/03/2022', 1, 'Ok maybe I should have read more but I couldn''t get past 50%, and I at least try to finish but this was like watching 2 three year olds arguing. Honestly the story just wasn''t that interesting as it seems just another formula adventure that just flat dragged. If you can stomach finishing it, drop me a line and tell me it got even a touch interesting.')


INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'Erich', 'Krauss', 'https://www.amazon.com/Erich-Krauss/e/B001H6SYJU/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'Primitives', '02/22/2007', 10, 'Thirty years after The Great Fatigue infected the globe—and the treatment regressed most of the human race to a primitive state—Seth Keller makes a gruesome discovery in his adoptive father’s makeshift lab. This revelation forces him to leave the safety of his desert home and the only other person left in the world…at least, as far as he knows.
Three thousand miles away in the jungles of Costa Rica, Sarah Peoples has made her own discovery—just as horrific, and just as life-changing. It will take her far from the fledgling colony of New Haven, yet never out of reach of its ruthless authoritarian leader.
On separate journeys a world apart, Seth and Sarah find themselves swept up in a deadly race to save humankind. Their fates will come crashing together in an epic struggle between good and evil, where the differences aren’t always clear. Among the grim realities of civilization’s demise, they discover that the remaining survivors may pose an even greater threat than the abominations they were taught to fear.
Fighting for their lives, they’re confronted with a haunting question.
Does humanity deserve to survive?' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Grady Harp', '05/10/2022', 5, 'Nevada author Erich Krauss is a professional Muay Thai fighter and global traveler. He is the founder and publisher of Victory Belt Publishing and has published over thirty books, including such titles as MUAY THAI UNLEASHED, WAVE OF DESTRUCTION, WALL OF FLAME, BRAWL, UFC (Ultimate Fighting Championship), ON THE LINE, THE ULTIMATE MIXED MARTIAL ARTIST and now PRIMITIVES, a post apocalyptic novel described as a tale of bravery and self-discovery in the ruins of a dying world, where the darkest sides of human nature are revealed.
Of particular interest, Krauss admixes his personal experience as a survivalist and his struggle with Lyme disease with his gift for storytelling, and the result is a pulsating, authentically detailed tale of bravery and self-discovery found in the ruins of a dying world, where the darkest sides of human nature are revealed.
The synopsis suggests the impact of this book: ‘The story of two unlikely heroes thrust into a post-apocalyptic mission to restore humanity. Thirty years after The Great Fatigue infected the globe—and the treatment regressed most of the human race to a primitive state—Seth Keller makes a gruesome discovery in his adoptive father’s makeshift lab. This revelation forces him to leave the safety of his desert home and the only other person left in the world…at least, as far as he knows. Three thousand miles away in the jungles of Costa Rica, Sarah Peoples has made her own discovery—just as horrific, and just as life-changing. It will take her far from the fledgling colony of New Haven, yet never out of reach of its ruthless authoritarian leader. On separate journeys a world apart, Seth and Sarah find themselves swept up in a deadly race to save humankind. Their fates will come crashing together in an epic struggle between good and evil, where the differences aren’t always clear. Among the grim realities of civilization’s demise, they discover that the remaining survivors may pose an even greater threat than the abominations they were taught to fear. Fighting for their lives, they’re confronted with a haunting question. Does humanity deserve to survive?’
A brief excerpt shares the author’s writing skill – ‘ Sixty feet above me at the top of the butte, a hand dangles off the side of a steel platform. I grip the metal handle and start cranking. With each rotation, thick cables move, and the platform inches downward…As usual, the figure strapped to the platform is double wrapped in a translucent white sheet of industrial plastic – the Professor bought rolls and rolls of the stuff before the whole world went to hell…’
Krauss has composed an apocalyptic novel that consumes interest on every page while introducing fine philosophical issues. This is an important novel, highly recommended. Grady Harp, May 22')


INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'James', 'Brayken', 'https://www.amazon.com/James-Brayken/e/B09VY2WHPK/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'The Veiled Edge of Contact', '07/16/2004', 10, 'A lost tribe. A missing wife. An alien arrival.
Welcome to the jungle . . .
Okon was comfortable. Then his wife did something inconsiderate: she disappeared.
When Okon realizes she left behind a desperate message with instructions on how to find her, he enters the largest jungle in Africa to follow her trail.
But the longer Okon searches for his wife, and the deeper he goes into the jungle, the more tangled he becomes in an astonishing discovery and the lives of an unlikely group of strangers.
Okon’s comfortable home has never seemed so far away and danger never so close . . . or so alien.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Ryuku', '05/10/2022', 1, 'This is one amazing and truly unexpected story. It kept me glued to it, it made me laugh (out loud!). I had lots of fun, if I''m being honest. The main character is useless, not only is he aware of it, but people around him keep reminding him of the fact. Of course he grows, but his growth is a really painful one. One I enjoyed to no end.
I can''t say enough good things about this adventure. Maybe the only thing you need to know is that it was worth every second I spent fully immersed in it.')


INSERT INTO [dbo].[Authors] ( FirstName, LastName, WebSite ) values 
 ( 'A.G.', 'Riddle', 'https://www.amazon.com/A-G-Riddle/e/B00C32LQBK/ref=aufs_dp_fta_dsk' )
SELECT @AuthorId = Scope_Identity()
 
INSERT INTO [dbo].[Books] ( Title, PublishDate, CategoryId, Synopsis ) values 
 ( 'The Lost Colony', '02/24/2008', 10, 'On Eos, the last survivors of the Long Winter face their greatest challenge yet—and race to unravel the deepest secrets of the grid. It’s a journey across space and time and into humanity’s past and future—with a twist you’ll never forget.' )
 SELECT @BookId = Scope_Identity()

INSERT INTO [dbo].[AuthorBooks] ( AuthorId, BookId ) values ( @AuthorId, @BookId)

INSERT INTO [dbo].[Reviews] ( BookId, ReviewAuthor, ReviewDate, Stars, Review) values 
( @BookId, 'Kindle Customer', '11/14/2019', 2, 'You could tell the author was grasping at straws at how to end the series. Early chapters reminded me of the clickbait stories you see on Facebook, (if you liked that wait til you see the NEXT chapter! Etc). Later chapters became very long and convoluted, including of all things hand drawn blueprints for houses.....yeah you read that right. The addition to the plot by that little tidbit was out of character and very duct taped onto the plot. It was almost laugh out loud bad. I know the book released 10 days later than expected so I''m guessing the last few chapters were very very hurried. I haven''t been this disappointed by a series ending since Mass Effect.....and that one ended in lawsuits')

