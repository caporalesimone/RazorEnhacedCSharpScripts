using System;
using System.Collections.Generic;

//#forcedebug
//#import <../Libs/stored_data.cs>
//#import <../Libs/loot_container.cs>

namespace RazorEnhanced
{
    internal class LockpickChest
    {
        private const int LOCKPICK_DELAY = 1000;
        private const int REMOVE_TRAP_DELAY = 10000;
        private const int MOVE_ITEMS_DELAY = 600;
        private const int LOCKPICK_TENTATIVES = 3;
        private const int REMOVE_TRAP_TENTATIVES = 3;

        private readonly StoredData json_storedData = new StoredData();

        public LockpickChest()
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

            Misc.Pause(300);

            if (chest.Properties.Count <= 1)
            {
                bool lockpicked = LockPickChest(chestSerial);
                if (lockpicked == false)
                {
                    Player.HeadMessage(33, "You failed to lockpick");
                    return;
                };
                Misc.Pause(1500);
            }

            bool trapRemoved = RemoveTrap(chestSerial);
            if (trapRemoved == false)
            {
                Player.HeadMessage(33, "You failed to remove the trap");
                return;
            };

            Misc.Pause(1000);

            LootContainer loot = new LootContainer();
            loot.Loot(chestSerial);
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
                Misc.SendMessage("Less than 100 in lockpicking, better to train the skill", 33);
            }

            Journal journal = new Journal();
            Player.HeadMessage(50, "Lockpicking the chest");
            for (int i = LOCKPICK_TENTATIVES; i > 0; --i)
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

            for(int i = REMOVE_TRAP_TENTATIVES; i > 0; --i)
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
    }
}