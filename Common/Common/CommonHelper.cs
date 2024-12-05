#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyStardewMods.Common.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace MyStardewMods.Common
{
    //I admit I got this from PathosChild, but who doesn't want to use his awesome codes.
    /// <summary>Provides common utility methods for interacting with the game code shared by my various mods.</summary>
    internal static class CommonHelper
    {
        /*********
        ** Fields
        *********/
        /// <summary>A blank pixel which can be colorised and stretched to draw geometric shapes.</summary>
        private static readonly Lazy<Texture2D> LazyPixel = new Lazy<Texture2D>(() =>
        {
            var pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        });


        /// <summary>The width of the borders drawn by <see cref="DrawTab"/>.</summary>
        public const int ButtonBorderWidth = 4 * Game1.pixelZoom;

        /*********
        ** Accessors
        *********/
        /// <summary>A blank pixel which can be colorised and stretched to draw geometric shapes.</summary>
        public static Texture2D Pixel => LazyPixel.Value;

        /// <summary>The width of the horizontal and vertical scroll edges (between the origin position and start of content padding).</summary>
        public static readonly Vector2 ScrollEdgeSize = new Vector2(CommonSprites.Scroll.TopLeft.Width * Game1.pixelZoom, CommonSprites.Scroll.TopLeft.Height * Game1.pixelZoom);

        private static readonly IDictionary<int, int> ResourceUpgradeLevelsNeeded = new Dictionary<int, int>
        {
            [ResourceClump.meteoriteIndex] = Tool.gold,
            [ResourceClump.boulderIndex] = Tool.steel
        };

        /*********
        ** Public methods
        *********/
        /****
        ** Game
        ****/
        /// <summary>Get all game locations.</summary>
        /// <param name="includeTempLevels">Whether to include temporary mine/dungeon locations.</param>
        public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
        {
            var locations = Game1.locations
                .Concat(
                    from location in Game1.locations
                    from indoors in location.GetInstancedBuildingInteriors()
                    select indoors
                );

            if (includeTempLevels)
                locations = locations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels);

            return locations;
        }

        /// <summary>Get a player's current tile position.</summary>
        /// <param name="player">The player to check.</param>
        public static Vector2 GetPlayerTile(Farmer? player)
        {
            Vector2 position = player?.Position ?? Vector2.Zero;
            return new Vector2((int)(position.X / Game1.tileSize), (int)(position.Y / Game1.tileSize)); // note: player.getTileLocationPoint() isn't reliable in many cases, e.g. right after a warp when riding a horse
        }

        /// <summary>Get whether an item ID is non-empty, ignoring placeholder values like "-1".</summary>
        /// <param name="itemId">The unqualified item ID to check.</param>
        /// <param name="allowZero">Whether to allow zero as a valid ID.</param>
        public static bool IsItemId(string itemId, bool allowZero = true)
        {
            return
                !string.IsNullOrWhiteSpace(itemId)
                && (
                    !int.TryParse(itemId, out int id)
                    || id >= (allowZero ? 0 : 1)
                );
        }

        /****
        ** Fonts
        ****/
        /// <summary>Get the dimensions of a space character.</summary>
        /// <param name="font">The font to measure.</param>
        public static float GetSpaceWidth(SpriteFont font)
        {
            return font.MeasureString("A B").X - font.MeasureString("AB").X;
        }

        /****
        ** UI
        ****/
        /// <summary>Draw a pretty hover box for the given text.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="label">The text to display.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The maximum width to display.</param>
        public static Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, in Vector2 position, float wrapWidth)
        {
            const int paddingSize = 27;
            const int gutterSize = 20;

            var labelSize = spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(gutterSize), wrapWidth); // draw text to get wrapped text dimensions
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)position.X, (int)position.Y, (int)labelSize.X + paddingSize + gutterSize, (int)labelSize.Y + paddingSize, Color.White);
            spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(gutterSize), wrapWidth); // draw again over texture box

            return labelSize + new Vector2(paddingSize);
        }

        /// <summary>Draw a tab texture to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="x">The X position at which to draw.</param>
        /// <param name="y">The Y position at which to draw.</param>
        /// <param name="innerWidth">The width of the button's inner content.</param>
        /// <param name="innerHeight">The height of the button's inner content.</param>
        /// <param name="innerDrawPosition">The position at which the content should be drawn.</param>
        /// <param name="align">The button's horizontal alignment relative to <paramref name="x"/>. The possible values are 0 (left), 1 (center), or 2 (right).</param>
        /// <param name="alpha">The button opacity, as a value from 0 (transparent) to 1 (opaque).</param>
        /// <param name="forIcon">Whether the button will contain an icon instead of text.</param>
        /// <param name="drawShadow">Whether to draw a shadow under the tab.</param>
        public static void DrawTab(SpriteBatch spriteBatch, int x, int y, int innerWidth, int innerHeight, out Vector2 innerDrawPosition, int align = 0, float alpha = 1, bool forIcon = false, bool drawShadow = true)
        {
            // calculate outer coordinates
            var outerWidth = innerWidth + CommonHelper.ButtonBorderWidth * 2;
            var outerHeight = innerHeight + Game1.tileSize / 3;
            var offsetX = align switch
            {
                1 => -outerWidth / 2,
                2 => -outerWidth,
                _ => 0
            };

            // calculate inner coordinates
            {
                var iconOffsetX = forIcon ? -Game1.pixelZoom : 0;
                var iconOffsetY = forIcon ? 2 * -Game1.pixelZoom : 0;
                innerDrawPosition = new Vector2(x + CommonHelper.ButtonBorderWidth + offsetX + iconOffsetX, y + CommonHelper.ButtonBorderWidth + iconOffsetY);
            }

            // draw texture
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x + offsetX, y, outerWidth, outerHeight + Game1.tileSize / 16, Color.White * alpha, drawShadow: drawShadow);
        }

        /// <summary>Draw a button background.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="position">The top-left pixel coordinate at which to draw the button.</param>
        /// <param name="contentSize">The button content's pixel size.</param>
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The button's outer bounds.</param>
        /// <param name="padding">The padding between the content and border.</param>
        public static void DrawButton(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 0)
        {
            DrawContentBox(
                spriteBatch: spriteBatch,
                texture: CommonSprites.Button.Sheet,
                background: CommonSprites.Button.Background,
                top: CommonSprites.Button.Top,
                right: CommonSprites.Button.Right,
                bottom: CommonSprites.Button.Bottom,
                left: CommonSprites.Button.Left,
                topLeft: CommonSprites.Button.TopLeft,
                topRight: CommonSprites.Button.TopRight,
                bottomRight: CommonSprites.Button.BottomRight,
                bottomLeft: CommonSprites.Button.BottomLeft,
                position: position,
                contentSize: contentSize,
                contentPos: out contentPos,
                bounds: out bounds,
                padding: padding
            );
        }

        /// <summary>Draw a scroll background.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="position">The top-left pixel coordinate at which to draw the scroll.</param>
        /// <param name="contentSize">The scroll content's pixel size.</param>
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The scroll's outer bounds.</param>
        /// <param name="padding">The padding between the content and border.</param>
        public static void DrawScroll(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 5)
        {
            DrawContentBox(
                spriteBatch: spriteBatch,
                texture: CommonSprites.Scroll.Sheet,
                background: in CommonSprites.Scroll.Background,
                top: CommonSprites.Scroll.Top,
                right: CommonSprites.Scroll.Right,
                bottom: CommonSprites.Scroll.Bottom,
                left: CommonSprites.Scroll.Left,
                topLeft: CommonSprites.Scroll.TopLeft,
                topRight: CommonSprites.Scroll.TopRight,
                bottomRight: CommonSprites.Scroll.BottomRight,
                bottomLeft: CommonSprites.Scroll.BottomLeft,
                position: position,
                contentSize: contentSize,
                contentPos: out contentPos,
                bounds: out bounds,
                padding: padding
            );
        }

        /// <summary>Draw a generic content box like a scroll or button.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="background">The source rectangle for the background.</param>
        /// <param name="top">The source rectangle for the top border.</param>
        /// <param name="right">The source rectangle for the right border.</param>
        /// <param name="bottom">The source rectangle for the bottom border.</param>
        /// <param name="left">The source rectangle for the left border.</param>
        /// <param name="topLeft">The source rectangle for the top-left corner.</param>
        /// <param name="topRight">The source rectangle for the top-right corner.</param>
        /// <param name="bottomRight">The source rectangle for the bottom-right corner.</param>
        /// <param name="bottomLeft">The source rectangle for the bottom-left corner.</param>
        /// <param name="position">The top-left pixel coordinate at which to draw the button.</param>
        /// <param name="contentSize">The button content's pixel size.</param>
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The box's outer bounds.</param>
        /// <param name="padding">The padding between the content and border.</param>
        public static void DrawContentBox(SpriteBatch spriteBatch, Texture2D texture, in Rectangle background, in Rectangle top, in Rectangle right, in Rectangle bottom, in Rectangle left, in Rectangle topLeft, in Rectangle topRight, in Rectangle bottomRight, in Rectangle bottomLeft, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding)
        {
            var cornerWidth = topLeft.Width * Game1.pixelZoom;
            var cornerHeight = topLeft.Height * Game1.pixelZoom;
            var innerWidth = (int)(contentSize.X + padding * 2);
            var innerHeight = (int)(contentSize.Y + padding * 2);
            var outerWidth = innerWidth + cornerWidth * 2;
            var outerHeight = innerHeight + cornerHeight * 2;
            var x = (int)position.X;
            var y = (int)position.Y;

            // draw scroll background
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y + cornerHeight, innerWidth, innerHeight), background, Color.White);

            // draw borders
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y, innerWidth, cornerHeight), top, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y + cornerHeight + innerHeight, innerWidth, cornerHeight), bottom, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x, y + cornerHeight, cornerWidth, innerHeight), left, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight, cornerWidth, innerHeight), right, Color.White);

            // draw corners
            spriteBatch.Draw(texture, new Rectangle(x, y, cornerWidth, cornerHeight), topLeft, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), bottomLeft, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y, cornerWidth, cornerHeight), topRight, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), bottomRight, Color.White);

            // set out params
            contentPos = new Vector2(x + cornerWidth + padding, y + cornerHeight + padding);
            bounds = new Rectangle(x, y, outerWidth, outerHeight);
        }

        /// <summary>Show an informational message to the player.</summary>
        /// <param name="message">The message to show.</param>
        /// <param name="duration">The number of milliseconds during which to keep the message on the screen before it fades (or <c>null</c> for the default time).</param>
        public static void ShowInfoMessage(string message, int? duration = null)
        {
            Game1.addHUDMessage(new HUDMessage(message, 3) { noIcon = true, timeLeft = duration ?? HUDMessage.defaultTime });
        }

        /// <summary>Show an error message to the player.</summary>
        /// <param name="message">The message to show.</param>
        public static void ShowErrorMessage(string message)
        {
            Game1.addHUDMessage(new HUDMessage(message, 3));
        }

        /****
        ** Drawing
        ****/
        /// <summary>Draw a sprite to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="x">The X-position at which to start the line.</param>
        /// <param name="y">The X-position at which to start the line.</param>
        /// <param name="size">The line dimensions.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public static void DrawLine(this SpriteBatch batch, float x, float y, in Vector2 size, in Color? color = null)
        {
            batch.Draw(Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), color ?? Color.White);
        }

        /// <summary>Draw a block of text to the screen with the specified wrap width.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The width at which to wrap the text.</param>
        /// <param name="color">The text color.</param>
        /// <param name="bold">Whether to draw bold text.</param>
        /// <param name="scale">The font scale.</param>
        /// <returns>Returns the text dimensions.</returns>
        public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string text, in Vector2 position, float wrapWidth, in Color? color = null, bool bold = false, float scale = 1)
        {
            if (text == null)
                return new Vector2(0, 0);

            // get word list
            var words = new List<string>();
            foreach (var word in text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // split on newlines
                var wordPart = word;
                int newlineIndex;
                while ((newlineIndex = wordPart.IndexOf(Environment.NewLine, StringComparison.InvariantCulture)) >= 0)
                {
                    if (newlineIndex == 0)
                    {
                        words.Add(Environment.NewLine);
                        wordPart = wordPart.Substring(Environment.NewLine.Length);
                    }
                    else if (newlineIndex > 0)
                    {
                        words.Add(wordPart.Substring(0, newlineIndex));
                        words.Add(Environment.NewLine);
                        wordPart = wordPart.Substring(newlineIndex + Environment.NewLine.Length);
                    }
                }

                // add remaining word (after newline split)
                if (wordPart.Length > 0)
                    words.Add(wordPart);
            }

            // track draw values
            float xOffset = 0;
            float yOffset = 0;
            var lineHeight = font.MeasureString("ABC").Y * scale;
            var spaceWidth = GetSpaceWidth(font) * scale;
            float blockWidth = 0;
            var blockHeight = lineHeight;
            foreach (var word in words)
            {
                // check wrap width
                var wordWidth = font.MeasureString(word).X * scale;
                if (word == Environment.NewLine || ((wordWidth + xOffset) > wrapWidth && (int)xOffset != 0))
                {
                    xOffset = 0;
                    yOffset += lineHeight;
                    blockHeight += lineHeight;
                }
                if (word == Environment.NewLine)
                    continue;

                // draw text
                var wordPosition = new Vector2(position.X + xOffset, position.Y + yOffset);
                if (bold)
                    Utility.drawBoldText(batch, word, font, wordPosition, color ?? Color.Black, scale);
                else
                    batch.DrawString(font, word, wordPosition, color ?? Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

                // update draw values
                if (xOffset + wordWidth > blockWidth)
                    blockWidth = xOffset + wordWidth;
                xOffset += wordWidth + spaceWidth;
            }

            // return text position & dimensions
            return new Vector2(blockWidth, blockHeight);
        }

        /****
        ** Error handling
        ****/
        /// <summary>Intercept errors thrown by the action.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">A callback invoked if an error is intercepted.</param>
        public static void InterceptErrors(this IMonitor monitor, string verb, Action action, Action<Exception> onError = null!)
        {
            monitor.InterceptErrors(verb, null!, action, onError);
        }

        /// <summary>Intercept errors thrown by the action.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="detailedVerb">A more detailed form of <see cref="verb"/> if applicable. This is displayed in the log, so it can be more technical and isn't constrained by the sprite font.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">A callback invoked if an error is intercepted.</param>
        public static void InterceptErrors(this IMonitor monitor, string verb, string detailedVerb, Action action, Action<Exception> onError = null!)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                monitor.InterceptError(ex, verb, detailedVerb);
                onError?.Invoke(ex);
            }
        }

        /// <summary>Log an error and warn the user.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="detailedVerb">A more detailed form of <see cref="verb"/> if applicable. This is displayed in the log, so it can be more technical and isn't constrained by the sprite font.</param>
        public static void InterceptError(this IMonitor monitor, Exception ex, string verb, string detailedVerb = null!)
        {
            detailedVerb = detailedVerb ?? verb;
            monitor.Log($"Something went wrong {detailedVerb}:\n{ex}", LogLevel.Error);
            ShowErrorMessage($"Huh. Something went wrong {verb}. The error log has the technical details.");
        }
        

        public static IEnumerable<GameLocation> GetLocations(IModHelper helper)
        {
            GameLocation[] locs = (Context.IsMainPlayer ? Game1.locations : helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach(GameLocation location in locs.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels))
            {
                yield return location;

                foreach (GameLocation indoors in location.GetInstancedBuildingInteriors())
                    yield return indoors;
            }
        }


        public static void DrawAffectedArea(SpriteBatch batch, IModHelper? Helper, int distance = 0, bool PlayerOrMouse = false)
        {
            var ShowFromMouse = !PlayerOrMouse && Helper != null ? Helper.Input.GetCursorPosition().Tile : Game1.player.Tile;
            foreach(Vector2 tile in GetAffectedTile(ShowFromMouse, distance))
            {
                Rectangle area = new((int)(tile.X * Game1.tileSize - Game1.viewport.X), (int)(tile.Y * Game1.tileSize - Game1.viewport.Y), Game1.tileSize, Game1.tileSize);

                Color color = Color.Green;

                batch.DrawLine(area.X, area.Y, new Vector2(area.Width, area.Height), color * 0.2f);

                int borderSize = 1;
                Color borderColor = color * 0.5f;
                batch.DrawLine(area.X, area.Y, new Vector2(area.Width, borderSize), borderColor); // top
                batch.DrawLine(area.X, area.Y, new Vector2(borderSize, area.Height), borderColor); // left
                batch.DrawLine(area.X + area.Width, area.Y, new Vector2(borderSize, area.Height), borderColor); // right
                batch.DrawLine(area.X, area.Y + area.Height, new Vector2(area.Width, borderSize), borderColor); // bottom

            }
        }

        public static bool TryGetEnricher(GameLocation location, Vector2 tile, [NotNullWhen(true)] out Chest? enricher,
            [NotNullWhen(true)] out Item? fertilizer)
        {
            (Chest enricher, Item fertilizer)? entry = GetEnricher(location, tile);
            fertilizer = entry?.fertilizer;
            enricher = entry?.enricher;
            return entry != null;
        }

        private static (Chest enricher, Item fertilizer)? GetEnricher(GameLocation location, Vector2 tile)
        {
            foreach (SObject sprinkler in location.Objects.Values)
            {
                if (sprinkler.IsSprinkler() &&
                    sprinkler.heldObject.Value is { QualifiedItemId: "(O)913" } enricherObj &&
                    enricherObj.heldObject.Value is Chest enricher && sprinkler.IsInSprinklerRangeBroadphase(tile) &&
                    sprinkler.GetSprinklerTiles().Contains(tile) && enricher.Items.FirstOrDefault() is
                        { Category: SObject.fertilizerCategory } fertilizer)
                {
                    return (enricher, fertilizer);
                }
            }

            return null;
        }

        public static IEnumerable<Vector2> GetAffectedTile(Vector2 origin, int distance)
        {
            for (int x = -distance; x <= distance; x++)
            {
                for (int y = -distance; y <= distance; y++)
                {
                    yield return new Vector2(origin.X + x, origin.Y + y);
                }
            }
        }

        public static void GetRadialAdjacentTile(Vector2 origin, Vector2 tile, out Vector2 adjacent, out int facingDirection)
        {
            facingDirection = Utility.getDirectionFromChange(tile, origin);
            adjacent = facingDirection switch
            {
                Game1.up => new Vector2(tile.X, tile.Y + 1),
                Game1.down => new Vector2(tile.X, tile.Y -1),
                Game1.left => new Vector2(tile.X + 1, tile.Y),
                Game1.right => new Vector2(tile.X - 1, tile.Y),
                _ => tile
            };
        }

        public static Rectangle GetAbsoluteTileArea(Vector2 tile)
        {
            Vector2 position = tile * Game1.tileSize;
            return new Rectangle((int)position.X, (int)position.Y, Game1.tileSize, Game1.tileSize);
        }

        public static bool UseTool(Tool tool, Vector2 tile, Farmer player, GameLocation location)
        {
            UpdateToolBeforeUse(tool, tile, player);
            tool.swingTicker++;
            tool.DoFunction(location, (int)player.lastClick.X, (int)player.lastClick.Y, 0, player);
            return true;
        }

        public static bool UseWeapon(MeleeWeapon weapon, Vector2 tile, Farmer player, GameLocation location)
        {
            bool atk = location.damageMonster(
                areaOfEffect: GetAbsoluteTileArea(tile),
                minDamage: weapon.minDamage.Value,
                maxDamage: weapon.maxDamage.Value,
                isBomb: false,
                knockBackModifier: weapon.knockback.Value,
                addedPrecision: weapon.addedPrecision.Value,
                critChance: weapon.critChance.Value,
                critMultiplier: weapon.critMultiplier.Value,
                triggerMonsterInvincibleTimer: weapon.type.Value != MeleeWeapon.dagger,
                who: player
                );
            if (atk)
                location.playSound(weapon.type.Value == MeleeWeapon.club ? "clubhit" : "daggerswipe", tile);
            return true;
        }

        public static bool CanBreakBoulder(GameLocation location, Vector2 tile, SFarmer player, Tool tool, IModHelper helper,
            [NotNullWhen(true)] out Func<Tool, bool> applyTool)
        {
            return GetResourceClumpOnTile(location, tile, player, helper.Reflection, out ResourceClump? clump,
                       out applyTool) &&
                   (ResourceUpgradeLevelsNeeded.TryGetValue(clump.parentSheetIndex.Value, out int requireUpgradeLevel) ||
                    tool.UpgradeLevel >= requireUpgradeLevel);
        }
        public static bool CheckTileAction(GameLocation location, Vector2 tile, Farmer player)
        {
            return location.checkAction(new Location((int)tile.X, (int)tile.Y), Game1.viewport, player);
        }

        public static void UseItem(Farmer player, Item item) 
        {
            item.Stack -= 1;
            if(item.Stack <= 0)
            {
                player.removeItemFromInventory(item);
            }
        }

        public static void UseItem(Chest chest, Item item)
        {
            item.Stack -= 1;
            if (item.Stack <= 0)
            {
                IInventory inventory = chest.GetItemsForPlayer();
                for (int i = 0; i <= inventory.Count; i++)
                {
                    Item slot = inventory[i];
                    if (slot != null && object.ReferenceEquals(item, slot))
                    {
                        inventory[i] = null;
                        break;
                    }
                }
            }
        }
        public static void CancelAnimation(Farmer player, params int[] animationIds)
        {
            int animationId = player.FarmerSprite.currentSingleAnimation;
            foreach(int id in animationIds)
            {
                if(id == animationId)
                {
                    player.completelyStopAnimatingOrDoingAction();
                    player.forceCanMove();
                    break;
                }
            }
        }

        public static bool UpdateToolBeforeUse(Tool tool, Vector2 tile, Farmer player)
        {
            player.lastClick = GetToolPixelPosition(tile);
            tool.lastUser = player;
            return true;
        }
        
        public static Vector2 GetToolPixelPosition(Vector2 tile)
        {
            return (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
        }


        //Methods to do certain actions on objects/terrain

        #region Resource Clumps

        public static bool IsResourceClumpOnTile(GameLocation location, Vector2 tile, IReflectionHelper reflection)
        {
            return GetResourceClumpOnTile(location, tile, Game1.player, reflection, out _, out _);
        }

        public static bool GetResourceClumpOnTile(GameLocation location, Vector2 tile, Farmer player, IReflectionHelper reflection, [NotNullWhen(true)] out ResourceClump? clump, [NotNullWhen(true)] out Func<Tool, bool>? applyTool) 
        {
            Rectangle tileArea = GetAbsoluteTileArea(tile);

            foreach(ResourceClump cur in location.resourceClumps)
            {
                if (cur.getBoundingBox().Intersects(tileArea))
                {
                    clump = cur;
                    applyTool = tool => UseTool(tool, tile, player, location);
                    return true;
                }
            }

            clump = null;
            applyTool = null;
            return false;
        }
        #endregion

        #region Animals
        public static FarmAnimal? BestHarvestableAnimal(Tool tool, GameLocation location, Vector2 tile)
        {
            Vector2 useAt = GetToolPixelPosition(tile);
            FarmAnimal? animal = Utility.GetBestHarvestableFarmAnimal(
                animals: location.Animals.Values,
                tool: tool,
                toolRect: new Rectangle((int)useAt.X, (int)useAt.Y, Game1.tileSize, Game1.tileSize)
                );

            if (animal == null || !animal.CanGetProduceWithTool(tool) || !CommonHelper.IsItemId(animal.currentProduce.Value, allowZero: false) || animal.isBaby())
                return null;

            return animal;
        }
        #endregion

        public static bool GetHoeDirt(TerrainFeature? tileFeature, SObject? tileObj, [NotNullWhen(true)] out HoeDirt? dirt, out bool isCoveredByObj, out IndoorPot? pot)
        {
            //Is Garden Pot
            if(tileObj is IndoorPot foundPot)
            {
                pot = foundPot;
                dirt = pot.hoeDirt.Value;
                isCoveredByObj = false;
                return true;
            }

            //Dirt Found
            if((dirt = tileFeature as HoeDirt) != null)
            {
                pot = null;
                isCoveredByObj = tileObj != null;
                return true;
            }

            //Nothing was found
            pot = null;
            dirt = null;
            isCoveredByObj = false;
            return false;
        }

        public static bool BreakContainer(Vector2 tile, SObject? tileObj, Farmer player, Tool tool)
        {
            if(tileObj is BreakableContainer)
            {
                UpdateToolBeforeUse(tool, tile, player);
                return tileObj.performToolAction(tool);
            }

            if(tileObj is { TypeDefinitionId: ItemRegistry.type_object, Name: "SupplyCrate"} and not Chest && UpdateToolBeforeUse(tool, tile, player) && tileObj.performToolAction(tool))
            {
                tileObj.performRemoveAction();
                Game1.currentLocation.Objects.Remove(tile);
                return true;
            }

            return false;
        }


        public static bool ClearDeadCrop(GameLocation location, Vector2 tile, TerrainFeature? tileFeature, Farmer player)
        {
            return tileFeature is HoeDirt { crop: not null } dirt && dirt.crop.dead.Value && UseTool(new Pickaxe(), tile, player, location);
        }

        public static bool HarvestGrass(Grass? grass, GameLocation location, Vector2 tile, Farmer player, Tool tool)
        {
            if (grass == null || !location.terrainFeatures.ContainsKey(tile))
                return false;

            grass.numberOfWeeds.Value = 0;
            grass.TryDropItemsOnCut(tool);
            location.terrainFeatures.Remove(tile);
            return true;
        }

        /*
        public static bool TryStartCooldown(string key, TimeSpan delay)
        {
            long currentTime = (long)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            if (!this.CooldownStartTimes.TryGetValue(key, out long startTime) || (currentTime - startTime) >= delay.TotalMilliseconds)
            {
                this.CooldownStartTimes[key] = currentTime;
                return true;
            }

            return false;
        }*/
        
        public static string FormatNumber(int val)
        {
            //return $"{val:#,0}";
            return val.ToString("N0");
        }

        public static string PluralOrNot(int amt)
        {
            return amt > 1 ? "'s" : "";
        }

        public static void DoHud(string msg)
        {
            Game1.addHUDMessage(new HUDMessage(msg));
        }
    }
}
