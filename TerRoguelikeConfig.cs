using Terraria.ModLoader;
using TerRoguelike.Schematics;
using static TerRoguelike.Managers.SpawnManager;
using TerRoguelike.Managers;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.UI;
using Terraria;
using TerRoguelike.Shaders;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using System;

namespace TerRoguelike
{
	public class TerRoguelikeConfig : ModConfig
	{
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public override void OnChanged()
        {
            for (int i = 0; i <= 10; i++)
            {
                if (Math.Abs(ScreenshakeIntensity - i) < 0.065f)
                {
                    ScreenshakeIntensity = i;
                    break;
                }
            }
        }

        [Header("Mods.TerRoguelike.VisualConfig")]
        [DefaultValue(true)]
        public bool EnemyLocationArrow;
        [DefaultValue(true)]
        public bool ObjectiveLocationArrow;
        [DefaultValue(true)]
        public bool BossHealthbar;

        [DefaultValue(1f)]
        [SliderColor(50, 165, 220, 128)]
        [Range(0f, 10f)]
        public float ScreenshakeIntensity;

        [Header("Mods.TerRoguelike.MiscConfig")]
        [DefaultValue(true)]
        public bool FullyDeletePlayerAndWorldFiles;
        [DefaultValue(true)]
        public bool TileFramingOptimization;
    }
}