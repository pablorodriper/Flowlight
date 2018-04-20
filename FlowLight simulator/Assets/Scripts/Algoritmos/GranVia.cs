using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranVia : MonoBehaviour {

    private GameController controller;
    private Cruce cruce;

    //Variables usadas para pausar los semáforos a la vez que los coches
    private string proximaInvocacion; //Guarda la invocación a realizar
    private float tiempoFinInvocacion; //Guarda el tiempo en el que debería acabar la invocación
    private float tiempoInicioInvocacion; //Guarda el tiempo en el que empezó la invocación

    //Variables específicas del algoritmo GranVia
    public bool primeraUnSentido = false;
    public GranVia siguientePrincipal;
    private float tiempoCierrePrincipal = 0f;
    public Dictionary<string, int> traficos = new Dictionary<string, int>();
    public Dictionary<string, bool> verdesPrincipal = new Dictionary<string, bool>();
    private float tiempoUltimoVerde = 0f;
    private string calleMayorTrafico;//TODO eliminar esto

    // Use this for initialization
    void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();
        cruce = GetComponent<Cruce>();

        if (controller.semaforos == GameController.Semaforos.GranVia) {
            cruce.posicionSemaforos = (cruce.posicionPrincipal + 1) % cruce.cicloSemaforos.Count;
            cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
            Invocar("AcabarPosicion", 2f);
        } else {
            tiempoFinInvocacion = controller.tiempoSimulacion + 100f;
        }

    }

    // Update is called once per frame
    void Update() {

        if (controller.semaforos == GameController.Semaforos.GranVia) {
            if (!cruce.soyEsclavo) {
                int mayorTrafico = 0;
                if (!primeraUnSentido) {
                    foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionPrincipal].verdes) {
                        int numeroCoches = entrada.cochesAcercandose.Count;
                        if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
                    }
                    foreach (Posicion entrada in cruce.esclavo.cicloSemaforos[cruce.posicionPrincipal].verdes) {
                        int numeroCoches = entrada.cochesAcercandose.Count;
                        if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
                    }
                }
                siguientePrincipal.AvisarTrafico(mayorTrafico, tiempoUltimoVerde, cruce.posicionSemaforos == cruce.posicionPrincipal, name);
            }

            if (controller.tiempoTotal >= tiempoFinInvocacion && controller.semaforos == GameController.Semaforos.GranVia) {
                Invoke(proximaInvocacion, 0f);
            }
        }

    }

    private void Invocar(string funcion, float tiempo) {
        proximaInvocacion = funcion;
        tiempoInicioInvocacion = controller.tiempoTotal;
        tiempoFinInvocacion = tiempoInicioInvocacion + tiempo;
    }

    //Cuando queremos cambiar de semáforo en verde, llamamos a esta función, que hace todo el ciclo de poner en ámbar, rojo, y verde el que toque en cada caso
    public void AcabarPosicion() {

        if (cruce.posicionSemaforos != cruce.posicionPrincipal) {
            //Si resto de posiciones no tienen tráfico, no cambiamos
            bool cambiar = false;
            for (int posicion = 0; posicion < cruce.cicloSemaforos.Count; posicion++) {
                if (posicion != cruce.posicionSemaforos && cruce.CalcularTraficoPosicion(cruce.cicloSemaforos[posicion]) > 0) cambiar = true;
                if (posicion == cruce.posicionPrincipal) cambiar = true; //TODO ver si se puede cambiar esto para sólo abrirle a la principal si es la siguiente
            }
            if (cambiar == false) {
                Invocar("AcabarPosicion", 0.01f);
                return;
            }
            //En cualquier otro caso, abrimos el siguiente semáforo
            PonerEnAmbar();
        } else { //Estamos en la posición principal
            /*if (primeraUnSentido) {
                siguientePrincipal.AvisarCierrePrincipal(controller.tiempoTotal, name);
            }*/
            PonerEnAmbar();
        }

    }

    public void PonerEnAmbar() {

        if (cruce.posicionSemaforos == cruce.posicionPrincipal) {
            int mayorTraficoSecundaria = 0;
            foreach (Posicion entrada in cruce.cicloSemaforos[1].verdes) {
                int numeroCoches = entrada.cochesAcercandose.Count;
                if (numeroCoches > mayorTraficoSecundaria) mayorTraficoSecundaria = numeroCoches;
            }
            if (cruce.esclavo != null) {
                foreach (Posicion entrada in cruce.esclavo.cicloSemaforos[1].verdes) {
                    int numeroCoches = entrada.cochesAcercandose.Count;
                    if (numeroCoches > mayorTraficoSecundaria) mayorTraficoSecundaria = numeroCoches;
                }
            }
            if (((MaximoTrafico() <= (controller.tiempoTotal - tiempoUltimoVerde) / 7 + 1  && MaximoTrafico() <= 7) || (MaximoTrafico() >= 14 && controller.tiempoTotal - tiempoUltimoVerde > 15f)) && mayorTraficoSecundaria > 0) {
                if (MaximoTrafico() >= 14 && MaximoTrafico() < 7) Debug.LogError("14");

                tiempoCierrePrincipal = 0f;
                cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Ambar);
                Invocar("PonerEnRojo", cruce.tiempoAmbar);
            } else {
                Invocar("PonerEnAmbar", 0.1f);
            }
        } else {
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

    }

    public void PonerEnRojo() {

        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Rojo);
        cruce.posicionSemaforos = (++cruce.posicionSemaforos) % cruce.cicloSemaforos.Count;

        //Ponemos en verde el semáforo que toque
        Invocar("PonerEnVerde", cruce.tiempoAmbar);
    }

    public void PonerEnVerde() {

        tiempoUltimoVerde = controller.tiempoTotal;
        cruce.CambiarSemaforos(cruce.posicionSemaforos, Semaforo.Color.Verde);
        int mayorTrafico = 0; float mayorDistancia = 0f;
        foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionSemaforos].verdes) {
            int numeroCoches = entrada.cochesAcercandose.Count;
            if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
            float distancia = 0f;
            if (entrada.cochesAcercandose.Count >= 1) distancia = Vector3.Distance(entrada.cochesAcercandose[0].transform.position, entrada.transform.position);
            if (distancia > mayorDistancia) mayorDistancia = distancia;
        }
        if (!cruce.soyEsclavo) {
            foreach (Posicion entrada in cruce.esclavo.cicloSemaforos[cruce.posicionSemaforos].verdes) {
                int numeroCoches = entrada.cochesAcercandose.Count;
                if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
                float distancia = 0f;
                if (entrada.cochesAcercandose.Count >= 1) distancia = Vector3.Distance(entrada.cochesAcercandose[0].transform.position, entrada.transform.position);
                if (distancia > mayorDistancia) mayorDistancia = distancia;
            }
        }

        Invocar("AcabarPosicion", cruce.tiempoPorCoche * mayorTrafico /*+ mayorDistancia / 12f*/);

    }

    public void AvisarCierrePrincipal(float tiempoCierre, string nombreEmisor) {

        //Pasamos la bola hasta que estén todos informados
        if (!nombreEmisor.Equals(name)) siguientePrincipal.AvisarCierrePrincipal(tiempoCierre, nombreEmisor);

        if (tiempoCierrePrincipal == 0f) tiempoCierrePrincipal = tiempoCierre;
        else {
            if (cruce.posicionSemaforos == cruce.posicionPrincipal && cruce.cicloSemaforos[cruce.posicionSemaforos].verdes[0].semaforoVerde == true) {
                Invocar("PonerEnAmbar", Mathf.Max(tiempoCierre, tiempoCierrePrincipal) - controller.tiempoTotal);
                tiempoCierrePrincipal = Mathf.Max(tiempoCierre, tiempoCierrePrincipal);
            }
        }

    }

    public void AvisarTrafico(int trafico, float tiempoVerde, bool verdePrincipal, string nombreEmisor) {

        //Pasamos la bola hasta que estén todos informados
        if (!nombreEmisor.Equals(name)) siguientePrincipal.AvisarTrafico(trafico, tiempoVerde, verdePrincipal, nombreEmisor);
        if (tiempoVerde > tiempoUltimoVerde) tiempoUltimoVerde = tiempoVerde;

        if (!traficos.ContainsKey(nombreEmisor)) traficos.Add(nombreEmisor, trafico);
        else traficos[nombreEmisor] = trafico;

        if (!verdesPrincipal.ContainsKey(nombreEmisor)) verdesPrincipal.Add(nombreEmisor, verdePrincipal);
        else verdesPrincipal[nombreEmisor] = verdePrincipal;

    }

    public int MaximoTrafico() {
        int maximoTrafico = 0;
        foreach (string calle in traficos.Keys) {
            if (traficos[calle] > maximoTrafico) maximoTrafico = traficos[calle];
            calleMayorTrafico = calle;
        }
        return maximoTrafico;
    }

    public bool AlgunaPrincipalEnRojo() {
        bool algunaEnRojo = false;
        foreach (string calle in traficos.Keys) {
            if (!verdesPrincipal[calle]) algunaEnRojo = true;
        }
        return algunaEnRojo;
    }

}
