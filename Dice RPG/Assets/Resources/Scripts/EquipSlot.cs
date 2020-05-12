using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    public GameObject diceDisplayer;

    public Item equippedItem;
    public string slotName;
    
    public void EquipItem(Item elt)
    {
        equippedItem = elt;
        Sprite resSprite = Resources.Load<Sprite>("Images/" + equippedItem.myInfo.myName + ".png");
        GetComponent<Image>().sprite = resSprite;
        if (equippedItem.myInfo.myRarity != null)
        {
            this.transform.Find("RarityGem").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Gem_" + equippedItem.myInfo.myRarity + ".png");
            this.transform.Find("RarityGem").gameObject.SetActive(true);
        }
        if (equippedItem.myInfo.cursed) { this.transform.Find("CurseSymbol").gameObject.SetActive(true); }
    }
    public void UnequipItem()
    {
        if (equippedItem != null)
        {
            Player thePlayer = equippedItem.owner;
            equippedItem.owner.AddToInventory(equippedItem);
            equippedItem.owner.DisplayInventory();
            DisplayDice(false);
            if(equippedItem.myInfo.nbHands == 2) { thePlayer.BlockSlot(thePlayer.myRightHand, false); }

            equippedItem = null;

            thePlayer.UpdateVisualInfo();

            GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Slot_"+slotName+".png");
            
            this.transform.Find("RarityGem").gameObject.SetActive(false);
            this.transform.Find("CurseSymbol").gameObject.SetActive(false);

            GameObject.Find("Background").GetComponent<GameHandler>().DisplayInventory(true);
        }
    }

    public void DisplayDice(bool display)
    {
        
        if (equippedItem != null && equippedItem.myDice != null)
        {
            if (display) { diceDisplayer.GetComponent<ObjectDisplayer>().DisplayDice(equippedItem.myDice); }
            else { diceDisplayer.GetComponent<ObjectDisplayer>().Hide(); }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
