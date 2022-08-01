using BankOfFerngill.Framework.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Network;

namespace BankOfFerngill.Framework.Menu.Pages
{
    public class DepositPage : IClickableMenu
    {

        private IMonitor _monitor;
        private ITranslationHelper _i18N;
        private BankData _bankData;
        private int menuX;
        private int menuY;
        private int _width;
        private int _height;
        
        
        //Page variables and delegate
        public delegate void DoneBehaviour(string s);
        private readonly ClickableComponent _title;
        private readonly ClickableTextureComponent _okButtonCc;
        private readonly TextBox _textBox;
        private readonly DoneBehaviour _doneBehaviour;
        public DepositPage(int x, int y, int width, int height, IMonitor monitor, ITranslationHelper i18n,
            BankData bankData) : base(x, y, width, height)
        {
            initialize(x, y, width, height);
            _monitor = monitor;
            _i18N = i18n;
            _bankData = bankData;
            menuX = x;
            menuY = y;
            _width = width;
            _height = height;
            
            
            _title = new ClickableComponent(new Rectangle(menuX + 600, menuY + 100, _width - 400, 128), _i18N.Get("bank.deposit.title"));
            
            _textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor)
            {
                X = menuX + (width - 512) / 2,
                Y = _title.bounds.Y + (height - 200),
                Width = 512,
                Height = 192
            };
            Game1.keyboardDispatcher.Subscriber = _textBox;
            _textBox.OnEnterPressed += TextBoxEnter;
            _textBox.Selected = true;

            _doneBehaviour = DoDeposit;
            
            _okButtonCc =
                new ClickableTextureComponent(
                    new Rectangle(_textBox.X + _textBox.Width + 32 + 4, _textBox.Y, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
        }

        public override void draw(SpriteBatch b)
        {
            Utility.drawTextWithShadow(b, _title.name, Game1.dialogueFont, new Vector2(_title.bounds.X, _title.bounds.Y), Color.Black);
            SpriteText.drawString(b, _i18N.Get("bank.deposit.description", new{player_name = Game1.player.Name, bank_balance = ModEntry.FormatNumber(_bankData.MoneyInBank)}), menuX + 45, _title.bounds.Y + 64, 999999, _width, 9999, 0.75f, 0.865f, junimoText: false);
            
            _textBox.Draw(b);
            _okButtonCc.draw(b);
            
            //Draw the mouse
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            
            _textBox.Update();

            
            if (_okButtonCc.containsPoint(x, y))
            {
                _monitor.Log($"Ok button clicked X:{x}, Y:{y}");
                TextBoxEnter(_textBox);
            }
        }

        private void TextBoxEnter(TextBox sender)
        {
            if (sender.Text.Length >= 1)
            {
                if (_doneBehaviour is not null)
                {
                    _doneBehaviour(sender.Text);
                    _textBox.Selected = false;
                }
                else
                {
                    _monitor.Log("The Ok button was null.");
                }
            }
        }

        private void DoDeposit(string s)
        {
            _monitor.Log($"Ok button was pressed {s}");
        }
        
    }
}