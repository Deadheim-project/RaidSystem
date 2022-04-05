using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RaidSystem.PlayerInfoService;

namespace RaidSystem
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy))]
        public static class Destroy
        {
            private static void Postfix(WearNTear __instance)
            {
                if (!RaidSystem.RespawnOtherTeamWard.Value) return;

                string redPrefab = ("RS_" + RaidSystem.TeamRedPrefab.Value);
                string bluePrefab = ("RS_" + RaidSystem.TeamBluePrefab.Value);

                if (__instance.m_piece.gameObject.name.Contains(bluePrefab)) Util.RespawnPrefab(redPrefab, RaidSystem.TeamBlueAlias.Value, __instance.transform.position, __instance.transform.rotation);
                else if (__instance.m_piece.gameObject.name.Contains(redPrefab)) Util.RespawnPrefab(bluePrefab, RaidSystem.TeamRedAlias.Value, __instance.transform.position, __instance.transform.rotation);
            }
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

        public static bool hasAwake = false;
        [HarmonyPatch(typeof(Game), "Logout")]
        public static class Logout
        {
            private static void Postfix()
            {
                hasAwake = false;
            }
        }

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static class OnSpawned
        {
            private static void Postfix()
            {
                if (hasAwake == true) return;
                hasAwake = true;

                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "FileSyncRaidSystem", new ZPackage());
            }
        }

        [HarmonyPatch(typeof(PrivateArea), nameof(PrivateArea.RPC_TogglePermitted))]
        public static class RPC_TogglePermitted
        {
            private static bool Prefix(PrivateArea __instance, long uid, long playerID, string name)
            {
                string redPrefab = ("RS_" + RaidSystem.TeamRedPrefab.Value);
                string bluePrefab = ("RS_" + RaidSystem.TeamBluePrefab.Value);

                string prefabName = __instance.m_piece.gameObject.name;
                if (prefabName.Contains("guard_stone")) return true;

                if (prefabName.Contains(redPrefab))
                {
                    if (RaidSystem.PlayerInfoList.Exists(x => x.PlayerId == playerID.ToString() && x.Team == "Red")) return true;
                }

                if (prefabName.Contains(bluePrefab))
                {
                    if (RaidSystem.PlayerInfoList.Exists(x => x.PlayerId == playerID.ToString() && x.Team == "Blue")) return true;
                }

                return false;
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
                    if (!Util.IsRaidEnabledHere(__instance.transform.position)) return false;

                    if (Util.IsRaidDisabledThisTime()) return false;

                    hit.ApplyModifier(1 - (RaidSystem.WardReductionDamage.Value / 100));
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "    - " + e.StackTrace);
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
        public static class ApplyDamage
        {
            public static void Postfix(Character __instance, HitData hit)
            {
                if (!(__instance.GetHealth() <= 0f)) return;

                if (!hit.GetAttacker()) return;
                if (!hit.GetAttacker().IsPlayer()) return;
                if (!__instance.IsPlayer()) return;

                Player killer = (Player)hit.GetAttacker();
                PlayerInfo playerInfo = RaidSystem.PlayerInfoList.FirstOrDefault(x => x.PlayerId == killer.GetPlayerID().ToString());
                if (playerInfo is null) return;

                Player deadPlayer = (Player)__instance;
                PlayerInfo deadPlayerInfo = RaidSystem.PlayerInfoList.FirstOrDefault(x => x.PlayerId == deadPlayer.GetPlayerID().ToString());

                if (deadPlayerInfo is null) return;
                if (playerInfo.Team == deadPlayerInfo.Team) return;

                GameObject trophyToSpawn = deadPlayerInfo.Team == "Blue" ? RaidSystem.blueTrophy : RaidSystem.redTrophy;
                killer.m_inventory.AddItem(trophyToSpawn, 1);

            }
        }
    }
}
