using StardewModdingAPI;

namespace FarmHelper.Framework;

public class CheatSettings
{
    public SButton TeleportKey { get; set; } = SButton.NumPad9;
    public SButton MineDownLevelKey { get; set; } = SButton.NumPad8;
    public SButton AddFestivalScoreKey { get; set; } = SButton.NumPad7;
    public int FestivalScoreToAdd { get; set; } = 100;

}