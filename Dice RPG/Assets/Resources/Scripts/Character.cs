using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public CharacterContent myInfo;
    public Dice myBaseAttackDice;

    public List<Effect> myConditions = new List<Effect>();

    public Character() { }
    public Character(CharacterContent myContent)
    {
        myInfo = new CharacterContent(myContent);
        myInfo.pv = myInfo.pvMax;

        if (myInfo.faceValues != null) {
            myBaseAttackDice = new Dice(myInfo.faceValues, myInfo.faceEffects);
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
            DiceFace aFace = myBaseAttackDice.rollDice();
            values.Add(aFace);
        }
        else { values.Add(new DiceFace(0)); }
        
        return values;
    }
    public virtual void takeHit(int value)
    {
        myInfo.pv -= value;
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
    public void AddCondition(Effect eff)
    {
        bool toBeAdded = true;
        foreach(Effect cond in myConditions) // Is the character already under that condition ?
        {
            if(cond.nameEffect == eff.nameEffect)
            {
                toBeAdded = false;
                cond.duration += eff.duration; // C'est trèèèès risqué. Chaque condition ne doit avoir qu'un seul chiffre !
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
            else if(cond.duration == -1) { copyCond.Add(cond); } // effets permanents
        }
        myConditions = copyCond;
    }
}
