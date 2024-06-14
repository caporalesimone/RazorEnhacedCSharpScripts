using System.Collections.Generic;
using System.Linq;

namespace RazorEnhanced
{
    internal class Scroll_Sorter
    {
        public Scroll_Sorter()
        {
            // This is a constructor
        }

        public void Run()
        {

            var Circle1 = new List<int> {
                0x1f2d, // Reactive Armor
                0x1f2e, // Clumsy
                0x1f2f, // Create Food
                0x1f30, // Feeblemind
                0x1f31, // Heal
                0x1f32, // Magic Arrow
                0x1f33, // Night Sight
                0x1f34, // Weaken
            };
            var Circle2 = new List<int> {
                0x1f35, // Agility
                0x1f36, // Cunning
                0x1f37, // Cure
                0x1f38, // Harm
                0x1f39, // Magic Trap
                0x1f3a, // Magic Untrap
                0x1f3b, // Protection
                0x1f3c, // Strength
            };
            var Circle3 = new List<int>
            {
                0x1f3d, // Bless
                0x1f3e, // Fireball
                0x1f3f, // Magic Lock
                0x1f40, // Poison
                0x1f41, // Telekinesis
                0x1f42, // Teleport
                0x1f43, // Unlock
                0x1f44, // Wall of Stone
            };
            var Circle4 = new List<int>
            {
                0x1f45, // Arch Cure
                0x1f46, // Arch Protection
                0x1f47, // Curse
                0x1f48, // Fire Field
                0x1f49, // Greater Heal
                0x1f4a, // Lightning
                0x1f4b, // Mana Drain
                0x1f4c, // Recall
            };
            var Circle5 = new List<int>
            {
                0x1f4d, // Blade Spirit
                0x1f4e, // Dispel Field
                0x1f4f, // Incognito
                0x1f50, // Magic Reflection
                0x1f51, // Mind Blast
                0x1f52, // Paralyze
                0x1f53, // Poison Field
                0x1f54, // Summon Creature
            };
            var Circle6 = new List<int>
            {
                0x1f55, // Dispel
                0x1f56, // Energy Bolt
                0x1f57, // Explosion
                0x1f58, // Invisibility
                0x1f59, // Mark
                0x1f5a, // Mass Curse
                0x1f5b, // Paralyze Field
                0x1f5c, // Reveal
            };
            var Circle7 = new List<int>
            {
                0x1f5d, // Chain Lightning
                0x1f5e, // Energy Field
                0x1f5f, // Flamestrike
                0x1f60, // Gate Travel
                0x1f61, // Mana Vampire
                0x1f62, // Mass Dispel
                0x1f63, // Meteor Swarm
                0x1f64, // Polymorph
            };
            var Circle8 = new List<int>
            {
                0x1f65, // Earthquake
                0x1f66, // Energy Vortex
                0x1f67, // Ressurrection
                0x1f68, // Summon Air Elemental
                0x1f69, // Summon Daemon
                0x1f6a, // Summon Earth Elemental
                0x1f6b, // Summon Fire Elemental
                0x1f6c, // Summon Water Elemental
            };


            Dictionary<int, List<int>> database = new()
                {
                    // Container Serial, [array of scroll IDs]
                    { 0x407FD886, Circle1 },
                    { 0x413D585B, Circle2 },
                    { 0x413C1B1C, Circle3 },
                    { 0x413C1B1B, Circle4 },
                    { 0x410FE465, Circle5 },
                    { 0x410FE466, Circle6 },
                    { 0x40FAE1BC, Circle7 },
                    { 0x40FAE1BB, Circle8 },
                };

            Target tgt = new();
            Item container = Items.FindBySerial(tgt.PromptTarget("Select a container with scrolls"));

            if (container == null)
            {
                Player.HeadMessage(33, "Invalid container");
                return;
            }

            foreach (var itm in container.Contains)
            {
                if (itm == null)
                    continue;

                int scrollID = itm.ItemID;
                int ContainerSerial = database.FirstOrDefault(x => x.Value.Contains(scrollID)).Key;

                if (ContainerSerial == 0)
                {
                    continue;
                }

                if (container.Serial == ContainerSerial)
                    continue;

                int ScrollCircle = database.Keys.ToList().IndexOf(ContainerSerial) + 1;
                Player.HeadMessage(33, $"{itm.Name} cirlce {ScrollCircle}");
                Items.Move(itm, ContainerSerial, 0);
                Misc.Pause(500);
            }
        }
    }
}
