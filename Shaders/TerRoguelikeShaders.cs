using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria;

namespace TerRoguelike.Shaders
{
    public class TerRoguelikeShaders
    {
        private const string ShaderPath = "Shaders/";
        internal const string CalamityShaderPrefix = "TerRoguelike:";

        internal static Effect BasicTintShader;

        public static void LoadShaders()
        {
            AssetRepository terAss = TerRoguelike.Instance.Assets;

            // Shorthand to load shaders immediately.
            // Strings provided to LoadShader are the .xnb file paths.
            Effect LoadShader(string path) => terAss.Request<Effect>($"{ShaderPath}{path}", AssetRequestMode.ImmediateLoad).Value;

            BasicTintShader = LoadShader("BasicTint");
            RegisterMiscShader(BasicTintShader, "TintPass", "BasicTint");
        }
        private static void RegisterMiscShader(Effect shader, string passName, string registrationName)
        {
            Ref<Effect> shaderPointer = new(shader);
            MiscShaderData passParamRegistration = new(shaderPointer, passName);
            GameShaders.Misc[$"{CalamityShaderPrefix}{registrationName}"] = passParamRegistration;
        }
    }
}
