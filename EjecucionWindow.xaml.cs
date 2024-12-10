using fitnessTracker24_25.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using fitnessTracker24_25.Vista;
using ControlzEx.Standard;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI;


namespace fitnessTracker24_25
{
    //Argumentos para el evento que se dispara al seleccionar una ejecución
    public class EjecucionSelecionadaEventArgs : EventArgs
    {
        public Ejecucion Ejecucion { get; set; }
        public EjecucionSelecionadaEventArgs(Ejecucion e) { Ejecucion = e; }
    }
    
    //Delegado para manejar eventos de selección de ejecuciones
    public delegate void EjecucionSelecionadaEventHandler(Object sender, EjecucionSelecionadaEventArgs e);

    public partial class EjecucionWindow : Window
    {
        private MainWindow mainWindow;
        private AddEjecucionWindow addEjecucionWindow;
        public event EjecucionSelecionadaEventHandler NuevaSelecionEjecucion;
        public ObservableCollection<Ejecucion> Ejecuciones { get; set; }
        private Ejercicio ejercicio;

        public EjecucionWindow(Ejercicio ejercicioSeleccionado, MainWindow mainWindow)
        {
            InitializeComponent();
            /*Guarda la referencia a la ventana principal, para que si se cierre la ventana principal,
            se cierre esta ventana en caso de estar abierta. Esto hace que esta ventana se pueda abrir de forma no modal pero
            que no pueda estar ejecutandose sin la ventana principal*/
            this.mainWindow = mainWindow; 
            ejercicio = ejercicioSeleccionado;
            //Actualizar la interfaz para reflejar los cambios
            ActualizarEjecuciones(ejercicioSeleccionado);
            DibujarGraficoBarras(ejercicioSeleccionado);
            //Redibujar el gráfico al cambiar el tamaño del canvas
            CanvasGrafico.SizeChanged += (s, e) => DibujarGraficoBarras(ejercicioSeleccionado); 
            Title = "Detalles ejecicio: {ejercicioSeleccionado.NombreEjercicio}";
        }

        //Manejador del evento Click del boton Aceptar
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Abrimos la ventana para introducir los datos de la nueva ejecucion de forma modal
            addEjecucionWindow = new AddEjecucionWindow();
            addEjecucionWindow.ShowDialog();
            addEjecucionWindow.Owner = this;
            if (addEjecucionWindow.DialogResult == true)
            {
                Ejecucion nuevaEjecucion = new Ejecucion(addEjecucionWindow.RepeticionesTextBox.IntValue, addEjecucionWindow.PesoTextBox.DoubleValue, FormarFechaHora(addEjecucionWindow.FechaPicker.SelectedDate.Value, addEjecucionWindow.HoraPicker.SelectedDateTime.Value));
                ejercicio.ejecuciones.Add(nuevaEjecucion);
                ActualizarEjecuciones(ejercicio);
                DibujarGraficoBarras(ejercicio);
                mainWindow.DibujarGraficoRadial(addEjecucionWindow.FechaPicker.SelectedDate.Value);
            }
        }

        //Manejador del evento Click del boton Delete
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Ejecucion ejecucionSeleccionado = (Ejecucion)DataGridEjecuciones.SelectedItem;
            string msg = "Seguro que deseas eliminar el la ejecucion: " + ejecucionSeleccionado.FechaHora;
            string titulo = "¿Quieres eliminar?";
            MessageBoxButton btn = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;
            MessageBoxResult result = MessageBox.Show(msg, titulo, btn, icon);
            if (result == MessageBoxResult.Yes)
            {
                ejercicio.ejecuciones.Remove(ejecucionSeleccionado);
                //Actualizar la interfaz para reflejar los cambios
                ActualizarEjecuciones(ejercicio);
                DibujarGraficoBarras(ejercicio);
            }
            mainWindow.DibujarGraficoRadial(ejecucionSeleccionado.FechaHora);
            ejecucionSeleccionado = null;
            NuevaSelecionEjecucion?.Invoke(this, new EjecucionSelecionadaEventArgs(ejecucionSeleccionado)); //Manda el evento al MainWindows con la selecionada
        }

        //Método para manejar cambios en la selección del DataGrid
        private void ListViewEjecuciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ejecucion ejecucionSeleccionada = (Ejecucion)DataGridEjecuciones.SelectedItem;

            //Habilitar o deshabilitar el botón de eliminar, si hay o no una seleccion
            if (ejecucionSeleccionada == null)
            {
                DeleteButton.IsEnabled = false;
            }
            else
            {
                DeleteButton.IsEnabled = true;
            }
            // Notificar la selección actual
            NuevaSelecionEjecucion?.Invoke(this, new EjecucionSelecionadaEventArgs(ejecucionSeleccionada));
        }

        //Método para manejar doble clic en el DataGrid
        private void DataGridEjecuciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Verificar si hay una ejecución seleccionada
            if (DataGridEjecuciones.SelectedItem is Ejecucion ejecucionSeleccionada)
            {
                // Actualizar la fecha seleccionada en el DatePicker
                if (mainWindow != null)
                {
                    mainWindow.DatePickerGrafico.SelectedDate = ejecucionSeleccionada.FechaHora;
                }
                else
                {
                    MessageBox.Show("No se pudo encontrar la ventana principal.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No se ha seleccionado ninguna ejecución.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Método para actualizar el DataGrid con las ejecuciones ordenadas cronológicamente
        public void ActualizarEjecuciones(Ejercicio ejercicio)
        {
            //Hacemos que las ejecuciones se ordenen cronológicamente
            DataGridEjecuciones.ItemsSource = ejercicio.ejecuciones.OrderBy(x => x.FechaHora);

            //Modificamos el titulo de la ventana con el nombre del ejercicio
            Title = $"Detalles ejecicio: {ejercicio.NombreEjercicio}";
        }

        //Método para actualizar el gráfico de barras
        public void ActualizarGraficoBarras(Ejercicio ejercicio)
        {
            TituloLabel.Content = ejercicio.NombreEjercicio;
            DibujarGraficoBarras(ejercicio);
        }

        /*Esta funcion formatea la fecha y la hora*/
        /*Con esta funcion componemos la fecha y la hora de una ejecucion.*/
        private DateTime FormarFechaHora(DateTime fechaSeleccionada, DateTime horaSeleccionada)
        {
            DateTime fechaCompleta = new DateTime(
                fechaSeleccionada.Year,
                fechaSeleccionada.Month,
                fechaSeleccionada.Day,
                horaSeleccionada.Hour,
                horaSeleccionada.Minute,
                horaSeleccionada.Second);

            return fechaCompleta;
        }

        //Método para dibujar un gráfico de barras que muestra repeticiones por día y el peso en las ejecuciones
        private void DibujarGraficoBarras(Ejercicio ejercicio)
        {
            // Limpiar el canvas
            CanvasGrafico.Children.Clear();

            // Verificar si hay ejecuciones disponibles
            if (ejercicio.ejecuciones == null || !ejercicio.ejecuciones.Any())
            {
                MessageBox.Show("No hay ejecuciones registradas para este ejercicio.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Agrupar ejecuciones por fecha
            var ejecucionesPorDia = ejercicio.ejecuciones
                .GroupBy(e => e.FechaHora.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Configurar dimensiones y márgenes del gráfico
            double anchoCanvas = CanvasGrafico.ActualWidth > 0 ? CanvasGrafico.ActualWidth : 700;
            double altoCanvas = CanvasGrafico.ActualHeight > 0 ? CanvasGrafico.ActualHeight : 300;
            double margenIzquierdo = 60;
            double margenDerecho = 60;
            double margenSuperior = 40;
            double margenInferior = 60;

            // Escalar las barras según el máximo de repeticiones y peso
            double maxRepeticiones = ejercicio.ejecuciones.Max(e => e.Repeticiones);
            double maxPeso = ejercicio.ejecuciones.Max(e => e.Peso);
            double escalaAlturaRepeticiones = (altoCanvas - margenSuperior - margenInferior) / maxRepeticiones;
            double escalaAlturaPeso = maxPeso > 0 ? (altoCanvas - margenSuperior - margenInferior) / maxPeso : 0;

            // Dibujar el eje Y para repeticiones (variando de 2 en 2)
            for (int i = 0; i <= maxRepeticiones; i += 2)
            {
                double y = altoCanvas - margenInferior - (i * escalaAlturaRepeticiones);

                // Línea del eje Y
                var linea = new Line
                {
                    X1 = margenIzquierdo,
                    Y1 = y,
                    X2 = anchoCanvas - margenDerecho,
                    Y2 = y,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                CanvasGrafico.Children.Add(linea);

                // Etiqueta del eje Y
                var etiquetaY = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 10,
                    Foreground = Brushes.Blue
                };
                Canvas.SetLeft(etiquetaY, margenIzquierdo - 30);
                Canvas.SetTop(etiquetaY, y - 10);
                CanvasGrafico.Children.Add(etiquetaY);
            }

            // Etiqueta del eje Y para repeticiones
            var etiquetaYRepeticiones = new TextBlock
            {
                Text = "Repeticiones",
                FontSize = 12,
                Foreground = Brushes.Blue,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(etiquetaYRepeticiones, margenIzquierdo - 50);
            Canvas.SetTop(etiquetaYRepeticiones, margenSuperior - 25);
            CanvasGrafico.Children.Add(etiquetaYRepeticiones);

            // Dibujar el eje Y derecho para peso
            if (maxPeso > 0)
            {
                for (int i = 0; i <= maxPeso; i += 10)
                {
                    double y = altoCanvas - margenInferior - (i * escalaAlturaPeso);

                    // Etiqueta del eje Y derecho
                    var etiquetaYDerecho = new TextBlock
                    {
                        Text = i.ToString(),
                        FontSize = 10,
                        Foreground = Brushes.Red
                    };
                    Canvas.SetLeft(etiquetaYDerecho, anchoCanvas - margenDerecho + 5);
                    Canvas.SetTop(etiquetaYDerecho, y - 10);
                    CanvasGrafico.Children.Add(etiquetaYDerecho);
                }
            }

            // Etiqueta del eje Y derecho para peso
            var etiquetaYPeso = new TextBlock
            {
                Text = "Peso (kg)",
                FontSize = 12,
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(etiquetaYPeso, anchoCanvas - margenDerecho + 5);
            Canvas.SetTop(etiquetaYPeso, margenSuperior - 20);
            CanvasGrafico.Children.Add(etiquetaYPeso);

            // Dibujar el eje X y las barras
            int j = 0;
            var puntosPolilinea = new PointCollection();
            foreach (var ejecucionDia in ejecucionesPorDia)
            {
                double x = margenIzquierdo + j * (anchoCanvas - margenIzquierdo - margenDerecho) / ejecucionesPorDia.Count;
                double anchoBarra = (anchoCanvas - margenIzquierdo - margenDerecho) / ejecucionesPorDia.Count * 0.8;
                double xInicial = x; // Guardar posición inicial de las barras para calcular el centro

                foreach (var ejecucion in ejecucionDia.Value)
                {
                    double alturaBarra = ejecucion.Repeticiones * escalaAlturaRepeticiones;
                    double y = altoCanvas - margenInferior - alturaBarra;

                    // Dibujar la barra
                    var barra = new Rectangle
                    {
                        Width = anchoBarra / ejecucionDia.Value.Count,
                        Height = alturaBarra,
                        Fill = Brushes.Blue
                    };
                    Canvas.SetLeft(barra, x);
                    Canvas.SetTop(barra, y);
                    CanvasGrafico.Children.Add(barra);

                    // Etiqueta del total de repeticiones
                    var repLabel = new TextBlock
                    {
                        Text = ejecucion.Repeticiones.ToString(),
                        FontSize = 10,
                        Foreground = Brushes.Black
                    };
                    Canvas.SetLeft(repLabel, x + (anchoBarra / ejecucionDia.Value.Count * 0.3));
                    Canvas.SetTop(repLabel, y - 15);
                    CanvasGrafico.Children.Add(repLabel);

                    // Agregar punto a la polilínea
                    double yMaxPeso = maxPeso > 0 ? altoCanvas - margenInferior - (ejecucion.Peso * escalaAlturaPeso) : altoCanvas - margenInferior;
                    var puntoPolilinea = new Point(x + (anchoBarra / ejecucionDia.Value.Count * 0.5), yMaxPeso);
                    puntosPolilinea.Add(puntoPolilinea);

                    // Dibujar el punto
                    var punto = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = Brushes.Red,
                        Tag = ejecucion
                    };
                    Canvas.SetLeft(punto, puntoPolilinea.X - 4); // Centrar
                    Canvas.SetTop(punto, puntoPolilinea.Y - 4); // Centrar
                    CanvasGrafico.Children.Add(punto);

                    // Agregar eventos para el tooltip
                    punto.MouseEnter += (s, e) =>
                    {
                        var el = s as Ellipse;
                        var ejec = el.Tag as Ejecucion;
                        var tooltip = new ToolTip { Content = $"Fecha: {ejec.FechaHora}\nPeso: {ejec.Peso} kg" };
                        el.ToolTip = tooltip;
                        tooltip.IsOpen = true;
                    };

                    punto.MouseLeave += (s, e) =>
                    {
                        var el = s as Ellipse;
                        var tooltip = el.ToolTip as ToolTip;
                        if (tooltip != null)
                        {
                            tooltip.IsOpen = false;
                        }
                    };

                    x += anchoBarra / ejecucionDia.Value.Count;
                }

                // Calcular el centro de las barras para la fecha
                double xCentro = xInicial + (anchoBarra / 2);

                // Dibujar etiqueta de la fecha
                var fechaLabel = new TextBlock
                {
                    Text = ejecucionDia.Key.ToShortDateString(),
                    FontSize = 10,
                    RenderTransform = new RotateTransform(45),
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(fechaLabel, xCentro - 20); // Ajustar para centrar la etiqueta
                Canvas.SetTop(fechaLabel, altoCanvas - margenInferior + 5);
                CanvasGrafico.Children.Add(fechaLabel);

                j++;
            }

            // Dibujar la polilínea
            var polilinea = new Polyline
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Points = puntosPolilinea
            };
            CanvasGrafico.Children.Add(polilinea);
        }
    }
}


