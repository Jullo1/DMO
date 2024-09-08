using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    DuelEngine engine;
    Text textUI;

    //soundtrack
    AudioSource[] backgroundMusic;
    [SerializeField] AudioClip[] trackStarts;
    [SerializeField] AudioClip[] trackLoops;
    int currentTrack = -1;
    bool audioFixGl;

    void Awake()
    {
        backgroundMusic = GetComponents<AudioSource>();
        engine = FindObjectOfType<DuelEngine>();
        textUI = GameObject.FindGameObjectWithTag("CardTextUI").gameObject.GetComponent<Text>();
    }

    void Start()
    {
        ChangeBackgroundMusic(0);
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

    public void ChangeBackgroundMusic(int phase)
    {
        if (phase == currentTrack) return;
        currentTrack = phase;

        backgroundMusic[0].clip = trackStarts[phase];
        backgroundMusic[1].clip = trackLoops[phase];

        backgroundMusic[0].Play();
        if (Application.platform != RuntimePlatform.WebGLPlayer) backgroundMusic[1].PlayDelayed(backgroundMusic[0].clip.length);
        else audioFixGl = false;
    }

    public void ChangeTextUI(string text)
    {
        textUI.text = text;
    }

}
