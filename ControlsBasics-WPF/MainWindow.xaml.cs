//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private readonly KinectSensorChooser sensorChooser;
        private Boolean  mano_cerrada;
        private Point    lInicial;
        private Point    xInicial;
        private Point    zInicial;
        private Point    zMax;
        private Point    xMax;        
        private Boolean  lVertical;
        private Boolean  lHorizontal;
        private Boolean  xVertical;
        private Boolean  xDiagonal;
        private Boolean  xDiagonal2;
        private Boolean  zHorizontal;
        private Boolean  zHorizontal2;
        private Boolean  zDiagonal;
        private int      margen_error;
        private Boolean  debug;
        HandPointer      mano_actual;
        private int[]    resultado; 

        public MainWindow()
        {
            this.InitializeComponent();
            //Aquí se realiza la primera inicialización de las variables de
            //control de las funciones de reconocimiento de gestos
            mano_cerrada = false;
            lVertical    = true;
            lHorizontal  = false;
            xVertical    = false;
            xDiagonal    = true;
            xDiagonal2   = false;
            zHorizontal  = true;
            zHorizontal2 = false;
            zDiagonal    = false;
            debug        = true; //Modo debug: Poner a false para que no salgan mensajes por pantalla
            margen_error = 100;
            resultado    = new int[3];

            //Inicializamos los sensores de Kinect necesarios para el reconocimiento de gestos
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);           
        }
        
        //Aquí se recogen los eventos tanto de inicialización como de cambio de estado del sensor para
        //la configuración inicial asi como para tratar el evento de otro sensor conectado
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {            
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        //Detenemos el sensor en caso de cerrar la ventana el programa
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }
        
        //Aquí recogemos el evento de mano cerrada que es el que inicia el reconocimiento
        //de los gestos
        private void manoCerrada(object sender, RoutedEventArgs e)
        {
            //Guardamos en un objeto la mano activa para acceder correctamente a su
            //posición en cada momento
            if (this.kinectRegion.HandPointers[0].IsActive)
                mano_actual = this.kinectRegion.HandPointers[0];
            else
                mano_actual = this.kinectRegion.HandPointers[1];

            //Inicializamos las variables de historia de posición necesarias para el
            //reconocimiento de cada gesto
            lInicial = mano_actual.GetPosition(wrapPanel);
            zInicial = mano_actual.GetPosition(wrapPanel);            
            xInicial = mano_actual.GetPosition(wrapPanel);
            zMax     = zInicial;
            xMax     = xInicial;
            //La variable mano_cerrada es la que activa el reconocimiento de gestos
            mano_cerrada = true;
            if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Mano cerrada", "#FFFFFFFF"));            
        }

        //Aqui recogemos el evento de mano abierta que interrumpe cualquier reconocimiento de gestos
        //Pone todas las variables de control al estado inicial
        private void manoAbierta(object sender, RoutedEventArgs e)
        {
            mano_cerrada = false;
            lVertical    = true;
            lHorizontal  = false;
            xVertical    = false;
            xDiagonal    = true;
            xDiagonal2   = false;
            zHorizontal  = true;
            zHorizontal2 = false;
            zDiagonal    = false;
            for (int i = 0; i < resultado.Length; i++)
                resultado[i] = 0;
            var converter = new System.Windows.Media.BrushConverter();
            this.tile.Background =  (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
            if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Mano abierta", "#FFFFFFFF"));
        }

        //Aquí recogemos el evento de mano fuera de la región vinculada al sensor que interrumpe cualquier
        //reconocimiento de gestos
        //Pone todas las variables de control al estado inicial
        private void manoFuera(object sender, RoutedEventArgs e)
        {
            mano_cerrada = false;
            lVertical    = true;
            lHorizontal  = false;
            xVertical    = false;
            xDiagonal    = true;
            xDiagonal2   = false;
            zHorizontal  = true;
            zHorizontal2 = false;
            zDiagonal    = false;
            for (int i = 0; i < resultado.Length; i++)
                resultado[i] = 0;
            var converter = new System.Windows.Media.BrushConverter();
            this.tile.Background =  (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
            if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Mano fuera de la pantalla", "#FFFFFFFF"));
        }

        //Aquí recogemos los eventos de movimiento de la mano pero únicamente se procede a reconocer
        //gestos en caso de que la variable mano_cerrada = true
        private void moviendoMano(object sender, RoutedEventArgs e)
        {
            if (mano_cerrada)
            {
                //Para cada gesto comprobamos que no se ha producido ninguno de ellos
                //El resultado de la comprobación lo guardamos en el array resultado
                //El contenido de resultado se interpreta de la siguiente forma:
                // -1: el movimiento realizado no coincide con el gesto por lo que no se vuelve a comprobar
                //  0: el movimiento realizado hasta el momento es válido para el gesto
                //  1: se ha detectado el gesto
                if (resultado[0] != -1) resultado[0] = detectarGestoL();
                if (resultado[1] != -1) resultado[1] = detectarGestoZ();
                if (resultado[2] != -1) resultado[2] = detectarGestoX();

                //En caso de que se haya detectado un gesto ponemos el color del tile a verde y mostramos
                //por pantalla el gesto detectado
                if (resultado[0] == 1 || resultado[1] == 1 || resultado[2] == 1)
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF0A501C");
                    mano_cerrada = false;

                    if (resultado[0] == 1) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L detectado correctamente", "#FF0A501C"));
                    if (resultado[1] == 1) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z detectado correctamente", "#FF0A501C"));
                    if (resultado[2] == 1) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X detectado correctamente", "#FF0A501C"));
                }

                //Si el movimiento realizado no coincide con ningún gesto detenemos la detección de gestos
                //hasta que se vuelva a empezar de cero (acción de cerrar la mano)
                if (resultado[0] == -1 && resultado[1] == -1 && resultado[2] == -1)
                {
                    mano_cerrada = false;
                }
            }                        
        }

        //Detección del gesto de movimiento en L
        //Mediante una variables de control podemos diferenciar 3 estados distintos en el reconocimiento
        //del gesto. En primer lugar se espera que el usuario baje la mano una distancia mínima. Una vez
        //superada la distancia mínima de bajada se admite que se siga bajando la mano o bien que comience
        //el movimiento horizontal. Este umbral de tolerancia es necesario debido a que es poco probable (y difícil)
        //que un usuario mueva exactamente una distancia en vertical antes de comenzar el desplazamiento horizontal.
        //Finalmente, cuando se detecta el movimiento horizontal por parte del usuario se espera que se alcance una
        //distancia de movimiento hasta aceptar el movimiento. En este caso no es relevante que el usuario se pase de
        //la distancia definida debido a que una vez alcanzada dicha distancia el gesto ya se considera reconocido    
        private int detectarGestoL()
        {
            int salida = 0;
            //Etapa de reconocimiento de movimiento vertical de bajada obligatorio antes de tener la opcion de un movimiento horizontal
            if (lVertical && !lHorizontal)
            {
                //Controlamos con un margen de error que realmente se trata de un movimiento vertical sin grandes desvíos
                //en el eje horizontal con respecto del punto en que se cerró la mano para comenzar la detección de gestos
                if (Math.Abs(lInicial.X - mano_actual.GetPosition(wrapPanel).X) < margen_error)
                {
                    //Si se ha alcanzado el movimiento vertical mínimo modificamos las variables para pasar al siguiente estado
                    if (mano_actual.GetPosition(wrapPanel).Y - lInicial.Y > 200)
                    {
                        lVertical = false;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L (Paso 1). Descenso vertical completado. Se admite un margen de bajada", "#FFFFFFFF"));                            
                    }
                    //Si se ha producido un movimiento de subida en vez de bajada interrumpimos la detección del gesto L
                    if (mano_actual.GetPosition(wrapPanel).Y - lInicial.Y < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L. Movimiento fallido (Has subido la mano). Empieza de nuevo", "#FFF01010"));
                    }
                }
                else
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L. Movimiento fallido (Te has desviado en horizontal). Empieza de nuevo", "#FFF01010"));
                }
            }
            //Estado en el que se contempla un margen de bajada antes de comenzar el movimiento horizontal
            if (!lVertical && !lHorizontal)
            {
                //En caso de un movimiento excesivo de bajada dejamos de reconocer el gesto
                if (mano_actual.GetPosition(wrapPanel).Y - lInicial.Y > 400)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L. Movimiento fallido (Te has pasado bajando la mano). Empieza de nuevo", "#FFF01010"));                    
                }
                else
                {
                    //Si se detecta un movimiento en horizontal hacia la izquierda se interrumpe
                    //la detección del gesto
                    if (mano_actual.GetPosition(wrapPanel).X - lInicial.X < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L. Movimiento fallido (Te has movido a la izquierda). Empieza de nuevo", "#FFF01010"));
                    }
                    //Si se ha detectado un movimiento horizontal hacia la derecha se pasa a la última
                    //fase del gesto L
                    if (mano_actual.GetPosition(wrapPanel).X - lInicial.X > margen_error)
                    {
                        lHorizontal = true;
                        //Nos quedamos con la última posición antes de pasar al siguiente estado para
                        //poder detectar una desviación indebida en vertical con respecto de dicha posición
                        lInicial = mano_actual.GetPosition(wrapPanel);
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L (Paso 2). Comenzando movimiento en horizontal", "#FFFFFFFF"));
                    }
                }
            }
            //Último estado en el que se detecta el movimiento horizontal
            if(!lVertical && lHorizontal)
            {
                //Controlamos con un margen de error que realmente se trata de un movimiento horizontal sin grandes desvíos
                //en el eje vertical con respecto del punto en que comienzo el movimiento en horizontal
                if (Math.Abs(lInicial.Y - mano_actual.GetPosition(wrapPanel).Y) < margen_error)
                {
                    //Si detectamos una cantidad de movimiento horizontal devolvemos el código de
                    //reconocimiento del gesto
                    if (mano_actual.GetPosition(wrapPanel).X - lInicial.X > 500)
                    {
                        salida = 1;
                    }
                    //Si se ha producido un desplazamiento hacia la izquierda se interrumpe la detección del gesto
                    if (mano_actual.GetPosition(wrapPanel).X - lInicial.X < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L. Movimiento fallido (Te has movido a la izquierda). Empieza de nuevo", "#FFF01010"));
                    }
                }
                else
                {
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto L. Movimiento fallido (Te has movido en horizontal). Empieza de nuevo", "#FFF01010"));
                    salida = -1;
                }
            }

            return salida;
        }

        //detección del gesto de movimiento Z
        //Mediante una variables de control podemos diferenciar 5 estados distintos en el reconocimiento
        //del gesto. En primer lugar se espera que el usuario desplace la mano en horizontal hacia la derecha una distancia mínima.
        //Una vez superada la distancia mínima de desplazamiento se admite que se siga desplazando la mano o bien que comience
        //el movimiento en diagonal de bajada. Este umbral de tolerancia es necesario debido a que es poco probable (y difícil)
        //que un usuario mueva exactamente una distancia en horizontal antes de comenzar el desplazamiento en diagonal.
        //Una vez detectado el movimiento en diagonal por parte del usuario se espera que se alcance una
        //distancia de movimiento mínimo hasta aceptar un movimiento en horizontal hacia la derecha. Aquí es necesario dejar un margen
        //de desplazamiento por las mismas razones que para el movimiento horizontal. Finalmente se pasa a detectar el segundo movimiento
        //en horizontal hasta alcanzar un desplazamiento mínimo. En este caso no es relevante que el usuario se pase de
        //la distancia definida debido a que una vez alcanzada dicha distancia el gesto ya se considera reconocido
        private int detectarGestoZ()
        {
            int salida = 0;
            //Etapa de reconocimiento de movimiento en horizontal hacia la derecha obligatorio antes de tener la opción de un movimiento en diagonal
            if (zHorizontal && !zDiagonal && !zHorizontal2)
            {
                //Controlamos con un margen de error que realmente se trata de un movimiento horizontal sin grandes desvíos
                //en el eje vertical con respecto del punto en que se cerró la mano para comenzar la detección de gestos
                if (Math.Abs(zInicial.Y - mano_actual.GetPosition(wrapPanel).Y) < margen_error)
                {
                    //Si se ha alcanzado el movimiento horizontal mínimo modificamos las variables para pasar al siguiente estado
                    if (mano_actual.GetPosition(wrapPanel).X - zInicial.X > 400)
                    {
                        zHorizontal = false;
                        //Inicializamos unas variables de referencia para controlar las posiciones extremo que se van alcanzando en
                        //los distintos estados
                        zMax.X = mano_actual.GetPosition(wrapPanel).X;
                        zMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z (Paso 1). Primer segmento de Z completado. Se admite un margen de movimiento adicional en horizontal", "#FFFFFFFF"));                            
                    }
                    //Si se ha producido un movimiento hacia la izquierda con respecto al punto de partida interrumpimos la detección del gesto Z
                    if (mano_actual.GetPosition(wrapPanel).X - zInicial.X < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Has movido la mano a la izquierda). Empieza de nuevo", "#FFF01010"));
                    }
                }
                else
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Te has desviado en vertical). Empieza de nuevo", "#FFF01010"));
                }
            }

            //Estado en el que se contempla un margen en horizontal antes de comenzar el movimiento en diagonal
            if (!zDiagonal && !zHorizontal && !zHorizontal2)
            {
                //En caso de un movimiento excesivo hacia la derecha dejamos de reconocer el gesto
                if (mano_actual.GetPosition(wrapPanel).X - zInicial.X > 1000)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (te has pasado moviendo la mano en horizontal). Empieza de nuevo", "#FFF01010"));
                }
                else
                {
                    //Actualizamos las posiciones extremo detectadas hasta el momento
                    if (mano_actual.GetPosition(wrapPanel).X > zMax.X)
                    {
                        zMax.X = mano_actual.GetPosition(wrapPanel).X;
                        zMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                    }
                    //Si se detecta un movimiento de subida se interrumpe
                    if (mano_actual.GetPosition(wrapPanel).Y - zInicial.Y < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Te has movido hacia arriba). Empieza de nuevo", "#FFF01010"));
                    }
                    //Si se ha detectado un movimiento en diagonal se pasa al siguiente estado
                    if (mano_actual.GetPosition(wrapPanel).X < zMax.X - margen_error / 2 && mano_actual.GetPosition(wrapPanel).Y > zInicial.Y + margen_error / 2)
                    {
                        zDiagonal = true;
                        //Nos quedamos con la última posición antes de pasar al siguiente estado para
                        //poder detectar un movimiento indebido que interrumpa el gesto
                        zInicial = mano_actual.GetPosition(wrapPanel);
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z (Paso 2). Comenzando movimiento Z en Diagonal", "#FFFFFFFF"));
                    }
                }
            }

            //Estado en el que se controla el desplazamiento mínimo obligatorio en diagonal antes de poder continuar
            //con el movimiento horizontal
            if (zDiagonal && !zHorizontal && !zHorizontal2)
            {
                //Si se ha cumplido con el desplazamiento mínimo pasamos al siguiente estado
                if (mano_actual.GetPosition(wrapPanel).X < zInicial.X - 200 && mano_actual.GetPosition(wrapPanel).Y > zInicial.Y + 200)
                {
                    zHorizontal2 = true;
                    zMax.X = mano_actual.GetPosition(wrapPanel).X;
                    zMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z (Paso 3). Movimiento en diagonal completado. Se admite un margen de movimiento adicional en diagonal", "#FFFFFFFF"));
                }
                //Si nos hemos movido en la dirección opuesta a la diagonal de bajada interrumpimos la detección del gesto
                if (mano_actual.GetPosition(wrapPanel).X > zInicial.X + margen_error / 2 && mano_actual.GetPosition(wrapPanel).Y < zInicial.Y - margen_error / 2)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Has desplazado en la diagonal opuesta). Empieza de nuevo", "#FFF01010"));
                }
            }

            //Estado en el que se contempla un margen de avanzar en diagonal antes de comenzar el movimiento horizontal
            if (zDiagonal && !zHorizontal && zHorizontal2)
            {
                //En caso de un movimiento en diagonal excesivo interrumpimos el reconocimiento del gesto
                if (mano_actual.GetPosition(wrapPanel).X < zInicial.X - 350 && mano_actual.GetPosition(wrapPanel).Y > zInicial.Y + 350)
                {
                    salida = -1;
					if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Te has movido demasiado en diagonal). Empieza de nuevo", "#FFF01010"));
                }
                else
                {
                    //Actualizamos las posiciones extremo detectadas hasta el momento
                    if (mano_actual.GetPosition(wrapPanel).X < zMax.X)
                    {
                        zMax.X = mano_actual.GetPosition(wrapPanel).X;
                        zMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                    }
                    //Si se ha detectado un movimiento de subida mayor que un margen de error se interrumpe la detección del gesto
                    if (mano_actual.GetPosition(wrapPanel).Y - zMax.Y < -margen_error/2)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Te has movido hacia arriba). Empieza de nuevo", "#FFF01010"));
                    }
                    //Si se detecta un movimiento en horizontal hacia la derecha se pasa al siguiete estado
                    if (mano_actual.GetPosition(wrapPanel).X > zMax.X + margen_error)
                    {
                        zDiagonal = false;
                        //Nos quedamos con la última posición antes de pasar al siguiente estado para
                        //poder detectar un movimiento indebido que interrumpa el gesto
                        zInicial = mano_actual.GetPosition(wrapPanel);
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z (Paso 4). Comenzando movimiento en horizontal (segmento 3)", "#FFFFFFFF"));
                    }
                }
            }

            //último estado en el que se detecta el segundo movimiento en horizontal
            if (!zDiagonal && !zHorizontal && zHorizontal2)
            {
                //Controlamos con un margen de error que realmente se trata de un movimiento horizontal sin grandes desvíos
                //en el eje vertical con respecto del punto en que comenzó el movimiento en horizontal
                if (Math.Abs(zInicial.Y - mano_actual.GetPosition(wrapPanel).Y) < margen_error)
                {
                    //Si detectamos una cantidad de movimiento horizontal devolvemos el código de
                    //reconocimiento del gesto
                    if (mano_actual.GetPosition(wrapPanel).X - zInicial.X > 600)
                    {                        
                        salida = 1;
                    }
                    //Si se ha producido un desplazamiento hacia la izquierda se interrumpe la detección del gesto
                    if (mano_actual.GetPosition(wrapPanel).X - zInicial.X < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Te has movido a la izquierda). Empieza de nuevo", "#FFF01010"));
                    }
                }
                else
                {
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto Z. Movimiento fallido (Te has movido hacia arriba). Empieza de nuevo", "#FFF01010"));
                    salida = -1;
                }
            }

            return salida;
        }

        //detección del gesto de movimiento X
        //Mediante una variables de control podemos diferenciar 5 estados distintos en el reconocimiento
        //del gesto. En primer lugar se espera que el usuario desplace la mano en diagonal hacia la derecha descendente una distancia mínima.
        //Una vez superada la distancia mínima de desplazamiento se admite que se siga desplazando la mano o bien que comience
        //el movimiento en vertical de subido. Este umbral de tolerancia es necesario debido a que es poco probable (y difícil)
        //que un usuario mueva exactamente una distancia en diagonal antes de comenzar el desplazamiento en vertical.
        //Una vez detectado el movimiento en vertical por parte del usuario se espera que se alcance una
        //distancia de movimiento mínimo hasta aceptar el segundo movimiento en diagonal hacia la izquierda descendente. Aquí es necesario dejar un margen
        //de desplazamiento por las mismas razones que para el movimiento diagonal primero. Finalmente se pasa a detectar el segundo movimiento
        //en diagonal hasta alcanzar un desplazamiento mínimo. En este caso no es relevante que el usuario se pase de
        //la distancia definida debido a que una vez alcanzada dicha distancia el gesto ya se considera reconocido
        private int detectarGestoX()
        {
            int salida = 0;
            //Estado en el que se controla el desplazamiento mínimo obligatorio en diagonal antes de poder continuar
            //con el movimiento horizontal
            if (xDiagonal && !xVertical && !xDiagonal2)
            {
                //Si se ha cumplido con el desplazamiento mínimo pasamos al siguiente estado
                if (mano_actual.GetPosition(wrapPanel).X > xInicial.X + 200 && mano_actual.GetPosition(wrapPanel).Y > xInicial.Y + 200)
                {
                    xVertical = true;
                    xMax.X = mano_actual.GetPosition(wrapPanel).X;
                    xMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X (Paso 1).  Primera diagonal de X completada. Se admite un margen de movimiento adicional en diagonal", "#FFFFFFFF"));
                }
                //Si nos hemos movido en la dirección opuesta a la diagonal de bajada interrumpimos la detección del gesto
                if (mano_actual.GetPosition(wrapPanel).X < xInicial.X - margen_error / 2 && mano_actual.GetPosition(wrapPanel).Y < xInicial.Y - margen_error / 2)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Has desplazado en la diagonal opuesta). Empieza de nuevo", "#FFF01010"));
                }
            }

            //Estado en el que se contempla un margen de avanzar en diagonal antes de comenzar el movimiento vertical
            if (xDiagonal && xVertical && !xDiagonal2)
            {
                //En caso de un movimiento en diagonal excesivo interrumpimos el reconocimiento del gesto
                if (mano_actual.GetPosition(wrapPanel).X > xInicial.X + 500 && mano_actual.GetPosition(wrapPanel).Y > xInicial.Y + 400)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Te has movido demasiado en diagonal). Empieza de nuevo", "#FFF01010"));
                }
                else
                {
                    //Actualizamos las posiciones extremo detectadas hasta el momento
                    if (mano_actual.GetPosition(wrapPanel).Y > xMax.Y)
                        xMax.Y = mano_actual.GetPosition(wrapPanel).Y;

                    //Si se detecta un movimiento en horizontal hacia la derecha se pasa al siguiente estado
                    if (mano_actual.GetPosition(wrapPanel).Y - xMax.Y < -margen_error / 2)
                    {
                        xDiagonal = false;
                        //Nos quedamos con la última posición antes de pasar al siguiente estado para
                        //poder detectar un movimiento indebido que interrumpa el gesto
                        xInicial = mano_actual.GetPosition(wrapPanel);
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X (Paso 2). Comenzando movimiento en vertical", "#FFFFFFFF"));
                    }

                    //Si se ha detectado un movimiento de subida mayor que un margen de error se interrumpe la detección del gesto
                    if (mano_actual.GetPosition(wrapPanel).X < xMax.X - margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Te has movido hacia arriba). Empieza de nuevo", "#FFF01010"));
                    }
                }
            }
            
            //Estado en el que se detecta el movimiento en vertical
            if (!xDiagonal && xVertical && !xDiagonal2)
            {
                //Si se detecta desplazamiento en horizontal mayor que un margen de error se interrumpe
                //el reconocimiento del gesto
                if (Math.Abs(xInicial.X - mano_actual.GetPosition(wrapPanel).X) < margen_error)
                {
                    //En caso de satisfacer el desplazamiento mínimo en vertical pasamos al siguiente estado
                    if (xInicial.Y - mano_actual.GetPosition(wrapPanel).Y > 200)
                    {
                        xDiagonal2 = true;
                        xMax.X = mano_actual.GetPosition(wrapPanel).X;
                        xMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X (Paso 3). Movimiento en vertical completado. Se admite un margen de movimiento adicional en vertical", "#FFFFFFFF"));
                    }
                    //Si se detecta un movimiento de descenso se interrumpe la detección del gesto
                    if (xInicial.Y - mano_actual.GetPosition(wrapPanel).Y < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Has movido la mano hacia abajo). Empieza de nuevo", "#FFF01010"));
                    }
                }
                else
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Te has desviado en horizontal). Empieza de nuevo", "#FFF01010"));
                }
            }

            //Estado en el que se contempla un margen en vertical antes de comenzar el movimiento en diagonal
            if (!xDiagonal && xVertical && xDiagonal2)
            {
                //En caso de un movimiento excesivo hacia arriba dejamos de reconocer el gesto
                if (xInicial.Y - mano_actual.GetPosition(wrapPanel).Y > 500)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (has subido demasiado la mano). Empieza de nuevo", "#FFF01010"));
                }
                else
                {
                    //Actualizamos las posiciones extremo detectadas hasta el momento
                    if (mano_actual.GetPosition(wrapPanel).Y < xMax.Y)
                    {
                        xMax.X = mano_actual.GetPosition(wrapPanel).X;
                        xMax.Y = mano_actual.GetPosition(wrapPanel).Y;
                    }
                    //Si se detecta un movimiento de bajada se interrumpe
                    if (xInicial.Y - mano_actual.GetPosition(wrapPanel).Y < -margen_error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Has bajado la mano). Empieza de nuevo", "#FFF01010"));
                    }
                    //Si se detecta un movimiento en diagonal se pasa al siguiente estado
                    if (mano_actual.GetPosition(wrapPanel).X < xMax.X - margen_error / 2 && mano_actual.GetPosition(wrapPanel).Y > xMax.Y + margen_error / 2)
                    {
                        xVertical = false;
                        //Nos quedamos con la última posición antes de pasar al siguiente estado para
                        //poder detectar un movimiento indebido interrumpa el gesto
                        xInicial = mano_actual.GetPosition(wrapPanel);
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X (Paso 4). Comenzando segunda diagonal de la X", "#FFFFFFFF"));
                    }
                }
            }

            //último estado en el que se detecta el segundo movimiento en diagonal, esta vez descendente hacia la izquierda
            if (!xDiagonal && !xVertical && xDiagonal2)
            {
                //Si detectamos una cantidad de movimiento diagonal devolvemos el código de
                //reconocimiento del gesto
                if (mano_actual.GetPosition(wrapPanel).X < xInicial.X -250 && mano_actual.GetPosition(wrapPanel).Y > xInicial.Y + 200)
                {
                    salida = 1;
                }

                //Si se ha producido un desplazamiento en la dirección opuesta se interrumpe la detección del gesto
                if (mano_actual.GetPosition(wrapPanel).X > xInicial.X + margen_error / 2 && mano_actual.GetPosition(wrapPanel).Y < xMax.Y - margen_error / 2)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Gesto X. Movimiento fallido (Te has movido en la diagonal opuesta). Empieza de nuevo", "#FFF01010"));
                }
            }

            return salida;
        }
    }
}
