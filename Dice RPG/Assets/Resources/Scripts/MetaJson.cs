using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class MetaJson
{
    public Dictionary<string, CharacterContent> allMobs = new Dictionary<string, CharacterContent>();
    public Dictionary<string, ItemContent> allItems = new Dictionary<string, ItemContent>();

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
    }

}