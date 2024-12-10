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
        public ObservableCollection<Musculos> Musculo { get; set; }

        public AddEjecucionWindow AddEjecucionWindow
        {
            get => default;
            set
            {
            }
        }

        public List<Musculos> musculosSeleccionados;
        public MainWindow mainWindow = new MainWindow();
        private string comando;
        private Ejercicio ejercicioSeleccionado;

        /*Constructor de la clase, este constructor se usa cuando queremos modificar un ejercicio.
         Lo que hace es que aparece una ventana con los datos del ejercicio seleccionado para modificarlos
        En caso de que el usuario no quiera modificarlo, tiene que darle a cancelar.*/
        public AddEjercicioWindow(Ejercicio ejercicioSeleccionado, string comando)
        {
            InitializeComponent();
            //Hacemos que la ventana no sea redimensionable
            ResizeMode = ResizeMode.CanMinimize;
            //Definimos el estilo de la ventana
            WindowStyle = WindowStyle.ToolWindow;
            this.comando = comando;
            this.ejercicioSeleccionado = ejercicioSeleccionado;
            InicioModificar(ejercicioSeleccionado);
            
            DataGridMusculos.ItemsSource = Musculo;
        }

        /*Constructor de la clase, este constructor se usa cuando queremos añadir un ejercicio.*/
        public AddEjercicioWindow(string comando)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.CanMinimize;
            WindowStyle = WindowStyle.ToolWindow;
            InicioMusculosFalse();
            this.comando = comando;
            DataGridMusculos.ItemsSource = Musculo;
        }

        /*Manejador del evento Click del boton Aceptar*/
        private void AceptarButton_Click(object sender, RoutedEventArgs e)
        {
            //Crear una lista para almacenar los músculos seleccionados
            // Initialize the collection
            musculosSeleccionados = Musculo.Where(m => m.Seleccion).ToList();

            //Obtener la ventana principal
            /*La creacion de esta variable la hacemos para poder acceder a la observable collection de la ventana principal*/
            //Referencia en el manual de desarrollador
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            switch (comando)
            {
                case "Añadir":
                    if (string.IsNullOrEmpty(NombreTextBox.Text) || string.IsNullOrEmpty(DescripcionTextBox.Text) || musculosSeleccionados.Count == 0)
                    {
                        string msg = "Te falta por completar algún dato, asegúrate que has introduccido el nombre, la descripcion y has seleccionado un ejercicio";
                        string titulo = "Corrije los datos";
                        MessageBoxButton btn = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        MessageBox.Show(msg, titulo, btn, icon);
                    }
                    else if (mainWindow == null)
                    {
                        MessageBox.Show("Error: No se pudo acceder a la ventana principal.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (mainWindow.ejercicios.Any(ej => ej.NombreEjercicio.Equals(NombreTextBox.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Validar nombres duplicados
                        string msg = "Ya existe un ejercicio con ese nombre";
                        string titulo = "Cambia el nombre";
                        MessageBoxButton btn = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        MessageBox.Show(msg, titulo, btn, icon);

                    }
                    else
                    {
                        DialogResult = true;
                    }
                    break;
                case "Modificar":
                    if (mainWindow == null)
                    {
                        MessageBox.Show("Error: No se pudo acceder a la ventana principal.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (mainWindow.ejercicios.Any(ej => !ej.NombreEjercicio.Equals(ejercicioSeleccionado.NombreEjercicio, StringComparison.OrdinalIgnoreCase) && ej.NombreEjercicio.Equals(NombreTextBox.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        string msg = "Ya existe un ejercicio con ese nombre";
                        string titulo = "Error";
                        MessageBoxButton btn = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        MessageBox.Show(msg, titulo, btn, icon);
                    }
                    else
                    {
                        DialogResult = true;
                    }
                    break;
            }
        }

        //Manejador del evento Click del boton Cancelar
        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void InicioMusculosFalse()
        {
            Musculo = new ObservableCollection<Musculos>
            {
                new Musculos ( "Espalda",false ),
                new Musculos ("Pecho", false),
                new Musculos ("Piernas", false),
                new Musculos ("Brazos", false),
                new Musculos ("Abdomen", false)
            };
        }

        private void InicioModificar(Ejercicio ejercicioSeleccionado)
        {
            Title = "ModificarEjercicio";
            NombreTextBox.Text = ejercicioSeleccionado.NombreEjercicio;
            DescripcionTextBox.Text = ejercicioSeleccionado.DescripcionEjercicio;
            InicioMusculosFalse();
            foreach (Musculos i in ejercicioSeleccionado.MusculosEjercicio)
            {
                if (Musculo.Any(m => m.NombreMusculo == i.NombreMusculo))
                {
                    Musculo.First(m => m.NombreMusculo == i.NombreMusculo).Seleccion = true;
                }
            } 
        }
    }
}
