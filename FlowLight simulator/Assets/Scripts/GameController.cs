using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    /****************************  Definiciones  *******************************/

    public enum Semaforos { SinSemaforos, Ciclo, CicloTiempoVariable, VaciarMayorTrafico, GranVia, VariosCruces };



    /****************************  Variables  *******************************/


    [Header("Ajustes de simulación")]
    public Semaforos semaforos = Semaforos.Ciclo;
    public Inicio.DensidadTrafico densidadDelTrafico = Inicio.DensidadTrafico.Normal;
    public float tiempoSimulacion = 3600f;
    [Range(0, 10)]
    public float velocidadReproduccion = 5f;
    private readonly float velocidadMaxima = 10f;
    public bool pausa;

    [Header("Datos de simulación")]
    public float tiempoTotal = 0f;
    public int fps;

    [Header("Estadísticas")]
    public float paradasPorCoche = 0f;
    public float esperaPorCoche = 0f;
    public float esperaPorSemaforo = 0f;
    public float esperaMaxima = 0f;
    
    [Header("Otras estadísticas")]
    public int pasadasEsperadas = 0;
    public int pasadasSinEsperar = 0;
    public int pasadasTotales = 0;

    //Variables privadas u ocultas
    private float tiempoEsperaTotal = 0f;
    private int cochesMuertos = 0;
    private int ParadasTotalesMuertos = 0; //Indica el total de paradas que hicieron los coches que ya están muertos
    private float totalEsperadoMuertos = 0f;
    [HideInInspector] public float delta;
    private float touchTime;

    private static readonly float ZoomSpeedTouch = 0.1f;
    private static readonly float[] ZoomBounds = new float[] { 15f, 60f };

    private Camera cam;
    private GameObject bordes;

    private Vector2 worldStartPoint;
    private int panFingerId; // Touch mode only

    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only


    /****************************  Funciones  *******************************/

    //Función para registrar datos de un coche
    public void RegistrarEspera(float tiempoEspera) {
        pasadasTotales++;
        if (tiempoEspera == 0f) pasadasSinEsperar++;
        else pasadasEsperadas++;
        if (tiempoEspera > esperaMaxima) esperaMaxima = tiempoEspera;
        tiempoEsperaTotal += tiempoEspera;
        esperaPorSemaforo = tiempoEsperaTotal / pasadasEsperadas;
    }


    //Función para registrar la muerte de un coche
    public void RegistrarMuerte(int paradas, float tiempoEsperado) {
        cochesMuertos++;
        ParadasTotalesMuertos += paradas;
        paradasPorCoche = ParadasTotalesMuertos / (float)cochesMuertos;
        totalEsperadoMuertos += tiempoEsperado;
        esperaPorCoche = totalEsperadoMuertos / cochesMuertos;
    }

    //Inicialización
    private void Start() {
        delta = velocidadReproduccion * Time.deltaTime;//Ajustamos el delta a la velocidad de reproducción
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        bordes = GameObject.Find("Bordes");
    }

    //El Update se usa para procesar las entradas del usuario
    private void Update() {

        delta = velocidadReproduccion * Time.deltaTime;//Ajustamos el delta a la velocidad de reproducción

        //Cogemos señales del Imput
        int cambioVelocidad = 0;

        switch (Input.touchCount) {

            case 1: // 1 dedo
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch currentTouch = Input.GetTouch(0);

                if (currentTouch.phase == TouchPhase.Began) {
                    worldStartPoint = currentTouch.position;
                    touchTime = Time.time;
                }

                if (currentTouch.phase == TouchPhase.Moved) {
                    if (Time.time - touchTime > 0.1) {
                        float xInicial = Camera.main.transform.position.x;
                        float zInicial = Camera.main.transform.position.z;
                        Vector2 worldDelta = currentTouch.position - worldStartPoint;
                        Camera.main.transform.Translate(-worldDelta.x / 15, -worldDelta.y / 15, 0);
                        float x, z;
                        if (CamaraDentroDeBorde(0)) x = Camera.main.transform.position.x; else x = xInicial;
                        if (CamaraDentroDeBorde(1)) z = Camera.main.transform.position.z; else z = zInicial;
                        Camera.main.transform.SetPositionAndRotation(new Vector3(x, Camera.main.transform.position.y, z), Camera.main.transform.rotation);
                    }
                    worldStartPoint = currentTouch.position;

                }else if (currentTouch.phase == TouchPhase.Ended || currentTouch.phase == TouchPhase.Canceled) {
                    if (Time.time - touchTime <= 0.3) {
                        if (Input.GetTouch(0).position.x > (Screen.width * 2 / 3)) {
                            cambioVelocidad = 1;
                        } else if (Input.GetTouch(0).position.x < (Screen.width / 3)) {
                            cambioVelocidad = -1;
                        } else {
                            pausa = !pausa;
                        }
                    }
                }
                break;

            case 2: // 2 dedos

                //Hacer zoom
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame) {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                } else {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }

                //Mover
                currentTouch = Input.GetTouch(0);

                if (currentTouch.phase == TouchPhase.Began) {
                    worldStartPoint = currentTouch.position;
                    touchTime = Time.time;
                }

                if (currentTouch.phase == TouchPhase.Moved) {
                    if (Time.time - touchTime > 0.1) {
                        float xInicial = Camera.main.transform.position.x;
                        float zInicial = Camera.main.transform.position.z;
                        Vector2 worldDelta = currentTouch.position - worldStartPoint;
                        Camera.main.transform.Translate(-worldDelta.x / 15, -worldDelta.y / 15, 0);
                        float x, z;
                        if (CamaraDentroDeBorde(0)) x = Camera.main.transform.position.x; else x = xInicial;
                        if (CamaraDentroDeBorde(1)) z = Camera.main.transform.position.z; else z = zInicial;
                        Camera.main.transform.SetPositionAndRotation(new Vector3(x, Camera.main.transform.position.y, z), Camera.main.transform.rotation);
                    }
                    worldStartPoint = currentTouch.position;

                }

                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }



        if (Input.GetKeyDown(KeyCode.Escape)) {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
		        Application.Quit();
            #endif
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || cambioVelocidad == 1) {
            if (velocidadReproduccion < velocidadMaxima) velocidadReproduccion += 0.25f;
            delta = velocidadReproduccion * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || cambioVelocidad == -1) {
            if (velocidadReproduccion > 0.25) velocidadReproduccion -= 0.25f;
            delta = velocidadReproduccion * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            pausa = !pausa;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            pausa = false;
        }
        
        fps = (int) (1 / Time.deltaTime + 1);

        if (tiempoTotal > tiempoSimulacion) {
            pausa = true;
            tiempoTotal = tiempoSimulacion;
        } else {
            if (!pausa) tiempoTotal += delta;
        }

    }

    /*private Vector2 getWorldPoint(Vector2 screenPoint) {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(screenPoint), out hit);
        return hit.point;
    }*/

    void ZoomCamera(float offset, float speed) {
        if (offset == 0) {
            return;
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }

    private bool CamaraDentroDeBorde(int eje) {
        if (eje == 0) {
            float x = cam.transform.position.x;
            float minx = bordes.transform.position.x - bordes.transform.lossyScale.x / 2;
            float maxx = bordes.transform.position.x + bordes.transform.lossyScale.x / 2;
            return (x > minx && x < maxx);
        } else {
            float z = cam.transform.position.z;
            float minz = bordes.transform.position.z - bordes.transform.lossyScale.z / 2;
            float maxz = bordes.transform.position.z + bordes.transform.lossyScale.z / 2;
            return (z > minz && z < maxz);
        }
    }

}
