using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fitnessTracker24_25.Modelo
{
    public class Ejecucion : INotifyPropertyChanged
    {
        public int ejecuciones;
        public double peso;
        public DateTime fecheHora;
        public event PropertyChangedEventHandler PropertyChanged;

        public Ejecucion(int ejecuciones, double peso, DateTime fecheHora)
        {
            this.ejecuciones = ejecuciones;
            this.peso = peso;   
            this.fecheHora = fecheHora;
        }

        public int Repeticiones{
            get { return ejecuciones; }
            set { ejecuciones = value; OnPropertyChanged("Repeticiones"); }
        }

        public double Peso
        {
            get { return peso; }
            set { peso = value; OnPropertyChanged("Peso"); }
        }

        public DateTime FechaHora
        {
            get { return fecheHora; }
            set { fecheHora = value; OnPropertyChanged("FechaHora"); }
        }

        public string FechaHoraFormateada
        {
            get { return fecheHora.ToString("dd/MM/yyyy HH:mm:ss"); }
        }


        // Método que lanza el evento PropertyChanged cuando se cambia el valor de cualquier propiedad
        void OnPropertyChanged(String propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
