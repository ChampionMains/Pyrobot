﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs.Jobs
{
    public class ConfirmFlairUpdatedMailJob
    {
        private const string Subject = "Your flair has been updated";

        private readonly ApplicationConfiguration _config;
        private readonly IRedditMessengerService _mailService;
        private readonly IUserService _userService;

        public ConfirmFlairUpdatedMailJob(ApplicationConfiguration config, IRedditMessengerService mailService,
            IUserService userService)
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

            if (!await _mailService.SendMessageAsync(user.Name, Subject, GetMailmessage(user, summoner)))
            {
                throw new InvalidOperationException("Unable to send confirmation mail message.");
            }
        }

        private string GetMailmessage(User user, Summoner summoner)
        {
            const string pattern = @"*I'm a bot whose purpose is to deliver League of Legends flairs.*

----

> **This message is to notify you that the flair `{flair}` has been delivered to your Reddit account.**

> **From time to time, we'll check if your rank changes and update your flair. You won't hear back from me again. Thanks.**

----

[Report a problem](https://www.reddit.com/message/compose?to=kivinkujata&subject=Issue with FeralFlair) | 
[Author](https://www.reddit.com/message/compose?to=kivinkujata&subject=Ranked Flairs) |
[GitHub](https://github.com/jessehallam/RedditRankedFlairs) | {version}";

            var message = pattern.Replace("{flair}", LeagueUtil.Stringify(summoner.LeagueInfo));
            message = pattern.Replace("{version}", _config.FlairBotVersion);
            return message;
        }
    }
}