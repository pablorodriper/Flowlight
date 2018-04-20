using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariosCruces : MonoBehaviour {

    private GameController controller;
    private Cruce cruce;

    //Variables para gestionar varios cruces
    private int idMensajesEnviados = 0;
    private float esperaEntreCruces = 5f;

    //Variables usadas para pausar los semáforos a la vez que los coches
    private string proximaInvocacion; //Guarda la invocación a realizar
    [SerializeField] private float tiempoFinInvocacion; //Guarda el tiempo en el que debería acabar la invocación

    // Use this for initialization
    void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();
        cruce = GetComponent<Cruce>();

        if (controller.semaforos == GameController.Semaforos.VariosCruces) {
            cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
            Invocar("PonerEnAmbar", cruce.tiempoPorCoche);
        } else {
            tiempoFinInvocacion = controller.tiempoSimulacion + 100f;
        }

    }

    // Update is called once per frame
    void Update() {

        if (controller.tiempoTotal >= tiempoFinInvocacion) {
            Invoke(proximaInvocacion, 0f);
        }

    }

    private void Invocar(string funcion, float tiempo) {
        proximaInvocacion = funcion;
        tiempoFinInvocacion = controller.tiempoTotal + tiempo;
    }

    //Cuando queremos cambiar de semáforo en verde, llamamos a esta función, que hace todo el ciclo de poner en ámbar, rojo, y verde el que toque en cada caso
    public void PonerEnAmbar() {

        //Si resto de posiciones no tienen tráfico, no cambiamos
        bool cambiar = false;
        for (int posicion = 0; posicion < cruce.cicloSemaforos.Count; posicion++) {
            if (posicion != cruce.posicionSemaforos && cruce.CalcularTraficoPosicion(cruce.cicloSemaforos[posicion]) > 0) cambiar = true;
        }
        if (cambiar == false) {
            Invocar("PonerEnAmbar", 0.01f);
            return;
        }
        //En cualquier otro caso, abrimos el siguiente semáforo
        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Ambar);
        Invocar("PonerEnRojo", cruce.tiempoAmbar);

    }

    public void PonerEnRojo() {

        //Avisamos a los cruces que corresponda de los coches que se les avecinan
        foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionSemaforos].verdes) {
            foreach (Posicion.Salida salida in entrada.siguientesPosiciones) {
                //if (salida.prioritaria) {
                if (salida.posicion.SiguienteCruce() != null) { //Si la salida desemboca en un fin, SiguienteCruce() devolverá null
                    salida.posicion.SiguienteCruce().cruce.GetComponent<VariosCruces>().SePusoEnRojo(salida.posicion.SiguienteCruce()); //Incrementamos idMensajesEnviados para que empiece en 1. Si recibimos un 0 lo ignoramos
                }
                //}
            }
        }

        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Rojo);
        cruce.posicionSemaforos = ++cruce.posicionSemaforos % cruce.cicloSemaforos.Count;

        //Si la posición que toca no tiene tráfico, pasamos a la siguiente
        if (cruce.CalcularTraficoPosicion(cruce.cicloSemaforos[cruce.posicionSemaforos]) == 0) {
            Invocar("PonerEnRojo", 0.001f);
            return;
        }
        //En caso contrario, ponemos en verde el que toque
        Invocar("PonerEnVerde", cruce.tiempoAmbar);
    }

    public void PonerEnVerde() {
        
        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde); //Ponemos en verde los semáforos físicos que correspondan

        //Avisamos a los cruces que corresponda de los coches que se les avecinan
        foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionSemaforos].verdes) {
            foreach (Posicion.Salida salida in entrada.siguientesPosiciones) {
                //if (salida.prioritaria) {
                if (salida.posicion.SiguienteCruce() != null) { //Si la salida desemboca en un fin, SiguienteCruce() devolverá null
                    salida.posicion.SiguienteCruce().cruce.GetComponent<VariosCruces>().EnviarCoches(salida.posicion.SiguienteCruce(), ++idMensajesEnviados, (int)Mathf.Ceil(entrada.cochesAcercandose.Count * (salida.probabilidad / 100f))); //Incrementamos idMensajesEnviados para que empiece en 1. Si recibimos un 0 lo ignoramos
                }
                //}
            }
        }

        //Calculamos el tiempo que tenemos que mantener abierto el cruce
        int mayorTrafico = 0;
        foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionSemaforos].verdes) {
            int numeroCoches = entrada.cochesAcercandose.Count + entrada.traficoRecibido;
            if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
        }
        Invocar("PonerEnAmbar", cruce.tiempoPorCoche * mayorTrafico);
    }

    public void EnviarCoches(Posicion miEntrada, int identificadorMensaje, int numeroDeCoches) {
        
        if (numeroDeCoches != 0) { //Si nos envían 0 coches, nada tiene sentido
            if (cruce.cicloSemaforos[cruce.posicionSemaforos].verdes.Contains(miEntrada) && proximaInvocacion.Equals("PonerEnAmbar")) { //Si el semáforo de esa entrada está en verde
                tiempoFinInvocacion += numeroDeCoches * cruce.tiempoPorCoche + Mathf.Max(0, esperaEntreCruces - (tiempoFinInvocacion - controller.tiempoTotal));
                foreach (Posicion.Salida salida in miEntrada.siguientesPosiciones) {
                    //if (salida.prioritaria) {
                    if (salida.posicion.SiguienteCruce() != null) { //Si la salida desemboca en un fin, SiguienteCruce() devolverá null
                        salida.posicion.SiguienteCruce().cruce.GetComponent<VariosCruces>().SigueAbierto(salida.posicion.SiguienteCruce(), ++idMensajesEnviados, numeroDeCoches); //Incrementamos idMensajesEnviados para que empiece en 1. Si recibimos un 0 lo ignoramos
                    }
                    //}
                }
            } else {
                miEntrada.traficoRecibido = numeroDeCoches;
            }
        }

    }

    public void SigueAbierto(Posicion miEntrada, int identificadorMensaje, int numeroDeCoches) {
        
        if (numeroDeCoches != 0) { //Si nos envían 0 coches, nada tiene sentido
            if (cruce.cicloSemaforos[cruce.posicionSemaforos].verdes.Contains(miEntrada) && proximaInvocacion.Equals("PonerEnAmbar")) { //Si el semáforo de esa entrada está en verde
                tiempoFinInvocacion += numeroDeCoches * cruce.tiempoPorCoche + Mathf.Max(0, esperaEntreCruces - (tiempoFinInvocacion - controller.tiempoTotal));
            } else {
                miEntrada.traficoRecibido += numeroDeCoches;
            }
        }

    }

    public void SePusoEnRojo(Posicion miEntrada) {

        miEntrada.traficoRecibido = 0;

    }

}
