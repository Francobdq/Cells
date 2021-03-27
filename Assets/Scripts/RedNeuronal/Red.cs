using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Neurona
{
    float[] w; // Peso
    float b; // bias

    float z; // z es el ultimo resultado de salida en crudo (la suma ponderada)    

    // Suma ponderada de la entrada por los pesos
    float Z(float[] x)
    { // suma ponderada
        float suma = b;
        for (int i = 0; i < w.Length; i++)
        {
            suma += x[i] * w[i];
        }

        z = suma;
        return z;

    }

    /*public void ObtieneDatos(ref float[] w, ref float b)
    {
        w = new float[this.w.Length];
        for (int i = 0; i < this.w.Length; i++)
        {
            w[i] = this.w[i];
        }
        b = this.b;
    }*/
    private float AleatorioMenosUnoUno()
    {
        return (Random.Range(0f,1f) * 2 - 1);
    }
    public void MutaNeurona(float probabilidadPesos)
    {

        for (int i = 0; i < w.Length; i++)
        {
            if (Random.Range(0f, 1f) < probabilidadPesos)
                w[i] += AleatorioMenosUnoUno()/2;//Random.Range(w[i] - mul, w[i] + mul);
        }
        if (Random.Range(0f, 1f) < probabilidadPesos)
            b += AleatorioMenosUnoUno()/2;//Random.Range(b - mul, b + mul);
        
    }

    // Salida de la Neurona
    public float Salida(float[] x)
    {
        return Z(x);
    }

    public float ObtenerBias()
    {
        return b;
    }
    public float[] ObtenerPesos()
    {
        return w;
    }

    public Neurona(int numeroDeEntradas, bool aleatorio, Neurona neu)
    {
        if (aleatorio || neu == null)
        {
            if (neu == null && !aleatorio)
                Debug.Log("Aleatorio es verdadero pero la neurona no existe.");
            //Random.InitState(seedRandom); // para que las entradas sean todas iguales
            b = AleatorioMenosUnoUno();
            w = new float[numeroDeEntradas];
            for (int i = 0; i < numeroDeEntradas; i++)
            {
                w[i] = AleatorioMenosUnoUno();
            }
        }
        else
        {
            w = new float[neu.w.Length];
            for(int i = 0; i < neu.w.Length; i++)
            {
                w[i] = neu.w[i];
            }
            
            b = neu.b;
        }
    }
}
[System.Serializable]
public class Capa
{
    [HideInInspector]public List<Neurona> neurons;
    [SerializeField] string funcionDeActivacion;
    [HideInInspector]public int numeroDeNeuronas;
    public float[] salida;
    
    FuncionesActivacion funcionesActivacion;

    

    // Capa
        public float[] ActivarCapa(float[] x)
        {

            List<float> salidas = new List<float>();
            for (int i = 0; i < numeroDeNeuronas; i++)
            {
                salidas.Add(funcionesActivacion.SeleccionaFuncionActivacion(neurons[i].Salida(x), funcionDeActivacion));
            }
            salida = salidas.ToArray();
            return salidas.ToArray();
        }
        public List<Neurona> ObtenerCopiaDeNeuronasCapa()
        {
            List<Neurona> aux = new List<Neurona>();
            for(int i = 0; i < neurons.Count; i++)
            {
                aux.Add(neurons[i]);
            }
            return aux;
        }
        public void ModificaCapa(float probabilidadPesos)
        {
            for (int i = 0; i < numeroDeNeuronas; i++)
            {
                neurons[i].MutaNeurona(probabilidadPesos);
            }
        }
        public Capa(int numeroDeNeuronas, int numeroDeEntradas, string funcionDeActivacion, FuncionesActivacion funcionesActivacion, bool aleatorio, Neurona[] neu)
        {
            this.numeroDeNeuronas = numeroDeNeuronas;
            neurons = new List<Neurona>();
            this.funcionesActivacion = funcionesActivacion;
            this.funcionDeActivacion = funcionDeActivacion;
            for (int i = 0; i < numeroDeNeuronas; i++)
            {
                neurons.Add(new Neurona(numeroDeEntradas, aleatorio, (neu == null) ? null : neu[i]));
            }
        }
}
[System.Serializable]
public class Red
{
    public string idCell;

    public string[] funcionDeActivacion;

    public List<Capa> Capas;
    public int[] neuronasPorCapa;
    public float[] entrada;

    FuncionesActivacion funcionesDeActivacionScript;

    private void CrearCapa(int[] neuronasPorCapa, FuncionesActivacion funcionesActivacion, bool aleatorio, List<Neurona[]> neu)
    {
        for (int i = 0; i < neuronasPorCapa.Length; i++)
        {
            Capas.Add(new Capa(neuronasPorCapa[i], i == 0 ? neuronasPorCapa[i] : neuronasPorCapa[i - 1], funcionDeActivacion[i], funcionesActivacion, aleatorio, (neu == null) ? null : neu[i]));
        }
    }
    public string[] DevuelveFuncionDeActivacion()
    {
        return funcionDeActivacion;
    }
    private void InicializarRed(int[] neuronasPorCapa, string[] funcionDeActivacion, string idCell, FuncionesActivacion funcionesActivacion, bool aleatorio, List<Neurona[]> neu = null)
    {
        Capas = new List<Capa>();
        this.idCell = idCell;
        //RandomParaContinuar = Random.Range(0, 10000);
        //Random.InitState(Random.Range(0, 10000));
        //int seed = Random.Range(0, 10000);
        CrearCapa(neuronasPorCapa, funcionesActivacion, aleatorio, neu);
        //Random.InitState(RandomParaContinuar);
    }
    public float[] Activar(float[] inputs)
    {
        float[] outputs = new float[0];
        entrada = inputs;
        for (int i = 0; i < Capas.Count; i++)
        {
            outputs = Capas[i].ActivarCapa(inputs);
            inputs = outputs;
        }
        return outputs;
    }


    public void ModificaRed(float probabilidadPesos)
    {
        for(int i = 0; i < Capas.Count; i++)
        {
            Capas[i].ModificaCapa(probabilidadPesos);
        }
    }
    
    public List<Neurona[]> ObtenerDatosRed(Red ACopiarDatos)//ref string idCell, ref string[]funcionDeActivacion, ref List<Capa> Capas, ref List<int> neuronasPorCapa, ref List<Neurona[]> neuronasAux)
    {
        ACopiarDatos.idCell = idCell;

        ACopiarDatos.funcionDeActivacion = this.funcionDeActivacion;


        List<Neurona[]> neuronasAux = new List<Neurona[]>();
        for (int i = 0; i < this.Capas.Count; i++)
        {
            neuronasAux.Add(this.Capas[i].ObtenerCopiaDeNeuronasCapa().ToArray());
        }

        //ACopiarDatos.neuronasPorCapa = new List<int>();

        for (int i = 0; i < this.neuronasPorCapa.Length; i++)
            ACopiarDatos.neuronasPorCapa[i] = this.neuronasPorCapa[i];

        return neuronasAux;
    }

    public void CopiarDatosRed(Red red)
    {
        List<Neurona[]> neuronasAux = red.ObtenerDatosRed(this);
        InicializarRed(neuronasPorCapa, funcionDeActivacion, idCell, funcionesDeActivacionScript, false, neuronasAux);
    }
    
    public Red(int[] neuronasPorCapa, string[] funcionDeActivacion, string idCell, FuncionesActivacion funcionesActivacion)
    {
        this.neuronasPorCapa = neuronasPorCapa;
        this.funcionDeActivacion = funcionDeActivacion;
        this.funcionesDeActivacionScript = funcionesActivacion;
        InicializarRed(neuronasPorCapa, funcionDeActivacion, idCell, funcionesActivacion, true);
    }
}

    
