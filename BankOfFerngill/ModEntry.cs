using System;
using System.Collections.Generic;
using BankOfFerngill.Framework;
using BankOfFerngill.Framework.Configs;
using BankOfFerngill.Framework.Data;
using BankOfFerngill.Framework.Menu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BankOfFerngill
{
    public class ModEntry : Mod
    {
        private BoFConfig _config;
        private ITranslationHelper _i18N;
        private IModEvents _events;
        private BankData _bankData;
        private bool Debugging;
        private int _maxLoan = 0;
        private int _loanOwed = 0;
        private int _lostAmt = 0;
        private int _gainedAmt = 0;
        private int _giftAmt = 0;

        private readonly List<Vector2> _vaultCoords = new()
        {
            new Vector2(54,1), 
            new Vector2(54,2),
            new Vector2(54,3),
            new Vector2(54,4),
            new Vector2(54,5),
            new Vector2(55,1), 
            new Vector2(55,2),
            new Vector2(55,3),
            new Vector2(55,4),
            new Vector2(55,5),
            new Vector2(56,1), 
            new Vector2(56,2),
            new Vector2(56,3),
            new Vector2(56,4),
            new Vector2(56,5),
            new Vector2(57,1), 
            new Vector2(57,2),
            new Vector2(57,3),
            new Vector2(57,4),
            new Vector2(57,5),
            
        };

        private readonly List<Vector2> _deskCords = new()
        {
            new Vector2(48, 4),
            new Vector2(48, 5),
            new Vector2(48, 6),
            new Vector2(49, 4),
            new Vector2(49, 5),
            new Vector2(49, 6),
            new Vector2(50, 4),
            new Vector2(50, 5),
            new Vector2(50, 6),
            new Vector2(51, 4),
            new Vector2(51, 5),
            new Vector2(51, 6)
        };

        private Vector2 _jojaMartCoords = new Vector2(24, 24);
        
        
        /// <summary>
        /// The entry method
        /// </summary>
        /// <param name="helper">Helper interface</param>
        public override void Entry(IModHelper helper)
        {
            //Load the config file
            _config = helper.ReadConfig<BoFConfig>();
            
            //Set up translations
            _i18N = Helper.Translation;
            
            //Load Events
            _events = helper.Events;
            
            
            //Events
            _events.Input.ButtonPressed += OnButtonPressed; //Event that triggers when a button is pressed.
            _events.GameLoop.SaveLoaded += OnSaveLoaded; //Event that triggers when a Save is loaded.
            //_events.GameLoop.Saving += OnSaving; //Event that triggers before a save is saved.
            _events.GameLoop.SaveCreated += OnSaveCreated; //Event that triggers when a new save is created.
            _events.GameLoop.DayEnding += OnDayEnding; //Event that triggers when the day ends.
            _events.GameLoop.Saved += OnSaved; //Event that triggers after a save is saved.
            _events.GameLoop.GameLaunched += OnGameLaunched; //Event that triggers when the game is launched.
            _events.Content.AssetRequested += OnAssetRequested; //Event that triggers when the game is requesting an asset.
        }
        
        //Event Methods

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (cfgMenu is null) return;
            
            //Register mod
            cfgMenu.Register(
                mod: ModManifest,
                reset: () => _config =  new BoFConfig(),
                save: () => Helper.WriteConfig(_config)
                );
            
            cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Bank of Ferngill Settings",
                tooltip: null
                );
            
            cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Base Banking Interest",
                tooltip: () => "The base value that gets calculated into the daily interest for any money in the bank",
                getValue: () => _config.BaseBankingInterest,
                setValue: value => _config.BaseBankingInterest = value
            );
            
            cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableRandomEvents,
                setValue: value => _config.EnableRandomEvents = value,
                name: () => "Enable Random Events",
                tooltip:() => "Enable Random Events, that happens during the nightly update. Win money, Lose money and so on. (Coming Soon)"
            );
            
            cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableVaultRoomDeskActivation,
                setValue: value => _config.EnableVaultRoomDeskActivation = value,
                name: () => "Click desk for Bank",
                tooltip:() => "Enable if you want to bypass needing the vault room completed to activate the bank."
            );
            
            cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Loan Settings",
                tooltip: null
                );
            
            cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Loan Interest",
                tooltip: () => "The base value that gets calculated into the interest for any loan you take out.",
                getValue: () => _config.LoanSettings.LoanBaseInterest,
                setValue: value => _config.LoanSettings.LoanBaseInterest = value
            );
            
            cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.LoanSettings.PayBackLoanDaily,
                setValue: value => _config.LoanSettings.PayBackLoanDaily = value,
                name: () => "Pay Back a Portion of Loan Daily",
                tooltip:() => "If enabled you will pay back a portion of the loan each night. If possible"
            );
            
            cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percentage of Loan to Pay Back Daily",
                tooltip: () => "Percent of Loan Paid Daily",
                getValue: () => _config.LoanSettings.PercentageOfLoanToPayBackDaily,
                setValue: value => _config.LoanSettings.PercentageOfLoanToPayBackDaily = value
            );
            
            cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.LoanSettings.EnableUnlimitedLoansAtOnce,
                setValue: value => _config.LoanSettings.EnableUnlimitedLoansAtOnce = value,
                name: () => "Unlimited Loans",
                tooltip:() => "If enabled you will be able to have any number of loans at once."
            );
            
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.IsDown(SButton.F5))
            {
                _config = Helper.ReadConfig<BoFConfig>();
                Monitor.Log("The config file was reloaded.");
            }

            if (e.IsDown(SButton.NumPad4) && Debugging)
            {
                DoBanking();
            }

            if (e.IsDown(SButton.F10) && Debugging)
            {
                var stockTanked = 0;
                var stockRose = 0;
                var customerAppreciation = 0;
                var accountHacked = 0;
                var debtPaid = 0;
                var nothingDone = 0;
                
                //DoRandomEvent();
                for (var i = 0; i < 20; i++)
                {
                    var r = Game1.random.NextDouble();
                    var ran = new Random();
                    /*
                    switch (ran.NextDouble())
                    {
                        case < .05:
                            debtPaid++;
                            break;
                        case < .07:
                            accountHacked++;
                            break;
                        case < .10:
                            customerAppreciation++;
                            break;
                        case < .15:
                            stockRose++;
                            break;
                        case < .20:
                            stockTanked++;
                            break;
                        default:
                            nothingDone++;
                            break;
                        
                    }*/

                    var ra = ran.NextDouble();
                    Monitor.Log($"Random was: {ra}",LogLevel.Alert);

                    switch (ra)
                    {
                        case < .05:
                            debtPaid++;
                            continue;
                        case < .07:
                            accountHacked++;
                            continue;
                        case < .10:
                            customerAppreciation++;
                            continue;
                        case < .15:
                            stockRose++;
                            continue;
                        case < .20:
                            stockTanked++;
                            continue;
                    }
                }
                Monitor.Log($"StockTanked: {stockTanked}, StockRose: {stockRose}, CustomerAppreciation: {customerAppreciation}, AccountHacked: {accountHacked}, DebtPaid: {debtPaid}, NothingDone: {nothingDone}", LogLevel.Warn);
            }
            if(e.IsDown(SButton.Escape) && Game1.activeClickableMenu is BankMenu or BankInfoMenu)
            {
                Game1.exitActiveMenu();
            }
            if (e.IsDown(SButton.MouseRight) && 
                (Game1.currentLocation.Name.Contains("Community") && ((_vaultCoords.Contains(Game1.currentCursorTile) && Game1.player.mailReceived.Contains("ccVault")) || 
                                                                      (_config.EnableVaultRoomDeskActivation && _deskCords.Contains(Game1.currentCursorTile) && Game1.player.mailReceived.Contains("ccVault")))) || 
                (Game1.currentLocation.Name.Contains("JojaMart") && _jojaMartCoords == Game1.currentCursorTile) && Game1.player.mailReceived.Contains("jojaVault"))
            {
                DoBanking();
            }
        }

        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            _bankData = new BankData();
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _bankData = Helper.Data.ReadSaveData<BankData>(ModManifest.UniqueID) ?? new BankData();

            _bankData.BankInterest = _config.BaseBankingInterest > 0 ? _config.BaseBankingInterest : 0;
            _bankData.LoanInterest = _config.LoanSettings.LoanBaseInterest > 0 ? _config.LoanSettings.LoanBaseInterest : 0;
            
            _loanOwed = 0;
            _maxLoan = 0;
            
            //Enable or disable Debugging Based on FarmerName
            Debugging = Game1.player.Name.Contains("Vrillion");
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            //Write the bank data to the save before it actually saves.
            if (_bankData is not null)
            {
                if(_bankData.MoneyInBank > 0)
                    _bankData.MoneyInBank += CalculateInterest(_bankData.MoneyInBank, _bankData.BankInterest);

                var loanOwed = _bankData.LoanedMoney - _bankData.MoneyPaidBack;
                if (_config.LoanSettings.PayBackLoanDaily && Game1.player.Money >=
                    CalculateInterest(_bankData.LoanedMoney, _config.LoanSettings.PercentageOfLoanToPayBackDaily))
                {
                    var total = loanOwed > CalculateInterest(_bankData.LoanedMoney, _config.LoanSettings.PercentageOfLoanToPayBackDaily)
                        ? CalculateInterest(_bankData.LoanedMoney, _config.LoanSettings.PercentageOfLoanToPayBackDaily)
                        : loanOwed;
                    _bankData.MoneyPaidBack += total;
                    
                    Game1.player.Money -= total;

                    if (_bankData.MoneyPaidBack == _bankData.LoanedMoney && _bankData.TotalNumberOfLoans > 0)
                    {
                        _bankData.MoneyPaidBack = 0;
                        _bankData.LoanedMoney = 0;
                        _bankData.TotalNumberOfLoans = 0;
                        _bankData.NumberOfLoansPaidBack++;
                    }
                    
                }
                
                //Do Random Event.
                if(_config.EnableRandomEvents && _bankData.MoneyInBank > 0)
                    DoRandomEvent();
                
                //Write save data.
                Helper.Data.WriteSaveData(ModManifest.UniqueID, _bankData);
            }
               
            
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
            
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                    //Now we add the mail.
                    data["bankStockTanked"] = _i18N.Get("bank.events.stockTanked", new {player_name = Game1.player.Name, lost_amt = _lostAmt });
                    data["bankStockRose"] = _i18N.Get("bank.events.stockRose", new {player_name = Game1.player.Name, gain_amt = _gainedAmt });
                    data["bankCustomerAppreciation"] = _i18N.Get("bank.events.customerAppreciation", new {player_name = Game1.player.Name, gift_amt = _giftAmt });
                    data["bankAccountHacked"] = _i18N.Get("bank.events.accountHacked", new {player_name = Game1.player.Name });
                    data["bankDebtPaid"] = _i18N.Get("bank.events.debtPaid", new {player_name = Game1.player.Name });
                });
            }
        }
        
        #region Custom Methods
        private void DoBanking()
        {
            if (Game1.activeClickableMenu is not null)
                return;

            _maxLoan = CalculateInterest(Convert.ToInt32(Game1.player.totalMoneyEarned), 10);;
            _loanOwed = _bankData.LoanedMoney - _bankData.MoneyPaidBack;
                
            //Dialogue Responses
            var mainResponses = new List<Response> { new("BankInfo", "Bank Account Information") };
            if (_bankData.MoneyInBank > 0)
            {
                mainResponses.Add(new Response("Withdraw", "Make a Withdraw"));
            }
            mainResponses.Add(new Response("Deposit", "Make a Deposit"));
            if (_bankData.LoanedMoney == 0 || _config.LoanSettings.EnableUnlimitedLoansAtOnce)
            {
                mainResponses.Add(new Response("GetLoan", "Get a Loan"));
            }

            if (_bankData.LoanedMoney > 0)
            {
                mainResponses.Add(new Response("PayBackLoan", "Make a Loan Payment"));
            }
                
            //Now we create the dialogue
            Game1.currentLocation.createQuestionDialogue("Choose an Option from below.", mainResponses.ToArray(),
                delegate(Farmer _, string whichAnswer)
                {
                    Game1.activeClickableMenu = whichAnswer switch
                    {
                        "BankInfo" => new BankInfoMenu(Monitor, Helper.Translation, _bankData),
                        "Withdraw" => new BankMenu(Monitor, Helper.Translation, _config, _bankData, DoWithdraw, _i18N.Get("bank.withdraw.title"), FormatString(_i18N.Get("bank.withdraw.description", new { player_name = Game1.player.Name, bank_balance = _bankData.MoneyInBank}), 1280)),
                        "Deposit" => new BankMenu(Monitor, Helper.Translation, _config, _bankData, DoDeposit, _i18N.Get("bank.deposit.title"), FormatString(_i18N.Get("bank.deposit.description", new { player_name = Game1.player.Name, bank_balance = _bankData.MoneyInBank}), 1280)),                        
                        "GetLoan" => new BankMenu(Monitor, Helper.Translation, _config, _bankData, DoGetLoan, _i18N.Get("bank.getLoan.title"), FormatString(_i18N.Get("bank.getLoan.description", new { player_name = Game1.player.Name, loan_interest = _bankData.LoanInterest, max_loan = _maxLoan, total_money_earned = Game1.player.totalMoneyEarned }), 1280)),
                        "PayBackLoan" => new BankMenu(Monitor, Helper.Translation, _config, _bankData, DoPayLoan, _i18N.Get("bank.payLoan.title"), FormatString(_i18N.Get("bank.payLoan.description", new { player_name = Game1.player.Name, loan_owed = _loanOwed }), 1280)),
                        _ => Game1.activeClickableMenu = null
                    };
                });

        }
        
        //Private Methods
        private void DoRandomEvent()
        {
            //Coming Soon
            
            var rand = Game1.random.Next(0, 100);
            //var chance = GetPercentage(rand, 100);
            _lostAmt = GetRandomAmt(1, GetRandomAmt(1, CalculateInterest(_bankData.MoneyInBank > 0 ? _bankData.MoneyInBank : 0, _bankData.BankInterest)));
            _gainedAmt = GetRandomAmt(1, GetRandomAmt(1, CalculateInterest(_bankData.MoneyInBank > 0 ? _bankData.MoneyInBank : 0, _bankData.BankInterest)));
            _giftAmt = GetRandomAmt(1, GetRandomAmt(1, CalculateInterest(_bankData.MoneyInBank > 0 ? _bankData.MoneyInBank : 0, _bankData.BankInterest)));
            var hackedAmt = GetPercentage(_bankData.MoneyInBank, 75);
            
            if(Debugging)
                Monitor.Log($"Random: {rand} and check Value was {0.05 + Game1.player.DailyLuck}");
            
            //Now we invalidate the mail, this way the correct values get added.
            Helper.GameContent.InvalidateCache("Data/mail");
            
            //Now we do our calculations for events.

            switch (rand)
            {
                case > 20:
                {
                    Game1.player.mailForTomorrow.Add("bankAccountHacked");
                    if(Debugging)
                        Monitor.Log($"AccountHacked Lost: {hackedAmt}");
                    break;
                }
                case > 15:
                {
                    Game1.player.mailForTomorrow.Add("bankStockTanked");
                    if (Debugging)
                        Monitor.Log($"StockTanked Lost: {_lostAmt}G");
                    break;
                }
                case > 10:
                {
                    Game1.player.mailForTomorrow.Add("bankStockRose");
                    if(Debugging)
                        Monitor.Log($"StockRose Gained: {_gainedAmt}G");
                    break;
                }
                case > 7:
                {
                    Game1.player.mailForTomorrow.Add("bankCustomerAppreciation");
                    if(Debugging)
                        Monitor.Log($"CustomerAppreciation Gained: {_giftAmt}G");
                    break;
                }
                case > 5:
                {
                    Game1.player.mailForTomorrow.Add("bankDebtPaid");
                    if(Debugging)
                        Monitor.Log($"DebtPaid ForgivenAmt: {_bankData.LoanedMoney}G");
                    break;
                }
                default:
                {
                    if(Debugging)
                        Monitor.Log($"(Rand: {rand}) Nothing was triggered in the event");
                    break;
                }
            }
                

        }
        

        private int GetPercentage(int initialValue, int percentage)
        {
            return (initialValue * percentage) / 100;
        }
        
        private int GetRandomAmt(int min, int max)
        {
            return Game1.random.Next(min, max);
        }
        
        
        //Bank Menus
        private void DoWithdraw(string val)
        {
            try
            {
                var withdrawAmt = int.Parse(val);
                var haveEnoughMoney = _bankData.MoneyInBank >= withdrawAmt;
                if (!haveEnoughMoney)
                {
                    Game1.showGlobalMessage(_i18N.Get("bank.notEnoughMoneyInBank", new { amt = FormatNumber(withdrawAmt)}));
                    return;
                }
                if (withdrawAmt > 0)
                {
                    _bankData.MoneyInBank -= withdrawAmt;
                    Game1.player.Money += withdrawAmt;
                    Game1.exitActiveMenu();
                    Game1.showGlobalMessage(_i18N.Get("bank.withdraw.doWithdraw", new { amt = FormatNumber(withdrawAmt)}));
                }
                else
                {
                    Monitor.Log($"{val} wasn't a valid int.");
                }
            }
            catch (Exception ex)
            {
                Game1.showGlobalMessage(_i18N.Get("bank.nonNumeric"));
                Monitor.Log($"An error was thrown.\r\n {ex}");
            }
            
        }
        
        private void DoDeposit(string val)
        {
            try
            {
                var depositAmt = int.Parse(val);
                var haveEnoughMoney = Game1.player.Money >= depositAmt;
                if (!haveEnoughMoney)
                {
                    Game1.showGlobalMessage(_i18N.Get("bank.notEnoughMoneyOnPlayer", new { amt = FormatNumber(depositAmt) }));
                    return;
                }
                if (depositAmt > 0)
                {
                    _bankData.MoneyInBank += depositAmt;
                    Game1.player.Money -= depositAmt;
                    Game1.exitActiveMenu();
                    Game1.showGlobalMessage(_i18N.Get("bank.deposit.doDeposit", new { amt = FormatNumber(depositAmt) }));
                }
                else
                {
                    Monitor.Log($"{val} wasn't a valid int.");
                }
            }
            catch (Exception ex)
            {
                //Game1.exitActiveMenu();
                Game1.showGlobalMessage(_i18N.Get("bank.nonNumeric"));
                Monitor.Log($"An error was thrown.\r\n {ex}");
            }
            
        }
        
        private void DoGetLoan(string val)
        {
            try
            {
                int loanAmt = int.Parse(val);
                if (loanAmt > _maxLoan)
                {
                    Game1.showGlobalMessage(_i18N.Get("bank.getLoan.canGetThatMuch", new { amt = loanAmt}));
                }
                else if (_bankData.LoanedMoney - _bankData.MoneyPaidBack > 0 && !_config.LoanSettings.EnableUnlimitedLoansAtOnce)
                {
                    Game1.showGlobalMessage(_i18N.Get("bank.getLoan.stillOwe", new { loan_owned = _bankData.LoanedMoney - _bankData.MoneyPaidBack}));
                }
                else
                {
                    _bankData.LoanedMoney = ModEntry.CalculateInterest(loanAmt, _bankData.LoanInterest) + loanAmt;
                    _bankData.TotalNumberOfLoans++;
                    Game1.player.Money += loanAmt;
                    Game1.exitActiveMenu();
                    Game1.showGlobalMessage(_i18N.Get("bank.getLoan.loanTaken", new {loan = loanAmt, loan_interest = CalculateInterest(loanAmt, _bankData.LoanInterest)} ));
                }
            }
            catch (Exception ex)
            {
                Game1.showGlobalMessage(_i18N.Get("bank.nonNumeric"));
                Monitor.Log($"An error was thrown. \r\n{ex}");
            }
        }
        
        private void DoPayLoan(string val)
        {
            try
            {
                int amtLoanPay = int.Parse(val);
                if (amtLoanPay > _bankData.LoanedMoney - _bankData.MoneyPaidBack)
                {
                    Game1.showGlobalMessage(_i18N.Get("bank.payLoan.DontOweThatMuch",new { loan_owed = _bankData.LoanedMoney - _bankData.MoneyPaidBack }));
                }
                else if (Game1.player.Money < amtLoanPay)
                {
                    Game1.showGlobalMessage(_i18N.Get("bank.notEnoughMoneyOnPlayer", new { amt = FormatNumber(amtLoanPay) }));
                }
                else
                {
                    _loanOwed = _bankData.LoanedMoney - _bankData.MoneyPaidBack;
                    var total = _loanOwed > CalculateInterest(_bankData.LoanedMoney, _config.LoanSettings.PercentageOfLoanToPayBackDaily)
                        ? CalculateInterest(_bankData.LoanedMoney, _config.LoanSettings.PercentageOfLoanToPayBackDaily)
                        : _loanOwed;
                    
                    _bankData.MoneyPaidBack += amtLoanPay; //LoanedMoney -= amtLoanPay;
                    Game1.player.Money -= amtLoanPay;
                    if (_bankData.MoneyPaidBack == _bankData.LoanedMoney)
                    {
                        _bankData.LoanedMoney = 0;
                        _bankData.MoneyPaidBack = 0;
                        _bankData.NumberOfLoansPaidBack++;
                        _bankData.TotalNumberOfLoans--;
                    }
                    var s = _bankData.LoanedMoney > 0 ? _i18N.Get("bank.payLoan.payTowards", new { amt = FormatNumber(amtLoanPay), loan_balance = _bankData.LoanedMoney - _bankData.MoneyPaidBack}) : _i18N.Get("bank.payLoan.paidOff");
                    Game1.exitActiveMenu();
                    Game1.showGlobalMessage(s);
                }

            }
            catch (Exception ex)
            {
                Game1.showGlobalMessage(_i18N.Get("bank.nonNumeric"));
                Monitor.Log($"An error was thrown.\r\n {ex}");
            }
        }
        
        #endregion
        
        
        //Public methods
        public static int CalculateInterest(int val, int interest)
        {
            return (val * interest / 100);
        }
        public static string FormatString(string val, int width)
        {
            int maxWidth = width;//UiWidth - 10;
            string outer = "";

            foreach (string ori in val.Replace("\r\n", "\n").Split('\n'))
            {
                var line = "";
                foreach (var word in ori.Split(' '))
                {
                    if (line == "")
                    {
                        line = word;
                    }
                    else if (Game1.smallFont.MeasureString(line + " " + word).X <= maxWidth)
                    {
                        line += "\n " + word;
                    }
                    else
                    {
                        outer = line;
                        line = word;
                    }
                }

                if (line != "")
                {
                    outer = line;
                }
            }

            return outer;
        }
        
        public static string FormatNumber(int val)
        {
            return $"{val:#,0}";
        }
        
    }
    
    }