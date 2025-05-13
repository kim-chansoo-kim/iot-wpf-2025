using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBookRentalShop01.Models
{
    public class Rental : ObservableObject
    {
        private int _idx;
        private int _memberIdx;
        private int _bookIdx;
        private DateTime _rentaldate;
        private DateTime _returndate;

        public int Idx
        {
            get => _idx;
            set => SetProperty(ref _idx, value);
        }
        public int MemberIdx 
        {
            get => _memberIdx;
            set => SetProperty(ref _memberIdx, value);
        }
        public int BookIdx 
        {
            get => _bookIdx;
            set => SetProperty(ref _bookIdx, value); 
        }
        public DateTime RentalDate
        {
            get => _rentaldate;
            set => SetProperty(ref _rentaldate, value);
        }
        public DateTime ReturnDate 
        { 
            get => _returndate; 
            set => SetProperty(ref _returndate, value);
        }
    }
}
