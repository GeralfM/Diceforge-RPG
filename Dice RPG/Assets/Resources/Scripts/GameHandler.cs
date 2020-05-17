using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public ConfrontationHandler myCombatHandler;
    public HealingHandler myHealingHandler;
    public UpgradeHandler myUpgradeHandler;
    public LootHandler myLootHandler;
    public LoreHandler myLoreHandler;
    public MetaJson myData;
    public Player thePlayer;

    public GameObject currentScene;
    public GameObject confrontScene;
    public GameObject lootScene;
    public GameObject healScene;
    public GameObject upgradeScene;
    public GameObject deathScene;
    public GameObject eventScene;

    public GameObject playerInventory;
    public GameObject menu;
    public GameObject playerDiceDetail;
    public GameObject playerPV;
    public GameObject playerArmor;
    public GameObject playerGold;
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject body;
    public GameObject necklace;
    public GameObject playerConditionsTab;

    public GameObject cheatCodeButton;

    public WeightedBag sceneProb;

    public GameObject followerIcon;
    public bool specialEvent = false;
    public string nextScene;

    // Start is called before the first frame update
    void Start()
    {
        myData = new MetaJson();

        myLoreHandler.myData = myData; myLoreHandler.InitializeLore();
        myCombatHandler.allMobs = myData.allMobs;
        myLootHandler.allItems = myData.allItems; myLootHandler.GeneratePoolItems(); myLootHandler.GeneratePoolItemsShop();
        SetFollower();
        NewGame();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("escape")) { Application.Quit(); }
    }

    public void NewGame()
    {
        if (currentScene != null) { currentScene.SetActive(false); }
        deathScene.SetActive(false);
        myLootHandler.followerTab.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text = "";
        sceneProb = new WeightedBag();

        thePlayer = new Player();
        initializePlayer();
        myCombatHandler.thePlayer = thePlayer; myCombatHandler.levelConfront = 0; myCombatHandler.mobPool = new WeightedBag();
        myUpgradeHandler.thePlayer = thePlayer;
        myLootHandler.thePlayer = thePlayer;

        sceneProb.AddElement("Heal", 0f); sceneProb.AddElement("Upgrade", 0f);

        //currentScene = upgradeScene;
        //upgradeScene.SetActive(true);
        //myUpgradeHandler.InitUpgradeScene();
        nextScene = "Confront";
        CheckSpecialEvent();
    }
    public void EndOfConfrontation(bool playerDead)
    {
        confrontScene.SetActive(false);
        if (playerDead) {
            TerminatePlayer();
            deathScene.SetActive(true);
        }
        else {
            sceneProb.AddWeight("Heal", 0.05f); sceneProb.AddWeight("Upgrade", 0.05f);

            currentScene = lootScene;
            lootScene.SetActive(true); DisplayInventory(false);
            myLootHandler.InitializeLoot(myCombatHandler.enemy);
        }
    }
    public void TerminatePlayer()
    {
        myCombatHandler.Interrupt();

        foreach (EquipSlot elt in new List<EquipSlot> { thePlayer.myHead, thePlayer.myLeftHand, thePlayer.myRightHand, thePlayer.myBody, thePlayer.myNecklace }) { elt.UnequipItem(); }
        thePlayer.myConditions = new List<Effect>();
        thePlayer.DisplayConditions();
        DisplayInventory(false);
    }

    public void NextScene()
    {
        if(currentScene == lootScene)
        {
            nextScene = sceneProb.Draw(true);
            if(nextScene == "nothing") { nextScene = "Confront"; }
            myLootHandler.followerTab.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text = "";
            lootScene.SetActive(false);
        }
        else if(new List<GameObject> { healScene, upgradeScene }.Contains(currentScene))
        {
            nextScene = "Confront";
            currentScene.SetActive(false);
        }
        CheckSpecialEvent();
    }
    public void CheckSpecialEvent()
    {
        myLoreHandler.ExecuteFromTrigger("New_" + nextScene, null);
        if (!specialEvent) { SetupNextScene(); }
        else { eventScene.SetActive(true); currentScene = eventScene; myLoreHandler.SetupEventScene(); }
    }
    public void SetupNextScene()
    {
        specialEvent = false;
        switch (nextScene)
        {
            case "Heal":
                sceneProb.AddWeight("Heal", -0.2f);
                currentScene = healScene;
                currentScene.SetActive(true);
                myHealingHandler.InitializeScene(thePlayer); break;
            case "Upgrade":
                sceneProb.AddWeight("Upgrade", -0.2f);
                currentScene = upgradeScene;
                currentScene.SetActive(true);
                myUpgradeHandler.InitUpgradeScene(); break;
            case "Confront":
                currentScene = confrontScene;
                currentScene.SetActive(true);
                myCombatHandler.InitNewConfront(); break;
        }
    }

    public void SwitchDisplayInventory() { if (!deathScene.activeSelf) { DisplayInventory(!playerInventory.activeSelf); } }
    public void DisplayInventory(bool active)
    {
        playerInventory.SetActive(active); currentScene.SetActive(!active);
        if (active && menu.activeSelf) { DisplayMenu(false); };
        thePlayer.DisplayInventory();
    }
    public void SwitchDisplayMenu() { if (!deathScene.activeSelf) { DisplayMenu(!menu.activeSelf); } }
    public void DisplayMenu(bool active)
    {
        menu.SetActive(active);
        if (active && playerInventory.activeSelf) { DisplayInventory(false); }
    }

    public void initializePlayer()
    {   
        thePlayer.myInventoryDisplayer = playerInventory.GetComponent<ObjectDisplayer>();

        thePlayer.myPV = playerPV;
        thePlayer.myArmorStat = playerArmor;
        thePlayer.myGold = playerGold;

        thePlayer.myHead = head.GetComponent<EquipSlot>();
        thePlayer.myLeftHand = leftHand.GetComponent<EquipSlot>();
        thePlayer.myRightHand = rightHand.GetComponent<EquipSlot>();
        thePlayer.myBody = body.GetComponent<EquipSlot>();
        thePlayer.myNecklace = necklace.GetComponent<EquipSlot>();

        thePlayer.myConditionsDisplayer = playerConditionsTab.GetComponent<ObjectDisplayer>();

        foreach (string elt in new List<string> { "Dagger" }) // only one dagger
        {
            Item newItem = new Item(myData.allItems[elt]);
            thePlayer.AddToInventory(newItem);
            thePlayer.TryEquipItem(newItem);
        }
        
        thePlayer.UpdateVisualInfo();
    }

    // SESSION-LEVEL TOOLS
    public void SelectNewFollower()
    {
        int followerChoice = Random.Range(1, 4); // Il va falloir gérer la multiplicité des icônes
        followerIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Follower_" + followerChoice + ".png");

        myData.allCounters["Selected_Follower"].count = followerChoice;
        myData.StoreCounters();
    }
    public void SetFollower()
    {
        int followerChoice = myData.allCounters["Selected_Follower"].count;
        if (followerChoice == 0) { SelectNewFollower(); }
        else { followerIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Follower_" + followerChoice + ".png"); }
    }

    public void ReinitSession()
    {
        currentScene.SetActive(false);

        myLoreHandler.ResetLore();
        myCombatHandler.allMobs = myData.allMobs;
        myLootHandler.allItems = myData.allItems;

        SelectNewFollower();
        TerminatePlayer();
    }

    public void CheatCode()
    {
        foreach (ItemContent elt in myData.allItems.Values)
        {
            if (elt.unlocked) { thePlayer.AddToInventory(new Item(myData.allItems[elt.myName])); }
        }
        thePlayer.DisplayInventory();
        cheatCodeButton.SetActive(false);
    }
}
