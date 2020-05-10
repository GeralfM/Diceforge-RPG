using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealingHandler : MonoBehaviour
{
    public GameObject MyHealButton;
    public GameObject MyCostText;

    public Player thePlayer;

    public int nbClic;
    public int amountHeal;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void InitializeScene(Player player)
    {
        thePlayer = player;
        nbClic = 0;

        DisplayHeal();
        DisplayCost();
    }
    public void DisplayHeal()
    {
        amountHeal = (int)Mathf.Ceil((thePlayer.myInfo.pvMax - thePlayer.myInfo.pv) / 2.0f);
        MyHealButton.transform.Find("TextHeal").GetComponent<Text>().text = "Heal " + amountHeal + " PV";
    }
    public void DisplayCost()
    {
        MyCostText.GetComponent<Text>().text = "Cost : " + ( (nbClic == 0) ? "free" : (10 * nbClic)+"" );
        MyHealButton.GetComponent<Button>().interactable = thePlayer.myInfo.gold >= 10 * nbClic;
    }

    public void ClicHeal()
    {
        thePlayer.Heal(amountHeal);
        thePlayer.myInfo.gold -= 10 * nbClic;

        thePlayer.DisplayPV();
        thePlayer.DisplayGold();

        nbClic++;
        DisplayHeal();
        DisplayCost();
    }
}
