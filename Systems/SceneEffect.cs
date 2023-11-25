﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Rooms;
using Terraria.WorldBuilding;
using static tModPorter.ProgressUpdate;
using Terraria.GameContent.Generation;
using tModPorter;
using Terraria.Localization;
using TerRoguelike.World;
using TerRoguelike.MainMenu;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ReLogic.Utilities;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Systems
{
    public class SceneEffect : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(TerRoguelike.Instance, "Tracks/Blank");
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override bool IsSceneEffectActive(Player player)
        {
            return TerRoguelikeWorld.IsTerRoguelikeWorld;
        }
    }
}