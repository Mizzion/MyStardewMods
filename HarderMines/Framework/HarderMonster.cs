using StardewValley.Monsters;

namespace HarderMines.Framework
{
    internal class HarderMonster
    {
        public bool AttributesIniated;
        public Monster Monster;

        public HarderMonster(Monster monster)
        {
            Monster = monster;
            AttributesIniated = false;
        }
    }
}
