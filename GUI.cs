using Jotunn.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;
using static RaidSystem.PlayerInfoService;
using System.Linq;
using System.Collections.Generic;

namespace RaidSystem
{
    class GUI
    {
        public static void ToggleMenu()
        {
            if (Player.m_localPlayer && (RaidSystem.Menu && !RaidSystem.Menu.activeSelf))
            {
                if (GUIManager.Instance == null)
                {
                    Debug.LogError("GUIManager instance is null");
                    return;
                }

                if (!GUIManager.CustomGUIFront)
                {
                    Debug.LogError("GUIManager CustomGUI is null");
                    return;
                }

                if (RaidSystem.HasTeam)                
                    LoadMenu();
                else
                {
                    GUIChooseTeam.CreateChooseTeamMenu();
                    return;
                }
            }

            bool state = !RaidSystem.Menu.activeSelf;

            RaidSystem.Menu.SetActive(state);
        }

        public static void DestroyMenu()
        {
            RaidSystem.Menu.SetActive(false);
        }

        public static void LoadMenu()
        {
            if (Player.m_localPlayer == null) return;

            UnityEngine.Object.Destroy(RaidSystem.Menu);

            foreach(var item in RaidSystem.menuItems)
            {
                UnityEngine.Object.Destroy(item.Value);
            }

            RaidSystem.menuItems = new();

            RaidSystem.Menu = GUIManager.Instance.CreateWoodpanel(
                                                                       parent: GUIManager.CustomGUIFront.transform,
                                                                       anchorMin: new Vector2(0.5f, 0.5f),
                                                                       anchorMax: new Vector2(0.5f, 0.5f),
                                                                       position: new Vector2(0, 0),
                                                                       width: 500,
                                                                       height: 700,
                                                                       draggable: true);
            RaidSystem.Menu.SetActive(false);

            GameObject scrollView = GUIManager.Instance.CreateScrollView(parent: RaidSystem.Menu.transform,
                    showHorizontalScrollbar: false,
                    showVerticalScrollbar: true,
                    handleSize: 8f,
                    handleColors: GUIManager.Instance.ValheimScrollbarHandleColorBlock,
                    handleDistanceToBorder: 50f,
                    slidingAreaBackgroundColor: new Color(0.1568628f, 0.1019608f, 0.0627451f, 1f),
                    width: 400f,
                    height: 380f
                );

            var tf = (RectTransform)scrollView.transform;
            tf.anchoredPosition = new Vector2(-20, -50);
            scrollView.SetActive(true);
            RaidSystem.menuItems.Add("scrollView", scrollView);

            string playerTeam = RaidSystem.localPlayerInfo.Team == "Blue" ? RaidSystem.TeamBlueAlias.Value : RaidSystem.TeamRedAlias.Value;

            GameObject teamTextObj = GUIManager.Instance.CreateText(
                text: playerTeam,
                parent: RaidSystem.Menu.transform,
                anchorMin: new Vector2(0,0),
                anchorMax: new Vector2(0,0),
                position: new Vector2(200, 640),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 350f,
                height: 40f,
                addContentSizeFitter: false);
            RaidSystem.menuItems.Add("teamText", teamTextObj);

            GameObject realmsConquestedText = GUIManager.Instance.CreateText(
          text: RaidSystem.TerritoriesConquestedText.Value + " " + Util.GetTerritoriesConquestedText(),
          parent: RaidSystem.Menu.transform,
          anchorMin: new Vector2(0, 0),
          anchorMax: new Vector2(0, 0),
          position: new Vector2(370, 612),
          font: GUIManager.Instance.AveriaSerifBold,
          fontSize: 18,
          color: GUIManager.Instance.ValheimOrange,
          outline: true,
          outlineColor: Color.black,
          width: 350f,
          height: 40f,
          addContentSizeFitter: false);
            RaidSystem.menuItems.Add("realmsConquestedText", realmsConquestedText);

            GameObject playerNameObj = GUIManager.Instance.CreateText(
                text: RaidSystem.localPlayerInfo.Nick,
                parent: RaidSystem.Menu.transform,
                anchorMin: new Vector2(0,0),
                anchorMax: new Vector2(0,0),
                position: new Vector2(200, 590),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 25,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 350f,
                height: 50f,
                addContentSizeFitter: false);
            RaidSystem.menuItems.Add("playerNameText", playerNameObj);

            GameObject descriptionInputObj = GUIManager.Instance.CreateInputField(
            placeholderText: RaidSystem.localPlayerInfo.Description,
            parent: RaidSystem.Menu.transform,
            anchorMin: new Vector2(0, 0),
            anchorMax: new Vector2(0, 0),
            position: new Vector2(160, 560),
            contentType: InputField.ContentType.Standard,
            fontSize: 18,
            width: 270f,
            height: 30f
            );

            RaidSystem.menuItems.Add("descriptionInputObj", descriptionInputObj);

            GameObject buttonUpdate = GUIManager.Instance.CreateButton(
                text: RaidSystem.ButtonUpdateText.Value,
                parent: RaidSystem.Menu.transform,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0f, 0f),
                position: new Vector2(360, 560),
                width: 90,
                height: 30f);
            buttonUpdate.SetActive(true);

            RaidSystem.menuItems.Add("buttonUpdate", buttonUpdate);
            InputField inputComponent = descriptionInputObj.GetComponent<InputField>();
            inputComponent.text = RaidSystem.localPlayerInfo.Description;
            Button buttonUpdater = buttonUpdate.GetComponent<Button>();
            buttonUpdater.onClick.AddListener(delegate { new PlayerInfoService().UpdatePlayer(inputComponent); });    

            CreateItems(scrollView);

            scrollView.transform.Find("Scroll View").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;

            GameObject TextRealmList = GUIManager.Instance.CreateText(
    text: RaidSystem.TeamMemberListText.Value,
    parent: RaidSystem.Menu.transform,
    anchorMin: new Vector2(0, 0),
    anchorMax: new Vector2(0, 0),
    position: new Vector2(200, 490),
    font: GUIManager.Instance.AveriaSerifBold,
    fontSize: 25,
    color: GUIManager.Instance.ValheimOrange,
    outline: true,
    outlineColor: Color.black,
    width: 350f,
    height: 40f,
    addContentSizeFitter: false);
            RaidSystem.menuItems.Add("textRealmLIst", TextRealmList);

            GameObject buttonObject = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: RaidSystem.Menu.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, -300f),
                width: 170,
                height: 45f);
            buttonObject.SetActive(true);
            RaidSystem.menuItems.Add("buttonObject", buttonObject);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(DestroyMenu);
        }

        private static void CreateItems(GameObject scrollView)
        {
            GameObject x = GUIManager.Instance.CreateText(
                text: "\n",
             parent: scrollView.transform.Find("Scroll View/Viewport/Content"),
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 10,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 150f,
                height: 30f,
                addContentSizeFitter: false);

            RaidSystem.menuItems.Add("x", x);


            foreach (PlayerInfo playerInfo in RaidSystem.PlayerInfoList.Where(x => x.Team == RaidSystem.localPlayerInfo.Team).ToList())
            {
                GameObject nameText = GUIManager.Instance.CreateText(
                    text: playerInfo.Nick,
                 parent: scrollView.transform.Find("Scroll View/Viewport/Content"),
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 20,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 150f,
                    height: 25f,
                    addContentSizeFitter: false);
                RaidSystem.menuItems.Add("nameText" + Guid.NewGuid(), nameText);

                GameObject descriptionText = GUIManager.Instance.CreateText(
                  text: playerInfo.Description,
             parent: scrollView.transform.Find("Scroll View/Viewport/Content"),
                  anchorMin: new Vector2(0.5f, 1f),
                  anchorMax: new Vector2(0.5f, 1f),
                  position: new Vector2(0, 0f),
            font: GUIManager.Instance.AveriaSerifBold,
                  fontSize: 18,
                  color: GUIManager.Instance.ValheimOrange,
                  outline: true,
                  outlineColor: Color.black,
                  width: 300f,
                  height: 20f,
                  addContentSizeFitter: false);

                RaidSystem.menuItems.Add("descriptionText" + Guid.NewGuid(), descriptionText);

                GameObject spacador = GUIManager.Instance.CreateText(
                    text: "",
                 parent: scrollView.transform.Find("Scroll View/Viewport/Content"),
                    anchorMin: new Vector2(0.5f, -5f),
                    anchorMax: new Vector2(0.5f, -5f),
                    position: new Vector2(0f, -20f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 10,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 150f,
                    height: 10f,
                    addContentSizeFitter: false);
                RaidSystem.menuItems.Add("spacador" + Guid.NewGuid(), spacador);
            }
        }
    }
}
