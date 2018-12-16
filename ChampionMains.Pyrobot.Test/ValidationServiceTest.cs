using System;
using ChampionMains.Pyrobot.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChampionMains.Pyrobot.Test
{
    [TestClass]
    public class ValidationServiceTest
    {
        [TestMethod]
        public void TestBasic()
        {
            var validator = new ValidationService("deadbeef1998");
            var token = validator.GenerateToken("LugnutsK", "1skjlfKSFJLK99--2983a", "NA", "LugnutsK", 28);
            Console.WriteLine(token);
            Assert.IsTrue(validator.ValidateToken(token, "Lugnutsk", "1skjlfKSFJLK99--2983a", "NA", "LugNUtsK", 28));
            // Different profile icon.
            Assert.IsFalse(validator.ValidateToken(token, "LugnutsK", "1skjlfKSFJLK99--2983a", "NA", "LugnutsK", 27));
            Assert.IsFalse(validator.ValidateToken(token, "LugnutsK", "1skjlfKSFJLK99-2983a", "NA", "LugnutsK", 28));
        }

        [TestMethod]
        public void TestExpired()
        {
            var validator = new ValidationService("deadbeef1998");
            var oldToken = validator.GenerateTokenInternal(
                ValidationService.CurrentEpochMillis() - 6 * 60_000,
                "LugnutsK", "1skjlfKSFJLK99--2983a", "NA", "LugnutsK", 5);
            Console.WriteLine(oldToken);
            Assert.IsFalse(validator.ValidateToken(oldToken, "LugnutsK", "1skjlfKSFJLK99--2983a", "NA", "LugnutsK", 5));
        }
    }
}
