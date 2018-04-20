# FlowLight

FlowLight es un sistema de gestión de semáforos para regular el tráfico en base a datos recogidos en tiempo real. Para ello se utilizan cámaras con las que se detecta el número de vehículos y un algoritmo que decide el tiempo de apertura de los semáforos.

## Código propio
* [Flowlight Simulator](https://github.com/parope23/Flowlight/tree/master/FlowLight%20simulator): Simulador creado con Unity para probar los algoritmos de tráfico diseñados.

* [Probador de algoritmos JAVA](https://github.com/parope23/Flowlight/tree/master/Probador%20de%20algoritmos%20JAVA): Proyecto de JAVA para probar validez de los algoritmos.

* [Procesar imágenes propias.ipynb](https://github.com/parope23/Flowlight/blob/master/Procesar%20im%C3%A1genes%20propias.ipynb): Notebook de Python utilizado para crear el dataset de entrenamiento.

* [XML2JSON](https://github.com/parope23/Flowlight/blob/master/XML2JSON.ipynb): Notebook de Python para transformar las detecciones del dataset DETRAC de archivos XML a formato VGG en archivos JSON.

## Repositorios públicos utilizados

* [Tensorflow Object Detection API](https://github.com/tensorflow/models/tree/master/research/object_detection): Framework de software libre construido sobre Tensorflow para construir, entrenar y desplegar modelos de detección de objetos.

* [Movidius™ Neural Compute SDK](https://github.com/movidius/ncsdk): SDK necesario para el funcionamiento del Movidius Neural Compute Stick.

* [Movidius™ Neural Compute AppZoo - VideoObjects](https://github.com/movidius/ncappzoo/tree/master/apps/video_objects): Proyecto compatible con Movidius Neural Compute Stick para detección y clasificación de objetos utilizando la arquitectura SSD_MobileNet sobre Caffe.

* [Dataset UA-DETRAC](http://detrac-db.rit.albany.edu/): Dataset con imágenes y anotaciones de vehículos en vías urbanas.
