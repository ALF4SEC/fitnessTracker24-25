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
    public class EjecucionSelecionadaEventArgs : EventArgs
    {
        public Ejecucion Ejecucion { get; set; }
        public EjecucionSelecionadaEventArgs(Ejecucion e) { Ejecucion = e; }
    }

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
            this.mainWindow = mainWindow; // Guarda la referencia a la ventana principal
            ejercicio = ejercicioSeleccionado;
            ActualizarEjecuciones(ejercicioSeleccionado);
            DibujarGraficoBarras(ejercicioSeleccionado);
            CanvasGrafico.SizeChanged += (s, e) => DibujarGraficoBarras(ejercicioSeleccionado); Title = $"Detalles ejecicio: {ejercicioSeleccionado.NombreEjercicio}";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            addEjecucionWindow = new AddEjecucionWindow();
            addEjecucionWindow.ShowDialog();
            addEjecucionWindow.Owner = this;
            if (addEjecucionWindow.DialogResult == true)
            {
                Ejecucion nuevaEjecucion = new Ejecucion(addEjecucionWindow.RepeticionesTextBox.IntValue, addEjecucionWindow.PesoTextBox.DoubleValue, FormarFechaHora(addEjecucionWindow.FechaPicker.SelectedDate.Value, addEjecucionWindow.HoraPicker.SelectedDateTime.Value));
                ejercicio.ejecuciones.Add(nuevaEjecucion);
                ActualizarEjecuciones(ejercicio);
                DibujarGraficoBarras(ejercicio);
            }
        }

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
                ActualizarEjecuciones(ejercicio);
                DibujarGraficoBarras(ejercicio);
            }
            ejecucionSeleccionado = null;
            NuevaSelecionEjecucion?.Invoke(this, new EjecucionSelecionadaEventArgs(ejecucionSeleccionado)); //Manda el evento al MainWindows con la selecionada
                                                                                                            //en este caso un null porque se he eliminado
        }

        private void ListViewEjecuciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ejecucion ejecucionSeleccionada = (Ejecucion)DataGridEjecuciones.SelectedItem;

            if (ejecucionSeleccionada == null) //No hay fecha selecionada 
            {
                DeleteButton.IsEnabled = false;
            }
            else
            {
                DeleteButton.IsEnabled = true;
            }
            NuevaSelecionEjecucion?.Invoke(this, new EjecucionSelecionadaEventArgs(ejecucionSeleccionada)); //Manda el evento al MainWindows con la selecionada o un null si no hay ninguna
        }

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

        public void ActualizarEjecuciones(Ejercicio ejercicio)
        {
            // Actualiza el DataContext para reflejar los cambios
            DataGridEjecuciones.ItemsSource = ejercicio.ejecuciones.OrderBy(x => x.FechaHora);

            // Opcional: Puedes actualizar el título de la ventana
            Title = $"Detalles ejecicio: {ejercicio.NombreEjercicio}";
        }

        public void ActualizarGraficoBarras(Ejercicio ejercicio)
        {
            TituloLabel.Content = ejercicio.NombreEjercicio;
            DibujarGraficoBarras(ejercicio);
        }

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

        private void DibujarGraficoBarras(Ejercicio ejercicio)
        {
            CanvasGrafico.Children.Clear();

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

            // Configurar dimensiones del gráfico
            double anchoCanvas = CanvasGrafico.ActualWidth > 0 ? CanvasGrafico.ActualWidth : 700;
            double altoCanvas = CanvasGrafico.ActualHeight > 0 ? CanvasGrafico.ActualHeight : 300;
            double margenIzquierdo = 60;
            double margenDerecho = 60;
            double margenSuperior = 40;
            double margenInferior = 60;

            // Escalas
            double maxRepeticiones = 100; // Máximo de 100 repeticiones
            double maxPeso = ejercicio.ejecuciones.Max(e => e.Peso); // Máximo peso alcanzado
            double escalaAlturaRepeticiones = (altoCanvas - margenSuperior - margenInferior) / maxRepeticiones;
            double escalaAlturaPeso = maxPeso > 0 ? (altoCanvas - margenSuperior - margenInferior) / maxPeso : 0;

            // Dibujar el eje Y para repeticiones
            for (int i = 0; i <= maxRepeticiones; i += 10)
            {
                double y = altoCanvas - margenInferior - (i * escalaAlturaRepeticiones);

                // Línea del eje Y
                var linea = new Line
                {
                    X1 = margenIzquierdo,
                    Y1 = y,
                    X2 = anchoCanvas - margenDerecho,
                    Y2 = y,
                    Stroke = Brushes.Blue,
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
                double anchoBarra = (anchoCanvas - margenIzquierdo - margenDerecho) / ejecucionesPorDia.Count * 0.8; // Espacio entre días

                foreach (var ejecucion in ejecucionDia.Value)
                {
                    double alturaBarra = ejecucion.Repeticiones * escalaAlturaRepeticiones;
                    double y = altoCanvas - margenInferior - alturaBarra;

                    // Dibujar la barra
                    var barra = new Rectangle
                    {
                        Width = anchoBarra / ejecucionDia.Value.Count, // Sin separación entre barras del mismo día
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
                    Canvas.SetTop(repLabel, y - 15); // Justo encima de la barra
                    CanvasGrafico.Children.Add(repLabel);

                    // Agregar punto a la polilínea
                    double yMaxPeso = maxPeso > 0 ? altoCanvas - margenInferior - (ejecucion.Peso * escalaAlturaPeso) : altoCanvas - margenInferior;
                    puntosPolilinea.Add(new Point(x + (anchoBarra / ejecucionDia.Value.Count * 0.5), yMaxPeso));

                    // Dibujar punto en la polilínea con ToolTip
                    var punto = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = Brushes.Red,
                        Tag = ejecucion // Usar la propiedad Tag para almacenar la ejecución
                    };
                    Canvas.SetLeft(punto, x + (anchoBarra / ejecucionDia.Value.Count * 0.5) - 2.5);
                    Canvas.SetTop(punto, yMaxPeso - 2.5);
                    CanvasGrafico.Children.Add(punto);

                    // Asignar eventos de mouse para mostrar ToolTip
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

                // Etiqueta de la fecha en el eje X
                var fechaLabel = new TextBlock
                {
                    Text = ejecucionDia.Key.ToShortDateString(),
                    FontSize = 10,
                    RenderTransform = new RotateTransform(45)
                };
                Canvas.SetLeft(fechaLabel, x - (anchoBarra / ejecucionDia.Value.Count * 0.5) - 20); // Centrar la fecha en la barra
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

            // Etiqueta del eje X
            var etiquetaX = new TextBlock
            {
                Text = "Fecha",
                FontSize = 12,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(etiquetaX, anchoCanvas / 2 - 20);
            Canvas.SetTop(etiquetaX, altoCanvas - margenInferior + 40);
            CanvasGrafico.Children.Add(etiquetaX);

            // Suscribirse al evento SizeChanged para redibujar el gráfico cuando cambie el tamaño del Canvas
            CanvasGrafico.SizeChanged += (s, e) => DibujarGraficoBarras(ejercicio);
        }
    }
}


