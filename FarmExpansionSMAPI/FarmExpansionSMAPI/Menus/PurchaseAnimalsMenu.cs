using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using xTile.Dimensions;

namespace FarmExpansionSMAPI.Menus
{
    public class PurchaseAnimalsMenu : IClickableMenu
    {
        GameLocation previousLocation = Game1.currentLocation;

        Farm currentFarm = Game1.getFarm();

        private string[] farmName = new string[2] { "Farm", "Expansion" };

        private int currentFarmIndex;

        public static int menuHeight = Game1.tileSize * 4;

        public static int menuWidth = Game1.tileSize * 6;

        private List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();

        private ClickableTextureComponent backButton;

        private ClickableTextureComponent forwardButton;

        private ClickableTextureComponent okButton;

        private ClickableTextureComponent doneNamingButton;

        private ClickableTextureComponent randomButton;

        private ClickableTextureComponent hovered;

        private bool onFarm;

        private bool namingAnimal;

        private bool freeze;

        private FarmAnimal animalBeingPurchased;

        private TextBox textBox;

        private TextBoxEvent e;

        private Building newAnimalHome;

        private int priceOfAnimal;

        public PurchaseAnimalsMenu() : base(Game1.viewport.Width / 2 - menuWidth / 2 - borderWidth * 2, Game1.viewport.Height / 2 - menuHeight - borderWidth * 2, menuWidth + borderWidth * 2, menuHeight + borderWidth, false)
        {
            height += Game1.tileSize * 2;
            Game1.player.forceCanMove();
            List<StardewValley.Object> stock = getPurchaseAnimalStock();
            for (int i = 0; i < stock.Count(); i++)
            {
                animalsToPurchase.Add(new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + borderWidth + i % 3 * Game1.tileSize * 2, yPositionOnScreen + spaceToClearTopBorder + borderWidth / 2 + i / 3 * (Game1.tileSize + Game1.tileSize / 3), Game1.tileSize * 2, Game1.tileSize), string.Concat(stock[i].salePrice()), stock[i].Name, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16), 4f, false, stock[i].type == null)
                {
                    item = stock[i]
                });
            }
            okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - Game1.tileSize - borderWidth, Game1.tileSize, Game1.tileSize), "", "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f);
            randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + Game1.tileSize * 4 / 5 + Game1.tileSize, Game1.viewport.Height / 2, Game1.tileSize, Game1.tileSize), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), Game1.pixelZoom);
            backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + Game1.tileSize / 4, yPositionOnScreen, Game1.tileSize, Game1.tileSize), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), Game1.pixelZoom);
            forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + SpriteText.getWidthOfString("Expansion") * 2 - Game1.tileSize / 2, yPositionOnScreen, Game1.tileSize, Game1.tileSize), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), Game1.pixelZoom);

            textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
            textBox.X = Game1.viewport.Width / 2 - Game1.tileSize * 3;
            textBox.Y = Game1.viewport.Height / 2;
            textBox.Width = Game1.tileSize * 4;
            textBox.Height = Game1.tileSize * 3;
            e = new TextBoxEvent(textBoxEnter);
            randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + Game1.tileSize + Game1.tileSize * 3 / 4 - Game1.pixelZoom * 2, Game1.viewport.Height / 2 + Game1.pixelZoom, Game1.tileSize, Game1.tileSize), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), Game1.pixelZoom);
            doneNamingButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + Game1.tileSize / 2 + Game1.pixelZoom, Game1.viewport.Height / 2 - Game1.pixelZoom * 2, Game1.tileSize, Game1.tileSize), "", "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f);
        }

        private void populateAnimalStock()
        {
            animalsToPurchase.Clear();
            currentFarm = (currentFarmIndex == 0) ? Game1.getFarm() : FarmExpansionSMAPI.getFarmExtension();
            List<StardewValley.Object> stock = getPurchaseAnimalStock();
            for (int i = 0; i < stock.Count(); i++)
            {
                animalsToPurchase.Add(new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + borderWidth + i % 3 * Game1.tileSize * 2, yPositionOnScreen + spaceToClearTopBorder + borderWidth / 2 + i / 3 * (Game1.tileSize + Game1.tileSize / 3), Game1.tileSize * 2, Game1.tileSize), string.Concat(stock[i].salePrice()), stock[i].Name, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16), 4f, false, stock[i].type == null)
                {
                    item = stock[i]
                });
            }
        }

        private List<StardewValley.Object> getPurchaseAnimalStock()
        {
            List<StardewValley.Object> list = new List<StardewValley.Object>();
            StardewValley.Object item = new StardewValley.Object(100, 1, false, 400, 0)
            {
                name = "Chicken",
                type = ((currentFarm.isBuildingConstructed("Coop") || currentFarm.isBuildingConstructed("Deluxe Coop") || currentFarm.isBuildingConstructed("Big Coop")) ? null : "Requires construction of a Coop")
            };
            list.Add(item);
            item = new StardewValley.Object(100, 1, false, 750, 0)
            {
                name = "Dairy Cow",
                type = ((currentFarm.isBuildingConstructed("Barn") || currentFarm.isBuildingConstructed("Deluxe Barn") || currentFarm.isBuildingConstructed("Big Barn")) ? null : "Requires construction of a Barn")
            };
            list.Add(item);
            item = new StardewValley.Object(100, 1, false, 2000, 0)
            {
                name = "Goat",
                type = ((currentFarm.isBuildingConstructed("Big Barn") || currentFarm.isBuildingConstructed("Deluxe Barn")) ? null : "Requires construction of a Big Barn")
            };
            list.Add(item);
            item = new StardewValley.Object(100, 1, false, 2000, 0)
            {
                name = "Duck",
                type = ((currentFarm.isBuildingConstructed("Big Coop") || currentFarm.isBuildingConstructed("Deluxe Coop")) ? null : "Requires construction of a Big Coop")
            };
            list.Add(item);
            item = new StardewValley.Object(100, 1, false, 4000, 0)
            {
                name = "Sheep",
                type = (currentFarm.isBuildingConstructed("Deluxe Barn") ? null : "Requires construction of a Deluxe Barn")
            };
            list.Add(item);
            item = new StardewValley.Object(100, 1, false, 4000, 0)
            {
                name = "Rabbit",
                type = (currentFarm.isBuildingConstructed("Deluxe Coop") ? null : "Requires construction of a Deluxe Coop")
            };
            list.Add(item);
            item = new StardewValley.Object(100, 1, false, 8000, 0)
            {
                name = "Pig",
                type = (currentFarm.isBuildingConstructed("Deluxe Barn") ? null : "Requires construction of a Deluxe Barn")
            };
            list.Add(item);
            return list;
        }

        public void textBoxEnter(TextBox sender)
        {
            if (!namingAnimal)
            {
                return;
            }
            if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is PurchaseAnimalsMenu))
            {
                textBox.OnEnterPressed -= e;
                return;
            }
            if (sender.Text.Count() >= 1)
            {
                if (Utility.areThereAnyOtherAnimalsWithThisName(sender.Text))
                {
                    Game1.showRedMessage("Name Unavailable");
                    return;
                }
                textBox.OnEnterPressed -= e;
                animalBeingPurchased.name = sender.Text;
                animalBeingPurchased.home = newAnimalHome;
                animalBeingPurchased.homeLocation = new Vector2(newAnimalHome.tileX, newAnimalHome.tileY);
                animalBeingPurchased.setRandomPosition(animalBeingPurchased.home.indoors);
                (newAnimalHome.indoors as AnimalHouse).animals.Add(animalBeingPurchased.myID, animalBeingPurchased);
                (newAnimalHome.indoors as AnimalHouse).animalsThatLiveHere.Add(animalBeingPurchased.myID);
                newAnimalHome = null;
                namingAnimal = false;
                Game1.player.money -= priceOfAnimal;
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(setUpForReturnAfterPurchasingAnimal), 0.02f);
            }
        }

        public void setUpForReturnAfterPurchasingAnimal()
        {
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation = previousLocation;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(null, 0.02f);
            onFarm = false;
            okButton.bounds.X = xPositionOnScreen + width + 4;
            Game1.displayHUD = true;
            Game1.displayFarmer = true;
            freeze = false;
            textBox.OnEnterPressed -= e;
            textBox.Selected = false;
            Game1.viewportFreeze = false;
            Game1.globalFadeToClear(new Game1.afterFadeFunction(marnieAnimalPurchaseMessage), 0.02f);
        }

        public void marnieAnimalPurchaseMessage()
        {
            exitThisMenu(true);
            Game1.player.forceCanMove();
            freeze = false;
            Game1.drawDialogue(Game1.getCharacterFromName("Marnie"), string.Concat(new string[]
            {
                "Great! I'll send little ",
                animalBeingPurchased.name,
                " to ",
                animalBeingPurchased.isMale() ? "his" : "her",
                " new home right away."
            }));
        }

        public void setUpForAnimalPlacement()
        {
            Game1.displayFarmer = false;
            Game1.currentLocation = currentFarm;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(null, 0.02f);
            onFarm = true;
            freeze = false;
            okButton.bounds.X = Game1.viewport.Width - Game1.tileSize * 2;
            okButton.bounds.Y = Game1.viewport.Height - Game1.tileSize * 2;
            Game1.displayHUD = false;
            Game1.viewportFreeze = true;
            Game1.viewport.Location = new Location(49 * Game1.tileSize, 5 * Game1.tileSize);
            Game1.panScreen(0, 0);
        }

        public void setUpForReturnToShopMenu()
        {
            freeze = false;
            Game1.displayFarmer = true;
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation = previousLocation;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(null, 0.02f);
            onFarm = false;
            okButton.bounds.X = xPositionOnScreen + width + 4;
            okButton.bounds.Y = yPositionOnScreen + height - Game1.tileSize - borderWidth;
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            namingAnimal = false;
            textBox.OnEnterPressed -= e;
            textBox.Selected = false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.globalFade || freeze)
            {
                return;
            }
            if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
            {
                if (onFarm)
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(setUpForReturnToShopMenu), 0.02f);
                    Game1.playSound("smallSelect");
                }
                else
                {
                    Game1.exitActiveMenu();
                    Game1.playSound("bigDeSelect");
                }
            }
            if (onFarm)
            {
                Vector2 tile = new Vector2((x + Game1.viewport.X) / Game1.tileSize, (y + Game1.viewport.Y) / Game1.tileSize);
                Building buildingAt = currentFarm.getBuildingAt(tile);
                if (buildingAt != null && !namingAnimal)
                {
                    if (buildingAt.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn))
                    {
                        if ((buildingAt.indoors as AnimalHouse).isFull())
                        {
                            Game1.showRedMessage("That Building Is Full");
                        }
                        else if (animalBeingPurchased.harvestType != 2)
                        {
                            namingAnimal = true;
                            newAnimalHome = buildingAt;
                            if (animalBeingPurchased.sound != null && Game1.soundBank != null)
                            {
                                Cue cue = Game1.soundBank.GetCue(animalBeingPurchased.sound);
                                cue.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
                                cue.Play();
                            }
                            textBox.OnEnterPressed += e;
                            Game1.keyboardDispatcher.Subscriber = textBox;
                            textBox.Text = animalBeingPurchased.name;
                            textBox.Selected = true;
                        }
                        else if (Game1.player.money >= priceOfAnimal)
                        {
                            newAnimalHome = buildingAt;
                            animalBeingPurchased.home = newAnimalHome;
                            animalBeingPurchased.homeLocation = new Vector2(newAnimalHome.tileX, newAnimalHome.tileY);
                            animalBeingPurchased.setRandomPosition(animalBeingPurchased.home.indoors);
                            (newAnimalHome.indoors as AnimalHouse).animals.Add(animalBeingPurchased.myID, animalBeingPurchased);
                            (newAnimalHome.indoors as AnimalHouse).animalsThatLiveHere.Add(animalBeingPurchased.myID);
                            newAnimalHome = null;
                            namingAnimal = false;
                            if (animalBeingPurchased.sound != null && Game1.soundBank != null)
                            {
                                Cue cue2 = Game1.soundBank.GetCue(animalBeingPurchased.sound);
                                cue2.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
                                cue2.Play();
                            }
                            Game1.player.money -= priceOfAnimal;
                            Game1.addHUDMessage(new HUDMessage("Purchased " + animalBeingPurchased.type, Color.LimeGreen, 3500f));
                            animalBeingPurchased = new FarmAnimal(animalBeingPurchased.type, MultiplayerUtility.getNewID(), Game1.player.uniqueMultiplayerID);
                        }
                        else if (Game1.player.money < priceOfAnimal)
                        {
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        }
                    }
                    else
                    {
                        Game1.showRedMessage(animalBeingPurchased.type.Split(new char[]
                        {
                            ' '
                        }).Last() + "s Can't Live There.");
                    }
                }
                if (namingAnimal && doneNamingButton.containsPoint(x, y))
                {
                    textBoxEnter(textBox);
                    Game1.playSound("smallSelect");
                    return;
                }
                if (namingAnimal && randomButton.containsPoint(x, y))
                {
                    animalBeingPurchased.name = Dialogue.randomName();
                    textBox.Text = animalBeingPurchased.name;
                    randomButton.scale = randomButton.baseScale;
                    Game1.playSound("drumkit6");
                    return;
                }
            }
            else
            {
                if (backButton.containsPoint(x, y))
                {
                    currentFarmIndex = (currentFarmIndex == 0) ? 1 : 0;
                    populateAnimalStock();
                    backButton.scale = backButton.baseScale;
                }
                if (forwardButton.containsPoint(x, y))
                {
                    currentFarmIndex = (currentFarmIndex == 0) ? 1 : 0;
                    populateAnimalStock();
                    forwardButton.scale = forwardButton.baseScale;
                }
                foreach (ClickableTextureComponent current in animalsToPurchase)
                {
                    if (current.containsPoint(x, y) && (current.item as StardewValley.Object).type == null)
                    {
                        int num = Convert.ToInt32(current.name);
                        if (Game1.player.money >= num)
                        {
                            Game1.globalFadeToBlack(new Game1.afterFadeFunction(setUpForAnimalPlacement), 0.02f);
                            Game1.playSound("smallSelect");
                            onFarm = true;
                            animalBeingPurchased = new FarmAnimal(current.hoverText, MultiplayerUtility.getNewID(), Game1.player.uniqueMultiplayerID);
                            priceOfAnimal = num;
                        }
                        else
                        {
                            Game1.addHUDMessage(new HUDMessage("Not Enough Money", Color.Red, 3500f));
                        }
                    }
                }
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (Game1.globalFade || freeze)
            {
                return;
            }
            if (!Game1.globalFade && onFarm)
            {
                if (!namingAnimal)
                {
                    if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
                    {
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(setUpForReturnToShopMenu), 0.02f);
                        return;
                    }
                    if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                    {
                        Game1.panScreen(0, 4);
                        return;
                    }
                    if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                    {
                        Game1.panScreen(4, 0);
                        return;
                    }
                    if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                    {
                        Game1.panScreen(0, -4);
                        return;
                    }
                    if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                    {
                        Game1.panScreen(-4, 0);
                        return;
                    }
                }
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.globalFade && readyToClose())
            {
                Game1.player.forceCanMove();
                Game1.exitActiveMenu();
                Game1.playSound("bigDeSelect");
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (onFarm && !namingAnimal)
            {
                int num = Game1.getOldMouseX() + Game1.viewport.X;
                int num2 = Game1.getOldMouseY() + Game1.viewport.Y;
                if (num - Game1.viewport.X < Game1.tileSize)
                {
                    Game1.panScreen(-8, 0);
                }
                else if (num - (Game1.viewport.X + Game1.viewport.Width) >= -Game1.tileSize)
                {
                    Game1.panScreen(8, 0);
                }
                if (num2 - Game1.viewport.Y < Game1.tileSize)
                {
                    Game1.panScreen(0, -8);
                }
                else if (num2 - (Game1.viewport.Y + Game1.viewport.Height) >= -Game1.tileSize)
                {
                    Game1.panScreen(0, 8);
                }
                Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    Keys key = pressedKeys[i];
                    receiveKeyPress(key);
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            hovered = null;
            if (Game1.globalFade || freeze)
            {
                return;
            }
            if (okButton != null)
            {
                if (okButton.containsPoint(x, y))
                {
                    okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
                }
                else
                {
                    okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
                }
            }
            if (onFarm)
            {
                Vector2 tile = new Vector2((x + Game1.viewport.X) / Game1.tileSize, (y + Game1.viewport.Y) / Game1.tileSize);
                foreach (Building current in currentFarm.buildings)
                {
                    current.color = Color.White;
                }
                Building buildingAt = currentFarm.getBuildingAt(tile);
                if (buildingAt != null)
                {
                    if (buildingAt.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn) && !(buildingAt.indoors as AnimalHouse).isFull())
                    {
                        buildingAt.color = Color.LightGreen * 0.8f;
                    }
                    else
                    {
                        buildingAt.color = Color.Red * 0.8f;
                    }
                }
                if (doneNamingButton != null)
                {
                    if (doneNamingButton.containsPoint(x, y))
                    {
                        doneNamingButton.scale = Math.Min(1.1f, doneNamingButton.scale + 0.05f);
                    }
                    else
                    {
                        doneNamingButton.scale = Math.Max(1f, doneNamingButton.scale - 0.05f);
                    }
                }
                randomButton.tryHover(x, y, 0.5f);
                return;
            }
            backButton.tryHover(x, y, 1f);
            forwardButton.tryHover(x, y, 1f);
            foreach (ClickableTextureComponent current2 in animalsToPurchase)
            {
                if (current2.containsPoint(x, y))
                {
                    current2.scale = Math.Min(current2.scale + 0.05f, 4.1f);
                    hovered = current2;
                }
                else
                {
                    current2.scale = Math.Max(4f, current2.scale - 0.025f);
                }
            }
        }

        public static string getAnimalDescription(string name)
        {
            switch (name)
            {
                case "Chicken":
                    return "Well cared-for adult chickens lay eggs every day." + Environment.NewLine + "Lives in the coop.";
                case "Duck":
                    return "Happy adults lay duck eggs every other day." + Environment.NewLine + "Lives in the coop.";
                case "Rabbit":
                    return "These are wooly rabbits! They shed precious wool every few days." + Environment.NewLine + "Lives in the coop.";
                case "Dairy Cow":
                    return "Adults can be milked daily. A milk pail is required to harvest the milk." + Environment.NewLine + "Lives in the barn.";
                case "Pig":
                    return "These pigs are trained to find truffles!" + Environment.NewLine + "Lives in the barn.";
                case "Goat":
                    return "Happy adults provide goat milk every other day. A milk pail is required to harvest the milk." + Environment.NewLine + "Lives in the barn.";
                case "Sheep":
                    return "Adults can be shorn for wool. Sheep who form a close bond with their owners can grow wool faster. A pair of shears is required to harvest the wool." + Environment.NewLine + "Lives in the barn.";
            }
            return "";
        }

        public override void draw(SpriteBatch b)
        {
            if (!onFarm && !Game1.dialogueUp && !Game1.globalFade)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                SpriteText.drawStringWithScrollBackground(b, farmName[currentFarmIndex], xPositionOnScreen + width / 4 + Game1.tileSize / 5, yPositionOnScreen, "Expansion", 1f, -1);
                Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false);
                Game1.dayTimeMoneyBox.drawMoneyBox(b, -1, -1);
                backButton.draw(b);
                forwardButton.draw(b);
                using (List<ClickableTextureComponent>.Enumerator enumerator = animalsToPurchase.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ClickableTextureComponent current = enumerator.Current;
                        current.draw(b, ((current.item as StardewValley.Object).type != null) ? (Color.Black * 0.4f) : Color.White, 0.87f);
                    }
                    goto IL_29D;
                }
            }
            if (!Game1.globalFade && onFarm)
            {
                string s = "Choose a " + animalBeingPurchased.buildingTypeILiveIn + " for your new " + animalBeingPurchased.type.Split(new char[]
                {
                    ' '
                }).Last();
                SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, Game1.tileSize / 4, "", 1f, -1);
                if (namingAnimal)
                {
                    b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                    Game1.drawDialogueBox(Game1.viewport.Width / 2 - Game1.tileSize * 4, Game1.viewport.Height / 2 - Game1.tileSize * 3 - Game1.tileSize / 2, Game1.tileSize * 8, Game1.tileSize * 3, false, true, null, false);
                    Utility.drawTextWithShadow(b, "Name your new animal: ", Game1.dialogueFont, new Vector2(Game1.viewport.Width / 2 - Game1.tileSize * 4 + Game1.tileSize / 2 + 8, Game1.viewport.Height / 2 - Game1.tileSize * 2 + 8), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                    textBox.Draw(b);
                    doneNamingButton.draw(b);
                    randomButton.draw(b);
                }
            }
            IL_29D:
            if (!Game1.globalFade && okButton != null)
            {
                okButton.draw(b);
            }
            if (hovered != null)
            {
                if ((hovered.item as StardewValley.Object).type != null)
                {
                    drawHoverText(b, Game1.parseText((hovered.item as StardewValley.Object).type, Game1.dialogueFont, Game1.tileSize * 5), Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null);
                }
                else
                {
                    SpriteText.drawStringWithScrollBackground(b, hovered.hoverText, xPositionOnScreen + spaceToClearSideBorder + Game1.tileSize, yPositionOnScreen + height + -Game1.tileSize / 2 + spaceToClearTopBorder / 2 + 8, "Truffle Pig", 1f, -1);
                    SpriteText.drawStringWithScrollBackground(b, "$" + hovered.name + "g", xPositionOnScreen + spaceToClearSideBorder + Game1.tileSize * 2, yPositionOnScreen + height + Game1.tileSize + spaceToClearTopBorder / 2 + 8, "$99999g", (Game1.player.Money >= Convert.ToInt32(hovered.name)) ? 1f : 0.5f, -1);
                    drawHoverText(b, Game1.parseText(getAnimalDescription(hovered.hoverText), Game1.smallFont, Game1.tileSize * 5), Game1.smallFont, 0, 0, -1, hovered.hoverText, -1, null, null, 0, -1, -1, -1, -1, 1f, null);
                }
            }
            drawMouse(b);
        }
    }
}
