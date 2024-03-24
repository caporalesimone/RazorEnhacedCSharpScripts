using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

//#forcedebug

namespace RazorEnhanced
{
    internal class PlantsPicker
    {
        private const int HOME_RUNE = 0;
        private const int MAX_RUNE = 5;
        private const int START_FROM_RUNE_NUMBER = 1;
        private const int FIND_RADIUS = 35;

        private int SERIAL_RUNEBOOK = 0; // If 0 disable recall
        private int SERIAL_CONTAINER = 0x417BE7A9;

        private List<int> PLANTS = new List<int>()
        { 
            0x0C51, 0x0C52, 0x0C53, 0x0C54, // Cotton
            0x1A99, 0x1A9A, 0x1A9B,         // Flax
         // 0x0C5E, 0x0C5F, 0x0C60, // Vines but this plant does not change graphics so this script remains blocked
            
            0x0C61, 0x0C62, 0x0C63, // Turnip
            0x0C6F, // Onions
            0x0C76, // Carrots
         
         // 0x0C55, 0x0C56, 0x0C57, 0x0C58, // Wheat
         };

        private List<int> RESOURCES = new List<int>()
        {
                    0x0DF9, // Bale of cotton
            0x1A9C, 0x1A9D, // Flax bundle
            
            0x0D39, 0x0D3A, // Turnip
            0x0C6B,         // Pumpkin from vines
            0x0C6D, 0x0C6E, // Onion
            0x0C77, 0x0C78, // Carrots
            
            0x1EBD, // Wheat
        };

        public PlantsPicker()
        {
        }

        public void Run ()
        {
            int rune = START_FROM_RUNE_NUMBER; // Rune 0 is home
            

            while(true)
            {
                Misc.Pause(10);
                Recall(rune);

                bool harvestResult = HarvestTheField();
                
                if (!harvestResult) GoBackHome();
                
                if (rune++ >= MAX_RUNE)
                {
                    GoBackHome();
                    rune = START_FROM_RUNE_NUMBER; // Restart from the begin
                }

                if (Player.Weight > Player.MaxWeight - 5)
                {
                    Player.HeadMessage(33, "Overloaded");
                    if (SERIAL_RUNEBOOK != 0)
                    {
                        GoBackHome();
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }

        private List<Item> FindItemOnGround(int range, List<int> graphics)
        {
            var findFilter = new Items.Filter()
            {
                Enabled = true,
                RangeMax = range,
                Graphics = graphics
            };

            var foundItems = Items.ApplyFilter(findFilter);
            if (foundItems != null || foundItems.Count > 0)
            {
                // Sort by distance
                foundItems.Sort((item1, item2) =>
                {
                    double distance1 = Misc.Distance(item1.Position.X, item1.Position.Y, Player.Position.X, Player.Position.Y);
                    double distance2 = Misc.Distance(item2.Position.X, item2.Position.Y, Player.Position.X, Player.Position.Y);

                    return distance1.CompareTo(distance2);
                });

                return foundItems;
            }

            return new List<Item>();
        }

        private void MoveToPlant(Item plant)
        {
            if (plant == null) return;

            if (Player.Position.Equals(plant.Position)) return; // This happen if 2 plants are in the same tile (Server bug?)

            Player.PathFindTo(plant.Position);

            int safeExit = 200;

            while (true)
            {
                if (safeExit-- <= 0)
                {
                    Player.HeadMessage(33, "Safe EXIT during pathfinding");
                    break;
                }

                double dist = Misc.Distance(Player.Position.X, Player.Position.Y, plant.Position.X, plant.Position.Y);
                if (dist <= 1) break;

                Misc.Pause(10);
            }
        }

        private void HarvestPlant(Item plant)
        {
            Items.Message(plant.Serial, 33, ">.<");
            Items.UseItem(plant);
            Misc.Pause(700);
            List<Item> harvest = FindItemOnGround(FIND_RADIUS, RESOURCES);
            foreach (Item harvestItem in harvest)
            {
                Items.Move(harvestItem, Player.Backpack, -1);
                Misc.Pause(700);
            }
        }

        private bool HarvestTheField()
        {
            List<Item> groundPlants = FindItemOnGround(FIND_RADIUS, PLANTS);
            
            while (groundPlants.Count != 0)
            {
                Player.HeadMessage(25, $"Harvesting");

                Item plant = groundPlants.ElementAt(0);
                MoveToPlant(plant);
                HarvestPlant(plant);

                if ((Player.Weight > Player.MaxWeight - 20) || (Player.Mana < 20)) return false;

                // Search Again
                groundPlants = FindItemOnGround(FIND_RADIUS, PLANTS);
            }
            Player.HeadMessage(25, "All plants harvested");
            return true;
        }

        private void Recall(int runePosition )
        {
            if (SERIAL_RUNEBOOK == 0) {
                Misc.Pause(500);
                return;
            }
            Items.UseItem(SERIAL_RUNEBOOK);
            Gumps.WaitForGump(0, 20000);
            uint gump = Gumps.CurrentGump();
            Misc.Pause(50);
            Gumps.SendAction(gump, 5 + runePosition * 6);
            Misc.Pause(3500);
        }

        private void UnloadResources()
        {
            Player.HeadMessage(33, "Moving all the harvest");
            foreach (var resource in RESOURCES)
            {
                Item item = Items.FindByID(resource, 0, Player.Backpack.Serial);
                if (item != null) 
                {
                    Items.UseItem(SERIAL_CONTAINER);
                    Misc.Pause(700);
                    Items.Move(item, SERIAL_CONTAINER, item.Amount);
                    Misc.Pause(700);
                }
            }
            Misc.Pause(1000);
        }

        private void GoBackHome()
        {
            if (SERIAL_RUNEBOOK == 0) {
                Misc.Pause(500);
                return;
            }
            
            Recall(HOME_RUNE);
            UnloadResources();

            while(Player.Mana < Player.ManaMax)
            {
                Player.HeadMessage(33, $"Resting - Mana: {Player.Mana}/{Player.ManaMax}");
                Misc.Pause(2000);
            }
        }
    }
}
