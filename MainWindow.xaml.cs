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
using System.Collections;
using fitnessTracker24_25.Modelo;
using fitnessTracker24_25.Vista;
using System.Collections.ObjectModel;

namespace fitnessTracker24_25
{
    public class EjercicioSelecionadaEventArgs : EventArgs
    {
        public Ejercicio Ejercicio { get; set; }
        public EjercicioSelecionadaEventArgs(Ejercicio e) { Ejercicio = e; }
    }

    public delegate void EjercicioSelecionadaEventHandler(Object sender, EjercicioSelecionadaEventArgs e);

    public partial class MainWindow : Window
    {
        AddEjercicioWindow addEjercicioWindow;
        private EjecucionWindow ejecucionWindow;
        public ObservableCollection<Ejercicio> ejercicios;
        public event EjercicioSelecionadaEventHandler NuevaSeleccionEjercicio;


        public MainWindow()
        {
            InitializeComponent();

            // Inicializar colección de ejercicios
            ejercicios = new ObservableCollection<Ejercicio>
            {
                new Ejercicio("Dominadas", "Ejercicio para espalda", new List<string> { "Espalda", "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(10, 50.0, System.DateTime.Now.AddMinutes(-30)),
                        new Ejecucion(8, 55.0, System.DateTime.Now.AddMinutes(-15)),
                        new Ejecucion(6, 60.0, System.DateTime.Now.AddMinutes(-5))
                    }
                },
                new Ejercicio("Press de banca", "Ejercicio para pecho", new List<string> { "Pecho" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(12, 75.0, System.DateTime.Now.AddMinutes(-60)),
                        new Ejecucion(10, 80.0, System.DateTime.Now.AddMinutes(-30)),
                        new Ejecucion(8, 85.0, System.DateTime.Now)
                    }
                },
                new Ejercicio("Sentadillas", "Ejercicio para piernas", new List<string> { "Piernas" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(15, 100.0, System.DateTime.Now.AddHours(-3)),
                        new Ejecucion(12, 110.0, System.DateTime.Now.AddHours(-2)),
                        new Ejecucion(10, 120.0, System.DateTime.Now.AddHours(-1))
                    }
                },
                new Ejercicio("Remo con barra", "Ejercicio para espalda y brazos", new List<string> { "Espalda", "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(12, 50.0, System.DateTime.Now.AddMinutes(-45)),
                        new Ejecucion(10, 55.0, System.DateTime.Now.AddMinutes(-30)),
                        new Ejecucion(8, 60.0, System.DateTime.Now)
                    }
                },
                new Ejercicio("Press militar", "Ejercicio para hombros", new List<string> { "Hombros" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(12, 40.0, System.DateTime.Now.AddMinutes(-120)),
                        new Ejecucion(10, 45.0, System.DateTime.Now.AddMinutes(-90)),
                        new Ejecucion(8, 50.0, System.DateTime.Now.AddMinutes(-60))
                    }
                },
                new Ejercicio("Flexiones", "Ejercicio para pecho y brazos", new List<string> { "Pecho", "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(20, 0.0, System.DateTime.Now.AddHours(-4)), // Peso corporal
                        new Ejecucion(18, 0.0, System.DateTime.Now.AddHours(-3)),
                        new Ejecucion(15, 0.0, System.DateTime.Now.AddHours(-2))
                    }
                },
                new Ejercicio("Peso muerto", "Ejercicio para espalda y piernas", new List<string> { "Espalda", "Piernas" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(8, 100.0, System.DateTime.Now.AddHours(-3)),
                        new Ejecucion(6, 110.0, System.DateTime.Now.AddHours(-2)),
                        new Ejecucion(4, 120.0, System.DateTime.Now.AddHours(-1))
                    }
                },
                new Ejercicio("Curl de bíceps", "Ejercicio para brazos", new List<string> { "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(15, 15.0, System.DateTime.Now.AddMinutes(-120)),
                        new Ejecucion(12, 17.5, System.DateTime.Now.AddMinutes(-90)),
                        new Ejecucion(10, 20.0, System.DateTime.Now.AddMinutes(-60))
                    }
                },
                new Ejercicio("Plancha abdominal", "Ejercicio para abdomen", new List<string> { "Abdomen" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(1, 0.0, System.DateTime.Now.AddHours(-2)), // 1 minuto
                        new Ejecucion(1, 0.0, System.DateTime.Now.AddHours(-1)), // 1 minuto
                        new Ejecucion(1, 0.0, System.DateTime.Now) // 1 minuto
                    }
                },
                new Ejercicio("Zancadas", "Ejercicio para piernas y glúteos", new List<string> { "Piernas"})
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(12, 30.0, System.DateTime.Now.AddMinutes(-45)),
                        new Ejecucion(10, 35.0, System.DateTime.Now.AddMinutes(-30)),
                        new Ejecucion(8, 40.0, System.DateTime.Now)
                    }
                }
            };

            DataGridEjercicio.ItemsSource = ejercicios;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            addEjercicioWindow = new AddEjercicioWindow();
            addEjercicioWindow.ShowDialog();
            addEjercicioWindow.Owner = this;
            if (addEjercicioWindow.DialogResult == true)
            {
                Ejercicio nuevoEjercicio = new Ejercicio(addEjercicioWindow.NombreTextBox.Text, addEjercicioWindow.DescripcionTextBox.Text, addEjercicioWindow.musculosSeleccionados);
                ejercicios.Add(nuevoEjercicio);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Ejercicio ejercicioSelecionado = (Ejercicio)DataGridEjercicio.SelectedItem;
            addEjercicioWindow = new AddEjercicioWindow(ejercicioSelecionado);
            addEjercicioWindow.ShowDialog();
            addEjercicioWindow.Owner = this;
            if (addEjercicioWindow.DialogResult == true)
            {
                ejercicios.Remove(ejercicioSelecionado);
                Ejercicio nuevoEjercicio = new Ejercicio(addEjercicioWindow.NombreTextBox.Text, addEjercicioWindow.DescripcionTextBox.Text, addEjercicioWindow.musculosSeleccionados);
                ejercicios.Add(nuevoEjercicio);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Ejercicio ejercicioSelecionado = (Ejercicio)DataGridEjercicio.SelectedItem;
            string msg = "Seguro que deseas eliminar el Ejercicio: " + ejercicioSelecionado.NombreEjercicio;
            string titulo = "¿Quieres eliminar?";
            MessageBoxButton btn = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;
            MessageBoxResult result = MessageBox.Show(msg, titulo, btn, icon);
            if (result == MessageBoxResult.Yes)
            {
                ejercicios.Remove(ejercicioSelecionado);
            }
            ejercicioSelecionado = null;
            NuevaSeleccionEjercicio?.Invoke(this, new EjercicioSelecionadaEventArgs(ejercicioSelecionado)); //Manda el evento al MainWindows con la selecionada
                                                                                                       //en este caso un null porque se he eliminado
            if(ejecucionWindow != null && ejecucionWindow.IsVisible)
            {
                ejecucionWindow.Close();
            }
        }

        private void DataGridEjercicio_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Ejercicio ejercicioSeleccionado = (Ejercicio)DataGridEjercicio.SelectedItem;
            if (ejecucionWindow == null || !ejecucionWindow.IsVisible)
            {
                // Crea una nueva instancia si no existe o está cerrada
                ejecucionWindow = new EjecucionWindow(ejercicioSeleccionado);
                ejecucionWindow.Show(); // Abre la ventana de forma no modal
                ejecucionWindow.ActualizarEjecuciones(ejercicioSeleccionado);
                ejecucionWindow.ActualizarGraficoBarras(ejercicioSeleccionado);
            }
            else
            {
                // Actualiza el contenido si la ventana ya está abierta
                ejecucionWindow.ActualizarEjecuciones(ejercicioSeleccionado);
                ejecucionWindow.ActualizarGraficoBarras(ejercicioSeleccionado);
            }
        }

        private void DataGridEjercicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Ejercicio ejercicioSeleccionado = (Ejercicio)DataGridEjercicio.SelectedItem;

            if (ejercicioSeleccionado == null) //No hay fecha selecionada 
            {
                EditButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
            else
            {
                EditButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
            }
            NuevaSeleccionEjercicio?.Invoke(this, new EjercicioSelecionadaEventArgs(ejercicioSeleccionado)); //Manda el evento al MainWindows con la selecionada o un null si no hay ninguna
        }

        private void DatePickerFecha_SelectedDate(object sender, EventArgs e)
        {
            if (DatePickerFecha.SelectedDate.HasValue)
            {
                DateTime fechaSeleccionada = DatePickerFecha.SelectedDate.Value;
                DibujarGrafico(fechaSeleccionada);
            }
        }

        private void DibujarGrafico(DateTime fecha)
        {
            // Limpiar el canvas antes de dibujar un nuevo gráfico
            CanvasGrafico.Children.Clear();

            // Obtener las repeticiones agrupadas por grupo muscular en la fecha seleccionada
            var gruposMusculares = CalcularRepeticionesPorGrupo(fecha);

            if (!gruposMusculares.Any())
            {
                // Mostrar mensaje si no hay datos para la fecha seleccionada
                MessageBox.Show("No hay datos de repeticiones para la fecha seleccionada.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Configurar propiedades básicas del gráfico
            const double radio = 150; // Radio máximo del gráfico
            double centroX = CanvasGrafico.Width / 2; // Coordenada X del centro
            double centroY = CanvasGrafico.Height / 2; // Coordenada Y del centro

            // Calcular ángulos para cada grupo muscular
            var angulos = CalcularAngulosRadiales(gruposMusculares);

            // Generar puntos para el polígono
            var puntosPoligono = CalcularPuntosPoligono(gruposMusculares, angulos, radio, centroX, centroY);

            // Dibujar el polígono
            DibujarPoligono(puntosPoligono);

            // Agregar etiquetas para los grupos musculares
            AgregarEtiquetas(gruposMusculares, angulos, radio, centroX, centroY);
        }

        private Dictionary<string, int> CalcularRepeticionesPorGrupo(DateTime fecha)
        {
            return ejercicios
                .SelectMany(e => e.ejecuciones
                    .Where(ex => ex.FechaHora.Date == fecha.Date)
                    .Select(ex => new { Musculos = string.Join(", ", e.MusculosEjercicio), ex.Repeticiones }))
                .GroupBy(g => g.Musculos)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Repeticiones));
        }


        // Método auxiliar para calcular los ángulos radiales
        private Dictionary<string, double> CalcularAngulosRadiales(Dictionary<string, int> gruposMusculares)
        {
            var angulos = new Dictionary<string, double>();
            double totalGrupos = gruposMusculares.Count;
            int i = 0;

            foreach (var grupo in gruposMusculares.Keys)
            {
                angulos[grupo] = (2 * Math.PI / totalGrupos) * i;
                i++;
            }

            return angulos;
        }

        // Método auxiliar para calcular los puntos del polígono
        private PointCollection CalcularPuntosPoligono(Dictionary<string, int> gruposMusculares, Dictionary<string, double> angulos, double radio, double centroX, double centroY)
        {
            var puntos = new PointCollection();

            foreach (var grupo in gruposMusculares)
            {
                double escala = Math.Min(grupo.Value / 100.0, 1.0); // Escala de 0 a 1 basada en las repeticiones
                double x = centroX + radio * escala * Math.Cos(angulos[grupo.Key]);
                double y = centroY - radio * escala * Math.Sin(angulos[grupo.Key]);
                puntos.Add(new Point(x, y));
            }

            return puntos;
        }

        // Método auxiliar para dibujar el polígono en el Canvas
        private void DibujarPoligono(PointCollection puntos)
        {
            var grafico = new Polygon
            {
                Points = puntos,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Fill = Brushes.LightBlue,
                Opacity = 0.7
            };

            CanvasGrafico.Children.Add(grafico);
        }

        // Método auxiliar para agregar etiquetas a los grupos musculares
        private void AgregarEtiquetas(Dictionary<string, int> gruposMusculares, Dictionary<string, double> angulos, double radio, double centroX, double centroY)
        {
            foreach (var grupo in gruposMusculares)
            {
                double x = centroX + (radio + 20) * Math.Cos(angulos[grupo.Key]); // Posicionar un poco más afuera del polígono
                double y = centroY - (radio + 20) * Math.Sin(angulos[grupo.Key]);

                var etiqueta = new TextBlock
                {
                    Text = $"{grupo.Key} ({grupo.Value} rep)",
                    Foreground = Brushes.Black,
                    FontSize = 12
                };

                Canvas.SetLeft(etiqueta, x);
                Canvas.SetTop(etiqueta, y);
                CanvasGrafico.Children.Add(etiqueta);
            }
        }
    }
}

