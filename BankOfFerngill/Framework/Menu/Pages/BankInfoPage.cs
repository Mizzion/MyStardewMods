using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace BankOfFerngill.Framework.Menu.Pages
{
    public class BankInfoPage : IClickableMenu
    {
        private int menuX;
        private int menuY;
        private int page;
        
        public BankInfoPage(int x, int y, int width, int height, int pageNum) : base(x, y, width, height)
        {
            menuX = x;
            menuY = y;
            page = pageNum;
        }

        public override void draw(SpriteBatch b)
        {
            switch (page)
            {
                case 0:
                    Utility.drawBoldText(b, "Test", Game1.dialogueFont, new Vector2(menuX + 50, menuY + 200), Game1.textColor);
                    break;
                case 1:
                    Utility.drawBoldText(b, "Test1", Game1.dialogueFont, new Vector2(menuX + 50, menuY + 200), Game1.textColor);
                    break;
                case 2:
                    Utility.drawBoldText(b, "Test2", Game1.dialogueFont, new Vector2(menuX + 50, menuY + 200), Game1.textColor);
                    break;
                case 3:
                    Utility.drawBoldText(b, "Test3", Game1.dialogueFont, new Vector2(menuX + 50, menuY + 200), Game1.textColor);
                    break;
                case 4:
                    Utility.drawBoldText(b, "Test4", Game1.dialogueFont, new Vector2(menuX + 50, menuY + 200), Game1.textColor);
                    break;
            }
        }
    }
}