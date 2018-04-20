using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Posicion : MonoBehaviour {


    #region structs
    /****************************  Structs  *******************************/

    [System.Serializable]
    public struct Salida {
        public Posicion posicion;
        public int probabilidad;
        public List<Maniobra> maniobrasConflictivas;
    }

    //Struct que contiene la información de una posición del ciclo de semáforos
    [System.Serializable]
    public struct Maniobra {
        public Posicion salida;
        public List<Posicion> entradasNoConflictivas;
    }
    #endregion



    /****************************  Variables  *******************************/

    //Tipo de Posicion
    public enum Tipo { Entrada, Medio, Salida }
    public enum Padre { Cruce, Inicio, Fin, Recta, Curva }

    //Variables del cruce (sólo las tienen aquellas posiciones que pertenecen a un cruce -> padre = Cruce)
    public List<Salida> siguientesPosiciones = new List<Salida>(); //Indica las posibles maniobras en caso de que haya cruce o bifurcación
    [Space]
    [Header("Otras variables")]
    public bool entradaSinSemaforos = false; //Si esta variable está a true, no se utilizan semáforos para esta entrada
    public Cruce cruce = null;
    public Semaforo semaforo;
    public List<Movimiento> cochesAcercandose;
    public int prioridad = 0; //Indica si una entrada tiene prioridad sobre las demás de su cruce. Menor número implica mayor prioridad

    //Variables empleadas por el algoritmo GranVia
    public int traficoAlRecibir;
    public int traficoRecibido;

    //Variables generales (todas las posiciones las tienen)
    [SerializeField] public bool semaforoVerde = true;
    public Tipo tipo;
    public Padre padre;
    [SerializeField] private Posicion siguientePosicion; //Indica la siguiente posición en caso de que no haya cruce ni bifurcación
    [HideInInspector] public Movimiento ultimoCocheEnAcercarse; //Indica el último coche que empezó a acercarse a esta posición
    private GameController controller;
    private SinSemaforos sinSemaforos;




    /******************************  Métodos  *******************************/

    private void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();
        
        if (padre == Padre.Cruce) {
            cruce = transform.parent.GetComponent<Cruce>();
            sinSemaforos = cruce.GetComponent<SinSemaforos>();
        }
        GetComponent<MeshRenderer>().enabled = false;

    }

    public Posicion SiguienteCruce() {

        Posicion aux = siguientePosicion;
        while (aux.padre != Posicion.Padre.Cruce && aux.padre != Posicion.Padre.Fin) {
            aux = aux.siguientePosicion;
        }
        if (aux.padre == Padre.Fin) return null;
        else return aux;

    }

    public Posicion SiguientePosicion() {

        if (tipo == Tipo.Entrada && padre == Padre.Cruce) return CalcularSalidaCruce();
        else return siguientePosicion;

    }

    private Posicion CalcularSalidaCruce() { //TODO cambiar por el de verdad

        int maximo = 0;
        foreach (Salida salida in siguientesPosiciones) {
            maximo += salida.probabilidad;
        }
        float numeroElegido = Random.Range(0, maximo);
        int acumulado = 0;
        Posicion ultimaPosicion = null; //Este valor inicial nunca se usa
        foreach (Salida salida in siguientesPosiciones) {
            ultimaPosicion = salida.posicion;
            acumulado += salida.probabilidad;
            if (numeroElegido <= acumulado) return salida.posicion;
        }
        Debug.LogError("No se ha encontrado una posición adecuada");
        return ultimaPosicion;

    }

    public void CambiarSemaforo(Semaforo.Color color) {

        if (semaforo != null) semaforo.CambiarColor(color);

        if (color == Semaforo.Color.Rojo) semaforoVerde = false;
        else semaforoVerde = true;

    }

    public void CocheAcercandose(Movimiento coche) {

        cochesAcercandose.Add(coche);

    }

    public bool SemaforoVerde(Posicion salida) {

        if (controller.semaforos == GameController.Semaforos.SinSemaforos || entradaSinSemaforos) return sinSemaforos.PuedoPasar(this, salida);
        else return semaforoVerde;

    }

    public void EntrarEnCruce(Movimiento coche) {

        //Añadimos el coche a la lista de coches que se acercan a la salida por la que va a salir
        cochesAcercandose.Add(coche);

    }

    public void SalirDeCruce(Movimiento coche) {

        cochesAcercandose.Remove(coche); //Eliminamos el coche que sale del cruce de la lista de coches que se acercan a la salida

    }

    public int CochesEstorbando(Posicion salidaObjetivo) {

        foreach (Salida salida in siguientesPosiciones) {
            if (salida.posicion == salidaObjetivo) {
                int cochesEstorbando = 0;
                foreach (Maniobra conflicto in salida.maniobrasConflictivas) {
                    cochesEstorbando += conflicto.salida.cochesAcercandose.Count;
                }
                return cochesEstorbando;
            }
        }
        return 0;

    }

}
