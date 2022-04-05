using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaidSystem
{
    public class Cloning
    {
        public static void LoadAssets()
        {
            PrefabManager.OnPrefabsRegistered += AddTeamTokens;
        }

        public static void AddTrophies()
        {
            CustomItem redTeamPlayerTrophy = new CustomItem(RaidSystem.TeamRedAlias.Value + "PlayerTrophy", "TrophySkeleton");
            ItemDrop redTeamPlayerTrophyItemDrop = redTeamPlayerTrophy.ItemDrop;
            redTeamPlayerTrophyItemDrop.m_itemData.m_shared.m_name = RaidSystem.TeamRedAlias.Value + " Player Trophy";
            redTeamPlayerTrophyItemDrop.m_itemData.m_shared.m_description = "Someone has died.";
            redTeamPlayerTrophyItemDrop.m_itemData.m_shared.m_maxStackSize = 10;
            RaidSystem.redTrophy = redTeamPlayerTrophy.ItemPrefab;
            ItemManager.Instance.AddItem(redTeamPlayerTrophy);

            CustomItem blueTeamPlayerTrophy = new CustomItem(RaidSystem.TeamRedAlias.Value + "BluePlayerTrophy", "TrophySkeleton");
            ItemDrop blueTeamPlayerTrophyItemDrop = blueTeamPlayerTrophy.ItemDrop;
            blueTeamPlayerTrophyItemDrop.m_itemData.m_shared.m_name = RaidSystem.TeamRedAlias.Value + " Player Trophy";
            blueTeamPlayerTrophyItemDrop.m_itemData.m_shared.m_description = "Someone has died.";
            blueTeamPlayerTrophyItemDrop.m_itemData.m_shared.m_maxStackSize = 10;
            RaidSystem.blueTrophy = blueTeamPlayerTrophy.ItemPrefab;
            ItemManager.Instance.AddItem(blueTeamPlayerTrophy);
        }

        public static void AddCustomPrivateAreas()
        {
            var hammer = ObjectDB.instance.m_items.FirstOrDefault(x => x.name == "Hammer");

            PieceTable table = hammer.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;

            foreach (string prefab in new List<string> { RaidSystem.TeamBluePrefab.Value, RaidSystem.TeamRedPrefab.Value })
            {
                string newName = "RS_" + prefab;

                if (table.m_pieces.Exists(x => x.name == newName))
                {
                    continue;
                }

                GameObject customRaidWard = PrefabManager.Instance.CreateClonedPrefab(newName, prefab);
                if (!customRaidWard)
                {
                    Debug.LogError("original prefab not found for " + prefab);
                    continue;
                }

                Piece piece = customRaidWard.GetComponent<Piece>();
                if (piece is null) piece = customRaidWard.AddComponent<Piece>();

                WearNTear wearNTear = customRaidWard.GetComponent<WearNTear>();
                if (!wearNTear) customRaidWard.AddComponent<WearNTear>();
                wearNTear.m_health = RaidSystem.HitPoints.Value;

                piece.m_description = RaidSystem.TeamBluePrefab.Value == prefab ? RaidSystem.TeamBlueAlias.Value : RaidSystem.TeamRedAlias.Value;
                piece.m_name = customRaidWard.name;

                var requirements = new List<Piece.Requirement>();

                foreach (string str in RaidSystem.Cost.Value.Split(','))
                {
                    string prefabName = str.Split(':')[0];
                    int amount = Convert.ToInt32(str.Split(':')[1]);
                    bool recover = Convert.ToBoolean(str.Split(':')[2]);

                    var requirement = new Piece.Requirement();
                    requirement.m_resItem = PrefabManager.Instance.GetPrefab(prefabName).GetComponent<ItemDrop>();
                    requirement.m_amount = amount;
                    requirement.m_recover = recover;

                    requirements.Add(requirement);
                }

                if (!RaidSystem.RemoveTokenCost.Value)
                {
                    var requirement = new Piece.Requirement();
                    requirement.m_recover = false;
                    requirement.m_amount = 1;
                    var prefabName = prefab == RaidSystem.TeamBluePrefab.Value ? "BlueToken" : "RedToken";
                    requirement.m_resItem = PrefabManager.Instance.GetPrefab(prefabName).GetComponent<ItemDrop>();
                    requirements.Add(requirement);
                }

                piece.m_resources = requirements.ToArray();

                PrivateArea privateArea = customRaidWard.AddComponent<PrivateArea>();
                PrivateArea guardStonePrivateARea = PrefabManager.Instance.GetPrefab("guard_stone").GetComponent<PrivateArea>();
                privateArea.m_radius = RaidSystem.AreaRadius.Value;
                privateArea.m_name = newName;
                privateArea.m_removedPermittedEffect = guardStonePrivateARea.m_removedPermittedEffect;
                privateArea.m_flashEffect = guardStonePrivateARea.m_flashEffect;
                privateArea.m_activateEffect = guardStonePrivateARea.m_activateEffect;
                privateArea.m_addPermittedEffect = guardStonePrivateARea.m_addPermittedEffect;
                privateArea.m_connectEffect = guardStonePrivateARea.m_connectEffect;
                privateArea.m_deactivateEffect = guardStonePrivateARea.m_deactivateEffect;
                privateArea.m_model = guardStonePrivateARea.m_model;
                privateArea.m_connectEffect = guardStonePrivateARea.m_connectEffect;
                privateArea.m_inRangeEffect = guardStonePrivateARea.m_inRangeEffect;
                privateArea.m_areaMarker = guardStonePrivateARea.m_areaMarker;
                privateArea.m_enabledEffect = guardStonePrivateARea.m_enabledEffect;

                Vector3 newScale = piece.transform.transform.localScale;
                newScale.x *= RaidSystem.Scale.Value;
                newScale.y *= RaidSystem.Scale.Value;
                newScale.z *= RaidSystem.Scale.Value;
                piece.transform.localScale = newScale;

                PieceManager.Instance.RegisterPieceInPieceTable(customRaidWard, "Hammer", "RaidSystem");

                if (!RaidSystem.OnlyAdminCanBuild.Value) continue;

                if (!SynchronizationManager.Instance.PlayerIsAdmin)
                {
                    table.m_pieces.Remove(customRaidWard);
                }
            }
        }

        private static void AddTeamTokens()
        {
            try
            {
                CustomItem CI = new CustomItem("BlueToken", "Thunderstone");
                ItemDrop itemDrop = CI.ItemDrop;
                itemDrop.m_itemData.m_shared.m_name = "Blue Token";
                itemDrop.m_itemData.m_shared.m_description = "Create your team ward.";
                itemDrop.m_itemData.m_shared.m_maxStackSize = 10;
                ItemManager.Instance.AddItem(CI);

                CustomItem CI2 = new CustomItem("RedToken", "Thunderstone");
                ItemDrop itemDrop2 = CI2.ItemDrop;
                itemDrop2.m_itemData.m_shared.m_name = "Red Token";
                itemDrop2.m_itemData.m_shared.m_description = "Create your team ward.";
                itemDrop2.m_itemData.m_shared.m_maxStackSize = 10;
                ItemManager.Instance.AddItem(CI2);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Error while adding cloned item: {ex.Message}");
            }
            finally
            {
                PrefabManager.OnPrefabsRegistered -= AddTeamTokens;
            }
        }
    }
}
