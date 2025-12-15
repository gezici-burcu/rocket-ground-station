using LiveCharts;
using LiveCharts.Wpf;
using RoketYerIstasyonu.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace RoketYerIstasyonu.ViewModels
{
    public class AnaViewModel : INotifyPropertyChanged
    {
        public TelemetriVerisi TelemetriVerisi { get; } = new TelemetriVerisi();

        private string _vbatText = "11.7 V (%85)";
        private string _gpsText = "FIX (9/12)";
        private string _rssiText = "-72 dBm";
        private string _sonPaketText = "65 ms";

        public string VBatText { get => _vbatText; set => SetField(ref _vbatText, value); }
        public string GpsText { get => _gpsText; set => SetField(ref _gpsText, value); }
        public string RssiText { get => _rssiText; set => SetField(ref _rssiText, value); }
        public string SonPaketText { get => _sonPaketText; set => SetField(ref _sonPaketText, value); }

        public SeriesCollection IrtifaSerisi { get; }
        public SeriesCollection BasincSerisi { get; }
        public SeriesCollection SicaklikSerisi { get; }

        public ChartValues<double> IrtifaValues { get; } = new ChartValues<double>();
        public ChartValues<double> BasincValues { get; } = new ChartValues<double>();
        public ChartValues<double> SicaklikValues { get; } = new ChartValues<double>();

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Random _rng = new Random();
        private int _t = 0;

        public AnaViewModel()
        {
            // 1) DUMMY “UÇUŞ ANI” SNAPSHOT
            TelemetriVerisi.Yukseklik = 1500;
            TelemetriVerisi.Hiz = 120;
            TelemetriVerisi.Basinc = 84;    // kPa
            TelemetriVerisi.Sicaklik = 19;  // °C
            TelemetriVerisi.Durum = "ASCENT / UÇUŞ AKTİF";

            TelemetriVerisi.Pitch = 3.5;
            TelemetriVerisi.Roll = -1.2;
            TelemetriVerisi.Yaw = 12.0;

            // 2) Grafik serileri
            IrtifaSerisi = new SeriesCollection
            {
                new LineSeries { Title = "İrtifa", Values = IrtifaValues, PointGeometry = null }
            };

            BasincSerisi = new SeriesCollection
            {
                new LineSeries { Title = "Basınç", Values = BasincValues, PointGeometry = null }
            };

            SicaklikSerisi = new SeriesCollection
            {
                new LineSeries { Title = "Sıcaklık", Values = SicaklikValues, PointGeometry = null }
            };

            // 3) Başlangıç grafikleri “dolu”
            SeedCharts();

            // 4) Canlı sim (SS için “çalışıyormuş hissi”)
            _timer.Interval = TimeSpan.FromMilliseconds(120);
            _timer.Tick += (_, __) => TickSim();
            _timer.Start();
        }

        private void SeedCharts()
        {
            IrtifaValues.Clear();
            BasincValues.Clear();
            SicaklikValues.Clear();

            // 0..60 s: apogee belirgin
            for (int i = 0; i <= 60; i++)
            {
                double alt =
                    i <= 32 ? (i * 55) : (32 * 55) - ((i - 32) * 40);
                if (alt < 0) alt = 0;
                alt += _rng.NextDouble() * 12;
                IrtifaValues.Add(alt);

                // Basınç U-shape (temsili)
                double p =
                    i <= 32 ? 101 - (i * 0.55) : (101 - 32 * 0.55) + ((i - 32) * 0.28);
                p += (_rng.NextDouble() - 0.5) * 0.35;
                BasincValues.Add(p);

                // Sıcaklık hafif dalga
                double temp = 19 + Math.Sin(i / 6.0) * 0.6 + (_rng.NextDouble() - 0.5) * 0.25;
                SicaklikValues.Add(temp);
            }

            _t = 61;
        }

        private void TickSim()
        {
            // Son paket: 50–80 ms arası
            SonPaketText = $"{_rng.Next(50, 81)} ms";

            // Orientation (hızlı demo)
            TelemetriVerisi.Pitch = 3 + Math.Sin(_t / 10.0) * 4 + (_rng.NextDouble() - 0.5) * 0.4;
            TelemetriVerisi.Roll = -1 + Math.Cos(_t / 11.0) * 3 + (_rng.NextDouble() - 0.5) * 0.4;
            TelemetriVerisi.Yaw = (TelemetriVerisi.Yaw + 0.9) % 360;

            // Grafik akışı (son 80 nokta)
            AddPoint(IrtifaValues, FakeAltitude(_t), 80);
            AddPoint(BasincValues, FakePressure(_t), 80);
            AddPoint(SicaklikValues, FakeTemp(_t), 80);

            TelemetriVerisi.Yukseklik = IrtifaValues[IrtifaValues.Count - 1];
            TelemetriVerisi.Basinc = BasincValues[BasincValues.Count - 1];
            TelemetriVerisi.Sicaklik = SicaklikValues[SicaklikValues.Count - 1];

            TelemetriVerisi.Hiz = 120 + Math.Sin(_t / 7.0) * 15;

            _t++;
        }

        private static void AddPoint(ChartValues<double> values, double v, int max)
        {
            values.Add(v);
            while (values.Count > max) values.RemoveAt(0);
        }

        private double FakeAltitude(int t)
        {
            double baseAlt = 1500 + Math.Sin(t / 12.0) * 180;
            double bump = Math.Sin(t / 30.0) * 120;
            return Math.Max(0, baseAlt + bump + (_rng.NextDouble() - 0.5) * 8);
        }

        private double FakePressure(int t)
        {
            double x = Math.Sin(t / 20.0);
            double p = 90 - (x * 6);
            return p + (_rng.NextDouble() - 0.5) * 0.3;
        }

        private double FakeTemp(int t)
        {
            return 19 + Math.Sin(t / 8.0) * 0.7 + (_rng.NextDouble() - 0.5) * 0.25;
        }

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
