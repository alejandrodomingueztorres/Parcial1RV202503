using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ManagerUsuarios manejadorRegistro;
    public TMP_Text textoBienvenida;

    // Start is called before the first frame update
    void Start()
    {
        manejadorRegistro.campoCorreo.placeholder.GetComponent<TMP_Text>().text = "Correo de ejemplo";
        manejadorRegistro.campoCiudad.placeholder.GetComponent<TMP_Text>().text = "Ciudad";
    }

    public void Inicio()
    {
        var usuario = manejadorRegistro.ObtenerUsuarioActual();
        if (usuario != null)
        {
            textoBienvenida.text = $"!Bienvenido, {usuario.nombre}¡";
        }
    }

       public void MostrarPanelRegistro()
    {
        manejadorRegistro.panelRegistro.SetActive(true);
        LimpiarFormulario();
    }

      private void LimpiarFormulario()
    {
        manejadorRegistro.campoNombre.text = "";
        manejadorRegistro.campoEdad.text = "";
        manejadorRegistro.campoCorreo.text = "";
        manejadorRegistro.campoCiudad.text = "";
        manejadorRegistro.LimpiarMensajeValidacion();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
