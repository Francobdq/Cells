using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    int columnas;
    int filas;
    int bordesAgua;


    public GameObject[] floorTiles, outerWallTiles, watherTiles, desiertoTiles;// inWallsTiles, foodTiles, enemyTiles;
    //public GameObject exit;

    private Transform boardHolder;
    private List<Vector2> gridPositions = new List<Vector2>(); // gridPositions es una variable. esta linea crea la lista para la generacion dentro del array 6x6


    public SpriteRenderer[,] tableroTiles;  // Los tiles del mapa
    public Color[] coloresPasto; // los colores que tomar� el pasto (comenzando desde el vacio hasta el lleno)

    void InitializeList()
    {
        gridPositions.Clear();
        for (int x = 1; x < columnas - 1; x++)
        {
            for (int y = 1; y < filas - 1; y++)
            {
                gridPositions.Add(new Vector2(x, y));
            }
        }
    }


    Vector2 RandomPosition()
    {   // genera los elementos dentro del mapa (comida, paredes, enemigos)
        int randomIndex = Random.Range(0, gridPositions.Count); // aleatorio entre 0 y el total de elementos de gridPositions
        Vector2 randomPosition = gridPositions[randomIndex];  // obtenemos el valor dentro de la posicion
        gridPositions.RemoveAt(randomIndex); // remueve el elemento de la lista y entre parentesis la posicion que se quiere eliminar
        return randomPosition;
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int min, int max)
    {
        
        int objectCount = Random.Range(min, max + 1); // un valor entre la cantidad de ese objeto minima y maxima
        for (int i = 0; i < objectCount; i++)
        {
            Vector2 randomPosition = RandomPosition();
            GameObject tileChoise = GetRandomInArray(tileArray); // recordar que GetRandomInArray es un metodo declarado debajo
            Instantiate(tileChoise, randomPosition, Quaternion.identity);
        }
    }

    void BoardSetup(ref float[,] mapa)
    {
        boardHolder = new GameObject("Mapa").transform;
        GameObject toInstantiate;
        CrearMapa(ref mapa);
        for (int x = -1; x < columnas + 1; x++)
        {
            for (int y = -1; y < filas+ 1; y++)
            {
                /*if(x >= 0 && y >= 0 && x < columnas && y < columnas)
                    mapa[x, y] = -1000;*/
                if (x <= 0/*-1*/ || y <= 0/*-1*/ || x >= columnas-1 || y >= filas-1)
                {
                    toInstantiate = GetRandomInArray(outerWallTiles);
                }
                else
                {
                    if ((x > bordesAgua && y > bordesAgua) && (x < columnas - bordesAgua - 1 && y < filas - bordesAgua - 1))
                    {
                        switch (mapa[x, y])
                        {
                            case 0:
                                    mapa[x, y] = -100;
                                    toInstantiate = GetRandomInArray(watherTiles);
                                break;
                            case 1:
                                    mapa[x, y] = 1;
                                    toInstantiate = GetRandomInArray(floorTiles);
                                break;
                            case 2:
                                    mapa[x,y]  = -50;
                                    toInstantiate = GetRandomInArray(desiertoTiles);
                                break;
                            default:
                                mapa[x, y] = -100;
                                toInstantiate = GetRandomInArray(watherTiles);
                                break;
                        }
                    }
                    else
                    {
                        mapa[x, y] = -1000;
                        toInstantiate = GetRandomInArray(watherTiles);
                    }
                    
                }
                GameObject instancia = Instantiate(toInstantiate, new Vector2(x, y), Quaternion.identity);
                if (x > 0 && y > 0 && x < columnas - 1 && y < filas - 1)
                {
                    tableroTiles[x, y] = instancia.GetComponent<SpriteRenderer>();
                }

                instancia.transform.SetParent(boardHolder);
            }
        }
    }

    float RedondearConComa(float valor)
    {
        if(valor > 1)
        {
            Debug.Log("El valor no puede ser mayor a uno");
            return -1;
        }

        /*if (valor > 0.5f)
        {
            if (valor > 0.66)
                return 2;
            else
                return 1;
        }
        else
            return 0;*/

        
        if (valor > 0.5f)
        {
            if (valor > 0.76f)
                return 2f;
            else
                return 1f;
        }
        else
            return 0;
            
            
    }
    void CrearMapa(ref float[,]mapa)
    {   
        float escala = 7f; // zoom          // 5 20 5    ---    8 20 20
        float offsetX = 20f; // movimiento
        float offsetY = 20f; 
        for (int x = bordesAgua; x < columnas-bordesAgua+1; x++)
        {
            for (int y = bordesAgua; y < filas-bordesAgua+1; y++)
            {
                mapa[x,y] = RedondearConComa(Mathf.PerlinNoise((float)x/columnas * escala + offsetX, (float)y/filas * escala + offsetY));
            }
        }
    }
    static GameObject GetRandomInArray(GameObject[] array)
    {
        return array[Random.Range(0, array.Length)];
    }



    bool coloresPorDefecto = false;
    public void ColoresPorDefecto(float[,] mapa) {
        coloresPorDefecto = true;
        

    }

    public IEnumerator ActualizaColores(float[,] mapa) {

        yield return new WaitForSeconds(1f);


        int constante = 500; // para verificar el rango de valores
        bool noSalir = true;
        while (noSalir) {

            for (int i = 0; i < tableroTiles.GetLength(0); i++) {
                for (int j = 0; j < tableroTiles.GetLength(1); j++)
                {
                    if (mapa[i, j] >= 0) {
                        int k = 0;
                        bool salida = false;
                        while (k < coloresPasto.Length && !salida) // Prueba todas las opciones de colores
                        {
                            if (mapa[i, j] > constante / coloresPasto.Length * k && mapa[i, j] < constante / coloresPasto.Length * (k+1))
                            {
                                tableroTiles[i, j].color = coloresPasto[k];
                                salida = true;
                            }
                            k++;
                        } 
                           
                                
                    }
                    //yield return new WaitForFixedUpdate();
                }
            }

            yield return new WaitForSeconds(1f);

            if (coloresPorDefecto) {
                coloresPorDefecto = false;
                noSalir = false;
            }
            
        }

        // Vuelve los colores al inicio
        for (int i = 0; i < tableroTiles.GetLength(0); i++)
        {
            for (int j = 0; j < tableroTiles.GetLength(1); j++)
            {
                if (mapa[i, j] > 0)
                {
                    tableroTiles[i, j].color = coloresPasto[coloresPasto.Length - 1];
                }
                //yield return new WaitForFixedUpdate();
            }
        }

    }


    public void SetupScene(int seed,ref float[,] mapa, int columnas, int filas, int bordesAgua)
    {
        this.bordesAgua = bordesAgua;
        mapa = new float[columnas, filas];
        for (int i = 0; i < mapa.GetLength(0); i++)
        {
            for (int j = 0; j < mapa.GetLength(1); j++)
            {
               
            }
        }
        this.columnas = columnas;
        this.filas = filas;
        tableroTiles = new SpriteRenderer[mapa.GetLength(0), mapa.GetLength(1)];
        //Random.InitState(seed);
        BoardSetup(ref mapa);
        InitializeList();
        //LayoutObjectAtRandom(inWallsTiles, 5, 9); // arreglo wileTiles declarado arriba, entre 5 y 9 muros
        //LayoutObjectAtRandom(foodTiles, 1, 5);
        //LayoutObjectAtRandom(watherTiles, 3, 5);
        //int enemyCount = (int)Mathf.Log(level, 2); // usa el logartimo base 2 con el nivel para la cantidad de enemigos 
        //LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount); // esto instancia varias veces, aparte de ser aleatorio (funcion creada arriba)
        //Instantiate(exit, new Vector2(columnas - 1, filas - 1), Quaternion.identity);

    }
}
