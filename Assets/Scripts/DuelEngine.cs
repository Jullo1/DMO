using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public enum Phase { Undefined, Draw, Standby, Main, Battle, Main2, End };
public enum Zone { Undefined, Field, Hand, Deck, Graveyard, Banished, Fusion };
public enum SummonCondition { Normal, Special, Fusion, Ritual };

public class DuelEngine : MonoBehaviour
{
    GameManager game;

    Player player;
    Player opponent;

    Field playerField;
    Deck playerDeck;
    Hand playerHand;
    Graveyard playerGraveyard;
    Banished playerBanished;
    FusionDeck playerFusionDeck;

    Field opponentField;
    Deck opponentDeck;
    Hand opponentHand;
    Graveyard opponentGraveyard;
    Banished opponentBanished;
    FusionDeck opponentFusionDeck;
    FieldCard fieldCard;

    public bool playerTurn;
    public Phase currentPhase;
    public int currentTurn;

    public Text currentTurnText;
    public Text opponentTurnText;
    public Text currentPhaseText;
    public Text nextPhaseText;

    Monster attackCard;

    public int tributesLeft;
    public Monster[] tributes = new Monster[2];
    Monster tributeSummonCard;
    bool tributeIsSet;
    bool initiatedTribute;

    AudioSource[] aud;
    //duel sfx
    [SerializeField] AudioClip drawSound;
    [SerializeField] AudioClip playSound;
    [SerializeField] AudioClip searchSound;
    [SerializeField] AudioClip sendSound;
    [SerializeField] AudioClip statusSound;
    [SerializeField] AudioClip damageSound;

    //menus sfx
    [SerializeField] AudioClip blipSound;
    [SerializeField] AudioClip selectSound;
    [SerializeField] AudioClip decideSound;
    [SerializeField] AudioClip cancelSound;
    [SerializeField] AudioClip cantSound;
    [SerializeField] AudioClip inSound;

    EventSystem playerInputs;
    [SerializeField] GameObject gameOverText;

    void Awake()
    {
        game = FindObjectOfType<GameManager>();
        aud = GetComponents<AudioSource>();
        playerInputs = FindAnyObjectByType<EventSystem>();

        //Assign field elements
        foreach (Field field in FindObjectsOfType<Field>())
        {
            if (field.tag == "Player")
                playerField = field;
            else if (field.tag == "Opponent")
                opponentField = field;
        }
        foreach (Deck deck in FindObjectsOfType<Deck>())
        {
            if (deck.tag == "Player")
                playerDeck = deck;
            else if (deck.tag == "Opponent")
                opponentDeck = deck;
        }
        foreach (Hand hand in FindObjectsOfType<Hand>())
        {
            if (hand.tag == "Player")
                playerHand = hand;
            else if (hand.tag == "Opponent")
                opponentHand = hand;
        }
        foreach (Graveyard graveyard in FindObjectsOfType<Graveyard>())
        {
            if (graveyard.tag == "Player")
                playerGraveyard = graveyard;
            else if (graveyard.tag == "Opponent")
                opponentGraveyard = graveyard;
        }
        foreach (Banished banished in FindObjectsOfType<Banished>())
        {
            if (banished.tag == "Player")
                playerBanished = banished;
            else if (banished.tag == "Opponent")
                opponentBanished = banished;
        }
        foreach (FusionDeck fusion in FindObjectsOfType<FusionDeck>())
        {
            if (fusion.tag == "Player")
                playerFusionDeck = fusion;
            else if (fusion.tag == "Opponent")
                opponentFusionDeck = fusion;
        }
        fieldCard = FindObjectOfType<FieldCard>();

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.tag == "Player")
                this.player = player;
            else if (player.tag == "Opponent")
                opponent = player;
        }
    }

    void Start()
    {
        DuelStart();
    }

    IEnumerator AIMove()
    {
        yield return new WaitForSeconds(0.5f);
        if (playerTurn) yield break;

        switch (currentPhase)
        {
            case Phase.Draw:
                NextPhase();
                yield break;
            case Phase.Main:
                if (opponentHand.slotList.Count > 0) //play a card
                {
                    int cardsInField = 0;

                    //first check field
                    for (int i = 0; i < opponentField.monsterSlots.Length; i++)
                    {
                        if (!opponentField.monsterSlots[i].container) continue;
                        cardsInField++;
                    }

                    int cardToPlay = -1;
                    int maxValue = 0;
                    bool playInDef = false;
                    int maxValueField = 0;
                    int tributesNeeded = 0;
                    for (int i = 0; i < opponentHand.slotList.Count; i++)
                    {
                        if (!opponentHand.slotList[i].container) continue;
                        Monster handMonster = opponentHand.slotList[i].container.GetComponent<Monster>();

                        //check if able to play a level 7 or higher card
                        if (handMonster.level > 6) tributesNeeded = 2;
                        else if (handMonster.level > 4) tributesNeeded = 1;
                        else tributesNeeded = 0;

                        if (tributesNeeded != 0)
                        {
                            if (cardsInField < tributesNeeded) continue;
                            for (int j = 0; j < opponentField.monsterSlots.Length; j++)
                            {
                                if (!opponentField.monsterSlots[j].container) continue;
                                Monster fieldMonster = opponentField.monsterSlots[j].container.GetComponent<Monster>();

                                if (fieldMonster.atk >= maxValueField || fieldMonster.def >= maxValueField)
                                {
                                    maxValueField = opponentField.monsterSlots[j].container.GetComponent<Monster>().atk;
                                    playInDef = false;
                                    if (fieldMonster.def > maxValueField) { maxValueField = fieldMonster.def; playInDef = true; }
                                }
                            }
                            if (handMonster.atk > maxValueField || handMonster.def > maxValueField)
                            {
                                cardToPlay = i;
                                maxValue = handMonster.atk;
                                playInDef = false;
                                if (handMonster.def > maxValue) { maxValue = handMonster.def; playInDef = true; }
                            }
                        }

                        else //if its a level 4 or lower
                        {
                            if (handMonster.atk >= maxValue || handMonster.def >= maxValue) //check for the strongest card
                            {
                                cardToPlay = i;
                                maxValue = handMonster.atk;
                                playInDef = false;
                                if (handMonster.def > maxValue) { maxValue = handMonster.def; playInDef = true; }
                            }
                        }
                    }
                    if (cardToPlay != -1)
                    {
                        opponentHand.PlayCard(cardToPlay + 1, playInDef);

                        int tributes = 0;
                        Monster playMonster = (Monster)opponentHand.slotList[cardToPlay].container;
                        if (playMonster.level > 6) tributes = 2;
                        else if (playMonster.level > 4) tributes = 1;

                        if (tributes > 0)
                        {
                            int selectedCard = -1;
                            for (int i = 0; i < tributes; i++)
                            {
                                int lowestAtk = int.MaxValue;
                                for (int j = 0; j < opponentField.monsterSlots.Length; j++)
                                {
                                    if (!opponentField.monsterSlots[j].container || selectedCard == j) continue;
                                    Monster fieldMonster = (Monster)opponentField.monsterSlots[j].container;

                                    if (fieldMonster.atk < lowestAtk)
                                    {
                                        lowestAtk = fieldMonster.atk;
                                        selectedCard = j;
                                    }
                                }
                                SelectTribute((Monster)opponentField.monsterSlots[selectedCard].container);
                            }
                        }
                    }
                }

                //SCRIPT to change into atk positions (check for def in main phase 2)

                NextPhase();
                yield break;

            case Phase.Battle:
                bool targetIsAttackPos = false;
                for (int i = 0; i < opponentField.monsterSlots.Length; i++)
                {
                    if (!opponentField.monsterSlots[i].container) continue;
                    Monster fieldMonster = (Monster)opponentField.monsterSlots[i].container;

                    int attackTarget = -1;
                    if (fieldMonster != null)
                    {
                        if (fieldMonster.isAttackPosition)
                        {
                            int targetPower = 0;
                            //check for the strongest card this card can defeat
                            for (int j = 0; j < playerField.monsterSlots.Length; j++)
                            {
                                if (!playerField.monsterSlots[j].container) continue;
                                Monster playerFieldMonster = (Monster)playerField.monsterSlots[j].container;

                                if (playerFieldMonster.isAttackPosition)
                                {
                                    if (fieldMonster.atk > playerFieldMonster.atk)
                                    {
                                        if (playerFieldMonster.atk > targetPower) //change to this target if its stronger than the previous one
                                        {
                                            targetIsAttackPos = true;
                                            attackTarget = j;
                                            targetPower = playerFieldMonster.atk;
                                            continue; //priorize cards in attack position
                                        }
                                    }
                                }
                                else  //if its in defense position
                                {
                                    if (targetIsAttackPos) continue; //skip this target if already have an attack position card to attack (always priorizing attack position cards)

                                    if(!playerFieldMonster.isFaceUp)
                                    {
                                        attackTarget = j;
                                    }
                                    else if (fieldMonster.atk > playerFieldMonster.def)
                                    {
                                        if (playerFieldMonster.def > targetPower)
                                        {
                                            attackTarget = j;
                                            targetPower = playerFieldMonster.def;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (attackTarget != -1)
                    {
                        yield return new WaitForSeconds(0.5f);
                        InitiateAttack(fieldMonster);
                        Attack(attackTarget + 1);
                    }
                }

                for (int i = 0; i < opponentField.monsterSlots.Length; i++)
                {
                    if (!opponentField.monsterSlots[i].container) continue;
                    Monster fieldMonster = (Monster) opponentField.monsterSlots[i].container;
                    if (fieldMonster.isAttackPosition && !fieldMonster.hasBattled)
                    {
                        //check for empty field
                        for (int j = 0; j < playerField.monsterSlots.Length; j++)
                        {
                            if (playerField.monsterSlots[j].container) break;
                            if (j == playerField.monsterSlots.Length - 1)
                            {
                                yield return new WaitForSeconds(0.5f);
                                InitiateAttack(fieldMonster);
                            }
                        }
                    }
                }
                NextPhase();
                yield break;

            case Phase.Main2:
                //SCRIPT to change into def positions
                NextPhase();
                yield break;
        }
    }

    void UpdateUITexts()
    {
        currentPhaseText.text = currentPhase.ToString();
        //nextPhaseText.text = GetNextPhase().ToString();
    }

    void DuelStart()
    {
        PlaySound("start");
        player.changeLpRoutine = StartCoroutine(player.ChangeLP(8000, true, true));
        opponent.changeLpRoutine = StartCoroutine(opponent.ChangeLP(8000, false, false));

        player.DrawCard(5);
        opponent.DrawCard(5);
        NextPhase();
    }
    public void ToggleInputs()
    {
        playerInputs.enabled = !playerInputs.enabled;
    }

    Phase GetNextPhase()
    {
        switch (currentPhase)
        {
            case Phase.Draw: return Phase.Standby;
            case Phase.Standby: return Phase.Main;
            case Phase.Main: return Phase.Battle;
            case Phase.Battle: return Phase.Main2;
            case Phase.Main2: return Phase.End;
        }
        PassTurn(); return Phase.Draw;
    }

    void PassTurn()
    {
        playerTurn = !playerTurn;
        currentTurn++;
        UpdatePlayerTurnText(playerTurn);
    }

    void UpdatePlayerTurnText(bool playerTurn)
    {
        currentTurnText.gameObject.SetActive(playerTurn);
        opponentTurnText.gameObject.SetActive(!playerTurn);
    }

    public void NextPhaseButton()
    {
        if (playerTurn) NextPhase();
    }

    void NextPhase()
    {
        currentPhase = GetNextPhase();
        ExecutePhase();
        UpdateUITexts();
    }

    public void PlaySound(string sound)
    {
        //aud[0] = system, aud[1] = battle/effects, aud[2] = cards
        int type = 0;
        switch (sound)
        {
            case "draw":
                type = 2;
                aud[type].clip = drawSound;
                break;
            case "play":
                type = 2;
                aud[type].clip = playSound;
                break;
            case "send":
                type = 2;
                aud[type].clip = sendSound;
                break;
            case "search":
                type = 2;
                aud[type].clip = searchSound;
                break;
            case "select":
                type = 0;
                aud[type].clip = selectSound;
                break;
            case "damage":
                type = 1;
                aud[type].clip = damageSound;
                break;
            case "cancel":
                type = 0;
                aud[type].clip = cancelSound;
                break;
            case "blip":
                type = 0;
                aud[type].clip = blipSound;
                break;
        }
        aud[type].Play();
    }

    void ExecutePhase()
    {
        switch (currentPhase)
        {
            case Phase.Draw:
                if (playerTurn && player.targetLp < 4000) game.ChangeBackgroundMusic(1); //losing music
                else if (playerTurn && opponent.targetLp < 4000) game.ChangeBackgroundMusic(2); //losing music
                break;
            case Phase.Standby:
                if (playerTurn) player.DrawCard(1);
                else opponent.DrawCard(1);
                NextPhase();
                break;
            case Phase.Main:
                if (playerTurn)
                {
                    playerHand.canNormalSummon = true;
                    foreach (Slot slot in playerField.monsterSlots)
                        if (slot.container)
                        {
                            Monster monster = slot.container as Monster;
                            monster.canChangePos = true;
                            monster.hasBattled = false;
                        }
                }
                else
                {
                    opponentHand.canNormalSummon = true;
                    foreach (Slot slot in opponentField.monsterSlots)
                        if (slot.container)
                        {
                            Monster monster = slot.container as Monster;
                            monster.canChangePos = true;
                            monster.hasBattled = false;
                        }
                }
                break;
            case Phase.Battle:
                if (currentTurn == 1) //can't attack on first turn of the duel
                {
                    foreach (Slot slot in playerField.monsterSlots)
                        if (slot.container)
                        {
                            Monster monster = slot.container as Monster;
                            monster.hasBattled = true;
                        }
                    foreach (Slot slot in opponentField.monsterSlots)
                        if (slot.container)
                        {
                            Monster monster = slot.container as Monster;
                            monster.hasBattled = true;
                        }
                }
                break;
            case Phase.Main2:
                break;
            case Phase.End:
                NextPhase();
                break;
        }
        CancelTribute(false);//in case tribute summon was initiated, it will be cancelled when changing phase
        if (!playerTurn) StartCoroutine(AIMove());
    }

    public void MoveCard(Card card, Zone destination, bool set = false, bool isPlayer = true, bool destroyed = false, bool giveControl = false)
    {
        Slot slot = card.gameObject.GetComponentInParent<Slot>();
        Zone previousLocation = slot.location;

        switch (destination) //add card to destination
        {
            case Zone.Field:

                if (set) PlaySound("send");
                else PlaySound("play");

                if (isPlayer)
                {
                    if (!giveControl)
                    {
                        if (!playerField.CheckFull(card))
                            playerField.PlayMonster((Monster)card, set);
                        else return; //end function, so that it doesn't remove card
                    }
                    else if (giveControl)
                    {
                        if (!opponentField.CheckFull(card))
                            opponentField.PlayMonster((Monster)card, set);
                        else return;
                    }
                }
                else if (!isPlayer)
                {
                    if (!giveControl)
                    {
                        if (!opponentField.CheckFull(card))
                            opponentField.PlayMonster((Monster)card, set);
                        else return;
                    }
                    else if (giveControl)
                    {
                        if (!playerField.CheckFull(card))
                            playerField.PlayMonster((Monster)card, set);
                        else return;
                    }
                }
                break;
            case Zone.Hand:
                if (card.ownedByPlayer) playerHand.AddCard(card);
                else opponentHand.AddCard(card);
                break;
            case Zone.Deck:
                PlaySound("send");
                if (card.ownedByPlayer) playerDeck.AddCard(card);
                else opponentDeck.AddCard(card);
                break;
            case Zone.Graveyard:
                if (card.ownedByPlayer) playerGraveyard.AddCard(card);
                else opponentGraveyard.AddCard(card);
                break;
            case Zone.Banished:
                PlaySound("search");
                if (card.ownedByPlayer) playerBanished.AddCard(card);
                else opponentBanished.AddCard(card);
                break;
            case Zone.Fusion:
                PlaySound("send");
                if (card.ownedByPlayer) playerFusionDeck.AddCard(card);
                else opponentFusionDeck.AddCard(card);
                break;
        }

        if (previousLocation == Zone.Deck) card.GetComponentInParent<Deck>().count--;
        else if (previousLocation == Zone.Graveyard) card.GetComponentInParent<Graveyard>().count--;
        else if (previousLocation == Zone.Hand) card.GetComponentInParent<Hand>().count--;
        slot.RemoveCard(); //remove card from previous location
    }

    public void InitiateAttack(Monster card)
    {
        if (card.hasBattled || !card.isAttackPosition)
        {
            Debug.Log("This card can't attack yet");
            PlaySound("cancel");
            return;
        }
        PlaySound("select");
        attackCard = card;
        if (attackCard.GetComponentInParent<Slot>().tag == "Player") //check for empty field for direct attack
        {
            foreach (Slot slot in opponentField.monsterSlots)
            {
                if (slot.container)
                {
                    Debug.Log("Choose target");
                    return;
                }
            }
            PlaySound("damage");
            opponent.changeLpRoutine = StartCoroutine(opponent.ChangeLP(-attackCard.atk));
            attackCard.hasBattled = true;
            attackCard = null;
        }
        else if (attackCard.GetComponentInParent<Slot>().tag == "Opponent")
        {
            foreach (Slot slot in playerField.monsterSlots)
            {
                if (slot.container)
                {
                    Debug.Log("Choose target");
                    return;
                }
            }
            PlaySound("damage");
            player.changeLpRoutine = StartCoroutine(player.ChangeLP(-attackCard.atk));
            attackCard.hasBattled = true;
            attackCard = null;
        }
    }

    public void Attack(int fieldIndex)
    {
        if (attackCard) //if attackCard is assigned, then battle will occur when selecting a valid target
        {
            if (attackCard.GetComponentInParent<Slot>().tag == "Player")
            {
                Monster defendCard = (Monster)opponentField.monsterSlots[fieldIndex - 1].container;
                if (defendCard.isAttackPosition)
                {
                    if (attackCard.atk > defendCard.atk)
                    {
                        opponent.changeLpRoutine = StartCoroutine(opponent.ChangeLP(defendCard.atk - attackCard.atk));
                        MoveCard(defendCard, Zone.Graveyard, false, false, true);
                    }
                    else if (attackCard.atk == defendCard.atk)
                    {
                        MoveCard(attackCard, Zone.Graveyard, false, true, true);
                        MoveCard(defendCard, Zone.Graveyard, false, false, true);
                    }
                    else if (attackCard.atk < defendCard.atk)
                    {
                        player.changeLpRoutine = StartCoroutine(player.ChangeLP(attackCard.atk - defendCard.atk));
                        MoveCard(attackCard, Zone.Graveyard, false, true, true);
                    }
                }
                else if (!defendCard.isAttackPosition) //defending monster is in defense position
                {
                    if (!defendCard.isFaceUp) defendCard.ToggleFaceUp(true); //flip face up if it was face down
                    if (attackCard.atk > defendCard.def)
                    {
                        defendCard.TogglePosition(true, true); //force rotate card before placing it in the graveyard
                        MoveCard(defendCard, Zone.Graveyard, false, false, true);
                    }
                    else if (attackCard.atk < defendCard.def)
                    {
                        player.changeLpRoutine = StartCoroutine(player.ChangeLP(attackCard.atk - defendCard.def));
                    } //nothing will happen if atk value = def value
                }
            }
            else if (attackCard.GetComponentInParent<Slot>().tag == "Opponent")
            {
                Monster defendCard = (Monster)playerField.monsterSlots[fieldIndex - 1].container;
                if (defendCard.isAttackPosition)
                {
                    if (attackCard.atk > defendCard.atk)
                    {
                        player.changeLpRoutine = StartCoroutine(player.ChangeLP(defendCard.atk - attackCard.atk));
                        MoveCard(defendCard, Zone.Graveyard, false, true, true);
                    }
                    else if (attackCard.atk == defendCard.atk)
                    {
                        MoveCard(attackCard, Zone.Graveyard, false, false, true);
                        MoveCard(defendCard, Zone.Graveyard, false, true, true);
                    }
                    else if (attackCard.atk < defendCard.atk)
                    {
                        opponent.changeLpRoutine = StartCoroutine(opponent.ChangeLP(attackCard.atk - defendCard.atk));
                        MoveCard(attackCard, Zone.Graveyard, false, false, true);
                    }
                }
                else if (!defendCard.isAttackPosition)
                {
                    if (!defendCard.isFaceUp) defendCard.ToggleFaceUp(true);
                    if (attackCard.atk > defendCard.def)
                    {
                        defendCard.TogglePosition(true, true);
                        MoveCard(defendCard, Zone.Graveyard, false, true, true);
                    }
                    else if (attackCard.atk < defendCard.def)
                    {
                        opponent.changeLpRoutine = StartCoroutine(opponent.ChangeLP(attackCard.atk - defendCard.def));
                    }
                }
            }
            attackCard.hasBattled = true;
            attackCard = null;
            PlaySound("damage");
        }
    }

    public void InitiateTribute(Monster card, int tributes, bool set)
    {
        PlaySound("blip");
        initiatedTribute = true;
        tributeSummonCard = card;
        tributesLeft = tributes;
        tributeIsSet = set;
    }

    public void CancelTribute(bool playSound = true)
    {
        if (!initiatedTribute) return;
        if (playSound) PlaySound("cancel");

        tributeSummonCard = null;
        tributesLeft = 0;
        for (int i = 0; i < tributes.Length; i++)
            tributes[i] = null;

        initiatedTribute = false;
    }

    public void SelectTribute(Monster card)
    {
        if ((playerTurn && !card.ownedByPlayer) || (!playerTurn && card.ownedByPlayer)) //prevent players from using enemy cards
        {
            CancelTribute();
            return;
        }

        PlaySound("blip");
        tributes[tributesLeft - 1] = card;
        tributesLeft--;
        if (tributesLeft == 0)
        {
            foreach (Monster monster in tributes)
            {
                if (monster)
                {
                    monster.TogglePosition(true, true);
                    MoveCard(monster, Zone.Graveyard, false, card.ownedByPlayer, false); //send tributes to graveyard
                }
            }
            MoveCard(tributeSummonCard, Zone.Field, tributeIsSet, card.ownedByPlayer);
            if (card.ownedByPlayer) playerHand.canNormalSummon = false;
            else if (!card.ownedByPlayer) opponentHand.canNormalSummon = false;
            CancelTribute(false); //clean up for next tribute summon
        }
    }

    public void EndDuel(bool isPlayer)
    {
        Debug.Log("Game Over");
        gameOverText.SetActive(true);
        //Reset();
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
