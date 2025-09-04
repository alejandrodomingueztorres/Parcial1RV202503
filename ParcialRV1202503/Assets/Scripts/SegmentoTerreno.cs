using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentoTerreno : MonoBehaviour
{
    [Header("Configuración del Segmento")]
    public float longitudSegmento = 50f;
    public float anchoCarretera = 10f;

    [Header("Elementos del Segmento")]
    public GameObject sueloCarretera;
    public GameObject[] lineasCarril;
    public GameObject aceraIzquierda;
    public GameObject aceraDerecha;

    [Header("Puntos de Spawn")]
    public Transform[] puntosSpawnObstaculos; 
    public Transform[] puntosSpawnLatas; 

    [Header("Decoraciones")]
    public GameObject[] decoracionesLaterales; 

    void Start()
    {
        ConfigurarSegmento();
    }

    void ConfigurarSegmento()
    {
        
        if (sueloCarretera != null)
        {
            sueloCarretera.transform.localScale = new Vector3(anchoCarretera, 1, longitudSegmento);
            sueloCarretera.transform.localPosition = new Vector3(0, 0, longitudSegmento * 0.5f);
        }

       
        ConfigurarAceras();

        
        ConfigurarLineasCarril();

        
        ConfigurarPuntosSpawn();
    }

    void ConfigurarAceras()
    {
        float posicionAcera = anchoCarretera * 0.5f + 1f;

        if (aceraIzquierda != null)
        {
            aceraIzquierda.transform.localPosition = new Vector3(-posicionAcera, 0.25f, longitudSegmento * 0.5f);
            aceraIzquierda.transform.localScale = new Vector3(2f, 0.5f, longitudSegmento);
        }

        if (aceraDerecha != null)
        {
            aceraDerecha.transform.localPosition = new Vector3(posicionAcera, 0.25f, longitudSegmento * 0.5f);
            aceraDerecha.transform.localScale = new Vector3(2f, 0.5f, longitudSegmento);
        }
    }

    void ConfigurarLineasCarril()
    {
        if (lineasCarril == null) return;

        float espacioLineas = 4f; 
        int numeroLineas = Mathf.FloorToInt(longitudSegmento / espacioLineas);

        for (int i = 0; i < lineasCarril.Length && i < numeroLineas; i++)
        {
            if (lineasCarril[i] != null)
            {
                float posZ = i * espacioLineas + espacioLineas * 0.5f;
                lineasCarril[i].transform.localPosition = new Vector3(0, 0.01f, posZ);
                lineasCarril[i].transform.localScale = new Vector3(0.3f, 0.1f, 2f);
            }
        }
    }

    void ConfigurarPuntosSpawn()
    {
        

        foreach (Transform punto in puntosSpawnObstaculos)
        {
            if (punto != null)
            {
                // Asegurar que estén dentro del segmento
                Vector3 pos = punto.localPosition;
                pos.z = Mathf.Clamp(pos.z, 0, longitudSegmento);
                pos.x = Mathf.Clamp(pos.x, -3f, 3f); // Dentro de los carriles
                punto.localPosition = pos;
            }
        }
    }

    // Método para obtener posiciones aleatorias de spawn
    public Vector3 ObtenerPosicionSpawnAleatoria()
    {
        float x = Random.Range(-3f, 3f); 
        float z = Random.Range(5f, longitudSegmento - 5f); 
        return transform.TransformPoint(new Vector3(x, 0.5f, z));
    }

    // Verificar si una posición está libre en este segmento
    public bool PosicionLibre(Vector3 posicionMundo, float radio = 2f)
    {
        Vector3 posicionLocal = transform.InverseTransformPoint(posicionMundo);

        // Verificar si está dentro de este segmento
        if (posicionLocal.z < 0 || posicionLocal.z > longitudSegmento)
            return true; 

        // Buscar objetos cercanos
        Collider[] objetos = Physics.OverlapSphere(posicionMundo, radio);
        foreach (Collider obj in objetos)
        {
            if (obj.CompareTag("Obstaculo") || obj.CompareTag("Lata"))
                return false;
        }

        return true;
    }
}
