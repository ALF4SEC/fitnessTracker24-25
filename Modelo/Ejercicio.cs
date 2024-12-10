using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fitnessTracker24_25.Modelo
{
    public class Ejercicio : INotifyPropertyChanged
    {
        public string nombreEjercicio;
        public string descripcionEjercicio;
        public List<Musculos> musculoEjercicio;
        public ObservableCollection<Ejecucion> ejecuciones;
        public event PropertyChangedEventHandler PropertyChanged;

        public Ejercicio(string nombreEjercicio, string descripcionEjercicio, List<Musculos> musculoEjercicio)
        {
            this.nombreEjercicio = nombreEjercicio;
            this.descripcionEjercicio = descripcionEjercicio;
            this.musculoEjercicio = musculoEjercicio;
            ejecuciones = new ObservableCollection<Ejecucion>();
        }

        public string NombreEjercicio
        { 
            get { return nombreEjercicio; }
            set { nombreEjercicio = value; OnPropertyChanged("NombreEjercicio"); } 
        }

        public string DescripcionEjercicio
        {
            get { return descripcionEjercicio; }
            set { descripcionEjercicio = value; OnPropertyChanged("DescripcionEjercicio"); }
        }

        public List<Musculos> MusculosEjercicio
        {
            get { return musculoEjercicio; }
            set { musculoEjercicio = value; OnPropertyChanged("MusculosEjercicio"); }
        }

        [JsonIgnore]
        public string GruposMuscularesString
        {
            get { return string.Join(", ", MusculosEjercicio.Select(m => m.NombreMusculo)); }
        }

        [JsonIgnore]
        public IEnumerable<Ejecucion> EjecucionesOrdenadas
        {
            get
            {
                return ejecuciones.OrderBy(e => e.FechaHora);
            }
        }


        // Método que lanza el evento PropertyChanged cuando se cambia el valor de cualquier propiedad
        void OnPropertyChanged(String propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
