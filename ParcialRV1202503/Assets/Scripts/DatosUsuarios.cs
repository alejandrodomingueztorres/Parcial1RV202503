using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DatosUsuarios : MonoBehaviour
{

    public string nombre;
    public int edad;
    public string correo;
    public string ciudad;
    public int puntajeMaximo;
    public DatosUsuarios(string name, int age, string email, string city)
    {
        this.nombre  = name;
        this.edad  = age;
        this.correo  = email;
        this.ciudad  = city;
        this.puntajeMaximo  = 0;
    }


}

 [System.Serializable]
public class ListaDatosUsuarios
{
    public DatosUsuarios[] Usuarios;
}
