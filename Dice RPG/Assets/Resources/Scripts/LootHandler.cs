using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

public class LootHandler : MonoBehaviour
{
    public GameObject followerTab;

    public ObjectDisplayer myLootDiceDisplayer;
    public ObjectDisplayer myLootChoiceDisplayer;

    public Dictionary<string, ItemContent> allItems;

    public Player thePlayer;
    public Dice lootDice;
    public WeightedBag itemPool = new WeightedBag();
    public WeightedBag commonRarities = new WeightedBag();
    public WeightedBag rareRarities = new WeightedBag();

    public Dictionary<string, WeightedBag> shopPools = new Dictionary<string, WeightedBag>();

    // Start is called before the first frame update
    void Start()
    {
        lootDice = new Dice(new List<string> { "GOLD", "ITEM", "ANY\nONE" });

        commonRarities.AddElement("Common", 0.9f); commonRarities.AddElement("Rare", 0.1f);
        rareRarities.AddElement("Common", 0.3f); rareRarities.AddElement("Rare", 0.55f); rareRarities.AddElement("Epic", 0.1f); rareRarities.AddElement("Legendary", 0.05f);
    }
    // Update is called once per frame
    void Update() { }

    public void InitializeLoot(Character monster)
    {
        followerTab.SetActive(false);
        GameObject.Find("Background").GetComponent<LoreHandler>().ExecuteFromTrigger("Kill_"+monster.myInfo.myName, followerTab );

        myLootDiceDisplayer.DisplayDice(lootDice);
    
        List<DiceFace> selected = lootDice.rollDistinctFaces(monster.myInfo.lootChoices);
        
        List<Item> choices = new List<Item>();
        foreach(DiceFace aFace in selected)
        {
            Item newItem = null;
            switch (aFace.faceName)
            {
                case "GOLD":
                    ItemContent content = new ItemContent(); content.myName = "Gold"; content.goldValue = monster.myInfo.lootValue;
                    newItem = new Item(content);
                    break;
                case "ITEM":
                    newItem = SpawnRandomItem( (monster.myInfo.myRarity == " - Champion")?"Rare":"Common" );
                    newItem.SetOwner(thePlayer);
                    break;
            }
            if (newItem.myInfo.myType == "Weapon" && Random.Range(0f, 1f) < 0.1f) { newItem.SetCurse(); }
            choices.Add(newItem);
        }

        myLootChoiceDisplayer.DisplayItemCollection(choices);
    }

    public void CollectLoot(Item collected)
    {
        if(collected.myInfo.myName == "Gold")
        {
            thePlayer.myInfo.gold += collected.myInfo.goldValue;
            thePlayer.myGold.GetComponent<Text>().text = thePlayer.myInfo.gold + "";
        }
        else { thePlayer.myInventory.Add(collected); }

        myLootChoiceDisplayer.Hide();
    }

    public Item SpawnRandomItem(string rarityLevel)
    {
        string chosen = itemPool.Draw(false);
        Item newItem = new Item(allItems[chosen]);

        string rarityItem = null;
        if (rarityLevel == "Common") { rarityItem = commonRarities.Draw(false); }
        else if (rarityLevel == "Rare") { rarityItem = rareRarities.Draw(false); }

        SetRarity(newItem, rarityItem);

        return newItem;
    }
    public void SetRarity(Item newItem, string rarityItem)
    {
        if (newItem.myInfo.myType == "Weapon") // Modifications uniques aux armes
        {
            Effect buff = null;
            switch (rarityItem)
            {
                case "Rare":
                    buff = new Effect("Strength", -1, "item", new List<int> { 1 }); break;
                case "Epic":
                    buff = new Effect("Strength", -1, "item", new List<int> { 2 }); break;
                case "Legendary":
                    buff = new Effect("Strength", -1, "item", new List<int> { 5 }); break;
            }
            if (buff != null) { newItem.myEffects.Add(buff); }
        }
        else if (newItem.myInfo.mySubType == "Armor") // Modifications uniques aux armures
        {
            switch (rarityItem)
            {
                case "Rare":
                    newItem.myInfo.damageReduction += 1; break;
                case "Epic":
                    newItem.myInfo.damageReduction += 2; break;
                case "Legendary":
                    newItem.myInfo.damageReduction += 5; break;
            }
        }

        if (rarityItem != "Common")
        { // Modifications générales
            newItem.myInfo.myRarity = rarityItem;
            switch (rarityItem)
            {
                case "Rare":
                    newItem.myInfo.goldValue *= 2; break;
                case "Epic":
                    newItem.myInfo.goldValue *= 3; break;
                case "Legendary":
                    newItem.myInfo.goldValue *= 5; break;
            }
            newItem.myInfo.myDescription += "\n" + rarityItem.ToUpper();
        }

        if (new List<string> { "Glasses" }.Contains(newItem.myInfo.myName)) { specialUpgrade(newItem, rarityItem); }
    }

    public void GeneratePoolItems()
    {
        foreach(string itemName in allItems.Keys)
        {
            if(allItems[itemName].unlocked) itemPool.AddElement(itemName, 1f / allItems[itemName].goldValue);
        }
    }
    public void GeneratePoolItemsShop()
    {
        List<int> allPrices = new List<int>();
        foreach(ItemContent elt in allItems.Values)
        {
            if (elt.unlocked) { allPrices.Add(elt.goldValue); }
        }

        float meanValues = (float)allPrices.Average(); float accu = 0;
        allPrices.ForEach(x => accu += Mathf.Pow(meanValues - x, 2));
        float stdValues = Mathf.Sqrt(accu / allPrices.Count);

        new List<string> { "Low", "Middle", "High" }.ForEach(x => shopPools.Add(x, new WeightedBag()));
        foreach(string elt in allItems.Keys)
        {
            if (allItems[elt].unlocked)
            {
                if (allItems[elt].goldValue < meanValues - stdValues / 3) { shopPools["Low"].AddElement(elt, 1f / allItems[elt].goldValue); }
                else if (allItems[elt].goldValue < meanValues + stdValues / 3) { shopPools["Middle"].AddElement(elt, 1f / allItems[elt].goldValue); }
                else { shopPools["High"].AddElement(elt, 1f / allItems[elt].goldValue); }
            }
        }
    }

    public void specialUpgrade(Item toUpgrade, string rarity)
    {
        switch (toUpgrade.myInfo.myName)
        {
            case "Glasses":
                switch (rarity)
                {
                    case "Rare":
                        toUpgrade.myEffects[0].effectValues[1] = 4 ; break;
                    case "Epic":
                        toUpgrade.myEffects[0].effectValues[1] = 5 ; break;
                    case "Legendary":
                        toUpgrade.myEffects[0].effectValues[1] = 6 ; break;
                } break;
        }
    }
}
