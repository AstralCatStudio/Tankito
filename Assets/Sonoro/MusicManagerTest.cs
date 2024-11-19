using UnityEngine;

public class MusicManagerTest : MonoBehaviour
{
    private float timeBetweenPhases = 5f;
    private float elapsedTime = 0f;
    private int currentPhase = 0;

    void Start()
    {
        //*
        if (MusicManager.Instance == null)
        {
            Debug.LogError("No se ha encontrado una instancia de MusicManager");
            return;
        }

        MusicManager.Instance.SetSong("VICTORY");

        /*
        MusicManager.Instance.SetPhase(0);
        MusicManager.Instance.SetPhase(1);
        MusicManager.Instance.SetPhase(2);
        MusicManager.Instance.SetPhase(3);
        MusicManager.Instance.SetPhase(4);
        MusicManager.Instance.SetPhase(5);
        MusicManager.Instance.SetPhase(0);
        
         //*/
    }

    void Update()
    {
        ///*
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= timeBetweenPhases)
        {
            elapsedTime = 0f;

            if (MusicManager.Instance.currentClips != null)
            {
                currentPhase = (currentPhase + 1) % MusicManager.Instance.currentClips.Length;
                Debug.Log($"Cambiando a fase {currentPhase} de la canción actual.");
                MusicManager.Instance.SetPhase(currentPhase);
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Cambiando a la canción MENU");
            MusicManager.Instance.SetSong("MENU");
            currentPhase = 0;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Cambiando a la canción PLAYA");
            MusicManager.Instance.SetSong("PLAYA");
            currentPhase = 0;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Emitiendo sonido");
            MusicManager.Instance.PlaySoundPitch("snd_parry");
        }

        //*/
    }
}
