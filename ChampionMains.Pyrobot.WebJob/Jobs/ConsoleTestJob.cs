using System;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    public class ConsoleTestJob
    {
        public async Task Execute([QueueTrigger(WebJobQueue.ConsoleTest)] string text)
        {
            Console.Out.WriteLine(text);
            await Task.Yield();
        }
    }
}
