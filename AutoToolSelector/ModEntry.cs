using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Tools;

namespace AutoToolSelector
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += ButtonPressed;
            helper.Events.GameLoop.OneSecondUpdateTicked += OneSecondUpdateTicked;
        }

        private void OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {

        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;


        }
    }
}
