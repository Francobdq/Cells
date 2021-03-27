using System.Collections.Generic;
using UnityEngine;

public class GeneraCell : MonoBehaviour
{
    public GameObject cellGO;
    Cell cell;
    Cell cellPadre;
    GameController gameController;
    // Datos que necesita la celula
    string id = "";
    float velocidadMax = 0;
    float diametro = 0;
    float comidaInicial = 0;

    Color color = Color.white;
    Red redNeuronal;
    List<Cell> cellsVivas;

    public string mejorTotal;

    List<string> acortarValores;
    private void Start()
    {
        if (cellsVivas == null)
            cellsVivas = new List<Cell>();

        acortarValores = new List<string>();
    }
    private float RandomEntreValores(float valor, float entreValor, float minimo = 0)
    {
        float salida = Random.Range(valor - entreValor, valor + entreValor);
        if(salida < minimo)
            salida = minimo;
        return salida;
    }
    // Muta a la celula dependiendo de una determinada probabilidad el multiplicador se encarga de distinguir las diferencias
    private void Mutar(int probabilidadDeMutar, float mul = 1f)
    {

        if(Random.Range(0, 100) < probabilidadDeMutar)
            velocidadMax = RandomEntreValores(velocidadMax, 0.2f * mul);

        //Debug.Log("Diametro: " +diametro);
        if(Random.Range(0, 100) < probabilidadDeMutar)
            diametro = RandomEntreValores(diametro,0.5f*mul, 2f);

        if (Random.Range(0, 100) < probabilidadDeMutar)
        {
            float diferencia = Mathf.Clamp(20 * mul, 0, 255) / 255; // es el minimo que puede bajar o el maximo que puede subir (ver RandomEntreValores)
            color.r = Mathf.Clamp(RandomEntreValores(color.r, diferencia), 0, 1);
            color.g = Mathf.Clamp(RandomEntreValores(color.g, diferencia), 0, 1);
            color.b = Mathf.Clamp(RandomEntreValores(color.b, diferencia), 0, 1);
            //Debug.Log("r:" + color.r + " g:" + color.g + " b:" + color.b);
        }
        //Debug.Log("Diametro: "+diametro);
    }

    private void Instanciar(Vector2 posicionPadre, float probabilidadPesosH,bool puedeAtacar, float mulH = 1, string cargarDeMemoria = "")
    {
        if (gameController == null)
            gameController = GetComponent<GameController>();

        Vector2 posicion = posicionPadre;
        float posX = Random.Range(-0.05f, 0.05f);
        float posY = Random.Range(-0.05f, 0.05f);
        posicion.x += (posX != 0) ? posX :  0.05f;
        posicion.y += (posY != 0) ? posY : -0.05f;

        GameObject cellHija = Instantiate(cellGO, posicion, Quaternion.identity);

        cellHija.name = id;

        Cell cellH = cellHija.GetComponent<Cell>();
        if (cellsVivas == null)
            cellsVivas = new List<Cell>();
        cellsVivas.Add(cellH);

        if (cellH.redNeuronal == null)
            cellH.InicializaRed();

        Red redH = cellH.redNeuronal;
        cellH.CargaDatos(id, velocidadMax, diametro, comidaInicial, color, gameController.ObtenerYear(), puedeAtacar);
        if(cellPadre != null)
            redH.CopiarDatosRed(cellPadre.redNeuronal);
        if(cargarDeMemoria != "")
        {
            if(SaveLoad.redesGuardadas.Count == 0)
            {
                SaveLoad.Load(cargarDeMemoria, true);
            }
                
            //Debug.Log("redesGuardadas: " + SaveLoad.redesGuardadas.Count);
            redH.CopiarDatosRed(SaveLoad.redesGuardadas[0]);
        }
            
        if(probabilidadPesosH > 0f)
            redH.ModificaRed(probabilidadPesosH);

        cellH.IniciarCiclo();

    }
    private int ExisteElementoEnPos(string dato) {
        for (int i = acortarValores.Count-1; i >= 0; i--) { // Busca al reves ya que es m�s probable encontrarlo en el borde
            if (acortarValores[i].Equals(dato))
                return i;
        }
        return -1;    
    }
    private void CopiarValores(Cell cellPadre)
    {

        if (cellPadre.id.Length < 100) // Si el largo es menor a 100
            this.id = cellPadre.id;
        else {
            int valor = ExisteElementoEnPos(cellPadre.id);
            if (valor != -1)// Si existe
                this.id = "." + valor.ToString();
            else {
                acortarValores.Add(cellPadre.id);
                this.id = "." + acortarValores.Count.ToString();
            }
            
        }
        //this.velocidadMax = 5cellPadre.velocidadMax;
        this.diametro = cellPadre.diametro;
        this.comidaInicial = cellPadre.comida;
        this.color = cellPadre.color;

        this.cellPadre = cellPadre;
        
    }
    public List<Cell> ObtenerLista()
    {
        
        List<Cell> copiaLista = new List<Cell>();
        if(cellsVivas != null)
            for (int  i = 0;  i < cellsVivas.Count;  i++)
            {
                copiaLista.Add(cellsVivas[i]);
            }
        return copiaLista;
    }
    public void RecorrerListaBuscandoRecord()
    {
        if(cellsVivas.Count > 0)
        {
            gameController.RecordAct(cellsVivas[0]);
            if (cellsVivas[0].edad > gameController.ObtenerRecordTotal()) // la mayor es siempre la primera al ser una lista (y si hay mas de un valor igual, no importa quien sea)
                gameController.RecordTot(cellsVivas[0]);
        }
        /*int cantEspecie = -1;
        for(int i = 0; i < cellsVivas.Count; i++)
        {
            cellsVivas[i].id;
        }*/
    }
    public void VaciarDeLista(Cell vaciar)
    {
        if (!cellsVivas.Remove(vaciar))
        {
            Debug.Log("No se pudo eliminar la celula de la lista");
        }
    }
    private static string ToHex(int n)
    {
      return System.Convert.ToString(n, 16).ToUpper();
    }

    private Color ColorAleatorio() {
        return new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public void GenerarPorPadre(Cell cellPadre, float comidaInicial,int cantHijos, int probabilidadMutar = 10, float probabilidadMutarPesos = 0.01f, float mul = 0.05f)
    {
        probabilidadMutarPesos *= gameController.radiacion;
        mul *= gameController.radiacion;
        this.cell = cellPadre;

        // Obtiene los valores de la celula padre
        CopiarValores(cellPadre);
        this.comidaInicial = comidaInicial;
        this.id += "-" + cantHijos;
        Mutar(probabilidadMutar);
        Instanciar(cellPadre.gameObject.transform.position, probabilidadMutarPesos, true, mul);

    }
    public void GenerarDeMemoriaYMutar(string CellGenerar,int numId, Vector2 pos, int probabilidadMutar = 30, float mulM = 1f, float probabilidadMutarPesos = 0.03f, float mul = 1f)
    {
        if (cellsVivas == null)
            cellsVivas = new List<Cell>();
        // Basico
        this.id = ToHex(numId)+ "m";
        velocidadMax = 5;
        diametro = 3;
        comidaInicial = 300;
        color = ColorAleatorio();
        color.a = 255;

        Mutar(probabilidadMutar, mulM);

        Instanciar(pos, probabilidadMutarPesos, false, mul, CellGenerar);

    }
    public void GenerarAleatoria(int numId, Vector2 pos, int probabilidadMutar = 80,float mulM = 1.5f, int probabilidadMutarPesos = 0)
    {
        if(cellsVivas == null)
            cellsVivas = new List<Cell>();
        // Basico
        this.id = ToHex(numId);
        velocidadMax = 5;
        diametro = 3;
        comidaInicial = 300;
        color = ColorAleatorio();
        color.a = 255;

        Mutar(probabilidadMutar, mulM);

        Instanciar(pos, probabilidadMutarPesos, false);
    }
    private void FixedUpdate()
    {
        if(cellsVivas != null)
            RecorrerListaBuscandoRecord();
    }
}
