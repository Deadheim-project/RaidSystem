using System.Collections.Generic;

namespace RaidSystem
{
    public class JsonLoader
    {
        public List<PlayerInfo> GetPlayerInfoList(string json)
        {
            Root playerInfoList = SimpleJson.SimpleJson.DeserializeObject<Root>(json);
            return playerInfoList.PlayerInfoList;
        }

        public class PlayerInfo
        {
            public string Nick { get; set; }
            public string SteamId { get; set; }
            public string PlayerId { get; set; }
            public string Description { get; set; }
            public string Team { get; set; }
        }

        public class Root
        {
            public List<PlayerInfo> PlayerInfoList { get; set; }
        }
    }
}
