using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.Graphics.Effects;

namespace TerRoguelike.Shaders
{
    public class TerRoguelikeShaders
    {
        private const string ShaderPath = "Shaders/";
        internal const string CalamityShaderPrefix = "TerRoguelike:";

        internal static Effect BasicTintShader;
        internal static Effect CircularGradientWithEdge;
        internal static Effect ProtectiveBubbleShield;

        public static void LoadShaders()
        {
            AssetRepository terAss = TerRoguelike.Instance.Assets;

            // Shorthand to load shaders immediately.
            // Strings provided to LoadShader are the .xnb file paths.
            Effect LoadShader(string path) => terAss.Request<Effect>($"{ShaderPath}{path}", AssetRequestMode.ImmediateLoad).Value;


            BasicTintShader = LoadShader("BasicTint");
            RegisterMiscShader(BasicTintShader, "TintPass", "BasicTint");

            CircularGradientWithEdge = LoadShader("CircularGradientWithEdge");
            RegisterMiscShader(CircularGradientWithEdge, "CircularGradientWithEdgePass", "CircularGradientWithEdge");

            ProtectiveBubbleShield = LoadShader("ProtectiveBubbleShield");
            RegisterScreenShader(ProtectiveBubbleShield, "ShieldPass", "ProtectiveBubbleShield");

        }
        private static void RegisterMiscShader(Effect shader, string passName, string registrationName)
        {
            Ref<Effect> shaderPointer = new(shader);
            MiscShaderData passParamRegistration = new(shaderPointer, passName);
            GameShaders.Misc[$"{CalamityShaderPrefix}{registrationName}"] = passParamRegistration;
        }
        private static void RegisterScreenShader(Effect shader, string passName, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            Ref<Effect> shaderPointer = new(shader);
            ScreenShaderData passParamRegistration = new(shaderPointer, passName);
            RegisterSceneFilter(passParamRegistration, registrationName, priority);
        }
        private static void RegisterSceneFilter(ScreenShaderData passReg, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            string prefixedRegistrationName = $"{CalamityShaderPrefix}{registrationName}";
            Filters.Scene[prefixedRegistrationName] = new Filter(passReg, priority);
            Filters.Scene[prefixedRegistrationName].Load();
        }
    }
}
