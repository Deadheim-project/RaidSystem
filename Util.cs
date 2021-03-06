using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RaidSystem
{
    public static class Util
    {
        public static string GetTerritoriesConquestedText(string playerTeam)
        {
            string redPrefab = ("RS_" + RaidSystem.TeamRedPrefab.Value);
            string bluePrefab = ("RS_" + RaidSystem.TeamBluePrefab.Value);

            int countRed = GetPrefabCount(redPrefab.GetStableHashCode());
            int countBlue = GetPrefabCount(bluePrefab.GetStableHashCode());
            int total = countRed + countBlue;

            return (playerTeam == "Blue" ? countBlue : countRed) + "/" + total;
        }

        public static string GetTerritoriesInfoInString()
        {
            string territories = "";
            int redPrefab = ("RS_" + RaidSystem.TeamRedPrefab.Value).GetStableHashCode();
            int bluePrefab = ("RS_" + RaidSystem.TeamBluePrefab.Value).GetStableHashCode();

            foreach (List<ZDO> zdoList in ZDOMan.instance.m_objectsBySector)
            {
                if (zdoList == null) continue;

                for (int index = 0; index < zdoList.Count; ++index)
                {
                    ZDO zdo2 = zdoList[index];
                    if (zdo2.GetPrefab() == redPrefab)
                    {
                        territories += "Red," + (int)zdo2.m_position.x + "," + (int)zdo2.m_position.y + "," + (int)zdo2.m_position.z;
                        territories += "|";
                    }
                    else if (zdo2.GetPrefab() == bluePrefab)
                    {
                        territories += "Blue," + (int)zdo2.m_position.x + "," + (int)zdo2.m_position.y + "," + (int)zdo2.m_position.z;
                        territories += "|";
                    }
                }
            }

            return territories;
        }


        private static int GetPrefabCount(int prefabHash)
        {
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

        public static void RespawnPrefab(string prefab, string alias, Vector3 position, Quaternion rotation, string crafterName = "")
        {
            Task.Run(async () =>
            {
                await Task.Delay(RaidSystem.SpawnDelayMS.Value);
                GameObject bluePrefab = PrefabManager.Instance.GetPrefab(prefab);
                GameObject instatiated = UnityEngine.Object.Instantiate<GameObject>(bluePrefab, position, rotation);

                if (crafterName is not "")
                {
                    instatiated.GetComponent<ItemDrop>().m_itemData.m_crafterName = crafterName;
                }

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
