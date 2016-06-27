using System;
using System.Collections.Generic;
using ChampionMains.Pyrobot.Data.Models;
using System.Data.Entity.Migrations;
using System.Linq;

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

            var champions = new [] {
                new Champion() {Id = 266, Name = "Aatrox", Identifier = "Aatrox"},
                new Champion() {Id = 103, Name = "Ahri", Identifier = "Ahri"},
                new Champion() {Id = 84, Name = "Akali", Identifier = "Akali"},
                new Champion() {Id = 12, Name = "Alistar", Identifier = "Alistar"},
                new Champion() {Id = 32, Name = "Amumu", Identifier = "Amumu"},
                new Champion() {Id = 34, Name = "Anivia", Identifier = "Anivia"},
                new Champion() {Id = 1, Name = "Annie", Identifier = "Annie"},
                new Champion() {Id = 22, Name = "Ashe", Identifier = "Ashe"},
                new Champion() {Id = 136, Name = "Aurelion Sol", Identifier = "AurelionSol"},
                new Champion() {Id = 268, Name = "Azir", Identifier = "Azir"},
                new Champion() {Id = 432, Name = "Bard", Identifier = "Bard"},
                new Champion() {Id = 53, Name = "Blitzcrank", Identifier = "Blitzcrank"},
                new Champion() {Id = 63, Name = "Brand", Identifier = "Brand"},
                new Champion() {Id = 201, Name = "Braum", Identifier = "Braum"},
                new Champion() {Id = 51, Name = "Caitlyn", Identifier = "Caitlyn"},
                new Champion() {Id = 69, Name = "Cassiopeia", Identifier = "Cassiopeia"},
                new Champion() {Id = 31, Name = "Cho'Gath", Identifier = "Chogath"},
                new Champion() {Id = 42, Name = "Corki", Identifier = "Corki"},
                new Champion() {Id = 122, Name = "Darius", Identifier = "Darius"},
                new Champion() {Id = 131, Name = "Diana", Identifier = "Diana"},
                new Champion() {Id = 119, Name = "Draven", Identifier = "Draven"},
                new Champion() {Id = 36, Name = "Dr. Mundo", Identifier = "DrMundo"},
                new Champion() {Id = 245, Name = "Ekko", Identifier = "Ekko"},
                new Champion() {Id = 60, Name = "Elise", Identifier = "Elise"},
                new Champion() {Id = 28, Name = "Evelynn", Identifier = "Evelynn"},
                new Champion() {Id = 81, Name = "Ezreal", Identifier = "Ezreal"},
                new Champion() {Id = 9, Name = "Fiddlesticks", Identifier = "FiddleSticks"},
                new Champion() {Id = 114, Name = "Fiora", Identifier = "Fiora"},
                new Champion() {Id = 105, Name = "Fizz", Identifier = "Fizz"},
                new Champion() {Id = 3, Name = "Galio", Identifier = "Galio"},
                new Champion() {Id = 41, Name = "Gangplank", Identifier = "Gangplank"},
                new Champion() {Id = 86, Name = "Garen", Identifier = "Garen"},
                new Champion() {Id = 150, Name = "Gnar", Identifier = "Gnar"},
                new Champion() {Id = 79, Name = "Gragas", Identifier = "Gragas"},
                new Champion() {Id = 104, Name = "Graves", Identifier = "Graves"},
                new Champion() {Id = 120, Name = "Hecarim", Identifier = "Hecarim"},
                new Champion() {Id = 74, Name = "Heimerdinger", Identifier = "Heimerdinger"},
                new Champion() {Id = 420, Name = "Illaoi", Identifier = "Illaoi"},
                new Champion() {Id = 39, Name = "Irelia", Identifier = "Irelia"},
                new Champion() {Id = 40, Name = "Janna", Identifier = "Janna"},
                new Champion() {Id = 59, Name = "Jarvan IV", Identifier = "JarvanIV"},
                new Champion() {Id = 24, Name = "Jax", Identifier = "Jax"},
                new Champion() {Id = 126, Name = "Jayce", Identifier = "Jayce"},
                new Champion() {Id = 202, Name = "Jhin", Identifier = "Jhin"},
                new Champion() {Id = 222, Name = "Jinx", Identifier = "Jinx"},
                new Champion() {Id = 429, Name = "Kalista", Identifier = "Kalista"},
                new Champion() {Id = 43, Name = "Karma", Identifier = "Karma"},
                new Champion() {Id = 30, Name = "Karthus", Identifier = "Karthus"},
                new Champion() {Id = 38, Name = "Kassadin", Identifier = "Kassadin"},
                new Champion() {Id = 55, Name = "Katarina", Identifier = "Katarina"},
                new Champion() {Id = 10, Name = "Kayle", Identifier = "Kayle"},
                new Champion() {Id = 85, Name = "Kennen", Identifier = "Kennen"},
                new Champion() {Id = 121, Name = "Kha'Zix", Identifier = "Khazix"},
                new Champion() {Id = 203, Name = "Kindred", Identifier = "Kindred"},
                new Champion() {Id = 96, Name = "Kog'Maw", Identifier = "KogMaw"},
                new Champion() {Id = 7, Name = "LeBlanc", Identifier = "Leblanc"},
                new Champion() {Id = 64, Name = "Lee Sin", Identifier = "LeeSin"},
                new Champion() {Id = 89, Name = "Leona", Identifier = "Leona"},
                new Champion() {Id = 127, Name = "Lissandra", Identifier = "Lissandra"},
                new Champion() {Id = 236, Name = "Lucian", Identifier = "Lucian"},
                new Champion() {Id = 117, Name = "Lulu", Identifier = "Lulu"},
                new Champion() {Id = 99, Name = "Lux", Identifier = "Lux"},
                new Champion() {Id = 54, Name = "Malphite", Identifier = "Malphite"},
                new Champion() {Id = 90, Name = "Malzahar", Identifier = "Malzahar"},
                new Champion() {Id = 57, Name = "Maokai", Identifier = "Maokai"},
                new Champion() {Id = 11, Name = "Master Yi", Identifier = "MasterYi"},
                new Champion() {Id = 21, Name = "Miss Fortune", Identifier = "MissFortune"},
                new Champion() {Id = 62, Name = "Wukong", Identifier = "MonkeyKing"},
                new Champion() {Id = 82, Name = "Mordekaiser", Identifier = "Mordekaiser"},
                new Champion() {Id = 25, Name = "Morgana", Identifier = "Morgana"},
                new Champion() {Id = 267, Name = "Nami", Identifier = "Nami"},
                new Champion() {Id = 75, Name = "Nasus", Identifier = "Nasus"},
                new Champion() {Id = 111, Name = "Nautilus", Identifier = "Nautilus"},
                new Champion() {Id = 76, Name = "Nidalee", Identifier = "Nidalee"},
                new Champion() {Id = 56, Name = "Nocturne", Identifier = "Nocturne"},
                new Champion() {Id = 20, Name = "Nunu", Identifier = "Nunu"},
                new Champion() {Id = 2, Name = "Olaf", Identifier = "Olaf"},
                new Champion() {Id = 61, Name = "Orianna", Identifier = "Orianna"},
                new Champion() {Id = 80, Name = "Pantheon", Identifier = "Pantheon"},
                new Champion() {Id = 78, Name = "Poppy", Identifier = "Poppy"},
                new Champion() {Id = 133, Name = "Quinn", Identifier = "Quinn"},
                new Champion() {Id = 33, Name = "Rammus", Identifier = "Rammus"},
                new Champion() {Id = 421, Name = "Rek'Sai", Identifier = "RekSai"},
                new Champion() {Id = 58, Name = "Renekton", Identifier = "Renekton"},
                new Champion() {Id = 107, Name = "Rengar", Identifier = "Rengar"},
                new Champion() {Id = 92, Name = "Riven", Identifier = "Riven"},
                new Champion() {Id = 68, Name = "Rumble", Identifier = "Rumble"},
                new Champion() {Id = 13, Name = "Ryze", Identifier = "Ryze"},
                new Champion() {Id = 113, Name = "Sejuani", Identifier = "Sejuani"},
                new Champion() {Id = 35, Name = "Shaco", Identifier = "Shaco"},
                new Champion() {Id = 98, Name = "Shen", Identifier = "Shen"},
                new Champion() {Id = 102, Name = "Shyvana", Identifier = "Shyvana"},
                new Champion() {Id = 27, Name = "Singed", Identifier = "Singed"},
                new Champion() {Id = 14, Name = "Sion", Identifier = "Sion"},
                new Champion() {Id = 15, Name = "Sivir", Identifier = "Sivir"},
                new Champion() {Id = 72, Name = "Skarner", Identifier = "Skarner"},
                new Champion() {Id = 37, Name = "Sona", Identifier = "Sona"},
                new Champion() {Id = 16, Name = "Soraka", Identifier = "Soraka"},
                new Champion() {Id = 50, Name = "Swain", Identifier = "Swain"},
                new Champion() {Id = 134, Name = "Syndra", Identifier = "Syndra"},
                new Champion() {Id = 223, Name = "Tahm Kench", Identifier = "TahmKench"},
                new Champion() {Id = 163, Name = "Taliyah", Identifier = "Taliyah"},
                new Champion() {Id = 91, Name = "Talon", Identifier = "Talon"},
                new Champion() {Id = 44, Name = "Taric", Identifier = "Taric"},
                new Champion() {Id = 17, Name = "Teemo", Identifier = "Teemo"},
                new Champion() {Id = 412, Name = "Thresh", Identifier = "Thresh"},
                new Champion() {Id = 18, Name = "Tristana", Identifier = "Tristana"},
                new Champion() {Id = 48, Name = "Trundle", Identifier = "Trundle"},
                new Champion() {Id = 23, Name = "Tryndamere", Identifier = "Tryndamere"},
                new Champion() {Id = 4, Name = "Twisted Fate", Identifier = "TwistedFate"},
                new Champion() {Id = 29, Name = "Twitch", Identifier = "Twitch"},
                new Champion() {Id = 77, Name = "Udyr", Identifier = "Udyr"},
                new Champion() {Id = 6, Name = "Urgot", Identifier = "Urgot"},
                new Champion() {Id = 110, Name = "Varus", Identifier = "Varus"},
                new Champion() {Id = 67, Name = "Vayne", Identifier = "Vayne"},
                new Champion() {Id = 45, Name = "Veigar", Identifier = "Veigar"},
                new Champion() {Id = 161, Name = "Vel'Koz", Identifier = "Velkoz"},
                new Champion() {Id = 254, Name = "Vi", Identifier = "Vi"},
                new Champion() {Id = 112, Name = "Viktor", Identifier = "Viktor"},
                new Champion() {Id = 8, Name = "Vladimir", Identifier = "Vladimir"},
                new Champion() {Id = 106, Name = "Volibear", Identifier = "Volibear"},
                new Champion() {Id = 19, Name = "Warwick", Identifier = "Warwick"},
                new Champion() {Id = 101, Name = "Xerath", Identifier = "Xerath"},
                new Champion() {Id = 5, Name = "Xin Zhao", Identifier = "XinZhao"},
                new Champion() {Id = 157, Name = "Yasuo", Identifier = "Yasuo"},
                new Champion() {Id = 83, Name = "Yorick", Identifier = "Yorick"},
                new Champion() {Id = 154, Name = "Zac", Identifier = "Zac"},
                new Champion() {Id = 238, Name = "Zed", Identifier = "Zed"},
                new Champion() {Id = 115, Name = "Ziggs", Identifier = "Ziggs"},
                new Champion() {Id = 26, Name = "Zilean", Identifier = "Zilean"},
                new Champion() {Id = 143, Name = "Zyra", Identifier = "Zyra"}
            };
            var cd = champions.ToDictionary(c => c.Name, c => c.Id);

            context.Champions.AddOrUpdate(c => c.Id, champions);


            context.SubReddits.AddOrUpdate(x => x.Name,
                new SubReddit()
                {
                    ChampionId = cd["Zyra"],
                    Name = "Umarrii",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                },
                new SubReddit()
                {
                    ChampionId = cd["Zyra"],
                    Name = "ZyraMains",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                }, 
                new SubReddit()
                {
                    ChampionId = cd["Nami"],
                    Name = "NamiMains",
                    AdminOnly = false,
                    RankEnabled = false,
                    ChampionMasteryEnabled = false,
                },
                new SubReddit()
                {
                    ChampionId = cd["Janna"],
                    Name = "JannaMains",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = true,
                },
                new SubReddit()
                {
                    ChampionId = cd["Sona"],
                    Name = "SonaMains",
                    AdminOnly = false,
                    RankEnabled = false,
                    ChampionMasteryEnabled = true,
                },
                new SubReddit()
                {
                    ChampionId = cd["Bard"],
                    Name = "BardMains",
                    AdminOnly = false,
                    RankEnabled = true,
                    ChampionMasteryEnabled = false,
                });
        }
    }
}
