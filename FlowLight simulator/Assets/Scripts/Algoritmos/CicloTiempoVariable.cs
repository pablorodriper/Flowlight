using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CicloTiempoVariable : MonoBehaviour {

    private GameController controller;
    private Cruce cruce;

    //Variables usadas para pausar los semáforos a la vez que los coches
    private string proximaInvocacion; //Guarda la invocación a realizar
    private float tiempoFinInvocacion; //Guarda el tiempo en el que debería acabar la invocación
    private float tiempoInicioInvocacion; //Guarda el tiempo en el que empezó la invocación

    // Use this for initialization
    void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();
        cruce = GetComponent<Cruce>();

        if (controller.semaforos == GameController.Semaforos.CicloTiempoVariable) {
            cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
            Invocar("PonerEnAmbar", cruce.tiempoPorCoche);
        } else {
            tiempoFinInvocacion = controller.tiempoSimulacion + 100f;
        }

    }

    // Update is called once per frame
    void Update() {

        if (controller.tiempoTotal >= tiempoFinInvocacion && controller.semaforos == GameController.Semaforos.CicloTiempoVariable) {
            Invoke(proximaInvocacion, 0f);
        }

    }

    private void Invocar(string funcion, float tiempo) {
        proximaInvocacion = funcion;
        tiempoInicioInvocacion = controller.tiempoTotal;
        tiempoFinInvocacion = tiempoInicioInvocacion + tiempo;
    }

    //Cuando queremos cambiar de semáforo en verde, llamamos a esta función, que hace todo el ciclo de poner en ámbar, rojo, y verde el que toque en cada caso
    public void PonerEnAmbar() {

        //Si resto de posiciones no tienen tráfico, no cambiamos
        bool cambiar = false;
        for (int posicion = 0; posicion < cruce.cicloSemaforos.Count; posicion++) {
            if (posicion != cruce.posicionSemaforos && cruce.CalcularTraficoPosicion(cruce.cicloSemaforos[posicion]) > 0) cambiar = true;
        }
        if (cruce.esclavo != null) {
            for (int posicion = 0; posicion < cruce.esclavo.cicloSemaforos.Count; posicion++) {
                if (posicion != cruce.posicionSemaforos && cruce.esclavo.CalcularTraficoPosicion(cruce.esclavo.cicloSemaforos[posicion]) > 0) cambiar = true;
            }
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

        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Rojo);
        cruce.posicionSemaforos = (++cruce.posicionSemaforos) % cruce.cicloSemaforos.Count;
        //Si la posición que toca no tiene tráfico, pasamos a la siguiente
        if (cruce.CalcularTraficoPosicion(cruce.cicloSemaforos[cruce.posicionSemaforos]) == 0 && (cruce.esclavo == null || cruce.esclavo.CalcularTraficoPosicion(cruce.esclavo.cicloSemaforos[cruce.posicionSemaforos]) == 0)) {
            if (name.Equals("Cruce B3")) Debug.Log(cruce.posicionSemaforos);
            Invocar("PonerEnRojo", 0.01f);
            return;
        }
        //En caso contrario, ponemos en verde el que toque
        Invocar("PonerEnVerde", cruce.tiempoAmbar);
    }

    public void PonerEnVerde() {

        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
        int mayorTrafico = 0;
        foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionSemaforos].verdes) {
            int numeroCoches = entrada.cochesAcercandose.Count;
            if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
        }
        if (cruce.esclavo != null) {
            foreach (Posicion entrada in cruce.esclavo.cicloSemaforos[cruce.posicionSemaforos].verdes) {
                int numeroCoches = entrada.cochesAcercandose.Count;
                if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
            }
        }

        Invocar("PonerEnAmbar", cruce.tiempoPorCoche * mayorTrafico);
    }

}
