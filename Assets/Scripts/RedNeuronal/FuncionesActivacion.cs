using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FuncionesActivacion
{

    // funcion Tangente hiperbolica

    public float TanH(float x)
    {
        if (x > 4)
            return 1;
        if (x < -4)
            return -1;
        return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x)); // devuelve valores entre -1 y 1
    }
    // funcion Sigmoide 
    public float Sigmoide(float x)
    {
        if (x > 4)
            return 1;
        if (x < -4)
            return 0;
        return 1 / (1 + Mathf.Exp(-x)); // devuelve valores entre 0 y 1
    }
    public float Relu(float x)
    {
        if (x <= 0)
            return 0;
        else
            return x;
    }

    public float LeakyRelu(float x) {
        if (x <= 0)
            return x*0.01f;
        else
            return x;
    }

    public float SeleccionaFuncionActivacion(float salidaNeurona, string funcionDeActivacion)
    {
        switch (funcionDeActivacion)
        {
            case "":
                return salidaNeurona;
            case "Sigmoide":
                return Sigmoide(salidaNeurona);
            case "TanH":
                return TanH(salidaNeurona);
            case "Relu":
                return Relu(salidaNeurona);
            case "LeakyRelu":
                return LeakyRelu(salidaNeurona);

            default:
                Debug.Log("ERROR. ENTRA EN CASO DEFAULT EN RED NEURONAL");
                return salidaNeurona;
        }
    }
}
