using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public Player owner; 
    public ItemContent myInfo;

    public Dice myDice;
    public List<Effect> myEffects = new List<Effect>();

    public Item(ItemContent content)
    {
        myInfo = new ItemContent(content);
        
        myDice = new Dice(myInfo.faceNames, myInfo.faceValues, myInfo.faceEffects, myInfo.faceEffectsValues); 
        myDice.myItem = this;
        if (myDice.myFaces.Count == 0) { myDice = null; }

        if (myInfo.effect != null) { myEffects.Add( new Effect(myInfo.effect, -1, myInfo.effectRange, myInfo.effectValues) ); }
        
        myInfo.improved = 0;
    }
    public void SetOwner(Player master)
    {
        owner = master;
        if (myDice != null) { myDice.myOwner = master; }
    }

    public void SetCurse()
    {
        myInfo.cursed = true;
        myDice.myFaces[0].effects.Add(new Effect("Cursed", -1, "face", new List<int> { -3 })); 
        myDice.myFaces[myDice.myFaces.Count-1].effects.Add(new Effect("Cursed", -1, "face", new List<int> { 3 }));
    }
}
