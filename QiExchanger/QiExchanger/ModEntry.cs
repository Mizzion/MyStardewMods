using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace QiExchanger
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod
    {
        private ModConfig _config;
        //private int ExchangeRate;
        //private bool interceptedFirstDialogue = false;
        private bool _isDebugging = true;

        private ITranslationHelper _i18N;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this._config = helper.ReadConfig<ModConfig>();
            this._i18N = helper.Translation;
            
            
            //Set Up Events
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }
        //Event Voids
        /// <summary>
        /// Event that gets triggered when we press a button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event arg</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (e.IsDown(_config.ActivationKey))
            {
                DoMenu("main");
            }
            else if (e.IsDown(SButton.F5))
            {
                _config = Helper.ReadConfig<ModConfig>();
                Log("Config was reloaded", LogLevel.Trace, true);
            }
        }

        /// <summary>
        /// Event that gets triggered when a menu is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event Args</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is null)
            {
                return;
            }

            if (e.NewMenu is DialogueBox db && Game1.player.currentLocation.Name.Contains("Club"))
            {
                var str = db.dialogues.ToArray().Aggregate("", (current, d) => current + d);
                if(str.Contains("Buy 100 Qi Coins for 1,000g"))
                {   
                    //Lets close the dialogue
                    db.closeDialogue();
                    DoMenu("main");
                }  
            }
        }
        
        //Custom Voids

        /// <summary>
        /// Process the Dialogue Menus
        /// </summary>
        /// <param name="menuType">The menu type. So we can switch into it.</param>
        private void DoMenu(string menuType)
        {
            var player = Game1.player;
            var hasQiCoins = player.clubCoins > 0;
            switch (menuType)
            {
              case "main":
                  Log("Made it to the DoMenu main switch");
                  var mainResponses = new[]
                  {
                      new Response("Buy", _i18N.Get("main.text.option.one")),
                      new Response("Sell", _i18N.Get("main.text.option.two"))
                  };
                  Game1.currentLocation.createQuestionDialogue(_i18N.Get("main.text", new {player_name = Game1.player.Name}), mainResponses, DoAnswers);
                  break;
              case "Sell":
                  Log("Made it to the DoMenu Sell switch");
                  if (!hasQiCoins)
                  {
                      break;
                  }
                  var sellResponses = new[]
                  {
                      new Response("100", _i18N.Get("option.one")),
                      new Response("1000", _i18N.Get("option.two")),
                      new Response("10000", _i18N.Get("option.three")),
                      new Response("100000", _i18N.Get("option.four")),
                      new Response("1000000", _i18N.Get("option.five"))
                  };
                  Game1.currentLocation.createQuestionDialogue(_i18N.Get("main.exchange.text", new{ player_name = player.Name, qi_amount = player.clubCoins, exchange_rate = _config.ExchangeRate}), sellResponses, DoAnswers);
                  break;
            }
        }

        /// <summary>
        /// Process the Dialogue answers
        /// </summary>
        /// <param name="who">The player</param>
        /// <param name="answer">The answer chosen</param>
        private void DoAnswers(Farmer who, string answer)
        {
            if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu is DialogueBox db &&
                Game1.player.currentLocation.Name.Contains("Club"))
            {
                var str = db.dialogues.ToArray().Aggregate("", (current, d) => current + d);
                Log($"Dialogue was: {str}");
                db.closeDialogue();
            }
            switch (answer)
            {
                case "Buy":
                    Log("Will be the DoAnswer buy menu");
                    break;
                case "Sell":
                    Log("Made it to the DoAnswer sell switch");
                    DoMenu("Sell");
                    break;
                case "100":
                    Log("Made it to the DoAnswer 100 switch");
                    DoExchange(100);
                    break;
                default:
                    Log("Hit the default");
                    break;
            }
        }

        private void DoExchange(int val)
        {
            Log("Made it to the exchange method");
            if (val > 0)
            {
                if (Game1.player.clubCoins >= val)
                {
                    var outer = _config.ExchangeRate > 0 ? (val * _config.ExchangeRate) : 0;
                    Game1.player.clubCoins -= val;
                    Game1.player.Money += Math.Max(0, outer);
                    Game1.drawObjectDialogue(_i18N.Get("do.exchange", new{ qi_coins = val, q_coins = outer}));
                }
                else
                {
                    Game1.drawObjectDialogue(_i18N.Get("not.enough.qicoins"));
                }
            }
            else
            {
                Log("A negative value was pass to the exchange", LogLevel.Trace, true);
            }
        }
        
        /// <summary>
        /// Checks to see if we are in debug mode(Development) and sends logs.
        /// </summary>
        /// <param name="msg">The message to show in the console</param>
        /// <param name="level">The log level. Default is trace</param>
        /// <param name="bypassDebugging">Whether or not we should do the log anyways.</param>
        private void Log(string msg, LogLevel level = LogLevel.Trace, bool bypassDebugging = false)
        {
            if(_isDebugging || bypassDebugging)
                Monitor.Log($"{msg}\r\n", level);
        }

       
    }
}
