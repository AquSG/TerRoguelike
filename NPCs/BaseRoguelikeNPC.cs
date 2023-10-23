using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerRoguelike;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerRoguelike.NPCs
{
    public abstract class BaseRoguelikeNPC : ModNPC
    {
        public virtual int modNPCID => -1; // the ModContent.NPCType<> of this item, used for collecting it's default information on the fly
        public virtual List<int> associatedFloors { get; set; }
        public virtual TerRoguelikeGlobalNPC modNPC 
        {
            get { return NPC.GetGlobalNPC<TerRoguelikeGlobalNPC>(); }
        }
        public virtual Vector2 DrawCenterOffset => Vector2.Zero;
        public override void SetDefaults()
        {
            NPC.value = 0f;
        }
    }
}
