using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Threading;

public class ConfrontationHandler : MonoBehaviour
{
    public Dictionary<string, CharacterContent> allMobs;
    public Player thePlayer;
    public Character enemy;
    public int levelConfront;

    public GameObject enemyPicture;
    public GameObject enemyName;
    public GameObject enemyPV;
    public GameObject enemyDiceDetail;
    public GameObject playerPV;

    public GameObject playerMainRoll;
    public GameObject playerSecondRoll;
    public GameObject enemyMainRoll;

    public GameObject diceFacePrefab;

    public GameObject attackButton;

    public bool playerAttacking;
    public Character attacker;
    public Character defendant;

    // Start is called before the first frame update
    void Start() { }
    // Update is called once per frame
    void Update() { }

    public void InitNewConfront()
    {
        levelConfront++;
        enemy = ChooseEnemy();

        // Displaying info
        enemyName.GetComponent<Text>().text = enemy.myInfo.myName + enemy.myInfo.myRarity;
        Sprite resSprite = Resources.Load<Sprite>("Images/" + enemy.myInfo.myName + ".png");
        enemyPicture.GetComponent<Image>().sprite = resSprite;
        displayPV(enemy.myInfo.pv, false);

        enemyDiceDetail.GetComponent<ObjectDisplayer>().DisplayDice(enemy.myBaseAttackDice);

        // Choosing first attacker. To be improved
        playerAttacking = true;
        
        // Launch first round
        NewRound();
    }

    public void NewRound()
    {
        thePlayer.ReduceConditionDuration(); enemy.ReduceConditionDuration();
        enemyDiceDetail.GetComponent<ObjectDisplayer>().DisplayDice(enemy.myBaseAttackDice);

        SetRoles(playerAttacking);
        if (playerAttacking) { SetPlayerChoices(true); }
        else { StartCoroutine(Attack()) ; }
    }
    public void EndRound()
    {
        playerAttacking = !playerAttacking;
        DisplayRoll(playerMainRoll, false);
        DisplayRoll(playerSecondRoll, false);
        DisplayRoll(enemyMainRoll, false);

        if (thePlayer.myInfo.pv <= 0 || enemy.myInfo.pv <= 0) { GetComponent<GameHandler>().EndOfConfrontation(thePlayer.myInfo.pv <= 0); }
        else { NewRound(); }
    }

    public void SetRoles(bool playerAtt)
    {
        attacker = playerAttacking ? thePlayer : enemy;
        defendant = playerAttacking ? enemy : thePlayer;
    }

    public void ClicAttack() { StartCoroutine(Attack()); }
    IEnumerator Attack()
    {
        SetPlayerChoices(false);

        List<DiceFace> att = attacker.getAttack();
        List<int> attValues = new List<int> { 0 };
        List<int> healValues = new List<int> { 0 }; // s'il y a de plus en plus de possibilités, un dictionnaire pourrait s'avérer nécessaire

        foreach (DiceFace aFace in att) {
            foreach(Effect eff in aFace.effects)
            {
                if (eff.nameEffect == "Heal") { healValues.Add(eff.effectValues[0]); }
                else if (eff.nameEffect == "Hit") { attValues.Add(aFace.value); }
                else if (eff.nameEffect == "Weakening") { defendant.AddCondition(new Effect("Weak", 2)); }
            }
        }
        
        DisplayRoll(playerAttacking ? playerMainRoll : enemyMainRoll, true, att[0]);
        enemyDiceDetail.GetComponent<ObjectDisplayer>().DisplayDice(enemy.myBaseAttackDice);

        if (att.Count==2) { DisplayRoll(playerSecondRoll, true, att[1]); };

        defendant.takeHit(attValues.Sum());
        attacker.Heal(healValues.Sum());

        displayPV(defendant.myInfo.pv, !playerAttacking);
        displayPV(attacker.myInfo.pv, playerAttacking);

        yield return new WaitForSeconds(2);
        EndRound();
    }

    public void DisplayRoll(GameObject target, bool display, DiceFace face = null)
    {
        if (!display && target.transform.Find("RollView") != null) { Destroy(target.transform.Find("RollView").gameObject); }
        else if (display)
        {
            GameObject newObject = Instantiate(diceFacePrefab, new Vector3(0, 0, -1), Quaternion.identity);
            newObject.transform.SetParent(target.transform);
            newObject.transform.localScale = new Vector3(0.16f, 0.19f, 0);
            newObject.transform.localPosition = new Vector3(0, 0, 0);
            newObject.name = "RollView";

            newObject.transform.Find("Text").GetComponent<Text>().text = face.value + "";
            foreach (Sprite aSpr in face.mySprites)
            {
                GameObject newLayer = Instantiate(newObject.transform.Find("DiceLayer").gameObject);
                newLayer.transform.SetParent(newObject.transform);
                newLayer.transform.localScale = new Vector3(1, 1, 0);
                newLayer.transform.localPosition = new Vector3(0, 0, 0);
                newLayer.transform.SetSiblingIndex(0);

                newLayer.GetComponent<Image>().sprite = aSpr;
                newLayer.SetActive(true);
            }

            newObject.SetActive(true);
        }
    }

    public void SetPlayerChoices(bool display)
    {
        attackButton.SetActive(display);
    }

    public Character ChooseEnemy()
    {
        Character theEnemy = new Character(allMobs["Rat"]);
        theEnemy.SetRarity(levelConfront);
        return theEnemy;
    }

    public void displayPV(int amount, bool isPlayer)
    {
        GameObject target = isPlayer ? playerPV : enemyPV;

        string bonusText = isPlayer ? (" / "+thePlayer.myInfo.pvMax) : "" ;
        target.GetComponent<Text>().text = amount + bonusText;
    }
}
