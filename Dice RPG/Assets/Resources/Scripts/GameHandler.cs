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

    // Start is called before the first frame update
    void Start()
    {
        myData = new MetaJson();

        myLoreHandler.myData = myData; myLoreHandler.InitializeLore();
        myCombatHandler.allMobs = myData.allMobs;
        myLootHandler.allItems = myData.allItems; myLootHandler.GeneratePoolItems(); myLootHandler.GeneratePoolItemsShop();
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
        currentScene = confrontScene;
        confrontScene.SetActive(true);
        myCombatHandler.InitNewConfront();
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
            string nextScene = sceneProb.Draw(true);
            lootScene.SetActive(false);

            switch (nextScene)
            {
                case "Heal":
                    sceneProb.AddWeight("Heal", -0.2f);
                    currentScene = healScene;
                    healScene.SetActive(true);
                    myHealingHandler.InitializeScene(thePlayer); break;
                case "Upgrade":
                    sceneProb.AddWeight("Upgrade", -0.2f);
                    currentScene = upgradeScene;
                    upgradeScene.SetActive(true);
                    myUpgradeHandler.InitUpgradeScene(); break;
                case "nothing":
                    currentScene = confrontScene;
                    confrontScene.SetActive(true);
                    myCombatHandler.InitNewConfront(); break;
            }
        }
        else if(new List<GameObject> { healScene, upgradeScene }.Contains(currentScene))
        {
            currentScene.SetActive(false);
            currentScene = confrontScene;
            confrontScene.SetActive(true);
            myCombatHandler.InitNewConfront();
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

    public void ReinitSession()
    {
        myLoreHandler.ResetLore();
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
