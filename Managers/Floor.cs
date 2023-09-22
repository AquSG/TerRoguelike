using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.NPCs;
using Terraria.Chat;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Managers
{
    public class Floor
    {
        public virtual int FloorID => 0;
        public virtual int StartRoomID => -1;
        public virtual int Stage => -1;
    }
}
