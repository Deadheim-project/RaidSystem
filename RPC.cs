using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static RaidSystem.PlayerInfoService;

namespace RaidSystem
{
    [HarmonyPatch]
    class RPC
    {
        static string FileLocation = RaidSystem.FileDirectory + RaidSystem.FileName;

        public static void RPC_FileSync(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer()) return;

            ZPackage playerInfoList = new ZPackage();

            if (!Directory.Exists(RaidSystem.FileDirectory))
            {
                Directory.CreateDirectory(RaidSystem.FileDirectory);
            }

            if (File.Exists(FileLocation))
            {
                playerInfoList.Write((File.ReadAllText(FileLocation)));
            } else
            {
                pkg.Write("");
            }

            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "FileSyncClientRaidSystem", playerInfoList);
        }

        public static void RPC_TerritoriesSync(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer()) return;
            string playerTeam = pkg.ReadString();

            ZPackage pkgToSend = new ZPackage();
            pkgToSend.Write(Util.GetTerritoriesConquestedText(playerTeam));

            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "TerritoriesClientRaidSystem", pkgToSend);
        }

        public static void RPC_TerritoriesSyncClient(long sender, ZPackage pkg)
        {
            if (ZNet.instance.IsServer()) return;
            GUI.SetTerritoriesConquestText(pkg.ReadString());
        }

        public static void RPC_GetTerritoriesInfoToDraw(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer()) return;
            ZPackage pkgToSend = new ZPackage();
            pkgToSend.Write(Util.GetTerritoriesInfoInString());

            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "GetTerritoriesInfoToDrawClient", pkgToSend);
        }

        public static void RPC_GetTerritoriesInfoToDrawClient(long sender, ZPackage pkg)
        {
            if (ZNet.instance.IsServer()) return;
            string[] territoryList = pkg.ReadString().Split('|');
            MapDrawer.CreateMapDrawing(territoryList);
        }

        public static void RPC_FileSyncClient(long sender, ZPackage pkg)
        {
            if (ZNet.instance.IsServer()) return;
            string json = pkg.ReadString();
            if (json == "") return;
            new PlayerInfoService().SetPlayerInfoList(json);
            GUI.LoadMenu();

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "GetTerritoriesInfoToDraw", new ZPackage());
        }

        public static void RPC_UpdateOrSaveData(long sender, ZPackage pkg)
        {
            if (!ZNet.instance.IsServer()) return;

            List<PlayerInfo> playerInfoList = new();

            string[] pkgStringArray = pkg.ReadString().Split(',');
            string nick = pkgStringArray[0];
            string steamId = pkgStringArray[1];
            string playerId = pkgStringArray[2];
            string description = pkgStringArray[3];
            string team = pkgStringArray[4];

            if (!Directory.Exists(RaidSystem.FileDirectory)) Directory.CreateDirectory(RaidSystem.FileDirectory);
            if (!File.Exists(FileLocation)) File.Create(FileLocation);
            else playerInfoList = new PlayerInfoService().GetPlayerInfoList(File.ReadAllText(FileLocation));

            PlayerInfo playerInfo = playerInfoList.FirstOrDefault(x => x.PlayerId == playerId);
            if (playerInfo is null)
            {
                playerInfo = new();
                playerInfoList.Add(playerInfo);
            }

            playerInfo.Nick = nick;
            playerInfo.SteamId = steamId;
            playerInfo.PlayerId = playerId;
            playerInfo.Description = description;
            playerInfo.Team = team;

            string playerInfoListJson = SimpleJson.SimpleJson.SerializeObject(playerInfoList);
            File.WriteAllText(FileLocation, playerInfoListJson);
            ZPackage package = new ZPackage();
            package.Write(playerInfoListJson);

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "FileSyncClientRaidSystem", package);
        }

        [HarmonyPatch(typeof(Game), "Start")]
        public static class GameStart
        {
            public static void Postfix()
            {
                if (ZRoutedRpc.instance == null)
                    return;

                ZRoutedRpc.instance.Register<ZPackage>("FileSyncRaidSystem", new Action<long, ZPackage>(RPC_FileSync));
                ZRoutedRpc.instance.Register<ZPackage>("FileSyncClientRaidSystem", new Action<long, ZPackage>(RPC_FileSyncClient));
                ZRoutedRpc.instance.Register<ZPackage>("UpdateOrSaveDataRaidSystem", new Action<long, ZPackage>(RPC_UpdateOrSaveData));
                ZRoutedRpc.instance.Register<ZPackage>("TerritoriesSyncRaidSystem", new Action<long, ZPackage>(RPC_TerritoriesSync));
                ZRoutedRpc.instance.Register<ZPackage>("TerritoriesClientRaidSystem", new Action<long, ZPackage>(RPC_TerritoriesSyncClient));
                ZRoutedRpc.instance.Register<ZPackage>("GetTerritoriesInfoToDraw", new Action<long, ZPackage>(RPC_GetTerritoriesInfoToDraw));
                ZRoutedRpc.instance.Register<ZPackage>("GetTerritoriesInfoToDrawClient", new Action<long, ZPackage>(RPC_GetTerritoriesInfoToDrawClient));
            }
        }
    }
}
