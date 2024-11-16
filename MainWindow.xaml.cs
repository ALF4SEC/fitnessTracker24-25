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

namespace fitnessTracker24_25
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEjercicioWindows();
            addWindow.Owner = this; // Establece la ventana principal como dueño
            addWindow.ShowDialog(); // Abre la ventana modal

            // Verificar si la acción fue confirmada
            if (addWindow.IsConfirmed)
            {
                // Procesar los datos ingresados
                string nombre = addWindow.ExerciseName;
                string descripcion = addWindow.Description;
                string grupoMuscular = addWindow.MuscleGroup;


                // Mostrar los datos (puedes sustituir esto por lógica para guardar en la base de datos)
                MessageBox.Show("Los datos se guardaron de forma correcta.");
            }
            else
            {
                // Acciones opcionales si se cancela
                MessageBox.Show("Acción cancelada por el usuario.");
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
