using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoketYerIstasyonu.Models
{
    public class TelemetriVerisi : INotifyPropertyChanged
    {
        private double _yukseklik;
        private double _hiz;
        private double _sicaklik;
        private double _basinc;
        private string _durum = "";

        private double _pitch;
        private double _roll;
        private double _yaw;

        public double Yukseklik { get => _yukseklik; set => SetField(ref _yukseklik, value); }
        public double Hiz { get => _hiz; set => SetField(ref _hiz, value); }
        public double Sicaklik { get => _sicaklik; set => SetField(ref _sicaklik, value); }
        public double Basinc { get => _basinc; set => SetField(ref _basinc, value); }

        public string Durum { get => _durum; set => SetField(ref _durum, value ?? ""); }

        public double Pitch { get => _pitch; set => SetField(ref _pitch, value); }
        public double Roll { get => _roll; set => SetField(ref _roll, value); }
        public double Yaw { get => _yaw; set => SetField(ref _yaw, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
