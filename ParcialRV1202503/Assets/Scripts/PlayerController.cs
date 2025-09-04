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

    private Rigidbody rb;
    private Vector3 movimientoLateral;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocidadNormal = velocidadHaciaAdelante;

        
        rb.velocity = new Vector3(0, 0, velocidadHaciaAdelante);
    }

    void Update()
    {
        ManejarMovimientoLateral();
        MantenerVelocidadAdelante();
    }

    void ManejarMovimientoLateral()
    {
        
        float inputHorizontal = 0f;

        
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            inputHorizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            inputHorizontal = 1f;

        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = touch.deltaPosition.x;
                inputHorizontal = deltaX > 0 ? 1f : -1f;
            }
        }

        // Calcular nueva posición lateral
        Vector3 posicionActual = transform.position;
        float nuevaX = posicionActual.x + (inputHorizontal * velocidadLateral * Time.deltaTime);

        // Limitar movimiento lateral
        nuevaX = Mathf.Clamp(nuevaX, -limiteLateral, limiteLateral);

        // Aplicar movimiento lateral
        transform.position = new Vector3(nuevaX, posicionActual.y, posicionActual.z);

        // Rotación suave hacia la dirección del movimiento
        if (inputHorizontal != 0)
        {
            float rotacionY = inputHorizontal * 15f; // Inclinación suave
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(0, rotacionY, 0), Time.deltaTime * 5f);
        }
        else
        {
            // Volver a la rotación normal
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.identity, Time.deltaTime * 5f);
        }
    }

    void MantenerVelocidadAdelante()
    {
        // Asegurar que siempre se mueva hacia adelante
        Vector3 velocidadActual = rb.velocity;
        rb.velocity = new Vector3(velocidadActual.x, velocidadActual.y, velocidadHaciaAdelante);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstaculo"))
        {
            // Ralentizar al chocar con obstáculo
            RalentizarPerro();
        }
        else if (other.CompareTag("Lata"))
        {
            // Recolectar lata
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
        velocidadHaciaAdelante *= 0.5f; // Reducir velocidad a la mitad

        //// Cambiar color o efecto visual (opcional)
        //GetComponent<Renderer>().color = Color.red;

        yield return new WaitForSeconds(duracionRalentizacion);

        // Restaurar velocidad normal
        velocidadHaciaAdelante = velocidadNormal;
        estaRalentizado = false;

        //// Restaurar color normal
        //GetComponent<Renderer>().color = Color.white;
    }

    void RecolectarLata(GameObject lata)
    {
        // Añadir puntos
        if (sistemaPuntos != null)
        {
            // Detener el incremento automático de puntos temporalmente
            //sistemaPuntos.RecolectarLata();
        }

        if (gameManager != null)
        {
            gameManager.AgregarPuntos(10); // 10 puntos por lata
        }

        // Destruir la lata con efecto
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
}
