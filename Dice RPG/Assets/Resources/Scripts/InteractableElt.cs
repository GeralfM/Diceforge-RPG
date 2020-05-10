using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableElt : MonoBehaviour
{
    public UpgradeHandler myHandler;
    public LootHandler myLootHandler;
    public GameObject diceDisplayer;
    
    public Item myItemRef;
    public DiceFace myFaceRef;
    
    public void DisplaySprite()
    {
        Sprite resSprite = Resources.Load<Sprite>("Images/" + myItemRef.myInfo.myName + ".png");
        GetComponent<Image>().sprite = resSprite;
    }
    public void HideSprite()
    {
        Sprite resSprite = Resources.Load<Sprite>("Images/UI_Mask.png");
        GetComponent<Image>().sprite = resSprite;
        myItemRef = null; myFaceRef = null; // Risqué
    }
    public void DisplayRarityGem(bool display)
    {
        if (myItemRef != null && myItemRef.myInfo.myRarity != null && display)
        {
            this.transform.Find("RarityGem").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Gem_" + myItemRef.myInfo.myRarity + ".png");
            this.transform.Find("RarityGem").gameObject.SetActive(true);
        }
        else { this.transform.Find("RarityGem").gameObject.SetActive(false); }
    }

    public void DisplayDescription(bool display) // C'est très brouillon
    {
        if (myFaceRef!=null || myItemRef.myInfo.myDescription != null) gameObject.transform.Find("DescrBackground").gameObject.SetActive(display);
        if (display) {
            gameObject.transform.Find("DescrBackground").Find("DescrText").GetComponent<Text>().text = (myFaceRef != null)? 
                myFaceRef.getEffectDescriptionFromFace() : 
                myItemRef.myInfo.myDescription;
        }
    }

    public void TryEquip()
    {
        bool success = myItemRef.owner.TryEquipItem(myItemRef);
        if (success) { diceDisplayer.GetComponent<ObjectDisplayer>().Hide(); }
    }

    public void ClicAndCollect()
    {
        DisplayDice(false);
        myLootHandler.CollectLoot(myItemRef);
    }

    // FOR UPGRADE HANDLER
    public void HoverItem(bool isHovered) { myHandler.SomethingHovered(this, isHovered); }
    public void ClicItem() { myHandler.SomethingClicked(this); }
    public void DisplayDice(bool display)
    {
        if (myItemRef!=null && myItemRef.myDice != null)
        {
            if (display) { diceDisplayer.GetComponent<ObjectDisplayer>().DisplayDice(myItemRef.myDice); }
            else { diceDisplayer.GetComponent<ObjectDisplayer>().Hide(); }
        }
    }

    public void HoverFace(bool isHovered) { myHandler.SomeFaceHovered(this, isHovered); }
    public void ClicFace() { myHandler.SomeFaceClicked(this); }

    public void BuyFace() { myHandler.BuyFace(myFaceRef); }
    public void BuyItem() { myHandler.BuyItem(myItemRef); }

    // FOR MELT HANDLER
    public void ClicItemToMelt() { myHandler.SomethingClickedToMelt(this); }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
