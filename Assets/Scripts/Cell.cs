using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    
    public string   id = "";
    public int      edad = 0;
    public bool     cargarRed = false;
    public string   RedACargar = "";
    public bool     teclado = false;
    //public float    velocidadMax = 5; // la velocidad maxima a la que puede ir (tiene sentido ya que fisicamente tenemos limite al que podemos ir en velocidad)
    public float    diametro = 3;

    public float    comida = 0;
    public Color    color = Color.grey;

    
    public GameObject tick; // marca algun premio o recompensa adquirido ( ser el mayor)
    public SpriteRenderer accion; // Marca la accion que se está realizando

    bool cambiarMuestraAccion = false; // si es verdadero deja de mostrar la accion(o bien la muestra) y vuelve a su estado original 
    bool mostrarAccion = true; // indica si se está o no mostrando la accion

    [SerializeField] Vector2 posTile; // es la posicion que le corresponde en la matriz

    int nacimiento = 0;

    float[,] mapa;
    float filas; // y
    float columnas; // x
    [SerializeField]
    float comidaSueloPosActual;

    FuncionesActivacion funcionActivacion;
    public Red redNeuronal;
    Cerebro cerebro;
    [SerializeField] float[] entradaRedd; // Solo para mostrar en unity
    public float[] salidaRed;
    [SerializeField] float[] memoria; 
    int cantEntradas = 8; // SIN CONTAR LA MEMORIA
    int cantSalidas = 6; // SIN CONTAR LA MEMORIA
    int cantMemoria = 0;
    



    GameController gameController;
    GeneraCell generaCell;


    public GameObject antena1;
    public GameObject antena2;
    public GameObject mov;
    //public GameObject sensorQuimico;

    Collider2D antena1Coll;
    Collider2D antena2Coll;
    SpriteRenderer antena1Ren;
    SpriteRenderer antena2Ren;

    public float antena1Toca = 0;
    public float antena2Toca = 0;
    Rigidbody2D rig;
    SpriteRenderer spriteRen;


    Cell contacto; // la celula que esta viendo (tocando con su antena, para herir)

    bool activado = false;
    bool comiendo = false;
    bool atacando = false;
    bool reproduciendose = false;
    bool murio = false;
    bool siendoAtacado = false;
    bool puedeAtacar = false; // en generacion aleatoria no atacarán, deberan evolucionar esa caracteristica

    float velocidadTiempo = 1; // la velocidad de juego en general

    public bool guardarEnRed;

    Vector2 posInicial;

    int cantHijos = 0;
    private void Awake()
    {
        cerebro = new Cerebro();
        salidaRed = new float[cantSalidas + cantMemoria];
        memoria = new float[cantMemoria];
        tick.SetActive(false);
        //redNeuronal = GetComponent<Red>();
        funcionActivacion = new FuncionesActivacion();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        generaCell = gameController.GetComponent<GeneraCell>();
        rig = GetComponent<Rigidbody2D>();
        spriteRen = GetComponent<SpriteRenderer>();
        color = GetComponent<SpriteRenderer>().color;
        antena1Ren = antena1.GetComponent<SpriteRenderer>();
        antena2Ren = antena2.GetComponent<SpriteRenderer>();
        antena1Coll = antena1.GetComponent<BoxCollider2D>();
        antena2Coll = antena2.GetComponent<BoxCollider2D>();
        // la posicion 10 en realidad esta entre 9.5 y 10.5
        posTile = new Vector2(Mathf.Round(rig.position.x), Mathf.Round(rig.position.y));

        velocidadTiempo = 1;
        InicializaRed();
        IniciarCiclo();
        posInicial = new Vector2(posTile.x, posTile.y);
        SondeoDeLaRed();
        ActualizarDiametro();
    }
    // Red Neuronal 
        // Las acciones se actualizan
        private void Acciones()
        {
            salidaRed[0] = funcionActivacion.TanH(salidaRed[0]); // Comer (0, 1)
            salidaRed[1] = funcionActivacion.TanH(salidaRed[1]); // reproducirse(0 , 1)
            salidaRed[2] = Mathf.Round(funcionActivacion.TanH(salidaRed[2])); // Rotacion (-1, 0, 1)
            salidaRed[3] = Mathf.Round(funcionActivacion.TanH(salidaRed[3])); // Movimiento (-1 , 0 , 1)

            float velocidad = funcionActivacion.Relu(salidaRed[4]);
            float velRotacion = funcionActivacion.Relu(salidaRed[5]);
            salidaRed[4] = (velocidad < 0.5f) ? 0.5f : velocidad; // velocidad
            salidaRed[5] = (velRotacion < 0.5f) ? 0.5f : velRotacion; // velocidad De rotación


            /*for (int i = 0; i < cantMemoria; i++)
            {
                salidaRed[i + cantSalidas] = memoria[i];
            }*/
        //salidaRed[5] = 0; // funcionActivacion.Sigmoide(salidaRed[5]); // Atacar o no en ese momento

        //Debug.Log("Salida red : " + salidaRed[2]);
    }
        // El sondeo de la red produce una actualizacion de la misma
        private void SondeoDeLaRed()
        {
            edad = gameController.ObtenerYear() - nacimiento;
                
            CheckeaAntenas();

            comidaSueloPosActual = (mapa == null) ? -100 : CheckeaComidaSuelo(); // al entrar al sondeo se actualiza la cantidad de comida que ve en el suelo
            float[] entrada = new float[cantEntradas + cantMemoria];
            //Debug.Log(comidaSueloPosActual);
            entrada[0] = antena1Toca; // 0 si no toca nada, 1 si es una celula y 0.5 si es un muro
            entrada[1] = antena2Toca;

            
            //entrada[2] = (atacando) ? 1 : 0;
            entrada[2] = (comiendo) ? 1 : 0;
            entrada[3] = (reproduciendose) ? 1 : 0;

            //entrada[5] = (siendoAtacado) ? 1 : 0; // si está o no siendo atacado

            entrada[4] = comida / 100; // la cantidad de comida de la celula
            entrada[5] = comidaSueloPosActual / 100;

            entrada[6] = transform.rotation.z;
                
            entrada[7] = gameController.Temperatura();


            for (int i = 0; i < cantMemoria; i++) {
                entrada[i + cantEntradas] = memoria[i];
            }
            entradaRedd = entrada;
            salidaRed = redNeuronal.Activar(entrada);
            Acciones();
        }
        
        // tiempos en los que se espera para visualizar las entradas nuevamente
        IEnumerator TiemposDeSondeo(float tiempoSondeo)
        {
            while (true)
            {
                yield return new WaitForSeconds(tiempoSondeo / velocidadTiempo);
                SondeoDeLaRed();
                
            }
        }
        public void InicializaRed()
        {
            int[] neuronasPorCapa = { cantEntradas + cantMemoria,cantEntradas/2, cantSalidas + cantMemoria }; 
            string[] funcionesDeActivacion = { "LeakyRelu", "LeakyRelu", "" };

            redNeuronal = new Red(neuronasPorCapa, funcionesDeActivacion, id, funcionActivacion);    
            //redNeuronal.InicializaRed(neuronasPorCapa, funcionesDeActivacion, id, funcionActivacion);
        }
        
    // Basicos Celula
        

        private void ActualizarDiametro()
        {
            float minDiametro = 1.5f;
            float maxDiametro = 1000000000;

            diametro = comida / 1000;
            
            if(diametro < minDiametro)
                diametro = minDiametro;
            else
                if(diametro > maxDiametro) // hay un error que aveces calcula infinito, esto busca eliminarlo
                    Muere();
            //diametro = (maxDiametro-minDiametro) / (1 + Mathf.Exp(-x + 4)) + minDiametro;
            transform.localScale = new Vector3(diametro, diametro, 1);
        }

        // Mata a la celula
        private void Muere()
        {
            if (!murio) {
                murio = true;
                generaCell.VaciarDeLista(this);
                //generaCell.RecorrerListaBuscandoRecord();
                gameController.cantCelulasVivas--;
                Destroy(this.gameObject);
            }
        }
        
        // Maneja el añadido de comida
        private void AgregaComida(float cantAgregada)
        {
            comida  += cantAgregada;
        }

        // Maneja la perdida de comida
        private void PierdeComida(float perdida, bool morir = true)
        {
            comida -= perdida;
            ActualizarDiametro();
            if (comida <= 0)
            {
                comida = 0;
                if(morir)
                    Muere();
            }
        }

        // Descuenta su alimento por los procesos que la mantienen viva    
        IEnumerator PerdidaDeComidaCorutina(float perdida)
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                PierdeComida(2*diametro);
            }
        }

        private void Teclado()
        {
            int rot = (int)Input.GetAxisRaw("Horizontal"); // devuelve 0 si no se pulsa ninguna tecla, 1 si se pulsa la de la derecha y -1 si se pulsa la de la izquierda
            int mov = (int)Input.GetAxisRaw("Vertical"); // 1 si se pulsa la de arriba y -1 si se pulsa la de abajo (y 0 si ninguna)
            
            bool comer = Input.GetButton("Jump");
            bool reproducirse = Input.GetKey(KeyCode.R);
            bool atacar = (Input.GetAxisRaw("Fire1") == 1) ? true : false;

            if(comer && !comiendo)
                StartCoroutine(Comiendo(1f));
            if(reproducirse && !reproduciendose)
                StartCoroutine(Reproduciendose(2f, 200 + comida/4));
            if(atacar && !atacando)
                StartCoroutine(Atacando(1f, 50f));

            Movimiento(mov, 5f);
            Rotar(rot, 5f);
        }
        private void IA()
        {
            bool comer = (salidaRed[0] > 0.5f) ? true : false;
            bool reproducirse = (salidaRed[1] > 0.5f ) ? true : false;
           // bool atacar = false; // (salidaRed[5] > 0.5f && puedeAtacar) ? true : false;   

            int rot = (int)salidaRed[2]; // devuelve 0 si no se pulsa ninguna tecla, 1 si se pulsa la de la derecha y -1 si se pulsa la de la izquierda
            int mov = (int)salidaRed[3]; // 1 si se pulsa la de arriba y -1 si se pulsa la de abajo (y 0 si ninguna)

            if((comer && reproducirse) || (comer && atacando) || (atacando && reproducirse))
            {
                if(comer && !comiendo && salidaRed[0] > salidaRed[1] /*&& salidaRed[0] > salidaRed[5]*/)
                    StartCoroutine(Comiendo(1f));
                else
                    if(reproducirse && !reproduciendose && salidaRed[1] > salidaRed[0] /*&& salidaRed[1] > salidaRed[5]*/)
                        StartCoroutine(Reproduciendose(2f, 200));
                    /*else
                        if(atacar &&  !atacando && salidaRed[5] > salidaRed[0] && salidaRed[5] > salidaRed[1])
                            StartCoroutine(Atacando(1f, 60f));*/
            }
            else
            {
                if (comer && !comiendo)
                    StartCoroutine(Comiendo(1f));
                else
                    if (reproducirse && !reproduciendose)
                        StartCoroutine(Reproduciendose(2f, 200));
                    /*else
                        if(atacar && !atacando)
                            StartCoroutine(Atacando(1f, 60f));*/
            }
            


            Movimiento(mov, salidaRed[4]);
            Rotar(rot, salidaRed[5]);
        }
        
    // Ayudas de Interacciones con el ambiente
        IEnumerator Reproduciendose(float tiempoEnReproducirse, float cantidadNecesariaParaSobrevivir)
        {
            reproduciendose = true;
            if(mostrarAccion)
                accion.color = Color.blue;
            yield return new WaitForSeconds(tiempoEnReproducirse / velocidadTiempo);
            if(mostrarAccion)
                accion.color = Color.white;
            reproduciendose = false;
            Reproducirse(cantidadNecesariaParaSobrevivir);
        }

        // Tiempo Que tarda en comer
        IEnumerator Comiendo(float tiempoEnComer)
        {
            comiendo = true;
            if(mostrarAccion)
                accion.color = Color.yellow;
            yield return new WaitForSeconds(tiempoEnComer / velocidadTiempo);
            if(mostrarAccion)
                accion.color = Color.white;
            comiendo = false;
            Comer();
        }
        // Tiempo Que tarda en atacar
        IEnumerator Atacando(float tiempoEnAtacar, float damage)
        {
            atacando = true;
            if(mostrarAccion)
                accion.color = Color.red;
            yield return new WaitForSeconds(tiempoEnAtacar / velocidadTiempo);
            if(mostrarAccion)
                accion.color = Color.white;
            atacando = false;
            Atacar(damage);
        }
        // Temporizador al morir 
        IEnumerator MuerteTemp()
        {
            yield return new WaitForEndOfFrame();
            Muere();
        }
        IEnumerator ActualizaRecibeAtaque(float seg)
        {
            yield return new WaitForSeconds(seg / velocidadTiempo);
            accion.color = Color.white;
            siendoAtacado = false;
        }
        // Dejar de ver
        /*private void OnTriggerExit2D(Collider2D collision)
        {
            CheckeaAntenas(collision, false);
            contacto = null;
        } */

        // Verifica cada antena para saber cual tocó que cosa
        private void CheckeaAntenas()
        {
        
            ContactFilter2D filter = new ContactFilter2D();

            if (antena1Coll.IsTouching(filter.NoFilter()))
            {
                if (mostrarAccion)
                    antena1Ren.color = Color.white;
                if(antena1Coll.CompareTag("Cell"))
                    antena1Toca = 1;
                else
                    antena1Toca = 0.5f;
            }
            else
            {
                if (mostrarAccion)
                    antena1Ren.color = Color.red;
                antena1Toca = 0;
            }
            if(antena2Coll.IsTouching(filter.NoFilter()))
            {
                if (mostrarAccion)
                    antena2Ren.color = Color.white;

                if (antena1Coll.CompareTag("Cell"))
                    antena2Toca = 1;
                else
                    antena2Toca = 0.5f;
                //antena2Toca = true;
            }
            else
            {
                if(mostrarAccion)
                    antena2Ren.color = Color.red;
                antena2Toca = 0;
            }

            
        }
            
    // Interacciones con el ambiente
        // Atacar
        private void Atacar(float damage)
        {
            PierdeComida(30);
            if (contacto != null)
            {
                AgregaComida(contacto.RecibirAtaque(damage));
            }
        }
        // Ve
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Finish"))
            {
                //CheckeaAntenas(collision, true);
                contacto = collision.GetComponent<Cell>(); // si es nulo, que siga nulo
            }
        }
        private float CheckeaComidaSuelo()
        {
            float comidaTile = -100;
            if((int) posTile.x >= columnas || (int) posTile.y >= filas || (int)posTile.x < 0 || (int)posTile.y < 0 || mapa == null)
            {
                Debug.Log("Se fue: length 0:" + columnas + " length 1: " + filas);
                Debug.Log("Se fue  x:" + (int)posTile.x + " y: " + (int)posTile.y + " Velocidad: " + salidaRed[4] + /*" VelocidadMax: " + velocidadMax +*/ "posInicial: " + posInicial + "edad: " + edad);
                Muere();
            }
            else
                comidaTile = mapa[(int)posTile.x, (int)posTile.y];
            return comidaTile;
        }
        // Se reproduce
        private void Reproducirse(float cantidadNecesariaParaVivir)
        {
            if(comida > cantidadNecesariaParaVivir*2f)
            {
                cantHijos++;
                generaCell.GenerarPorPadre(this, cantidadNecesariaParaVivir, cantHijos);
                gameController.cantCelulasVivas++;
                gameController.celulasTotales++;
                PierdeComida(cantidadNecesariaParaVivir);
            
            }
            else
            {
                PierdeComida(cantidadNecesariaParaVivir/2);
            }
                
        }
        
        // Come del suelo donde esta
        private void Comer()
        {
            PierdeComida(30); // gasta energia al comer
            
            float aux = comida;
            float comidaTile = CheckeaComidaSuelo();
            if(comidaTile <= 0)
                return;
            AgregaComida(comidaTile);

            if(comidaTile > 0)
                mapa[(int)posTile.x, (int)posTile.y] = 1;

            ActualizarDiametro();

        }
        // rota a una determinada direccion a cierta velocidad
        private void Rotar(int direccion, float velocidad) 
        {
            // La direccion puede ser -1, 0 o 1 e indica el sentido o la no rotacion (-1 izquierda, 0 no rotar, 1 derecha)
            if(direccion != 0)
            {
                //Debug.Log(direccion);
                transform.Rotate(0, 0, -direccion * velocidad); // ver si se compenza
            }

        }

        // Devuelve el valor de la posicion del mapa
        private float ValorPosTile(Vector2 pos) {
            return mapa[(int)pos.x, (int)pos.y];
        }
        
        // Mueve la cell adelante o atras a una determinada velocidad
        private void Movimiento(int vertical, float velocidad)
        {
            if (vertical != 0 && velocidad > 0)
            {


                Vector3 aux = mov.transform.localPosition; // copia el transform del movimiento
                
                aux.y = (((CheckeaComidaSuelo() == -100) ? 0.1f : 1f)*velocidad) * (0.015f * (1f / diametro)); // lo reduce a la posicion donde deberia ir (si la comida del suelo es -100 significa que es agua y por ende reduce su velocidad)
                mov.transform.localPosition = aux;
                Vector2 nuevaPosicion = mov.transform.position; // se mueve a esta nueva posicion

            

                if (nuevaPosicion.x < 1 || nuevaPosicion.y < 1 || nuevaPosicion.x >= columnas-2 || nuevaPosicion.y >= filas-2) { // Si se quiere ir, se repele
                    //Debug.Log("La posicion es menor a 0 y esto son los datos. Transform: " + transform.position + " mov : " + nuevaPosicion + " paso: " + aux.y + " dando como resultado: " + nuevaPosicion);
                    nuevaPosicion = transform.position;
                }
                    
                
                transform.position = nuevaPosicion;
                aux.y = 1;
                mov.transform.localPosition = aux;
                // la posicion 10 en realidad esta entre 9.5 y 10.5
                posTile = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));

                //la minima cantidad que puede perder de comida es 1/24
                float cantidadPerdida = (velocidad / 12) * (diametro/2);

                //pierde comida al moverse
                PierdeComida(cantidadPerdida);
            }
        }

    // Asignacion de datos Iniciales


        // Ingresa todos los valores base que le corresponden a la cell
        public void CargaDatos(string id,float velocidadMax, float diametro, float comidaInicial, Color color, int nacimiento, bool puedeAtacar, bool cargarRed = false)
        {
            this.nacimiento = nacimiento;
            this.id = id;
            this.cargarRed = cargarRed;
            //this.velocidadMax = velocidadMax;
            this.diametro = diametro;
            this.comida = comidaInicial;
            this.color = color;

            this.puedeAtacar = true;//puedeAtacar;
            CambiarMuestraAccion();
            spriteRen = GetComponent<SpriteRenderer>();
            // cambia el color
            spriteRen.color = color;

            // asigna diametro
            Vector3 nuevoDiametro = new Vector3(diametro, diametro, 1);
            transform.localScale = nuevoDiametro;

            
        }

    // Comunicacion de clases (programacion)
        public void DevuelveValores(ref string id,ref float velocidadMax,ref float diametro,ref float comidaMax,ref Color color)
        {
            id = this.id;
            //velocidadMax = this.velocidadMax;
            diametro = this.diametro;
            //comidaInicial = this.comida;
            color = this.color;
        }
        // Inicia el ciclo
        public void IniciarCiclo()
        {
            StartCoroutine(PerdidaDeComidaCorutina(10));
            StartCoroutine(TiemposDeSondeo(0.1f));
            this.mapa = gameController.mapa;
            filas = gameController.ObtenerFilas();
            columnas = gameController.ObtenerColumnas();
            activado = true; 
        }
        // controlar o no la celula 
        public void ControlarCell(bool controlar)
        {
            teclado = controlar;
        }
        // Recibe el ataque
        public float RecibirAtaque(float damage)
        {
            float comidaADar = (damage > comida) ? comida : damage;
            PierdeComida(damage, false);
            siendoAtacado = true;
            accion.color = Color.magenta;
            ActualizaRecibeAtaque(0.49f); // como se actualiza la red en 0.5f da tiempo a que no se detecte 2 veces ya habiendo salido
            if(comida <= 0)
                StartCoroutine(MuerteTemp());

            return comidaADar;
        }
        // Actualiza la velocidad de juego
        public void ActualizarVelJuego()
        {
            velocidadTiempo = 1;
        }
        public float[] DevuelveDatosRed()
        {
            float[] aux = new float[salidaRed.Length + cantMemoria];
            for (int i = 0; i < salidaRed.Length; i++)
            {
                aux[i] = salidaRed[i];
            }
            return aux;
        }
        // Carga la Red
        private void CargarRed()
        {
            string aux = RedACargar;
            RedACargar = "Cell" + RedACargar;
            if(RedACargar == "Cell")
                SaveLoad.Load();
            else
                SaveLoad.Load(RedACargar);
            redNeuronal = SaveLoad.redesGuardadas[0];
            RedACargar = aux;
        }
        // Actualiza frame
        public void CambiarMuestraAccion()
        {
            cambiarMuestraAccion = false;
            mostrarAccion = gameController.mostrarAccion;
            //tick.SetActive(mostrarAccion);
            accion.gameObject.SetActive(mostrarAccion);
            
        }
        private void Update()
        {
            if(!comiendo && !reproduciendose && !atacando && activado)
            {
                if(teclado)
                    Teclado();
                else
                    IA();
            }
            if(cargarRed == true)
            {
                cargarRed = false;
                CargarRed();
            }
            if(guardarEnRed == true)
            {
                guardarEnRed = false;
                gameController.GuardarRed(this.redNeuronal, ("/" + id + ".bdq"));
            }
            if (cambiarMuestraAccion)
            {
                CambiarMuestraAccion();
            }
        }
}
