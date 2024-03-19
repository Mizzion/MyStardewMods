using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Common.Integrations;
using Object = StardewValley.Object;

namespace EnhancedRelationships
{
    internal class EnhancedRelationships : Mod//, IAssetEditor
    {
        private ModConfig _config;        
        private readonly List<NPC> _birthdayMessageQueue = new();
        private readonly IDictionary<string, int> _gaveNpcGift = new Dictionary<string, int>();
        private IDictionary<string, string> _npcGifts = new Dictionary<string, string>();
        private const bool Debugging = true;
        private Mizzion.Stardew.Common.Integrations.GenericModConfigMenu.IGenericModConfigMenuApi _cfgMenu;

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();            
           
            //GameLoop Events
            Helper.Events.GameLoop.DayStarted += DayStarted;
            Helper.Events.GameLoop.Saving += Saving;
            Helper.Events.GameLoop.GameLaunched += GameLaunched;

            //Player Events
            Helper.Events.Player.InventoryChanged += InventoryChanged;
           

            //Let's add in the new content events
            Helper.Events.Content.AssetRequested += AssetRequested;
        }

        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            #region Generic Moc Config Menu
            /*
            _cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_cfgMenu is null) return;

            //Register mod
            _cfgMenu.Register(
                mod: ModManifest,
                reset: () => _config = new BoFConfig(),
                save: () => Helper.WriteConfig(_config)
                );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Bank of Ferngill Settings",
                tooltip: null
                );

            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Base Banking Interest",
                tooltip: () => "The base value that gets calculated into the daily interest for any money in the bank",
                getValue: () => _config.BaseBankingInterest,
                setValue: value => _config.BaseBankingInterest = value
            );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableRandomEvents,
                setValue: value => _config.EnableRandomEvents = value,
                name: () => "Enable Random Events",
                tooltip: () => "Enable Random Events, that happens during the nightly update. Win money, Lose money and so on. (Coming Soon)"
            );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableVaultRoomDeskActivation,
                setValue: value => _config.EnableVaultRoomDeskActivation = value,
                name: () => "Click desk for Bank",
                tooltip: () => "Enable if you want to bypass needing the vault room completed to activate the bank."
            );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Loan Settings",
                tooltip: null
                );

            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Loan Interest",
                tooltip: () => "The base value that gets calculated into the interest for any loan you take out.",
                getValue: () => _config.LoanSettings.LoanBaseInterest,
                setValue: value => _config.LoanSettings.LoanBaseInterest = value
            );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.LoanSettings.PayBackLoanDaily,
                setValue: value => _config.LoanSettings.PayBackLoanDaily = value,
                name: () => "Pay Back a Portion of Loan Daily",
                tooltip: () => "If enabled you will pay back a portion of the loan each night. If possible"
            );

            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percentage of Loan to Pay Back Daily",
                tooltip: () => "Percent of Loan Paid Daily",
                getValue: () => _config.LoanSettings.PercentageOfLoanToPayBackDaily,
                setValue: value => _config.LoanSettings.PercentageOfLoanToPayBackDaily = value
            );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.LoanSettings.EnableUnlimitedLoansAtOnce,
                setValue: value => _config.LoanSettings.EnableUnlimitedLoansAtOnce = value,
                name: () => "Unlimited Loans",
                tooltip: () => "If enabled you will be able to have any number of loans at once."
            );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Hard Mode Settings",
                tooltip: null
                );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.HardModSettings.EnableHarderMode,
                setValue: value => _config.HardModSettings.EnableHarderMode = value,
                name: () => "Enable Hard Mode",
                tooltip: () => "If enabled you will have debt that needs to be paid back before you can deposit any money."
            );
            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Starting Debt",
                tooltip: () => "How much debt to start with.",
                getValue: () => _config.HardModSettings.HowFarInDebtAtStart,
                setValue: value => _config.HardModSettings.HowFarInDebtAtStart = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.HardModSettings.BypassHavingToRepayDebtFirst,
                setValue: value => _config.HardModSettings.BypassHavingToRepayDebtFirst = value,
                name: () => "Bypass Repaying Debt First",
                tooltip: () => "If enabled you can deposit without first repaying your debt."
            );*/
            #endregion
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            try
            {
                _birthdayMessageQueue.Clear();
                var today = SDate.Now();
                var yesterday = today.AddDays(-1);
                var chars = Utility.getAllCharacters();

                if (today.DaysSinceStart != 1)
                    return;
                
                foreach (var character in chars)
                {
                    DoLogic(character, yesterday.Day, GetSeason(yesterday.Season));
                }

                foreach (var birthdayMessage in _birthdayMessageQueue)
                {
                    birthdayMessage.CurrentDialogue.Push(new Dialogue(birthdayMessage, PickRandomDialogue()));
                }
                    
            }
            catch(Exception ex)
            {
                Monitor.Log(ex.ToString());
            }
        }
        
        
        //Custom Voids
        private string GetSeason(Season season)
        {
            string seasonName = "";
            
            switch(season)
            {
                case Season.Spring:
                    seasonName = "Spring";
                break;
                
                case Season.Summer:
                    seasonName = "Summer";
                    break;
                case Season.Fall:
                    seasonName = "Fall";
                    break;
                case Season.Winter:
                    seasonName = "Winter";
                    break;
            }

            return seasonName;
        }

        private void DoLogic(NPC npc, int day, string season)
        {
            
        }

        private string PickRandomDialogue()
        {
            
            
        }

        private IDictionary<string, string> GetNpcGifts(bool loved = false)
        {
            var giftTastes = DataLoader.NpcGiftTastes(Game1.content);

            IDictionary<string, string> results = new Dictionary<string, string>();
            var giftNames = "";

            foreach (var tastes in giftTastes)
            {
                if (tastes.Value.Contains('/')) continue;
                
            }


            return results;
        }
    }
}
