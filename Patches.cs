using HarmonyLib;
using Jotunn.Managers;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace RaidSystem
{
    [HarmonyPatch]
    public class Patches
    {
        public static bool hasAwake;

        [HarmonyPatch(typeof(WearNTear), "Destroy")]
        public static class Destroy
        {
            private static void Postfix(WearNTear __instance)
            {
                if (!__instance.m_piece.gameObject.name.Contains("RaidWard"))
                    return;
                Util.RespawnPrefab("RaidWard", "Castelo Consquitado", ((Component)__instance).transform.position, ((Component)__instance).transform.rotation);
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
        private static class ZNet__OnNewConnection
        {
            public static void Postfix(ZNet __instance, ZNetPeer peer)
            {
                if (__instance.IsServer())
                    return;
                RaidSystem.SteamId = SteamUser.GetSteamID().ToString();
            }
        }

        [HarmonyPatch(typeof(Game), "Logout")]
        public static class Logout
        {
            private static void Postfix() => Patches.hasAwake = false;
        }

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static class OnSpawned
        {
            private static void Postfix()
            {
                if (Patches.hasAwake)
                    return;
                Patches.hasAwake = true;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "FileSyncRaidSystem", new object[1]
                {
          (object) new ZPackage()
                });
            }
        }

        [HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
        public static class CheckCanRemovePiece
        {
            [HarmonyPriority(0)]
            private static bool Prefix(Piece piece, Player __instance) => SynchronizationManager.Instance.PlayerIsAdmin || !Util.IsRaidEnabledHere(((Component)piece).transform.position);
        }

        [HarmonyPatch(typeof(Player), "PlacePiece")]
        public static class NoBuild_Patch
        {
            [HarmonyPriority(800)]
            private static bool Prefix(Piece piece, Player __instance) => SynchronizationManager.Instance.PlayerIsAdmin || !Util.IsRaidEnabledHere(((Component)__instance).transform.position) || piece.gameObject.GetComponent<Door>();
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        public static class RPC_Damage
        {
            [HarmonyPriority(0)]
            private static bool Prefix(WearNTear __instance, ref HitData hit, ZNetView ___m_nview)
            {
                try
                {
                    if (___m_nview == null)
                        return false;
                    bool flag = Util.IsRaidEnabledHere(((Component)__instance).transform.position);
                    if (!Util.CheckInPrivateArea(((Component)__instance).transform.position, false) && !flag)
                        return true;
                    if (!flag || Util.IsRaidDisabledThisTime() || !__instance.gameObject.name.Contains("RaidWard") && !__instance.gameObject.GetComponent<Door>())
                        return false;
                    hit.ApplyModifier((float)(1.0 - (double)RaidSystem.WardReductionDamage.Value / 100.0));
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError((object)(ex.Message + "    - " + ex.StackTrace));
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static class ApplyDamage
        {
            public static void Postfix(Character __instance, HitData hit)
            {
                try
                {
                    if ((double)__instance.GetHealth() > 0.0 || !hit.GetAttacker() || !hit.GetAttacker().IsPlayer() || !__instance.IsPlayer())
                        return;
                    Player killer = (Player)hit.GetAttacker();
                    PlayerInfoService.PlayerInfo playerInfo1 = RaidSystem.PlayerInfoList.FirstOrDefault<PlayerInfoService.PlayerInfo>((Func<PlayerInfoService.PlayerInfo, bool>)(x => x.PlayerId == killer.GetPlayerID().ToString()));
                    if (playerInfo1 == null)
                        return;
                    Player deadPlayer = (Player)__instance;
                    PlayerInfoService.PlayerInfo playerInfo2 = RaidSystem.PlayerInfoList.FirstOrDefault<PlayerInfoService.PlayerInfo>((Func<PlayerInfoService.PlayerInfo, bool>)(x => x.PlayerId == deadPlayer.GetPlayerID().ToString()));
                    if (playerInfo2 == null || playerInfo1.Team == playerInfo2.Team)
                        return;
                    UnityEngine.Object.Instantiate<GameObject>(playerInfo2.Team == "Blue" ? RaidSystem.blueTrophy : RaidSystem.redTrophy, ((Component)Player.m_localPlayer).transform.position, Quaternion.identity);
                }
                catch (Exception ex)
                {
                    Debug.Log((object)("Error droping player trophy " + ex.Message));
                }
            }
        }
    }
}
