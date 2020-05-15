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
    public bool alreadyRolled = false; // for copy of multiple-dices only

    public List<Effect> externalEffects;
    public List<DiceFace> simulatedDice;
    public Dictionary<string, int> simulatedStats;

    public System.Random ran = new System.Random();

    // May need to refactor all those constructors
    public Dice(List<string> fNames, List<int> fValues, List<string> fEffects, List<int>fEffectsValues)
    {
        if (fNames != null && fNames.Count>0)
        {
            for (int i = 0; i < fNames.Count; i++)
            {
                DiceFace newFace = new DiceFace(fNames[i]);
                newFace.value = fValues[i];
                myFaces.Add(newFace);
            }
        }
        else if (fValues != null)
        {
            for (int i = 0; i < fValues.Count; i++)
            {
                DiceFace newFace = new DiceFace(fValues[i]);
                List<int> effVal = new List<int>();  if (fEffectsValues!=null && fEffectsValues.Count>0) { effVal.Add(fEffectsValues[i]); }
                if (fEffects.Count > 0 && fEffects[i] != "") { newFace.effects.Add(new Effect(fEffects[i], -1, null, effVal)); }
                myFaces.Add(newFace);
            }
        }
    }
    public Dice(List<string> nameFaces) // constructor for spell or effect dices, may be redundant with previous
    {
        for (int i = 0; i < nameFaces.Count; i++) { myFaces.Add(new DiceFace(nameFaces[i])); }
    }
    public Dice(List<DiceFace> faces) { myFaces = faces; alreadyRolled = true; } // contructor for temporary dice
    

    public DiceFace rollFromRange(int idmin, int idmax)
    {
        SimulateDice();
        return simulatedDice[Random.Range(idmin, idmax)];
    }
    public DiceFace rollDice()
    {
        SimulateDice();
        return simulatedDice[Random.Range(0, myFaces.Count)];
    }
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

    // Toolbox to summarize a dice

    public void GetExternalEffects()
    {
        List<Effect> allEffects = new List<Effect>();
        if (myItem != null && myItem.myEffects != null) { allEffects.AddRange(myItem.myEffects); }
        if (myOwner != null) { allEffects.AddRange(myOwner.GetAllEffects()); }

        externalEffects = allEffects;
    }

    public void SimulateDice()
    {
        List<DiceFace> resultFaces = new List<DiceFace>();
        GetExternalEffects();
        myFaces.ForEach(x => resultFaces.Add(getFaceSummary(x))); // First passage. Now all faces are summarized

        if (!isConditionDice) {
            simulatedStats = new Dictionary<string, int>();
            int lowest = resultFaces[0].value; resultFaces.ForEach(x => lowest = (lowest > x.value) ? x.value : lowest);
            simulatedStats.Add("Lowest", lowest);
        }

        resultFaces.ForEach(x => FinalizeFaceSimulation(x)); // Second passage.

        simulatedDice = resultFaces;
    }

    public List<DiceFace> GetAllFinalFaces()
    {
        if (!alreadyRolled) { SimulateDice(); return simulatedDice; }
        else { return myFaces; }
    }

    public DiceFace getFaceSummary(DiceFace aFace)
    {
        if (aFace.alreadySummarized) { return aFace; }
        else if (!isConditionDice) { return getCopyFaceSummary(aFace); }
        else // it is a Condition Dice
        {
            aFace.mySprites = new List<Sprite>();
            aFace.mySprites.Add(Resources.Load<Sprite>("Images/Condition_" + aFace.faceName.ToLower() + ".png"));
            return aFace;
        }
    }
    public DiceFace getCopyFaceSummary(DiceFace aFace)
    {
        DiceFace finalFace = new DiceFace(aFace.value);
        finalFace.faceName = aFace.faceName;

        //Get all effects that apply to the face : global, from item and from face
        List<Effect> allEffects = new List<Effect>();
        allEffects.AddRange(aFace.effects);
        allEffects.AddRange(externalEffects);
        //if (myItem != null && myItem.myEffects != null) { allEffects.AddRange(myItem.myEffects); }
        //if (myOwner != null) { allEffects.AddRange(myOwner.GetAllEffects()); }
        
        if (allEffects.Count > 1)
        { allEffects.Sort(delegate (Effect a, Effect b) { return GetEffectPriority(a.nameEffect).CompareTo(GetEffectPriority(b.nameEffect)); }); }

        foreach (Effect eff in allEffects)
        {
            // Effets de transformation
            if (eff.nameEffect == "Transform" && finalFace.value == eff.effectValues[0]) {
                finalFace.value = eff.effectValues[1];
                finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_transformed.png"));
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
            else if (new List<string> { "Cursed" }.Contains(eff.nameEffect))
            {
                finalFace.value += eff.effectValues[0];
                finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_Cursed.png"));
            }
            // Effets de tests
            else if (eff.nameEffect == "Heals_If_Threshold")
            {
                if (finalFace.value >= eff.effectValues[0])
                {
                    finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_success.png"));
                    finalFace.effects.Add(new Effect("Heal", -1, "face", new List<int> { eff.effectValues[1] }));
                }
            }
            // Effets de conditions sans paramètre
            else if ( new List<string> { "Hit", "Weakening" , "Vulnerability" }.Contains(eff.nameEffect)) {
                finalFace.effects.Add(new Effect(eff.nameEffect, -1, null, null)); // to be improved
                finalFace.mySprites.Add( Resources.Load<Sprite>("Images/Dice_" + eff.nameEffect.ToLower() + ".png") );
            }
            // Effets de conditions avec paramètre
            else if (new List<string> { "LightPoison" }.Contains(eff.nameEffect))
            {
                finalFace.effects.Add(new Effect(eff.nameEffect, -1, null, eff.effectValues.GetRange(0,1))); // to be improved
                finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_" + eff.nameEffect.ToLower() + ".png"));
            }
            // Faces de dés nommées
            else if (! (new List<string> { "LightPoison_If_Lowest" }.Contains(eff.nameEffect)) )
                { finalFace.effects.Add(new Effect(eff.nameEffect, -1, null, null)); }
        }
        
        if (aFace.faceName != null) { finalFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_blue.png")); }
        finalFace.alreadySummarized = true; 
        return finalFace;
    }
    public void FinalizeFaceSimulation(DiceFace aFace) // everything is already setup, just need to apply post-simulation effects
    {
        List<string> nameEffects = new List<string>();
        aFace.effects.ForEach(x => nameEffects.Add(x.nameEffect));

        foreach (Effect eff in externalEffects)
        {
            if (eff.nameEffect == "LightPoison_If_Lowest" && simulatedStats["Lowest"] == aFace.value && nameEffects.Contains("Hit"))
            {
                aFace.mySprites.Add(Resources.Load<Sprite>("Images/Dice_LightPoison.png"));
                aFace.effects.Add(new Effect("LightPoison", -1, null, new List<int> { eff.effectValues[0] }));
            }
        }
        aFace.mySprites.Reverse();
    }

    public int GetEffectPriority(string nameEff)
    {
        int returnValue = 0;
        if (new List<string> { "Hit" }.Contains(nameEff)) { returnValue = -1; }
        // Effects under 0 have no importance in ordering
        else if (new List<string> { "Transform" }.Contains(nameEff)) { returnValue = 1; }
        else if (new List<string> { "Weak", "Strength" }.Contains(nameEff)) { returnValue = 2; }
        else if (new List<string> { "Cursed" }.Contains(nameEff)) { returnValue = 3; }
        else if (new List<string> { "Heals_If_Threshold", "LightPoison_If_Lowest" }.Contains(nameEff)) { returnValue = 4; }
        // Effects upper 100 have no importance in ordering
        else if (new List<string> { "Weakening", "Vulnerability", "LightPoison" }.Contains(nameEff)) { returnValue = 100; }

        return returnValue;
    }

    public int GetMinValue()
    {
        SimulateDice();
        return simulatedStats["Lowest"];
    }

    public List<string> ListMyFaces()
    {
        List<string> returnList = new List<string>();
        foreach(DiceFace aFace in myFaces) { returnList.Add(aFace.faceName); }
        return returnList;
    }
}
