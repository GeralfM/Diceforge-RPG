using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectDisplayer : MonoBehaviour
{
    public GameObject basicPrefab;
    public GameObject righArrow;
    public GameObject leftArrow;

    public List<Item> toBeDisplayed;
    public List<GameObject> displayedList = new List<GameObject>();

    public float xStep;
    public float yStep;
    public float rowLength;
    public int limit;
    public int page;
      
    public void DisplayItemCollection(List<Item> aList)
    {
        bool isUpgraded = basicPrefab.name == "ImprovedBuyGroup";

        Hide();
        toBeDisplayed = aList;

        int count=0;
        foreach (Item elt in aList)
        {
            if (limit == 0 || ( count >= page * limit && count < (page+1) * limit) )
            {
                GameObject newObject = Instantiate(basicPrefab, new Vector3(0, 0, -1), Quaternion.identity);
                newObject.transform.SetParent(basicPrefab.transform.parent);
                newObject.transform.localScale = basicPrefab.transform.localScale;
                newObject.transform.localPosition = basicPrefab.transform.localPosition + new Vector3(0 + (count % rowLength) * xStep, 0 - (int)(count / rowLength) * yStep, 0);
                
                newObject.transform.SetSiblingIndex(0);

                // Dans le cas d'un superprefab Upgrade
                if (!isUpgraded)
                {
                    newObject.GetComponent<InteractableElt>().myItemRef = elt;
                    newObject.GetComponent<InteractableElt>().DisplaySprite();
                }
                else
                {
                    newObject.transform.Find("ItemUpgradePrefab").GetComponent<InteractableElt>().myItemRef = elt;
                    newObject.transform.Find("Button").GetComponent<InteractableElt>().myItemRef = elt;

                    newObject.transform.Find("ItemUpgradePrefab").Find("ItemFrame").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + elt.myInfo.myName + ".png");
                    newObject.transform.Find("Button").Find("Text").GetComponent<Text>().text = "Buy : " + elt.myInfo.goldValue + " gold";
                    newObject.transform.Find("Button").GetComponent<Button>().interactable = elt.owner.myInfo.gold >= elt.myInfo.goldValue;
                }

                if (elt.myInfo.myName == "Gold")
                {
                    newObject.transform.Find("Text").GetComponent<Text>().text = elt.myInfo.goldValue + "";
                    newObject.transform.Find("Text").gameObject.SetActive(true);
                }
                if(elt.myInfo.myRarity != null)
                {
                    newObject.transform.Find("RarityGem").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Gem_"+ elt.myInfo.myRarity + ".png");
                    newObject.transform.Find("RarityGem").gameObject.SetActive(true);
                }

                newObject.SetActive(true);

                displayedList.Add(newObject);
            }
            count++;
        }
    }
    public void DisplayDice(Dice dice)
    {
        bool isUpgraded = basicPrefab.name == "ImprovedBuyGroup";
        Hide();

        int count = 0;
        foreach (DiceFace face in dice.myFaces)
        {
            DiceFace finalFace = dice.getFaceSummary(face);

            GameObject newObject = Instantiate(basicPrefab, new Vector3(0, 0, -1), Quaternion.identity);
            newObject.transform.SetParent(basicPrefab.transform.parent);
            newObject.transform.localScale = basicPrefab.transform.localScale;
            newObject.transform.localPosition = basicPrefab.transform.localPosition + new Vector3(0 + (count % rowLength) * xStep, 0 - (int)(count / rowLength) * yStep, 0);
            if (newObject.GetComponent<InteractableElt>() != null) { newObject.GetComponent<InteractableElt>().myFaceRef = face; } // for upgradeHandler

            // Test dans le cas d'un superprefab Upgrade
            if (!isUpgraded) { newObject.transform.Find("Text").GetComponent<Text>().text = (face.faceName != null) ? face.faceName : finalFace.value + ""; }
            else {
                newObject.transform.Find("DiceFacePrefab").Find("Text").GetComponent<Text>().text = finalFace.value + "";
                newObject.transform.Find("Button").GetComponent<InteractableElt>().myFaceRef = face;
                newObject.transform.Find("Button").Find("Text").GetComponent<Text>().text = "Buy : " + ( face.upgradeCost * Mathf.Pow(2,dice.myItem.myInfo.improved)) + " gold";
                newObject.transform.Find("Button").GetComponent<Button>().interactable = dice.myOwner.myInfo.gold >= ( face.upgradeCost * Mathf.Pow(2, dice.myItem.myInfo.improved));
            }

            foreach(Sprite aSpr in finalFace.mySprites)
            {
                Transform objectParent = isUpgraded ? newObject.transform.Find("DiceFacePrefab") : newObject.transform;
                
                GameObject newLayer = Instantiate(objectParent.Find("DiceLayer").gameObject) ;
                newLayer.transform.SetParent(objectParent);
                newLayer.transform.localScale = objectParent.Find("DiceLayer").localScale;
                newLayer.transform.localPosition = new Vector3(0,0,0);
                newLayer.transform.SetSiblingIndex(0);

                newLayer.GetComponent<Image>().sprite = aSpr;
                newLayer.SetActive(true);
            }
            
            newObject.SetActive(true);

            displayedList.Add(newObject);

            count++;
        }
    }
    public void Hide()
    {
        foreach (GameObject elt in displayedList)
        {
            Destroy(elt);
        }
        displayedList = new List<GameObject>();
    }

    public void ClicArrow(bool toRight)
    {
        page += toRight ? 1 : -1;
        ActualizeArrow();
        DisplayItemCollection(toBeDisplayed);
    }
    public void ActualizeArrow()
    {
        leftArrow.GetComponent<Button>().interactable = page >= 1;
        righArrow.GetComponent<Button>().interactable = (page + 1) * limit < toBeDisplayed.Count; 
    }

    // Start is called before the first frame update
    void Start()
    {
        //List<Item> zbluh = new List<Item> { new Item(), new Item(), new Item() };
        //DisplayItemCollection(zbluh);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
