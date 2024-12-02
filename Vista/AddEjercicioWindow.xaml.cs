using fitnessTracker24_25.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace fitnessTracker24_25.Vista
{
    public partial class AddEjercicioWindow : Window
    {
        public ObservableCollection<Musculos> Musculos { get; set; }
        public List<string> musculosSeleccionados;
        public MainWindow mainWindow = new MainWindow();

        public AddEjercicioWindow(Ejercicio ejercicioSeleccionado)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.CanMinimize;
            WindowStyle = WindowStyle.ToolWindow;
            Title = "ModificarEjercicio";
            NombreTextBox.Text = ejercicioSeleccionado.NombreEjercicio;
            DescripcionTextBox.Text = ejercicioSeleccionado.DescripcionEjercicio;
            Musculos = new ObservableCollection<Musculos>
            {
                new Musculos ( "Espalda",false ),
                new Musculos ("Pecho", false),
                new Musculos ("Piernas", false),
                new Musculos ("Brazos", false),
                new Musculos ("Abdomen", false)
            };
            foreach (string i in ejercicioSeleccionado.MusculosEjercicio)
            {
                if(Musculos.Any(m => m.NombreMusculo == i))
                {
                    Musculos.First(m => m.NombreMusculo == i).Seleccion = true;
                }
            }
            DataGridMusculos.ItemsSource = Musculos;
        }

        public AddEjercicioWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.CanMinimize;
            WindowStyle = WindowStyle.ToolWindow;
            Musculos = new ObservableCollection<Musculos>
            {
                new Musculos ( "Espalda",false ),
                new Musculos ("Pecho", false),
                new Musculos ("Piernas", false),
                new Musculos ("Brazos", false),
                new Musculos ("Abdomen", false)
            };

            DataGridMusculos.ItemsSource = Musculos;
        }

        private void AceptarButton_Click(object sender, RoutedEventArgs e)
        {
            //Crear una lista para almacenar los músculos seleccionados
            musculosSeleccionados = Musculos
                .Where(m => m.Seleccion) // Filtrar solo los seleccionados
                .Select(m => m.NombreMusculo) // Obtener los nombres
                .ToList(); // Convertir a una List<string>
            if (string.IsNullOrEmpty(NombreTextBox.Text) || string.IsNullOrEmpty(DescripcionTextBox.Text) || musculosSeleccionados.Count == 0)
            {
                string msg = "Te falta por completar algún dato, asegúrate que has introduccido el nombre, la descripcion y has seleccionado un ejercicio";
                string titulo = "Corrije los datos";
                MessageBoxButton btn = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(msg, titulo, btn, icon);
            }

            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow == null)
            {
                MessageBox.Show("Error: No se pudo acceder a la ventana principal.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            

            // Validar nombres duplicados
            if (mainWindow.ejercicios.Any(ej => ej.NombreEjercicio.Equals(NombreTextBox.Text, StringComparison.OrdinalIgnoreCase)))
            {
                string msg = "Ya existe un ejercicio con ese nombre";
                string titulo = "Cambia el nombre";
                MessageBoxButton btn = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(msg, titulo, btn, icon);
            }

            DialogResult = true;
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        
    }
}
