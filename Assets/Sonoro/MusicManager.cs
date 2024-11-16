using UnityEngine;
using System.Collections.Generic;

public class MusicManager : Singleton<MusicManager>
{
    [SerializeField] private AudioClip[] playaClips;
    [SerializeField] private AudioClip[] sushiClips;
    [SerializeField] private AudioClip[] barcoClips;
    [SerializeField] private AudioClip[] submarinoClips;
    [SerializeField] private AudioClip[] menuClips;
    [SerializeField] private AudioClip[] pveClips;

    [Range(0, 1)] public float volMusic = 1.0f; // Volumen de la música
    [Range(0, 1)] public float volSounds = 1.0f; // Volumen de los sonidos

    private readonly Dictionary<string, AudioClip[]> songs = new();
    private AudioSource audioSourceA;
    private AudioSource audioSourceB;
    private bool isPlayingA = true;

    public AudioClip[] currentClips { get; private set; }
    private int currentPhase = 0;
    private const float FadeDuration = 0.4f;
    private float fadeTimer = 0f;
    private bool isTransitioning = false;
    private bool isSongTransitioning = false;

    private string queuedSongID;
    private int queuedPhase = -1;
    private float queuedStartTime = 0f;

    private readonly List<AudioSource> soundPool = new();
    private const int PoolSize = 10;

    private float lastVolMusic = -1f;
    private float lastVolSounds = -1f;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        audioSourceA = CreateAudioSource(true);
        audioSourceB = CreateAudioSource(true);

        songs["PLAYA"] = playaClips;
        songs["SUSHI"] = sushiClips;
        songs["BARCO"] = barcoClips;
        songs["SUBMARINO"] = submarinoClips;
        songs["MENU"] = menuClips;
        songs["PVE"] = pveClips;

        InitializeSoundPool();
    }

    private void OnValidate()
    {
        volMusic = Mathf.Clamp01(volMusic);
        volSounds = Mathf.Clamp01(volSounds);
        UpdateMusicVolume();
        UpdateSoundVolume();
    }

    private void Update()
    {
        // Detectar cambios en los volúmenes
        if (!Mathf.Approximately(volMusic, lastVolMusic))
        {
            lastVolMusic = volMusic;
            UpdateMusicVolume();
        }

        if (!Mathf.Approximately(volSounds, lastVolSounds))
        {
            lastVolSounds = volSounds;
            UpdateSoundVolume();
        }

        // Transiciones de música
        if (isTransitioning || isSongTransitioning)
        {
            fadeTimer += Time.deltaTime;
            float fadeProgress = Mathf.Clamp01(fadeTimer / FadeDuration);

            AudioSource activeSource = isPlayingA ? audioSourceA : audioSourceB;
            AudioSource newSource = isPlayingA ? audioSourceB : audioSourceA;

            activeSource.volume = Mathf.Lerp(volMusic, 0, fadeProgress);
            newSource.volume = Mathf.Lerp(0, volMusic, fadeProgress);

            if (fadeProgress >= 1f)
            {
                activeSource.Stop();
                EndTransition();
            }
        }
    }

    private void UpdateMusicVolume()
    {
        if (audioSourceA != null) audioSourceA.volume = volMusic;
        if (audioSourceB != null) audioSourceB.volume = volMusic;
        Debug.Log($"Volumen de música actualizado: {volMusic}");
    }

    private void UpdateSoundVolume()
    {
        foreach (var source in soundPool)
        {
            if (!source.isPlaying) continue; // Solo actualizar fuentes activas
            source.volume = volSounds;
        }
        Debug.Log($"Volumen de sonidos actualizado: {volSounds}");
    }

    private AudioSource CreateAudioSource(bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.volume = volMusic; // Configurar el volumen inicial
        return source;
    }

    private void InitializeSoundPool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var source = CreateAudioSource(false);
            source.playOnAwake = false;
            source.volume = volSounds;
            source.enabled = false;
            soundPool.Add(source);
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in soundPool)
        {
            if (!source.isPlaying)
            {
                source.enabled = true;
                source.volume = volSounds;
                return source;
            }
        }

        var newSource = CreateAudioSource(false);
        newSource.playOnAwake = false;
        soundPool.Add(newSource);
        return newSource;
    }

    private void EndTransition()
    {
        isTransitioning = false;
        isSongTransitioning = false;
        fadeTimer = 0f;
        isPlayingA = !isPlayingA;

        if (!string.IsNullOrEmpty(queuedSongID))
        {
            SetSong(queuedSongID);
            queuedSongID = null;
        }
        else if (queuedPhase >= 0)
        {
            StartTransition(queuedPhase, queuedStartTime);
            queuedPhase = -1;
        }
    }

    public void SetPhase(int phase)
    {
        if (isTransitioning || isSongTransitioning || currentClips == null || phase == currentPhase) return;

        if (phase >= 0 && phase < currentClips.Length)
        {
            float currentTime = (isPlayingA ? audioSourceA : audioSourceB).time;
            StartTransition(phase, currentTime);
        }
    }

    public void SetSong(string songID)
    {
        if (isSongTransitioning || isTransitioning)
        {
            queuedSongID = songID;
            return;
        }

        if (!songs.TryGetValue(songID, out var clips))
        {
            Debug.LogError($"La canción '{songID}' no existe");
            return;
        }

        currentClips = clips;
        currentPhase = 0;
        PreloadClips(currentClips);
        StartSongTransition(0);
    }

    private void PreloadClips(AudioClip[] clips)
    {
        foreach (var clip in clips)
        {
            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                clip.LoadAudioData();
            }
        }
    }

    private void StartSongTransition(float startTime)
    {
        AudioSource newSource = isPlayingA ? audioSourceB : audioSourceA;

        if (currentClips == null || currentClips.Length == 0)
        {
            Debug.LogError("No hay clips disponibles para la transición.");
            return;
        }

        newSource.clip = currentClips[0];
        startTime = Mathf.Clamp(startTime, 0, newSource.clip.length);
        newSource.time = startTime;
        newSource.volume = volMusic;
        newSource.Play();

        isSongTransitioning = true;
        fadeTimer = 0f;
    }

    private void StartTransition(int newPhase, float startTime)
    {
        AudioSource newSource = isPlayingA ? audioSourceB : audioSourceA;

        if (currentClips == null || newPhase < 0 || newPhase >= currentClips.Length)
        {
            Debug.LogError("No hay clips disponibles para la transición o el índice está fuera de rango.");
            return;
        }

        newSource.clip = currentClips[newPhase];
        startTime = Mathf.Min(startTime, newSource.clip.length - 0.1f);
        newSource.time = startTime;
        newSource.volume = volMusic;
        newSource.Play();

        isTransitioning = true;
        fadeTimer = 0f;
        currentPhase = newPhase;
    }

    public void PlaySound(string soundName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Sonidos/{soundName}");

        if (clip == null)
        {
            Debug.LogError($"El sonido '{soundName}' no se encontró en la carpeta Resources/Sonidos.");
            return;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.pitch = 1.0f;
        audioSource.clip = clip;
        audioSource.volume = volSounds;
        audioSource.Play();
    }

    public void PlaySoundPitch(string soundName)
    {
        PlaySoundPitch(soundName, 0.1f);
    }

    public void PlaySoundPitch(string soundName, float pitchVariation)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Sonidos/{soundName}");

        if (clip == null)
        {
            Debug.LogError($"El sonido '{soundName}' no se encontró en la carpeta Resources/Sonidos.");
            return;
        }

        pitchVariation = Mathf.Clamp(pitchVariation, 0f, 1f);

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.clip = clip;

        float minPitch = 1f - pitchVariation;
        float maxPitch = 1f + pitchVariation;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = volSounds;
        audioSource.Play();
    }
}
