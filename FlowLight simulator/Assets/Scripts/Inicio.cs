using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inicio : MonoBehaviour {

    public enum DensidadTrafico { DeNoche, Poca, Normal, Bastante, Mucha, HoraPunta, Personalizada}

    [System.Serializable]
    public struct Densidad {
        public DensidadTrafico densidad;
        [Range(0,100)]
        public float velocidadGeneracion;
    }

    [System.Serializable]
    public struct Coche {
        public GameObject coche;
        public GameObject destino;
    }

    //Variables
    private GameController controller; //Script del GameController, donde se encuentran las variables globales
    [Range(0, 100)]
    public float velocidadGeneracion = 10f; //Numero de coches generados por minuto
    public List<Coche> coches;
    private Movimiento ultimoCocheCreado;
    public Posicion posicionGeneracion; //Indica la posicion en la que se generan los coches
    private float ultimaInvocacion = 0f; //Indica la última vez que se llamó a GenerarCoche()
    public List<Densidad> velocidadesDeGeneracion;

    // Use this for initialization
    void Start () {

        float angulo = transform.rotation.eulerAngles.y - 180;
        if (angulo < 0f) angulo += 360f;
        controller = GameObject.Find("GameController").GetComponent<GameController>(); //Obtenemos el script del GameController;

    }
	
	// Update is called once per frame
	void Update () {

        foreach (Densidad densidad in velocidadesDeGeneracion) {
            if (densidad.densidad == DensidadTrafico.Personalizada) break;
            if (densidad.densidad == controller.densidadDelTrafico) velocidadGeneracion = densidad.velocidadGeneracion;
        }

        if (controller.tiempoTotal > ultimaInvocacion + 1f) {
            GenerarCoche();
            ultimaInvocacion = controller.tiempoTotal;
        }
    }

    void GenerarCoche() {
        
        if ((ultimoCocheCreado == null || ( !controller.pausa && Vector3.Distance(transform.position, ultimoCocheCreado.transform.position) > 3f)) && Random.Range(0f, 100f) < (velocidadGeneracion)) {  //1*velocidadGeneración de cada 600 veces.
            
            
            GameObject coche = Instantiate(coches[Random.Range(0, coches.Count -1)].coche) as GameObject;
            Movimiento cocheCreado = coche.GetComponent<Movimiento>();
            cocheCreado.Inicializar(posicionGeneracion, posicionGeneracion.SiguientePosicion(), ultimoCocheCreado);
            ultimoCocheCreado = cocheCreado;

        }

    }

}
