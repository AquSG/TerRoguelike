using Terraria.ModLoader;
using TerRoguelike.Schematics;
using static TerRoguelike.Managers.SpawnManager;
using TerRoguelike.Managers;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.UI;
using Terraria;
using TerRoguelike.Shaders;
using Terraria.Graphics.Effects;
using TerRoguelike.Skies;
using Terraria.GameContent.Shaders;

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
            specialPendingItems = null;
            BarrierUI.Unload();
            DeathUI.Unload();
            CreditsUI.Unload();
            DebugUI.Unload();
            ItemBasinUI.Unload();
            ItemManager.Unload();
            EnemyHealthbarUI.Unload();
            NPCManager.Unload();
            TextureManager.Unload();
            ItemManager.UnloadStarterItems();
        }
        public void LoadClient()
        {
            TerRoguelikeShaders.LoadShaders();
            Filters.Scene["TerRoguelike:MoonLordClone"] = new Filter(new ModifiedMoonLordScreenShaderData("FilterMoonLord"), EffectPriority.VeryHigh);
            SkyManager.Instance["TerRoguelike:MoonLordSkyClone"] = new MoonLordSkyClone();
        }
        public override void PostSetupContent()
        {
            TextureManager.SetStaticDefaults();
            BarrierUI.Load();
            DeathUI.Load();
            CreditsUI.Load();
            DebugUI.Load();
            ItemBasinUI.Load();
            EnemyHealthbarUI.Load();
        }
    }
}