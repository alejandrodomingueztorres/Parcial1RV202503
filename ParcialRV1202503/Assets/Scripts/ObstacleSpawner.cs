using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] obstaculos; // Diferentes tipos de obst�culos
    public GameObject lataPrefab; // Prefab de la lata de comida

    [Header("Configuraci�n de Spawn")]
    public Transform jugador;
    public float distanciaSpawn = 50f; // Distancia adelante del jugador para generar
    public float distanciaMinima = 30f; // Distancia m�nima adelante del jugador
    public InfiniteRunner terreno; // Referencia al sistema de terreno infinito

    [Header("Frecuencia de Spawn")]
    public float intervaloObstaculos = 3f; // Segundos entre obst�culos
    public float intervaloLatas = 2f; // Segundos entre latas
    public int latasconsecutivasMax = 3; // M�ximo de latas consecutivas

    [Header("Dificultad")]
    public float incrementoDificultad = 0.1f; // Cada cu�nto se reduce el intervalo
    public float tiempoParaIncremento = 15f; // Cada 15 segundos aumenta dificultad
    public float intervaloMinimo = 1f; // Intervalo m�nimo entre obst�culos

    [Header("Sistema de Latas Obligatorias")]
    public float tiempoMaximoSinLata = 8f; // Tiempo m�ximo sin recoger lata antes de game over
    private float tiempoUltimaLata;
    private bool juegoTerminado = false;

    [Header("Referencias")]
    public GameManager gameManager;
    public Puntuacion sistemaPuntos;

    private List<GameObject> obstaculosActivos = new List<GameObject>();
    private List<GameObject> latasActivas = new List<GameObject>();
    private int latasConsecutivas = 0;

    void Start()
    {
        tiempoUltimaLata = Time.time;

        // Iniciar rutinas de generaci�n
        StartCoroutine(GenerarObstaculos());
        StartCoroutine(GenerarLatas());
        StartCoroutine(IncrementarDificultad());
        StartCoroutine(VerificarTiempoLatas());
    }

    void Update()
    {
        LimpiarObjetosLejanos();
    }

    IEnumerator GenerarObstaculos()
    {
        while (!juegoTerminado)
        {
            yield return new WaitForSeconds(intervaloObstaculos);

            // Solo generar si no hay demasiados obst�culos cerca
            if (ContarObstaculosCerca() < 3)
            {
                CrearObstaculo();
            }
        }
    }

    IEnumerator GenerarLatas()
    {
        while (!juegoTerminado)
        {
            yield return new WaitForSeconds(intervaloLatas);

            // Generar lata si no hay muchas activas
            if (latasActivas.Count < 5)
            {
                CrearLata();
            }
        }
    }

    IEnumerator IncrementarDificultad()
    {
        while (!juegoTerminado)
        {
            yield return new WaitForSeconds(tiempoParaIncremento);

            // Reducir intervalos (aumentar dificultad)
            intervaloObstaculos = Mathf.Max(intervaloMinimo, intervaloObstaculos - incrementoDificultad);
            intervaloLatas = Mathf.Max(intervaloMinimo * 0.5f, intervaloLatas - incrementoDificultad * 0.5f);

            Debug.Log($"Dificultad aumentada. Intervalo obst�culos: {intervaloObstaculos:F1}s");
        }
    }

    IEnumerator VerificarTiempoLatas()
    {
        while (!juegoTerminado)
        {
            yield return new WaitForSeconds(1f);

            // Verificar si ha pasado mucho tiempo sin recoger lata
            if (Time.time - tiempoUltimaLata > tiempoMaximoSinLata)
            {
                TerminarJuegoPorTiempo();
            }

            // Advertencia visual cuando queda poco tiempo
            float tiempoRestante = tiempoMaximoSinLata - (Time.time - tiempoUltimaLata);
            if (tiempoRestante <= 3f && tiempoRestante > 0f)
            {
                
                Debug.Log($"�CUIDADO! {tiempoRestante:F1} segundos para encontrar una lata!");
            }
        }
    }

    void CrearObstaculo()
    {
        Vector3 posicion = terreno.ObtenerPosicionParaObjeto(distanciaSpawn);

        // Verificar que la posici�n est� libre
        if (terreno.PosicionLibre(posicion))
        {
            // Seleccionar obst�culo aleatorio
            GameObject obstaculoElegido = obstaculos[Random.Range(0, obstaculos.Length)];
            GameObject nuevoObstaculo = Instantiate(obstaculoElegido, posicion,
                Quaternion.Euler(0, Random.Range(0, 360), 0));

            // Asegurar que tenga el tag correcto
            nuevoObstaculo.tag = "Obstaculo";

            obstaculosActivos.Add(nuevoObstaculo);
        }
    }

    void CrearLata()
    {
        // Buscar posici�n libre
        int intentos = 0;
        Vector3 posicion;

        do
        {
            posicion = terreno.ObtenerPosicionParaObjeto(Random.Range(distanciaMinima, distanciaSpawn));
            intentos++;
        } while (!terreno.PosicionLibre(posicion) && intentos < 10);

        if (intentos < 10)
        {
            GameObject nuevaLata = Instantiate(lataPrefab, posicion, Quaternion.identity);
            nuevaLata.tag = "Lata";

            // Agregar rotaci�n y movimiento a la lata para hacerla m�s visible
            StartCoroutine(AnimarLata(nuevaLata));

            latasActivas.Add(nuevaLata);
            latasConsecutivas++;

            // Si hay muchas latas consecutivas, pausar la generaci�n brevemente
            if (latasConsecutivas >= latasconsecutivasMax)
            {
                latasConsecutivas = 0;
                intervaloLatas += 1f; 
            }
        }
    }

    IEnumerator AnimarLata(GameObject lata)
    {
        Vector3 posicionInicial = lata.transform.position;
        float tiempo = 0f;

        while (lata != null)
        {
            tiempo += Time.deltaTime * 2f;

            // Rotaci�n continua
            lata.transform.Rotate(0, 90 * Time.deltaTime, 0);

            // Movimiento vertical suave
            float offsetY = Mathf.Sin(tiempo) * 0.3f;
            lata.transform.position = new Vector3(posicionInicial.x,
                posicionInicial.y + offsetY, posicionInicial.z);

            yield return null;
        }
    }

    int ContarObstaculosCerca()
    {
        int contador = 0;
        Vector3 posicionJugador = jugador.position;

        foreach (GameObject obstaculo in obstaculosActivos)
        {
            if (obstaculo != null)
            {
                float distancia = Vector3.Distance(obstaculo.transform.position, posicionJugador);
                if (distancia < distanciaSpawn)
                {
                    contador++;
                }
            }
        }

        return contador;
    }

    void LimpiarObjetosLejanos()
    {
        Vector3 posicionJugador = jugador.position;

        // Limpiar obst�culos lejanos
        for (int i = obstaculosActivos.Count - 1; i >= 0; i--)
        {
            if (obstaculosActivos[i] == null ||
                obstaculosActivos[i].transform.position.z < posicionJugador.z - 20f)
            {
                if (obstaculosActivos[i] != null)
                    Destroy(obstaculosActivos[i]);
                obstaculosActivos.RemoveAt(i);
            }
        }

        // Limpiar latas lejanas
        for (int i = latasActivas.Count - 1; i >= 0; i--)
        {
            if (latasActivas[i] == null ||
                latasActivas[i].transform.position.z < posicionJugador.z - 20f)
            {
                if (latasActivas[i] != null)
                    Destroy(latasActivas[i]);
                latasActivas.RemoveAt(i);
            }
        }
    }

    public void LataRecolectada()
    {
        tiempoUltimaLata = Time.time;
        latasConsecutivas = 0;
        Debug.Log("�Lata recolectada! Tiempo reiniciado.");
    }

    void TerminarJuegoPorTiempo()
    {
        if (!juegoTerminado)
        {
            juegoTerminado = true;
            Debug.Log("�GAME OVER! No recolectaste latas a tiempo.");

            if (gameManager != null)
            {
                gameManager.TerminarJuego();
            }
        }
    }

    public void DetenerGeneracion()
    {
        juegoTerminado = true;
    }

    // M�todo para obtener estad�sticas
    public int ObtenerObstaculosActivos()
    {
        return obstaculosActivos.Count;
    }

    public int ObtenerLatasActivas()
    {
        return latasActivas.Count;
    }

    public float ObtenerTiempoRestanteLata()
    {
        return tiempoMaximoSinLata - (Time.time - tiempoUltimaLata);
    }
}
