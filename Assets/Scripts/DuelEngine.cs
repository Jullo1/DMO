using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Phase { Undefined, Draw, Standby, Main, Battle, Main2, End };
public enum Zone { Undefined, Field, Hand, Deck, Graveyard, Banished, Fusion };
public enum SummonCondition { Normal, Special, Fusion, Ritual };

public class DuelEngine : MonoBehaviour
{
    Player player;
    Player opponent;

    Field playerField;
    Deck playerDeck;
    Hand playerHand;
    Graveyard playerGraveyard;
    Banished playerBanished;
    FusionDeck playerFusionDeck;

    EventSystem playerInputs;

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

    AudioSource aud;
    //duel sfx
    [SerializeField] AudioClip startSound;
    [SerializeField] AudioClip endSound;
    [SerializeField] AudioClip lpSound;
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

    void Awake()
    {
        aud = GetComponent<AudioSource>();
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

    public void NextPhase()
    {
        currentPhase = GetNextPhase();
        ExecutePhase();
        UpdateUITexts();
    }

    public void PlaySound(string sound)
    {
        switch (sound)
        {
            case "start":
                aud.clip = startSound;
                break;
            case "draw":
                aud.clip = drawSound;
                break;
            case "play":
                aud.clip = playSound;
                break;
            case "send":
                aud.clip = sendSound;
                break;
            case "search":
                aud.clip = searchSound;
                break;
            case "select":
                aud.clip = selectSound;
                break;
            case "damage":
                aud.clip = damageSound;
                break;
            case "lp":
                aud.clip = lpSound;
                break;
            case "end":
                aud.clip = endSound;
                break;
            case "cancel":
                aud.clip = cancelSound;
                break;
            case "blip":
                aud.clip = blipSound;
                break;
        }
        aud.Play();
    }

    void ExecutePhase()
    {
        switch (currentPhase)
        {
            case Phase.Draw:
                if (playerTurn) player.DrawCard(1);
                else opponent.DrawCard(1);
                NextPhase();
                break;
            case Phase.Standby:
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
        CancelTribute();//in case tribute summon was initiated, it will be cancelled when changing phase
    }

    public void MoveCard(Card card, Zone destination, bool set = false, bool isPlayer = true, bool destroyed = false, bool giveControl = false)
    {
        Slot slot = card.gameObject.GetComponentInParent<Slot>();
        Zone previousLocation = slot.location;

        switch (destination) //add card to destination
        {
            case Zone.Field:
                PlaySound("play");
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
            PlaySound("damage");
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
        PlaySound("blip");
        tributes[tributesLeft - 1] = card;
        tributesLeft--;
        if (tributesLeft == 0)
        {
            foreach (Monster monster in tributes)
            {
                if (monster) MoveCard(monster, Zone.Graveyard, false, card.ownedByPlayer, false); //send tributes to graveyard
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
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
