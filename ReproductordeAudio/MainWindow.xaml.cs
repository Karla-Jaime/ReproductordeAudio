using System;
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

using Microsoft.Win32;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

//Hilo -- proceso de la comp. threads
using System.Windows.Threading;

namespace ReproductordeAudio
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer; //intervalos de tiempo
        //LeerArchivo
        AudioFileReader reader;
        //ComunicaciónConLaTarjetaDeAudio

        //ExclusivoParaSalida
        WaveOut output;

        bool dragging = false;


        public MainWindow()
        {
            InitializeComponent();
            ListarDispositivosSalida();
            btnReproducir.IsEnabled = false;
            btnDetener.IsEnabled = false;                           
            btnPausa.IsEnabled = false;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            //Para añadir respuesta a eventos
            timer.Tick += Timer_Tick; 
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            
            lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0,8);
            //Actualiza 
            if (!dragging)
            {
                sldTiempo.Value = reader.CurrentTime.TotalSeconds;
            }
        }

        void ListarDispositivosSalida()
        {
            cbDispositivoSalida.Items.Clear();
            for( int i=0; i < WaveOut.DeviceCount; i++)
            {//Guarda las caracteristicas del Disp
                WaveOutCapabilities capacidades = WaveOut.GetCapabilities(i);
                cbDispositivoSalida.Items.Add(capacidades.ProductName);
            }
            cbDispositivoSalida.SelectedIndex = 0;
        }

        private void btnExaminar_Click(object sender, RoutedEventArgs e)
        {//Inicializarydar valor
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                txtRutaArchivo.Text = openFileDialog.FileName;
                btnReproducir.IsEnabled = true;
            }
        }
        private void btnReproducir_Click(object sender, RoutedEventArgs e)
        { 
            if(output != null && output.PlaybackState == PlaybackState.Paused)
            {
                //Retomo reproduccion
                output.Play();
                btnReproducir.IsEnabled = false;
                btnPausa.IsEnabled = true;
                btnDetener.IsEnabled = true;
            }
            else
            {
                //Inicializar y dar valor
                if (txtRutaArchivo.Text != null && txtRutaArchivo.Text != string.Empty)
                {
                    reader = new AudioFileReader(txtRutaArchivo.Text);
                    output = new WaveOut();

                    output.DeviceNumber = cbDispositivoSalida.SelectedIndex;

                    output.PlaybackStopped += Output_PlaybackStopped;
                    output.Init(reader);
                    output.Play();
                    btnReproducir.IsEnabled = false;
                    btnDetener.IsEnabled = true;
                    btnPausa.IsEnabled = true;

                    lblTiempoTotal.Text = reader.TotalTime.ToString().Substring(0,8);
                    //Se usara un hilo para cambiar la etiqueta 
                    //
                    sldTiempo.Maximum = reader.TotalTime.TotalSeconds;
                    sldTiempo.Value = reader.CurrentTime.TotalSeconds;

                    timer.Start();                                                       }
            }
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            timer.Stop();

            reader.Dispose();
            output.Dispose(); 
        }

        private void btnDetener_Click(object sender, RoutedEventArgs e)
        {            
            if (output != null)
            {
                output.Stop();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnDetener.IsEnabled = false;
            }            
        }

        private void btnPausa_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                output.Pause();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnDetener.IsEnabled = true;
            }            
        }
        //EVENTO Iniciar mov sld
        private void SldTiempo_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragging = true;
        }
        //EVENTO Terminar mov sld / cuando se suelta el sld
        private void SldTiempo_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            dragging = false;
            if(reader != null && output != null && output.PlaybackState != PlaybackState.Stopped){
                reader.CurrentTime = TimeSpan.FromSeconds(sldTiempo.Value);
            }
        }
    }
}
