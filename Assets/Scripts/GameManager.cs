using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    DuelEngine engine;

    //soundtrack
    AudioSource[] backgroundMusic;
    /* [SerializeField] AudioClip[] soundtrack;
    int currentMusicPhase = 0;
    float previousMusicState;
    bool[] alreadyPlayed = new bool[10];*/
    bool audioFixGl;

    void Awake()
    {
        backgroundMusic = GetComponents<AudioSource>();
        engine = FindObjectOfType<DuelEngine>();
    }

    void Start()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer) backgroundMusic[1].PlayDelayed(backgroundMusic[0].clip.length);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) engine.Reset(); 
        
        if (Application.platform == RuntimePlatform.WebGLPlayer) //fix for webgl music
            if (!backgroundMusic[0].isPlaying && !audioFixGl)
            {
                backgroundMusic[1].Play();
                audioFixGl = true;
            }
    }

    /*public void ChangeBackgroundMusic(int phase)
    {
        if (alreadyPlayed[phase] || phase < currentMusicPhase) return;
        alreadyPlayed[phase] = true;
        previousMusicState = backgroundMusic.time;
        currentMusicPhase = phase;
        backgroundMusic.clip = soundtrack[phase];
        backgroundMusic.Play();
    }*/

}
