using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Semaforo : MonoBehaviour
{

    public Material negro;
    public Material verde;
    public Material amarillo;
    public Material rojo;

    public GameObject bola_roja;
    public GameObject bola_amarilla;
    public GameObject bola_verde;

    public enum Color { Verde, Ambar, Rojo };

    void Start() {

        bola_roja.GetComponent<Renderer>().material = rojo;
        bola_verde.GetComponent<Renderer>().material = negro;
        bola_amarilla.GetComponent<Renderer>().material = negro;

    }


    void Update() {
    }

    public void CambiarColor(Color color){
        switch (color) {
            case Color.Verde:
                bola_roja.GetComponent<Renderer>().material = negro;
                bola_amarilla.GetComponent<Renderer>().material = negro;
                bola_verde.GetComponent<Renderer>().material = verde;
                break;
            case Color.Ambar:
                bola_roja.GetComponent<Renderer>().material = negro;
                bola_amarilla.GetComponent<Renderer>().material = amarillo;
                bola_verde.GetComponent<Renderer>().material = negro;
                break;
            default:
                bola_roja.GetComponent<Renderer>().material = rojo;
                bola_verde.GetComponent<Renderer>().material = negro;
                bola_amarilla.GetComponent<Renderer>().material = negro;
                break;
        }
    }
}
