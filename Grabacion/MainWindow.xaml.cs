﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Wave;
using NAudio.Dsp;



namespace Grabacion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveIn wavein;
        WaveFormat formato;
        WaveFileWriter writer;
        AudioFileReader reader;
        WaveOutEvent waveOut;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btniniciar_Click(object sender, RoutedEventArgs e)
        {
            
            wavein = new WaveIn();
            wavein.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = wavein.WaveFormat;

            wavein.DataAvailable += OnDaraAvailable;
            wavein.RecordingStopped += OnRectodingStopped;
            writer =
                new WaveFileWriter("sonido.wav", formato);

            wavein.StartRecording();
        }

        void OnRectodingStopped(object sender, StoppedEventArgs e)
        {
            writer.Dispose();
        }

        void OnDaraAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;

            double acumulador = 0;
            double nummuestras = bytesGrabados / 2;
            int exponente = 1;
            int numeroMuestrasComplejas = 0;
            int bitsMaximos = 0;

            do
            {
                bitsMaximos = (int)Math.Pow(2, exponente);
                exponente++;
            } while (bitsMaximos < nummuestras);

            exponente -= 2;
            numeroMuestrasComplejas = bitsMaximos / 2;

            Complex[] muestrasComplejas =
                new Complex[5];
            for (int i=0; i < bytesGrabados; i+=2)
            {
                short muestra = (short)(buffer[i + 1] << 8 | buffer[i]);

                float muestra32bits = (float)muestra / 32768.0f;

                slbvolumen.Value = Math.Abs(muestra32bits);

                if(i/2 < numeroMuestrasComplejas)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }
                

               // acumulador += muestra;
                //nummuestras++; meow
            }
            //double promedio = acumulador / nummuestras;
            //slbvolumen.Value = promedio;

            //writer.Write(buffer, 0, bytesGrabados);

            FastFourierTransform.FFT(true, exponente, muestrasComplejas);
            float[] valoresAbsolutos = new float[muestrasComplejas.Length];

            for(int i=0; 1 < muestrasComplejas.Length; i++)
            {
                valoresAbsolutos[i] = (float)
                    Math.Sqrt((muestrasComplejas[i].X * muestrasComplejas[i].X) + (muestrasComplejas[i].Y * muestrasComplejas[i].Y));
            }
            int indiceMaximo =
                valoresAbsolutos.ToList().IndexOf
                  (valoresAbsolutos.Max());



        }

        private void btnfinalizar_Click(object sender, RoutedEventArgs e)
        {
            wavein.StopRecording();
        }

        private void btnReproducir_Click(object sender, RoutedEventArgs e)
        {
            reader = new AudioFileReader("sonido.wav");
            waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
        }

        private void slbvolumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
