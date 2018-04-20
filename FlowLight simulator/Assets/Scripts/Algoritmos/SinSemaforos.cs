using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinSemaforos : MonoBehaviour {

    private GameController controller;
    private Cruce cruce;

    public bool muchoTrafico = false;

    // Use this for initialization
    void Start() {

        controller = GameObject.Find("GameController").GetComponent<GameController>();
        cruce = GetComponent<Cruce>();

        //Inicializamos el ciclo de los semáforos
        if (controller.semaforos == GameController.Semaforos.SinSemaforos) {
            Invoke("CambiarSemaforos", 0.01f);
        }

    }

    void CambiarSemaforos() {
        
        for (int i=0; i<cruce.cicloSemaforos.Count; i++) {
            cruce.CambiarSemaforos(i, Semaforo.Color.Verde);
        }

    }

    public bool PuedoPasar(Posicion miEntrada, Posicion miSalida) {

        foreach (Posicion.Salida salida in miEntrada.siguientesPosiciones) {
            if (salida.posicion == miSalida) {
                foreach(Posicion.Maniobra conflicto in salida.maniobrasConflictivas) {
                    /*if (!muchoTrafico) {
                        if (conflicto.entrada.prioridad < miEntrada.prioridad && miEntrada.CochesEstorbando(miSalida) > 0) return false;
                    }else {
                        if (conflicto.entrada.cochesAcercandose[0].TiempoEsperando() > miEntrada.cochesAcercandose[0].TiempoEsperando() || (conflicto.entrada.cochesAcercandose[0].TiempoEsperando() == 0f && conflicto.entrada.prioridad > miEntrada.prioridad)) return false;
                    }*/
                }
                return true;
            }
        }
        return true;

    }

}
