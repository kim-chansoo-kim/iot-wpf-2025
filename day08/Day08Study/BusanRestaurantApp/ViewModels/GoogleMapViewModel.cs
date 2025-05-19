using BusanRestaurantApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BusanRestaurantApp.ViewModels
{
    internal class GoogleMapViewModel : ObservableObject
    {
        private BusanItem _selectedMatjibItem;
        private string _matjibLocation;

        public GoogleMapViewModel()
        {
            MatjibLocation = "";
        }

        public BusanItem SelectedMatjibItem
        {
            get => _selectedMatjibItem;
            set
            {
                SetProperty(ref _selectedMatjibItem, value);
                // 위도(Latitude/Lat), 경도(Longittude/Lng)
                MatjibLocation = $"https://google.com/maps/place/{SelectedMatjibItem.Lat},{SelectedMatjibItem.Lng}";
            }
        }

        public string MatjibLocation
        {
            get => _matjibLocation;
            set => SetProperty(ref _matjibLocation, value);
        }
    }
}
