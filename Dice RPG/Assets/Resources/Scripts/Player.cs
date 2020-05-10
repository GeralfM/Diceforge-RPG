﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    public List<Item> myInventory = new List<Item>();

    public ObjectDisplayer myInventoryDisplayer;

    public GameObject myPV;
    public GameObject myGold;
    public GameObject myArmorStat;

    public EquipSlot myHead;
    public EquipSlot myLeftHand;
    public EquipSlot myRightHand;
    public EquipSlot myBody;

    public Player()
    {
        myInfo = new CharacterContent();
        myInfo.pvMax = 20;
        myInfo.pv = 20;
        myInfo.gold = 100;

        myBaseAttackDice = new Dice(new List<int> { 0 }, new List<string> { "Hit" });
        myBaseAttackDice.myOwner = this;
    }

    public override List<DiceFace> getAttack()
    {
        List<DiceFace> values = new List<DiceFace>();

        if (myLeftHand.equippedItem == null && myRightHand.equippedItem == null) { values.Add(myBaseAttackDice.rollDice()); }
        else
        {
            foreach (EquipSlot elt in new List<EquipSlot> { myLeftHand, myRightHand })
            {
                if (elt.equippedItem != null)
                {
                    DiceFace tryFace = elt.equippedItem.myDice.rollDice();
                    if (elt == myRightHand) { while (tryFace.value > 6) { tryFace = elt.equippedItem.myDice.rollDice(); } } // Attention au potentiel de boucle infinie !
                    values.Add(tryFace);
                }
            }
        }
        return values;
    }
    public override void takeHit(int value)
    {
        int dmgReduction = (myBody.equippedItem != null) ? myBody.equippedItem.myInfo.damageReduction : 0;
        myInfo.pv -= Mathf.Max( value - dmgReduction + SearchEffectValue("Vulnerable"), 0 );
    }

    public override List<Effect> GetAllEffects() // all ACTIVE effects only !
    {
        List<Effect> allEffects = new List<Effect>(); allEffects.AddRange(myConditions);
        // From equipment
        foreach (EquipSlot elt in new List<EquipSlot> { myHead, myLeftHand, myRightHand, myBody })
        {
            if(elt.equippedItem != null && elt.equippedItem.myEffects != null)
            {
                foreach(Effect eff in elt.equippedItem.myEffects)
                {
                    if( eff.rangeEffect == "character") { allEffects.Add(eff); }
                }
            }
        }
        return (allEffects);
    }

    public void AddToInventory(Item elt)
    {
        elt.SetOwner(this);
        myInventory.Add(elt);
    }

    public void DisplayInventory()
    {
        myInventoryDisplayer.DisplayItemCollection(myInventory);
    }
    public void DisplayPV() { myPV.GetComponent<Text>().text = myInfo.pv + " / " + myInfo.pvMax; }
    public void DisplayGold() { myGold.GetComponent<Text>().text = myInfo.gold+""; }

    public bool TryEquipItem(Item elt)
    {
        EquipSlot target = null;

        if (elt.myInfo.myType == "Weapon")
        {
            if (elt.myInfo.nbHands == 1) // il faut ajouter le test comme quoi un item sans faces <= 6 ne peut pas être équipé en secondaire !
            {
                if (myLeftHand.equippedItem == null) { target = myLeftHand; }
                else if (myLeftHand.equippedItem.myInfo.nbHands == 2) { myLeftHand.UnequipItem(); target = myLeftHand; }
                else if (myLeftHand.equippedItem.myInfo.nbHands == 1 && myRightHand.equippedItem == null && (elt.myDice == null || elt.myDice.GetMinValue() <= 6)) { target = myRightHand; }
            }
            else if (elt.myInfo.nbHands == 2)
            {
                myLeftHand.UnequipItem();
                myRightHand.UnequipItem();
                target = myLeftHand;
                BlockSlot(myRightHand, true);
            }
        }
        else if (elt.myInfo.myType == "Equipment")
        {
            if (elt.myInfo.target == "Body") {
                if(myBody.equippedItem != null) { myBody.UnequipItem(); }
                target = myBody;
            }
            if (elt.myInfo.target == "Head") {
                if (myHead.equippedItem != null) { myHead.UnequipItem(); }
                target = myHead;
            }
        }

        if (target != null) { EquipItem(elt, target); }
        return (target != null);
    }
    public void EquipItem(Item elt, EquipSlot slot)
    {
        slot.EquipItem(elt);
        myInventory.Remove(elt);
        UpdateVisualInfo();
    }
    public void BlockSlot(EquipSlot slot, bool isBlocked) // only visually for now
    {
        slot.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + ( isBlocked ? "BlackCross" : "UI_Mask" ) + ".png");
    }

    public void UpdateVisualInfo()
    {
        DisplayInventory();
        myPV.GetComponent<Text>().text = myInfo.pv + " / " + myInfo.pvMax;
        myArmorStat.GetComponent<Text>().text = (myBody.equippedItem != null) ? myBody.equippedItem.myInfo.damageReduction + "" : "";
        myGold.GetComponent<Text>().text = myInfo.gold + "";
    }

}
