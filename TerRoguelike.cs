using Terraria.ModLoader;
using TerRoguelike.Schematics;
using static TerRoguelike.Managers.SpawnManager;
using TerRoguelike.Managers;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.UI;
using Terraria;
using TerRoguelike.Shaders;

namespace TerRoguelike
{
	public class TerRoguelike : Mod
	{
		internal static TerRoguelike Instance;
        public override void Load()
        {
            Instance = this;
            TextureManager.Load();
            SchematicManager.Load();
            ItemManager.Load();
            NPCManager.Load();
            if (!Main.dedServ)
            {
                LoadClient();
            }
        }
        public override void Unload()
        {
            Instance = null;
            SchematicManager.Unload();
            RoomList = null;
            healingPulses = null;
            attackPlanRocketBundles = null;
            pendingEnemies = null;
            pendingItems = null;
            BarrierUI.Unload();
            DeathUI.Unload();
            DebugUI.Unload();
            ItemManager.Unload();
            NPCManager.Unload();
            TextureManager.Unload();
        }
        public void LoadClient()
        {
            TerRoguelikeShaders.LoadShaders();
        }
        public override void PostSetupContent()
        {
            TextureManager.SetStaticDefaults();
            BarrierUI.Load();
            DeathUI.Load();
            DebugUI.Load();
        }
    }
}