import java.util.*;
import java.lang.Thread;
import java.net.Socket;
import java.util.concurrent.Semaphore;
import java.io.*;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;

public class simulador {

	public static void main(String[] args) {

		Cruce p = new Cruce();
		p.principal(Integer.parseInt(args[0]), Integer.parseInt(args[1]), Integer.parseInt(args[2]), Integer.parseInt(args[3]), Integer.parseInt(args[4]),Integer.parseInt(args[5]),Integer.parseInt(args[6]),Integer.parseInt(args[7]),Integer.parseInt(args[8]),Integer.parseInt(args[9]),Integer.parseInt(args[10]),Integer.parseInt(args[11]),Integer.parseInt(args[12]),Integer.parseInt(args[13]),Integer.parseInt(args[14]));
		return;
	}
		
}

class Cruce{
	public final int TIEMPO_COCHE = 200;
	
	public static int total = 0;
	public boolean salir = false;
	
	public static final Semaphore NORTE = new Semaphore(1);
	public static final Semaphore SUR = new Semaphore(1);
	public static final Semaphore ESTE = new Semaphore(1);
	public static final Semaphore OESTE = new Semaphore(1);

	public static boolean SemaforoA;
	public static boolean SemaforoB;
	public static boolean SemaforoC;
	public static boolean SemaforoD;

	public long media_total=0;
	public long maximo_total=0;
	public long coches_total=0;
	public long tiempo_total=0;
	public int contador=0;

	public static long tiempo=0;
	public static long maximo=0;

	public static int norte = 0;
	public static int sur = 0;
	public static int este = 0;
	public static int oeste = 0;
	
	public static int total_norte = 0;
	public static int total_sur = 0;
	public static int total_este = 0;
	public static int total_oeste = 0;

	public static int calles = 0;
	public static int vista = 0;


	public static float p_norte;
	public static float p_sur;
	public static float p_este;
	public static float p_oeste;


	public static boolean acabar=false ;
	
	public static String [][] tabla=new String[12][4];
	public static Map<Double, Object[]> sig_cruce=null;
	public static boolean EMPEZAR_EJECUCION=false;
	



	public  void principal(int metodo, int random, int A, int B, int C, int D, int repeticiones, int prob_norte, int prob_sur, int prob_este, int prob_oeste, int calle, int vist, int mipuerto, int sincronizar) {
		
		int tSemaforo=6000;
		
		try {
		
		vista=vist;
		calles=calle;
		
		(new RecibirMensajes(mipuerto, TIEMPO_COCHE)).start();
		
		Scanner sc=new Scanner(System.in); 
		conectar_cruces(sc);
		sincronizar_cruces(sincronizar,sc);
		sc.close();

		if(vista==1)System.out.println("__"+metodo+"__"+random+"__"+A+"__"+B+"__"+C+"__"+D+"__"+repeticiones+"__"+prob_norte+"__"+prob_sur+"__"+prob_este+"__"+prob_este+"__"+calles+"__");

		p_este = ((float)prob_este)/100;
		p_sur = ((float)prob_sur)/100;
		p_norte = ((float)prob_norte)/100;
		p_oeste = ((float)prob_oeste)/100;

		if(vista==1)System.out.println("Probabilidades"+ p_norte+" "+p_sur+" "+p_este+" "+p_oeste);


		if(random == 2){
			A= (int) Math.floor(Math.random()*9+1);
			B= (int) Math.floor(Math.random()*9+1);
			C= (int) Math.floor(Math.random()*9+1);
			D= (int) Math.floor(Math.random()*9+1);
		}

		//////////COCHES INICIALES
		for(int x=0; x < A; x++)	(new coche("NORTE", TIEMPO_COCHE)).start();
		for(int x=0; x < B; x++)	(new coche("SUR", TIEMPO_COCHE)).start();
		for(int x=0; x < C; x++)	(new coche("ESTE", TIEMPO_COCHE)).start();
		for(int x=0; x < D; x++)	(new coche("OESTE", TIEMPO_COCHE)).start();
		///////////////////////

		(new introducir_coches(random, TIEMPO_COCHE)).start();

		while(true){
			
			SemaforoA = false;
			SemaforoB = false;
			SemaforoC = false;
			SemaforoD = false;
			
				switch(metodo){

					case 1:
						
						Thread.sleep(1000);
						ponerenVerde("NORTE",tSemaforo);
						
						Thread.sleep(1000);
						ponerenVerde("SUR",tSemaforo);
						
						Thread.sleep(1000);
						ponerenVerde("ESTE",tSemaforo);
						
						Thread.sleep(1000);
						ponerenVerde("OESTE",tSemaforo);
				
					break;

				case 2:

					int actual_N = 0;
					int actual_S = 0;
					int actual_E = 0;

					if (norte >= este && norte >= sur && norte >= oeste) {
						actual_N = norte;
						Thread.sleep(1000);
						ponerenVerde("NORTE", actual_N * (TIEMPO_COCHE + 1));

						if (norte != 0 || sur != 0 || este != 0) {
							printColas();
						}

					}
					if (sur >= este && sur > norte && sur >= oeste) {
						actual_S = sur;
						Thread.sleep(1000);
						ponerenVerde("SUR", actual_S * (TIEMPO_COCHE + 1));
						if (norte != 0 || sur != 0 || este != 0) {
							printColas();
						}
					}
					if (este > sur && este > norte && este >= oeste) {
						actual_E = este;
						Thread.sleep(1000);
						ponerenVerde("ESTE", actual_E * (TIEMPO_COCHE + 1));
						if (norte != 0 || sur != 0 || este != 0) {
							printColas();
						}
					}
					if (oeste > norte && oeste > sur && oeste > este) {
						actual_N = oeste;
						Thread.sleep(1000);
						ponerenVerde("OESTE", actual_N * (TIEMPO_COCHE + 1));

						if (norte != 0 || sur != 0 || este != 0) {
							printColas();
						}

					}

					break;
				

					case 3:
				
						if((norte >= este)&&( norte >= sur)&& (norte>=oeste)){
							Thread.sleep(1000);
							ponerenVerde("NORTE",tSemaforo);
						}
						if((sur >= este)&&( sur >= norte)&& (sur>=oeste)){
							Thread.sleep(1000);
							ponerenVerde("SUR",tSemaforo);
						}
						if((este >= norte)&&( este >= sur)&& (este>=oeste)){
							Thread.sleep(1000);
							ponerenVerde("ESTE",tSemaforo);
						}
						if((oeste >= norte)&&( oeste >= sur) && (oeste>=este)){
							Thread.sleep(1000);
							ponerenVerde("OESTE",tSemaforo);
						}
						printColas();

					break;

					case 4:
						int actual;
						
						if(norte != 0 || sur != 0 || este !=0){
						actual = norte;
						Thread.sleep(1000);
						ponerenVerde("NORTE",actual * (TIEMPO_COCHE+1));
						printColas();
						
						actual = sur;
						Thread.sleep(1000);
						ponerenVerde("SUR",actual * (TIEMPO_COCHE+1));
						printColas();

						actual=este;
						Thread.sleep(1000);
						ponerenVerde("ESTE",actual*(TIEMPO_COCHE+1));
						printColas();

						actual = oeste;
						Thread.sleep(1000);
						ponerenVerde("OESTE",actual * (TIEMPO_COCHE+1));
						printColas();
						}

					break;					

				}			
			
				if(acabar == true){
					if(norte==0 && sur == 0 && este == 0 && oeste==0){
						tiempo_total=tiempo_total+tiempo;
						media_total=media_total+(tiempo/total);
						maximo_total=maximo_total+maximo;
						coches_total=coches_total+total;
						if(contador<(repeticiones-1)){
							if(vista==1)System.out.println("ACABA "+contador+1+"---------------------------------");
								salir=false;
								acabar=false;
								total=0;
								tiempo=0;
								norte=0;
								sur=0;
								este=0;

								//////////COCHES INICIALES
								for(int x=0; x < A; x++)	(new coche("NORTE", TIEMPO_COCHE)).start();
								for(int x=0; x < B; x++)	(new coche("SUR", TIEMPO_COCHE)).start();
								for(int x=0; x < C; x++)	(new coche("ESTE", TIEMPO_COCHE)).start();
								for(int x=0; x < D; x++)	(new coche("OESTE", TIEMPO_COCHE)).start();
								///////////////////////
								contador++;

								(new introducir_coches(random, TIEMPO_COCHE)).start();
							
					}else{

							FileWriter fichero = null;
							PrintWriter pw = null;
							try {
								fichero = new FileWriter("prueba.txt", true);
								pw = new PrintWriter(fichero);

								pw.println(" Tiempo coches = " + (tiempo_total / repeticiones * 100));
								pw.println(" Tiempo espera medio = " + (media_total) / repeticiones * 100
										+ "----- coches totales: " + coches_total / repeticiones);
								pw.println(" Tiempo espera maximo = " + maximo_total / repeticiones * 100);

							} catch (Exception e) {
								e.printStackTrace();
							} finally {
								try {
									// Nuevamente aprovechamos el finally para
									// asegurarnos que se cierra el fichero.
									if (null != fichero)
										fichero.close();
								} catch (Exception e2) {
									e2.printStackTrace();
								}
							}

							System.out.println(" Tiempo coches = " + (tiempo_total / repeticiones * 100));
							System.out.println(" Tiempo espera medio = " + (media_total) / repeticiones * 100
									+ "----- coches totales: " + coches_total / repeticiones);
							System.out.println(" Tiempo espera maximo = " + maximo_total / repeticiones * 100);
							return;
						}
					}
				}

			}
		} catch (Exception e) {
			e.printStackTrace();
		}

	}
	
	
	public static void printColas() {
		if(vista==1)System.out.println("*************************************************************");
		if(vista==1)System.out.println("las colas son: "+ norte + " " + sur + " " + este + " " + oeste );
		if(vista==2){
			pintarGraficas();
		}
	}
	
	public static void ponerenVerde(String sem, int tiempo) throws InterruptedException {
		
		checksendnextCrossing(sem);
		printColas();
		switch(sem){
			case "NORTE": 
				SemaforoA=true;
				Thread.sleep(tiempo);
				SemaforoA=false;
				break;
			case "SUR": 
				SemaforoB=true;
				Thread.sleep(tiempo);
				SemaforoB=false;
				break;
			case "ESTE": 
				SemaforoC=true;
				Thread.sleep(tiempo);
				SemaforoC=false;
				break;
			case "OESTE": 
				SemaforoD=true;
				Thread.sleep(tiempo);
				SemaforoD=false;
				break;
		}
	}
	
	
	public static void conectar_cruces(Scanner sc) {
		int counter=0;
		String s;
		while(true) {
			System.out.print("SALE COCHE DESDE CARRIL: ");
			tabla[counter][0]=sc.nextLine();
			System.out.print("HACIA CARRIL: ");
			tabla[counter][1]=sc.nextLine();
			System.out.print("PUERTO DESTINO: ");
			tabla[counter][2]=sc.nextLine();
			System.out.print("PROBABILIDAD: ");
			tabla[counter++][3]=sc.nextLine();
			
			System.out.print("Mas conexiones?: ");
			s=sc.nextLine();
			if(s.equals("n")) {	
				break;
			}
			System.out.println("");
		}
	}
	
	public static void sincronizar_cruces(int sincronizar, Scanner scanner) throws InterruptedException {
		if(sincronizar!=0) {
			while(true) {
				if(EMPEZAR_EJECUCION==true)  break;
				Thread.sleep(1);
			}
			return;
		}else {
			int [] avisar_puertos=new int[100];
			int counter=0;
			int num=0;
			String s=null;
			
			System.out.println("\nDESPERTAR CRUCES CON PUERTOS: ");
			s=scanner.nextLine();
			scanner.close();
			
			scanner=new Scanner(s);
			while(scanner.hasNext()) {
				 if (scanner.hasNextInt()) avisar_puertos[counter++]=scanner.nextInt();
			}	
			
			scanner.close();
				
			for(int i=0;i<avisar_puertos.length;i++) 
				if(avisar_puertos[i]!=0) sendnextCrossing("START",avisar_puertos[i]);
		}
	}
	

	public static void sale(String dir){
		if(dir.equals("NORTE")){
			norte--;
		}
		if(dir.equals("SUR")){
			sur--;
		}
		if(dir.equals("ESTE")){
			este--;
		}
		if(dir.equals("OESTE")){
			oeste--;
		}
		sigCruce();
	}
	
	public static void sigCruce() {
		double p=Math.random();
		double acc=0;
		
		Iterator<Double> it = sig_cruce.keySet().iterator();
		while(it.hasNext()){
		  Double clave = it.next();
		  if(p < (clave.doubleValue()+acc)) {
			  Object [] carril_puerto=sig_cruce.get(clave);
			  
			  String carril=(String) carril_puerto[0];
			  int puerto=(Integer) carril_puerto[1];
			  
			  sendnextCrossing(carril,puerto);
			  break;
		  }
		  acc=acc+clave.doubleValue();
		}
		
	}
	
	public static void checksendnextCrossing(String origin) {
		sig_cruce=new TreeMap<Double, Object[]>();
		
		for(int i=0;i<Cruce.tabla.length;i++) {
			if(Cruce.tabla[i][0]==null) break;
			if(Cruce.tabla[i][0].equals(origin)){
					Object [] s= {Cruce.tabla[i][1],Integer.parseInt(Cruce.tabla[i][2])};
					sig_cruce.put(Double.parseDouble(Cruce.tabla[i][3]), s);	
				}
		}
	}
	
	public static void sendnextCrossing(String mensaje, int puerto) {
		try {
		System.out.println("\t***********Paquete destino puerto "+puerto+" y mensaje "+mensaje);
		Socket socket = new Socket("localhost", puerto);
		OutputStream os = socket.getOutputStream();
		PrintWriter pw = new PrintWriter(os, true);
		pw.printf(mensaje);
		socket.close();
		}catch(Exception Ex) {
			Ex.printStackTrace();
		}
	}
	
	public static void setTime(long time){
		if(time > maximo){
			maximo = time;
		}
		tiempo = tiempo + time;
	}

	public static void pintarGraficas(){
		int max =0;
		String SimA = "";
			System.out.println("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
			for(int y = 0; y<norte;y++){
				SimA=SimA+"|";
			}
			if(SemaforoA==true){
				System.out.println("\033[32mA"+":  "+SimA);
			}else{
				System.out.println("\033[31mA"+":  "+SimA);
			}
			SimA="";

			for(int y = 0; y<sur;y++){
				SimA=SimA+"|";
			}
			if(SemaforoB==true){
				System.out.println("\033[32mB"+":  "+SimA);
			}else{
				System.out.println("\033[31mB"+":  "+SimA);
			}
			SimA="";

			for(int y = 0; y<este;y++){
				SimA=SimA+"|";
			}
			if(SemaforoC==true){
				System.out.println("\033[32mC"+":  "+SimA);
			}else{
				System.out.println("\033[31mC"+":  "+SimA);
			}
			SimA="";

			for(int y = 0; y<oeste;y++){
				SimA=SimA+"|";
			}
			if(SemaforoD==true){
				System.out.println("\033[32mD"+":  "+SimA);
			}else{
				System.out.println("\033[31mD"+":  "+SimA);
			}
			SimA="";
		

			return;
	}
}

class coche extends Thread{

	String carril;
	long time_start, time_end;
	int  TIEMPO_COCHE;
	int id=0;

	public coche(String car, int TIEMPO_COCH){
		TIEMPO_COCHE = TIEMPO_COCH;
		carril = car;
		Cruce.total++;
		switch(carril){

			case "NORTE": 
				Cruce.norte++;
				id=++Cruce.total_norte;
				break;
			case "SUR": 
				Cruce.sur++;
				id=++Cruce.total_sur;
				break;
			case "ESTE": 
				Cruce.este++;
				id=++Cruce.total_este;
				break;
			case "OESTE": 
				Cruce.oeste++;
				id=++Cruce.total_oeste;
				break;
			default:
				break;
	
		}
	}

	public void run(){
		time_start = System.currentTimeMillis();

 		try {

 			if(carril.equals("NORTE")){
	            Cruce.NORTE.acquire();
	 			
 				while(!Cruce.SemaforoA) {
    			 sleep(1);
				}
	 
	            if(Cruce.vista==1)System.out.println("Sale coche de [ " + this.carril + " ] (N-"+id+")");
	 
	            Thread.sleep(TIEMPO_COCHE);
	 
	            Cruce.NORTE.release();
	            Cruce.sale("NORTE");
       		}	

       		if(carril.equals("SUR")){
       			
       			Cruce.SUR.acquire();
	            while(!Cruce.SemaforoB) {
    			 sleep(1);
				}
	 
	            if(Cruce.vista==1)System.out.println("Sale coche de [ " + this.carril + " ] (S-"+id+")");
	 
	            Thread.sleep(TIEMPO_COCHE);
	 
	            Cruce.SUR.release();
	            Cruce.sale("SUR");
       		}

       		if(carril.equals("ESTE")){
       			Cruce.ESTE.acquire();
       			while(!Cruce.SemaforoC) {
    			 sleep(1);
				}
	 
	            if(Cruce.vista==1)System.out.println("Sale coche de [ " + this.carril + " ] (E-"+id+")");
	 
	            Thread.sleep(TIEMPO_COCHE);
	 
	            Cruce.ESTE.release();
	            Cruce.sale("ESTE");
       		}

       		if(carril.equals("OESTE")){
       			Cruce.OESTE.acquire();
       			while(!Cruce.SemaforoD) {
    			 sleep(1);
				}
	 
	            if(Cruce.vista==1)System.out.println("Sale coche de [ " + this.carril + " ] (O-"+id+")");
	 
	            Thread.sleep(TIEMPO_COCHE);
	 
	            Cruce.OESTE.release();
	            Cruce.sale("OESTE");
       		}
        } catch (InterruptedException ex) {
            ex.printStackTrace();
        }

		
		time_end = System.currentTimeMillis();
		Cruce.setTime(time_end - time_start);
	}

}

class introducir_coches extends Thread{
	int rand;
	int TIEMPO_COCHE;

	public introducir_coches(int random, int TIEMPO_COCH){
		rand =random;
		TIEMPO_COCHE = TIEMPO_COCH;
	}

	public void run() {

		int x, A, B, C, D;
		try {

			for (int y = 0; y < 100; y++) {

				A=B=C=D=0;
				
				if(Math.random() < Cruce.p_norte) A=1;
				if(Math.random() < Cruce.p_sur) B=1;
				if(Math.random() < Cruce.p_este) C=1;
				if(Math.random() < Cruce.p_oeste) D=1;

				Thread.sleep((int) Math.floor(Math.random() * 2000 + 1));
				for (x = 0; x < A; x++) {
					if (Cruce.vista == 1) System.out.println(" ENTRA NORTE");
					(new coche("NORTE", TIEMPO_COCHE)).start();
				}
				for (x = 0; x < B; x++) {
					if (Cruce.vista == 1) System.out.println(" ENTRA SUR");
					(new coche("SUR", TIEMPO_COCHE)).start();
				}
				for (x = 0; x < C; x++) {
					if (Cruce.vista == 1) System.out.println(" ENTRA ESTE");
					(new coche("ESTE", TIEMPO_COCHE)).start();
				}
				if (Cruce.calles == 4) {
					for (x = 0; x < D; x++) {
						if (Cruce.vista == 1) System.out.println(" ENTRA OESTE");
						(new coche("OESTE", TIEMPO_COCHE)).start();
					}
				}
			}
		} catch (InterruptedException ex) {
			ex.printStackTrace();
		}

		System.out.println("\n\t********YA NO INTRODUCIMOS MAS COCHES");
		Cruce.acabar = true;

		return;
	}

}

class RecibirMensajes extends Thread {

	int puerto = 0;
	int TIEMPO_COCHE;

	public RecibirMensajes(int mipuerto, int TIEMPO_COCH) {
		puerto = mipuerto;
		TIEMPO_COCHE = TIEMPO_COCH;
	}

	public void run() {
		try {
			Socket socket;

			System.out.println("Creado el socket del servidor en el puerto " + puerto);
			ServerSocket serverSocket = new ServerSocket(puerto);

			while (true) {
				socket = serverSocket.accept();
				BufferedReader br = new BufferedReader(new InputStreamReader(socket.getInputStream()));
				String mensaje = br.readLine();
				if(mensaje.equals("START")) {
					Cruce.EMPEZAR_EJECUCION=true;
					continue;
				}
				System.out.println("\t**Nuevo coche en carril" + mensaje);
				(new coche(mensaje, TIEMPO_COCHE)).start();
			}
		} catch (Exception e) {
			System.out.println("Interrupted");
		}
	}
}
