using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    DuelEngine engine;

    Deck deck;
    Hand hand;

    int LP;
    public Text LPnum;

    AudioSource lpAudio;
    public Coroutine changeLpRoutine;
    int targetLp;
    bool stopLpRoutine = false;
    [SerializeField] AudioClip endSound;

    void Awake()
    {
        engine = FindObjectOfType<DuelEngine>().GetComponent<DuelEngine>();
        lpAudio = GetComponentInChildren<AudioSource>();

        foreach (Hand hand in FindObjectsOfType<Hand>())
            if (hand.tag == tag) //check for player or opponent
                this.hand = hand;

        foreach (Deck deck in FindObjectsOfType<Deck>())
            if (deck.tag == tag)
                this.deck = deck;
    }

    public IEnumerator ChangeLP(int amount, bool lockInputs = false, bool noSound = false)
    {
        //finish the previous routine
        if (changeLpRoutine != null)
        {
            lpAudio.Stop();
            stopLpRoutine = true;
            yield return new WaitForSeconds(0.08f);
            stopLpRoutine = false;
        }

        targetLp = LP + amount;
        int times = 50;
        if (lockInputs) engine.ToggleInputs();

        //game over
        if (targetLp <= 0)
        {
            amount = - (LP + 80);
            times = 80;
            engine.ToggleInputs();
            lpAudio.clip = endSound;
            lpAudio.Play();
        }
        else if (!noSound) lpAudio.Play();

        int count = 0;
        while (count < times)
        {
            if (LP < 0)
            {
                LP = 0;
                break;
            }
            else if (stopLpRoutine)
            {
                break;
            }
            yield return new WaitForSeconds(0.04f);
            LP += amount / times;
            LPnum.text = LP.ToString();
            count++;
        }

        if (lockInputs) engine.ToggleInputs();

        LP = targetLp;

        if (LP <= 0)
        {
            LP = 0;
            engine.ToggleInputs();
            engine.EndDuel(tag == "Player");
        }

        LPnum.text = LP.ToString();
    }

    public void DrawCard(int amount)
    {
        engine.PlaySound("draw");
        if (deck.count == 0)
        {
            engine.EndDuel(tag == "Player"); //ran out of cards, check if it's the player and end duel
            return;
        }
        for (int i = 0; i < amount; i++)
            engine.MoveCard(deck.slotList[deck.count-1].container, Zone.Hand);
    }
}
