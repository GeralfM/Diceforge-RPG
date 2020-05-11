using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterContent
{
    public string myName;
    public string myRarity; // not from json

    public int pvMax;
    public int pv; // not from json
    public int gold;

    public List<int> faceValues;
    public List<string> faceEffects;
    public List<int> faceSplits;
    public int levelAppear;
    public int presenceRate;
    public int lootChoices;
    public int lootValue;

    public CharacterContent() { }
    public CharacterContent(CharacterContent toCopy)
    {
        myName = toCopy.myName;
        pvMax = toCopy.pvMax; gold = toCopy.gold;
        faceValues = toCopy.faceValues; faceEffects = toCopy.faceEffects; faceSplits = toCopy.faceSplits;
        levelAppear = toCopy.levelAppear; presenceRate = toCopy.presenceRate;
        lootChoices = toCopy.lootChoices; lootValue = toCopy.lootValue;
    }
}
