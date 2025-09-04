using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Configuraci�n de Posici�n")]
    public Vector3 offset = new Vector3(0, 5, -8); // Posici�n relativa al jugador
    public float altura = 5f; // Altura fija de la c�mara
    public float distancia = 8f; // Distancia atr�s del jugador

    [Header("Suavizado")]
    public float suavidadSeguimiento = 5f; 
    public float suavidadRotacion = 3f; 

    [Header("L�mites")]
    public float limiteMovimientoLateral = 3f; 
    public bool seguirMovimientoLateral = true;
    public bool mantenerAlturaFija = true;

    [Header("Estabilizaci�n")]
    public bool estabilizacionAutomatica = true;
    public float fuerzaEstabilizacion = 10f;

    private Vector3 posicionDeseada;
    private Vector3 velocidadSuavizado;

    void Start()
    {
        if (target == null)
        {
            
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
                target = jugador.transform;
        }

        // Configurar posici�n inicial
        if (target != null)
        {
            ConfigurarPosicionInicial();
        }
    }

    void ConfigurarPosicionInicial()
    {
        Vector3 posicionInicial = target.position + offset;
        transform.position = posicionInicial;
        transform.LookAt(target);
    }

    void LateUpdate()
    {
        if (target == null) return;

        CalcularPosicionDeseada();
        ActualizarPosicionCamara();
        ActualizarRotacionCamara();

        if (estabilizacionAutomatica)
        {
            EstabilizarCamara();
        }
    }

    void CalcularPosicionDeseada()
    {
        Vector3 posicionTarget = target.position;

        // Calcular posici�n base
        posicionDeseada = new Vector3(
            seguirMovimientoLateral ?
                Mathf.Clamp(posicionTarget.x, -limiteMovimientoLateral, limiteMovimientoLateral) :
                0f,
            mantenerAlturaFija ? altura : posicionTarget.y + offset.y,
            posicionTarget.z + offset.z
        );
    }

    void ActualizarPosicionCamara()
    {
       
        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicionDeseada,
            ref velocidadSuavizado,
            1f / suavidadSeguimiento
        );
    }

    void ActualizarRotacionCamara()
    {
        // Calcular rotaci�n hacia el target
        Vector3 direccionMirada = target.position - transform.position;
        Quaternion rotacionDeseada = Quaternion.LookRotation(direccionMirada);

        // Suavizar la rotaci�n
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotacionDeseada,
            Time.deltaTime * suavidadRotacion
        );
    }

    void EstabilizarCamara()
    {
        // Corregir cualquier rotaci�n no deseada en los ejes X y Z
        Vector3 eulerAngles = transform.eulerAngles;

        // Mantener una inclinaci�n suave hacia abajo pero estable
        float anguloXDeseado = 15f; // Mirar ligeramente hacia abajo

        // Suavizar los �ngulos hacia valores estables
        eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, anguloXDeseado,
            Time.deltaTime * fuerzaEstabilizacion);
        eulerAngles.z = Mathf.LerpAngle(eulerAngles.z, 0f,
            Time.deltaTime * fuerzaEstabilizacion);

        transform.eulerAngles = eulerAngles;
    }

    
    public void SacudirCamara(float intensidad = 0.5f, float duracion = 0.2f)
    {
        StartCoroutine(EfectoSacudida(intensidad, duracion));
    }

    IEnumerator EfectoSacudida(float intensidad, float duracion)
    {
        Vector3 posicionOriginal = transform.localPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            float x = Random.Range(-1f, 1f) * intensidad;
            float y = Random.Range(-1f, 1f) * intensidad;

            transform.localPosition = posicionOriginal + new Vector3(x, y, 0);

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = posicionOriginal;
    }

   
    public void CambiarTarget(Transform nuevoTarget)
    {
        target = nuevoTarget;
        if (target != null)
        {
            ConfigurarPosicionInicial();
        }
    }

    // M�todo para resetear posici�n de c�mara
    public void ResetearPosicion()
    {
        if (target != null)
        {
            ConfigurarPosicionInicial();
        }
    }

    // Visualizaci�n en el editor
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Mostrar la posici�n deseada
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position + offset, 0.5f);

            // Mostrar l�nea de visi�n
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
