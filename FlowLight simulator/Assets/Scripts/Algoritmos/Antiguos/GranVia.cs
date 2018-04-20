/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranVia : MonoBehaviour {

    private struct EnviarCoches {
        public Posicion entrada;
        public int numeroCoches;

        public EnviarCoches(Posicion entrada, int numeroCoches) : this() {
            this.entrada = entrada;
            this.numeroCoches = numeroCoches;
        }
    }

    private GameController controller;
    private Cruce cruce;

    //Variables usadas para pausar los semáforos a la vez que los coches
    private string proximaInvocacion; //Guarda la invocación a realizar
    private float tiempoFinInvocacion; //Guarda el tiempo en el que debería acabar la invocación

    //Variables usadas sólo para gestionar la GranVía
    //Asumimos que la calle principal se encuentra en la posición 0 del ciclo de semáforos y la secundaria en la posción 1.
    public bool primerCruceAvenida = false;  //Indica si este cruce es el primero de la avenida.
    public GranVia siguientePrincipal = null; //Indica el siguiente cruce en el sentido de la calle principal. Si está a null, indica que es el último cruce de la avenida en dicho sentido
    public GranVia siguienteSecundaria = null; //Indica el siguiente cruce en el sentido de la calle secundaria. Si está a null, indica que es el último cruce que atraviesan los coches que van recto por la calle secundaria.
    public GranVia anteriorSecundaria = null; //Indica el cruce anterior en el sentido de la calle secundaria.
    private float esperaEntrePrincipales = 5f;
    private float esperaEntreSecundarias = 5f;

    //Variables que cambian en cada ciclo (también para gestionar la GranVía)
    private float tiempoCerrarPrincipalPorCoches = 0f; //Indica el momento en el que cerraríamos la principal teniendo en cuenta sólo el número de cohes
    private float tiempoCerrarPrincipalPorOla = 0f; //Indica el momento en el que cerraríamos la principal siguiendo la ola de cierres
    private float tiempoCerrarPrincipalPorSecundaria = 1f; //Indica el momento en el que cerraríamos la principal por esperar a que abra la secundaria que me envía coches
    private bool acabePrincipal = false;

    // Use this for initialization
    void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();
        cruce = GetComponent<Cruce>();

        if (controller.semaforos == GameController.Semaforos.GranVia) {
            cruce.CambiarSemaforos(cruce.cicloSemaforos[1], Semaforo.Color.Rojo);
            Invocar("AbrirPrincipal", cruce.tiempoPorCoche);
        } else {
            tiempoFinInvocacion = controller.tiempoSimulacion + 100f;
        }

    }

    // Update is called once per frame
    void Update() {

        if (controller.semaforos == GameController.Semaforos.GranVia) {

            //Para poder cerrar la principal, debe estar abierta && debe haber cerrado la anterior principal && debe haber abierto la secundaria que me envía coches (si hay una secundaria que me envía coches) (después del &&)
            if (!acabePrincipal && (tiempoCerrarPrincipalPorOla != 0f || primerCruceAvenida) && (siguientePrincipal == null || siguientePrincipal.cruce.posicionSemaforos == 0)) {
                if (Mathf.Max(tiempoCerrarPrincipalPorCoches, tiempoCerrarPrincipalPorOla, tiempoCerrarPrincipalPorSecundaria) <= controller.tiempoTotal) AcabePrincipal();
            }

            if (acabePrincipal && cruce.posicionSemaforos == 0 && (siguienteSecundaria != null || anteriorSecundaria.cruce.posicionSemaforos == 1)) {
                CambiarASecundaria();
            }

            if (cruce.posicionSemaforos == 1 && siguienteSecundaria == null && anteriorSecundaria.cruce.posicionSemaforos == 0 && (proximaInvocacion == null || (!proximaInvocacion.Equals("CerrarSecundaria") && !proximaInvocacion.Equals("AbrirPrincipal")))) {
                CambiarAPrincipal();
            }

            if (controller.tiempoTotal >= tiempoFinInvocacion && controller.semaforos == GameController.Semaforos.GranVia && proximaInvocacion != null) {
                Invoke(proximaInvocacion, 0f);
                proximaInvocacion = null;
            }

        }

    }

    private void Invocar(string funcion, float tiempo) {
        proximaInvocacion = funcion;
        tiempoFinInvocacion = controller.tiempoTotal + tiempo;
    }

    public void CambiarAPrincipal() {

        //TODO pensar qué hacer si la principal está vacía
        cruce.CambiarSemaforos(cruce.cicloSemaforos[1], Semaforo.Color.Ambar);
        Invocar("CerrarSecundaria", cruce.tiempoAmbar);

    }

    public void CerrarSecundaria() {
        
        cruce.CambiarSemaforos(cruce.cicloSemaforos[1], Semaforo.Color.Rojo);
        Invocar("AbrirPrincipal", cruce.tiempoAmbar);

    }

    public void AbrirPrincipal() {

        cruce.posicionSemaforos = 0;
        cruce.CambiarSemaforos(cruce.cicloSemaforos[0], Semaforo.Color.Verde);

        acabePrincipal = false;

        //Calculamos cuándo cerraríamos por coches
        foreach (Posicion entrada in cruce.cicloSemaforos[0].verdes) {
            if (entrada.traficoAlRecibir == 0) entrada.traficoAlRecibir = entrada.cochesAcercandose.Count;
            float tiempoCierre = cruce.tiempoPorCoche * (entrada.traficoAlRecibir + entrada.traficoRecibido) + controller.tiempoTotal;
            if (tiempoCierre > tiempoCerrarPrincipalPorCoches) tiempoCerrarPrincipalPorCoches = tiempoCierre;
        }

        //Avisamos al siguiente de la principal cúantos coches se le acercan. Éste deberá contar sus coches y sumarles estes, para así saber cuántos coches tendrá cuando abra.
        List<EnviarCoches> coches = new List<EnviarCoches>();
        foreach (Posicion entrada in cruce.cicloSemaforos[0].verdes) {
            foreach (Posicion.Salida salida in entrada.siguientesPosiciones) {
                if (salida.posicion.SiguienteCruce() != null && salida.posicion.SiguienteCruce().GetComponent<GranVia>() == siguientePrincipal) coches.Add(new EnviarCoches(salida.posicion.SiguienteCruce(), (int)Mathf.Ceil(entrada.cochesAcercandose.Count * (salida.probabilidad / 100f))));
            }
        }
        if (siguientePrincipal != null) siguientePrincipal.EnviarCochesPrincipal(coches);

    }

    public void AcabePrincipal() {
        
        if (siguientePrincipal != null) siguientePrincipal.AvisarCierrePrincipal(Vector3.Distance(transform.position, siguientePrincipal.transform.position) / 12f);
        acabePrincipal = true;

    }

    private void EnviarCochesPrincipal(List<EnviarCoches> coches) {
        
        //Antes de nada, propagamos el mensaje por la avenida
        if (siguientePrincipal != null) {
            List<EnviarCoches> propagarCoches = new List<EnviarCoches>();
            foreach (EnviarCoches coche in coches) {
                foreach (Posicion.Salida salida in coche.entrada.siguientesPosiciones) {
                    if (salida.posicion.SiguienteCruce() != null && salida.posicion.SiguienteCruce().GetComponent<GranVia>() == siguientePrincipal) propagarCoches.Add(new EnviarCoches(salida.posicion.SiguienteCruce(), coche.numeroCoches));
                }
            }
            siguientePrincipal.EnviarCochesPrincipal(propagarCoches);
        }

        //Si aún no recibimos ningún coche, guardamos los coches al recibir. Los coches recibidos lo aumentamos siempre
        foreach (EnviarCoches coche in coches) {
            if (coche.entrada.traficoAlRecibir == 0) coche.entrada.traficoAlRecibir = coche.entrada.cochesAcercandose.Count;
                coche.entrada.traficoRecibido += coche.numeroCoches;
        }
        
        //Si el semáforo está en verde, calculamos cuándo cerraríamos por coches
        if (cruce.posicionSemaforos == 0) {
            foreach (Posicion entrada in cruce.cicloSemaforos[0].verdes) {
                float tiempoCierre = cruce.tiempoPorCoche * (entrada.traficoAlRecibir + entrada.traficoRecibido) + controller.tiempoTotal;
                if (tiempoCierre > tiempoCerrarPrincipalPorCoches) tiempoCerrarPrincipalPorCoches = tiempoCierre;
            }
        }

    }

    public void AvisarCierrePrincipal(float espera) {
        tiempoCerrarPrincipalPorOla = controller.tiempoTotal + espera;
    }

    public void CambiarASecundaria() {
        
        //TODO pensar qué hacer si la secundaria está vacía

        cruce.posicionSemaforos = 1;

        foreach (Posicion entrada in cruce.cicloSemaforos[0].verdes) {
            entrada.traficoAlRecibir = 0;
            entrada.traficoRecibido = 0;
        }
        tiempoCerrarPrincipalPorCoches = 0f;
        tiempoCerrarPrincipalPorOla = 0f;
        tiempoCerrarPrincipalPorSecundaria = 0f;

        cruce.CambiarSemaforos(cruce.cicloSemaforos[0], Semaforo.Color.Ambar);
        Invocar("CerrarPrincipal", cruce.tiempoAmbar);

    }

    public void CerrarPrincipal() {

        cruce.CambiarSemaforos(cruce.cicloSemaforos[0], Semaforo.Color.Rojo);
        Invocar("AbrirSecundaria", cruce.tiempoAmbar);

    }

    public void AbrirSecundaria() {

        if (siguienteSecundaria != null) siguienteSecundaria.AvisarAperturaSecundaria(Vector3.Distance(transform.position, siguienteSecundaria.transform.position) / 12f);

        cruce.CambiarSemaforos(cruce.cicloSemaforos[1], Semaforo.Color.Verde);

        //Calculamos el tiempo que necesitamos mantener abierto el cruce
        int mayorTrafico = 0;
        foreach (Posicion entrada in cruce.cicloSemaforos[cruce.posicionSemaforos].verdes) {
            int numeroCoches = entrada.cochesAcercandose.Count + entrada.traficoRecibido;
            if (numeroCoches > mayorTrafico) mayorTrafico = numeroCoches;
        }
        if (siguienteSecundaria != null) Invocar("CambiarAPrincipal", cruce.tiempoPorCoche * mayorTrafico);

        //TODO avisar al siguiente de la secundaria cúantos coches se le acercan. Éste deberá contar sus coches y sumarles estes, para así saber cuántos coches tendrá cuando abra. Además, guardará este instante para restarle el tiempo que pase al tiempo entre cruces.

    }

    public void AvisarAperturaSecundaria(float espera) {
        tiempoCerrarPrincipalPorSecundaria = controller.tiempoTotal + espera;
    }

    public void SigueAbierto(Posicion miEntrada, int identificadorMensaje, int numeroDeCoches) {

        if (numeroDeCoches != 0) { //Si nos envían 0 coches, nada tiene sentido
            if (cruce.cicloSemaforos[cruce.posicionSemaforos].verdes.Contains(miEntrada) && proximaInvocacion.Equals("PonerEnAmbar")) { //Si el semáforo de esa entrada está en verde
                tiempoFinInvocacion += numeroDeCoches * cruce.tiempoPorCoche + Mathf.Max(0, esperaEntrePrincipales - (tiempoFinInvocacion - controller.tiempoTotal));
            } else {
                miEntrada.traficoRecibido += numeroDeCoches;
            }
        }

    }

    public void SePusoEnRojo(Posicion miEntrada) {

        miEntrada.traficoRecibido = 0;

    }

}*/
