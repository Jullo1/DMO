using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] List<Card> deck1 = new List<Card>();
    [SerializeField] List<Card> deck2 = new List<Card>();
    [SerializeField] List<Card> test = new List<Card>();

    public void ChooseDeck(int deck)
    {
        if (deck == 1) Deck.playerDeck = deck1;
        else if (deck == 2) Deck.playerDeck = deck2;
        SceneManager.LoadScene("Duel");
    }
}
