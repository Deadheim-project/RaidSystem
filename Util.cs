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
            string str1 = "RS_" + RaidSystem.TeamRedPrefab.Value;
            string str2 = "RS_" + RaidSystem.TeamBluePrefab.Value;
            int prefabCount1 = Util.GetPrefabCount(StringExtensionMethods.GetStableHashCode(str1));
            int prefabCount2 = Util.GetPrefabCount(StringExtensionMethods.GetStableHashCode(str2));
            int num = prefabCount1 + prefabCount2;
            return (playerTeam == "Blue" ? prefabCount2 : prefabCount1).ToString() + "/" + num.ToString();
        }

        public static string GetTerritoriesInfoInString()
        {
            string territoriesInfoInString = "";
            int stableHashCode1 = StringExtensionMethods.GetStableHashCode("RS_" + RaidSystem.TeamRedPrefab.Value);
            int stableHashCode2 = StringExtensionMethods.GetStableHashCode("RS_" + RaidSystem.TeamBluePrefab.Value);
            foreach (List<ZDO> zdoList in ZDOMan.instance.m_objectsBySector)
            {
                if (zdoList != null)
                {
                    for (int index = 0; index < zdoList.Count; ++index)
                    {
                        ZDO zdo = zdoList[index];
                        int num;
                        if (zdo.GetPrefab() == stableHashCode1)
                        {
                            string[] strArray = new string[7];
                            strArray[0] = territoriesInfoInString;
                            strArray[1] = "Red,";
                            num = (int)zdo.m_position.x;
                            strArray[2] = num.ToString();
                            strArray[3] = ",";
                            num = (int)zdo.m_position.y;
                            strArray[4] = num.ToString();
                            strArray[5] = ",";
                            num = (int)zdo.m_position.z;
                            strArray[6] = num.ToString();
                            territoriesInfoInString = string.Concat(strArray) + "|";
                        }
                        else if (zdo.GetPrefab() == stableHashCode2)
                        {
                            string[] strArray = new string[7];
                            strArray[0] = territoriesInfoInString;
                            strArray[1] = "Blue,";
                            num = (int)zdo.m_position.x;
                            strArray[2] = num.ToString();
                            strArray[3] = ",";
                            num = (int)zdo.m_position.y;
                            strArray[4] = num.ToString();
                            strArray[5] = ",";
                            num = (int)zdo.m_position.z;
                            strArray[6] = num.ToString();
                            territoriesInfoInString = string.Concat(strArray) + "|";
                        }
                    }
                }
            }
            return territoriesInfoInString;
        }

        private static int GetPrefabCount(int prefabHash)
        {
            int prefabCount = 0;
            foreach (List<ZDO> zdoList in ZDOMan.instance.m_objectsBySector)
            {
                if (zdoList != null)
                {
                    for (int index = 0; index < zdoList.Count; ++index)
                    {
                        if (zdoList[index].GetPrefab() == prefabHash)
                            ++prefabCount;
                    }
                }
            }
            return prefabCount;
        }

        public static void RespawnPrefab(
          string prefab,
          string alias,
          Vector3 position,
          Quaternion rotation,
          string crafterName = "")
        {
            Task.Run((Func<Task>)(async () =>
            {
                await Task.Delay(RaidSystem.SpawnDelayMS.Value);
                GameObject bluePrefab = PrefabManager.Instance.GetPrefab(prefab);
                GameObject instatiated = UnityEngine.Object.Instantiate<GameObject>(bluePrefab, position, rotation);
                if (!(crafterName == ""))
                    instatiated.GetComponent<ItemDrop>().m_itemData.m_crafterName = crafterName;
                new dWebHook().SendMessage("**" + alias + " - " + RaidSystem.ConquestMessage.Value + " X: " + ((int)position.x).ToString() + " Z:" + ((int)position.z).ToString() + "**");
                bluePrefab = (GameObject)null;
                instatiated = (GameObject)null;
            }));
        }

        public static bool IsRaidDisabledThisTime()
        {
            if (RaidSystem.RaidTimeToAllowUtc.Value == "")
                return true;
            string str1 = RaidSystem.RaidTimeToAllowUtc.Value;
            char[] chArray = new char[1] { ',' };
            foreach (string str2 in str1.Split(chArray))
            {
                if (!(str2 == "") && DateTime.UtcNow.Hour == Convert.ToInt32(str2))
                    return false;
            }
            return true;
        }

        public static bool IsRaidEnabledHere(Vector3 position)
        {
            if (RaidSystem.RaidEnabledPositions.Value == "")
                return true;
            string str1 = RaidSystem.RaidEnabledPositions.Value;
            char[] chArray = new char[1] { '|' };
            foreach (string str2 in str1.Split(chArray))
            {
                if (!(str2 == ""))
                {
                    string[] strArray = str2.Split(',');
                    int int32_1 = Convert.ToInt32(strArray[0]);
                    int int32_2 = Convert.ToInt32(strArray[1]);
                    int int32_3 = Convert.ToInt32(strArray[2]);
                    if ((double)Utils.DistanceXZ(position, new Vector3((float)int32_1, 0.0f, (float)int32_2)) < (double)int32_3)
                        return true;
                }
            }
            return false;
        }

        public static bool CheckInPrivateArea(Vector3 point, bool flash = false)
        {
            foreach (PrivateArea allArea in PrivateArea.m_allAreas)
            {
                if (allArea.IsEnabled() && allArea.IsInside(point, 0.0f))
                {
                    if (flash)
                        allArea.FlashShield(false);
                    return true;
                }
            }
            return false;
        }
    }
}