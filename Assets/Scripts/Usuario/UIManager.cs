using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public RectTransform[] NeuronasC1;
    public RectTransform[] NeuronasC2;
    public RectTransform[] NeuronasC3;

    
    public GameObject datosCell;
    public Text idCelulaText;
    public Text edad;
    public Slider velocidad;
    public Text velMax;
    public Text velFloat;
    public Slider comida;
    public Text comidaMax;
    public Text comidaFloat;


    bool datosCellActivo = false;
    float velMaxF;
    float comidaMaxF;

    public Cell celulaAVer;
    




    public Red redNeu;

    GameController gameController;
    GeneraCell generaCell;
    public float diferenciaEntreVelocidades = 0.25f;

    private void Start()
    {  
        datosCell.SetActive(false);
        gameController = GetComponent<GameController>();
        generaCell = GetComponent<GeneraCell>();
        Time.timeScale = gameController.ObtenerVelocidad();
    }
    private void ActualizaJuego()
    {
        
        List<Cell> lista = generaCell.ObtenerLista();
        if (lista.Count > 0) {
            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].ActualizarVelJuego();
            }
        }
        
    }

    public void DiferenciaEntreVel(float value)
    {
        diferenciaEntreVelocidades = value;
    }
    public void AumentarVelocidadJuego()
    {
        if(gameController == null)
            gameController = GetComponent<GameController>();

        Time.timeScale = (gameController.ObtenerVelocidad() + diferenciaEntreVelocidades);
        gameController.ModificarVelocidad(gameController.ObtenerVelocidad() + diferenciaEntreVelocidades);
        //ActualizaJuego();
    }
    public void DisminuirVelocidadJuego()
    {
        if (gameController == null)
            gameController = GetComponent<GameController>();

        Time.timeScale = (gameController.ObtenerVelocidad() - diferenciaEntreVelocidades);
        gameController.ModificarVelocidad(gameController.ObtenerVelocidad() - diferenciaEntreVelocidades);
        //ActualizaJuego();*/
    }



    public void MostrarOcutarAccionesCell()
    {
        List<Cell> lista = generaCell.ObtenerLista();
        if(lista != null)
        {
            gameController.mostrarAccion = !gameController.mostrarAccion;
            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].CambiarMuestraAccion();
            }
        }
            
    }

    public void MostrarOcultarDatosCell()
    {
        datosCell.SetActive(!datosCellActivo);
        datosCellActivo = !datosCellActivo;

        if (datosCellActivo)
        {
            StopCoroutine(FixedTrucho());
        }
        else
        {
            ActualizarDatosBasicos();
            StartCoroutine(FixedTrucho());
        }
            
    }
    /*private void DibujarLineas()
    {
        for(int j = 0; j < 2; j++)
        {
            List<float[]> capaAux = redNeu.ObtenerCapa(j);
            //Color colorADibujar;
            for (int i = 0; i < capaAux.Count; i++)
            {
                for (int k = 0; k < capaAux[i].Length; k++)
                {
                    if (capaAux[j][k] == 0) // neurona y pesos
                    {
                        Debug.DrawLine(NeuronasC1[k].position, NeuronasC2[j].position);
                    }
                }
            }
        }
        
        
    }
    private void Update()
    {
        DibujarLineas();
    }*/
    public void ActualizarDatosBasicos()
    {
        if (celulaAVer == null)
            return;
        string id = "";
        float velMax = 0;
        float diametro = 0;
        float comidaMax = 0;
        Color color = Color.white;
        celulaAVer.DevuelveValores(ref id,ref velMax,ref diametro,ref comidaMax,ref color);

        idCelulaText.text = id;
        this.velMax.text = velMax.ToString();
        velMaxF = velMax;
        this.comidaMax.text = comidaMax.ToString();
        comidaMaxF = comidaMax;
    }
    IEnumerator FixedTrucho()
    {
        //while (true)
        //{
            yield return new WaitForSeconds(0.1f);
            
        //}
    }
    private void FixedUpdate()
    {
        if (celulaAVer != null)
        {
            edad.text = celulaAVer.edad.ToString();
            float[] red = celulaAVer.DevuelveDatosRed();
            velocidad.value = red[4] / velMaxF; // la salida 4 es la velocidad
                                                //velFloat.text = red[4].ToString();
            comida.value = celulaAVer.comida / 100;
            //comidaFloat.text = celulaAVer.comida.ToString();
        }
        else
        {
            idCelulaText.text = "CelulaNula";
        }
    }
}
