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
        private event EjercicioSelecionadaEventHandler NuevaSeleccionEjercicio;
        private ObservableCollection<Musculos> gruposMusculares;
        public DateTime fechaGrafico;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            ejercicios = new ObservableCollection<Ejercicio>();
            
            IniciarMusculos();
            DataGridEjercicio.ItemsSource = ejercicios;
            fechaGrafico = DateTime.Now;
            DatePickerGrafico.SelectedDate = fechaGrafico;
            CanvasGrafico.SizeChanged += (s, e) => DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
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
            addEjercicioWindow = new AddEjercicioWindow("Añadir");
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
            ObservableCollection<Ejecucion> ejecucionesNueva = ejercicioSelecionado.ejecuciones;
            addEjercicioWindow = new AddEjercicioWindow(ejercicioSelecionado, "Modificar");
            addEjercicioWindow.ShowDialog();
            addEjercicioWindow.Owner = this;
            if (addEjercicioWindow.DialogResult == true)
            {

                ejercicios.Remove(ejercicioSelecionado);
                Ejercicio nuevoEjercicio = new Ejercicio(addEjercicioWindow.NombreTextBox.Text, addEjercicioWindow.DescripcionTextBox.Text, addEjercicioWindow.musculosSeleccionados);
                nuevoEjercicio.ejecuciones = ejecucionesNueva;
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

        public void DibujarGraficoRadial(DateTime fecha)
        {
            CanvasGrafico.Children.Clear();

            if (gruposMusculares == null || !gruposMusculares.Any())
            {
                MessageBox.Show("No hay grupos musculares registrados.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Filtrar ejecuciones por la fecha seleccionada
            var ejecucionesEnFecha = ejercicios
                .Where(e => e.ejecuciones != null)
                .SelectMany(e => e.ejecuciones
                    .Where(ex => ex.FechaHora.Date == fecha.Date)
                    .Select(ex => new
                    {
                        MusculosEjercicio = e.MusculosEjercicio,
                        ex.Repeticiones
                    }))
                .ToList();

            // Inicializar repeticionesPorGrupo con todos los grupos musculares y valores en 0
            var repeticionesPorGrupo = gruposMusculares
                .ToDictionary(g => g.NombreMusculo, g => 0);

            // Actualizar los valores en repeticionesPorGrupo con las repeticiones reales si existen
            foreach (var ejecucion in ejecucionesEnFecha)
            {
                foreach (var musculo in ejecucion.MusculosEjercicio)
                {
                    if (repeticionesPorGrupo.ContainsKey(musculo.NombreMusculo))
                    {
                        repeticionesPorGrupo[musculo.NombreMusculo] += ejecucion.Repeticiones;
                    }
                }
            }

            // Configurar dimensiones del gráfico
            double anchoCanvas = CanvasGrafico.ActualWidth > 0 ? CanvasGrafico.ActualWidth : 700;
            double altoCanvas = CanvasGrafico.ActualHeight > 0 ? CanvasGrafico.ActualHeight : 300;
            double centroX = anchoCanvas / 2;
            double centroY = altoCanvas / 2;
            double radio = Math.Min(anchoCanvas, altoCanvas) / 2 - 40;
            double maxRepeticionesVisual = 100; // Máximo visualizable

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
                var grupo = gruposMusculares[i];
                double repeticiones = repeticionesPorGrupo[grupo.NombreMusculo];

                // Limitar las repeticiones visualizables
                double repeticionesLimitadas = Math.Min(repeticiones, maxRepeticionesVisual);
                double longitud = (repeticionesLimitadas / maxRepeticionesVisual) * radio;
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

            // Agregar el polígono al Canvas primero
            CanvasGrafico.Children.Add(poligono);

            // Dibujar puntos en los vértices después del polígono
            for (int i = 0; i < numGrupos; i++)
            {
                double angulo = i * anguloIncremento;
                var grupo = gruposMusculares[i];
                double repeticiones = repeticionesPorGrupo[grupo.NombreMusculo];

                // Limitar las repeticiones visualizables
                double repeticionesLimitadas = Math.Min(repeticiones, maxRepeticionesVisual);
                double longitud = (repeticionesLimitadas / maxRepeticionesVisual) * radio;
                double x = centroX + longitud * Math.Cos(angulo);
                double y = centroY - longitud * Math.Sin(angulo);

                // Crear el punto
                var punto = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Red,
                    Tag = repeticiones // Usar la propiedad Tag para almacenar las repeticiones reales
                };

                Canvas.SetLeft(punto, x - 5);
                Canvas.SetTop(punto, y - 5);
                CanvasGrafico.Children.Add(punto);

                // Asignar eventos de mouse para mostrar ToolTip
                punto.MouseEnter += (s, e) =>
                {
                    var el = s as Ellipse;
                    var tooltip = new ToolTip { Content = $"{grupo.NombreMusculo}: {el.Tag} repeticiones" };
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
            }

            // Agregar etiquetas de los grupos musculares
            for (int i = 0; i < numGrupos; i++)
            {
                double angulo = i * anguloIncremento;
                double x = centroX + (radio + 20) * Math.Cos(angulo);
                double y = centroY - (radio + 20) * Math.Sin(angulo);

                var etiqueta = new TextBlock
                {
                    Text = gruposMusculares[i].NombreMusculo,
                    FontSize = 10,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(etiqueta, x - 10);
                Canvas.SetTop(etiqueta, y - 10);
                CanvasGrafico.Children.Add(etiqueta);
            }
        }

        private void SiguienteButton_Click(object sender, RoutedEventArgs e)
        {
            fechaGrafico = DatePickerGrafico.SelectedDate.Value.AddDays(+1);
            DatePickerGrafico.SelectedDate = DatePickerGrafico.SelectedDate.Value.AddDays(+1);
            DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
        }

        private void AnteriorButton_Click(object sender, RoutedEventArgs e)
        {
            fechaGrafico = DatePickerGrafico.SelectedDate.Value.AddDays(-1);
            DatePickerGrafico.SelectedDate = DatePickerGrafico.SelectedDate.Value.AddDays(-1);
            DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
        }

        private void DatePickerGrafico_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerGrafico.SelectedDate.HasValue)
            {
                fechaGrafico = DatePickerGrafico.SelectedDate.Value;
                DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
            }
        }

        private void IniciarMusculos()
        {
            gruposMusculares = new ObservableCollection<Musculos>
            {
                new Musculos("Espalda"),
                new Musculos("Brazos"),
                new Musculos("Piernas"),
                new Musculos("Abdomen"),
                new Musculos("Pecho")
            };
        }

        private void ImportarEjerciciosIniciales()
        {
            // Crear el diálogo para abrir
            OpenFileDialog abrirArchivo = new OpenFileDialog
            {
                Title = "Abrir ejercicios",
                Filter = "Archivo de Ejercicios (*.fitnessTracker)|*.fitnessTracker",
                DefaultExt = ".fitnessTracker"
            };

            // Mostrar el diálogo
            if (abrirArchivo.ShowDialog() == true)
            {
                try
                {
                    // Leer el archivo
                    string datosJson = File.ReadAllText(abrirArchivo.FileName);

                    // Convertir el JSON a ejercicios
                    var ejerciciosNuevos = JsonConvert.DeserializeObject<List<Ejercicio>>(datosJson);

                    // Validar los datos deserializados
                    if (ejerciciosNuevos == null || ejerciciosNuevos.Count == 0)
                    {
                        MessageBox.Show("El archivo está vacío o no tiene el formato correcto.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return;
                    }

                    // Borrar los ejercicios actuales y añadir los nuevos
                    if (ejercicios != null)
                    {
                        ejercicios.Clear();
                    }

                    foreach (var ejercicio in ejerciciosNuevos)
                    {
                        // Asegurar que las colecciones no sean nulas
                        if (ejercicio.MusculosEjercicio == null)
                            ejercicio.MusculosEjercicio = new List<Musculos>();

                        if (ejercicio.ejecuciones == null)
                            ejercicio.ejecuciones = new ObservableCollection<Ejecucion>();

                        ejercicios.Add(ejercicio);
                    }

                    MessageBox.Show("Los datos se han cargado correctamente.",
                                    "Éxito",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el archivo: {ex.Message}",
                                    "Error al abrir",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
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
                    ExportarDatos();
                }
                // Importar datos
                else if (sender == importarDatos)
                {
                    ImportarDatos();
                }
                // Vaciar datos
                else if (sender == vaciarDatos)
                {
                    VaciarDatos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportarDatos()
        {
            // Crear el diálogo para guardar
            SaveFileDialog guardarArchivo = new SaveFileDialog
            {
                Title = "Guardar ejercicios",
                Filter = "Archivo de Ejercicios (*.fitnessTracker)|*.fitnessTracker",
                DefaultExt = ".fitnessTracker"
            };

            // Mostrar el diálogo
            if (guardarArchivo.ShowDialog() == true)
            {
                try
                {
                    // Convertir los ejercicios a JSON
                    string datosJson = JsonConvert.SerializeObject(ejercicios, Formatting.Indented);

                    // Guardar en el archivo
                    File.WriteAllText(guardarArchivo.FileName, datosJson);

                    MessageBox.Show("Los datos se han guardado correctamente.",
                                    "Éxito",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo guardar el archivo: {ex.Message}",
                                    "Error al guardar",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void ImportarDatos()
        {
            // Crear el diálogo para abrir
            OpenFileDialog abrirArchivo = new OpenFileDialog
            {
                Title = "Abrir ejercicios",
                Filter = "Archivo de Ejercicios (*.fitnessTracker)|*.fitnessTracker",
                DefaultExt = ".fitnessTracker"
            };

            // Mostrar el diálogo
            if (abrirArchivo.ShowDialog() == true)
            {
                try
                {
                    // Leer el archivo
                    string datosJson = File.ReadAllText(abrirArchivo.FileName);

                    // Convertir el JSON a ejercicios
                    var ejerciciosNuevos = JsonConvert.DeserializeObject<List<Ejercicio>>(datosJson);

                    // Validar los datos deserializados
                    if (ejerciciosNuevos == null || ejerciciosNuevos.Count == 0)
                    {
                        MessageBox.Show("El archivo está vacío o no tiene el formato correcto.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return;
                    }

                    // Borrar los ejercicios actuales y añadir los nuevos
                    if(ejercicios != null)
                    {
                        ejercicios.Clear();
                    }
                    
                    foreach (var ejercicio in ejerciciosNuevos)
                    {
                        // Asegurar que las colecciones no sean nulas
                        if (ejercicio.MusculosEjercicio == null)
                            ejercicio.MusculosEjercicio = new List<Musculos>();

                        if (ejercicio.ejecuciones == null)
                            ejercicio.ejecuciones = new ObservableCollection<Ejecucion>();

                        ejercicios.Add(ejercicio);
                    }

                    MessageBox.Show("Los datos se han cargado correctamente.",
                                    "Éxito",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el archivo: {ex.Message}",
                                    "Error al abrir",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void VaciarDatos()
        {
            // Preguntar antes de borrar
            MessageBoxResult respuesta = MessageBox.Show(
                "¿Estás seguro de que quieres borrar todos los datos?",
                "Confirmar borrado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            // Si el usuario dice que sí, borrar los datos
            if (respuesta == MessageBoxResult.Yes)
            {
                ejercicios.Clear();
                MessageBox.Show("Se han borrado todos los datos",
                              "Datos borrados",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                DibujarGraficoRadial(DatePickerGrafico.SelectedDate.Value);
                ejecucionWindow.Close();
            }
        }
    }
}

