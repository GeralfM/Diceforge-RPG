using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceFace
{
    public int value;
    public int upgradeCost;
    public List<Effect> effects = new List<Effect>(); // only used in getFaceSummary for now
    public List<Sprite> mySprites = new List<Sprite> { Resources.Load<Sprite>("Images/Dice_nothing.png") };

    public string faceName; // for loot dice only
    public bool alreadySummarized = false;

    public DiceFace(int val) { value = val; upgradeCost = 10 * (val + 1); }
    public DiceFace(string aName) { faceName = aName; mySprites.Add(Resources.Load<Sprite>("Images/Dice_blue.png")); }
    public DiceFace(DiceFace toCopy)
    {
        value = toCopy.value; upgradeCost = toCopy.upgradeCost;
        foreach (Effect eff in toCopy.effects) { effects.Add(new Effect(eff)); }
        foreach (Sprite elt in toCopy.mySprites) { mySprites.Add(elt); }
    }

    public List<string> myEffectNames()
    {
        List<string> answer = new List<string>();

        foreach(Effect eff in effects) { answer.Add(eff.nameEffect); }

        return answer;
    }

    public string getEffectDescriptionFromFace()
    {
        string returnDescr = null;
        switch (faceName)
        {
            case "Weak":
                returnDescr = "WEAK :\n\nDice rolls have -1";
                break;
            case "Vulnerable":
                returnDescr = "VULNERABLE :\n\nSuffers additional damage from hit";
                break;
            case "Strength":
                returnDescr = "STRONG :\n\nDice rolls are higher";
                break;
        }
        return returnDescr;
    }
}
