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

namespace TerRoguelike.Managers
{
    public class NPCManager
    {
        internal static void Load()
        {
            AllNPCs = new List<BaseRoguelikeNPC>()
            {
                new Splinter(),
                new Spookrow(),
                new UndeadGuard(),
                new SandWorm(),
                new GiantBat(),
                new IcyMerman(),
                new SolarPillar(),
                new NebulaPillar(),
                new StardustPillar(),
                new VortexPillar(),
                new IceTortoise(),
                new Frostbiter(),
                new UndeadViking(),
                new DesertSpirit(),
                new AntlionCharger(),
                new Tortoise(),
                new Antlion(),
                new Lamia(),
                new AntlionSwarmer(),
                new Hornet(),
                new SpectreMaster(),
                new Diabolist(),
                new UndeadSharpshooter(),
                new DungeonSpirit(),
                new UndeadAssasin(),
                new UndeadBrute(),
                new CrimsonDreg(),
                new Ballista(),
                new WrathfulRoot(),
                new IchorSticker(),
                new BloodCrawler(),
                new GiantSpider(),
                new JungleCreeper(),
                new BlackRecluse(),
                new CrawlingSludge(),
                new UndeadPrisoner(),
                new StoneDrone(),
                new UndeadEnforcer(),
                new Tumbletwig(),
                new RockGolem(),
                new SeedLobber(),
                new CursedSlime(),
                new IchorSlime(),
                new LavaSlime(),
                new ShadowCaster(),
                new Corruptor(),
                new Crimator(),
                new Clinger(),
                new BloodthirstyAxe(),
                new IceSpirit(),
                new LavaBat(),
                new BoneSerpent(),
                new FireImp(),
                new Demon(),
                new EliteDemon(),
                new GiantMoth(),
                new OvergrownBat(),
                new UndeadHunter(),
                new FlyingSnake(),
                new Lihzahrd(),
                new DomesticatedHornet(),
                new LihzahrdSentry(),
                new TempleDevotee(),
                new LihzahrdConstruct(),
                new Corite(),
                new Daybreaker(),
                new Soladile(),
                new BrainSuckler(),
                new Omniwatcher(),
                new Predictor(),
                new AlienHornet(),
                new StormDiver(),
                new VortexWatcher(),
                new StarCell(),
                new StarSpewer(),
            };
        }
        internal static void Unload()
        {
            AllNPCs = null;
        }
        public static List<BaseRoguelikeNPC> AllNPCs;
        /// <summary>
        /// Choose a random enemy that has an associated floor ID and combat style.
        /// </summary>
        /// <param name="floorID"></param> 
        /// <param name="combatStyle"></param>
        /// <returns>Matching enemy. if no matching combat style, chooses any style. if absolutely no options, returns NPCID.None</returns>
        public static int ChooseEnemy(int floorID, int combatStyle)
        {
            List<BaseRoguelikeNPC> enemyPool = AllNPCs.FindAll(x => x.associatedFloors.Contains(floorID) && x.CombatStyle == combatStyle);
            if (!enemyPool.Any())
            {
                enemyPool = AllNPCs.FindAll(x => x.associatedFloors.Contains(floorID) && x.CombatStyle >= 0);
            }
            if (enemyPool.Any())
            {
                int randIndex = Main.rand.Next(enemyPool.Count);
                return enemyPool[randIndex].modNPCID;
            }
            return 0;
        }
    }
}
