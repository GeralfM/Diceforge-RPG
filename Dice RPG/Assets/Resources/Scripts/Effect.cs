using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public string nameEffect;
    public int duration;
    public List<int> effectValues; // list length depends on the effect
    public string rangeEffect;

    public Effect(string name, int theDuration, string range, List<int> values)
    {
        nameEffect = name;
        if (values != null) { effectValues = new List<int>(); values.ForEach(x => effectValues.Add(x)); }
        duration = theDuration;
        rangeEffect = range;
    }

    public Effect(Effect eff)
    {
        nameEffect = eff.nameEffect; duration = eff.duration; rangeEffect = eff.rangeEffect;
    }
}
