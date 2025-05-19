using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace WpfSmartHomeApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private double _homeTemp;
        private double _homeHumid;
        private string _detectResult;
        private bool _isDetectOn;
        private string _rainResult;
        private bool _isRainOn;
        private string _airconResult;
        private bool _isairconOn;
        private string _lightResult;
        private bool _islightOn;

        /*----------------------------------------------------------------------*/
        // 온도 속성
        public Double HomeTemp
        {
            get => _homeTemp;
            set => SetProperty(ref _homeTemp, value);
        }
        // 습도 속성
        public Double HomeHumid
        {
            get => _homeHumid;
            set => SetProperty(ref _homeHumid, value);
        }
        /*----------------------------------------------------------------------*/
        // 사람인지
        public string DetectResult
        {
            get => _detectResult;
            set => SetProperty(ref _detectResult, value);
        }
        // 사람인지 여부
        public bool IsDetectOn
        {
            get => _isDetectOn;
            set => SetProperty(ref _isDetectOn, value);
        }
        /*----------------------------------------------------------------------*/
        // 비인지
        public string RainResult
        {
            get => _rainResult;
            set => SetProperty(ref _rainResult, value);
        }
        // 비인지 여부
        public bool IsRainOn
        {
            get => _isRainOn;
            set => SetProperty(ref _isRainOn, value);
        }
        /*----------------------------------------------------------------------*/
        // 에어컨인지
        public string AirConResult
        {
            get => _airconResult;
            set => SetProperty(ref _airconResult, value);
        }
        // 에어컨인지 여부
        public bool IsAirconOn
        {
            get => _isairconOn;
            set => SetProperty(ref _isairconOn, value);
        }
        /*----------------------------------------------------------------------*/
        // 라이트인지
        public string LightResult
        {
            get => _lightResult;
            set => SetProperty(ref _lightResult, value);
        }
        // 라이트인지 여부
        public bool IsLightOn
        {
            get => _islightOn;
            set => SetProperty(ref _islightOn, value);
        }
        /*----------------------------------------------------------------------*/
        // LoadedCommand 에서 앞에 On이 붙고 Command는 삭제
        [RelayCommand]
        public void OnLoaded()
        {
            HomeTemp = 30;
            HomeHumid = 43.2;

            DetectResult = "Detected Human!";
            IsDetectOn = true;
            RainResult = "Raining";
            IsRainOn = true;
            AirConResult = "AirCon On";
            IsAirconOn = true;
            LightResult = "Light On";
            IsLightOn = true;
        }
    }
}
