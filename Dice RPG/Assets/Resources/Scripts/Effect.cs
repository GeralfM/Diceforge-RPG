using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public string nameEffect;
    public int duration;
    public List<int> effectValues; // list length depends on the effect
    public string rangeEffect;

    public Effect(string name, List<int> values, string range) { nameEffect = name; effectValues = values; rangeEffect = range; }
    public Effect(string name, int theDuration) { nameEffect = name; duration = theDuration; }

    public Effect(Effect eff)
    {
        nameEffect = eff.nameEffect; duration = eff.duration; rangeEffect = eff.rangeEffect;
        //effectValues = new List<int>();
        //foreach(int elt in eff.effectValues) { effectValues.Add(elt); }
    }
}
