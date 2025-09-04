using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteRunner : MonoBehaviour
{
    [Header("Configuraci�n del Terreno")]
    public GameObject[] segmentos; // Array de segmentos de terreno prefabricados
    public Transform jugador;
    public int segmentosVisibles = 5; // Cu�ntos segmentos mantener cargados
    public float longitudSegmento = 50f; // Longitud de cada segmento

    [Header("Objetos del Entorno")]
    public GameObject[] edificios; // Edificios para los lados
    public GameObject[] decoraciones; // �rboles, postes, etc.
    public float distanciaLateral = 8f; // Distancia desde el centro para colocar objetos

    private Queue<GameObject> segmentosActivos = new Queue<GameObject>();
    private Queue<GameObject> edificiosActivos = new Queue<GameObject>();
    private float posicionSiguienteSegmento = 0f;
    private int contadorSegmentos = 0;

    void Start()
    {
        // Crear segmentos iniciales
        for (int i = 0; i < segmentosVisibles; i++)
        {
            CrearSegmento();
        }

        // Crear edificios iniciales en los lados
        CrearEdificiosLaterales();
    }

    void Update()
    {
        // Verificar si necesitamos crear un nuevo segmento
        if (jugador.position.z > posicionSiguienteSegmento - (longitudSegmento * segmentosVisibles))
        {
            CrearSegmento();
            EliminarSegmentoAntiguo();
        }

        // Actualizar edificios laterales
        ActualizarEdificiosLaterales();
    }

    void CrearSegmento()
    {
        // Seleccionar un segmento aleatorio
        GameObject segmentoElegido = segmentos[Random.Range(0, segmentos.Length)];

        // Instanciar el segmento en la posici�n correcta
        Vector3 posicion = new Vector3(0, 0, posicionSiguienteSegmento);
        GameObject nuevoSegmento = Instantiate(segmentoElegido, posicion, Quaternion.identity);

        // A�adir el segmento a la cola
        segmentosActivos.Enqueue(nuevoSegmento);

        // Actualizar posici�n para el siguiente segmento
        posicionSiguienteSegmento += longitudSegmento;
        contadorSegmentos++;

        // Agregar decoraciones al segmento
        AgregarDecoraciones(nuevoSegmento, posicion);
    }

    void EliminarSegmentoAntiguo()
    {
        if (segmentosActivos.Count > segmentosVisibles)
        {
            GameObject segmentoAntiguo = segmentosActivos.Dequeue();
            Destroy(segmentoAntiguo);
        }
    }

    void AgregarDecoraciones(GameObject segmento, Vector3 posicionBase)
    {
        // A�adir decoraciones aleatorias a lo largo del segmento
        int cantidadDecoraciones = Random.Range(3, 8);

        for (int i = 0; i < cantidadDecoraciones; i++)
        {
            // Posici�n aleatoria en el segmento
            float z = Random.Range(posicionBase.z, posicionBase.z + longitudSegmento);
            float x = Random.Range(-distanciaLateral, distanciaLateral);

            // Evitar colocar decoraciones en el camino central
            if (Mathf.Abs(x) < 2f) continue;

            Vector3 posicionDecoracion = new Vector3(x, posicionBase.y, z);

            // Seleccionar decoraci�n aleatoria
            GameObject decoracion = decoraciones[Random.Range(0, decoraciones.Length)];
            GameObject nuevaDecoracion = Instantiate(decoracion, posicionDecoracion,
                Quaternion.Euler(0, Random.Range(0, 360), 0));

            // Hacer la decoraci�n hija del segmento para que se destruya junto con �l
            nuevaDecoracion.transform.SetParent(segmento.transform);
        }
    }

    void CrearEdificiosLaterales()
    {
        // Crear edificios en ambos lados del camino
        for (int i = 0; i < segmentosVisibles * 2; i++)
        {
            CrearEdificioLateral(i * longitudSegmento * 0.5f);
        }
    }

    void CrearEdificioLateral(float posicionZ)
    {
        // Edificio del lado izquierdo
        Vector3 posicionIzquierda = new Vector3(-distanciaLateral * 1.5f, 0, posicionZ);
        GameObject edificioIzquierdo = Instantiate(edificios[Random.Range(0, edificios.Length)],
            posicionIzquierda, Quaternion.identity);
        edificiosActivos.Enqueue(edificioIzquierdo);

        // Edificio del lado derecho
        Vector3 posicionDerecha = new Vector3(distanciaLateral * 1.5f, 0, posicionZ);
        GameObject edificioDerecho = Instantiate(edificios[Random.Range(0, edificios.Length)],
            posicionDerecha, Quaternion.identity);
        edificiosActivos.Enqueue(edificioDerecho);
    }

    void ActualizarEdificiosLaterales()
    {
        // Si el jugador ha avanzado lo suficiente, crear nuevos edificios
        if (jugador.position.z > posicionSiguienteSegmento - (longitudSegmento * segmentosVisibles * 1.5f))
        {
            CrearEdificioLateral(posicionSiguienteSegmento + longitudSegmento);

            // Eliminar edificios antiguos
            if (edificiosActivos.Count > segmentosVisibles * 4)
            {
                Destroy(edificiosActivos.Dequeue());
                Destroy(edificiosActivos.Dequeue());
            }
        }
    }

    // M�todo para obtener la posici�n donde generar obst�culos/latas
    public Vector3 ObtenerPosicionParaObjeto(float distanciaAdelante)
    {
        float z = jugador.position.z + distanciaAdelante;
        float x = Random.Range(-3f, 3f); // Dentro del camino principal
        return new Vector3(x, 0.5f, z);
    }

    // M�todo para verificar si una posici�n est� libre de obst�culos
    public bool PosicionLibre(Vector3 posicion, float radio = 2f)
    {
        Collider[] objetos = Physics.OverlapSphere(posicion, radio);
        foreach (Collider obj in objetos)
        {
            if (obj.CompareTag("Obstaculo") || obj.CompareTag("Lata"))
                return false;
        }
        return true;
    }
}
