using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

[System.Serializable]
public class MetaJsonMobs
{
    public List<CharacterContent> myMobs;
}

[System.Serializable]
public class MetaJsonItems
{
    public List<ItemContent> myItems;
}

[System.Serializable]
public class MetaJsonCounters
{
    public List<CounterContent> myCounters;

    public MetaJsonCounters() { myCounters = new List<CounterContent>(); }
}

[System.Serializable]
public class MetaJsonTriggers
{
    public List<TriggerContent> myTriggers;
}

public class MetaJson
{
    public Dictionary<string, CharacterContent> allMobs = new Dictionary<string, CharacterContent>();
    public Dictionary<string, ItemContent> allItems = new Dictionary<string, ItemContent>();
    public Dictionary<string, CounterContent> allCounters = new Dictionary<string, CounterContent>();
    public Dictionary<string, TriggerContent> allEvents = new Dictionary<string, TriggerContent>();

    public MetaJson()
    {
        // Load all mobs
        string loadedItem = JsonFileReader.LoadJsonAsResource("Jsons/Mobs");
        MetaJsonMobs loadedMobs = JsonUtility.FromJson<MetaJsonMobs>(loadedItem);

        foreach (CharacterContent elt in loadedMobs.myMobs)
        {
            allMobs.Add(elt.myName, elt);
        }

        // Load all items
        loadedItem = JsonFileReader.LoadJsonAsResource("Jsons/Items");
        MetaJsonItems loadedItems = JsonUtility.FromJson<MetaJsonItems>(loadedItem);

        foreach (ItemContent elt in loadedItems.myItems)
        {
            allItems.Add(elt.myName, elt);
        }

        // Load all events
        ReadEventsFile();

        // Load all counters, create file if not exists
        if (!System.IO.File.Exists(Application.persistentDataPath + "/Counters.json")) { ResetCountersFile(); }
        ReadCountersFile();

    }

    public void ResetCountersFile()
    {
        string loadedItem = JsonFileReader.LoadJsonAsResource("Jsons/Counters");
        JsonFileReader.WriteJsonToExternalResource("Counters.json", loadedItem);
    }
    public void ReadCountersFile()
    {
        string loadedItem = JsonFileReader.LoadJsonAsExternalResource("Counters.json");
        MetaJsonCounters loadedCounters = JsonUtility.FromJson<MetaJsonCounters>(loadedItem);

        allCounters = new Dictionary<string, CounterContent>();
        foreach (CounterContent elt in loadedCounters.myCounters)
        {
            allCounters.Add(elt.nameCounter, elt);
        }
    }
    public void StoreCounters()
    {
        MetaJsonCounters newMeta = new MetaJsonCounters();
        allCounters.Values.ToList().ForEach(x => newMeta.myCounters.Add(x));
        JsonFileReader.WriteJsonToExternalResource("Counters.json", JsonUtility.ToJson(newMeta));
    }

    public void ReadEventsFile()
    {
        string loadedItem = JsonFileReader.LoadJsonAsResource("Jsons/Triggers");
        MetaJsonTriggers loadedTriggers = JsonUtility.FromJson<MetaJsonTriggers>(loadedItem);

        allEvents = new Dictionary<string, TriggerContent>();
        foreach (TriggerContent elt in loadedTriggers.myTriggers)
        {
            allEvents.Add(elt.ID, elt);
        }
    }

}