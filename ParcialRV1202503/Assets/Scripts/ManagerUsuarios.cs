using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System;

public class ManagerUsuarios : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_InputField campoNombre;
    public TMP_InputField campoEdad;
    public TMP_InputField campoCorreo;
    public TMP_InputField campoCiudad;
    public Button botonRegistrar;
    public GameObject panelRegistro;
    public GameObject panelJuego;

    [Header("Validación")]
    public TMP_Text textoValidacion;
    public Color colorError = Color.red;
    public Color colorExito = Color.green;

    private List<DatosUsuarios> usuariosRegistrados = new List<DatosUsuarios>();
    private DatosUsuarios usuarioActual;
    private string rutaArchivoJson;

    void Start()
    {
        rutaArchivoJson = Path.Combine(Application.persistentDataPath, "datos_usuarios.json");
        CargarDatosUsuarios();
        ConfigurarUI();
    }

    private void ConfigurarUI()
    {
        botonRegistrar.onClick.AddListener(OnBotonRegistrarClic);
        
        // Validación en tiempo real
        campoCorreo.onValueChanged.AddListener(ValidarCorreo);
        campoEdad.onValueChanged.AddListener(ValidarEdad);
    }

    public void OnBotonRegistrarClic()
    {
        if (ValidarTodosCampos())
        {
            RegistrarNuevoUsuario();
        }
    }

    private bool ValidarTodosCampos()
    {
        if (string.IsNullOrEmpty(campoNombre.text))
        {
            MostrarMensajeValidacion("Por favor ingresa tu nombre", colorError);
            return false;
        }

        if (!int.TryParse(campoEdad.text, out int edad) || edad < 5 || edad > 120)
        {
            MostrarMensajeValidacion("Edad debe ser entre 5 y 120 años", colorError);
            return false;
        }

        if (!EsCorreoValido(campoCorreo.text))
        {
            MostrarMensajeValidacion("Por favor ingresa un correo válido", colorError);
            return false;
        }

        if (string.IsNullOrEmpty(campoCiudad.text))
        {
            MostrarMensajeValidacion("Por favor ingresa tu ciudad", colorError);
            return false;
        }

        if (EsCorreoYaRegistrado(campoCorreo.text))
        {
            MostrarMensajeValidacion("Este correo ya está registrado", colorError);
            return false;
        }

        return true;
    }

    private void ValidarCorreo(string correo)
    {
        if (!string.IsNullOrEmpty(correo) && !EsCorreoValido(correo))
        {
            MostrarMensajeValidacion("Formato de correo inválido", colorError);
        }
        else
        {
            LimpiarMensajeValidacion();
        }
    }

    private void ValidarEdad(string textoEdad)
    {
        if (!string.IsNullOrEmpty(textoEdad) && !int.TryParse(textoEdad, out int edad))
        {
            MostrarMensajeValidacion("La edad debe ser un número", colorError);
        }
        else
        {
            LimpiarMensajeValidacion();
        }
    }

    private bool EsCorreoValido(string correo)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(correo);
            return addr.Address == correo;
        }
        catch
        {
            return false;
        }
    }

    private bool EsCorreoYaRegistrado(string correo)
    {
        return usuariosRegistrados.Exists(usuario => 
            usuario.correo.Equals(correo, StringComparison.OrdinalIgnoreCase));
    }

    private void RegistrarNuevoUsuario()
    {
        usuarioActual = new DatosUsuarios(
            campoNombre.text,
            int.Parse(campoEdad.text),
            campoCorreo.text,
            campoCiudad.text
        );

        usuariosRegistrados.Add(usuarioActual);
        GuardarDatosUsuarios();

        MostrarMensajeValidacion("¡Registro exitoso!", colorExito);
        
        // Ocultar panel de registro después de un breve tiempo
        Invoke("OcultarPanelRegistro", 1.5f);
    }

    private void OcultarPanelRegistro()
    {
        panelRegistro.SetActive(false);
        panelJuego.SetActive(true);
        LimpiarMensajeValidacion();
        
        Debug.Log($"Usuario registrado: {usuarioActual.nombre}");
    }

    public void ActualizarPuntajeUsuario(int puntaje)
    {
        if (usuarioActual != null)
        {
            if (puntaje > usuarioActual.puntajeMaximo)
            {
                usuarioActual.puntajeMaximo = puntaje;
            }
            GuardarDatosUsuarios();
        }
    }

    private void GuardarDatosUsuarios()
    {
        ListaDatosUsuarios listaUsuarios = new ListaDatosUsuarios { Usuarios = usuariosRegistrados.ToArray() };
        string json = JsonUtility.ToJson(listaUsuarios, true);
        File.WriteAllText(rutaArchivoJson, json);
        
        // También guardar como CSV
        ExportarACSV();
    }

    private void CargarDatosUsuarios()
    {
        if (File.Exists(rutaArchivoJson))
        {
            string json = File.ReadAllText(rutaArchivoJson);
            ListaDatosUsuarios listaUsuarios = JsonUtility.FromJson<ListaDatosUsuarios>(json);
            usuariosRegistrados = new List<DatosUsuarios>(listaUsuarios.Usuarios);
        }
    }

    public void ExportarACSV()
    {
        string rutaArchivoCSV = Path.Combine(Application.persistentDataPath, "datos_usuarios.csv");
        
        using (StreamWriter escritor = new StreamWriter(rutaArchivoCSV))
        {
            // Escribir encabezados
            escritor.WriteLine("Nombre,Edad,Correo,Ciudad,PuntajeMaximo");
            
            // Escribir datos
            foreach (var usuario in usuariosRegistrados)
            {
                escritor.WriteLine(
                    $"\"{usuario.nombre}\"," +
                    $"{usuario.edad},"+
                    $"\"{usuario.correo}\"," +
                    $"\"{usuario.ciudad}\"," +
                    $"{usuario.puntajeMaximo}"
                );
            }
        }
        
        Debug.Log($"Datos exportados a: {rutaArchivoCSV}");
    }

    private void MostrarMensajeValidacion(string mensaje, Color color)
    {
        textoValidacion.text = mensaje;
        textoValidacion.color = color;
        textoValidacion.gameObject.SetActive(true);
    }

    public void LimpiarMensajeValidacion()
    {
        textoValidacion.gameObject.SetActive(false);
    }

    // Para acceder desde otros scripts
    public DatosUsuarios ObtenerUsuarioActual()
    {
        return usuarioActual;
    }

    public List<DatosUsuarios> ObtenerTodosUsuarios()
    {
        return usuariosRegistrados;
    }

    // Método para cuando el juego termina
    public void AlTerminarJuego(int puntajeFinal)
    {
        ActualizarPuntajeUsuario(puntajeFinal);
        Debug.Log($"Juego terminado. Puntaje: {puntajeFinal}");
    }

    // Método para buscar usuario por correo
    public DatosUsuarios ObtenerUsuarioPorCorreo(string correo)
    {
        return usuariosRegistrados.Find(usuario => 
            usuario.correo.Equals(correo, StringComparison.OrdinalIgnoreCase));
    }

    // Método para obtener mejores puntajes
    public List<DatosUsuarios> ObtenerMejoresPuntajes(int cantidad = 10)
    {
        return usuariosRegistrados
            .OrderByDescending(usuario => usuario.puntajeMaximo)
            .Take(cantidad)
            .ToList();
    }
}
