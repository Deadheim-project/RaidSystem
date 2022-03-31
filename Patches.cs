using HarmonyLib;
using Jotunn.Managers;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace RaidSystem
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public static class OnSpawned
        {
            private static void Postfix(WearNTear __instance)
            {
                RaidSystem.AddClonedItems();
            }
        }

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy))]
        public static class Destroy
        {
            private static void Postfix(WearNTear __instance)
            {
                if (!RaidSystem.RespawnOtherTeamWard.Value) return;

                string redPrefab = ("RS_" + RaidSystem.TeamRedPrefab.Value);
                string bluePrefab = ("RS_" + RaidSystem.TeamBluePrefab.Value);

                if (__instance.m_piece.gameObject.name.Contains(bluePrefab)) RespawnWard(bluePrefab, RaidSystem.TeamBlueAlias.Value, __instance);
                else if (__instance.m_piece.gameObject.name.Contains(redPrefab)) RespawnWard(redPrefab, RaidSystem.TeamRedAlias.Value, __instance);
            }
        }

        private static void RespawnWard(string prefab, string alias, WearNTear wearNtear)
        {
            Task.Run(async () =>
            {
                await Task.Delay(RaidSystem.SpawnDelayMS.Value);
                GameObject bluePrefab = PrefabManager.Instance.GetPrefab(prefab);
                UnityEngine.Object.Instantiate<GameObject>(bluePrefab, wearNtear.transform.position, wearNtear.transform.rotation);

                DiscordApi.SendMessage("**" + alias + " - " + RaidSystem.ConquestMessage.Value + " X: " + (int)wearNtear.transform.position.x + " Z:" + (int)wearNtear.transform.position.z + "**");
            });
        }

        [HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
        public static class CheckCanRemovePiece
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(Piece piece, Player __instance)
            {
                var prefabs = new List<string> { "RS_" + RaidSystem.TeamBluePrefab.Value, "RS_" + RaidSystem.TeamRedPrefab.Value };
                if (prefabs.Where(x => piece.gameObject.name.Contains(x)).Any()) return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
        private static class ZNet__OnNewConnection
        {
            public static void Postfix(ZNet __instance, ZNetPeer peer)
            {
                if (!__instance.IsServer())
                {
                    RaidSystem.SteamId = SteamUser.GetSteamID().ToString();
                }
            }
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        public static class RPC_Damage
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(WearNTear __instance, ref HitData hit, ZNetView ___m_nview)
            {
                try
                {
                    if (___m_nview is null) return false;
                    if (!PrivateArea.CheckInPrivateArea(__instance.transform.position)) return true;
                    if (!IsRaidEnabledHere(__instance.transform.position)) return false;
                    if (___m_nview is null) return false;
                    if (__instance.gameObject.name.Contains("guard_stone")) return false;
                    if (__instance.m_piece.m_nview.m_zdo.GetBool("isAdmin")) return false;

                    if (IsRaidDisabledThisTime()) return false;

                    hit.ApplyModifier(1 - (RaidSystem.WardReductionDamage.Value / 100));
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "    - " + e.StackTrace);
                    return false;
                }
            }

            private static bool IsRaidDisabledThisTime()
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

                foreach (string area in RaidSystem.RaidEnabledPositions.Value.Split('|').ToList())
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
