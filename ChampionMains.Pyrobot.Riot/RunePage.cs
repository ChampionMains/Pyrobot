using System.Collections.Generic;

namespace ChampionMains.Pyrobot.Riot
{
    public class RunePage
    {
        public bool Current { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public ICollection<RuneSlot> Slots { get; set; }
    }

    public class RuneSlot
    {
        public int RuneId { get; set; }
        public int RuneSlotId { get; set; }
    }
}