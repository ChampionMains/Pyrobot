using System;
using ChampionMains.Pyrobot.Data.Enums;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    public class ConsoleTestJob
    {
        public void Execute([QueueTrigger(WebJobQueue.ConsoleTest)] string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}
