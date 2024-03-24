using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mizzion.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
<<<<<<< Updated upstream
using SFarmer = StardewValley.Farmer;
=======
using StardewValley.Menus;
using Object = StardewValley.Object;
>>>>>>> Stashed changes

namespace EnhancedRelationships
{
    internal class EnhancedRelationships : Mod//, IAssetEditor
    {
        private ModConfig _config;

        private ITranslationHelper _i18N;
        private readonly List<NPC> _birthdayMessageQueue = new();
        private readonly IDictionary<string, int> _gaveNpcGift = new Dictionary<string, int>();
        private IDictionary<string, string> _npcGifts = new Dictionary<string, string>();
        private bool _debugging;

        public override void Entry(IModHelper helper)
        {
<<<<<<< Updated upstream
            _config = Helper.ReadConfig<ModConfig>();            
            Helper.Events.GameLoop.DayStarted += TimeEvents_AfterDayStarted;
            Helper.Events.GameLoop.Saving += SaveEvents_BeforeSave;
            Helper.Events.Player.InventoryChanged += DoNpcGift;
=======
            _config = Helper.ReadConfig<ModConfig>();
            _i18N = Helper.Translation;
           
            //GameLoop Events
            Helper.Events.GameLoop.DayStarted += DayStarted;
            Helper.Events.GameLoop.Saving += Saving;
            Helper.Events.GameLoop.GameLaunched += GameLaunched;
>>>>>>> Stashed changes

            //Lets add in the new content events
            Helper.Events.Content.AssetRequested += ContentEvents_AssetRequested;
        }

        private void DoNpcGift(object sender, InventoryChangedEventArgs e)
        {
<<<<<<< Updated upstream
            var player = Game1.player;
            var day = SDate.Now();
            foreach(var location in Game1.locations)
            {
                foreach(var npc in location.characters)
                {
                    if (!_gaveNpcGift.ContainsKey(npc.Name))
                    {
                        if (player.friendshipData.ContainsKey(npc.Name))
                        {
                            _gaveNpcGift.Add(npc.Name, player.friendshipData[npc.Name].GiftsToday);
                        }                        
                    }
                    else
                    {
                        if (npc.isBirthday(day.Season, day.Day))
                        {
                            _gaveNpcGift[npc.Name] = player.friendshipData[npc.Name].GiftsToday;
                        }
                    }                    
                }
            }
=======
            #region Generic Mod Config Menu
            /*
            _cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_cfgMenu is null) return;

            //Register mod
            _cfgMenu.Register(
                mod: ModManifest,
                reset: () => _config = new ModConfig(),
                save: () => Helper.WriteConfig(_config)
                );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Enhanced Relationships Settings",
                tooltip: null
                );
            
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.GetMail,
                setValue: value => _config.GetMail = value,
                name: () => "Get Birthday Mail",
                tooltip: () => "Should you get mail letting you know what the npc loves."
            );

            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Gifts to Keep NPC Happy",
                tooltip: () => "How many weekly gifts are needed to keep NPCs happy.",
                getValue: () => _config.AmtOfGiftsToKeepNpcHappy,
                setValue: value => _config.AmtOfGiftsToKeepNpcHappy = value
            );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableMissedBirthdays,
                setValue: value => _config.EnableMissedBirthdays = value,
                name: () => "Enable Missed Birthdays",
                tooltip: () => "Enable if you want to be punished for missing a birthday gift."
            );
            
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableRounded,
                setValue: value => _config.EnableRounded = value,
                name: () => "Enable Rounded Amounts.",
                tooltip: () => "Enable if you want your friend points to be rounded to the nearest whole number."
            );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Loan Settings",
                tooltip: null
                );

            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Basic Amount",
                tooltip: () => "The basic amount of points to give.",
                getValue: () => _config.BasicAmount,
                setValue: value => _config.BasicAmount = value
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
>>>>>>> Stashed changes
        }
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            if (Game1.player.Name.ToLower().Contains("debug"))
            {
                _debugging = true;
            }
            try
            {
                //Birthday tests was in AfterDay Started
                _birthdayMessageQueue.Clear();
                var today = SDate.Now();
<<<<<<< Updated upstream
                if (today.DaysSinceStart != 1)
=======
                var yesterday = today.DaysSinceStart < 2 ? today : today.AddDays(-1);
                var chars = Utility.getAllCharacters();

                if (today.DaysSinceStart < 2)
                    return;
                
                foreach (var character in chars)
>>>>>>> Stashed changes
                {
                    var yesterday = SDate.Now().AddDays(-1);
                    foreach (var location in Game1.locations)
                    {
                        foreach (var characters in location.characters)
                            DoLogic(characters, yesterday.Day, yesterday.Season);
                    }

<<<<<<< Updated upstream
                    foreach (var birthdayMessage in _birthdayMessageQueue)
                        birthdayMessage.CurrentDialogue.Push(new Dialogue(PickRandomDialogue(), birthdayMessage));
=======
                foreach (var npc in _birthdayMessageQueue)
                {
                    var randomText = PickRandomDialogue();
                    npc.CurrentDialogue.Push(new Dialogue(npc,null, randomText));
                    
                    if(Debugging)
                        Monitor.Log($"Message should be added for NPC: {npc.Name}. Message was: {randomText}");
>>>>>>> Stashed changes
                }
            }
            catch (Exception ex)
            {
                Monitor.Log(ex.ToString());
            }
           
        }
<<<<<<< Updated upstream
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
=======


        private void Saving(object sender, SavingEventArgs e)
        {
            if (!_config.GetMail)
                return;

            try
            {
                var tomorrow = SDate.Now().AddDays(1);
                var locations = DataLoader.Locations(Game1.content);
                var npcs = Utility.getAllCharacters();

                foreach (var npc in npcs.Where(npc => npc.Birthday_Season == tomorrow.SeasonKey && npc.Birthday_Day == tomorrow.Day))
                {
                    Game1.mailbox.Add($"birthdayMail{npc.Name}");
                    
                    if(Debugging)
                        Monitor.Log($"Added Mail for NPC:{npc.Name}");
                }
                
                //Process NPC and check for gifts
                foreach (var npc in npcs)
                {
                    if (_gaveNpcGift.ContainsKey(npc.Name) && _gaveNpcGift[npc.Name] != 1 && Game1.player.friendshipData.ContainsKey(npc.Name))
                    {
                        _gaveNpcGift[npc.Name] = Game1.player.friendshipData[npc.Name].GiftsToday;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log(ex.ToString());
            }
        }

        private void InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            var player = Game1.player;
            var locations = DataLoader.Locations(Game1.content);
            var npcs = Utility.getAllCharacters();

            foreach (var npc in npcs)
            {
                if (!_gaveNpcGift.ContainsKey(npc.Name))
                {
                    if (player.friendshipData.ContainsKey(npc.Name) && npc.isBirthday())
                    {
                        _gaveNpcGift.Add(npc.Name, player.friendshipData[npc.Name].GiftsToday);
                    }
                }
                else
                {
                    if (npc.isBirthday())
                    {
                        _gaveNpcGift[npc.Name] = player.friendshipData[npc.Name].GiftsToday;
                    }
                }
            }
        }

        private void AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit(asset =>
                {
                    _npcGifts = GetNpcGifts();

                    if (!_npcGifts.Any())
                        return;

                    foreach (var gift in _npcGifts)
                    {
                        var npc = asset.AsDictionary<string, string>().Data;
                        npc[$"birthdayMail{gift.Key}"] =
                            _i18N.Get("npc_mail", new { npc_name = gift.Key, npc_gift = gift.Value });
                    }
                });
            }
        }
        
        //Custom Voids
        private string GetSeason(Season season)
>>>>>>> Stashed changes
        {
            if (!_config.GetMail)
                return;
            try
            {
                var tomorrow = SDate.Now().AddDays(1);
                foreach (var location in Game1.locations)
                {
                    foreach (var npc in location.characters)
                    {
                        if (npc.isBirthday(tomorrow.Season, tomorrow.Day))
                        {
                            Game1.mailbox.Add($"birthDayMail{npc.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log(ex.ToString());
            }
                      
        }
<<<<<<< Updated upstream
=======

        
        
>>>>>>> Stashed changes
        private void DoLogic(NPC npc, int day, string season)
        {
            var player = Game1.player;
            var giftGiven = false;
<<<<<<< Updated upstream
            if (!player.friendshipData.ContainsKey(npc.Name) || player.friendshipData[npc.Name].Points <= 0)
                return;
            var index = player.getFriendshipHeartLevelForNPC(npc.Name);
            var basicAmount = _config.BasicAmount;
            index = index > 10 ? 10 : index;
            
            
            //Check to see if gift was given
            if (_gaveNpcGift.ContainsKey(npc.Name))
            {
                var giftInt = _gaveNpcGift[npc.Name];
                giftGiven = giftInt == 1;
            }
            //End check
            if (_config.EnableMissedBirthdays && npc.isBirthday(season, day) && giftGiven == false)
            {
                var amount = !_config.EnableRounded ? (int)Math.Floor(basicAmount * (double)_config.BirthdayMultiplier * _config.BirthdayHeartMultiplier[index] + basicAmount * (double)_config.HeartMultiplier[index]) : (int)Math.Ceiling(basicAmount * (double)_config.BirthdayMultiplier * _config.BirthdayHeartMultiplier[index] + basicAmount * (double)_config.HeartMultiplier[index]);
                amount *= -1;
                Game1.player.changeFriendship(amount, npc);
                _birthdayMessageQueue.Add(npc);
                if (_debugging)
                {
                    Monitor.Log($"Message should have been added for {npc.Name}");
                }

                _gaveNpcGift.Remove(npc.Name);
                Monitor.Log($"Decreased Friendship {amount} With : {npc.Name}");
            }
            else
            {
                if (player.friendshipData[npc.Name].GiftsThisWeek >= _config.AmtOfGiftsToKeepNpcHappy)
                    return;
                var amount = !_config.EnableRounded ? (int)Math.Floor(basicAmount * (double)_config.HeartMultiplier[index]) : (int)Math.Ceiling(basicAmount * (double)_config.HeartMultiplier[index]);
                player.changeFriendship(amount, npc);
                if (_debugging)
                {
                    Monitor.Log($"Increased Friendship {amount} With : {npc.Name} Index: {index}");
                }
                
=======

            if (!player.friendshipData.ContainsKey(npc.Name) || player.friendshipData[npc.Name].Points < 0)
                return;

            var index = player.getFriendshipLevelForNPC(npc.Name) > 10 ? 10 : player.getFriendshipLevelForNPC(npc.Name);
            
            if (_gaveNpcGift.TryGetValue(npc.Name, out var giftInt))
                giftGiven = giftInt == 1;

            if (Debugging)
            {
                Monitor.Log($"giftInt: {giftInt} giftGiven: {giftGiven} \r\n for NPC: {npc.Name} NPCBirthdaySeason: {npc.Birthday_Season} NPCBirthdayDay: {npc.Birthday_Day}");
            }

            if (_config.EnableMissedBirthdays && npc.Birthday_Season == season.ToLower() && npc.Birthday_Day == day &&
                giftGiven == false)
            {
                var newFriendship = GetAmount(true, index);
                ModifyFriendship(player, npc, newFriendship, true);
                
                _birthdayMessageQueue.Add(npc);
                
                if(Debugging)
                    Monitor.Log($"Message should have been added for NPC: {npc.Name}");
            }
            else
            {
                if(Debugging)
                    Monitor.Log($"Reached MissedBirthdays Else for NPC: {npc.Name}");
                
                if (player.friendshipData[npc.Name].GiftsThisWeek < _config.AmtOfGiftsToKeepNpcHappy)
                    return;
                
                ModifyFriendship(player, npc, GetAmount(false, index));
            }
        }

        private string PickRandomDialogue()
        {
            var rnd = new Random();
            var pickedDialogue = rnd.Next(0, 7);
            return _i18N.Get($"npc_dialogue{pickedDialogue}", new { player_name = Game1.player.Name });
        }

        private IDictionary<string, string> GetNpcGifts(bool loved = false)
        {
            //var giftTastes = DataLoader.NpcGiftTastes(Game1.content);
            var giftTastes = Game1.NPCGiftTastes;

            IDictionary<string, string> results = new Dictionary<string, string>();
            var giftNames = "";

            foreach (var tastes in giftTastes.Where(tastes => tastes.Value.Contains('/')))
            {
                foreach (var n in tastes.Value.Split('/')[1].Split(' '))
                {
                    if (n.Length > 0)
                    {
                        Object obj = new Object(n, 1);
                        if (obj.DisplayName != "Error Item") giftNames = giftNames + $"{obj.DisplayName}, ";
                    }
                }

                if (giftNames.Contains(", "))
                    results.Add(tastes.Key, giftNames.Substring(0, giftNames.Length - 2));
                giftNames = "";
>>>>>>> Stashed changes
            }
        }

        private void ContentEvents_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit(asset =>
                {
                    _npcGifts = GetNpcGifts();
                    var i18N = Helper.Translation;
                    foreach (var d in _npcGifts)
                    {
                        var npc = asset.AsDictionary<string, string>().Data;
                        npc["birthDayMail" + d.Key] =
                            i18N.Get("npc_mail", new { npc_name = d.Key, npc_gift = d.Value });
                    }
                });
            }
        }
        
        private string PickRandomDialogue()
        {
            var i18N = Helper.Translation;
            var rnd = new Random();
            var outer = rnd.Next(0, 7);
            //Return the translated Text    
            return i18N.Get("npc_dialogue"+outer, new { player_name = Game1.player.Name });           
        }
        //Mail Updates
        
        //Grab NPC Gifts Loves
        private IDictionary<string, string> GetNpcGifts(bool loved = true)
        {
            if (loved)
            {
                if (_debugging)
                    Monitor.Log($"Loved was true");
            }
            
            var outer = Game1.NPCGiftTastes;
            IDictionary<string, string> results = new Dictionary<string, string>();
            var giftNames = "";
            foreach (var o in outer)
            {
                if (o.Value.Contains('/'))
                {
                    giftNames = (from n in o.Value.Split('/')[1].Split(' ') where n.Length > 0 select new StardewValley.Object(Convert.ToInt32(n), 1) into obj where obj.DisplayName != "Error Item" select obj).Aggregate(giftNames, (current, obj) => current + $"{obj.DisplayName}, ");
                    if(giftNames.Contains(", "))
                        results.Add(o.Key, giftNames.Substring(0, giftNames.Length - 2));
                    giftNames = "";
                }
            }
            return results;
        }

        private int GetAmount(bool negative = false, int index = 0)
        {
            index = index > 10 ? 10 : index;
            
            return !_config.EnableRounded ? 
                (int)Math.Floor(_config.BasicAmount * (double)_config.BirthdayMultiplier * _config.BirthdayHeartMultiplier[index] + _config.BasicAmount * (double)_config.HeartMultiplier[index]) : 
                (int)Math.Ceiling(_config.BasicAmount * (double)_config.BirthdayMultiplier * _config.BirthdayHeartMultiplier[index] + _config.BasicAmount * (double)_config.HeartMultiplier[index]);
        }

        private void ModifyFriendship(Farmer player, NPC npc, int amount, bool negative = false)
        {
            if (player is null || npc is null)
                return;

            var amt = negative ? amount * -1 : amount;
            
            player.changeFriendship(amt, npc);
            
            var typeOfChange = negative ? "Decreased" : "Increased";
            Monitor.Log($"{typeOfChange} friendship for {npc.Name} by {amt} points");
        }
    
    }
}
