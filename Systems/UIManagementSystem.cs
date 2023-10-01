using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using TerRoguelike.Managers;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using Terraria.ID;
using TerRoguelike.Systems;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria.UI;
using TerRoguelike.UI;

namespace TerRoguelike.Systems
{
    public class UIManagementSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseIndex == -1)
                return;

            layers.Insert(mouseIndex, new LegacyGameInterfaceLayer("Barrier Bar", () =>
            {
                BarrierUI.Draw(Main.spriteBatch, Main.LocalPlayer);
                return true;
            }, InterfaceScaleType.None));
        }
    }
}
