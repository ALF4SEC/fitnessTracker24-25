using MahApps.Metro.Controls;
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

namespace fitnessTracker24_25.Vista
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class AddEjecucionWindow : Window
    {
        public AddEjecucionWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.CanMinimize;
            WindowStyle = WindowStyle.ToolWindow;
        }

        private void AceptarButton_Click(object sender, RoutedEventArgs e)
        {
            if (FechaPicker.SelectedDate.HasValue == false || HoraPicker.SelectedDateTime.HasValue == false || RepeticionesTextBox.IntValue<=0 || PesoTextBox.DoubleValue<=0)
            {
                string msg = "Te falta por completar algún dato, asegúrate que has introduccido las fecha, la hora, el peso y las repeticiones";
                string titulo = "Completa los datos";
                MessageBoxButton btn = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(msg, titulo, btn, icon);
            }
            else if (RepeticionesTextBox.IntValue > 100)
            {
                string msg = "Las repeticiones no pueden ser mayor de 100";
                string titulo = "Corrije los datos";
                MessageBoxButton btn = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(msg, titulo, btn, icon);
            }
            else
            {
                DialogResult = true;
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
