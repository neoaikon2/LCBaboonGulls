using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace LCBaboongulls
{
    // Plugin defs
    [BepInPlugin("org.bepinex.plugins.lethalcompany.baboongulls", "LCBaboonGulls", "1.0.0")]
    [BepInProcess("Lethal Company.exe")]
    [BepInDependency("BepInEx-BepInExPack-5.4.2100")]
    public class LCModMain :BaseUnityPlugin
    {
        // Seagull SFX Members
        readonly string assetBundlePath = "\\baboongullsfx.data";
        public static AudioClip sfxMine = AudioClip.Create("null", 1024, 1, 12000, false);
        GameObject sfxTester = new GameObject();
        private void Awake()
        {
            // Get the path to the seagull sfx asset bundle
            string sfxPath = BepInEx.Paths.PatcherPluginPath + assetBundlePath;
#if DEBUG            
            Logger.LogDebug("Attempting to load asset bundle...");
#endif
            // Try to load the asset bundle
            try
            {
                AssetBundle sfxBundle = AssetBundle.LoadFromFile(sfxPath);
                sfxMine = (AudioClip)sfxBundle.LoadAsset("Assets/mine.mp3");
                sfxTester = (GameObject)sfxBundle.LoadAsset("Assets/BaboonGullSfxTester.prefab");
                Logger.LogDebug("Asset bundle loaded successfully!");
                Baboonhawk_Patches.seagullsMine = sfxMine;
                Baboonhawk_Patches.seagullTester = sfxTester;
            } catch(Exception ex)
            {
                Logger.LogError("Encountered an exception loading the asset bundle");
                Logger.LogError(ex.Message);
            }

#if DEBUG
            Logger.LogDebug("Injecting GrabScrap postfix into BaboonBirdAI");
#endif
            // Inject harmony patch
            Harmony harmony = new Harmony("org.bepinex.plugins.lethalcompany.baboongulls");
            MethodInfo original = AccessTools.Method(typeof(BaboonBirdAI), "GrabScrap");
            MethodInfo patch = AccessTools.Method(typeof(Baboonhawk_Patches), "GrabScrapPatch");
            harmony.Patch(original, new HarmonyMethod(patch));
        }
    }
        
    public class Baboonhawk_Patches
    {
        // Members
        public static GameObject seagullTester = new GameObject();
        public static AudioClip seagullsMine = AudioClip.Create("null", 1024, 1, 12000, false);

        // Harmony postfix patch for the BaboonBirdAI.GrabScrap method
        [HarmonyPatch(typeof(BaboonBirdAI), "GrabScrapPatch")]
        [HarmonyPostfix]
        public static void GrabScrapPatch(BaboonBirdAI __instance)
        {
            if (seagullsMine != null)
            {
#if DEBUG
                Console.Write("Rockin', Rockin' and rollin'");
                Console.Write("Down to the beach I'm strolling");
                Console.Write("But the seagulls poke at my head, Not fun!");
                Console.Write("I said 'Seagulls mmgh! Stop it now!'");
#endif
                RoundManager.PlayRandomClip(__instance.creatureVoice, new AudioClip[] { seagullsMine }, true, 1f, 1105);
            } else
            {
                Console.Write("[LC Baboongulls] ERROR - Sound effect not present");
            }
        }
    }
}
