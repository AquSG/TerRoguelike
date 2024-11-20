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
        //Shader loading and registering lifted from the Calamity Mod
        private const string ShaderPath = "Shaders/";
        internal const string TerRoguelikeShaderPrefix = "TerRoguelike:";

        internal static Asset<Effect> BasicTintShader;
        internal static Asset<Effect> CircularGradientWithEdge;
        internal static Asset<Effect> CircularGradientOuter;
        internal static Asset<Effect> ProtectiveBubbleShield;
        internal static Asset<Effect> AncientTwigEffect;
        internal static Asset<Effect> MaskOverlay;
        internal static Asset<Effect> SideFade;
        internal static Asset<Effect> ConeFade;
        internal static Asset<Effect> ConeSnippet;
        internal static Asset<Effect> Pixelation;
        internal static Asset<Effect> DualContrast;
        internal static Asset<Effect> SpecialPortal;
        internal static Asset<Effect> ColorMaskOverlay;
        internal static Asset<Effect> CircularPulse;

        public static void LoadShaders()
        {
            AssetRepository terAss = TerRoguelike.Instance.Assets;

            // Shorthand to load shaders immediately.
            // Strings provided to LoadShader are the .xnb file paths.
            Asset<Effect> LoadShader(string path) => terAss.Request<Effect>($"{ShaderPath}{path}", AssetRequestMode.AsyncLoad);


            BasicTintShader = LoadShader("BasicTint"); //lifted from the Calamity Mod
            RegisterMiscShader(BasicTintShader, "TintPass", "BasicTint");

            CircularGradientWithEdge = LoadShader("CircularGradientWithEdge"); //lifted from the Calamity Mod. Though I technically helped make the edited version of this one.
            RegisterMiscShader(CircularGradientWithEdge, "CircularGradientWithEdgePass", "CircularGradientWithEdge");

            CircularGradientOuter = LoadShader("CircularGradientOuter"); //Edited version of other circle gradient
            RegisterMiscShader(CircularGradientOuter, "CircularGradientOuterPass", "CircularGradientOuter");

            ProtectiveBubbleShield = LoadShader("ProtectiveBubbleShield"); //lifted from the Calamity Mod
            RegisterScreenShader(ProtectiveBubbleShield, "ShieldPass", "ProtectiveBubbleShield");

            AncientTwigEffect = LoadShader("AncientTwigEffect");
            RegisterScreenShader(AncientTwigEffect, "ShieldPass", "AncientTwigEffect");

            SideFade = LoadShader("SideFade");
            RegisterScreenShader(SideFade, "SideFadePass", "SideFade");

            ConeFade = LoadShader("ConeFade");
            RegisterScreenShader(ConeFade, "ConeFadePass", "ConeFade");

            ConeSnippet = LoadShader("ConeSnippet");
            RegisterScreenShader(ConeSnippet, "ConeSnippetPass", "ConeSnippet");

            Pixelation = LoadShader("Pixelation");
            RegisterScreenShader(Pixelation, "PixelationPass", "Pixelation");

            DualContrast = LoadShader("DualContrast");
            RegisterScreenShader(DualContrast, "DualContrastPass", "DualContrast");

            MaskOverlay = LoadShader("MaskOverlay");
            RegisterScreenShader(MaskOverlay, "MaskOverlayPass", "MaskOverlay");

            SpecialPortal = LoadShader("SpecialPortal");
            RegisterScreenShader(SpecialPortal, "SpecialPortalPass", "SpecialPortal");

            ColorMaskOverlay = LoadShader("ColorMaskOverlay");
            RegisterScreenShader(ColorMaskOverlay, "ColorMaskOverlayPass", "ColorMaskOverlay");

            CircularPulse = LoadShader("CircularPulse");
            RegisterMiscShader(CircularPulse, "CircularPulsePass", "CircularPulse");
        }
        private static void RegisterMiscShader(Asset<Effect> shader, string passName, string registrationName)
        {
            MiscShaderData passParamRegistration = new(shader, passName);
            GameShaders.Misc[$"{TerRoguelikeShaderPrefix}{registrationName}"] = passParamRegistration;
        }
        private static void RegisterScreenShader(Asset<Effect> shader, string passName, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            ScreenShaderData passParamRegistration = new(shader, passName);
            RegisterSceneFilter(passParamRegistration, registrationName, priority);
        }
        private static void RegisterSceneFilter(ScreenShaderData passReg, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            string prefixedRegistrationName = $"{TerRoguelikeShaderPrefix}{registrationName}";
            Filters.Scene[prefixedRegistrationName] = new Filter(passReg, priority);
            Filters.Scene[prefixedRegistrationName].Load();
        }
    }
}
