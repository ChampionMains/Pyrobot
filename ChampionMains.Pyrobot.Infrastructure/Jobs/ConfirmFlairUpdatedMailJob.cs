using System;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Jobs
{
    public class ConfirmFlairUpdatedMailJob
    {
        private const string Subject = "Your flair has been updated";

        private readonly ApplicationConfiguration _config;
        private readonly RedditService _mailService;
        private readonly UserService _userService;

        public ConfirmFlairUpdatedMailJob(ApplicationConfiguration config, RedditService mailService, UserService userService)
        {
            _config = config;
            _mailService = mailService;
            _userService = userService;
        }

        public void Execute(int userId, int summonerId)
        {
            ExecuteAsync(userId, summonerId).Wait();
        }

        private async Task ExecuteAsync(int userId, int summonerId)
        {
            var user = await _userService.FindAsync(userId);
            var summoner = user?.Summoners.FirstOrDefault(x => x.Id == summonerId);

            if (user == null) return;
            if (summoner == null) return;

            try
            {
                await _mailService.SendMessageAsync(user.Name, Subject, GetMailmessage(user, summoner));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to send confirmation mail message.", e);
            }
        }

        private string GetMailmessage(User user, Summoner summoner)
        {
            const string pattern = @"*I'm a bot whose purpose is to deliver League of Legends flairs.*

----

> **This message is to notify you that the flair `{flair}` has been delivered to your Reddit account.**

> **From time to time, we'll check if your rank changes and update your flair. You won't hear back from me again. Thanks.**

----

[Report a problem](https://www.reddit.com/message/compose?to=kivinkujata&subject=Issue+with+FeralFlair) | 
[Author](https://www.reddit.com/message/compose?to=kivinkujata&subject=Ranked+Flairs) |
[GitHub](https://github.com/jessehallam/RedditRankedFlairs) | {version}";

            return pattern.Replace("{flair}", RankUtil.Stringify(summoner.Rank))
                .Replace("{version}", _config.FlairBotVersion);
        }
    }
}