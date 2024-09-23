using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public enum Phase { Undefined, Draw, Standby, Main, Battle, Main2, End };
public enum Zone { Undefined, Field, Hand, Deck, Graveyard, Banished, Fusion };
public enum SummonCondition { Normal, Special, Fusion, Ritual };
public enum RestrictionType { None, Battle }

public class DuelEngine : MonoBehaviour
{
    GameManager game;

    Player player;
    Player opponent;

    Field playerField;
    Deck playerDeck;
    Hand playerHand;
    Graveyard playerGraveyard;
    FusionDeck playerFusionDeck;

    Field opponentField;
    Deck opponentDeck;
    Hand opponentHand;
    Graveyard opponentGraveyard;
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
    public Card activatedCard = null;

    AudioSource[] aud;
    //duel sfx
    [SerializeField] AudioClip drawSound;
    [SerializeField] AudioClip playSound;
    [SerializeField] AudioClip searchSound;
    [SerializeField] AudioClip sendSound;
    [SerializeField] AudioClip statusSound;
    [SerializeField] AudioClip damageSound;
    [SerializeField] AudioClip magicSound;
    [SerializeField] AudioClip magic2Sound;

    //menus sfx
    [SerializeField] AudioClip blipSound;
    [SerializeField] AudioClip selectSound;
    [SerializeField] AudioClip decideSound;
    [SerializeField] AudioClip cancelSound;
    [SerializeField] AudioClip cantSound;
    [SerializeField] AudioClip inSound;

    EventSystem playerInputs;
    [SerializeField] Text alertText;
    Animator alertTextAnim;

    void Awake()
    {
        game = FindObjectOfType<GameManager>();
        aud = GetComponents<AudioSource>();
        playerInputs = FindAnyObjectByType<EventSystem>();
        alertTextAnim = alertText.gameObject.GetComponent<Animator>();

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
        StartCoroutine(DuelStart());
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
                if (opponentHand.cardList.Count > 0) //play a card
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
                    for (int i = 0; i < opponentHand.cardList.Count; i++)
                    {
                        if (!opponentHand.cardList[i]) continue;
                        Monster handMonster = opponentHand.cardList[i].GetComponent<Monster>();

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
                        Monster playMonster = (Monster)opponentHand.cardList[cardToPlay];
                        opponentHand.PlayCard(cardToPlay, playInDef);

                        int tributes = 0;
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

    IEnumerator DuelStart()
    {
        AlertText("Duel Start", true);
        PlaySound("start");
        player.changeLpRoutine = StartCoroutine(player.ChangeLP(8000, true, true));
        opponent.changeLpRoutine = StartCoroutine(opponent.ChangeLP(8000, false, false));

        yield return new WaitForSeconds(0.1f);
        player.DrawCard(5);
        opponent.DrawCard(5);
        yield return new WaitForSeconds(1.7f);
        NextPhase();
    }
    public void ToggleInputs()
    {
        playerInputs.enabled = !playerInputs.enabled;
    }

    public void Draw(bool isPlayer, int amount)
    {
        if (isPlayer) player.DrawCard(amount);
        else opponent.DrawCard(amount);
    }

    public void ChangeLP(bool isPlayer, int amount)
    {
        if (isPlayer) StartCoroutine(player.ChangeLP(amount));
        else StartCoroutine(opponent.ChangeLP(amount));
    }

    Phase GetNextPhase()
    {
        switch (currentPhase)
        {
            case Phase.Draw: return Phase.Standby;
            case Phase.Standby: return Phase.Main;
            case Phase.Main:
                if (playerTurn && player.cantAttack > 0) return Phase.End;
                else if (!playerTurn && opponent.cantAttack > 0) return Phase.End;
                else return Phase.Battle;
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
            case "cant":
                type = 0;
                aud[type].clip = cantSound;
                break;
            case "blip":
                type = 0;
                aud[type].clip = blipSound;
                break;
            case "magic":
                type = 1;
                aud[type].clip = magicSound;
                break;
            case "magic2":
                type = 1;
                aud[type].clip = magic2Sound;
                break;
            case "in":
                type = 0;
                aud[type].clip = inSound;
                break;
        }
        aud[type].Play();
    }

    void ExecutePhase()
    {
        switch (currentPhase)
        {
            case Phase.Draw:
                AlertText("Draw a card");
                if (playerTurn && player.targetLp < 4000 && player.targetLp > 0) game.ChangeBackgroundMusic(1); //losing music
                //else if (playerTurn && opponent.targetLp < 4000 && opponent.targetLp > 0) game.ChangeBackgroundMusic(2); //winning music
                break;
            case Phase.Standby:
                AlertText("");
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
                if (playerTurn && player.cantAttack > 0)
                    player.cantAttack--;
                else if (!playerTurn && opponent.cantAttack > 0)
                    opponent.cantAttack--;
                CheckCardDurations();
                NextPhase();
                break;
        }
        CancelTribute(false);//in case tribute summon was initiated, it will be cancelled when changing phase
        if (!playerTurn) StartCoroutine(AIMove());
    }

    void CheckCardDurations()
    {
        foreach (Field field in FindObjectsOfType<Field>())
        {
            foreach (Slot slot in field.spellTrapSlots)
            {
                if (slot.container == null) continue;
                SpellTrap spellTrap = slot.container.GetComponent<SpellTrap>();
                if (spellTrap.turnsLeft == 1) MoveCard(spellTrap, Zone.Graveyard, false, spellTrap.ownedByPlayer);
                if (spellTrap.turnsLeft > 0) spellTrap.turnsLeft--;
            }
        }
    }

    public void MoveCard(Card card, Zone destination, bool set = false, bool isPlayer = true, bool destroyed = false, bool specialSummon = false, bool giveControl = false)
    {
        switch (destination) //add card to destination
        {
            case Zone.Field:
                if (card.GetType() == typeof(SpellTrap))
                {
                    if (!playerField.CheckFull(card, false))
                    {
                        playerField.PlaySpellTrap((SpellTrap)card, set);
                    }
                    else AlertText("Field is full!", true);
                    break;
                }

                if (isPlayer)
                {
                    if (!giveControl)
                    {
                        if (!playerField.CheckFull(card, true))
                        {
                            playerField.PlayMonster((Monster)card, set);
                            if (!specialSummon) playerHand.canNormalSummon = false;
                            card.GetComponent<Monster>().hasBattled = false;
                        }
                        else AlertText("Field is full!", true);
                    }
                    else if (giveControl)
                    {
                        if (!playerField.CheckFull(card, true))
                        {
                            opponentField.PlayMonster((Monster)card, set);
                            if (!specialSummon) playerHand.canNormalSummon = false;
                            card.GetComponent<Monster>().hasBattled = false;
                        }
                        else AlertText("Field is full!", true);
                    }
                }
                else if (!isPlayer)
                {
                    if (!giveControl)
                    {
                        if (!opponentField.CheckFull(card, true))
                            opponentField.PlayMonster((Monster)card, set);
                        else AlertText("Field is full!", true);
                    }
                    else if (giveControl)
                    {
                        if (!playerField.CheckFull(card, true))
                            playerField.PlayMonster((Monster)card, set);
                        else AlertText("Field is full!", true);
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
                card.ToggleFaceUp(true);
                if (card.ownedByPlayer) playerGraveyard.AddCard(card);
                else opponentGraveyard.AddCard(card);
                break;
            case Zone.Fusion:
                PlaySound("send");
                if (card.ownedByPlayer) playerFusionDeck.AddCard(card);
                else opponentFusionDeck.AddCard(card);
                break;
        }
    }

    public void InitiateAttack(Monster card)
    {
        if (card.hasBattled || !card.isAttackPosition)
        {
            AlertText("This card can't attack yet", true);
            PlaySound("cant");
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
                    AlertText("Choose target");
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
                    AlertText("Choose target");
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
        AlertText("");
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

        AlertText("");
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
        AlertText("Press ESC to reset...");
    }

    public void AlertText(string message, bool staticMessage = false)
    {
        if (currentPhase == Phase.Draw && message != "Draw a card") return;

        alertTextAnim.SetBool("static", staticMessage);
        if (playerTurn) alertText.text = message;
    }

    public void Reset()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void InitiateSelectTarget(Card card)
    {
        activatedCard = card;
        AlertText("Choose a target");

        if (card.GetType() == typeof(SpellTrap))
            PlaySound("in");
    }

    public void SelectTarget(Card target)
    {
        if (activatedCard.GetType() == typeof(SpellTrap))
        {
            SpellTrap spellTrap = activatedCard as SpellTrap;
            if (spellTrap.targetType == Target.Monster)
            {
                Monster monster = target as Monster;
                if (spellTrap.effectType == EffectType.ChangePosition)
                {
                    if (monster.isAttackPosition == spellTrap.requiresAtkPos)
                        spellTrap.target = target;
                }
                else
                {
                    if (monster.type == spellTrap.requiredType)
                    {
                        spellTrap.target = target;
                        if (spellTrap.spellType == SpellType.Equip) monster.equips.Add(spellTrap);
                    }
                }
            }

            if (spellTrap.target != null)
            {
                spellTrap.TriggerEffects(true);
                activatedCard = null;
                AlertText("");
                PlaySound("magic");
                return;
            }
        }
        AlertText("Invalid target", true);
    }

    public void ManualTriggerGraveyard(bool state, bool isPlayer)
    {
        if (isPlayer) playerGraveyard.ManualTrigger(state);
        else opponentGraveyard.ManualTrigger(state);
    }

    public void ApplyRestriction(bool isPlayer, RestrictionType restrictionType, int amount)
    {
        switch (restrictionType)
        {
            case RestrictionType.Battle:
                if (isPlayer) player.cantAttack = 3;
                else opponent.cantAttack = 3;
                break;
        }
    }
}
