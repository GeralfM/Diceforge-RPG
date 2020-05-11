﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemContent
{
    public string myName;
    public string myType;
    public string mySubType;
    public string myDescription;

    public int goldValue;
    public string myRarity; // not from json for now
    public int improved;
    public bool cursed=false;

    // WEAPONS
    public int nbHands;
    public List<int> faceValues;
    public List<string> faceEffects;

    // EQUIPMENT
    public string target;
    public int damageReduction;
    public string effect;
    public List<int> effectValues;
    public string effectRange;

    public ItemContent() { }
    public ItemContent(ItemContent toCopy)
    {
        myName = toCopy.myName; myType = toCopy.myType; mySubType = toCopy.mySubType; myDescription = toCopy.myDescription;
        goldValue = toCopy.goldValue; improved = toCopy.improved; cursed = toCopy.cursed;
        nbHands = toCopy.nbHands; faceValues = toCopy.faceValues; faceEffects = toCopy.faceEffects;
        target = toCopy.target; damageReduction = toCopy.damageReduction; effect = toCopy.effect; effectValues = toCopy.effectValues; effectRange = toCopy.effectRange;
    }
}
