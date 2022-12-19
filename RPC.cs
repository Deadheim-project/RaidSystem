using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RaidSystem
{
    [HarmonyPatch]
    internal class RPC
    {
        private static string FileLocation = RaidSystem.FileDirectory + RaidSystem.FileName;

        public static void RPC_FileSync(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer())
                return;
            ZPackage zpackage = new ZPackage();
            if (!Directory.Exists(RaidSystem.FileDirectory))
                Directory.CreateDirectory(RaidSystem.FileDirectory);
            if (File.Exists(RPC.FileLocation))
                zpackage.Write(File.ReadAllText(RPC.FileLocation));
            else
                pkg.Write("");
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "FileSyncClientRaidSystem", new object[1]
            {
        (object) zpackage
            });
        }

        public static void RPC_TerritoriesSync(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer())
                return;
            string playerTeam = pkg.ReadString();
            ZPackage zpackage = new ZPackage();
            zpackage.Write(Util.GetTerritoriesConquestedText(playerTeam));
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "TerritoriesClientRaidSystem", new object[1]
            {
        (object) zpackage
            });
        }

        public static void RPC_TerritoriesSyncClient(long sender, ZPackage pkg)
        {
            if (ZNet.instance.IsServer())
                return;
            GUI.SetTerritoriesConquestText(pkg.ReadString());
        }

        public static void RPC_GetTerritoriesInfoToDraw(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer())
                return;
            ZPackage zpackage = new ZPackage();
            zpackage.Write(Util.GetTerritoriesInfoInString());
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "GetTerritoriesInfoToDrawClient", new object[1]
            {
        (object) zpackage
            });
        }

        public static void RPC_GetTerritoriesInfoToDrawClient(long sender, ZPackage pkg)
        {
            if (ZNet.instance.IsServer())
                return;
            pkg.ReadString().Split('|');
        }

        public static void RPC_FileSyncClient(long sender, ZPackage pkg)
        {
            if (ZNet.instance.IsServer())
                return;
            string json = pkg.ReadString();
            if (json == "")
                return;
            new PlayerInfoService().SetPlayerInfoList(json);
            GUI.LoadMenu();
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "GetTerritoriesInfoToDraw", new object[1]
            {
        (object) new ZPackage()
            });
        }

        public static void RPC_UpdateOrSaveData(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer())
                return;
            List<PlayerInfoService.PlayerInfo> source = new List<PlayerInfoService.PlayerInfo>();
            string[] strArray = pkg.ReadString().Split(',');
            string str1 = strArray[0];
            string str2 = strArray[1];
            string playerId = strArray[2];
            string str3 = strArray[3];
            string str4 = strArray[4];
            if (!Directory.Exists(RaidSystem.FileDirectory))
                Directory.CreateDirectory(RaidSystem.FileDirectory);
            if (!File.Exists(RPC.FileLocation))
                File.Create(RPC.FileLocation);
            else
                source = new PlayerInfoService().GetPlayerInfoList(File.ReadAllText(RPC.FileLocation));
            PlayerInfoService.PlayerInfo playerInfo = source.FirstOrDefault<PlayerInfoService.PlayerInfo>((Func<PlayerInfoService.PlayerInfo, bool>)(x => x.PlayerId == playerId));
            if (playerInfo == null)
            {
                playerInfo = new PlayerInfoService.PlayerInfo();
                source.Add(playerInfo);
            }
            playerInfo.Nick = str1;
            playerInfo.SteamId = str2;
            playerInfo.PlayerId = playerId;
            playerInfo.Description = str3;
            playerInfo.Team = str4;
            string contents = SimpleJson.SimpleJson.SerializeObject((object)source);
            File.WriteAllText(RPC.FileLocation, contents);
            ZPackage zpackage = new ZPackage();
            zpackage.Write(contents);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "FileSyncClientRaidSystem", new object[1]
            {
        (object) zpackage
            });
        }

        [HarmonyPatch(typeof(Game), "Start")]
        public static class GameStart
        {
            public static void Postfix()
            {
                if (ZRoutedRpc.instance == null)
                    return;
                ZRoutedRpc.instance.Register<ZPackage>("FileSyncRaidSystem", new Action<long, ZPackage>(RPC.RPC_FileSync));
                ZRoutedRpc.instance.Register<ZPackage>("FileSyncClientRaidSystem", new Action<long, ZPackage>(RPC.RPC_FileSyncClient));
                ZRoutedRpc.instance.Register<ZPackage>("UpdateOrSaveDataRaidSystem", new Action<long, ZPackage>(RPC.RPC_UpdateOrSaveData));
                ZRoutedRpc.instance.Register<ZPackage>("TerritoriesSyncRaidSystem", new Action<long, ZPackage>(RPC.RPC_TerritoriesSync));
                ZRoutedRpc.instance.Register<ZPackage>("TerritoriesClientRaidSystem", new Action<long, ZPackage>(RPC.RPC_TerritoriesSyncClient));
                ZRoutedRpc.instance.Register<ZPackage>("GetTerritoriesInfoToDraw", new Action<long, ZPackage>(RPC.RPC_GetTerritoriesInfoToDraw));
                ZRoutedRpc.instance.Register<ZPackage>("GetTerritoriesInfoToDrawClient", new Action<long, ZPackage>(RPC.RPC_GetTerritoriesInfoToDrawClient));
            }
        }
    }
}
