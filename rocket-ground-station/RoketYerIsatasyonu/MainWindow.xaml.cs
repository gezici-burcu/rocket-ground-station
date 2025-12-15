using System;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using RoketYerIstasyonu.ViewModels;

namespace RoketYerIstasyonu
{
    public partial class MainWindow : Window
    {
        private readonly AxisAngleRotation3D _pitch = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
        private readonly AxisAngleRotation3D _roll = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);
        private readonly AxisAngleRotation3D _yaw = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);

        private Transform3DGroup _rotationGroup = new Transform3DGroup();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new AnaViewModel();

            Build3DRocket();   // Artık sadece "boş 3D sahne" kuruyor
            BindOrientation();  // Pitch/Roll/Yaw textler için kalsın
        }

        private void Build3DRocket()
        {
            // Tek transform: yaw->pitch->roll (ileride model eklersen hazır)
            _rotationGroup = new Transform3DGroup();
            _rotationGroup.Children.Add(new RotateTransform3D(_yaw));
            _rotationGroup.Children.Add(new RotateTransform3D(_pitch));
            _rotationGroup.Children.Add(new RotateTransform3D(_roll));

            // 3D sahneyi temizle, sadece ışık bırak (SAÇMA ŞEKİLLER GİDER)
            HelixView.Children.Clear();
            HelixView.Children.Add(new SunLight());

            // Kamera (sahne boşken de düzgün dursun)
            HelixView.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0.8, 3.5),
                LookDirection = new Vector3D(0, -0.2, -3.5),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 55
            };
        }

        private void BindOrientation()
        {
            // Eski C# uyumlu (is not kullanmıyorum)
            var vm = DataContext as AnaViewModel;
            if (vm == null) return;

            vm.TelemetriVerisi.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.TelemetriVerisi.Pitch) ||
                    e.PropertyName == nameof(vm.TelemetriVerisi.Roll) ||
                    e.PropertyName == nameof(vm.TelemetriVerisi.Yaw))
                {
                    Dispatcher.Invoke(() =>
                    {
                        _pitch.Angle = vm.TelemetriVerisi.Pitch;
                        _roll.Angle = vm.TelemetriVerisi.Roll;
                        _yaw.Angle = vm.TelemetriVerisi.Yaw;
                    });
                }
            };
        }

        // STL yükleme şimdilik kapalı (RocketModel hatası vs. yok)
        // private void LoadRocketModel() { }
    }
}
