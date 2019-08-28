using System;

namespace ChampionMains.Pyrobot.WebJob
{
    public class WebJobConfiguration
    {
        public TimeSpan TimeoutBulkUpdate { get; set; } = TimeSpan.FromMinutes(5);
        public int BulkUpdateSaveBatchSize { get; set; } = 100;
        public int BulkUpdateUpdateBatchSize { get; set; } = 100;
        public int BulkUpdateNumBatches { get; set; } = 2;
    }
}
