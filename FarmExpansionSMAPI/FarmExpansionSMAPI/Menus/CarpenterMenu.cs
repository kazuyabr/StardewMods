using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using xTile.Dimensions;

namespace FarmExpansionSMAPI.Menus
{
    public class CarpenterMenu : IClickableMenu
    {
        GameLocation previousLocation = Game1.currentLocation;

        Farm currentFarm = Game1.getFarm();

        public int maxWidthOfBuildingViewer = 7 * Game1.tileSize;

        public int maxHeightOfBuildingViewer = 8 * Game1.tileSize;

        public int maxWidthOfDescription = 6 * Game1.tileSize;

        private List<BluePrint> blueprints;

        private int currentBlueprintIndex;

        private int currentFarmIndex;

        private ClickableTextureComponent okButton;

        private ClickableTextureComponent cancelButton;

        private ClickableTextureComponent backButton;

        private ClickableTextureComponent forwardButton;

        private ClickableTextureComponent upgradeIcon;

        private ClickableTextureComponent demolishButton;

        private ClickableTextureComponent pFarmButton;

        private ClickableTextureComponent nFarmButton;

        private Building currentBuilding;

        private string buildingDescription;

        private string buildingName;

        private string[] farmName = new string[2] { "Farm", "Expansion" };

        private List<Item> ingredients = new List<Item>();

        private int price;

        private bool onFarm;

        private bool drawBG = true;

        private bool freeze;

        private bool upgrading;

        private bool demolishing;

        private string hoverText = "";

        public BluePrint CurrentBlueprint
        {
            get
            {
                return blueprints[currentBlueprintIndex];
            }
        }

        public CarpenterMenu()
        {
            Game1.player.forceCanMove();
            resetBounds();
            blueprints = new List<BluePrint>();
            blueprints.Add(new BluePrint("Coop"));
            blueprints.Add(new BluePrint("Barn"));
            blueprints.Add(new BluePrint("Well"));
            blueprints.Add(new BluePrint("Silo"));
            if (!Game1.getFarm().isBuildingConstructed("Stable"))
            {
                blueprints.Add(new BluePrint("Stable"));
            }
            blueprints.Add(new BluePrint("Slime Hutch"));
            if (currentFarm.isBuildingConstructed("Coop"))
            {
                blueprints.Add(new BluePrint("Big Coop"));
            }
            if (currentFarm.isBuildingConstructed("Big Coop"))
            {
                blueprints.Add(new BluePrint("Deluxe Coop"));
            }
            if (currentFarm.isBuildingConstructed("Barn"))
            {
                blueprints.Add(new BluePrint("Big Barn"));
            }
            if (currentFarm.isBuildingConstructed("Big Barn"))
            {
                blueprints.Add(new BluePrint("Deluxe Barn"));
            }
            setNewActiveBlueprint();
        }

        private void populateFarmBlueprints()
        {
            blueprints.Clear();
            currentFarm = (currentFarmIndex == 0) ? Game1.getFarm() : FarmExpansionSMAPI.getFarmExtension();

            blueprints.Add(new BluePrint("Coop"));
            blueprints.Add(new BluePrint("Barn"));
            blueprints.Add(new BluePrint("Well"));
            blueprints.Add(new BluePrint("Silo"));
            if (!Game1.getFarm().isBuildingConstructed("Stable"))
            {
                blueprints.Add(new BluePrint("Stable"));
            }
            blueprints.Add(new BluePrint("Slime Hutch"));
            if (currentFarm.isBuildingConstructed("Coop"))
            {
                blueprints.Add(new BluePrint("Big Coop"));
            }
            if (currentFarm.isBuildingConstructed("Big Coop"))
            {
                blueprints.Add(new BluePrint("Deluxe Coop"));
            }
            if (currentFarm.isBuildingConstructed("Barn"))
            {
                blueprints.Add(new BluePrint("Big Barn"));
            }
            if (currentFarm.isBuildingConstructed("Big Barn"))
            {
                blueprints.Add(new BluePrint("Deluxe Barn"));
            }
        }

        private void resetBounds()
        {
            xPositionOnScreen = Game1.viewport.Width / 2 - maxWidthOfBuildingViewer - spaceToClearSideBorder;
            yPositionOnScreen = Game1.viewport.Height / 2 - maxHeightOfBuildingViewer / 2 - spaceToClearTopBorder + Game1.tileSize / 2;
            width = maxWidthOfBuildingViewer + maxWidthOfDescription + spaceToClearSideBorder * 2 + Game1.tileSize;
            height = maxHeightOfBuildingViewer + spaceToClearTopBorder;
            initialize(xPositionOnScreen, yPositionOnScreen, width, height, true);
            //ok is build (hammer icon)
            okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 3 - Game1.pixelZoom * 3, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, Game1.tileSize, Game1.tileSize), "OK", null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), Game1.pixelZoom, false, false);
            //cancel button is bottom right cancel button
            cancelButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - Game1.tileSize, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, Game1.tileSize, Game1.tileSize), "OK", null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false, false);
            //back button is left arrow
            //DEFbackButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + Game1.tileSize, yPositionOnScreen + maxHeightOfBuildingViewer, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), Game1.pixelZoom);
            //backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - Game1.tileSize * 3 / 2, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), Game1.pixelZoom);
            backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - spaceToClearSideBorder - Game1.tileSize / 5/* + Game1.tileSize + ((width - (maxWidthOfBuildingViewer + Game1.tileSize * 2)) / 2 - SpriteText.getWidthOfString("Deluxe Barn") / 2)*/, yPositionOnScreen, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), Game1.pixelZoom);
            //forward button is right arrow
            //DEFforwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - Game1.tileSize * 4 + Game1.tileSize / 4, yPositionOnScreen + maxHeightOfBuildingViewer, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), Game1.pixelZoom);
            //forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen/* + Game1.tileSize*/, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), Game1.pixelZoom);
            forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - spaceToClearSideBorder - borderWidth - Game1.tileSize + Game1.tileSize * 2 / 5/* + Game1.tileSize*/, yPositionOnScreen, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), Game1.pixelZoom);
            //demolish button is demolish (x icon)
            demolishButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 2 - Game1.pixelZoom * 2, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize - Game1.pixelZoom, Game1.tileSize, Game1.tileSize), "Demolish Buildings", null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), Game1.pixelZoom, false, false);
            //upgrade icon is small button showing if a building is an upgrade or new building
            upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - Game1.tileSize * 2 + Game1.tileSize / 2, yPositionOnScreen + Game1.pixelZoom * 2, 9 * Game1.pixelZoom, 13 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), Game1.pixelZoom);
            pFarmButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + Game1.tileSize * 2 + Game1.tileSize * 2 / 7, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), Game1.pixelZoom);
            nFarmButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 5 - Game1.pixelZoom * 3 - Game1.tileSize * 2 / 5, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), Game1.pixelZoom);
            //SpriteText.drawStringWithScrollBackground(b, buildingName, xPositionOnScreen + maxWidthOfBuildingViewer - spaceToClearSideBorder - Game1.tileSize / 4 + Game1.tileSize + ((width - (maxWidthOfBuildingViewer + Game1.tileSize * 2)) / 2 - SpriteText.getWidthOfString("Deluxe Barn") / 2), yPositionOnScreen, "Deluxe Barn", 1f, -1);
        }

        public void setNewActiveBlueprint()
        {
            if (blueprints[currentBlueprintIndex].name.Contains("Coop"))
            {
                currentBuilding = new Coop(blueprints[currentBlueprintIndex], Vector2.Zero);
            }
            else if (blueprints[currentBlueprintIndex].name.Contains("Barn"))
            {
                currentBuilding = new Barn(blueprints[currentBlueprintIndex], Vector2.Zero);
            }
            else
            {
                currentBuilding = new Building(blueprints[currentBlueprintIndex], Vector2.Zero);
            }
            price = blueprints[currentBlueprintIndex].moneyRequired;
            ingredients.Clear();
            foreach (KeyValuePair<int, int> current in blueprints[currentBlueprintIndex].itemsRequired)
            {
                ingredients.Add(new StardewValley.Object(current.Key, current.Value, false, -1, 0));
            }
            buildingDescription = blueprints[currentBlueprintIndex].description;
            buildingName = blueprints[currentBlueprintIndex].name;
        }

        public override void performHoverAction(int x, int y)
        {
            cancelButton.tryHover(x, y, 0.1f);
            base.performHoverAction(x, y);
            if (onFarm)
            {
                if ((upgrading || demolishing) && !freeze)
                {
                    foreach (Building current in currentFarm.buildings)
                    {
                        current.color = Color.White;
                    }
                    Building buildingAt = currentFarm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
                    if (buildingAt == null)
                    {
                        buildingAt = currentFarm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY() + Game1.tileSize * 2) / Game1.tileSize));
                        if (buildingAt == null)
                        {
                            buildingAt = currentFarm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY() + Game1.tileSize * 3) / Game1.tileSize));
                        }
                    }
                    if (upgrading)
                    {
                        if (buildingAt != null && CurrentBlueprint.nameOfBuildingToUpgrade != null && CurrentBlueprint.nameOfBuildingToUpgrade.Equals(buildingAt.buildingType))
                        {
                            buildingAt.color = Color.Lime * 0.8f;
                            return;
                        }
                        if (buildingAt != null)
                        {
                            buildingAt.color = Color.Red * 0.8f;
                            return;
                        }
                    }
                    else if (demolishing && buildingAt != null)
                    {
                        buildingAt.color = Color.Red * 0.8f;
                    }
                }
                return;
            }
            backButton.tryHover(x, y, 1f);
            forwardButton.tryHover(x, y, 1f);
            pFarmButton.tryHover(x, y, 1f);
            nFarmButton.tryHover(x, y, 1f);
            okButton.tryHover(x, y, 0.1f);
            demolishButton.tryHover(x, y, 0.1f);
            if (CurrentBlueprint.isUpgrade() && upgradeIcon.containsPoint(x, y))
            {
                hoverText = "Upgrade from " + CurrentBlueprint.nameOfBuildingToUpgrade;
                return;
            }
            if (demolishButton.containsPoint(x, y))
            {
                hoverText = "Demolish Buildings";
                return;
            }
            if (okButton.containsPoint(x, y) && CurrentBlueprint.doesFarmerHaveEnoughResourcesToBuild())
            {
                hoverText = "Build";
                return;
            }
            hoverText = "";
        }

        public override void receiveKeyPress(Keys key)
        {
            if (freeze)
            {
                return;
            }
            if (!onFarm)
            {
                base.receiveKeyPress(key);
            }
            if (!Game1.globalFade && onFarm)
            {
                if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(returnToCarpentryMenu), 0.02f);
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
                }
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (onFarm && !Game1.globalFade)
            {
                int num = Game1.getOldMouseX() + Game1.viewport.X;
                int num2 = Game1.getOldMouseY() + Game1.viewport.Y;
                if (num - Game1.viewport.X < Game1.tileSize)
                {
                    Game1.panScreen(-8, 0);
                }
                else if (num - (Game1.viewport.X + Game1.viewport.Width) >= -Game1.tileSize * 2)
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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (freeze)
            {
                return;
            }
            if (!onFarm)
            {
                base.receiveLeftClick(x, y, playSound);
            }
            if (cancelButton.containsPoint(x, y))
            {
                if (onFarm)
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(returnToCarpentryMenu), 0.02f);
                    Game1.playSound("smallSelect");
                    return;
                }
                exitThisMenu(true);
                Game1.player.forceCanMove();
                Game1.playSound("bigDeSelect");
            }
            if (!onFarm && backButton.containsPoint(x, y))
            {
                currentBlueprintIndex--;
                if (currentBlueprintIndex < 0)
                {
                    currentBlueprintIndex = blueprints.Count() - 1;
                }
                setNewActiveBlueprint();
                Game1.playSound("shwip");
                backButton.scale = backButton.baseScale;
            }
            if (!onFarm && forwardButton.containsPoint(x, y))
            {
                currentBlueprintIndex = (currentBlueprintIndex + 1) % blueprints.Count;
                setNewActiveBlueprint();
                forwardButton.scale = forwardButton.baseScale;
                Game1.playSound("shwip");
            }
            if (!onFarm && pFarmButton.containsPoint(x, y))
            {
                currentFarmIndex = (currentFarmIndex == 0) ? 1 : 0;
                currentBlueprintIndex = 0;
                populateFarmBlueprints();
                setNewActiveBlueprint();
                Game1.playSound("shwip");
                pFarmButton.scale = pFarmButton.baseScale;
            }
            if (!onFarm && nFarmButton.containsPoint(x, y))
            {
                currentFarmIndex = (currentFarmIndex == 0) ? 1 : 0;
                currentBlueprintIndex = 0;
                populateFarmBlueprints();
                setNewActiveBlueprint();
                nFarmButton.scale = nFarmButton.baseScale;
                Game1.playSound("shwip");
            }
            if (!onFarm && demolishButton.containsPoint(x, y))
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(setUpForBuildingPlacement), 0.02f);
                Game1.playSound("smallSelect");
                onFarm = true;
                demolishing = true;
            }
            if (okButton.containsPoint(x, y) && !onFarm && Game1.player.money >= price && blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild())
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(setUpForBuildingPlacement), 0.02f);
                Game1.playSound("smallSelect");
                onFarm = true;
            }
            if (onFarm && !freeze && !Game1.globalFade)
            {
                if (demolishing)
                {
                    Building buildingAt = currentFarm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
                    if (buildingAt != null && (buildingAt.daysOfConstructionLeft > 0 || buildingAt.daysUntilUpgrade > 0))
                    {
                        Game1.addHUDMessage(new HUDMessage("Can't demolish during construction", Color.Red, 3500f));
                        return;
                    }
                    if (buildingAt != null && buildingAt.indoors is AnimalHouse && (buildingAt.indoors as AnimalHouse).animalsThatLiveHere.Count() > 0)
                    {
                        Game1.addHUDMessage(new HUDMessage("Can't demolish until animals are relocated", Color.Red, 3500f));
                        return;
                    }
                    if (buildingAt != null && currentFarm.destroyStructure(buildingAt))
                    {
                        Game1.flashAlpha = 1f;
                        buildingAt.showDestroyedAnimation(currentFarm);
                        Game1.playSound("explosion");
                        Utility.spreadAnimalsAround(buildingAt, currentFarm);
                        DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(returnToCarpentryMenu), 1500);
                        freeze = true;
                    }
                    return;
                }
                else if (upgrading)
                {
                    Building buildingAt2 = currentFarm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
                    if (buildingAt2 != null && CurrentBlueprint.name != null && buildingAt2.buildingType.Equals(CurrentBlueprint.nameOfBuildingToUpgrade))
                    {
                        CurrentBlueprint.consumeResources();
                        buildingAt2.daysUntilUpgrade = 2;
                        buildingAt2.showUpgradeAnimation(currentFarm);
                        Game1.playSound("axe");
                        DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(returnToCarpentryMenuAfterSuccessfulBuild), 1500);
                        freeze = true;
                        return;
                    }
                    if (buildingAt2 != null)
                    {
                        Game1.addHUDMessage(new HUDMessage("Incorrect Building Type", Color.Red, 3500f));
                    }
                    return;
                }
                else
                {
                    if (tryToBuild())
                    {
                        CurrentBlueprint.consumeResources();
                        DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(returnToCarpentryMenuAfterSuccessfulBuild), 2000);
                        freeze = true;
                        return;
                    }
                    Game1.addHUDMessage(new HUDMessage("Can't Build There", Color.Red, 3500f));
                }
            }
        }

        public bool tryToBuild()
        {
            return currentFarm.buildStructure(CurrentBlueprint, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize), false, Game1.player);
        }

        public void returnToCarpentryMenu()
        {
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation = previousLocation;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(null, 0.02f);
            onFarm = false;
            resetBounds();
            upgrading = false;
            freeze = false;
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            Game1.viewport.Location = new Location(5 * Game1.tileSize, 24 * Game1.tileSize);
            drawBG = true;
            demolishing = false;
            Game1.displayFarmer = true;
        }

        public void returnToCarpentryMenuAfterSuccessfulBuild()
        {
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation = previousLocation;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(new Game1.afterFadeFunction(robinConstructionMessage), 0.02f);
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            Game1.viewport.Location = new Location(5 * Game1.tileSize, 24 * Game1.tileSize);
            freeze = true;
            Game1.displayFarmer = true;
        }

        public void robinConstructionMessage()
        {
            exitThisMenu(true);
            Game1.player.forceCanMove();
            string str = upgrading ? (CurrentBlueprint.name.ToLower().Split(new char[]
            {
                ' '
            }).Last() + " upgrade") : ("new " + CurrentBlueprint.name.ToLower());
            Game1.drawDialogue(Game1.getCharacterFromName("Robin"), "Okay, I'll have your " + str + " ready first thing tomorrow morning!$h");
            foreach (Building building in currentFarm.buildings)
            {
                if (building.daysOfConstructionLeft > 0)
                {
                    building.daysOfConstructionLeft = 1;
                }
                if (building.daysUntilUpgrade > 0)
                {
                    building.daysUntilUpgrade = 1;
                }
            }
        }

        public void setUpForBuildingPlacement()
        {
            Game1.currentLocation.cleanupBeforePlayerExit();
            hoverText = "";
            Game1.currentLocation = currentFarm;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(null, 0.02f);
            onFarm = true;
            cancelButton.bounds.X = Game1.viewport.Width - Game1.tileSize * 2;
            cancelButton.bounds.Y = Game1.viewport.Height - Game1.tileSize * 2;
            Game1.displayHUD = false;
            Game1.viewportFreeze = true;
            Game1.viewport.Location = new Location(49 * Game1.tileSize, 5 * Game1.tileSize);
            Game1.panScreen(0, 0);
            drawBG = false;
            freeze = false;
            Game1.displayFarmer = false;
            if (!demolishing && CurrentBlueprint.nameOfBuildingToUpgrade != null && CurrentBlueprint.nameOfBuildingToUpgrade.Count() > 0)
            {
                upgrading = true;
            }
        }

        public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
        {
            resetBounds();
        }

        public override void draw(SpriteBatch b)
        {
            if (drawBG)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            }
            if (Game1.globalFade || freeze)
            {
                return;
            }
            if (!onFarm)
            {
                base.draw(b);
                drawTextureBox(b, xPositionOnScreen - Game1.tileSize * 3 / 2, yPositionOnScreen - Game1.tileSize / 4, maxWidthOfBuildingViewer + Game1.tileSize, maxHeightOfBuildingViewer + Game1.tileSize, Color.White);
                currentBuilding.drawInMenu(b, xPositionOnScreen + maxWidthOfBuildingViewer / 2 - currentBuilding.tilesWide * Game1.tileSize / 2 - Game1.tileSize, yPositionOnScreen + maxHeightOfBuildingViewer / 2 - currentBuilding.getSourceRectForMenu().Height * Game1.pixelZoom / 2);
                if (CurrentBlueprint.isUpgrade())
                {
                    upgradeIcon.draw(b);
                }
                SpriteText.drawStringWithScrollBackground(b, buildingName, xPositionOnScreen + maxWidthOfBuildingViewer - spaceToClearSideBorder - Game1.tileSize / 4 + Game1.tileSize + ((width - (maxWidthOfBuildingViewer + Game1.tileSize * 2)) / 2 - SpriteText.getWidthOfString("Deluxe Barn") / 2), yPositionOnScreen, "Deluxe Barn", 1f, -1);
                SpriteText.drawStringWithScrollBackground(b, farmName[currentFarmIndex], xPositionOnScreen + ((width - (spaceToClearSideBorder + Game1.tileSize * 2)) / 2 - SpriteText.getWidthOfString("Expansion") / 2) - Game1.tileSize / 2, yPositionOnScreen + maxHeightOfBuildingViewer + Game1.tileSize, "Expansion", 1f, -1);
                drawTextureBox(b, xPositionOnScreen + maxWidthOfBuildingViewer - Game1.tileSize / 4, yPositionOnScreen + Game1.tileSize * 5 / 4, maxWidthOfDescription + Game1.tileSize, maxWidthOfDescription + Game1.tileSize * 3 / 2, Color.White);
                Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfDescription + Game1.tileSize, yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4), Game1.textColor, 1f, -1f, -1, -1, 0.25f, 3);
                Vector2 location = new Vector2(xPositionOnScreen + maxWidthOfDescription + Game1.tileSize / 4 + Game1.tileSize, yPositionOnScreen + Game1.tileSize * 4 + Game1.tileSize / 2);
                SpriteText.drawString(b, "$", (int)location.X, (int)location.Y, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1);
                Utility.drawTextWithShadow(b, price + "g", Game1.dialogueFont, new Vector2(location.X + Game1.tileSize + Game1.pixelZoom, location.Y + Game1.pixelZoom), (Game1.player.money >= price) ? Game1.textColor : Color.Red, 1f, -1f, -1, -1, 1f, 3);
                location.X -= Game1.tileSize / 4;
                location.Y -= Game1.tileSize / 3;
                foreach (Item current in ingredients)
                {
                    location.Y += Game1.tileSize + Game1.pixelZoom;
                    current.drawInMenu(b, location, 1f);
                    bool flag = !(current is StardewValley.Object) || Game1.player.hasItemInInventory((current as StardewValley.Object).parentSheetIndex, current.Stack, 0);
                    Utility.drawTextWithShadow(b, current.Name, Game1.dialogueFont, new Vector2(location.X + Game1.tileSize + Game1.pixelZoom * 4, location.Y + Game1.pixelZoom * 5), flag ? Game1.textColor : Color.Red, 1f, -1f, -1, -1, 1f, 3);
                }
                backButton.draw(b);
                forwardButton.draw(b);
                okButton.draw(b, blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild() ? Color.White : (Color.Gray * 0.8f), 0.88f);
                demolishButton.draw(b);
                pFarmButton.draw(b);
                nFarmButton.draw(b);
            }
            else
            {
                string s = upgrading ? ("Select a " + CurrentBlueprint.nameOfBuildingToUpgrade + " to upgrade.") : (demolishing ? "Choose a structure to demolish" : "Choose a location");
                SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, Game1.tileSize / 4, "", 1f, -1);
                if (!upgrading && !demolishing)
                {
                    Vector2 vector = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize, (Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize);
                    for (int i = 0; i < CurrentBlueprint.tilesHeight; i++)
                    {
                        for (int j = 0; j < CurrentBlueprint.tilesWidth; j++)
                        {
                            int num = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(j, i);
                            Vector2 vector2 = new Vector2(vector.X + j, vector.Y + i);
                            if (Game1.player.getTileLocation().Equals(vector2) || Game1.currentLocation.isTileOccupied(vector2, "") || !Game1.currentLocation.isTilePassable(new Location((int)vector2.X, (int)vector2.Y), Game1.viewport) || Game1.currentLocation.doesTileHaveProperty((int)vector2.X, (int)vector2.Y, "Diggable", "Back") == null)
                            {
                                num++;
                            }
                            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, vector2 * Game1.tileSize), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + num * 16, 388, 16, 16)), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.999f);
                        }
                    }
                }
            }
            cancelButton.draw(b);
            b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
            if (hoverText.Count() > 0)
            {
                drawHoverText(b, hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

    }
}
