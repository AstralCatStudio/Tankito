using UnityEngine;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioClip[] playaClips;
    public AudioClip[] sushiClips;
    public AudioClip[] barcoClips;
    public AudioClip[] submarinoClips;
    public AudioClip[] menuClips;
    public AudioClip[] pveClips;

    private Dictionary<string, AudioClip[]> songs = new Dictionary<string, AudioClip[]>();
    private AudioSource audioSourceA;
    private AudioSource audioSourceB;
    private bool isPlayingA = true;

    public AudioClip[] currentClips;
    private int currentPhase = 0;
    private float fadeDuration = 1.0f;
    private float fadeTimer = 0f;
    private bool isTransitioning = false;
    private bool isSongTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSourceA = gameObject.AddComponent<AudioSource>();
        audioSourceB = gameObject.AddComponent<AudioSource>();
        audioSourceA.loop = true;
        audioSourceB.loop = true;

        songs["PLAYA"] = playaClips;
        songs["SUSHI"] = sushiClips;
        songs["BARCO"] = barcoClips;
        songs["SUBMARINO"] = submarinoClips;
        songs["MENU"] = menuClips;
        songs["PVE"] = pveClips;
    }

    private void Update()
    {
        if (isTransitioning || isSongTransitioning)
        {
            fadeTimer += Time.deltaTime;
            float fadeProgress = Mathf.Clamp01(fadeTimer / fadeDuration);

            AudioSource activeSource = isPlayingA ? audioSourceA : audioSourceB;
            AudioSource newSource = isPlayingA ? audioSourceB : audioSourceA;

            activeSource.volume = Mathf.Lerp(1, 0, fadeProgress);
            newSource.volume = Mathf.Lerp(0, 1, fadeProgress);

            if (fadeProgress >= 1f)
            {
                activeSource.Stop();
                isTransitioning = false;
                isSongTransitioning = false;
                fadeTimer = 0f;
                isPlayingA = !isPlayingA;
            }
        }
    }

    public void SetSong(string songID)
    {
        if (!songs.ContainsKey(songID))
        {
            Debug.LogError($"ERROR: La canción con ID: '{songID}' no existe.");
            return;
        }

        currentClips = songs[songID];
        currentPhase = 0;

        // Precargar cada clip de la canción seleccionada
        foreach (AudioClip clip in currentClips)
        {
            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                clip.LoadAudioData();
            }
        }

        StartSongTransition(0);
        //Debug.Log($"Transición hacia la canción '{songID}' en fase {currentPhase}.");
    }

    private void StartSongTransition(float startTime)
    {
        AudioSource newSource = isPlayingA ? audioSourceB : audioSourceA;

        newSource.clip = currentClips[0];

        if (newSource.clip.loadState != AudioDataLoadState.Loaded)
        {
            //Debug.LogWarning($"Clip '{newSource.clip.name}' aún no se ha cargado completamente. Reiniciando el tiempo a 0.");
            startTime = 0f;
        }

        newSource.time = Mathf.Clamp(startTime, 0, newSource.clip.length);
        newSource.volume = 0f;
        newSource.Play();

        isSongTransitioning = true;
        fadeTimer = 0f;
    }

    public void SetPhase(int phase)
    {
        if (currentClips == null || phase < 0 || phase >= currentClips.Length || phase == currentPhase) return;

        float currentTime = (isPlayingA ? audioSourceA : audioSourceB).time;
        StartTransition(phase, currentTime);
    }

    private void StartTransition(int newPhase, float startTime)
    {
        AudioSource activeSource = isPlayingA ? audioSourceA : audioSourceB;
        AudioSource newSource = isPlayingA ? audioSourceB : audioSourceA;

        newSource.clip = currentClips[newPhase];

        // Verificar si el clip está completamente cargado
        if (newSource.clip.loadState != AudioDataLoadState.Loaded)
        {
            //Debug.LogWarning($"Clip '{newSource.clip.name}' aún no se ha cargado completamente. Reiniciando el tiempo a 0.");
            startTime = 0f;
        }

        startTime = Mathf.Clamp(startTime, 0, newSource.clip.length);
        newSource.time = startTime;
        newSource.volume = 0f;
        newSource.Play();

        isTransitioning = true;
        fadeTimer = 0f;
        currentPhase = newPhase;
    }
}
