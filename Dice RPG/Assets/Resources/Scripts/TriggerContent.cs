using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriggerContent
{
    public string ID;
    public bool active=false;

    public string triggerName;

    public List<string> conditionNames;
    public List<float> conditionValues;
    public List<string> conditionData;

    public List<string> effectNames;
    public List<float> effectValues;
    public List<string> effectData;
}
