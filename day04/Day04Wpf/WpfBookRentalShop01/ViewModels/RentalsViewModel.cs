using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfBookRentalShop01.Helpers;
using WpfBookRentalShop01.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace WpfBookRentalShop01.ViewModels
{
    public partial class RentalsViewModel : ObservableObject
    {
        private IDialogCoordinator dialogCoordinator;

        private ObservableCollection<Rental> _rentals;

        public ObservableCollection<Rental> Rentals
        {
            get => _rentals;
            set => SetProperty(ref _rentals, value);
        }

        private Rental _selectedRental;

        public Rental SelectedRental
        {
            get => _selectedRental;
            set
            {
                SetProperty(ref _selectedRental, value);
                _isUpdate = true;
            }
        }
        private bool _isUpdate;

        public RentalsViewModel(IDialogCoordinator coordinator)
        {
            this.dialogCoordinator = coordinator;
            InitVariable();
            LoadGridFromDb();
        }
        private void InitVariable()
        {
            SelectedRental = new Rental
            {
                Idx = 0,
                MemberIdx = 0,
                BookIdx = 0,
                RentalDate = DateTime.MinValue,
                ReturnDate = DateTime.MinValue,
            };
            _isUpdate = false;
        }

        private async void LoadGridFromDb()
        {
            try
            {
                string query = "SELECT idx, memberidx, bookidx, rentaldate, returndate FROM rentaltbl";

                ObservableCollection<Rental> rentals = new ObservableCollection<Rental>();

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var idx = reader.GetInt32("idx");
                        var memberidx = reader.GetInt32("memberidx");
                        var bookidx = reader.GetInt32("bookidx");
                        var rentaldate = reader.GetDateTime("rentaldate");
                        var returndate = reader.GetDateTime("renturndate");

                        rentals.Add(new Rental
                        {
                            Idx = idx,
                            MemberIdx = idx,
                            BookIdx = idx,
                            RentalDate = rentaldate,
                            ReturnDate = returndate,
                        });
                    }
                }
                Rentals = rentals; // View에 바인딩필요
            }
            catch (Exception ex)
            {
                Common.LOGGER.Error(ex.Message);
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            Common.LOGGER.Info("책 데이터 로드");
        }
        [RelayCommand]
        public void SetInit()
        {
            InitVariable();
        }

        [RelayCommand]
        public async void SaveData()
        {
            try
            {
                string query = string.Empty;

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    // 기존 데이터 수정
                    if (_isUpdate)
                    {
                        query = @"UPDATE rentaltbl
                                     SET memberidx = @memberidx,
                                         bookidx = @bookidx,
                                         rentaldate = @rentaldate,                                         rentalDate = @rentalDate,                                         rentalDate = @rentalDate,
                                         returndate = @returndate,
                                   WHERE idx = @idx";
                    }
                    // 새 데이터 신규 등록
                    else
                    {
                        query = @"INSERT INTO rentaltbl (memberidx, bookidx, rentaldate, returndate)
                                  VALUES (@memberidx, @bookidx, @rentaldate, @returndate);";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@division", SelectedRental.MemberIdx);
                    cmd.Parameters.AddWithValue("@names", SelectedRental.BookIdx);
                    cmd.Parameters.AddWithValue("@author", SelectedRental.RentalDate);
                    cmd.Parameters.AddWithValue("@isbn", SelectedRental.ReturnDate);
                    // 업데이트일때만 @idx필요
                    if (_isUpdate) cmd.Parameters.AddWithValue("@idx", SelectedRental.Idx);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info("대여 데이터 저장완료!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "저장", "저장성공!");
                    }
                    else
                    {
                        Common.LOGGER.Info("대여 데이터 저장실패!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "저장", "저장실패!");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LOGGER.Error(ex.Message);
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            LoadGridFromDb(); // 저장이 끝난 후 다시 DB내용을 그리드에 다시 그림
        }

        [RelayCommand]
        public async void DeleteData()
        {
            if (_isUpdate == false)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "삭제", "데이터를 선택하세요.");
                return;
            }

            var result = await this.dialogCoordinator.ShowMessageAsync(this, "삭제여부", "삭제하시겠습니까?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
            {
                return;
            }

            try
            {
                string query = "DELETE FROM rentaltbl WHERE idx = @idx";

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", SelectedRental.Idx);

                    int resultCnt = cmd.ExecuteNonQuery(); // 한건 삭제가되면 resultCnt = 1, 안지워지면 resultCnt = 0

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info($"대여 데이터 {SelectedRental.Idx} 삭제완료!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제성공!");
                    }
                    else
                    {
                        Common.LOGGER.Info("책 데이터 삭제실패!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제실패...");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LOGGER.Error(ex);
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            LoadGridFromDb(); // 저장이 끝난 후 다시 DB내용을 그리드에 다시 그림
        }
    }
}
