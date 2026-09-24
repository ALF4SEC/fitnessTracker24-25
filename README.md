# fitnessTracker24-25

Aplicación de escritorio para Windows, hecha en C# con WPF (.NET Framework 4.7.2), para registrar ejercicios de gimnasio y lo que se hace en cada uno. Los datos se guardan e intercambian en archivos JSON.

![Captura de la aplicación](fitnessTracker.png)

## Funcionalidades

**Pestaña Ejercicios**

- Lista de ejercicios con nombre, descripción y grupos musculares que trabaja: espalda, brazos, piernas, abdomen y pecho.
- Botones para añadir, modificar y eliminar ejercicios.
- Al hacer doble clic en un ejercicio se abre una ventana con su historial.

**Ventana de detalle de un ejercicio**

- Lista de ejecuciones con repeticiones, peso, fecha y hora. Se pueden añadir y borrar.
- Gráfico de barras con la evolución de las ejecuciones.

**Pestaña Daily Insights**

- Gráfico radial que reparte las repeticiones de un día entre los grupos musculares.
- El día se elige con un calendario o con los botones Anterior y Siguiente.

**Menú de datos**

- Importar: carga ejercicios de un archivo `.fitnessTracker`, que es un JSON.
- Exportar: guarda todos los ejercicios y sus ejecuciones en un archivo.
- Vaciar: borra todos los datos.

El archivo `ejerciciosIniciales.fitnessTracker` trae unos cuantos ejercicios de ejemplo para importar.

## Estructura

| Ruta | Contenido |
|---|---|
| `MainWindow.xaml(.cs)` | Ventana principal: pestañas de ejercicios y Daily Insights, gráfico radial e importación y exportación. |
| `EjecucionWindow.xaml(.cs)` | Historial y gráfico de barras de un ejercicio. |
| `Vista/` | Formularios para añadir o editar ejercicios y ejecuciones. |
| `Modelo/` | Clases `Ejercicio`, `Ejecucion` y `Musculos`, que implementan `INotifyPropertyChanged` para el data binding. |
| `Utiles/IntTextBox.cs` | Caja de texto que solo acepta números enteros. |
| `Diagrama de clases.jpg` | Diagrama de clases del modelo. |

## Requisitos y ejecución

- Windows y Visual Studio con el paquete de desarrollo de escritorio de .NET.
- .NET Framework 4.7.2.
- Paquetes NuGet: MahApps.Metro, ControlzEx, Microsoft.Xaml.Behaviors.Wpf y Newtonsoft.Json. Visual Studio los restaura solo a partir de `packages.config`.

Abrir `fitnessTracker24-25.sln`, compilar y ejecutar.
