using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using fitnessTracker24_25.Modelo;
using fitnessTracker24_25.Vista;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;

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
        public ObservableCollection<Ejercicio> ejercicios { get; set; }
        public event EjercicioSelecionadaEventHandler NuevaSeleccionEjercicio;
        private List<string> gruposMusculares = new List<string> { "Brazos", "Espalda", "Piernas", "Core", "Pecho" };


        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed; // Suscribirse al evento de cierre
            IniciarEjericicios();
            DataGridEjercicio.ItemsSource = ejercicios;
            DatePickerGrafico.SelectedDate = DateTime.Now;
            CanvasGrafico.SizeChanged += (s, e) => DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value, ejercicios, gruposMusculares);
        }
        
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (ejecucionWindow != null && ejecucionWindow.IsVisible)
            {
                ejecucionWindow.Close();
            }
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
            if (DataGridEjercicio.SelectedItem is Ejercicio ejercicioSeleccionado)
            {
                if (ejecucionWindow == null || !ejecucionWindow.IsVisible)
                {
                    ejecucionWindow = new EjecucionWindow(ejercicioSeleccionado, this);
                    ejecucionWindow.Show();
                }

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

        private void DibujarGraficoRadial(DateTime fecha, ObservableCollection<Ejercicio> ejercicios, List<string> gruposMusculares)
        {
            CanvasGrafico.Children.Clear();

            if (ejercicios == null || !ejercicios.Any())
            {
                MessageBox.Show("No hay ejercicios registrados.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Filtrar ejecuciones por la fecha seleccionada
            var ejecucionesEnFecha = ejercicios
                .Where(e => e.ejecuciones != null)
                .SelectMany(e => e.ejecuciones.Where(ex => ex.FechaHora.Date == fecha.Date)
                .Select(ex => new { e.MusculosEjercicio, ex.Repeticiones }))
                .ToList();

            // Agrupar repeticiones por grupo muscular
            var repeticionesPorGrupo = ejecucionesEnFecha
                .SelectMany(e => e.MusculosEjercicio.Select(g => new { Grupo = g, e.Repeticiones }))
                .GroupBy(e => e.Grupo)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Repeticiones));

            // Configurar dimensiones del gráfico
            double anchoCanvas = CanvasGrafico.ActualWidth > 0 ? CanvasGrafico.ActualWidth : 700;
            double altoCanvas = CanvasGrafico.ActualHeight > 0 ? CanvasGrafico.ActualHeight : 300;
            double centroX = anchoCanvas / 2;
            double centroY = altoCanvas / 2;
            double radio = Math.Min(anchoCanvas, altoCanvas) / 2 - 40;
            double maxRepeticiones = 100; // Máximo de 100 repeticiones

            // Dibujar ejes radiales
            int numGrupos = gruposMusculares.Count;
            double anguloIncremento = 2 * Math.PI / numGrupos;

            for (int i = 0; i < numGrupos; i++)
            {
                double angulo = i * anguloIncremento;
                double x = centroX + radio * Math.Cos(angulo);
                double y = centroY - radio * Math.Sin(angulo);

                // Línea radial
                var linea = new Line
                {
                    X1 = centroX,
                    Y1 = centroY,
                    X2 = x,
                    Y2 = y,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                CanvasGrafico.Children.Add(linea);
            }

            // Dibujar el polígono de repeticiones
            var puntosPoligono = new PointCollection();

            for (int i = 0; i < numGrupos; i++)
            {
                double angulo = i * anguloIncremento;
                string grupo = gruposMusculares[i];
                double repeticiones = repeticionesPorGrupo.ContainsKey(grupo) ? repeticionesPorGrupo[grupo] : 0;
                double repeticionesLimitadas = Math.Min(repeticiones, maxRepeticiones); // Limitar las repeticiones al máximo de 100
                double longitud = (repeticionesLimitadas / maxRepeticiones) * radio;
                double x = centroX + longitud * Math.Cos(angulo);
                double y = centroY - longitud * Math.Sin(angulo);

                puntosPoligono.Add(new Point(x, y));
            }

            var poligono = new Polygon
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Fill = Brushes.LightBlue,
                Points = puntosPoligono
            };

            CanvasGrafico.Children.Add(poligono);

            // Dibujar puntos en los vértices después de agregar el polígono
            for (int i = 0; i < puntosPoligono.Count; i++)
            {
                var punto = puntosPoligono[i];
                var grupo = gruposMusculares[i];
                var repeticiones = repeticionesPorGrupo.ContainsKey(grupo) ? repeticionesPorGrupo[grupo] : 0;

                var ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Red,
                    Tag = repeticiones // Usar la propiedad Tag para almacenar el número real de repeticiones
                };

                // Asignar eventos de mouse
                ellipse.MouseEnter += (s, e) =>
                {
                    var el = s as Ellipse;
                    var tooltip = new ToolTip { Content = $"{el.Tag} repeticiones" };
                    el.ToolTip = tooltip;
                    tooltip.IsOpen = true;
                };

                ellipse.MouseLeave += (s, e) =>
                {
                    var el = s as Ellipse;
                    var tooltip = el.ToolTip as ToolTip;
                    if (tooltip != null)
                    {
                        tooltip.IsOpen = false;
                    }
                };

                Canvas.SetLeft(ellipse, punto.X - 5);
                Canvas.SetTop(ellipse, punto.Y - 5);
                CanvasGrafico.Children.Add(ellipse);
            }

            // Agregar etiquetas de los grupos musculares al final de los ejes
            for (int i = 0; i < numGrupos; i++)
            {
                double angulo = i * anguloIncremento;
                double x = centroX + radio * Math.Cos(angulo);
                double y = centroY - radio * Math.Sin(angulo);

                var etiqueta = new TextBlock
                {
                    Text = gruposMusculares[i],
                    FontSize = 10,
                    Foreground = Brushes.Black
                };

                // Ajustar la posición de la etiqueta para que siempre sea visible
                double offsetX = 30 * Math.Cos(angulo);
                double offsetY = 30 * Math.Sin(angulo);
                Canvas.SetLeft(etiqueta, centroX + (radio + offsetX) * Math.Cos(angulo) - 10);
                Canvas.SetTop(etiqueta, centroY - (radio + offsetY) * Math.Sin(angulo) - 10);
                CanvasGrafico.Children.Add(etiqueta);
            }
        }

        private void SiguienteButton_Click(object sender, RoutedEventArgs e)
        {
            DatePickerGrafico.SelectedDate = DatePickerGrafico.SelectedDate.Value.AddDays(+1);
            DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value, ejercicios, gruposMusculares);
        }

        private void AnteriorButton_Click(object sender, RoutedEventArgs e)
        {
            DatePickerGrafico.SelectedDate = DatePickerGrafico.SelectedDate.Value.AddDays(-1);
            DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value, ejercicios, gruposMusculares);
        }

        private void DatePickerGrafico_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerGrafico.SelectedDate.HasValue)
            {
                DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value, ejercicios, gruposMusculares);
            }
        }
        
        private void IniciarEjericicios()
        {
            ejercicios = new ObservableCollection<Ejercicio>
            {
                new Ejercicio("Dominadas", "Ejercicio para espalda", new List<string> { "Espalda", "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(10, 50.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(8, 55.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(6, 60.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(10, 52.0, DateTime.Now.AddDays(-7)),
                        new Ejecucion(8, 57.0, DateTime.Now.AddDays(-6)),
                        new Ejecucion(6, 62.0, DateTime.Now) // Hoy
                    }
                },
                new Ejercicio("Press de banca", "Ejercicio para pecho", new List<string> { "Pecho" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(12, 75.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(10, 80.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(8, 85.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(12, 77.0, DateTime.Now),
                        new Ejecucion(10, 82.0, DateTime.Now.AddDays(-6)),
                        new Ejecucion(8, 87.0, DateTime.Now)
                    }
                },
                new Ejercicio("Sentadillas con salto", "Ejercicio avanzado para piernas", new List<string> { "Piernas" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(15, 80.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(12, 90.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(10, 100.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(20, 95.0, DateTime.Now),
                        new Ejecucion(18, 97.0, DateTime.Now),
                        new Ejecucion(15, 105.0, DateTime.Now.AddDays(-5))
                    }
                },
                new Ejercicio("Remo en máquina", "Ejercicio para espalda y brazos", new List<string> { "Espalda", "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(12, 55.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(10, 60.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(8, 65.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(12, 58.0, DateTime.Now.AddDays(-7)),
                        new Ejecucion(10, 63.0, DateTime.Now),
                        new Ejecucion(8, 67.0, DateTime.Now)
                    }
                },
                new Ejercicio("Press militar", "Ejercicio para hombros", new List<string> { "Brazos" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(15, 45.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(12, 50.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(10, 55.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(15, 48.0, DateTime.Now.AddDays(-7)),
                        new Ejecucion(12, 53.0, DateTime.Now),
                        new Ejecucion(10, 57.0, DateTime.Now)
                    }
                },
                new Ejercicio("Burpees", "Ejercicio de cuerpo completo", new List<string> { "Espalda", "Brazos", "Piernas" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(20, 0.0, DateTime.Now.AddDays(-10)), // Peso corporal
                        new Ejecucion(18, 0.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(15, 0.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(25, 0.0, DateTime.Now),
                        new Ejecucion(22, 0.0, DateTime.Now),
                        new Ejecucion(18, 0.0, DateTime.Now.AddDays(-5))
                    }
                },
                new Ejercicio("Plancha lateral", "Ejercicio para abdomen y oblicuos", new List<string> { "Abdomen" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(1, 0.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(1, 0.0, DateTime.Now.AddDays(-9)), 
                        new Ejecucion(1, 0.0, DateTime.Now.AddDays(-8)), 
                        new Ejecucion(1, 0.0, DateTime.Now),           
                        new Ejecucion(1, 0.0, DateTime.Now),
                        new Ejecucion(1, 0.0, DateTime.Now.AddDays(-5))
                    }
                },
                new Ejercicio("Zancadas", "Ejercicio para piernas y glúteos", new List<string> { "Piernas" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(15, 30.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(12, 35.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(10, 40.0, DateTime.Now),
                        new Ejecucion(15, 32.0, DateTime.Now),
                        new Ejecucion(12, 37.0, DateTime.Now.AddDays(-6)),
                        new Ejecucion(10, 42.0, DateTime.Now)
                    }
                },
                new Ejercicio("Swing con kettlebell", "Ejercicio para cadera y glúteos", new List<string> { "Piernas", "Espalda" })
                {
                    ejecuciones = new ObservableCollection<Ejecucion>
                    {
                        new Ejecucion(15, 20.0, DateTime.Now.AddDays(-10)),
                        new Ejecucion(12, 25.0, DateTime.Now.AddDays(-9)),
                        new Ejecucion(10, 30.0, DateTime.Now.AddDays(-8)),
                        new Ejecucion(18, 28.0, DateTime.Now),
                        new Ejecucion(16, 29.0, DateTime.Now),
                        new Ejecucion(14, 31.0, DateTime.Now)
                    }
                }
            };
        }

        //-------------------------------MENÚ-----------------------------------
        //Método con todas las funcionalidades del menú
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Exportar datos
                if (sender == exportarDatos)
                {
                    SaveFileDialog exportDialog = new SaveFileDialog()
                    {
                        Title = "Exportar datos de los ejercicios",
                        DefaultExt = ".fitnessTracker",
                        Filter = "Archivo de Ejercicios (*.fitnessTracker)|*.fitnessTracker",
                        AddExtension = true
                    };

                    if ((bool)exportDialog.ShowDialog())
                    {
                        try
                        {
                            // Serializar la colección de ejercicios
                            string jsonString = JsonConvert.SerializeObject(ejercicios, Formatting.Indented);
                            File.WriteAllText(exportDialog.FileName, jsonString);
                            MessageBox.Show("Datos exportados correctamente.", "Exportación", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al exportar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                // Importar datos
                else if (sender == importarDatos)
                {
                    OpenFileDialog importDialog = new OpenFileDialog()
                    {
                        Title = "Importar datos de ejercicios",
                        DefaultExt = ".fitnessTracker",
                        Filter = "Archivo de Ejercicios (*.fitnessTracker)|*.fitnessTracker",
                        AddExtension = true
                    };

                    if ((bool)importDialog.ShowDialog())
                    {
                        try
                        {
                            // Leer el archivo JSON
                            string jsonString = File.ReadAllText(importDialog.FileName);

                            // Deserializar los datos en una lista de ejercicios
                            List<Ejercicio> ejerciciosImportados = JsonConvert.DeserializeObject<List<Ejercicio>>(jsonString);

                            // Validar si se importaron datos válidos
                            if (ejerciciosImportados == null || !ejerciciosImportados.Any())
                            {
                                MessageBox.Show("El archivo no contiene datos válidos o está vacío.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            // Limpiar y agregar los ejercicios importados
                            ejercicios.Clear();
                            foreach (var ejercicio in ejerciciosImportados)
                            {
                                ejercicios.Add(ejercicio);
                            }

                            MessageBox.Show("Datos importados correctamente.", "Importación", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (JsonSerializationException jsonEx)
                        {
                            MessageBox.Show($"Error al procesar el archivo JSON: {jsonEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (IOException ioEx)
                        {
                            MessageBox.Show($"Error al leer el archivo: {ioEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                // Vaciar todos los datos
                else if (sender == vaciarDatos)
                {
                    string msg = "¿Estás seguro de eliminar todos los registros?";
                    string titulo = "Confirmación";
                    MessageBoxButton btn = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBoxResult result = MessageBox.Show(msg, titulo, btn, icon);

                    if (result == MessageBoxResult.Yes)
                    {   
                        ejercicios.Clear();
                        MessageBox.Show("Todos los datos han sido eliminados.", "Eliminación", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

