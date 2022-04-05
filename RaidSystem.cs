using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using UnityEngine;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using static RaidSystem.PlayerInfoService;

namespace RaidSystem
{
    [BepInPlugin(PluginGUID, PluginGUID, Version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
    public class RaidSystem : BaseUnityPlugin
    {
        public const string PluginGUID = "Detalhes.RaidSystem";
        public const string Version = "1.3.1";
        Harmony harmony = new Harmony(PluginGUID);
        public static readonly string ModPath = Path.GetDirectoryName(typeof(RaidSystem).Assembly.Location);

        public static ConfigEntry<string> TeamBluePrefab;
        public static ConfigEntry<string> TeamRedPrefab;
        public static ConfigEntry<string> TeamBlueAlias;
        public static ConfigEntry<string> TeamRedAlias;
        public static ConfigEntry<float> ColorAlfa;
        public static ConfigEntry<int> RadiusDrawMap;

        public static ConfigEntry<string> TeamBlueColorOverlap;
        public static ConfigEntry<string> TeamRedColorOverlap;

        public static ConfigEntry<string> WebhookUrl;
        public static ConfigEntry<string> AdminList;

        public static ConfigEntry<string> RaidTimeToAllowUtc;
        public static ConfigEntry<string> RaidEnabledPositions;

        public static ConfigEntry<string> ConquestMessage;
        public static ConfigEntry<string> ChooseTeamMessage;
        public static ConfigEntry<string> ButtonUpdateText;
        public static ConfigEntry<string> TeamMemberListText;
        public static ConfigEntry<string> TerritoriesConquestedText;
        public static ConfigEntry<string> Cost;

        public static ConfigEntry<int> HitPoints;
        public static ConfigEntry<int> AreaRadius;
        public static ConfigEntry<int> SpawnDelayMS;
        public static ConfigEntry<int> Scale;
        public static ConfigEntry<float> WardReductionDamage;
        public static ConfigEntry<bool> RespawnOtherTeamWard;
        public static ConfigEntry<bool> OnlyAdminCanBuild;
        public static ConfigEntry<bool> RemoveTokenCost;
        public static ConfigEntry<KeyCode> KeyboardShortcut;
 
        public static List<PlayerInfo> PlayerInfoList = new();
        public static bool HasTeam = false;
        public static PlayerInfo localPlayerInfo;

        public static GameObject blueTrophy;
        public static GameObject redTrophy;

        public static Dictionary<string, GameObject> menuItems = new Dictionary<string, GameObject>();
        public static GameObject Menu;

        public static string FileDirectory = BepInEx.Paths.ConfigPath + @"/RaidSystem/";
        public static string FileName = "Detalhes.RaidSystem.json";

        public static string SteamId = "xxxxxx";

        private void Awake()
        {
            Config.SaveOnConfigSet = true;

            RemoveTokenCost = Config.Bind("Craft Server config", "RemoveTokenCost", false,
new ConfigDescription("RemoveTokenCost", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            OnlyAdminCanBuild = Config.Bind("Craft Server config", "OnlyAdminCanBuild", true,
new ConfigDescription("OnlyAdminCanBuild", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));


            Cost = Config.Bind("Craft Server config", "Cost", "wood:1000:false,stone:1000:false",
            new ConfigDescription("Cost", null,
            new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TeamBluePrefab = Config.Bind("Craft Server config", "TeamBluePrefab", "$custompiece_wolf",
            new ConfigDescription("TeamBluePrefab", null,
            new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TeamRedPrefab = Config.Bind("Craft Server config", "TeamRedPrefab", "$custompiece_elk",
            new ConfigDescription("TeamRedPrefab", null,
            new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RadiusDrawMap = Config.Bind("Server config", "RadiusDrawMap", 30,
new ConfigDescription("300", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));      

            ColorAlfa = Config.Bind("Server config", "ColorAlfa",0.7f,
new ConfigDescription("0.1f,1f", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TeamRedColorOverlap = Config.Bind("Server config", "TeamRedColorOverlap", "red",
new ConfigDescription("blue,red,blue,green,cyan,clear,grey,magentawhite,black,yellow,gray", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TeamBlueColorOverlap = Config.Bind("Server config", "TeamBlueColorOverlap", "blue",
new ConfigDescription("blue,red,blue,green,cyan,clear,grey,magentawhite,black,yellow,gray", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));


            TeamMemberListText = Config.Bind("Text Server config", "TeamMemberListText", "Realm Members: ",
new ConfigDescription("TeamMemberListText", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TerritoriesConquestedText = Config.Bind("Text Server config", "TerritoriesConquestedText", "Cities under control:",
new ConfigDescription("TerritoriesConquestedText", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TeamMemberListText = Config.Bind("Text Server config", "TeamMemberListText", "Realm Members: ",
new ConfigDescription("TeamMemberListText", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ButtonUpdateText = Config.Bind("Text Server config", "ButtonUpdateText", "Update",
new ConfigDescription("ButtonUpdateText", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));


            ChooseTeamMessage = Config.Bind("Text Server config", "ChooseTeamMessage", "Choose Realm",
new ConfigDescription("ChooseTeamMessage", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ConquestMessage = Config.Bind("Text Server config", "ConquestMessage", "Has conquest the base located at:",
new ConfigDescription("ConquestMessage", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RaidTimeToAllowUtc = Config.Bind("Server config", "RaidTimeToAllowUtc", "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24",
new ConfigDescription("Utc hours that raids will be enabled", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RaidEnabledPositions = Config.Bind("Server config", "RaidEnabledPositions", "",
new ConfigDescription("RaidEnabledPositions", null,
        new ConfigurationManagerAttributes { IsAdminOnly = true }));

            AreaRadius = Config.Bind("Server config", "AreaRadius", 150,
        new ConfigDescription("AreaRadius", null,
        new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WardReductionDamage = Config.Bind("Server config", "WardReductionDamage", 99.0f,
            new ConfigDescription("WardReductionDamage", null,
                     new ConfigurationManagerAttributes { IsAdminOnly = true }));

            AdminList = Config.Bind("Server config", "AdminList", "steamid steamid",
        new ConfigDescription("AdminList", null,
        new ConfigurationManagerAttributes { IsAdminOnly = true })); 

            TeamBlueAlias = Config.Bind("Server config", "TeamBlueAlias", "Bretonnia",
            new ConfigDescription("TeamBlueAlias", null,
            new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TeamRedAlias = Config.Bind("Server config", "TeamRedAlias", "Kattegat",
            new ConfigDescription("TeamRedAlias", null,
            new ConfigurationManagerAttributes { IsAdminOnly = true }));

            HitPoints = Config.Bind("Server config", "HitPoints", 10000,
new ConfigDescription("HitPoints", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            SpawnDelayMS = Config.Bind("Server config", "SpawnDelayMS", 5000,
new ConfigDescription("SpawnDelayMS", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            Scale = Config.Bind("Server config", "Scale", 3,
new ConfigDescription("Scale", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RespawnOtherTeamWard = Config.Bind("Server config", "RespawnOtherTeamWard", true,
new ConfigDescription("RespawnOtherTeamWard", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WebhookUrl = Config.Bind("Server config", "WebhookUrl", "",
new ConfigDescription("WebhookUrl", null,
new ConfigurationManagerAttributes { IsAdminOnly = true }));

            KeyboardShortcut = Config.Bind("Client config", "KeyboardShortcutConfig",
                KeyCode.PageUp,
                    new ConfigDescription("Client side KeyboardShortcut", null, null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false }));

            harmony.PatchAll();

            Cloning.LoadAssets();

            SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
            {
                if (attr.InitialSynchronization)
                {
                    Cloning.AddCustomPrivateAreas();
                    Cloning.AddTrophies();              
                }
                else
                {
                    Jotunn.Logger.LogMessage("Config sync event received");
                }
            };
        }
   

        private void Update()
        {
            if (Input.GetKeyDown(KeyboardShortcut.Value))
            {
                Player localPlayer = Player.m_localPlayer;
                if (!localPlayer || localPlayer.IsDead() || (localPlayer.InCutscene() || localPlayer.IsTeleporting()))
                    return;

                GUI.ToggleMenu();
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "GetGoldServer", new ZPackage());
            }
        }

    }
}
