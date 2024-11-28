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

        //MusicManager.Instance.SetSong("MENU");
        //MusicManager.Instance.SetPhase(0);

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
        /*
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
        //*/



        if (Input.GetKeyDown(KeyCode.Alpha1)) // Entra en batalla en la playa
        {
            Debug.Log("ENTRA EN BATALLA");
            MusicManager.Instance.InitPartida(2); // 0 - playa, 1 - sushi, 2 - barco
            MusicManager.Instance.FasePartida(4,4); 
        }
        

        if (Input.GetKeyDown(KeyCode.Alpha2)) // Muere alguien
        {
            Debug.Log("MUERE ALGUIEN Y HAY 3");
            MusicManager.Instance.FasePartida(3, 4); // primero jugadores vivos, despues jugadores totales
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) // Muere alguien
        {
            Debug.Log("MUERE ALGUIEN Y HAY 2");
            MusicManager.Instance.FasePartida(2, 4);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4)) // Se eligen potenciadores
        {
            Debug.Log("SE ELIGEN POTENCIADORES");
            MusicManager.Instance.FasePotenciadores();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) // Se terminan de elegir potenciadores
        {
            Debug.Log("SE TERMINA DE ELEGIR POTENCIADORES");
            MusicManager.Instance.FasePartida(4, 4);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6)) // Se termina la partida y se pierde
        {
            Debug.Log("SE TERMINA LA PARTIDA");
            MusicManager.Instance.FinPartida();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)) //  Se ven los resultados
        {
            Debug.Log("SE TERMINA LA PARTIDA");
            MusicManager.Instance.Resultados(1); // 0 si pierde, 1 si gana
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)) // Semaforo1
        {
            Debug.Log("SE TERMINA LA PARTIDA");
            MusicManager.Instance.Semaforo0();
            MusicManager.Instance.MuteSong();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) // Semaforo
        {
            Debug.Log("SE TERMINA LA PARTIDA");
            MusicManager.Instance.Semaforo1();
            MusicManager.Instance.InitPartida(1); // 0 - playa, 1 - sushi, 2 - barco
            MusicManager.Instance.FasePartida(4, 4);
        }

        /*
            // Cuenta atrás primeros pip
            MusicManager.Instance.Semaforo0();
            MusicManager.Instance.MuteSong();

            // Cuenta atrás ultimo pip
            MusicManager.Instance.Semaforo1();

            MusicManager.Instance.InitPartida(2); // 0 - playa, 1 - sushi, 2 - barco
            
            // Cuando se muere alguien
            MusicManager.Instance.FasePartida(3, 4); // primero jugadores vivos, despues jugadores totales
         
            // Cuando se pasa a los potenciadores
            MusicManager.Instance.FasePotenciadores();

            // Al volver a la partida
            MusicManager.Instance.FasePartida(4, 4);
            
            // Cuando muere el utlimo en la ultima partida
            MusicManager.Instance.FinPartida();

            // En la pantalla de resultados
            MusicManager.Instance.Resultados(1); // 0 si pierde, 1 si gana
            
         
         
         
         */



        /////////////////////////////////////

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

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("Emitiendo sonido");
            MusicManager.Instance.PlayDisparo();
        }

        //*/
    }
}
