using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Dice
{
    public List<DiceFace> myFaces = new List<DiceFace>();
    public List<int> faceSplits;
    public Character myOwner;
    public Item myItem;

    public bool isConditionDice = false; // for conditions only

    public System.Random ran = new System.Random();

    public Dice(List<int> fValues, List<string> fEffects)
    {
        for (int i=0; i<fValues.Count; i++)
        {
            DiceFace newFace = new DiceFace(fValues[i]);
            if(fEffects.Count>0 && fEffects[i]!="") { newFace.effects.Add(new Effect(fEffects[i], null, null)); }
            myFaces.Add(newFace);
        }
    }
    public Dice(List<DiceFace> faces) { myFaces = faces; }
    public Dice(List<string> nameFaces)
    {
        for (int i = 0; i < nameFaces.Count; i++) { myFaces.Add(new DiceFace(nameFaces[i])); }
    }

    public DiceFace rollFromRange(int idmin, int idmax) { return getFaceSummary(myFaces[Random.Range(idmin, idmax)]);  }
    public DiceFace rollDice(){ return getFaceSummary(myFaces[Random.Range(0, myFaces.Count)]); }
    public List<DiceFace> rollDistinctFaces(int N) // only for loot dice for now
    { 
        List<DiceFace> selected =  myFaces.OrderBy(x => ran.Next()).Take(N).ToList();

        for(int i = 0; i < selected.Count; i++)
        {
            if(selected[i].faceName == "ANY\nONE")
            {
                List<string> myFaceNames = ListMyFaces(); myFaceNames.Remove("ANY\nONE");
                Dice reroll = new Dice(myFaceNames);
                selected[i] = reroll.rollDistinctFaces(1)[0];
            }
        }
        return selected;
    }

    public DiceFace getFaceSummary(DiceFace aFace)
    {
        if (aFace.alreadySummarized) { return aFace; }
        else if (!isConditionDice) { return getCopyFaceSummary(aFace); }
        else
        {
            aFace.mySprites = new List<Sprite>();
            aFace.mySprites.Add(Resources.Load<Sprite>("Images/Condition_" + aFace.faceName.ToLower() + ".png"));
            return aFace;
        }
    }
    public DiceFace getCopyFaceSummary(DiceFace aFace)
    {
        DiceFace finalFace = new DiceFace(aFace.value);

        //Get all effects that apply to the face : global, from item and from face
        List<Effect> allEffects = new List<Effect>();
        allEffects.AddRange(aFace.effects);
        if (myItem != null && myItem.myEffects != null) { allEffects.AddRange(myItem.myEffects); }
        if (myOwner != null) { allEffects.AddRange(myOwner.GetAllEffects()); }
        
        if (allEffects.Count > 1)
        { allEffects.Sort(delegate (Effect a, Effect b) { return GetEffectPriority(a.nameEffect).CompareTo(GetEffectPriority(b.nameEffect)); }); }

        foreach (Effect eff in allEffects) // Les effets se résolvent dans l'ordre suivant :
        {
            // Effets de transformation
            if (eff.nameEffect == "Transform" && finalFace.value == eff.effectValues[0]) {
                finalFace.value = eff.effectValues[1];
                finalFace.mySprites.Add( Resources.Load<Sprite>("Images/Dice_transformed.png") );
            }
            else if (eff.nameEffect == "Weak")
            {
                finalFace.value--;
                finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_reduced.png"));
            }
            else if (eff.nameEffect == "Strength")
            {
                finalFace.value += eff.effectValues[0];
                finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_augmented.png"));
            }
            else if (eff.nameEffect == "Cursed")
            {
                finalFace.value += eff.effectValues[0];
                finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_cursed.png"));
            }
            // Effets de tests
            else if (eff.nameEffect == "Heals_If_Threshold")
            {
                if (finalFace.value >= eff.effectValues[0]) { finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_success.png")); }
                finalFace.effects.Add(new Effect( "Heal", new List<int> { finalFace.value >= eff.effectValues[0] ? eff.effectValues[1] : 0 }, null ));
            }
            // Effets de conditions
            else if ( new List<string> { "Hit", "Weakening" , "Vulnerability" }.Contains(eff.nameEffect)) {
                finalFace.effects.Add(new Effect(eff.nameEffect, null, null));
                finalFace.mySprites.Add( Resources.Load<Sprite>("Images/Dice_" + eff.nameEffect.ToLower() + ".png") );
            }
        }
        
        if (aFace.faceName != null) { finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_blue.png")); }
        finalFace.mySprites.Reverse(); finalFace.alreadySummarized = true;
        return finalFace;
    }

    public int GetEffectPriority(string nameEff)
    {
        int returnValue = 0;
        if (new List<string> { "Hit" }.Contains(nameEff)) { returnValue = -1; }
        // Effects under 0 have no importance in ordering
        else if (new List<string> { "Transform" }.Contains(nameEff)) { returnValue = 1; }
        else if (new List<string> { "Weak", "Strength" }.Contains(nameEff)) { returnValue = 2; }
        else if (new List<string> { "Cursed" }.Contains(nameEff)) { returnValue = 3; }
        else if (new List<string> { "Heals_If_Threshold" }.Contains(nameEff)) { returnValue = 4; }
        // Effects upper 100 have no importance in ordering
        else if (new List<string> { "Weakening", "Vulnerability" }.Contains(nameEff)) { returnValue = 100; }

        return returnValue;
    }

    public int GetMinValue()
    {
        List<int> values = new List<int>();
        foreach(DiceFace aFace in myFaces) { values.Add(getFaceSummary(aFace).value); }
        return values.Min();
    }

    public List<string> ListMyFaces()
    {
        List<string> returnList = new List<string>();
        foreach(DiceFace aFace in myFaces) { returnList.Add(aFace.faceName); }
        return returnList;
    }
}
