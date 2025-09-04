using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public ManagerUsuarios manejadorRegistro;
    private int puntajeActual = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (manejadorRegistro.ObtenerUsuarioActual() == null)
        {
            manejadorRegistro.panelRegistro.SetActive(true);
        }
        else
        {
            IniciarJuego();
        }
    }

    public void IniciarJuego()
    {
        puntajeActual = 0;
        Debug.Log("Juego iniciado");
    }

     public void AgregarPuntos(int puntos)
    {
        puntajeActual += puntos;
        Debug.Log($"Puntos añadidos: {puntos}. Puntaje actual: {puntajeActual}");
    }

     public void TerminarJuego()
    {
        // Guardar puntaje final
        manejadorRegistro.AlTerminarJuego(puntajeActual);
        
        // Mostrar pantalla de game over
        Debug.Log($"Juego terminado. Puntaje final: {puntajeActual}");
        
        // Exportar datos automáticamente
        manejadorRegistro.ExportarACSV();
    }
    public void ExportarDatos()
    {
        manejadorRegistro.ExportarACSV();
    }

    // Método para mostrar mejores puntajes
    public void MostrarMejoresPuntajes()
    {
        var mejoresPuntajes = manejadorRegistro.ObtenerMejoresPuntajes(5);
        Debug.Log("=== MEJORES 5 PUNTAJES ===");
        
        for (int i = 0; i < mejoresPuntajes.Count; i++)
        {
            var usuario = mejoresPuntajes[i];
            Debug.Log($"{i + 1}. {usuario.nombre} - {usuario.puntajeMaximo} pts");
        }
    }

}
