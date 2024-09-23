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
using ReLogic.Content;
using Terraria.Audio;

namespace TerRoguelike.Managers
{
    public class TextureManager
    {
        public static bool TexturesLoaded = false;
        public static Dictionary<string, Texture2D> TexDict = [];
        public static Dictionary<string, Asset<Texture2D>> TexAssetDict = [];
        internal static void Load()
        {
            List<string> pathList = new List<string>()
            {
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
                "TerRoguelike/NPCs/Enemy/StoneDroneGlow",
                "TerRoguelike/Projectiles/PaladinHammer",
                "TerRoguelike/Particles/Spark",
                "TerRoguelike/Particles/ThinSpark",
                "TerRoguelike/Particles/Square",
                "TerRoguelike/Particles/AnimatedSmoke",
                "TerRoguelike/ExtraTextures/GodRay",
                "TerRoguelike/ExtraTextures/TallFire",
                "TerRoguelike/NPCs/Enemy/Boss/BrambleHollowGlow",
                "TerRoguelike/Projectiles/LeafBall",
                "TerRoguelike/NPCs/Enemy/Boss/CorruptionParasiteHead",
                "TerRoguelike/NPCs/Enemy/Boss/CorruptionParasiteBody",
                "TerRoguelike/NPCs/Enemy/Boss/CorruptionParasiteTail",
                "TerRoguelike/NPCs/Enemy/Boss/ParasiticEgg",
                "TerRoguelike/ExtraTextures/YellowArrow",
                "TerRoguelike/NPCs/Enemy/Boss/IceQueenGlow",
                "TerRoguelike/Projectiles/Snowflake",
                "TerRoguelike/ExtraTextures/StarrySky",
                "TerRoguelike/NPCs/Enemy/Boss/PharaohSpiritGlow",
                "TerRoguelike/Projectiles/SandnadoLight",
                "TerRoguelike/ExtraTextures/Crust",
                "TerRoguelike/NPCs/Enemy/Boss/WallOfFleshBody",
                "TerRoguelike/NPCs/Enemy/Boss/WallOfFleshEye",
                "TerRoguelike/NPCs/Enemy/Boss/WallOfFleshMouth",
                "TerRoguelike/ExtraTextures/HellBeamWave",
                "TerRoguelike/ExtraTextures/BlobbyNoise",
                "TerRoguelike/ExtraTextures/BlobbyNoiseSmall",
                "TerRoguelike/ExtraTextures/CurvedSpike",
                "TerRoguelike/NPCs/Enemy/Boss/HungryTether",
                "TerRoguelike/NPCs/Enemy/Boss/SkeletronEye",
                "TerRoguelike/Projectiles/BoneSpearTip",
                "TerRoguelike/NPCs/Enemy/Boss/TempleGolemEyes",
                "TerRoguelike/NPCs/Enemy/Boss/TempleGolemGlow",
                "TerRoguelike/Projectiles/RockDebris",
                "TerRoguelike/Tiles/ItemBasinGlow",
                "TerRoguelike/Tiles/ItemBasin_Highlight",
                "TerRoguelike/UI/Random",
                "TerRoguelike/UI/NoCircle",
                "TerRoguelike/UI/BasinOptionBox",
                "TerRoguelike/UI/BasinOptionBoxHover",
                "TerRoguelike/UI/BasinOptionsBackground",
                "TerRoguelike/ExtraTextures/BigArrow",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordCore",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordCoreCracks",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordEmptyEye",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordInnerEye",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordLowerArm",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordUpperArm",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordMouth",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordSideEye",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordTopEye",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordTopEyeOverlay",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordHead",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordHand",
                "TerRoguelike/NPCs/Enemy/Boss/MoonLordBodyHalf",
                "TerRoguelike/NPCs/Enemy/Boss/TrueEyeOfCthulhu",
                "TerRoguelike/Particles/Wriggler",
                "TerRoguelike/ExtraTextures/Circle",
                "TerRoguelike/ExtraTextures/Perlin",
                "TerRoguelike/Particles/Shard",
                "TerRoguelike/NPCs/Enemy/Boss/TrueBrainEye",
                "TerRoguelike/ExtraTextures/MoonDeadExplosion",
                "TerRoguelike/ExtraTextures/MoonDeadHead",
                "TerRoguelike/ExtraTextures/MoonDeadShoulder",
                "TerRoguelike/ExtraTextures/MoonDeadSpine",
                "TerRoguelike/ExtraTextures/MoonDeadTorso",
                "TerRoguelike/ExtraTextures/Moon",
                "TerRoguelike/ExtraTextures/PhantasmalBeamWave",
                "TerRoguelike/NPCs/Enemy/Boss/TrueBrainDeathFrames",
                "TerRoguelike/NPCs/Enemy/Boss/TrueBrainGoreFrames",
                "TerRoguelike/UI/QuestionMark",
                "TerRoguelike/MainMenu/UiMoon",
                "TerRoguelike/TerPlayer/CeremonialCrownGems",
                "TerRoguelike/Projectiles/DisposableTurretMinionHead",
                "TerRoguelike/ExtraTextures/InverseGlow",
                "TerRoguelike/ExtraTextures/Streaks",
            };
            foreach (string path in pathList)
            {
                AddTex(path);
            }
        }
        public static void SetStaticDefaults()
        {
            TexDict.Clear();
            foreach (var asset in TexAssetDict)
            {
                TexDict.Add(asset.Key, asset.Value.Value);
            }
            TexAssetDict = null;
            TexturesLoaded = true;
        }
        internal static void Unload()
        {
            TexDict = null;
            TexturesLoaded = false;
            TexAssetDict = null;
        }
        internal static void AddTex(string path)
        {
            string name = path.Substring(path.LastIndexOf("/") + 1);
            TexAssetDict.Add(name, ModContent.Request<Texture2D>(path, AssetRequestMode.AsyncLoad));

            // this just fills in a placeholder blank texture since loading isn't fully done yet. 
            TexDict.Add(name, TexAssetDict[name].Value);
        }
    }
}
