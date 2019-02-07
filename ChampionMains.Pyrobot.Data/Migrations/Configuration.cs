using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using ChampionMains.Pyrobot.Data.Models;
using MingweiSamuel.Camille.Enums;
using EnumChampion = MingweiSamuel.Camille.Enums.Champion;
using Champion = ChampionMains.Pyrobot.Data.Models.Champion;

namespace ChampionMains.Pyrobot.Data.Migrations
{

    public sealed class Configuration : DbMigrationsConfiguration<UnitOfWork>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(UnitOfWork context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            var champions = ((EnumChampion[]) Enum.GetValues(typeof(EnumChampion)))
                .Select(c => new Champion {Id = (short) c, Name = c.Name(), Identifier = c.Identifier()})
                .ToArray();
            context.Champions.AddOrUpdate(c => c.Id, champions);

            var cd = champions.ToDictionary(c => c.Name, c => c.Id);
            AddSubredditIfNew(context.Subreddits,
                new Subreddit
                {
                    ChampionId = cd["Zyra"],
                    Name = "ZyraMains",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = true,
                    BindEnabled = false
                },
                new Subreddit
                {
                    ChampionId = cd["Bard"],
                    Name = "BardMains",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = false,
                    BindEnabled = true
                },
                new Subreddit
                {
                    ChampionId = cd["Leona"],
                    Name = "LeonaMains",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = true,
                    BindEnabled = false
                },
                // ADMIN ONLY BELOW
                new Subreddit
                {
                    ChampionId = cd["Soraka"],
                    Name = "SorakaMains",
                    AdminOnly = true,
                    RankEnabled = false,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = false,
                    BindEnabled = false
                },
                new Subreddit
                {
                    ChampionId = cd["Nami"],
                    Name = "NamiMains",
                    AdminOnly = true,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = true,
                    BindEnabled = false
                },
                new Subreddit
                {
                    ChampionId = cd["Sona"],
                    Name = "SonaMains",
                    AdminOnly = true,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = true,
                    BindEnabled = false
                },
                new Subreddit
                {
                    ChampionId = cd["Zyra"],
                    Name = "Umarrii",
                    AdminOnly = true,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = true,
                    BindEnabled = false
                },
                new Subreddit
                {
                    ChampionId = cd["Zyra"],
                    Name = "LugnutsK",
                    AdminOnly = true,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                    PrestigeEnabled = true,
                    BindEnabled = false
                });
        }

        private void AddSubredditIfNew(IDbSet<Subreddit> dbSet, params Subreddit[] subreddits)
        {
            foreach (var subreddit in subreddits)
            {
                if (dbSet.FirstOrDefault(r => r.Name == subreddit.Name) == null)
                {
                    dbSet.Add(subreddit);
                }
            }
        }
    }
}
