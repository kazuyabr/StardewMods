using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using SerializerUtils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using StardewValley.Objects;

/* Advize's Farm Expansion Mod v2.0 */

namespace FarmExpansionSMAPI
{

    public class FarmExpansionSMAPI : Mod
    {

        public static bool addCrows = false;
        private static bool addHouse = false;
        private static bool backwardsCompatible = true;
        private static string modPath = "";
        private static int patchCount;

        public override void Entry(params object[] objects)
        {
            modPath = PathOnDisk;
            LoadConfig();
            //ControlEvents.KeyPressed += Event_KeyPressed; // For Dev Purposes
            ControlEvents.ControllerButtonPressed += Event_ControllerButtonPressed;
            ControlEvents.MouseChanged += Event_MouseChanged;
            PlayerEvents.LoadedGame += Event_PlayerLoadedGame;
            GameEvents.UpdateTick += Event_UpdateTick;
            TimeEvents.DayOfMonthChanged += Event_DayOfMonthChanged;
            LocationEvents.CurrentLocationChanged += Event_CurrentLocationChanged;
            Command.RegisterCommand("include_types", "Includes types to serialize").CommandFired += Command_IncludeTypes;
        }

        /*static void Event_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString().Equals("K"))
            {
                if (Game1.hasLoadedGame && Game1.activeClickableMenu == null)
                {
                    Game1.activeClickableMenu = new Menus.CarpenterMenu();
                }
                return;
            }
            if (e.KeyPressed.ToString().Equals("N"))
            {
                if (Game1.hasLoadedGame && Game1.activeClickableMenu == null)
                {
                    Game1.activeClickableMenu = new Menus.PurchaseAnimalsMenu();
                }
                return;
            }
            if (e.KeyPressed.ToString().Equals("V"))
            {
                if (Game1.hasLoadedGame)
                {
                    Farm farm = Game1.getLocationFromName("Farm") as Farm;
                    foreach (FarmAnimal farmAnimal in farm.animals.Values)
                    {
                        try { Log.Success("O1 : " + farmAnimal.name + " : " + farmAnimal.myID + " : " + farmAnimal.home.nameOfIndoors); } catch { Log.Error("Failed on " + farmAnimal.name); }
                    }
                    foreach (Building building in farm.buildings)
                    {
                        if (building.indoors is AnimalHouse)
                        {
                            try { Log.Debug(building.nameOfIndoors + " : " + building.currentOccupants); } catch { Log.Error("Failed"); }
                            AnimalHouse currentBuilding = building.indoors as AnimalHouse;
                            foreach (FarmAnimal buildingAnimal in currentBuilding.animals.Values)
                            {
                                try { Log.Success("I1 : " + buildingAnimal.name + " : " + buildingAnimal.myID + " : " + buildingAnimal.home.nameOfIndoors); } catch { Log.Error("Failed on " + buildingAnimal.name); }
                            }
                        }
                    }
                    FarmExtension farmExtension = getFarmExtension();
                    foreach (FarmAnimal farmAnimal in farmExtension.animals.Values)
                    {
                        try { Log.Success("O2 : " + farmAnimal.name + " : " + farmAnimal.myID + " : " + farmAnimal.home.nameOfIndoors); } catch { Log.Error("Failed on " + farmAnimal.name); }
                    }
                    foreach (Building building in farmExtension.buildings)
                    {
                        if (building.indoors is AnimalHouse)
                        {
                            try { Log.Debug(building.nameOfIndoors + " : " + building.currentOccupants); } catch { Log.Error("Failed"); }
                            AnimalHouse currentBuilding = building.indoors as AnimalHouse;
                            foreach (FarmAnimal buildingAnimal in currentBuilding.animals.Values)
                            {
                                try { Log.Success("I2 : " + buildingAnimal.name + " : " + buildingAnimal.myID + " : " + buildingAnimal.home.nameOfIndoors); } catch { Log.Error("Failed on " + buildingAnimal.name); }
                            }
                        }
                    }
                }
            }
        }*/

        static void Event_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (!Game1.hasLoadedGame) return;
            if (e.ButtonPressed == Buttons.A)
            {
                CheckForAction();
            }
        }

        static void Event_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (!Game1.hasLoadedGame) return;
            if (e.NewState.RightButton == ButtonState.Pressed && e.PriorState.RightButton != ButtonState.Pressed)
            {
                CheckForAction();
            }
        }

        static void Event_PlayerLoadedGame(object sender, EventArgs e)
        {
            if (SaveGame.loaded == null) return;
            foreach (GameLocation gl in SaveGame.loaded.locations)
            {
                if (gl.name.Equals("FarmExpansion"))
                {
                    FarmExtension fe = (FarmExtension)Game1.getLocationFromName(gl.name);
                    fe.resourceClumps = ((FarmExtension)gl).resourceClumps;
                    fe.buildings = ((FarmExtension)gl).buildings;
                    fe.animals = ((FarmExtension)gl).animals;
                    foreach (Building building in fe.buildings)
                    {
                        building.load();
                        if(building.indoors != null)
                        {
                            if (building.indoors.GetType() == typeof(AnimalHouse))
                            {
                                foreach (KeyValuePair<long, FarmAnimal> animalsInBuilding in ((AnimalHouse)building.indoors).animals)
                                {
                                    FarmAnimal animal = animalsInBuilding.Value;
                                    string str = animal.type;
                                    if (animal.age < animal.ageWhenMature)
                                    {
                                        str = "Baby" + (animal.type.Equals("Duck") ? "White Chicken" : animal.type);
                                    }
                                    else if (animal.showDifferentTextureWhenReadyForHarvest && animal.currentProduce <= 0)
                                    {
                                        str = "Sheared" + animal.type;
                                    }
                                    animal.sprite = new AnimatedSprite(Game1.content.Load<Texture2D>("Animals\\" + str), 0, animal.frontBackSourceRect.Width, animal.frontBackSourceRect.Height);
                                    if (getFarmExtension() != null)
                                    {
                                        foreach (Building current in getFarmExtension().buildings)
                                        {
                                            if (current.tileX == (int)animal.homeLocation.X && current.tileY == (int)animal.homeLocation.Y)
                                            {
                                                animal.home = current;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    fe.piecesOfHay = (gl as FarmExtension).piecesOfHay;
                    RepairBuildingWarps();
                }
            }
        }

        static void Event_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.locations.Count < 47) return;
            for (int i = 0; i <= Game1.locations.Count - 1; i++)
            {
                if (Game1.locations[i].Name == "FarmExpansion")
                {
                    GameEvents.UpdateTick -= Event_UpdateTick;
                    return;
                }
            }
            
            string relativeAssetPath = Path.Combine(modPath, "FarmExtension.xnb");
            string mapName = "FarmExpansion";

            FarmExtension farmExtension = new FarmExtension(LoadMap(relativeAssetPath), mapName);

            farmExtension.isFarm = true;
            farmExtension.isOutdoors = true;
            Game1.locations.Add(farmExtension);

            if (backwardsCompatible)
            {
                GameLocation temp = new GameLocation(LoadMap(relativeAssetPath), "FarmGreenhouseExtension");
                temp.isFarm = true;
                temp.isOutdoors = true;
                Game1.locations.Add(temp);
            }

            Game1.locations[1].warps.Add(new Warp(-1, 44, mapName, 46, 4, false));
            Game1.locations[1].warps.Add(new Warp(-1, 45, mapName, 46, 5, false));
            Game1.locations[1].warps.Add(new Warp(-1, 46, mapName, 46, 6, false));

            //Game1.locations[1].warps.Add(new Warp(64, 19, "FarmGreenhouseExtension", 62, 23, false));

            PatchMap(Game1.locations[1], FarmEdits());

            if(addHouse)
            {
                PatchMap(farmExtension, FarmExtensionEdits());
                //farmExtension.setTileProperty(63, 13, "Buildings", "Action", "Warp 64 18 Farm");
                farmExtension.setTileProperty(63, 13, "Buildings", "Action", "TestProperty");
            }

            Game1.getLocationFromName("ScienceHouse").setTileProperty(8, 19, "Buildings", "Action", "CustomCarpenter");
            Game1.getLocationFromName("AnimalShop").setTileProperty(12, 15, "Buildings", "Action", "CustomAnimalShop");

            GameEvents.UpdateTick -= Event_UpdateTick;
        }

        static void Event_DayOfMonthChanged(object sender, EventArgs e)
        {
            if (Game1.isRaining)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> pair in getFarmExtension().terrainFeatures)
                {
                    if (pair.Value != null && pair.Value is HoeDirt)
                    {
                        ((HoeDirt)pair.Value).state = 1;
                    }
                }
            }
            foreach (Building expansionBuilding in getFarmExtension().buildings)
            {
                if (expansionBuilding.indoors != null)
                {
                    for (int i = expansionBuilding.indoors.objects.Count - 1; i >= 0; i--)
                    {
                        if (expansionBuilding.indoors.objects[expansionBuilding.indoors.objects.Keys.ElementAt(i)].minutesElapsed(3000 - Game1.timeOfDay, expansionBuilding.indoors))
                        {
                            expansionBuilding.indoors.objects.Remove(expansionBuilding.indoors.objects.Keys.ElementAt(i));
                        }
                    }
                }
            }
            RepairBuildingWarps();
        }

        static void Event_CurrentLocationChanged(object sender, EventArgs e)
        {
            if (patchCount < 1)
            {
                patchCount++;
                return;
            }
            PatchMap(Game1.locations[1], FarmEdits());
            Game1.locations[1].warps.Add(new Warp(-1, 44, getFarmExtension().name, 46, 4, false));
            Game1.locations[1].warps.Add(new Warp(-1, 45, getFarmExtension().name, 46, 5, false));
            Game1.locations[1].warps.Add(new Warp(-1, 46, getFarmExtension().name, 46, 6, false));
            if (backwardsCompatible)
            {
                patchLocation();
            }
            LocationEvents.CurrentLocationChanged -= Event_CurrentLocationChanged;
        }

        static void Command_IncludeTypes(object sender, EventArgsCommand e)
        {
            SerializerUtility.AddType(typeof(FarmExtension));
        }

        static void RepairBuildingWarps()
        {
            foreach (Building building in getFarmExtension().buildings)
            {
                if (building.indoors != null)
                {
                    List<Warp> warps = new List<Warp>();
                    foreach (Warp warp in building.indoors.warps)
                    {
                        warps.Add(new Warp(warp.X, warp.Y, "FarmExpansion", building.humanDoor.X + building.tileX, building.humanDoor.Y + building.tileY + 1, false));
                    }
                    building.indoors.warps.Clear();
                    building.indoors.warps.AddRange(warps);
                }
            }
        }

        static void CheckForAction()
        {
            if (!Game1.player.UsingTool && !Game1.pickingTool && !Game1.menuUp && (!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && !Game1.nameSelectUp && Game1.numberOfSelectedItems == -1 && !Game1.fadeToBlack && Game1.activeClickableMenu == null)
            {
                Vector2 grabTile = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
                if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
                {
                    grabTile = Game1.player.GetGrabTile();
                }
                xTile.Tiles.Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize), Game1.viewport.Size);
                xTile.ObjectModel.PropertyValue propertyValue = null;
                if (tile != null)
                {
                    tile.Properties.TryGetValue("Action", out propertyValue);
                }

                if (propertyValue != null)
                {
                    if (propertyValue == "TestProperty")
                    {
                        Game1.drawObjectDialogue("Cottage interior not yet implemented.");
                    }
                    if (propertyValue == "CustomCarpenter")
                    {
                        if (Game1.player.getTileY() > grabTile.Y)
                        {
                            carpenter(new Location((int)grabTile.X, (int)grabTile.Y));
                        }
                    }
                    if (propertyValue == "CustomAnimalShop")
                    {
                        if (Game1.player.getTileY() > grabTile.Y)
                        {
                            animalShop(new Location((int)grabTile.X, (int)grabTile.Y));
                        }
                    }
                }
            }
        }

        /*private static void testPropertyAnswer(Farmer who, string whichAnswer)
        {
            Log.Success("testPropertyAnswer called " + whichAnswer);
            questionActive = false;
            if (whichAnswer == "Yes")
            {
                Game1.activeClickableMenu = new CarpenterMenu();
            }
        }*/

        private static void animalShop(Location tileLocation)
        {
            foreach (NPC current in Game1.currentLocation.characters)
            {
                if (current.name.Equals("Marnie"))
                {
                    if (!current.getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y - 1)))
                    {
                        return;
                    }
                    current.faceDirection(2);
                    Game1.currentLocation.createQuestionDialogue("", new string[]
                    {
                        "Supplies Shop",
                        "Purchase Animals",
                        "Leave"
                    }, animalShop2, null);
                    return;
                }
            }
            if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
            {
                Game1.drawObjectDialogue("Closed for the day.^^-Marnie");
            }
        }

        private static void animalShop2(Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "Supplies":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(), 0, "Marnie");
                    return;
                case "Purchase":
                    Game1.activeClickableMenu = new Menus.PurchaseAnimalsMenu();
                    return;
                default:
                    return;
            }
        }

        private static void carpenter(Location tileLocation)
        {
            foreach (NPC current in Game1.currentLocation.characters)
            {
                if (current.name.Equals("Robin"))
                {
                    if (Vector2.Distance(current.getTileLocation(), new Vector2(tileLocation.X, tileLocation.Y)) > 3f)
                    {
                        return;
                    }
                    current.faceDirection(2);
                    if (Game1.player.daysUntilHouseUpgrade < 0)
                    {
                        if (Game1.player.houseUpgradeLevel < 2)
                        {
                            Game1.currentLocation.createQuestionDialogue("What can I do for you?", new string[]
                            {
                                    "Shop",
                                    "Upgrade House",
                                    "Construct Farm Buildings",
                                    "Leave"
                            }, carpenter2, null);
                        }
                        else
                        {
                            Game1.currentLocation.createQuestionDialogue("What can I do for you?", new string[]
                            {
                                    "Shop",
                                    "Construct Farm Buildings",
                                    "Leave"
                            }, carpenter2, null);
                        }
                    }
                    else
                    {
                        Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                    }
                    return;
                }
            }
            if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
            {
                Game1.drawObjectDialogue("Sorry! Shop's closed for the day... I'm at Caroline's place for aerobics club.^^-Robin");
            }
        }

        private static void carpenter2(Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "Shop":
                    Game1.player.forceCanMove();
                    Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                    return;
                case "Upgrade":
                    switch (Game1.player.houseUpgradeLevel)
                    {
                        case 0:
                            Game1.currentLocation.createQuestionDialogue(Game1.parseText("I can increase the size of your house and add a kitchen. It will cost 10,000g and you'll also need to provide me with 450 pieces of wood. Are you interested?"), new string[]
                            {
                                "Yes",
                                "No"
                            }, "upgrade");
                            return;
                        case 1:
                            Game1.currentLocation.createQuestionDialogue(Game1.parseText("I can increase the size of your house and add a nursery. It will cost 50,000g and you'll also need to provide me with 150 pieces of hardwood. Are you interested?"), new string[]
                            {
                                "Yes",
                                "No"
                            }, "upgrade");
                            return;
                        default:
                            return;
                    }
                case "Construct":
                    Game1.activeClickableMenu = new Menus.CarpenterMenu();
                    return;
                default:
                    return;
            }
        }

        public static FarmExtension getFarmExtension()
        {
            return Game1.getLocationFromName("FarmExpansion") as FarmExtension;
        }

        static Map LoadMap(string filePath)
        {
            Map map = null;
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            ContentManager cm = new ContentManager(new GameServiceContainer(), path);
            map = cm.Load<Map>(fileName);

            if (map == null) throw new FileLoadException();

            return map;
        }

        private static void LoadConfig() //Remnant of older versions, should switch to SMAPI config implementation
        {
            string configLocation = Path.Combine(modPath, "config.ini");

            if (!File.Exists(configLocation))
            {
                try
                {
                    File.Create(configLocation).Close();
                    StreamWriter sw = new StreamWriter(configLocation);
                    sw.WriteLine("addCrows=false");
                    sw.WriteLine("addHouse=false");
                    sw.WriteLine("backwardsCompatible=true");
                    sw.Close();
                }
                catch (Exception e) { Log.Error(e); }
            }
            else
            {
                try
                {
                    StreamReader sr = new StreamReader(configLocation);
                    if (sr.ReadLine().Contains("true")) addCrows = true;
                    if (sr.ReadLine().Contains("true")) addHouse = true;
                    if (sr.ReadLine().Contains("false")) backwardsCompatible = false;
                }
                catch (Exception e) { Log.Error(e); }
            }
        }

        private static void PatchMap(GameLocation gl, List<Tile> tileArray)
        {
            try
            {
                foreach (Tile tile in tileArray)
                {
                    if (tile.tileIndex < 0)
                    {
                        gl.removeTile(tile.x, tile.y, tile.layer);
                        continue;
                    }

                    if (gl.map.Layers[tile.l].Tiles[tile.x, tile.y] == null)
                    {
                        // Following 10 lines are a context specific fix
                        int tileSheetIndex = tile.tileSheet;
                        for (int i = 0; i < gl.map.TileSheets.Count; i++)
                        {
                            if (gl.map.TileSheets[i].Id.Equals("untitled tile sheet"))
                            {
                                tileSheetIndex = i;
                                break;
                            }
                        }
                        gl.map.Layers[tile.l].Tiles[tile.x, tile.y] = new StaticTile(gl.map.GetLayer(tile.layer), gl.map.TileSheets[tileSheetIndex], BlendMode.Alpha, tile.tileIndex);
                        //gl.map.Layers[tile.l].Tiles[tile.x, tile.y] = new StaticTile(gl.map.GetLayer(tile.layer), gl.map.TileSheets[tile.tileSheet], BlendMode.Alpha, tile.tileIndex);
                    }
                    else
                    {
                        gl.setMapTileIndex(tile.x, tile.y, tile.tileIndex, tile.layer);
                    }
                }
            }
            finally
            {
                tileArray.Clear();
            }
        }

        private static void patchLocation()
        {
            try
            {
                GameLocation source = Game1.getLocationFromName("FarmGreenhouseExtension");
                GameLocation comparison = new GameLocation(LoadMap(Path.Combine(modPath, "FarmExtension.xnb")), "FarmGreenhouseExtension");
                FarmExtension target = getFarmExtension();

                bool a = source.largeTerrainFeatures.SequenceEqual(comparison.largeTerrainFeatures);
                bool b = source.terrainFeatures.Count == comparison.terrainFeatures.Count;
                bool c = source.debris.SequenceEqual(comparison.debris);
                bool d = source.objects.Count == comparison.objects.Count;
                bool e = source.numberOfSpawnedObjectsOnMap == comparison.numberOfSpawnedObjectsOnMap;

                bool a2 = source.largeTerrainFeatures.SequenceEqual(target.largeTerrainFeatures);
                bool b2 = source.terrainFeatures.Count == target.terrainFeatures.Count;
                bool c2 = source.debris.SequenceEqual(target.debris);
                bool d2 = source.objects.Count == target.objects.Count;
                bool e2 = source.numberOfSpawnedObjectsOnMap == target.numberOfSpawnedObjectsOnMap;

                if (a && b && c && d && e)
                {
                    //Log.Error("Source & Comparison are equal, returning");
                    return;
                }
                if(a2 && b2 && c2 && d2 && e2)
                {
                    //Log.Error("Source & Target are equal, returning");
                    return;
                }

                target.largeTerrainFeatures.Clear();
                target.terrainFeatures.Clear();
                target.debris.Clear();
                target.numberOfSpawnedObjectsOnMap = 0;
                target.objects.Clear();
                target.resourceClumps.Clear();

                target.largeTerrainFeatures = source.largeTerrainFeatures;
                target.terrainFeatures = source.terrainFeatures;
                target.debris = source.debris;
                target.numberOfSpawnedObjectsOnMap = source.numberOfSpawnedObjectsOnMap;
                target.objects = source.objects;
            }
            finally
            {
                Game1.locations.Remove(Game1.getLocationFromName("FarmGreenhouseExtension"));
            }
        }

        private static List<Tile> FarmEdits()
        {
            List<Tile> list = new List<Tile>()
            {
                new Tile(0, 0, 38, 175), new Tile(0, 1, 38, 175), new Tile(0, 0, 43, 537),
                new Tile(0, 1, 43, 537), new Tile(0, 2, 43, 586), new Tile(0, 0, 44, 566),
                new Tile(0, 1, 44, 537), new Tile(0, 2, 44, 618), new Tile(0, 0, 45, 587),
                new Tile(0, 1, 45, 473), new Tile(0, 0, 46, 587), new Tile(0, 1, 46, 587),
                new Tile(0, 0, 48, 175), new Tile(0, 1, 48, 175),

                new Tile(1, 0, 39, 175), new Tile(1, 1, 39, 175), new Tile(1, 2, 39, 444),
                new Tile(1, 0, 40, 446), new Tile(1, 1, 40, 468), new Tile(1, 2, 40, 469),
                new Tile(1, 0, 41, 492), new Tile(1, 1, 41, 493), new Tile(1, 2, 41, 494),
                new Tile(1, 0, 42, 517), new Tile(1, 1, 42, 518), new Tile(1, 2, 42, 519),
                new Tile(1, 0, 43, 542), new Tile(1, 1, 43, 543), new Tile(1, 2, 43, 544),
                new Tile(1, 0, 44, -1), new Tile(1, 1, 44, -1), new Tile(1, 2, 44, -1),
                new Tile(1, 0, 45, -1), new Tile(1, 1, 45, -1), new Tile(1, 2, 45, -1),
                new Tile(1, 0, 46, -1), new Tile(1, 1, 46, -1), new Tile(1, 2, 46, -1),
                new Tile(1, 0, 47, 175), new Tile(1, 1, 47, 175), new Tile(1, 0, 48, -1),

                new Tile(3, 0, 36, -1), new Tile(3, 1, 36, -1), new Tile(3, 0, 37, -1),
                new Tile(3, 1, 37, -1), new Tile(3, 0, 38, -1), new Tile(3, 1, 38, -1),
                new Tile(3, 0, 39, -1), new Tile(3, 1, 39, -1), new Tile(3, 0, 40, -1),
                new Tile(3, 0, 41, -1), new Tile(3, 0, 46, 414), new Tile(3, 1, 46, 413),
                new Tile(3, 2, 46, 438), new Tile(3, 0, 47, 175), new Tile(3, 1, 47, 175),
                new Tile(3, 2, 47, 394)
            };
            return list;
        }

        private static List<Tile> FarmExtensionEdits()
        {
            List<Tile> list = new List<Tile>()
            {
                new Tile(0, 57, 23, 566), new Tile(0, 58, 23, 537), new Tile(0, 59, 23, 618),
                new Tile(0, 58, 24, 473), new Tile(0, 59, 22, 586), new Tile(0, 59, 21, 586),
                new Tile(0, 59, 20, 586), new Tile(0, 59, 19, 568), new Tile(0, 60, 19, 567),
                new Tile(0, 61, 19, 568), new Tile(0, 62, 19, 567), new Tile(0, 63, 19, 568),
                new Tile(0, 64, 19, 567), new Tile(0, 65, 19, 568), new Tile(0, 66, 19, 567),
                new Tile(0, 66, 20, 588), new Tile(0, 66, 21, 613), new Tile(0, 67, 21, 537),
                new Tile(0, 60, 22, 587), new Tile(0, 61, 22, 587), new Tile(0, 62, 22, 587),
                new Tile(0, 63, 22, 587), new Tile(0, 64, 22, 587), new Tile(0, 65, 22, 587),
                new Tile(0, 66, 22, 587), new Tile(0, 67, 22, 613), new Tile(0, 60, 23, 587),
                new Tile(0, 61, 23, 587), new Tile(0, 62, 23, 587), new Tile(0, 63, 23, 587),
                new Tile(0, 64, 23, 587), new Tile(0, 65, 23, 587), new Tile(0, 66, 23, 587),
                new Tile(0, 67, 23, 587), new Tile(0, 68, 23, 613), new Tile(0, 69, 23, 591),

                new Tile(1, 59, 22, 544), new Tile(1, 60, 22, -1), new Tile(1, 61, 22, -1),
                new Tile(1, 62, 22, -1), new Tile(1, 63, 22, -1), new Tile(1, 64, 22, -1),
                new Tile(1, 65, 22, -1), new Tile(1, 66, 22, -1), new Tile(1, 67, 22, -1),
                new Tile(1, 68, 22, 540), new Tile(1, 59, 21, 519), new Tile(1, 60, 21, -1),
                new Tile(1, 61, 21, -1), new Tile(1, 62, 21, -1), new Tile(1, 63, 21, -1),
                new Tile(1, 64, 21, -1), new Tile(1, 65, 21, -1), new Tile(1, 66, 21, 541),
                new Tile(1, 67, 21, 539), new Tile(1, 68, 21, 515), new Tile(1, 59, 20, 494),
                new Tile(1, 60, 20, -1), new Tile(1, 61, 20, -1), new Tile(1, 62, 20, -1),
                new Tile(1, 63, 20, -1), new Tile(1, 64, 20, -1), new Tile(1, 65, 20, -1),
                new Tile(1, 66, 20, 516), new Tile(1, 67, 20, 514), new Tile(1, 68, 20, 490),
                new Tile(1, 59, 19, 469), new Tile(1, 60, 19, -1), new Tile(1, 61, 19, -1),
                new Tile(1, 62, 19, -1), new Tile(1, 63, 19, -1), new Tile(1, 64, 19, -1),
                new Tile(1, 65, 19, -1), new Tile(1, 66, 19, 491), new Tile(1, 67, 19, 489),
                new Tile(1, 68, 19, 465), new Tile(1, 59, 18, 444), new Tile(1, 63, 18, -1),
                new Tile(1, 66, 18, 466), new Tile(1, 67, 18, 464), new Tile(1, 68, 18, 440),
                new Tile(1, 59, 17, 419), new Tile(1, 64, 17, -1), new Tile(1, 65, 17, -1),
                new Tile(1, 66, 17, 441), new Tile(1, 59, 16, 438), new Tile(1, 62, 16, -1),
                new Tile(1, 63, 16, -1), new Tile(1, 64, 16, -1), new Tile(1, 66, 16, 439),
                new Tile(1, 60, 15, -1), new Tile(1, 61, 15, -1), new Tile(1, 62, 15, -1),
                new Tile(1, 63, 15, -1), new Tile(1, 64, 15, -1), new Tile(1, 66, 15, 148),
                new Tile(1, 59, 14, 1522), new Tile(1, 60, 14, 1158), new Tile(1, 61, 14, 1159),
                new Tile(1, 62, 14, 921), new Tile(1, 63, 14, -1), new Tile(1, 64, 14, -1),
                new Tile(1, 65, 14, 1163), new Tile(1, 66, 14, 1164), new Tile(1, 59, 13, 1497),
                new Tile(1, 60, 13, 1133), new Tile(1, 61, 13, 1134), new Tile(1, 62, 13, 1135),
                new Tile(1, 63, 13, 1136), new Tile(1, 64, 13, 1137), new Tile(1, 65, 13, 1138),
                new Tile(1, 66, 13, 1139), new Tile(1, 67, 13, -1), new Tile(1, 60, 12, 1108),
                new Tile(1, 61, 12, 1109), new Tile(1, 62, 12, 1110), new Tile(1, 63, 12, 1111),
                new Tile(1, 64, 12, 1112), new Tile(1, 65, 12, 1113), new Tile(1, 66, 12, 1114),
                new Tile(1, 60, 11, 1083), new Tile(1, 61, 11, 1084), new Tile(1, 62, 11, 1085),
                new Tile(1, 63, 11, 1086), new Tile(1, 64, 11, 1087), new Tile(1, 65, 11, 1088),
                new Tile(1, 66, 11, 1089), new Tile(1, 60, 10, 1058), new Tile(1, 61, 10, 1059),
                new Tile(1, 62, 10, 1060), new Tile(1, 63, 10, 1061), new Tile(1, 64, 10, 1062),
                new Tile(1, 65, 10, 1063), new Tile(1, 66, 10, 1064), new Tile(1, 60, 9, 1033),
                new Tile(1, 61, 9, 1034), new Tile(1, 62, 9, 1035), new Tile(1, 63, 9, 1036),
                new Tile(1, 64, 9, 1037), new Tile(1, 65, 9, 1038), new Tile(1, 66, 9, 1039),
                new Tile(1, 60, 8, 1008), new Tile(1, 61, 8, 1009), new Tile(1, 62, 8, 1010),
                new Tile(1, 63, 8, 1011), new Tile(1, 64, 8, 1012), new Tile(1, 65, 8, 1013),
                new Tile(1, 66, 8, 1014),

                new Tile(3, 62, 15, -1), new Tile(3, 62, 14, -1), new Tile(3, 64, 14, -1),
                new Tile(3, 66, 14, 123), new Tile(3, 62, 13, 896), new Tile(3, 59, 12, -1),
                new Tile(3, 60, 12, -1), new Tile(3, 61, 12, -1), new Tile(3, 62, 12, 871),
                new Tile(3, 63, 12, -1), new Tile(3, 64, 12, -1), new Tile(3, 59, 11, -1),
                new Tile(3, 60, 11, -1), new Tile(3, 61, 11, -1), new Tile(3, 62, 11, -1),
                new Tile(3, 63, 11, -1), new Tile(3, 64, 11, -1), new Tile(3, 59, 10, -1),
                new Tile(3, 60, 10, -1), new Tile(3, 61, 10, -1), new Tile(3, 62, 10, -1),
                new Tile(3, 63, 10, -1), new Tile(3, 64, 10, -1), new Tile(3, 60, 9, -1),
                new Tile(3, 61, 9, -1), new Tile(3, 62, 9, -1), new Tile(3, 63, 9, -1),
                new Tile(3, 60, 7, 983), new Tile(3, 61, 7, 984), new Tile(3, 62, 7, 985),
                new Tile(3, 63, 7, 986), new Tile(3, 64, 7, 987), new Tile(3, 65, 7, 988),
                new Tile(3, 66, 7, 989), new Tile(3, 61, 6, 959), new Tile(3, 62, 6, 960),
                new Tile(3, 63, 6, 961), new Tile(3, 64, 6, 962), new Tile(3, 65, 6, 963),
                new Tile(3, 61, 5, 934), new Tile(3, 62, 5, 935), new Tile(3, 63, 5, 936),
                new Tile(3, 64, 5, 937)
            };
            return list;
        }
    }

    [Serializable]
    public class FarmExtension : Farm
    {

        public FarmExtension()
        {
        }

        public FarmExtension(Map m, string name) : base(m, name)
        {
        }

        public override void DayUpdate(int dayOfMonth)
        {
            temporarySprites.Clear();
            for (int i = terrainFeatures.Count() - 1; i >= 0; i--)
            {
                terrainFeatures.ElementAt(i).Value.dayUpdate(this, terrainFeatures.ElementAt(i).Key);
            }
            if (largeTerrainFeatures != null)
            {
                foreach (LargeTerrainFeature current in largeTerrainFeatures)
                {
                    current.dayUpdate(this);
                }
            }
            foreach (StardewValley.Object current2 in objects.Values)
            {
                current2.DayUpdate(this);
            }
            debris.Clear();
            lightLevel = 0f;
            addLightGlows();
            lastItemShipped = null;
            foreach (Building current in buildings)
            {
                current.dayUpdate(dayOfMonth);
            }
            for (int i = animals.Count() - 1; i >= 0; i--)
            {
                animals.ElementAt(i).Value.dayUpdate(this);
            }
            if (characters.Count() > 5)
            {
                int num = 0;
                for (int j = characters.Count - 1; j >= 0; j--)
                {
                    if (characters[j] is GreenSlime && Game1.random.NextDouble() < 0.035)
                    {
                        characters.RemoveAt(j);
                        num++;
                    }
                }
                if (num > 0)
                {
                    Game1.showGlobalMessage((num == 1) ? "A slime escaped during the night." : (num + " slimes escaped during the night."));
                }
            }
            Dictionary<Vector2, TerrainFeature>.KeyCollection keys = terrainFeatures.Keys;
            for (int k = keys.Count() - 1; k >= 0; k--)
            {
                if (terrainFeatures[keys.ElementAt(k)] is HoeDirt && (terrainFeatures[keys.ElementAt(k)] as HoeDirt).crop == null && Game1.random.NextDouble() <= 0.1)
                {
                    terrainFeatures.Remove(keys.ElementAt(k));
                }
            }
            if (terrainFeatures.Count() > 0 && Game1.currentSeason.Equals("fall") && Game1.dayOfMonth > 1 && Game1.random.NextDouble() < 0.05)
            {
                for (int l = 0; l < 10; l++)
                {
                    TerrainFeature value = terrainFeatures.ElementAt(Game1.random.Next(terrainFeatures.Count())).Value;
                    if (value is Tree && (value as Tree).growthStage >= 5 && !(value as Tree).tapped)
                    {
                        (value as Tree).treeType = 7;
                        (value as Tree).loadSprite();
                        break;
                    }
                }
            }
            if (FarmExpansionSMAPI.addCrows)
            {
                addCrows();
            }
            if (!Game1.currentSeason.Equals("winter"))
            {
                spawnWeedsAndStones(Game1.currentSeason.Equals("summer") ? 30 : 20, false, true);
            }
            spawnWeeds(false);
            if (dayOfMonth == 1)
            {
                for (int m = terrainFeatures.Count - 1; m >= 0; m--)
                {
                    if (terrainFeatures.ElementAt(m).Value is HoeDirt && (terrainFeatures.ElementAt(m).Value as HoeDirt).crop == null && Game1.random.NextDouble() < 0.8)
                    {
                        terrainFeatures.Remove(terrainFeatures.ElementAt(m).Key);
                    }
                }
                spawnWeedsAndStones(20, false, false);
                if (Game1.currentSeason.Equals("spring") && Game1.stats.DaysPlayed > 1u)
                {
                    spawnWeedsAndStones(40, false, false);
                    spawnWeedsAndStones(40, true, false);
                    for (int n = 0; n < 15; n++)
                    {
                        int num2 = Game1.random.Next(map.DisplayWidth / Game1.tileSize);
                        int num3 = Game1.random.Next(map.DisplayHeight / Game1.tileSize);
                        Vector2 vector = new Vector2(num2, num3);
                        StardewValley.Object @object;
                        objects.TryGetValue(vector, out @object);
                        if (@object == null && doesTileHaveProperty(num2, num3, "Diggable", "Back") != null && isTileLocationOpen(new Location(num2 * Game1.tileSize, num3 * Game1.tileSize)) && !isTileOccupied(vector, "") && doesTileHaveProperty(num2, num3, "Water", "Back") == null)
                        {
                            terrainFeatures.Add(vector, new Grass(1, 4));
                        }
                    }
                    growWeedGrass(40);
                }
            }
            growWeedGrass(1);
        }

        new public void spawnWeedsAndStones(int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
        {
            if (!Game1.currentSeason.Equals("winter"))
            {
                if (objects.Count() <= 0 && spawnFromOldWeeds)
                {
                    return;
                }
                int num = (numDebris != -1) ? numDebris : ((Game1.random.NextDouble() < 0.95) ? ((Game1.random.NextDouble() < 0.25) ? Game1.random.Next(10, 21) : Game1.random.Next(5, 11)) : 0);
                if (Game1.isRaining)
                {
                    num *= 2;
                }
                if (Game1.dayOfMonth == 1)
                {
                    num *= 5;
                }
                for (int i = 0; i < num; i++)
                {
                    Vector2 value = spawnFromOldWeeds ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : new Vector2(Game1.random.Next(map.Layers[0].LayerWidth), Game1.random.Next(map.Layers[0].LayerHeight));
                    while (spawnFromOldWeeds && value.Equals(Vector2.Zero))
                    {
                        value = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
                    }
                    KeyValuePair<Vector2, StardewValley.Object> keyValuePair = new KeyValuePair<Vector2, StardewValley.Object>(Vector2.Zero, null);
                    if (spawnFromOldWeeds)
                    {
                        keyValuePair = objects.ElementAt(Game1.random.Next(objects.Count()));
                    }
                    Vector2 value2 = spawnFromOldWeeds ? keyValuePair.Key : Vector2.Zero;
                    if (doesTileHaveProperty((int)(value.X + value2.X), (int)(value.Y + value2.Y), "Diggable", "Back") != null && (doesTileHaveProperty((int)(value.X + value2.X), (int)(value.Y + value2.Y), "Type", "Back") == null || !doesTileHaveProperty((int)(value.X + value2.X), (int)(value.Y + value2.Y), "Type", "Back").Equals("Wood")) && (isTileLocationTotallyClearAndPlaceable(value + value2) || (spawnFromOldWeeds && ((objects.ContainsKey(value + value2) && objects[value + value2].parentSheetIndex != 105) || (terrainFeatures.ContainsKey(value + value2) && (terrainFeatures[value + value2] is HoeDirt || terrainFeatures[value + value2] is Flooring))))) && doesTileHaveProperty((int)(value.X + value2.X), (int)(value.Y + value2.Y), "NoSpawn", "Back") == null && (spawnFromOldWeeds || !objects.ContainsKey(value + value2)))
                    {
                        int num2 = -1;
                        
                        if (Game1.random.NextDouble() < 0.5 && !weedsOnly && (!spawnFromOldWeeds || keyValuePair.Value.Name.Equals("Stone") || keyValuePair.Value.Name.Contains("Twig")))
                        {
                            if (Game1.random.NextDouble() < 0.5)
                            {
                                num2 = ((Game1.random.NextDouble() < 0.5) ? 294 : 295);
                            }
                            else
                            {
                                num2 = ((Game1.random.NextDouble() < 0.5) ? 343 : 450);
                            }
                        }
                        else if (!spawnFromOldWeeds || keyValuePair.Value.Name.Contains("Weed"))
                        {
                            num2 = getWeedForSeason(Game1.random, Game1.currentSeason);
                        }
                        if (!spawnFromOldWeeds && Game1.random.NextDouble() < 0.05)
                        {
                            terrainFeatures.Add(value + value2, new Tree(Game1.random.Next(3) + 1, Game1.random.Next(3)));
                            continue;
                        }
                        if (num2 != -1)
                        {
                            bool flag2 = false;
                            if (objects.ContainsKey(value + value2))
                            {
                                StardewValley.Object @object = objects[value + value2];
                                if (@object is Fence || @object is Chest)
                                {
                                    continue;
                                }
                                if (@object.name != null && !@object.Name.Contains("Weed") && !@object.Name.Equals("Stone") && !@object.name.Contains("Twig") && @object.name.Count() > 0)
                                {
                                    flag2 = true;
                                }
                                objects.Remove(value + value2);
                            }
                            else if (terrainFeatures.ContainsKey(value + value2))
                            {
                                try
                                {
                                    flag2 = (terrainFeatures[value + value2] is HoeDirt || terrainFeatures[value + value2] is Flooring);
                                }
                                catch (Exception)
                                {
                                }
                                if (!flag2)
                                {
                                    return;
                                }
                                terrainFeatures.Remove(value + value2);
                            }
                            if (flag2)
                            {
                                Game1.showGlobalMessage("The spreading weeds have caused damage to your farm.");
                            }
                            objects.Add(value + value2, new StardewValley.Object(value + value2, num2, 1));
                        }
                    }
                }
            }
        }

        new public void spawnWeeds(bool weedsOnly)
        {
            int num = Game1.random.Next(5, 12);
            if (Game1.dayOfMonth == 1 && Game1.currentSeason.Equals("spring"))
            {
                num *= 15;
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int num2 = Game1.random.Next(map.DisplayWidth / Game1.tileSize);
                    int num3 = Game1.random.Next(map.DisplayHeight / Game1.tileSize);
                    Vector2 vector = new Vector2(num2, num3);
                    StardewValley.Object @object;
                    objects.TryGetValue(vector, out @object);
                    int num4 = -1;
                    int num5 = -1;
                    if (Game1.random.NextDouble() < 0.15 + (weedsOnly ? 0.05 : 0.0))
                    {
                        num4 = 1;
                    }
                    else if (!weedsOnly && Game1.random.NextDouble() < 0.35)
                    {
                        num5 = 1;
                    }
                    if (num5 != -1)
                    {
                        if (Game1.random.NextDouble() < 0.25)
                        {
                            return;
                        }
                    }
                    else if (@object == null && doesTileHaveProperty(num2, num3, "Diggable", "Back") != null && isTileLocationOpen(new Location(num2 * Game1.tileSize, num3 * Game1.tileSize)) && !isTileOccupied(vector, "") && doesTileHaveProperty(num2, num3, "Water", "Back") == null)
                    {
                        string text = doesTileHaveProperty(num2, num3, "NoSpawn", "Back");
                        if (text != null && (text.Equals("Grass") || text.Equals("All")))
                        {
                            continue;
                        }
                        if (num4 != -1 && !Game1.currentSeason.Equals("winter"))
                        {
                            int numberOfWeeds = Game1.random.Next(1, 3);
                            terrainFeatures.Add(vector, new Grass(num4, numberOfWeeds));
                        }
                    }
                }
            }
        }

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            return base.checkAction(tileLocation, viewport, who);
        }

        public override bool leftClick(int x, int y, Farmer who)
        {
            return base.leftClick(x, y, who);
        }

        public override void resetForPlayerEntry()
        {
            base.resetForPlayerEntry();
        }

        public override void draw(SpriteBatch b)
        {
            foreach (ResourceClump current in resourceClumps)
            {
                current.draw(b, current.tile);
            }
            foreach (KeyValuePair<long, FarmAnimal> current2 in animals)
            {
                current2.Value.draw(b);
            }
            foreach (Building current in buildings)
            {
                current.draw(b);
            }
            if (!Game1.eventUp)
            {
                for (int i = 0; i < characters.Count; i++)
                {
                    if (characters[i] != null)
                    {
                        characters[i].draw(b);
                    }
                }
            }
            for (int j = 0; j < projectiles.Count(); j++)
            {
                projectiles[j].draw(b);
            }
            for (int k = 0; k < farmers.Count(); k++)
            {
                if (!farmers[k].uniqueMultiplayerID.Equals(Game1.player.uniqueMultiplayerID))
                {
                    farmers[k].draw(b);
                }
            }
            if (critters != null)
            {
                for (int l = 0; l < critters.Count(); l++)
                {
                    critters[l].draw(b);
                }
            }
            drawDebris(b);
            foreach (KeyValuePair<Vector2, StardewValley.Object> current in objects)
            {
                current.Value.draw(b, (int)current.Key.X, (int)current.Key.Y, 1f);
            }
            foreach (TemporaryAnimatedSprite current2 in TemporarySprites)
            {
                current2.draw(b, false, 0, 0);
            }
            if (doorSprites != null)
            {
                foreach (TemporaryAnimatedSprite current3 in doorSprites.Values)
                {
                    current3.draw(b, false, 0, 0);
                }
            }
            if (largeTerrainFeatures != null)
            {
                foreach (LargeTerrainFeature current4 in largeTerrainFeatures)
                {
                    current4.draw(b);
                }
            }
            if (fishSplashAnimation != null)
            {
                fishSplashAnimation.draw(b, false, 0, 0);
            }
            if (orePanAnimation != null)
            {
                orePanAnimation.draw(b, false, 0, 0);
            }
        }

        public override void performTenMinuteUpdate(int timeOfDay)
        {
            base.performTenMinuteUpdate(timeOfDay);
            timeUpdate(10);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            if (wasUpdated && Game1.gameMode != 0)
            {
                return;
            }
            base.UpdateWhenCurrentLocation(time);
            fixAnimalLocation();
        }

        public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
            fixAnimalLocation();
        }

        public void fixAnimalLocation()
        {
            foreach (FarmAnimal animal in Game1.getFarm().animals.Values.ToList())
            {
                if (buildings.Contains(animal.home))
                {
                    Game1.getFarm().animals.Remove(animal.myID);
                    animals.Add(animal.myID, animal);
                }
            }
            foreach (Building building in buildings)
            {
                if (building.indoors == null)
                    continue;
                
                if (building.indoors is AnimalHouse)
                {
                    List<FarmAnimal> stuckAnimals = new List<FarmAnimal>();
                    foreach (FarmAnimal animal in ((AnimalHouse)building.indoors).animals.Values)
                    {
                        if (Game1.getFarm().isCollidingPosition(new Microsoft.Xna.Framework.Rectangle((building.tileX + building.animalDoor.X) * Game1.tileSize + 2, (building.tileY + building.animalDoor.Y) * Game1.tileSize + 2, (animal.isCoopDweller() ? Game1.tileSize : (Game1.tileSize * 2)) - 4, Game1.tileSize - 4), Game1.viewport, false, 0, false, animal, false, false, false) || Game1.getFarm().isCollidingPosition(new Microsoft.Xna.Framework.Rectangle((building.tileX + building.animalDoor.X) * Game1.tileSize + 2, (building.tileY + building.animalDoor.Y + 1) * Game1.tileSize + 2, (animal.isCoopDweller() ? Game1.tileSize : (Game1.tileSize * 2)) - 4, Game1.tileSize - 4), Game1.viewport, false, 0, false, animal, false, false, false))
                        {
                            if (Game1.random.NextDouble() < 0.002 && building.animalDoorOpen && Game1.timeOfDay < 1630 && !Game1.isRaining && !Game1.currentSeason.Equals("winter") && building.indoors.getFarmers().Count() == 0 && !building.indoors.Equals(Game1.currentLocation))
                            {
                                if (isCollidingPosition(new Microsoft.Xna.Framework.Rectangle((building.tileX + building.animalDoor.X) * Game1.tileSize + 2, (building.tileY + building.animalDoor.Y) * Game1.tileSize + 2, (animal.isCoopDweller() ? Game1.tileSize : (Game1.tileSize * 2)) - 4, Game1.tileSize - 4), Game1.viewport, false, 0, false, animal, false, false, false) || isCollidingPosition(new Microsoft.Xna.Framework.Rectangle((building.tileX + building.animalDoor.X) * Game1.tileSize + 2, (building.tileY + building.animalDoor.Y + 1) * Game1.tileSize + 2, (animal.isCoopDweller() ? Game1.tileSize : (Game1.tileSize * 2)) - 4, Game1.tileSize - 4), Game1.viewport, false, 0, false, animal, false, false, false))
                                {
                                    break;
                                }
                                else
                                {
                                    stuckAnimals.Add(animal);
                                }
                            }
                        }
                    }
                    foreach (FarmAnimal localBuildingAnimal in stuckAnimals)
                    {
                        if (localBuildingAnimal.noWarpTimer <= 0)
                        {
                            localBuildingAnimal.noWarpTimer = 3000;
                            ((AnimalHouse)building.indoors).animals.Remove(localBuildingAnimal.myID);
                            building.currentOccupants--;
                            animals.Add(localBuildingAnimal.myID, localBuildingAnimal);
                            localBuildingAnimal.faceDirection(2);
                            localBuildingAnimal.SetMovingDown(true);
                            localBuildingAnimal.position = new Vector2(building.getRectForAnimalDoor().X, (building.tileY + building.animalDoor.Y) * Game1.tileSize - (localBuildingAnimal.sprite.getHeight() * Game1.pixelZoom - localBuildingAnimal.GetBoundingBox().Height) + Game1.tileSize / 2);
                        }
                    }
                }
            }
            foreach (FarmAnimal localAnimal in animals.Values.ToList()) // Gets animals back inside
            {
                if (localAnimal.noWarpTimer <= 0 && localAnimal.home != null && localAnimal.home.getRectForAnimalDoor().Contains(localAnimal.GetBoundingBox().Center.X, localAnimal.GetBoundingBox().Top))
                {
                    if (Utility.isOnScreen(localAnimal.getTileLocationPoint(), Game1.tileSize * 3, this))
                    {
                        Game1.playSound("dwoop");
                    }
                    //localAnimal.noWarpTimer = 3000;
                    localAnimal.home.currentOccupants++;
                    animals.Remove(localAnimal.myID);
                    ((AnimalHouse)localAnimal.home.indoors).animals.Add(localAnimal.myID, localAnimal);

                    localAnimal.setRandomPosition(localAnimal.home.indoors);
                    localAnimal.faceDirection(Game1.random.Next(4));
                    localAnimal.controller = null;
                }
            }
        }
    }

}