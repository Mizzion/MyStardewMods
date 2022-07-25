using System;
using System.Collections.Generic;
using BankOfFerngill.Framework.Configs;
using BankOfFerngill.Framework.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BankOfFerngill.Framework.Menu
{
    public class BankInfoMenu : IClickableMenu
    {
        private readonly IMonitor Monitor;
        private ITranslationHelper _i18n;
        private BankData _bankData;

        private static int UIWidth = 1280;
        private static int UIHeight = 760;
        private int XPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - 
                           (UIWidth / 2);

        private int YPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                           (UIHeight / 2);

        private ClickableComponent TitleLabel;
        private Dictionary<string, string> _bankInfo;

        private string _titleLabel,
            _accountNumber,
            _accountBalance,
            _bankInterest,
            _loanBalance,
            _loanPaidBack,
            _loansPaidOff,
            _totalLoans,
            _loanInterest;
        
        public BankInfoMenu(IMonitor monitor, ITranslationHelper i18N, BankData bankData)
        {
            base.initialize(XPos, YPos, UIWidth, UIHeight);
            
            Monitor = monitor;
            _i18n = i18N; 
            //Helper.Translation.Get("haxorsprinkler.name");
            //i18n.Get("npc_dialogue"+outty, new { player_name = Game1.player.Name });
            _bankData = bankData ?? new BankData();
            
            TitleLabel = new ClickableComponent(new Rectangle(XPos + 500, YPos + 96, UIWidth - 400, 128), _i18n.Get("bank.info.title"));
        }
        
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            Game1.drawDialogueBox(XPos, YPos, UIWidth, UIHeight, false, true);
            //Draw Title Label
            
            Utility.drawTextWithShadow(b, TitleLabel.name, Game1.dialogueFont, new Vector2(TitleLabel.bounds.X, TitleLabel.bounds.Y), Color.Black);
            
            var y = 64;
            var total = _bankData.LoanedMoney - _bankData.MoneyPaidBack;
            //i18n.Get("npc_dialogue"+outty, new { player_name = Game1.player.Name });
            //SpriteText.drawStringHorizontallyCenteredAt(b, _titleLabel, XPos + 600, YPos + 120);
            SpriteText.drawString(b, _i18n.Get("bank.info.accountNumber", new{ account_number = Game1.player.UniqueMultiplayerID}), XPos + 45, TitleLabel.bounds.Y + (y * 1), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.accountBalance", new{ account_balance = _bankData.MoneyInBank}), XPos + 45, TitleLabel.bounds.Y + (y * 2), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.bankInterest", new{ bank_interest = _bankData.BankInterest}), XPos + 45, TitleLabel.bounds.Y + (y * 3), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.loanBalance", new{ loan_balance = _bankData.LoanedMoney, loan_owed = ModEntry.FormatNumber(total)}), XPos + 45, TitleLabel.bounds.Y + (y * 4), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.loanPaidBack", new{ loan_paid_back = _bankData.MoneyPaidBack}), XPos + 45, TitleLabel.bounds.Y + (y * 5), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.loansPaidOff", new{ loans_paid_off = _bankData.NumberOfLoansPaidBack}), XPos + 45, TitleLabel.bounds.Y + (y * 6), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.totalLoans", new{ total_loans = _bankData.TotalNumberOfLoans}), XPos + 45, TitleLabel.bounds.Y + (y * 7), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            SpriteText.drawString(b, _i18n.Get("bank.info.loanInterest", new{ loan_interest = _bankData.LoanInterest}), XPos + 45, TitleLabel.bounds.Y + (y * 8), 999999, -1, 9999, 0.75f, 0.865f, junimoText: false);
            
            
            //Draw the mouse
            drawMouse(b);
        }

        
    }
}