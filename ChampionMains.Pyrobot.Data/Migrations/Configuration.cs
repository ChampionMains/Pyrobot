using ChampionMains.Pyrobot.Data.Models;
using System.Data.Entity.Migrations;

namespace ChampionMains.Pyrobot.Data.Migrations
{

    public sealed class Configuration : DbMigrationsConfiguration<UnitOfWork>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
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
            context.Champions.AddOrUpdate(new Champion() { Id = 0, Name = "Annie" });
            context.Champions.AddOrUpdate(new Champion() { Id = 2, Name = "Olaf" });
            context.Champions.AddOrUpdate(new Champion() { Id = 3, Name = "Galio" });
            context.Champions.AddOrUpdate(new Champion() { Id = 4, Name = "Twisted Fate" });
            context.Champions.AddOrUpdate(new Champion() { Id = 5, Name = "Xin Zhao" });
            context.Champions.AddOrUpdate(new Champion() { Id = 6, Name = "Urgot" });
            context.Champions.AddOrUpdate(new Champion() { Id = 7, Name = "LeBlanc" });
            context.Champions.AddOrUpdate(new Champion() { Id = 8, Name = "Vladimir" });
            context.Champions.AddOrUpdate(new Champion() { Id = 9, Name = "Fiddlesticks" });
            context.Champions.AddOrUpdate(new Champion() { Id = 10, Name = "Kayle" });
            context.Champions.AddOrUpdate(new Champion() { Id = 11, Name = "Master Yi" });
            context.Champions.AddOrUpdate(new Champion() { Id = 12, Name = "Alistar" });
            context.Champions.AddOrUpdate(new Champion() { Id = 13, Name = "Ryze" });
            context.Champions.AddOrUpdate(new Champion() { Id = 14, Name = "Sion" });
            context.Champions.AddOrUpdate(new Champion() { Id = 15, Name = "Sivir" });
            context.Champions.AddOrUpdate(new Champion() { Id = 16, Name = "Soraka" });
            context.Champions.AddOrUpdate(new Champion() { Id = 17, Name = "Teemo" });
            context.Champions.AddOrUpdate(new Champion() { Id = 18, Name = "Tristana" });
            context.Champions.AddOrUpdate(new Champion() { Id = 19, Name = "Warwick" });
            context.Champions.AddOrUpdate(new Champion() { Id = 20, Name = "Nunu" });
            context.Champions.AddOrUpdate(new Champion() { Id = 21, Name = "Miss Fortune" });
            context.Champions.AddOrUpdate(new Champion() { Id = 22, Name = "Ashe" });
            context.Champions.AddOrUpdate(new Champion() { Id = 23, Name = "Tryndamere" });
            context.Champions.AddOrUpdate(new Champion() { Id = 24, Name = "Jax" });
            context.Champions.AddOrUpdate(new Champion() { Id = 25, Name = "Morgana" });
            context.Champions.AddOrUpdate(new Champion() { Id = 26, Name = "Zilean" });
            context.Champions.AddOrUpdate(new Champion() { Id = 27, Name = "Singed" });
            context.Champions.AddOrUpdate(new Champion() { Id = 28, Name = "Evelynn" });
            context.Champions.AddOrUpdate(new Champion() { Id = 29, Name = "Twitch" });
            context.Champions.AddOrUpdate(new Champion() { Id = 30, Name = "Karthus" });
            context.Champions.AddOrUpdate(new Champion() { Id = 31, Name = "Cho''Gath" });
            context.Champions.AddOrUpdate(new Champion() { Id = 32, Name = "Amumu" });
            context.Champions.AddOrUpdate(new Champion() { Id = 33, Name = "Rammus" });
            context.Champions.AddOrUpdate(new Champion() { Id = 34, Name = "Anivia" });
            context.Champions.AddOrUpdate(new Champion() { Id = 35, Name = "Shaco" });
            context.Champions.AddOrUpdate(new Champion() { Id = 36, Name = "Dr. Mundo" });
            context.Champions.AddOrUpdate(new Champion() { Id = 37, Name = "Sona" });
            context.Champions.AddOrUpdate(new Champion() { Id = 38, Name = "Kassadin" });
            context.Champions.AddOrUpdate(new Champion() { Id = 39, Name = "Irelia" });
            context.Champions.AddOrUpdate(new Champion() { Id = 40, Name = "Janna" });
            context.Champions.AddOrUpdate(new Champion() { Id = 41, Name = "Gangplank" });
            context.Champions.AddOrUpdate(new Champion() { Id = 42, Name = "Corki" });
            context.Champions.AddOrUpdate(new Champion() { Id = 43, Name = "Karma" });
            context.Champions.AddOrUpdate(new Champion() { Id = 44, Name = "Taric" });
            context.Champions.AddOrUpdate(new Champion() { Id = 45, Name = "Veigar" });
            context.Champions.AddOrUpdate(new Champion() { Id = 48, Name = "Trundle" });
            context.Champions.AddOrUpdate(new Champion() { Id = 50, Name = "Swain" });
            context.Champions.AddOrUpdate(new Champion() { Id = 51, Name = "Caitlyn" });
            context.Champions.AddOrUpdate(new Champion() { Id = 53, Name = "Blitzcrank" });
            context.Champions.AddOrUpdate(new Champion() { Id = 54, Name = "Malphite" });
            context.Champions.AddOrUpdate(new Champion() { Id = 55, Name = "Katarina" });
            context.Champions.AddOrUpdate(new Champion() { Id = 56, Name = "Nocturne" });
            context.Champions.AddOrUpdate(new Champion() { Id = 57, Name = "Maokai" });
            context.Champions.AddOrUpdate(new Champion() { Id = 58, Name = "Renekton" });
            context.Champions.AddOrUpdate(new Champion() { Id = 59, Name = "Jarvan IV" });
            context.Champions.AddOrUpdate(new Champion() { Id = 60, Name = "Elise" });
            context.Champions.AddOrUpdate(new Champion() { Id = 61, Name = "Orianna" });
            context.Champions.AddOrUpdate(new Champion() { Id = 62, Name = "Wukong" });
            context.Champions.AddOrUpdate(new Champion() { Id = 63, Name = "Brand" });
            context.Champions.AddOrUpdate(new Champion() { Id = 64, Name = "Lee Sin" });
            context.Champions.AddOrUpdate(new Champion() { Id = 67, Name = "Vayne" });
            context.Champions.AddOrUpdate(new Champion() { Id = 68, Name = "Rumble" });
            context.Champions.AddOrUpdate(new Champion() { Id = 69, Name = "Cassiopeia" });
            context.Champions.AddOrUpdate(new Champion() { Id = 72, Name = "Skarner" });
            context.Champions.AddOrUpdate(new Champion() { Id = 74, Name = "Heimerdinger" });
            context.Champions.AddOrUpdate(new Champion() { Id = 75, Name = "Nasus" });
            context.Champions.AddOrUpdate(new Champion() { Id = 76, Name = "Nidalee" });
            context.Champions.AddOrUpdate(new Champion() { Id = 77, Name = "Udyr" });
            context.Champions.AddOrUpdate(new Champion() { Id = 78, Name = "Poppy" });
            context.Champions.AddOrUpdate(new Champion() { Id = 79, Name = "Gragas" });
            context.Champions.AddOrUpdate(new Champion() { Id = 80, Name = "Pantheon" });
            context.Champions.AddOrUpdate(new Champion() { Id = 81, Name = "Ezreal" });
            context.Champions.AddOrUpdate(new Champion() { Id = 82, Name = "Mordekaiser" });
            context.Champions.AddOrUpdate(new Champion() { Id = 83, Name = "Yorick" });
            context.Champions.AddOrUpdate(new Champion() { Id = 84, Name = "Akali" });
            context.Champions.AddOrUpdate(new Champion() { Id = 85, Name = "Kennen" });
            context.Champions.AddOrUpdate(new Champion() { Id = 86, Name = "Garen" });
            context.Champions.AddOrUpdate(new Champion() { Id = 89, Name = "Leona" });
            context.Champions.AddOrUpdate(new Champion() { Id = 90, Name = "Malzahar" });
            context.Champions.AddOrUpdate(new Champion() { Id = 91, Name = "Talon" });
            context.Champions.AddOrUpdate(new Champion() { Id = 92, Name = "Riven" });
            context.Champions.AddOrUpdate(new Champion() { Id = 96, Name = "Kog''Maw" });
            context.Champions.AddOrUpdate(new Champion() { Id = 98, Name = "Shen" });
            context.Champions.AddOrUpdate(new Champion() { Id = 99, Name = "Lux" });
            context.Champions.AddOrUpdate(new Champion() { Id = 101, Name = "Xerath" });
            context.Champions.AddOrUpdate(new Champion() { Id = 102, Name = "Shyvana" });
            context.Champions.AddOrUpdate(new Champion() { Id = 103, Name = "Ahri" });
            context.Champions.AddOrUpdate(new Champion() { Id = 104, Name = "Graves" });
            context.Champions.AddOrUpdate(new Champion() { Id = 105, Name = "Fizz" });
            context.Champions.AddOrUpdate(new Champion() { Id = 106, Name = "Volibear" });
            context.Champions.AddOrUpdate(new Champion() { Id = 107, Name = "Rengar" });
            context.Champions.AddOrUpdate(new Champion() { Id = 110, Name = "Varus" });
            context.Champions.AddOrUpdate(new Champion() { Id = 111, Name = "Nautilus" });
            context.Champions.AddOrUpdate(new Champion() { Id = 112, Name = "Viktor" });
            context.Champions.AddOrUpdate(new Champion() { Id = 113, Name = "Sejuani" });
            context.Champions.AddOrUpdate(new Champion() { Id = 114, Name = "Fiora" });
            context.Champions.AddOrUpdate(new Champion() { Id = 115, Name = "Ziggs" });
            context.Champions.AddOrUpdate(new Champion() { Id = 117, Name = "Lulu" });
            context.Champions.AddOrUpdate(new Champion() { Id = 119, Name = "Draven" });
            context.Champions.AddOrUpdate(new Champion() { Id = 120, Name = "Hecarim" });
            context.Champions.AddOrUpdate(new Champion() { Id = 121, Name = "Kha''Zix" });
            context.Champions.AddOrUpdate(new Champion() { Id = 122, Name = "Darius" });
            context.Champions.AddOrUpdate(new Champion() { Id = 126, Name = "Jayce" });
            context.Champions.AddOrUpdate(new Champion() { Id = 127, Name = "Lissandra" });
            context.Champions.AddOrUpdate(new Champion() { Id = 131, Name = "Diana" });
            context.Champions.AddOrUpdate(new Champion() { Id = 133, Name = "Quinn" });
            context.Champions.AddOrUpdate(new Champion() { Id = 134, Name = "Syndra" });
            context.Champions.AddOrUpdate(new Champion() { Id = 136, Name = "Aurelion Sol" });
            context.Champions.AddOrUpdate(new Champion() { Id = 143, Name = "Zyra" });
            context.Champions.AddOrUpdate(new Champion() { Id = 150, Name = "Gnar" });
            context.Champions.AddOrUpdate(new Champion() { Id = 154, Name = "Zac" });
            context.Champions.AddOrUpdate(new Champion() { Id = 157, Name = "Yasuo" });
            context.Champions.AddOrUpdate(new Champion() { Id = 161, Name = "Vel''Koz" });
            context.Champions.AddOrUpdate(new Champion() { Id = 163, Name = "Taliyah" });
            context.Champions.AddOrUpdate(new Champion() { Id = 201, Name = "Braum" });
            context.Champions.AddOrUpdate(new Champion() { Id = 202, Name = "Jhin" });
            context.Champions.AddOrUpdate(new Champion() { Id = 203, Name = "Kindred" });
            context.Champions.AddOrUpdate(new Champion() { Id = 222, Name = "Jinx" });
            context.Champions.AddOrUpdate(new Champion() { Id = 223, Name = "Tahm Kench" });
            context.Champions.AddOrUpdate(new Champion() { Id = 236, Name = "Lucian" });
            context.Champions.AddOrUpdate(new Champion() { Id = 238, Name = "Zed" });
            context.Champions.AddOrUpdate(new Champion() { Id = 245, Name = "Ekko" });
            context.Champions.AddOrUpdate(new Champion() { Id = 254, Name = "Vi" });
            context.Champions.AddOrUpdate(new Champion() { Id = 266, Name = "Aatrox" });
            context.Champions.AddOrUpdate(new Champion() { Id = 267, Name = "Nami" });
            context.Champions.AddOrUpdate(new Champion() { Id = 268, Name = "Azir" });
            context.Champions.AddOrUpdate(new Champion() { Id = 412, Name = "Thresh" });
            context.Champions.AddOrUpdate(new Champion() { Id = 420, Name = "Illaoi" });
            context.Champions.AddOrUpdate(new Champion() { Id = 421, Name = "Rek''Sai" });
            context.Champions.AddOrUpdate(new Champion() { Id = 429, Name = "Kalista" });
            context.Champions.AddOrUpdate(new Champion() { Id = 432, Name = "Bard" });
        }
    }
}
