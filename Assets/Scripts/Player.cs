using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    DuelEngine engine;

    Hand hand;
    Deck deck;

    int LP;
    public Text LPnum;

    void Awake()
    {
        engine = FindObjectOfType<DuelEngine>().GetComponent<DuelEngine>();

        foreach (Hand hand in FindObjectsOfType<Hand>())
            if (hand.tag == tag) //check for player or opponent
                this.hand = hand;

        foreach (Deck deck in FindObjectsOfType<Deck>())
            if (deck.tag == tag)
                this.deck = deck;
    }

    public void ChangeLP(int amount)
    {
        LP += amount;
        LPnum.text = LP.ToString();
        if (LP <= 0) engine.EndDuel(tag == "Player");
    }

    public void DrawCard(int amount)
    {
        if (deck.count == 0)
        {
            engine.EndDuel(tag == "Player"); //ran out of cards, check if it's the player and end duel
            return;
        }
        for (int i = 0; i < amount; i++)
            engine.MoveCard(deck.slotList[deck.count-1].container, Zone.Hand);
    }
}
