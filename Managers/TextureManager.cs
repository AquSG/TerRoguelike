using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Items;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using Terraria.ModLoader.Core;
using TerRoguelike.NPCs.Enemy;
using TerRoguelike.NPCs.Enemy.Pillar;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Managers
{
    public class TextureManager
    {
        public static bool TexturesLoaded = false;
        public static Dictionary<string, Texture2D> TexDict = new Dictionary<string, Texture2D>();
        internal static void Load()
        {
            TexturesLoaded = true;
            List<string> pathList = new List<string>()
            {
                "TerRoguelike/NPCs/StillMoonLord",
                "TerRoguelike/Projectiles/VolatileRocket",
                "TerRoguelike/NPCs/Enemy/AntlionHead",
                "TerRoguelike/NPCs/Enemy/BallistaBase",
                "TerRoguelike/UI/BarrierBar",
                "TerRoguelike/UI/BarrierBarBorder",
                "TerRoguelike/NPCs/Enemy/BoneSerpentHead",
                "TerRoguelike/NPCs/Enemy/BoneSerpentBody",
                "TerRoguelike/NPCs/Enemy/BoneSerpentTail",
                "TerRoguelike/NPCs/Enemy/ClingerSegment1",
                "TerRoguelike/NPCs/Enemy/ClingerSegment2",
                "TerRoguelike/Projectiles/SingularClusterBomb",
                "TerRoguelike/Projectiles/Explosion",
                "TerRoguelike/Projectiles/Smoke",
                "TerRoguelike/NPCs/Enemy/CoriteGlow",
                "TerRoguelike/NPCs/Enemy/DaybreakerGlow",
                "TerRoguelike/NPCs/Enemy/DaybreakerArm",
                "TerRoguelike/Projectiles/Daybreak",
                "TerRoguelike/UI/DeathUI",
                "TerRoguelike/UI/MenuButton",
                "TerRoguelike/UI/MenuButtonHover",
                "TerRoguelike/ExtraTextures/CircularGlow",
                "TerRoguelike/ExtraTextures/LerpLineGradient",
                "TerRoguelike/NPCs/Enemy/IcyMermanHead",
                "TerRoguelike/NPCs/Enemy/LihzahrdConstructGlow",
                "TerRoguelike/NPCs/Enemy/LihzahrdSentryGlow",
                "TerRoguelike/TerPlayer/LenaGlow",
                "TerRoguelike/Projectiles/Missile",
                "TerRoguelike/ExtraTextures/CrossGlow",
                "TerRoguelike/Projectiles/PlanRocket",
                "TerRoguelike/Tiles/TemporaryBlock",
                "TerRoguelike/Projectiles/InvisibleProj",
                "TerRoguelike/World/Chain1",
                "TerRoguelike/World/Chain2",
                "TerRoguelike/NPCs/Enemy/SandWormHead",
                "TerRoguelike/NPCs/Enemy/SandWormBody",
                "TerRoguelike/NPCs/Enemy/SandWormTail",
                "TerRoguelike/NPCs/Enemy/SoladileGlow",
                "TerRoguelike/Projectiles/SpikedBallChain",
                "TerRoguelike/Projectiles/TempleBoulderGlow",
                "TerRoguelike/Projectiles/AdaptiveGunBullet",
                "TerRoguelike/MainMenu/XButton",
                "TerRoguelike/Shaders/OverheadWaves",
                "TerRoguelike/TerPlayer/Lena",
                "TerRoguelike/TerPlayer/DroneBuddyMinion",
                "TerRoguelike/Projectiles/HealingFungus",
                "TerRoguelike/Projectiles/SpikedBall",
                "TerRoguelike/NPCs/Enemy/UndeadBruteArm",
                "TerRoguelike/NPCs/Enemy/UndeadSharpshooterGun",
                "TerRoguelike/ExtraTextures/LineGradient",
                "TerRoguelike/NPCs/Enemy/BrainSucklerGlow",
                "TerRoguelike/NPCs/Enemy/OmniwatcherGlow",
                "TerRoguelike/NPCs/Enemy/PredictorGlow",
                "TerRoguelike/NPCs/Enemy/AlienHornetGlow",
                "TerRoguelike/NPCs/Enemy/StormDiverGlow",
                "TerRoguelike/NPCs/Enemy/VortexWatcherGlow",
                "TerRoguelike/ExtraTextures/CrossSpark",
                "TerRoguelike/Projectiles/BlackVortex",
                "TerRoguelike/Projectiles/SeekingStarCellGlow",
                "TerRoguelike/NPCs/Enemy/StarCellGlow",
                "TerRoguelike/NPCs/Enemy/StarSpewerGlow",
                "TerRoguelike/NPCs/Enemy/FlowInvaderGlow",
                "TerRoguelike/Projectiles/Comet",
                "TerRoguelike/Projectiles/FlowSpawnGlow",
            };
            foreach (string path in pathList)
            {
                AddTex(path);
            }
        }
        internal static void Unload()
        {
            TexDict = null;
            TexturesLoaded = false;
        }
        internal static void AddTex(string path)
        {
            string name = path.Substring(path.LastIndexOf("/") + 1);
            TexDict.Add(name, ModContent.Request<Texture2D>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
        }
    }
}
