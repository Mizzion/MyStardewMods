namespace FarmCleaner.Framework.Configs
{
    internal class TreeConfig
    {
        public bool RemoveTrees { get; set; } = true; //Whether or not to clear any trees
        public bool LeaveRandomTree { get; set; } = true; //Whether or not to randomize tree selection       
        public int RandomTreeChance { get; set; } = 10;
        public bool ShakeTree { get; set; } = true; //Whether or not to shake trees
        public bool RemoveSeed { get; set; } = false; // seedStage = 0;
        public bool RemoveSprout { get; set; } = false; // sproutStage = 1;
        public bool RemoveSapling { get; set; } = false; // saplingStage = 2;
        //public bool ClearBush { get; set; } = true; // bushStage = 3;
        public bool RemoveSmallTree { get; set; } = true; //treeStage = 4;
        public bool RemoveMatureTree { get; set; } = true; //treeStage = 5;

        public bool RemoveTappedTree { get; set; } = true; // Whether or not to remove tapped trees

        public bool RemoveFruitTree { get; set; } = true; //Whether or not to remove fruit trees
    }
}
