using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace RaidSystem
{
    public class PlayerInfoService
    {
        public List<PlayerInfo> GetPlayerInfoList(string json)
        {
            if (json == "") return new List<PlayerInfo>();
            List<PlayerInfo> playerInfoList = SimpleJson.SimpleJson.DeserializeObject<List<PlayerInfo>>(json);
            return playerInfoList;
        }

        public void SetPlayerInfoList(string json)
        {
            List<PlayerInfo> playerInfoList = SimpleJson.SimpleJson.DeserializeObject<List<PlayerInfo>>(json);
            RaidSystem.PlayerInfoList = playerInfoList;
            
            PlayerInfo localPlayerInfo = RaidSystem.PlayerInfoList.FirstOrDefault(x => x.PlayerId == Player.m_localPlayer.GetPlayerID().ToString());
            if (localPlayerInfo is not null) RaidSystem.HasTeam = true;
            RaidSystem.localPlayerInfo = localPlayerInfo;
        }

        public void PreparePlayerToSend(string team, string description = "")
        {
            ZPackage pkg = new ZPackage();
            string msg = Player.m_localPlayer.m_nview.GetZDO().GetString("playerName") + ",";
            msg += RaidSystem.SteamId + ",";
            msg += Player.m_localPlayer.GetPlayerID() + ",";
            msg += description + ",";
            msg += team;
            pkg.Write(msg);

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "UpdateOrSaveData", pkg);
            GUIChooseTeam.DestroyMenu();
        }

        public void UpdatePlayer(InputField input)
        {
            PreparePlayerToSend(RaidSystem.localPlayerInfo.Team, input.text);
        }

        public class PlayerInfo
        {
            public string Nick { get; set; }
            public string SteamId { get; set; }
            public string PlayerId { get; set; }
            public string Description { get; set; }
            public string Team { get; set; }
        }
    }
}
