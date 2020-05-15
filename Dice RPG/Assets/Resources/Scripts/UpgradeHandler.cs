using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeHandler : MonoBehaviour
{
    public LootHandler myItemGenerator;

    public ObjectDisplayer myItemDisplayer;
    public ObjectDisplayer myUpgradedFacesDisplayer;
    public GameObject myFaceDisplayer;

    public GameObject TradeTab;
    public GameObject UpgradeTab;
    public GameObject MeltTab;

    // FROM SHOP TAB
    public GameObject ButtonSell;
    public List<Item> buyCollection;
    public ObjectDisplayer buyCollectionDisplayer;
    // FROM MELT TAB
    public InteractableElt toMelt;
    public InteractableElt melted;
    public List<Item> meltCollection;
    public List<Item> meltSelection;
    public GameObject meltButton;
    public GameObject meltMenu;
    public ObjectDisplayer meltCollectionDisplayer;
    

    public InteractableElt currentlySelectedItem;
    public InteractableElt currentlySelectedFace;

    public Player thePlayer;

    // Start is called before the first frame update
    void Start() { }
    // Update is called once per frame
    void Update() { }

    public void InitUpgradeScene()
    {
        DisplayItemCollection();
        PrepareToSell(false);
        PrepareShopInventory();

        ClicButtonTab("None");
    }

    public void DisplayItemCollection()
    {
        List<Item> toBeDisplayed = new List<Item>();
        foreach(Item it in thePlayer.myInventory )
        {
            if (!(UpgradeTab.activeSelf && it.myInfo.myType != "Weapon")) { toBeDisplayed.Add(it); }
        }
        foreach(EquipSlot slot in new List<EquipSlot> { thePlayer.myHead, thePlayer.myLeftHand, thePlayer.myRightHand, thePlayer.myBody, thePlayer.myNecklace })
        {
            if (slot.equippedItem != null && !(UpgradeTab.activeSelf && slot.equippedItem.myInfo.myType != "Weapon")) { toBeDisplayed.Add(slot.equippedItem); }
        }

        myItemDisplayer.page = 0;
        myItemDisplayer.DisplayItemCollection(toBeDisplayed);
        myItemDisplayer.ActualizeArrow();
    }

    public void SomethingHovered(InteractableElt target, bool isHovered)
    {
        if (currentlySelectedItem == null) { target.DisplayDice(isHovered); }
    }
    public void SomethingClicked(InteractableElt target)
    {
        if (currentlySelectedItem == null)
        {
            currentlySelectedItem = target; currentlySelectedItem.transform.Find("ItemFrame").gameObject.SetActive(true);
            PrepareToSell(true); PrepareToMelt(true);
        }
        else if (currentlySelectedItem == target)
        {
            currentlySelectedItem.transform.Find("ItemFrame").gameObject.SetActive(false); currentlySelectedItem = null;
            PrepareToSell(false); PrepareToMelt(false);
        }
        else
        {
            if (currentlySelectedFace != null) { DisplayFace(null, false); currentlySelectedFace = null; }
            currentlySelectedItem.transform.Find("ItemFrame").gameObject.SetActive(false);
            currentlySelectedItem.DisplayDice(false);
            currentlySelectedItem = target;
            currentlySelectedItem.DisplayDice(true);
            currentlySelectedItem.transform.Find("ItemFrame").gameObject.SetActive(true);

            PrepareToSell(true); PrepareToMelt(true);
        }
    }

    public void ClicButtonTab(string whichTab)
    {
        NullifySelected();
        foreach(GameObject aTab in new List<GameObject> { TradeTab, UpgradeTab, MeltTab }) { aTab.SetActive(false); }
        switch (whichTab)
        {
            case "Trade":
                TradeTab.SetActive(true);
                DisplayItemsToBuy();
                break;
            case "Upgrade":
                UpgradeTab.SetActive(true);
                break;
            case "Melt":
                MeltTab.SetActive(true);
                break;
        }
        DisplayItemCollection();
    }

    // UPGRADE PART

    public void SomeFaceHovered(InteractableElt target, bool isHovered)
    {
        if (currentlySelectedFace == null)
        {
            bool isCursed = false; target.myFaceRef.effects.ForEach(x => isCursed = isCursed || (x.nameEffect == "Cursed"));

            DisplayFace(target.gameObject, isHovered);
            if (isHovered && !isCursed) { myUpgradedFacesDisplayer.DisplayDice(GenerateUpgrades(target.myFaceRef)); }
            else { myUpgradedFacesDisplayer.Hide(); }
        }
    }
    public void SomeFaceClicked(InteractableElt target)
    {
        if (currentlySelectedFace == null) { currentlySelectedFace = target; currentlySelectedFace.transform.Find("DiceSelect").gameObject.SetActive(true); }
        else if (currentlySelectedFace == target) { currentlySelectedFace.transform.Find("DiceSelect").gameObject.SetActive(false); currentlySelectedFace = null; }
        else
        {
            currentlySelectedFace.transform.Find("DiceSelect").gameObject.SetActive(false);
            DisplayFace(target.gameObject, false);
            myUpgradedFacesDisplayer.Hide();

            currentlySelectedFace = target;

            DisplayFace(target.gameObject, true);
            currentlySelectedFace.transform.Find("DiceSelect").gameObject.SetActive(true);
            myUpgradedFacesDisplayer.DisplayDice(GenerateUpgrades(target.myFaceRef));
        }
    }
    public void DisplayFace(GameObject model, bool display)
    {
        if (display)
        {
            GameObject aCopy = Instantiate(model, new Vector3(0, 0, -1), Quaternion.identity);
            aCopy.transform.SetParent(myFaceDisplayer.transform);
            aCopy.transform.localScale = new Vector3(0.16f, 0.19f, 0);
            aCopy.transform.localPosition = new Vector3(-2.62f, 0, 0);
            aCopy.transform.Find("DiceSelect").gameObject.SetActive(false);
            aCopy.name = "displayedFace";
        }
        else { Destroy(myFaceDisplayer.transform.Find("displayedFace").gameObject); myUpgradedFacesDisplayer.Hide(); }
    }

    public void NullifySelected()
    {
        if (currentlySelectedItem != null)
        {
            currentlySelectedItem.transform.Find("ItemFrame").gameObject.SetActive(false);
            currentlySelectedItem.DisplayDice(false);
            currentlySelectedItem = null;
        }
        if (currentlySelectedFace != null) { DisplayFace(null, false); currentlySelectedFace = null; }

        PrepareToSell(false); PrepareToMelt(false);
    }

    public Dice GenerateUpgrades( DiceFace toUpgrade )
    {
        Dice improvedDice = new Dice(null, new List<int>(), new List<string>(), null);
        improvedDice.myItem = currentlySelectedItem.myItemRef;

        // Adding 1 : solution by default
        DiceFace newFace = new DiceFace(toUpgrade);
        newFace.value++;
        improvedDice.myFaces.Add( newFace );
        improvedDice.myOwner = thePlayer;

        // Adding other effects
        List<string> otherEffets = new List<string>();
        foreach (DiceFace aFace in currentlySelectedItem.myItemRef.myDice.myFaces)
        {
            foreach(Effect eff in aFace.effects)
            {
                if (!otherEffets.Contains(eff.nameEffect) && !toUpgrade.myEffectNames().Contains(eff.nameEffect) && eff.nameEffect!="Cursed")
                {
                    otherEffets.Add(eff.nameEffect);
                    newFace = new DiceFace(toUpgrade);
                    newFace.effects.Add(new Effect(eff));
                    improvedDice.myFaces.Add(newFace);
                }
            }
        }

        return improvedDice;
    }
    public void BuyFace(DiceFace aFace)
    {
        thePlayer.myInfo.gold -= aFace.upgradeCost * (int)Mathf.Pow(2, currentlySelectedItem.myItemRef.myInfo.improved) ;
        currentlySelectedItem.myItemRef.myInfo.improved++;

        int replaceInd = currentlySelectedItem.myItemRef.myDice.myFaces.IndexOf(currentlySelectedFace.myFaceRef);
        aFace.upgradeCost += (aFace.value != currentlySelectedItem.myItemRef.myDice.myFaces[replaceInd].value) ?10:0;
        currentlySelectedItem.myItemRef.myDice.myFaces[replaceInd] = aFace;

        thePlayer.myGold.GetComponent<Text>().text = thePlayer.myInfo.gold+"";
        NullifySelected();
        DisplayItemCollection();
    }

    // BUY & SELL PART

    public void PrepareShopInventory()
    {
        buyCollection = new List<Item>();

        foreach(string elt in new List<string> { "Low", "Middle", "High" })
        {
            Item newItem = new Item(myItemGenerator.allItems[myItemGenerator.shopPools[elt].Draw(false)]);
            newItem.owner = thePlayer;
            if(newItem.myDice != null) { newItem.myDice.myOwner = thePlayer; }
            buyCollection.Add(newItem);
        }
    }
    public void DisplayItemsToBuy()
    {
        buyCollectionDisplayer.DisplayItemCollection(buyCollection);
    }
    public void BuyItem(Item anItem)
    {
        thePlayer.myInfo.gold -= anItem.myInfo.goldValue;
        thePlayer.AddToInventory(anItem);
        thePlayer.UpdateVisualInfo();
        buyCollection.Remove(anItem);

        DisplayItemsToBuy();
        DisplayItemCollection();
    }

    public void PrepareToSell(bool display)
    {
        bool canBeSold = display && !currentlySelectedItem.myItemRef.myInfo.cursed;
        string aText = null;

        if (display && currentlySelectedItem.myItemRef.myInfo.cursed) { aText = "Cursed items can't be sold !"; }
        else if (display) { aText = "Sell selected item for " + (currentlySelectedItem.myItemRef.myInfo.goldValue / 2) + " gold"; }
        else { aText = "Select an item to sell"; }

        ButtonSell.transform.Find("Text").GetComponent<Text>().text = aText;
        ButtonSell.GetComponent<Button>().interactable = canBeSold;
    }
    public void SellItem()
    {
        // Check if equiped
        foreach(EquipSlot slot in new List<EquipSlot> { thePlayer.myHead, thePlayer.myLeftHand, thePlayer.myRightHand, thePlayer.myBody, thePlayer.myNecklace })
            { if(slot.equippedItem == currentlySelectedItem.myItemRef)
                { slot.UnequipItem();
                    GameObject.Find("Background").GetComponent<GameHandler>().DisplayInventory(false);
                } }
        thePlayer.myInventory.Remove(currentlySelectedItem.myItemRef); // L'item est forcément dans l'inventaire

        thePlayer.myInfo.gold += currentlySelectedItem.myItemRef.myInfo.goldValue / 2;
        thePlayer.UpdateVisualInfo();
        DisplayItemCollection();

        NullifySelected();
    }

    // MELT PART

    public void PrepareToMelt(bool display)
    {
        if (display && currentlySelectedItem.myItemRef.myInfo.myRarity != "Legendary") {
            toMelt.myItemRef = currentlySelectedItem.myItemRef; toMelt.DisplaySprite(); toMelt.DisplayCurse(true);

            // Generate melted overview
            Item newItem = new Item(myItemGenerator.allItems[currentlySelectedItem.myItemRef.myInfo.myName]);
            newItem.SetOwner(thePlayer);
            string rarity = (currentlySelectedItem.myItemRef.myInfo.myRarity == null) ? "Rare" :
                (currentlySelectedItem.myItemRef.myInfo.myRarity == "Rare") ? "Epic" : "Legendary";
            myItemGenerator.SetRarity(newItem, rarity);
            melted.myItemRef = newItem; melted.DisplaySprite(); melted.DisplayRarityGem(true);

            // Check if we have three exemplaries
            meltCollection = new List<Item>(); meltSelection = new List<Item>();
            foreach (Item elt in thePlayer.myInventory)
            { if (elt.myInfo.myName == currentlySelectedItem.myItemRef.myInfo.myName && elt.myInfo.myRarity == currentlySelectedItem.myItemRef.myInfo.myRarity)
                { meltCollection.Add(elt); } }
            foreach (EquipSlot slot in new List<EquipSlot> { thePlayer.myHead, thePlayer.myLeftHand, thePlayer.myRightHand, thePlayer.myBody, thePlayer.myNecklace })
            { if (slot.equippedItem != null && slot.equippedItem.myInfo.myName == currentlySelectedItem.myItemRef.myInfo.myName && slot.equippedItem.myInfo.myRarity == currentlySelectedItem.myItemRef.myInfo.myRarity)
                { meltCollection.Add(slot.equippedItem); } }

            if(meltCollection.Count>=3) { meltButton.GetComponent<Button>().interactable = true; }
        }
        else {
            toMelt.HideSprite(); toMelt.DisplayCurse(false);
            melted.HideSprite(); 
            meltButton.GetComponent<Button>().interactable = false;

            DisplayMeltingIngredients(false);
        }
    }

    public void DisplayMeltingIngredients(bool display)
    {
        if (display)
        {
            meltMenu.SetActive(true);
            meltCollectionDisplayer.DisplayItemCollection(meltCollection);
            thePlayer.SetSlotInteractability(false);
        }
        else
        {
            meltMenu.SetActive(false);
            meltCollectionDisplayer.Hide();
            meltSelection = new List<Item>();
            thePlayer.SetSlotInteractability(true);
        }
    }

    public void ClicMelt()
    {
        if(meltCollection.Count == 3) { Melt(meltCollection); }
        else if(meltSelection.Count == 3) { Melt(meltSelection); }
        else if(meltCollection.Count > 3 && !meltMenu.activeSelf) { DisplayMeltingIngredients(true); }
    }
    public void Melt(List<Item> toMelt)
    {
        // Check if cursed
        int nbCurse = 0;
        toMelt.ForEach(x => nbCurse += x.myInfo.cursed ? 1 : 0);
        if( Random.Range(0f,1f) < nbCurse/3f ) { melted.myItemRef.SetCurse(); }
        // Check if equiped
        foreach (EquipSlot slot in new List<EquipSlot> { thePlayer.myHead, thePlayer.myLeftHand, thePlayer.myRightHand, thePlayer.myBody, thePlayer.myNecklace })
        {
            if (toMelt.Contains(slot.equippedItem))
            {
                slot.UnequipItem();
                GameObject.Find("Background").GetComponent<GameHandler>().DisplayInventory(false);
            }
        }
        thePlayer.myInventory.RemoveAll(x=> toMelt.Contains(x) ); // Les items sont forcément dans l'inventaire

        thePlayer.AddToInventory(melted.myItemRef);
        DisplayItemCollection();

        NullifySelected();
    }

    public void SomethingClickedToMelt(InteractableElt target)
    {
        if (meltSelection.Contains(target.myItemRef))
        {
            target.transform.Find("ItemFrame").gameObject.SetActive(false);
            meltSelection.Remove(target.myItemRef);
        }
        else
        {
            target.transform.Find("ItemFrame").gameObject.SetActive(true);
            meltSelection.Add(target.myItemRef);
        }
    }
}
