using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Threading;

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
        private string _currDateTime;
        private readonly DispatcherTimer _timer;

        public MainViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (sender, e) =>
            {
                CurrDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            _timer.Start();
        }

        /*----------------------------------------------------------------------*/
        // 온도 속성
        #region
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
        #endregion
        /*----------------------------------------------------------------------*/
        // 사람인지
        #region
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
        #endregion
        /*----------------------------------------------------------------------*/
        // 비인지
        #region
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
        #endregion
        /*----------------------------------------------------------------------*/
        // 에어컨인지
        #region
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
        #endregion
        /*----------------------------------------------------------------------*/
        // 라이트인지
        #region
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
        #endregion
        /*----------------------------------------------------------------------*/
        // 시계
        public string CurrDateTime
        {
            get => _currDateTime;
            set => SetProperty(ref _currDateTime, value);
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
