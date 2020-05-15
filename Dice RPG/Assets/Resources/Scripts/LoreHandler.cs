﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

public class LoreHandler : MonoBehaviour
{
    public MetaJson myData; // Note : myData.allEvents to access events by ID
    public Dictionary<string, List<TriggerContent>> myEventsByTrigger;
    public WeightedBag aBag;

    // Start is called before the first frame update
    void Start() { }
    // Update is called once per frame
    void Update() { }
    
    public void InitializeLore()
    {
        UnpackEvents();
    }
    public void ResetLore()
    {
        myData.ResetCountersFile();
        myData.ReadCountersFile();
        myData.ReadEventsFile();
        UnpackEvents();
    }

    public void UnpackEvents()
    {
        myEventsByTrigger = new Dictionary<string, List<TriggerContent>>();
        foreach (TriggerContent elt in myData.allEvents.Values)
        {
            if (elt.triggerName != null) // Group events by triggers
            {
                if (myEventsByTrigger.Keys.ToList().Contains(elt.triggerName)) { myEventsByTrigger[elt.triggerName].Add(elt); }
                else { myEventsByTrigger.Add(elt.triggerName, new List<TriggerContent> { elt }); }
            }
            if (myData.allCounters["Activated_events"].content.Contains(elt.ID)) { elt.active = true; }
        }
    }
    public void SetEventActive(string eventName, bool isActive)
    {
        myData.allEvents[eventName].active = isActive;

        if (!isActive) { myData.allCounters["Activated_events"].content.Remove(eventName); }
        else if (!myData.allCounters["Activated_events"].content.Contains(eventName)) { myData.allCounters["Activated_events"].content.Add(eventName); }

        myData.StoreCounters();
    }

    // NOW ALL FUNCTIONS TO HANDLE EVENTS

    public void ExecuteFromTrigger(string tName, GameObject target)
    {
        List<TriggerContent> toSolve = new List<TriggerContent>();

        foreach (TriggerContent elt in myEventsByTrigger[tName])
            { if (elt.active) { toSolve.Add(elt); } }

        toSolve.ForEach(x => ResolveEvent(x, target));
    }

    public void ResolveEvent(TriggerContent theEvent, GameObject target)
    {
        if ( CheckConditionsEvent(theEvent) )
        {
            for (int i = 0; i < theEvent.effectNames.Count; i++)
            {
                switch (theEvent.effectNames[i])
                {
                    case "Pool_commentary":
                        aBag = new WeightedBag();
                        for(int j=0;j<theEvent.effectData.Count-1; j++) { aBag.AddElement(theEvent.effectData[j], theEvent.effectValues[j] / 100f); }

                        string theText = aBag.Draw(true);
                        if (theText != "nothing") { target.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text = theText; target.SetActive(true); }
                        else { target.SetActive(false); }
                        break;
                    case "Commentary":
                        target.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text = theEvent.effectData[i];
                        target.SetActive(true);
                        break;
                    case "Increment_counter":
                        myData.allCounters[theEvent.effectData[i]].count += (int)theEvent.effectValues[i];
                        myData.StoreCounters();
                        break;
                    case "Activate":
                        SetEventActive(theEvent.effectData[i], true);
                        break;
                    case "Deactivate":
                        SetEventActive(theEvent.effectData[i], false);
                        break;
                }
            }
        }

    }

    public bool CheckConditionsEvent(TriggerContent theEvent)
    {
        bool validated = true;
        if (theEvent.conditionNames.Count == 0) { return validated; }
        else
        {
            for(int i=0; i<theEvent.conditionNames.Count; i++)
            {
                switch (theEvent.conditionNames[i])
                {
                    case "Counter>=threshold":
                        validated &= myData.allCounters[theEvent.conditionData[i]].count >= theEvent.conditionValues[i];
                        break;
                }
            }
            return validated;
        }
    }

}
