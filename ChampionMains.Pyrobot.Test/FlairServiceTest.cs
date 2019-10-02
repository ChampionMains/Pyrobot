using ChampionMains.Pyrobot.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reddit.Things;

namespace ChampionMains.Pyrobot.Test
{
    [TestClass]
    public class FlairServiceTest
    {
        [TestMethod]
        public void TestIsFlairUnchanged()
        {
            Assert.IsTrue(FlairService.IsFlairUnchanged(
                new FlairListResult
                {
                    FlairCssClass = "",
                    User = "LugnutsK",
                    FlairText = "hello",
                },
                new FlairListResult
                {
                    FlairCssClass = "",
                    User = "LugnutsK",
                    FlairText = "hello",
                }
            ));

            Assert.IsTrue(FlairService.IsFlairUnchanged(
                new FlairListResult
                {
                    FlairCssClass = "hello   world\tworld",
                    User = "LugnutsK",
                    FlairText = null,
                },
                new FlairListResult
                {
                    FlairCssClass = "world hello",
                    User = "LugnutsK",
                    FlairText = "",
                }
            ));

            Assert.IsTrue(FlairService.IsFlairUnchanged(
                null,
                new FlairListResult
                {
                    FlairCssClass = "   \t ",
                    User = "LugnutsK",
                    FlairText = "",
                }
            ));

            Assert.IsFalse(FlairService.IsFlairUnchanged(
                new FlairListResult
                {
                    FlairCssClass = "PANcake",
                    User = "LugnutsK",
                    FlairText = null,
                },
                new FlairListResult
                {
                    FlairCssClass = "pancake",
                    User = "LugnutsK",
                    FlairText = null,
                }
            ));
        }
    }
}
