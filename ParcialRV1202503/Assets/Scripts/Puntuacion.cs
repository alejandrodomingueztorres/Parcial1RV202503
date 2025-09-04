using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puntuacion : MonoBehaviour
{


    private int puntos = 0;
    public Text t;

    //public float speed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        puntos++;
        print(puntos);
        t.text = puntos.ToString();

    }
    


}
