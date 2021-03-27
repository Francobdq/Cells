using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlUsuario : MonoBehaviour
{
    public GameObject muestraTilePosMouse;

    GameController gameController;

    float[,] mapa;
    int filas;
    int columnas;

    public float comidaEnTile = 0;
    public Vector2 posicionTile;
    public Transform CameraTransform;

    CameraFollow CameraFollow;

    bool controlaCell = false;
    Cell celulaControlada = null;

    public UIManager UiManager;
    [SerializeField] float velocidadCam = 10f;
    
    private void Start()
    {
        
        StartCoroutine(FixedTrucho());
        gameController = GetComponent<GameController>();
        CameraFollow = CameraTransform.gameObject.GetComponent<CameraFollow>();
        UiManager.GetComponent<UIManager>();
    }
    private Vector2 ObtenerMousePosTile()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        return pos;
        
    }
    private float ObtenerCuantaComidaEnTile()
    {
        return ControlNoSalirse(ObtenerMousePosTile());
    }

    private void MuestraTilePosMouse()
    {
        muestraTilePosMouse.transform.position = ObtenerMousePosTile();
    }

    private float ControlNoSalirse(Vector2 mousePos)
    {
        float comidaTile = 0;
        if ((int)mousePos.x >= columnas || (int)mousePos.y >= filas || (int)mousePos.x < 0 || (int)mousePos.y < 0 || mapa == null)
        {

        }
        else
            comidaTile = mapa[(int)mousePos.x, (int)mousePos.y];
        return comidaTile;
    }
    private RaycastHit2D TocaMouse()
    {
        Vector2 objetivo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(objetivo, objetivo);
        return hit;
    }

    private void Click()
    {
        float click = Mathf.Clamp01(Mathf.Round(Input.GetAxisRaw("Fire1")));
        if(click == 1)
        {

            RaycastHit2D hit = TocaMouse();
            if (hit == true)
            {
                //Debug.Log("click");
                if (hit.collider.CompareTag("Cell"))
                {
                    UiManager.celulaAVer = hit.collider.gameObject.GetComponent<Cell>();
                    UiManager.ActualizarDatosBasicos();
                    /*celulaControlada = hit.collider.gameObject.GetComponent<Cell>();
                    controlaCell = true;
                    celulaControlada.ControlarCell(true);*/
                    //CameraFollow.Seguir(true, celulaControlada.transform);
                }
            }
        }
    }
    private void Zoom(float velocidadZoom)
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        float size = Camera.main.orthographicSize;
        Camera.main.orthographicSize = Mathf.Clamp(size - zoom * velocidadZoom, 0.5f, 500);
    }
    private void Velocidad()
    {

        if (Input.GetKey(KeyCode.E))
        {
            velocidadCam += 0.01f;
        }
        else
            if (Input.GetKey(KeyCode.Q)){
                velocidadCam -= 0.01f;
            }

    }
    private void MovCamara(float velocidad)
    {
        int horizontal = (int)Input.GetAxisRaw("Horizontal"); // devuelve 0 si no se pulsa ninguna tecla, 1 si se pulsa la de la derecha y -1 si se pulsa la de la izquierda
        int vertical = (int)Input.GetAxisRaw("Vertical"); // 1 si se pulsa la de arriba y -1 si se pulsa la de abajo (y 0 si ninguna)   

        if(horizontal != 0 || vertical != 0)
        {
            float posX =  CameraTransform.position.x + (horizontal * velocidad * Time.deltaTime);
            float posY =  CameraTransform.position.y + (vertical *velocidad * Time.deltaTime);

            CameraTransform.position = new Vector3(posX, posY, -10f);
        }
    }





    private void MuestraDatosNeurona()
    {
        if (TocaMouse().collider.CompareTag("Cell"))
        {

        }
    }


    public void InicializarMapa()
    {
        if(gameController == null)
            gameController = GetComponent<GameController>();
        mapa = gameController.mapa;
        filas = gameController.ObtenerFilas();
        columnas = gameController.ObtenerColumnas();
    }



    private void Act()
    {
        if (mapa != null)
        {

            if (!controlaCell)
            {
                comidaEnTile = ObtenerCuantaComidaEnTile();
                posicionTile = ObtenerMousePosTile();
                MuestraTilePosMouse();

                Click();
                Velocidad();
                MovCamara(velocidadCam);
                Zoom(velocidadCam);
                if (Input.GetKey(KeyCode.G))
                {
                    StartCoroutine(gameController.GenerarCelulas(1));
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    celulaControlada.ControlarCell(false);
                    CameraFollow.Seguir(false);
                    controlaCell = false;
                }
                if (celulaControlada == null)
                {
                    CameraFollow.Seguir(false);
                    controlaCell = false;
                }
            }

        }
        else
        {
            InicializarMapa();
        }
    }
    IEnumerator FixedTrucho()
    {
        while (true)
        {
            Act();
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
    
}
