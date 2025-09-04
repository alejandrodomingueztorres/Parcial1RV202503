using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadLateral = 10f;
    public float velocidadHaciaAdelante = 15f;
    public float limiteLateral = 4f; // Límites izquierda/derecha

    [Header("Animación y Efectos")]
    public Animator animator;
    public float duracionRalentizacion = 2f;
    private bool estaRalentizado = false;
    private float velocidadNormal;

    [Header("Referencias")]
    public GameManager gameManager;
    public Puntuacion sistemaPuntos;

    [Header("Control de Colisiones")]
    public bool usarRigidbody = false; // Toggle para usar o no Rigidbody
    public float fuerzaRebote = 5f; // Fuerza de rebote controlada

    private Rigidbody rb;
    private Vector3 movimientoLateral;
    private bool controlesHabilitados = true; // Para controlar cuándo el jugador puede moverse

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocidadNormal = velocidadHaciaAdelante;

        if (usarRigidbody && rb != null)
        {
            // Configurar Rigidbody para mayor control
            rb.freezeRotation = true; // Evita rotaciones no deseadas
            rb.drag = 2f; // Añade resistencia para control
            rb.velocity = new Vector3(0, 0, velocidadHaciaAdelante);
        }
    }

    void Update()
    {
        if (controlesHabilitados)
        {
            ManejarMovimientoLateral();
        }

        if (usarRigidbody)
        {
            MantenerVelocidadAdelanteRigidbody();
        }
        else
        {
            MantenerVelocidadAdelanteSinFisicas();
        }
    }

    void ManejarMovimientoLateral()
    {
        float inputHorizontal = 0f;

        // Input de teclado
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            inputHorizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            inputHorizontal = 1f;

        // Input táctil mejorado
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // Usar la posición del toque en lugar del delta para mejor control
                float touchX = (touch.position.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
                inputHorizontal = Mathf.Clamp(touchX, -1f, 1f);
            }
        }

        if (usarRigidbody && rb != null)
        {
            // Movimiento con Rigidbody controlado
            MoverConRigidbody(inputHorizontal);
        }
        else
        {
            // Movimiento directo con Transform (más predecible)
            MoverConTransform(inputHorizontal);
        }

        // Rotación suave hacia la dirección del movimiento
        AplicarRotacion(inputHorizontal);
    }

    void MoverConTransform(float inputHorizontal)
    {
        Vector3 posicionActual = transform.position;
        float nuevaX = posicionActual.x + (inputHorizontal * velocidadLateral * Time.deltaTime);
        nuevaX = Mathf.Clamp(nuevaX, -limiteLateral, limiteLateral);

        // Movimiento hacia adelante constante + movimiento lateral
        Vector3 movimientoAdelante = Vector3.forward * velocidadHaciaAdelante * Time.deltaTime;
        Vector3 nuevaPosicion = new Vector3(nuevaX, posicionActual.y, posicionActual.z) + movimientoAdelante;

        transform.position = nuevaPosicion;
    }

    void MoverConRigidbody(float inputHorizontal)
    {
        Vector3 posicionActual = transform.position;
        float nuevaX = posicionActual.x + (inputHorizontal * velocidadLateral * Time.deltaTime);
        nuevaX = Mathf.Clamp(nuevaX, -limiteLateral, limiteLateral);

        // Aplicar movimiento lateral directamente a la posición para evitar físicas locas
        transform.position = new Vector3(nuevaX, posicionActual.y, posicionActual.z);
    }

    void AplicarRotacion(float inputHorizontal)
    {
        if (inputHorizontal != 0)
        {
            float rotacionY = inputHorizontal * 15f;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(0, rotacionY, 0), Time.deltaTime * 5f);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.identity, Time.deltaTime * 5f);
        }
    }

    void MantenerVelocidadAdelanteRigidbody()
    {
        if (rb != null)
        {
            // Mantener solo la velocidad Z (hacia adelante), preservar Y para gravedad
            Vector3 velocidadActual = rb.velocity;
            rb.velocity = new Vector3(0, velocidadActual.y, velocidadHaciaAdelante);
        }
    }

    void MantenerVelocidadAdelanteSinFisicas()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstaculo"))
        {
            
            RalentizarPerro();

            
            // StartCoroutine(EfectoReboteControlado());
        }
        else if (other.CompareTag("Lata"))
        {
            RecolectarLata(other.gameObject);
        }
    }

    void RalentizarPerro()
    {
        if (!estaRalentizado)
        {
            StartCoroutine(EfectoRalentizacion());
        }
    }

    IEnumerator EfectoRalentizacion()
    {
        estaRalentizado = true;
        velocidadHaciaAdelante *= 0.5f;

        
        // GetComponent<Renderer>().color = Color.red;

        yield return new WaitForSeconds(duracionRalentizacion);

        velocidadHaciaAdelante = velocidadNormal;
        estaRalentizado = false;

        // GetComponent<Renderer>().color = Color.white;
    }

    
    IEnumerator EfectoReboteControlado()
    {
        controlesHabilitados = false; // Deshabilitar controles temporalmente

        float tiempoRebote = 0.3f;
        float fuerzaReboteActual = fuerzaRebote;

        for (float t = 0; t < tiempoRebote; t += Time.deltaTime)
        {
            float progreso = t / tiempoRebote;
            float reboteY = Mathf.Sin(progreso * Mathf.PI) * fuerzaReboteActual;

            // Aplicar un pequeño rebote vertical muy controlado
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y + reboteY * Time.deltaTime, pos.z);

            yield return null;
        }

        controlesHabilitados = true; 
    }

    void RecolectarLata(GameObject lata)
    {
        if (sistemaPuntos != null)
        {
            // sistemaPuntos.RecolectarLata();
        }

        if (gameManager != null)
        {
            gameManager.AgregarPuntos(10);
        }

        StartCoroutine(EfectoRecoleccion(lata));
    }

    IEnumerator EfectoRecoleccion(GameObject lata)
    {
        Vector3 posicionInicial = lata.transform.position;
        float tiempo = 0f;
        float duracion = 0.5f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;

            lata.transform.position = posicionInicial + Vector3.up * progreso * 2f;
            lata.transform.localScale = Vector3.one * (1f - progreso);

            yield return null;
        }

        Destroy(lata);
    }

   
    public void ResetearPosicion()
    {
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;
        controlesHabilitados = true;

        if (rb != null)
        {
            rb.velocity = new Vector3(0, 0, velocidadHaciaAdelante);
            rb.angularVelocity = Vector3.zero;
        }
    }
}
