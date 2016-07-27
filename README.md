# /r/ChampionMains Flairs

The official repository for [the site](http://championmains-pyrobot.azurewebsites.net/) used by the [/r/ChampionMains](https://championmains.reddit.com/) subreddit network
to add flairs for champion mastery and summoner rank.

Forked from [RedditRankedFlairs](https://github.com/jessehallam/RedditRankedFlairs/tree/Production)

### [User Documentation](https://championmains.github.io/Pyrobot/)


## Technical documentation

### Configuration

All configuration is performed using the Web.Config and App.config (for the Azure WebJob). For security reasons, the actual Web.Config is not included. *Don't forget to update your Machine Key.*

All settings are in the Web.template.config file. Key values are overwritten from Web.local.config (not included). App.config is also not included for the WebJob, but uses the
same settings.

If you need help configuring this, just submit an issue.

[Configuring OAuth](https://www.reddit.com/prefs/apps/): Add a 'web app' and a 'personal use script'.


### How it works

Reddit users visit the site and are prompted to sign in using their Reddit account. Reddit's OAuth 2.0 protocol allows the site to verify that 
their Reddit account is authentic.

After signing in, the visitor can specify one or more League of Legends accounts that they own. To verify ownership of those accounts, a challenge is made:

* The user must sign in to League of Legends and change an in-game field to a "validation code".

* Using the Riot Games [Developer API](https://developer.riotgames.com/), the field is retrieved and checked for authenticity.

Once these steps have been completed, the user is considered registered. From time to time, the user's rank is retrieved using the Riot API and
a flair is composed and sent using Reddit's OAuth 2.0 API.


### Technical Info

The solution was made using **Microsoft Visual Studio Community 2015** and targets **.NET Framework 4.5.2**. 

The web application project is built on **ASP.NET MVC 5**. Pages are rendered using the **Razor View Engine**. 
The majority of the website is a single HTML file which behaves like an app using **Angular.JS**.

Data Persistence is handled using **Entity Framework 6 (Code First)**. The production site uses the **System.Data.SqlClient** data provider and is backed
by **Microsoft SQL Server**.

The website is hosted on the cloud using **Microsoft Azure App Services**.


### Contributions

The following NuGet packages are included:

* **[Autofac](http://autofac.org/)** - IoC container and dependency injection.

* **[JSON.NET](http://www.newtonsoft.com/json)** - JSON framework for .NET.

* **[OWIN OAuth Providers](https://github.com/RockstarLabs/OwinOAuthProviders)** - for Reddit OAuth

### License

GPLv2