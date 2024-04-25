using System;
using System.Collections.Generic;

//#forcedebug
//#import <../Libs/stored_data.cs>

namespace RazorEnhanced
{
    internal class LootChest
    {
        private int LOCKPICK_DELAY = 1000;
        private int REMOVE_TRAP_DELAY = 10000;
        private int MOVE_ITEMS_DELAY = 500;

        private readonly StoredData json_storedData = new StoredData();

        public LootChest()
        {
        }

        public void Run()
        {
            Target target = new Target();

            Player.HeadMessage(50, "Target a Chest");

            int chestSerial = target.PromptTarget();
            if (chestSerial == 0)
            {
                Player.HeadMessage(33, "No target selected");
                return;
            }

            Item chest = Items.FindBySerial(chestSerial);
            if (!chest.IsContainer)
            {
                Player.HeadMessage(33, "Target is not a container");
                return;
            }

            if (Distance(Player.Position, chest.GetWorldPosition()) > 1)
            {
                Player.HeadMessage(33, "Chest is too far. You must be closer");
                return;
            }

            bool result = LockPickChest(chestSerial);
            if (result == false)
            {
                Player.HeadMessage(33, "You failed to lockpick");
                return;
            };

            Misc.Pause(1000);

            result = RemoveTrap(chestSerial);
            if (result == false)
            {
                Player.HeadMessage(33, "You failed to remove the trap");
                return;
            };

            Misc.Pause(1000);
            LootTheChest(chest);
        }

        private static int Distance(Point3D from, Point3D to)
        {
            int xDelta = Math.Abs(from.X - to.X);
            int yDelta = Math.Abs(from.Y - to.Y);

            return (xDelta > yDelta ? xDelta : yDelta);
        }

        private bool LockPickChest(int chestSerial)
        {
            Item lockpick = GetLockpickFromConfig();
            if ( lockpick == null )
            {
                Player.HeadMessage(33, "Unable to find a valid lockpick");
                return false;
            }

            double skill = Player.GetSkillValue("Lockpicking");
            if (skill < 100)
            {
                Player.HeadMessage(33, "Less than 100 in lockpicking, better to train the skill");
            }

            Journal journal = new Journal();
            Player.HeadMessage(50, "Lockpicking the chest");
            for (int i = 10; i >= 0; --i)
            {
                journal.Clear();
                Items.UseItem(lockpick.Serial);
                Target.WaitForTarget(2000); 
                Target.TargetExecute(chestSerial);
                Misc.Pause(LOCKPICK_DELAY);
                if (journal.Search("The lock quickly yields to your skill"))
                {
                    return true;
                }
                if (journal.Search("This does not appear to be locked"))
                {
                    return true;
                }
                Player.HeadMessage(50, $"Failed to lockpick, retrying still {i} times.");
            }
            return false;
        }

        private Item GetLockpickFromConfig()
        {
            int lockpickSerial = json_storedData.GetData<int>("LootChest.LockpickSerial", StoredData.StoreType.Character);
            Item lockpick = Items.FindBySerial(lockpickSerial);
            if (lockpick == null || lockpick.ItemID != 0x14FC)
            {
                Player.HeadMessage(33, "Select a lockpick");
                lockpick = Items.FindBySerial(new Target().PromptTarget());
                if (lockpick == null || lockpick.ItemID != 0x14FC) 
                {
                    Player.HeadMessage(33, "Invalid lockpick selected");
                    return null;
                }
                else
                {
                    json_storedData.StoreData(lockpick.Serial, "LootChest.LockpickSerial", StoredData.StoreType.Character);
                }
            }
            return lockpick;
        }

        private bool RemoveTrap(int chestSerial)
        {
            double skill = Player.GetSkillValue("Remove trap");
            if (skill < 100)
            {
                Misc.SendMessage("Less than 100 in Remove Trap, better to train the skill", 33);
            }

            Journal journal = new Journal();

            for(int i = 10; i >= 0; --i)
            {
                journal.Clear();
                Player.UseSkill("Remove Trap");
                Target.WaitForTarget(2000);
                Target.TargetExecute(chestSerial);
                Misc.Pause(1000);

                if (journal.Search("You successfully render the trap harmless"))
                {
                    return true;
                }
                if (journal.Search("That doesn't appear to be trapped"))
                {
                    return true;
                }
                if (journal.Search("That is locked"))
                {
                    return false;
                }

                Misc.SendMessage($"Failed to remove the trap, retrying still {i} times.", 33);
                Misc.Pause(REMOVE_TRAP_DELAY);
            }

            return false;
        }

        private void LootTheChest(Item chest)
        {
            List<int> lootList = new List<int>() {
                0x0EED, // Gold
                0x0F10, // Emerald
                0x0F13, // Ruby
                0x0F15, // Citrine
                0x0F16, // Amethyst
                0x0F11, 0x0F19, // Sapphire
                0x0F0F, 0x0F21, // Star sapphire
                0x0F25, // Amber
                0x0F26, // Diamond
                0x0F18, 0x0F2D, // Tourmaline
            };

            int lootBagSerial = json_storedData.GetData<int>("LootChest.LootBagSerial", StoredData.StoreType.Character);
            if (lootBagSerial == 0)
            {
                Player.HeadMessage(33, "Select a loot bag");
                Item lootBag = Items.FindBySerial(new Target().PromptTarget());
                lootBagSerial = lootBag.Serial;
                if (lootBag == null || !lootBag.IsContainer)
                {
                    Player.HeadMessage(33, "Invalid loot bag selected. Using Backpack this time");
                    lootBagSerial = Player.Backpack.Serial;
                } 
                else
                {
                    json_storedData.StoreData(lootBagSerial, "LootChest.LootBagSerial", StoredData.StoreType.Character);
                }
            }

            Items.UseItem(chest);
            Misc.Pause(1000);

            foreach (var loot in chest.Contains)
            {
                if (lootList.Contains(loot.ItemID))
                {
                    Player.HeadMessage(33, "Looting: " + loot.Name);
                    Items.Move(loot.Serial, lootBagSerial, 0);
                    Misc.Pause(MOVE_ITEMS_DELAY);
                }
            }
        }
    }
}