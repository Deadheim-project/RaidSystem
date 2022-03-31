﻿using HarmonyLib;
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
                RaidSystem.AddClonedItems(); // remover

                if (hasAwake == true) return;
                hasAwake = true;

                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "FileSync", new ZPackage());
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
                    if (___m_nview is null) return false;
                    if (__instance.gameObject.name.Contains("guard_stone")) return false;
                    if (__instance.m_piece.m_nview.m_zdo.GetBool("isAdmin")) return false;

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
    }
}
