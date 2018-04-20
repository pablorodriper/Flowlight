using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ciclo : MonoBehaviour {

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

        //Inicializamos el ciclo de los semáforos
        if (controller.semaforos == GameController.Semaforos.Ciclo) {
            for (int i = 0; i < cruce.cicloSemaforos.Count; i++) {
                cruce.CambiarSemaforos(i, Semaforo.Color.Rojo);
            }
            cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
            Invocar("PonerEnAmbar", cruce.cicloSemaforos[cruce.posicionSemaforos].tiempoVerde + cruce.esperaInicial);
        } else {
            tiempoFinInvocacion = controller.tiempoSimulacion + 100f;
        }

    }

    // Update is called once per frame
    void Update() {

        if (controller.tiempoTotal >= tiempoFinInvocacion && proximaInvocacion != null) {
            Invoke(proximaInvocacion, 0f);
            proximaInvocacion = null;
        }

    }

    private void Invocar(string funcion, float tiempo) {
        proximaInvocacion = funcion;
        tiempoInicioInvocacion = tiempoFinInvocacion;
        tiempoFinInvocacion = tiempoInicioInvocacion + tiempo;
    }

    //Cuando queremos cambiar de semáforo en verde, llamamos a esta función, que hace todo el ciclo de poner en ámbar, rojo, y verde el que toque en cada caso
    public void PonerEnAmbar() {

        //Cambiamos los semáforos actuales a ámbar
        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Ambar);
        Invocar("PonerEnRojo", cruce.tiempoAmbar);

    }

    public void PonerEnRojo() {

        //Ponemos todo a rojo
        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Rojo);
        cruce.posicionSemaforos = ++cruce.posicionSemaforos % cruce.cicloSemaforos.Count;
        //Ponemos en verde el que toque (cuando toque)
        Invocar("PonerEnVerde", cruce.tiempoAmbar);
    }

    public void PonerEnVerde() {

        //Ponemos los semáforos correspondientes en verde
        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
        Invocar("PonerEnAmbar", cruce.cicloSemaforos[cruce.posicionSemaforos].tiempoVerde);
    }

}
