using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RaidSystem
{
    public static class Util
    {
        public static string GetTerritoriesConquestedText()
        {
            string redPrefab = ("RS_" + RaidSystem.TeamRedPrefab.Value);
            string bluePrefab = ("RS_" + RaidSystem.TeamBluePrefab.Value);

            int countRed = GetPrefabCount(redPrefab.GetStableHashCode());
            int countBlue = GetPrefabCount(bluePrefab.GetStableHashCode());
            int total = countRed + countBlue;

            return (RaidSystem.localPlayerInfo.Team == "Blue" ? countBlue : countRed) + "/" + total;
        }


        private static int GetPrefabCount(int prefabHash)
        {
            long creatorID = Player.m_localPlayer.GetPlayerID();
            int prefabCount = 0;
            foreach (List<ZDO> zdoList in ZDOMan.instance.m_objectsBySector)
            {
                if (zdoList == null) continue;

                for (int index = 0; index < zdoList.Count; ++index)
                {
                    ZDO zdo2 = zdoList[index];
                    if (zdo2.GetPrefab() == prefabHash)
                    {
                        prefabCount++;
                    }
                }
            }
            return prefabCount;
        }

        public static void RespawnPrefab(string prefab, string alias, Vector3 position, Quaternion rotation)
        {
            Task.Run(async () =>
            {
                await Task.Delay(RaidSystem.SpawnDelayMS.Value);
                GameObject bluePrefab = PrefabManager.Instance.GetPrefab(prefab);
                UnityEngine.Object.Instantiate<GameObject>(bluePrefab, position, rotation);

                DiscordApi.SendMessage("**" + alias + " - " + RaidSystem.ConquestMessage.Value + " X: " + (int)position.x + " Z:" + (int)position.z + "**");
            });
        }

        public static bool IsRaidDisabledThisTime()
        {
            if (RaidSystem.RaidTimeToAllowUtc.Value == "") return true;

            foreach (string hour in RaidSystem.RaidTimeToAllowUtc.Value.Split(','))
            {
                if (hour == "") continue;
                int hourInt = Convert.ToInt32(hour);
                int utcHour = DateTime.UtcNow.Hour;

                if (utcHour == hourInt) return false;
            }

            return true;
        }

        public static bool IsRaidEnabledHere(Vector3 position)
        {
            if (RaidSystem.RaidEnabledPositions.Value == "") return true;

            foreach (string area in RaidSystem.RaidEnabledPositions.Value.Split('|'))
            {
                if (area == "") continue;
                string[] splittedArea = area.Split(',');
                int x = Convert.ToInt32(splittedArea[0]);
                int z = Convert.ToInt32(splittedArea[1]);
                int radius = Convert.ToInt32(splittedArea[2]);

                if (Utils.DistanceXZ(position, new Vector3(x: x, z: z, y: 0)) < radius) return true;
            }

            return false;
        }
    }
}
