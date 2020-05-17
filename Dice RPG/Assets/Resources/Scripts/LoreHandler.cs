using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

public class LoreHandler : MonoBehaviour
{
    public GameObject eventImage;
    public GameObject eventName;
    public List<string> eventData;

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
        myData.ResetMyContent();
        UnpackEvents();

        GetComponent<LootHandler>().GeneratePoolItems(); GetComponent<LootHandler>().GeneratePoolItemsShop();
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

        foreach(string elt in myData.allCounters["Unlocked_enemies"].content) { myData.allMobs[elt].unlocked = true; }
        foreach (string elt in myData.allCounters["Unlocked_items"].content) { myData.allItems[elt].unlocked = true; }
    }

    public void SetEventActive(string eventName, bool isActive)
    {
        myData.allEvents[eventName].active = isActive;

        if (!isActive) { myData.allCounters["Activated_events"].content.Remove(eventName); }
        else if (!myData.allCounters["Activated_events"].content.Contains(eventName)) { myData.allCounters["Activated_events"].content.Add(eventName); }

        myData.StoreCounters();
    }

    public void UnlockEnemy(string enemyName)
    {
        myData.allCounters["Unlocked_enemies"].content.Add(enemyName);
        myData.StoreCounters();

        myData.allMobs[enemyName].unlocked = true;
        if(GetComponent<ConfrontationHandler>().levelConfront >= myData.allMobs[enemyName].levelAppear)
            { GetComponent<ConfrontationHandler>().mobPool.AddElement(enemyName, myData.allMobs[enemyName].presenceRate * Mathf.Pow(0.7f, GetComponent<ConfrontationHandler>().levelConfront - myData.allMobs[enemyName].levelAppear) ); }
    }
    public void UnlockItem(string itemName)
    {
        myData.allCounters["Unlocked_items"].content.Add(itemName);
        myData.StoreCounters();

        myData.allItems[itemName].unlocked = true;
        GetComponent<LootHandler>().GeneratePoolItems(); GetComponent<LootHandler>().GeneratePoolItemsShop();
    }

    // NOW ALL FUNCTIONS TO HANDLE EVENTS

    public void ExecuteFromTrigger(string tName, GameObject target)
    {
        List<TriggerContent> toSolve = new List<TriggerContent>();

        if (myEventsByTrigger.Keys.Contains(tName))
        {
            foreach (TriggerContent elt in myEventsByTrigger[tName])
            { if (elt.active) { toSolve.Add(elt); } }

            foreach (TriggerContent elt in toSolve)
            { if (CheckConditionsEvent(elt)) { ResolveEvent(elt, target); } }
        }
    }

    public void ResolveEvent(TriggerContent theEvent, GameObject target)
    {
        for (int i = 0; i < theEvent.effectNames.Count; i++)
        {
            string theText;

            switch (theEvent.effectNames[i])
            {
                // COMMENTARY EVENTS
                case "Pool_commentary":
                    aBag = new WeightedBag();
                    for (int j = 0; j < theEvent.effectData.Count ; j++) { aBag.AddElement(theEvent.effectData[j], theEvent.effectValues[j] / 100f); }

                    theText = aBag.Draw(true);
                    if (theText != "nothing") { target.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text += theText + "\n"; target.SetActive(true); }
                    else { target.SetActive(false); }
                    break;

                case "Commentary":
                    target.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text += theEvent.effectData[i] + "\n";
                    target.SetActive(true);
                    break;

                case "Commentary_Variable":
                    List<string> copyText = new List<string>(); theEvent.effectData.ForEach(x => copyText.Add(x));
                    theText = DecodeString(copyText);
                    target.transform.Find("TextBackground").Find("Text").GetComponent<Text>().text += theText + "\n";
                    
                    target.SetActive(true);
                    break;
                    
                case "Increment_counter":
                    myData.allCounters[theEvent.effectData[i]].count += (int)theEvent.effectValues[i];
                    myData.StoreCounters();
                    
                    ExecuteFromTrigger("Incremented_"+theEvent.effectData[i], target);
                    break;

                case "Set_counter":
                    myData.allCounters[theEvent.effectData[i]].count = (int)theEvent.effectValues[i];
                    myData.StoreCounters();
                    break;

                // EVENTS
                case "Special_Event":
                    eventData = theEvent.effectData;
                    GetComponent<GameHandler>().specialEvent = true;
                    break;

                case "Force_encounter":
                    CharacterContent encounter = new CharacterContent( myData.allMobs[theEvent.effectData[i]] );

                    encounter.myName = theEvent.effectData[i+1]; // dangerous... an encounter can be force-set without name change
                    GetComponent<ConfrontationHandler>().forceEncounter = encounter;
                    break;

                case "Force_loot":
                    GetComponent<LootHandler>().forceLoot = theEvent.effectData[i];
                    break;

                case "Pool_triggerEvent":
                    aBag = new WeightedBag();
                    for (int j = 0; j < theEvent.effectData.Count ; j++) { aBag.AddElement(theEvent.effectData[j], theEvent.effectValues[j] / 100f); }

                    theText = aBag.Draw(true);
                    ResolveEvent(myData.allEvents[theText], target);
                    break;
                
                // TOOLS
                case "Activate":
                    SetEventActive(theEvent.effectData[i], true);
                    break;

                case "Deactivate":
                    SetEventActive(theEvent.effectData[i], false);
                    break;

                case "Deactivate_quest":
                    foreach(string elt in myData.allEvents.Keys.ToList())
                        { if (elt.Contains(theEvent.effectData[i])) { SetEventActive(elt, false); } }
                    break;

                case "Unlock_enemy":
                    UnlockEnemy(theEvent.effectData[i]);
                    break;

                case "Unlock_item":
                    UnlockItem(theEvent.effectData[i]);
                    break;
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
                    case "Counter<threshold":
                        validated &= myData.allCounters[theEvent.conditionData[i]].count < theEvent.conditionValues[i];
                        break;
                }
            }
            return validated;
        }
    }

    public string DecodeString(List<string> values)
    {
        if (values.Count == 1) { return values[0] + ""; }
        else
        {
            List<string> result = new List<string>();
            int leftVal; int rightVal;

            switch (values[1])
            {
                case "[Concat]":
                    result = new List<string> { values[0] + values[2] }; values.RemoveRange(0, 3); result.AddRange(values); 
                    break;
                case "[-]":
                    leftVal = values[0].Contains("[Param]") ? myData.allCounters[values[0].Replace("[Param]","") ].count : int.Parse(values[0]);
                    rightVal = values[2].Contains("[Param]") ? myData.allCounters[values[2].Replace("[Param]","")].count : int.Parse(values[2]);
                    result = new List<string> { leftVal - rightVal + "" }; values.RemoveRange(0, 3); result.AddRange(values);
                    break;
            }
            return DecodeString(result);
        }
    }

    // NOW ALL FUNCTIONS TO HANDLE TREE DIALOGS

    public void SetupEventScene()
    {
        eventImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Event - " + eventData[0] + ".png");
        eventName.GetComponent<Text>().text = eventData[1];
    }

    public void EndDialog()
    {
        ResolveEvent(myData.allEvents["HT-5a"], null); // hardcoded for now, waiting for Miseins's true tree dialog
    }

}
