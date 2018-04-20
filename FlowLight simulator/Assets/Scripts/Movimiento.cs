using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movimiento : MonoBehaviour {

    //TODO hacer que giren bien (interpolar posicion incicial y final)

    #region Variables
    /****************************  Variables y definiciones  *******************************/

    //Definiciones
    private float velocidadCrucero = 12f; //Velocidad máxima del vehículo
    private float aceleracion = 12f; //Aceleración del vehículo
    private float decelarcion = 18f; //Capacidad de frenada del vehículo
    public enum Giro { Recto, Izquierda, Derecha }
    public enum Orientacion { Norte, Sur, Este, Oeste }
    
    //Variables
    public float velocidadActual = 5f; //Velocidad instantánea del vehículo
    public bool andar = true; //Indica si el vehículo debería moverse (true) o pararse (false)
    public bool verde = true; //Indica si el siguiente semáforo está en verde (true) o en rojo (false)
    public bool puedoEntrar = true; //Indica si puedo entrar en el siguiente cruce (si está saturado de coches no puedo entrar)
    public bool voyAChocar = false;
    private GameController controller; //Script del GameController, donde se encuentran las variables globales
    public Movimiento esperandoPor; //Indica el coche por el que estoy esperando
    public Posicion proximaSalida = null; //Indica la posición por la que saldré del siguiente cruce (sólo se usa para saber si puedo entrar en el siguiente cruce)
    public int motivoEspera = 0;

    //Variables para las estadísticas
    private int paradas = 0; //Indica el número de veces que paró en un semáforo
    private float tiempoEsperado = 0f; //Indica el tiempo total que estuvo esperando el coche en semáforos
    public Girador girador = null; //Usado para girar o seguir recto en una intersección
    public Movimiento cocheDeDelante; //El coche que llevamos delante en una recta
    private float tiempoInicioEspera; //Indica el momento en el que el coche empezó a esperar por última vez
    [HideInInspector] public bool estoyEsperando; //Indica si el coche se llegó a parar en el semáforo actual

    #endregion
    

    /****************************  Inicialización  *******************************/

    public void Inicializar(Posicion posicionEntrada, Posicion posicionSalida, Movimiento cocheDeDelante) {

        controller = GameObject.Find("GameController").GetComponent<GameController>(); //Obtenemos el script del GameController;
        this.transform.position = posicionEntrada.transform.position;
        this.transform.rotation = posicionEntrada.transform.rotation;
        this.cocheDeDelante = cocheDeDelante;
        girador = new Girador(posicionEntrada, posicionSalida, this);

    }



    /****************************  Comportamiento  *******************************/

    void Update () {
        
        /*if (esperandoPor != null) {
            if (girador.posicionFinal.padre == Posicion.Padre.Cruce && girador.posicionFinal.tipo == Posicion.Tipo.Salida) Debug.DrawLine(transform.position + transform.forward * 1f, esperandoPor.transform.position, Color.red);
            else Debug.DrawLine(transform.position + transform.forward * 1f, esperandoPor.transform.position, Color.green);
        }*/

        //Si pulsamos el espacio, se pausa la simulación
        if (!controller.pausa) {

            puedoEntrar = PuedoEntrar();

            //Calculamos si el semáforo está en verde
            if (controller.semaforos != GameController.Semaforos.SinSemaforos) {
                verde = Verde();
            } else {
                verde = true;
            }

            voyAChocar = VoyAChocar();

            //Si el semáforo está en verde y no voy a chocar, puedo andar
            andar = verde && puedoEntrar && !voyAChocar;

            //Cambiar velocidad si es necesario
            if (velocidadActual < velocidadCrucero && andar) {
                velocidadActual += aceleracion * controller.delta;
                if (velocidadActual > velocidadCrucero) velocidadActual = velocidadCrucero;
            }
            if (velocidadActual > 0 && !andar) {
                velocidadActual -= decelarcion * controller.delta;
                if (velocidadActual < 0) velocidadActual = 0;
            }

            //Si me paro, empiezo a esperar (siempre que no esté ya esperando)
            if (velocidadActual == 0 && !estoyEsperando) {
                tiempoInicioEspera = controller.tiempoTotal;
                estoyEsperando = true;
            }

            //Moviemiento
            girador.Moverse(velocidadActual * controller.delta);

        }

    }

    public Posicion SalirDeCruce() { //Esta función es llamada por el girador cuando el coche llega a un punto de salida de intersección
        
        //Acutalizo los datos de tiempo esperando
        if (estoyEsperando) {
            controller.RegistrarEspera(controller.tiempoTotal - tiempoInicioEspera);
            tiempoEsperado += controller.tiempoTotal - tiempoInicioEspera;
            paradas++;
            estoyEsperando = false;
        } else {
            controller.RegistrarEspera(0f);
        }

        girador.posicionInicial.SalirDeCruce(this);

        return girador.posicionFinal.SiguientePosicion();

    }

    public Posicion EntrarEnCruce() {

        if (proximaSalida != null) {
            Posicion devolver = proximaSalida;
            proximaSalida = null;
            return devolver;
        }

        girador.posicionFinal.EntrarEnCruce(this);

        Posicion salida = girador.posicionFinal.SiguientePosicion();

        return salida;

    }




    /****************************  Funciones auxiliares  *******************************/

    private bool Verde() {

        bool verde;
        if (girador.posicionFinal.padre != Posicion.Padre.Cruce || girador.posicionFinal.SemaforoVerde(proximaSalida))
            verde = true;
        else {
            float distanciaEntrada = Mathf.Abs(Vector3.Distance(transform.position, girador.posicionFinal.transform.position));
            verde = !(distanciaEntrada < velocidadActual * 2f -2f || (distanciaEntrada < 1f && velocidadActual < 3f));
        }
        return verde;

    }

    private bool PuedoEntrar() {

        if (girador.posicionFinal.padre == Posicion.Padre.Cruce && girador.posicionFinal.tipo == Posicion.Tipo.Entrada) { //Nos estamos acercando a un cruce
            if (proximaSalida == null) proximaSalida = girador.posicionFinal.SiguientePosicion(); //Necesito saber por dónde saldré del cruce para saber si puedo entrar
            if (girador.posicionFinal.cruce.PuedoEntrar(girador.posicionFinal, proximaSalida)) return true;
            else {
                float distanciaEntrada = Mathf.Abs(Vector3.Distance(transform.position, girador.posicionFinal.transform.position));
                return !(distanciaEntrada < velocidadActual * 2f || distanciaEntrada < 1f);
            }
        } else return true;

    }

    private bool VoyAChocar() {

        //Evitamos que dos coches se esperen mutuamente
        if (Cruce() && velocidadActual < 2 && esperandoPor != null && esperandoPor.esperandoPor != null) {
            if (esperandoPor.esperandoPor == this && transform.position.x <= esperandoPor.transform.position.x && velocidadActual < 2f) return false;
            //Evitamos que tres coches se esperen mutuamente
            if (esperandoPor.esperandoPor.esperandoPor != null) {
                if (esperandoPor.esperandoPor.esperandoPor == this && transform.position.x <= esperandoPor.transform.position.x &&
                transform.position.x <= esperandoPor.esperandoPor.transform.position.x && velocidadActual < 2f) return false;
                //Evitamos que cuatro coches se esperen mutuamente
                if (esperandoPor.esperandoPor.esperandoPor.esperandoPor != null && esperandoPor.esperandoPor.esperandoPor.esperandoPor == this &&
                    transform.position.x <= esperandoPor.transform.position.x && transform.position.x <= esperandoPor.esperandoPor.transform.position.x &&
                    transform.position.x <= esperandoPor.esperandoPor.esperandoPor.transform.position.x && velocidadActual < 2f) return false;
            }
        }

        //Calculamos si vamos a chocar con el coche de delante
        if (cocheDeDelante != null) {
            float distancia = Vector3.Distance(transform.position, cocheDeDelante.transform.position);
            if ((distancia < velocidadActual * 0.9f || distancia < 3.5f) && Vector3.Angle(girador.posicionFinal.transform.position - transform.position, cocheDeDelante.transform.position - transform.position) < 45f) { esperandoPor = cocheDeDelante; motivoEspera = 1; return true; }
        }
        if (Cruce()) { //Estoy en un cruce
            Posicion.Salida salida = new Posicion.Salida();
            foreach (Posicion.Salida salidaPosible in girador.posicionInicial.siguientesPosiciones) {
                if (girador.posicionFinal == salidaPosible.posicion) salida = salidaPosible;
            }
            try {
                foreach (Posicion.Maniobra conflicto in salida.maniobrasConflictivas) {
                    Posicion posicion = conflicto.salida;
                    foreach (Movimiento coche in posicion.cochesAcercandose) {
                        //TODO no tener en cuenta aquellos coches cuya posicion inicial está incluída en entradasNoConfictivas
                        if (!conflicto.entradasNoConflictivas.Contains(girador.posicionInicial)) { 
                        Vector3 forward = girador.posicionFinal.transform.position - transform.position;
                        float distancia = Vector3.Distance(transform.position, coche.transform.position);
                        if (MismoDestino(coche) && !MismoOrigen(coche) && (distancia < velocidadActual * 0.9f || distancia < 4f) && Vector3.Angle(forward, coche.transform.position - transform.position) < 50f) { esperandoPor = coche; motivoEspera = 3; return true; }
                        if (coche.Cruce() && coche != cocheDeDelante && !MismoDestino(coche) && !MismoOrigen(coche) && (distancia < velocidadActual * 3f || distancia < 4f) && Vector3.Angle(forward, coche.transform.position - transform.position) < 30f && Vector3.Angle(transform.forward, coche.transform.forward) < 160f) { esperandoPor = coche; motivoEspera = 4; return true; }
                        }
                    }
                }
            }catch (NullReferenceException) {
                //Debug.Log(name + ": " + girador.posicionFinal.cruce.name + ", " + girador.posicionInicial.name);
            }
        }

        motivoEspera = 0;
        esperandoPor = null;
        return false;
    }

    public bool Cruce() { //Indica si estoy en un cruce

        if (girador.posicionFinal.padre == Posicion.Padre.Cruce) {
            if (girador.posicionFinal.tipo == Posicion.Tipo.Salida || girador.posicionFinal.tipo == Posicion.Tipo.Medio) return true;
            else if (Vector3.Distance(transform.position, girador.posicionFinal.transform.position) < 5f) return true;
            else return false;
        } else return false;

    }

    private bool MismoDestino(Movimiento coche) {

        return (girador.posicionFinal == coche.girador.posicionFinal);

    }

    private bool MismoOrigen(Movimiento coche) {

        return (girador.posicionInicial == coche.girador.posicionInicial);

    }

    public Posicion EliminarCoche() {

        controller.RegistrarMuerte(paradas, tiempoEsperado);
        Destroy(gameObject);
        return girador.posicionFinal;

    }

    [System.Serializable]
    public class Girador {

        private Movimiento script;
        public Posicion posicionInicial;
        public Posicion posicionFinal;
        public Vector3 estiradorInicial;
        public Vector3 estiradorFinal;
        [SerializeField] private float contador = 0f;

        public Girador(Posicion posicionInicial, Posicion primerDestino, Movimiento script) {

            this.script = script;
            this.posicionInicial = posicionInicial;
            this.posicionFinal = primerDestino;

            ReiniciarGiro();

        }

        private Posicion SiguientePosicion() {

            if (posicionFinal.padre == Posicion.Padre.Cruce) {
                if (posicionFinal.tipo == Posicion.Tipo.Entrada) return script.EntrarEnCruce();
                if (posicionFinal.tipo == Posicion.Tipo.Salida) return script.SalirDeCruce();
                else return posicionFinal.SiguientePosicion();
            } else if (posicionFinal.padre == Posicion.Padre.Fin) return script.EliminarCoche();
            else {
                return posicionFinal.SiguientePosicion();
            }

        }

        private void ReiniciarGiro() {

            //Actualizamos las listas de coches acercándose de las posiciones que abandonamos y empezamos a usar
            if (posicionInicial.padre != Posicion.Padre.Inicio) posicionInicial.cochesAcercandose.Remove(script);
            if (posicionFinal.padre != Posicion.Padre.Inicio) posicionFinal.CocheAcercandose(script);
            script.cocheDeDelante = posicionFinal.ultimoCocheEnAcercarse;
            posicionFinal.ultimoCocheEnAcercarse = script;
            estiradorInicial = posicionInicial.transform.position + posicionInicial.transform.forward * 0.333f * Vector3.Distance(posicionFinal.transform.position, posicionInicial.transform.position);
            estiradorFinal = posicionFinal.transform.position - posicionFinal.transform.forward * 0.333f * Vector3.Distance(posicionFinal.transform.position, posicionInicial.transform.position);

        }

        private Vector3 ObtenerCentro(Posicion posicionInicial, Posicion posicionFinal) {
            Vector3 centro = posicionInicial.transform.position + Vector3.Scale(posicionFinal.transform.position - posicionInicial.transform.position, new Vector3(Mathf.Abs(posicionInicial.transform.right.x), Mathf.Abs(posicionInicial.transform.right.y), Mathf.Abs(posicionInicial.transform.right.z)));
            return centro;
        }

        public void Moverse(float velocidad) {

            contador += velocidad / Vector3.Distance(posicionInicial.transform.position, posicionFinal.transform.position); //Aumentamos el contador
            if (contador > 1f) { //Cuando llegamos al final:
                Posicion aux = posicionFinal; //La posición en la que acabamos pasa a ser nuestra posición inicial
                posicionFinal = SiguientePosicion(); //Calculamos la siguiente posición (esta función es la que avisa de que salimos o entramos en un cruce, etc.)
                posicionInicial = aux;
                ReiniciarGiro(); //Situamos el girador en la nueva posición
                contador = 0f; //Reiniciamos el contador
            }
            PosicionActual();
            
        }

        private void PosicionActual() {
            Vector3 a = Vector3.Lerp(posicionInicial.transform.position, estiradorInicial, contador);
            Vector3 b = Vector3.Lerp(estiradorInicial, estiradorFinal, contador);
            Vector3 c = Vector3.Lerp(estiradorFinal, posicionFinal.transform.position, contador);
            Vector3 d = Vector3.Lerp(a, b, contador);
            Vector3 e = Vector3.Lerp(b, c, contador);
            script.transform.position = Vector3.Lerp(d, e, contador);
            script.transform.rotation = (contador == 0f)? posicionInicial.transform.rotation : (contador == 1f)? posicionFinal.transform.rotation : Quaternion.LookRotation(e - d, Vector3.up);
            return;
        }

        public Movimiento.Giro ObtenerGiro() {
            float angulo = Vector3.SignedAngle(posicionInicial.transform.forward, posicionFinal.transform.position - posicionInicial.transform.position, Vector3.up);
            if (angulo > 0f) return Movimiento.Giro.Derecha;
            else if (angulo < 0f) return Movimiento.Giro.Izquierda;
            else return Movimiento.Giro.Recto;
        }

    }

    public float TiempoEsperando() { //Indica cuánto tiempo llevo esperando en un semáforo. Si no estoy esperando, devuelve 0;

        if (estoyEsperando) return controller.tiempoTotal - tiempoInicioEspera;
        else return 0f;

    }

}
