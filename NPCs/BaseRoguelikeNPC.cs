using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerRoguelike.NPCs
{
    public abstract class BaseRoguelikeNPC : ModNPC
    {
        public virtual int modNPCID => -1; // the ModContent.NPCType<> of this item, used for collecting it's default information on the fly
        public virtual List<int> associatedFloors { get; set; }
        public virtual bool ignoreRoomWallCollision => false;
        public virtual TerRoguelikeGlobalNPC modNPC 
        {
            get { return NPC.GetGlobalNPC<TerRoguelikeGlobalNPC>(); }
        }
        public virtual int CombatStyle => -1; // Used for putting enemies into categories for spawning. -1: None (ignored), 0: Melee, 1: Ranged, 2: Hybrid
        public override void SetDefaults()
        {
            NPC.value = 0f;
        }
        public override bool CheckActive()
        {
            return false;
        }
    }
    public class Attack
    {
        public int Id;
        public int Weight;
        public int Duration;

        public Attack(int id, int weight, int duration)
        {
            Id = id;
            Weight = weight;
            Duration = duration;
        }
    }
}
