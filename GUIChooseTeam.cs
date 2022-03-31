using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace RaidSystem
{
    public class GUIChooseTeam
    {
        public static GameObject menu;

        public static void CreateChooseTeamMenu()
        {
            if (menu && menu.activeSelf)
            {
                DestroyMenu();
                return;
            }

            menu = GUIManager.Instance.CreateWoodpanel(
                                    parent: GUIManager.CustomGUIFront.transform,
                                    anchorMin: new Vector2(0.5f, 0.5f),
                                    anchorMax: new Vector2(0.5f, 0.5f),
                                    position: new Vector2(0, 0),
                                    width: 380,
                                    height: 220,
                                    draggable: true);

            GameObject textObject = GUIManager.Instance.CreateText(
                text: RaidSystem.ChooseTeamMessage.Value,
                parent: menu.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -60f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 25,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 300f,
                height: 40f,
                addContentSizeFitter: false);

            GameObject blueTeamButton = GUIManager.Instance.CreateButton(
               text: RaidSystem.TeamBlueAlias.Value,
               parent: menu.transform,
               anchorMin: new Vector2(0.5f, 0.5f),
               anchorMax: new Vector2(0.5f, 0.5f),
               position: new Vector2(90, -30),
               width: 150,
               height: 50f);
            blueTeamButton.SetActive(true);

            Button buttonTeamBlue = blueTeamButton.GetComponent<Button>();
            buttonTeamBlue.onClick.AddListener(delegate { new PlayerInfoService().PreparePlayerToSend("Blue", ""); });

            GameObject redTeamButton = GUIManager.Instance.CreateButton(
               text: RaidSystem.TeamRedAlias.Value,
               parent: menu.transform,
               anchorMin: new Vector2(0.5f, 0.5f),
               anchorMax: new Vector2(0.5f, 0.5f),
               position: new Vector2(-90, -30),
               width: 150,
               height: 50f);
            redTeamButton.SetActive(true);

            Button buttonRedTeam = redTeamButton.GetComponent<Button>();
            buttonRedTeam.onClick.AddListener(delegate { new PlayerInfoService().PreparePlayerToSend("Red", ""); });

            menu.SetActive(true);
        }



        public static void DestroyMenu()
        {
            try
            {
                menu.SetActive(false);
            }
            catch
            {

            }
        }
    }
}
