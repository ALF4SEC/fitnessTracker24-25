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
using System.Windows.Shapes;

namespace fitnessTracker24_25
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class AddEjercicioWindows : Window
    {
        public AddEjercicioWindows()
        {
            InitializeComponent();
        }

        public string ExerciseName { get; private set; }
        public string Description { get; private set; }
        public string MuscleGroup { get; private set; }
        public bool IsConfirmed { get; private set; }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Recoger valores de los cuadros de texto
            ExerciseName = NameTextBox.Text;
            Description = DescriptionTextBox.Text;
            MuscleGroup = GroupTextBox.Text;
            IsConfirmed = true; // Confirmar que se aceptó
            this.Close(); // Cerrar la ventana
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false; // Indicar que se canceló
            this.Close(); // Cerrar la ventana
        }
    }
}
