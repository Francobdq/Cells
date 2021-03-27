using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour
{
    [HideInInspector] public float[,] mapa;
    public bool AutoActualizar = false;
    public bool Estacion = false;
    public Vector2 entreValores; // entre que valores cambia la velocidad de produccion de comida de estacion (ejemplo, el punto maximo seria 0.3f y el minimo 1 por ejem
    [SerializeField] int year = 0;
    [SerializeField] int RecordTotal = 10;
    [SerializeField] int RecordActual = 0;
    [SerializeField] string IdRecordTotal = "";
    [SerializeField] string IdRecordActual = "";
    //[SerializeField] string especieMasCantidad = "";
    //[SerializeField] int cantidad = 0;
    public int cantCelulasVivas = 0;
    public int celulasTotales = 0;
    public int yearUltimaGeneracion = 0;
    public int cantCelulasMinimas;
    public float comidaMaximaTile = 1000;
    [SerializeField] float generacionDeComida = 1;
    [SerializeField] float generacionComidaTiempo = 0.3f;
    [SerializeField] float temperatura;
    [SerializeField] string generarDeMemoria = "";
    [SerializeField] float velocidad = 1;
    public int radiacion = 1;

    int guardaCelulasMinimasIniciales;
    int contadorAutoActualizar = 0;
    int contadorEstacion = 0;

    bool subiendoTemp = true; // si sube la temperatura baja la cantidad de tiempo de generacion de comida


    [SerializeField]List<int> yearsDeExtincion;


    BoardManager board;
    Cell cellRecord;
    Cell cellRecordActual;
    GeneraCell generaCell;

    bool empezo = false;
    bool generando = false;

    [HideInInspector] public bool mostrarAccion = true;

    readonly int filas = 150;
    readonly int columnas = 150;

    readonly int bordesAgua = 4;

    public bool actualizarColores = false;
    bool actualizando = false;
    void Awake()
    {
        Debug.Log("Guardar datos Aqu�: " + Application.persistentDataPath);
        year = 0;
        guardaCelulasMinimasIniciales = cantCelulasMinimas;
        yearsDeExtincion = new List<int>();
        board = GetComponent<BoardManager>();
        generaCell = GetComponent<GeneraCell>();
        int seedControl = Random.Range(0, 10000);
        board.SetupScene(10,ref mapa, columnas, filas, bordesAgua);
        //Random.InitState(seedControl);

        GameObject[] celula = GameObject.FindGameObjectsWithTag("Cell");
        for(int i = 0; i < celula.Length; i++)
        {
            celula[i].GetComponent<Cell>().IniciarCiclo();
        }
        RecorreMapaSumando(100);
        StartCoroutine(GenerarCelulas(cantCelulasMinimas));
        StartCoroutine(ComidaMapa());
        StartCoroutine(AutoGuardar());
        StartCoroutine(Year());
        empezo = true;
    }
    public void ModificarVelocidad(float Mod)
    {
        velocidad = Mod;
    }
    public float ObtenerVelocidad()
    {
        return velocidad;
    }
    public int ObtenerColumnas()
    {
        return columnas;
    }
    public int ObtenerFilas()
    {
        return filas;
    }
                                
    private void RecorreMapaSumando(float cantComidaSumandaTile)
    {
        for (int i = bordesAgua +1; i < columnas- bordesAgua-1; i++)
        {
            for (int j = bordesAgua +1; j < filas- bordesAgua-1; j++)
            {
                if(mapa[i,j] > 0) 
                {
                    mapa[i, j] += cantComidaSumandaTile;
                    if (mapa[i, j] > comidaMaximaTile)
                        mapa[i, j] = comidaMaximaTile;
                }
                
                
            }
        }
    }
    private void AutoActualizarGeneracion()
    {
        contadorAutoActualizar++;
        if (cantCelulasVivas == 0)
        {
            yearsDeExtincion.Add(year);
            year = 0;
            cantCelulasMinimas = guardaCelulasMinimasIniciales;
        }
        if (contadorAutoActualizar >= 100)
        {
            contadorAutoActualizar = 0;
            cantCelulasMinimas /= 2;
        }

        
    }
    private void ActualizarEstacion()
    {
        if (entreValores.x == 0 && entreValores.y == 0)
            Estacion = false;

        contadorEstacion++;
        if (contadorEstacion >= 5)
        {
            contadorEstacion = 0;
            if (subiendoTemp)
            {
                generacionComidaTiempo += 0.05f;
                if (generacionComidaTiempo >= entreValores.y)
                {
                    generacionComidaTiempo = entreValores.y;
                    subiendoTemp = false;
                }

            }
            else
            {
                generacionComidaTiempo -= 0.05f;
                if (generacionComidaTiempo <= entreValores.x)
                {
                    generacionComidaTiempo = entreValores.x;
                    subiendoTemp = true;
                }
            }
        }
    }
    private IEnumerator Year()
    {
        while (true)
        {
            ActualizarTemperatura();
            yield return new WaitForSeconds(5f / velocidad);
            year++;
            if (AutoActualizar)
            {
                AutoActualizarGeneracion();
            }
            if (Estacion)
            {
                ActualizarEstacion();
            }

                
        }
    }

    private void ActualizarTemperatura() {
        float diferencia = entreValores.y - entreValores.x;

        temperatura = (generacionComidaTiempo-entreValores.x) / diferencia;
    }

    public float Temperatura() {
        return temperatura; // la temperatura es la representacion de la generacionComidaTiempo normalziado entre 1 y 0
    }
    public void GuardarRed(Red red, string finalRuta) => SaveLoad.Save(red, finalRuta);

    IEnumerator AutoGuardar()
    {
        while (true)
        {
            yield return new WaitForSeconds(600f / velocidad); // guarda cada 10 min
            if (cellRecord != null)
            {
                generaCell.RecorrerListaBuscandoRecord();
                GuardarRed(cellRecord.redNeuronal, ("/" + cellRecord.id  + ".bdq"));
            }
            else
                Debug.Log("Cell es nula y no se puede guardar");
        }
    }

    private IEnumerator ComidaMapa()
    {
        while (true)
        {

            yield return new WaitForSeconds(generacionComidaTiempo / velocidad); // la generacion de comida por tiempo ya se actualiza en base a la temperatura
            RecorreMapaSumando(generacionDeComida);
        }
    }
    /*public void EspecieMasCantidad(string nombreEspecie, int cantidad)
    {
        especieMasCantidad = nombreEspecie;
        this.cantidad = cantidad;
    }*/
    public void RecordTot(Cell cell)
    {
        cellRecord = cell;
        RecordTotal = cell.edad;
        IdRecordTotal = cell.id;
    }
    public void RecordAct(Cell cell)
    {
        cellRecordActual = cell;
        RecordActual = cell.edad;
        IdRecordActual = cell.id;
    }
    public int ObtenerRecordActual()
    {
        return RecordActual;
    }
    public int ObtenerRecordTotal()
    {
        return RecordTotal;
    }
    public int ObtenerYear()
    {
        return year;
    }
    public IEnumerator GenerarCelulas(int cantidad)
    {
        
        generando = true;
        GeneraCell genCell = GetComponent<GeneraCell>();
        for (int i = 0; i < cantidad; i++)
        {
            float posX;
            float posY;
            do
            {
                posX = Mathf.Round(Random.Range(bordesAgua + 1, columnas - bordesAgua - 1));
                posY = Mathf.Round(Random.Range(bordesAgua + 1, filas - bordesAgua - 1));
            } while (mapa[(int)posX,(int)posY] < 1);
            
            if (generarDeMemoria == "")
                genCell.GenerarAleatoria(celulasTotales, new Vector2(posX, posY));
            else
                genCell.GenerarDeMemoriaYMutar(generarDeMemoria, celulasTotales, new Vector2(posX, posY));
            celulasTotales++;
            cantCelulasVivas++;
            //yield return new WaitForEndOfFrame();
        }
        yearUltimaGeneracion = year;
        generando = false;
        yield return null;
    }
    private void FixedUpdate()
    {
        if (empezo)
        {
            if(cantCelulasVivas < cantCelulasMinimas && !generando)
            {
                StartCoroutine(GenerarCelulas(Mathf.Abs(cantCelulasMinimas-cantCelulasVivas)));
            }
            if (actualizarColores && !actualizando) {
                actualizando = true;
                StartCoroutine(board.ActualizaColores(mapa));
            }
            else 
                if(!actualizarColores && actualizando)
                {
                    actualizando = false;
                    StopCoroutine(board.ActualizaColores(mapa));
                    board.ColoresPorDefecto(mapa);
                }
        }
        
    }
}
