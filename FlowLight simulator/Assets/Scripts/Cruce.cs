using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cruce : MonoBehaviour {


    #region Structs
    //Struct que contiene la información de una posición del ciclo de semáforos
    [System.Serializable]
    public struct PosicionCiclo {
        public List<Posicion> verdes;
        public float tiempoVerde;
    }

    #endregion

    //Variables
    private GameController controller;
    [Header ("Ciclo de semáforos")]
    public List<PosicionCiclo> cicloSemaforos;
    [Space]

    [Header("Otras variables")]
    public int posicionSemaforos = 0;
    public float esperaInicial = 0f;
    public float tiempoPorCoche = 0.8f; //Indica el tiempo que mantenemos el semáforo en verde por cada coche que hay esperando
    public float tiempoAmbar = 1.36f; //Tiempo que está el semáforo en ámbar (el tiempo en rojo es igual)

    public Cruce esclavo = null; //Si se mete un cruce en esta variable, este cruce forzará que el ciclo del esclavo sea igual al suyo. Es importante que ambos cruces tengan el mismo número de posiciones en el ciclo.
    public bool soyEsclavo = false; //Indica si este cruce es esclavo. Si no lo es, no recibirá órdenes de ningún maestro
    public float tiempoRetardoEsclavo = 1.2f; //Indica el tiempo de retardo que hay entre que cambia el maestro y que cambia el esclavo.
    private Semaforo.Color colorEsclavo;
    private int posicionEsclavo;

    //Variables usadas para pausar los semáforos a la vez que los coches
    private string proximaInvocacion; //Guarda la invocación interrumpida
    private float tiempoFinInvocacion; //Guarda el tiempo que faltaba para que acabara al pausar
    private float tiempoInicioInvocacion; //Guarda el tiempo en el que empezó la invocación

    //Variables para escoger repartos
    public int posicionPrincipal = 0;
    private int reparto = 50; //Reparto de tiempos actual

    //Métodos

    private void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();

        for (int i = 0; i < cicloSemaforos.Count; i++) {
            CambiarSemaforos(i, Semaforo.Color.Rojo);
        }

    }

    // Update is called once per frame
    void Update() {

        if (controller.tiempoTotal >= tiempoFinInvocacion && proximaInvocacion != null) {
            Invoke(proximaInvocacion, 0f);
            proximaInvocacion = null;
        }
        if (controller.semaforos == GameController.Semaforos.Ciclo) {
            soyEsclavo = false;
        }

        if (controller.densidadDelTrafico >= Inicio.DensidadTrafico.Mucha && reparto != 1) ActualizarReparto(1);
        else if (controller.densidadDelTrafico <= Inicio.DensidadTrafico.Bastante && reparto != 0) ActualizarReparto(0);

    }

    private void ActualizarReparto(int reparto) {
        float tiempoVerdePrincipal, tiempoVerdeSecundaria;
        if (reparto == 0) {
            tiempoVerdePrincipal = 24.5f;
            tiempoVerdeSecundaria = 9.5f;
        } else {
            tiempoVerdePrincipal = 30.9f;
            tiempoVerdeSecundaria = 12.3f;
        }
        for (int i = 0; i < cicloSemaforos.Count; i++) {
            if (i == posicionPrincipal) {
                PosicionCiclo posicion = cicloSemaforos[i];
                posicion.tiempoVerde = tiempoVerdePrincipal;
                cicloSemaforos[i] = posicion;
            } else {
                PosicionCiclo posicion = cicloSemaforos[i];
                posicion.tiempoVerde = tiempoVerdeSecundaria;
                cicloSemaforos[i] = posicion;
            }
        }
    }

    private void Invocar(string funcion, float tiempo) {
        proximaInvocacion = funcion;
        try {
            tiempoInicioInvocacion = controller.tiempoTotal;
        } catch (NullReferenceException) {}
        tiempoFinInvocacion = tiempoInicioInvocacion + tiempo;
    }

    public int PosicionConMasTrafico() {
        int posicionConMasTrafico = 0;
        int mayorTrafico = 0;
        int posicionActual = 0;
        foreach (PosicionCiclo posicion in cicloSemaforos) {
            int trafico = CalcularTraficoPosicion(posicion);
            if (trafico >= mayorTrafico) {
                mayorTrafico = trafico;
                posicionConMasTrafico = posicionActual;
            }
            posicionActual++;
        }
        return posicionConMasTrafico;
    }

    public int CalcularTraficoPosicion(PosicionCiclo posicionCiclo) {
        int trafico = 0;
        foreach (Posicion entrada in posicionCiclo.verdes) {
                trafico += entrada.cochesAcercandose.Count;
        }
        return trafico;
    }

    public bool PuedoEntrar(Posicion entrada, Posicion salida) {

        bool puedeEntrar = true;
        foreach (Movimiento coche in salida.cochesAcercandose) {
            if (coche.velocidadActual < 5f) puedeEntrar = false;
        }
        if (salida.SiguientePosicion().ultimoCocheEnAcercarse != null && salida.SiguientePosicion().ultimoCocheEnAcercarse.velocidadActual < 5f && Vector3.Distance(salida.SiguientePosicion().ultimoCocheEnAcercarse.transform.position, salida.transform.position) < 3f) puedeEntrar = false;
        return puedeEntrar;

    }

    public void CambiarSemaforos(int posicion, Semaforo.Color color) {

        if (!soyEsclavo) {
            if (esclavo != null) esclavo.ForzarEsclavo(posicion, color);
            foreach (Posicion entrada in cicloSemaforos[posicion].verdes) {
                entrada.CambiarSemaforo(color);
            }
        }

    }

    public void ForzarEsclavo(int posicion, Semaforo.Color color) {

        if (soyEsclavo) {
            if (tiempoRetardoEsclavo > 0f) {
                posicionEsclavo = posicion;
                colorEsclavo = color;
                Invocar("CambiarEsclavo", tiempoRetardoEsclavo);
            } else {
                foreach (Posicion entrada in cicloSemaforos[posicion].verdes) {
                    entrada.CambiarSemaforo(color);
                }
            }
        }

    }

    private void CambiarEsclavo() {

        if (soyEsclavo) {
            foreach (Posicion entrada in cicloSemaforos[posicionEsclavo].verdes) {
                entrada.CambiarSemaforo(colorEsclavo);
            }
        }

    }

}
