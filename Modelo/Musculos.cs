using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fitnessTracker24_25.Modelo
{
    public class Musculos : INotifyPropertyChanged
    {
        public string nombreMusculo;
        public bool seleccion;
        public event PropertyChangedEventHandler PropertyChanged;

        public Musculos(string nombreMusculo, bool seleccion)
        {
            this.nombreMusculo = nombreMusculo;
            this.seleccion = seleccion;
        }

        public string NombreMusculo
        {
            get { return nombreMusculo; }
            set { nombreMusculo = value; OnPropertyChanged("NombreEjercicio"); }
        }

        public bool Seleccion
        {
            get { return seleccion; }
            set { seleccion = value; OnPropertyChanged("Seleccion"); }
        }

        // Método que lanza el evento PropertyChanged cuando se cambia el valor de cualquier propiedad
        void OnPropertyChanged(String propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
