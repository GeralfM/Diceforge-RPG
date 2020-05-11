using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Character
{
    public ObjectDisplayer myConditionsDisplayer;
    public List<Effect> myConditions = new List<Effect>();

    public CharacterContent myInfo;
    public Dice myBaseAttackDice;
    
    public Character() { }
    public Character(CharacterContent myContent)
    {
        myInfo = new CharacterContent(myContent);
        myInfo.pv = myInfo.pvMax;

        if (myInfo.faceValues != null) {
            myBaseAttackDice = new Dice(myInfo.faceValues, myInfo.faceEffects);
            myBaseAttackDice.faceSplits = myInfo.faceSplits;

            myBaseAttackDice.myFaces.ForEach(x => x.effects.Add( new Effect("Hit", null, null) ));
            myBaseAttackDice.myOwner = this;
        }
    }

    public void SetRarity(int levelConfront)
    {
        if( Random.Range(0f, 1f) < (levelConfront - myInfo.levelAppear) * 0.1f)
        {
            myInfo.myRarity = " - Champion";
            myInfo.pvMax *= 2; myInfo.pv *= 2;
            myInfo.lootValue *= 2;

            Effect buff = new Effect("Strength", new List<int> { 2 }, "character");
            buff.duration = -1;
            myConditions.Add(buff);
        }
    }

    public virtual List<DiceFace> getAttack()
    {
        List<DiceFace> values = new List<DiceFace>();

        if (myBaseAttackDice != null) {

            List<int> rangeList = new List<int> { 0 };
            rangeList.AddRange(myBaseAttackDice.faceSplits); rangeList.Add(myBaseAttackDice.myFaces.Count);
            
            for(int i=0; i<rangeList.Count-1; i++)
            {
                values.Add(myBaseAttackDice.rollFromRange(rangeList[i], rangeList[i + 1]));
            }
        }
        else { values.Add(new DiceFace(0)); }
        
        return values;
    }
    public virtual void takeHit(int value)
    {
        myInfo.pv -= value + SearchEffectValue("Vulnerable");
    }
    public void Heal(int value)
    {
        myInfo.pv = Mathf.Min(myInfo.pv + value, myInfo.pvMax);
    }

    public virtual List<Effect> GetAllEffects() // Does only conditions for now
    {
        List<Effect> allEffects = new List<Effect>(); allEffects.AddRange(myConditions);
        return allEffects;
    }
    public int SearchEffectValue(string nameEff)
    {
        int returnValue = 0;
        GetAllEffects().ForEach(x => returnValue += ( x.nameEffect == nameEff ) ? x.effectValues[0] : 0);
        return returnValue;
    }

    // CONDITIONS

    public void AddCondition(Effect eff)
    {
        bool toBeAdded = true;
        foreach(Effect cond in myConditions) // Is the character already under that condition ?
        {
            if(cond.nameEffect == eff.nameEffect)
            {
                toBeAdded = false;
                if(new List<string> { "Weak" }.Contains(eff.nameEffect)) // Stackable dans le temps
                {
                    cond.duration += eff.duration;
                }
                else if (new List<string> { "Vulnerable" }.Contains(eff.nameEffect)) // Stackable en valeur
                {
                    cond.effectValues[0] += eff.effectValues[0];
                }
            }
        }
        if (toBeAdded) { myConditions.Add(eff); }
    }
    public void ReduceConditionDuration()
    {
        List<Effect> copyCond = new List<Effect>();
        foreach (Effect cond in myConditions) // Is the character already under that condition ?
        {
            if(cond.duration > 1) { cond.duration--; copyCond.Add(cond); }
            else if(cond.duration <= -1) { copyCond.Add(cond); } // effets permanents au moins jusqu'à la fin du combat
        }
        myConditions = copyCond;
    }
    public void EndCombatDurationEffect()
    {
        List<Effect> copyCond = new List<Effect>();
        foreach (Effect cond in myConditions) // Is the character already under that condition ?
        {
            if (cond.duration == -1) { copyCond.Add(cond); } // -2 pour les effets durant pour le combat uniquement
        }
        myConditions = copyCond;

        DisplayConditions();
    }

    public void DisplayConditions()
    {
        Dice myDiceConditions = new Dice(new List<string>());
        myDiceConditions.isConditionDice = true;

        foreach (Effect eff in myConditions)
        {
            if (eff.nameEffect != null && eff.nameEffect != "")
            {
                DiceFace aFace = new DiceFace(eff.nameEffect);

                if(new List<string> { "Weak" }.Contains(eff.nameEffect)) { aFace.value = eff.duration; }
                if (new List<string> { "Vulnerable" }.Contains(eff.nameEffect)) { aFace.value = eff.effectValues[0]; }

                myDiceConditions.myFaces.Add(aFace);
            }
        }

        myConditionsDisplayer.DisplayDice(myDiceConditions);
    }
}
