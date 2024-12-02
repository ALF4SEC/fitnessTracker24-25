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
        private AddEjecucionWindow addEjecucionWindow;
        public event EjecucionSelecionadaEventHandler NuevaSelecionEjecucion;
        public ObservableCollection<Ejecucion> Ejecuciones { get; set; }
        private Ejercicio ejercicio;

        public EjecucionWindow(Ejercicio ejercicioSeleccionado)
        {
            InitializeComponent();
            ejercicio = ejercicioSeleccionado;
            DataGridEjecuciones.ItemsSource = ejercicioSeleccionado.ejecuciones;
            DibujarGraficoBarras(ejercicioSeleccionado);
            Title = $"Detalles ejecicio: {ejercicioSeleccionado.NombreEjercicio}";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            addEjecucionWindow = new AddEjecucionWindow();
            addEjecucionWindow.ShowDialog();
            addEjecucionWindow.Owner = this;
            if (addEjecucionWindow.DialogResult == true)
            {
                Ejecucion nuevaEjecucion = new Ejecucion(addEjecucionWindow.RepeticionesTextBox.IntValue, addEjecucionWindow.PesoTextBox.DoubleValue, formarFechaHora(addEjecucionWindow.FechaPicker.SelectedDate.Value, addEjecucionWindow.HoraPicker.SelectedDateTime.Value));
                ejercicio.ejecuciones.Add(nuevaEjecucion);
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

        public void ActualizarEjecuciones(Ejercicio ejercicio)
        {
            // Actualiza el DataContext para reflejar los cambios
            DataGridEjecuciones.ItemsSource = ejercicio.ejecuciones;

            // Opcional: Puedes actualizar el título de la ventana
            Title = $"Detalles ejecicio: {ejercicio.NombreEjercicio}";
        }

        public void ActualizarGraficoBarras(Ejercicio ejercicio)
        {
            TituloLabel.Content = ejercicio.NombreEjercicio;
            DibujarGraficoBarras(ejercicio);
        }

        private DateTime formarFechaHora(DateTime fechaSeleccionada, DateTime horaSeleccionada)
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

            // Agrupar ejecuciones por fecha y calcular totales
            var ejecucionesPorDia = ejercicio.ejecuciones
                .GroupBy(e => e.FechaHora.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Repeticiones));

            // Configurar dimensiones del gráfico
            double anchoCanvas = CanvasGrafico.ActualWidth > 0 ? CanvasGrafico.ActualWidth : 700;
            double altoCanvas = CanvasGrafico.ActualHeight > 0 ? CanvasGrafico.ActualHeight : 300;
            double margen = 20;

            // Escalas
            double anchoBarra = (anchoCanvas - 2 * margen) / ejecucionesPorDia.Count;
            double maxRepeticiones = ejecucionesPorDia.Values.Max();
            double escalaAltura = (altoCanvas - 2 * margen) / maxRepeticiones;

            int i = 0;
            foreach (var ejecucion in ejecucionesPorDia)
            {
                // Coordenadas de la barra
                double x = margen + i * anchoBarra;
                double alturaBarra = ejecucion.Value * escalaAltura;
                double y = altoCanvas - margen - alturaBarra;

                // Dibujar la barra
                var barra = new Rectangle
                {
                    Width = anchoBarra * 0.8, // Un poco de separación entre barras
                    Height = alturaBarra,
                    Fill = Brushes.Blue
                };
                Canvas.SetLeft(barra, x + (anchoBarra * 0.1)); // Ajustar para centrar la barra
                Canvas.SetTop(barra, y);
                CanvasGrafico.Children.Add(barra);

                // Etiqueta de la fecha
                var fechaLabel = new TextBlock
                {
                    Text = ejecucion.Key.ToShortDateString(),
                    FontSize = 10,
                    RenderTransform = new RotateTransform(45)
                };
                Canvas.SetLeft(fechaLabel, x + (anchoBarra * 0.1));
                Canvas.SetTop(fechaLabel, altoCanvas - margen + 5);
                CanvasGrafico.Children.Add(fechaLabel);

                // Etiqueta del total de repeticiones
                var repLabel = new TextBlock
                {
                    Text = ejecucion.Value.ToString(),
                    FontSize = 10,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(repLabel, x + (anchoBarra * 0.3));
                Canvas.SetTop(repLabel, y - 15); // Justo encima de la barra
                CanvasGrafico.Children.Add(repLabel);

                i++;
            }

        }
    }
}

