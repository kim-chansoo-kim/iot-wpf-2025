using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfBookRentalShop01.Models;

namespace WpfBookRentalShop01.ViewModels
{
    public partial class BookgenreViewModel : ObservableObject
    {
        private ObservableCollection<Genre> _genre;

        public ObservableCollection<Genre> Genres
        {
            get => _genre;
            set => SetProperty(ref _genre, value);
        }

        private Genre _selectedGenre;
        public Genre SelectedGenre
        { 
            get => _selectedGenre;
            set
            {
                SetProperty(ref _selectedGenre, value);
                _isUpdate = true; // 수정할 상태
            }
        }

        private bool _isUpdate;

        public BookgenreViewModel()
        {
            _isUpdate = false; // 신규 작성
            LoadGridFromDb();
        }

        private void LoadGridFromDb()
        {
            try
            {
                string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
                string query = "SELECT division, names FROM divtbl";

                ObservableCollection<Genre> genres = new ObservableCollection<Genre>();

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var division = reader.GetString("division");
                        var names = reader.GetString("names");

                        genres.Add(new Genre
                        {
                            Division = division,
                            Names = names
                        });
                    }
                }
                Genres = genres;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        //SetInitCommand, SaveDataCommand, DeleteDataCommand
        [RelayCommand]
        public void SetInit()
        {
            _isUpdate = false;
            SelectedGenre = null;
        }

        [RelayCommand]
        public void SaveData()
        {
            _isUpdate = false;
            SelectedGenre = null;
        }

        [RelayCommand]
        public void DeleteData()
        {
            if (_isUpdate == false)
            {
                MessageBox.Show("선택된 데이터가 아니면 삭제할 수 없습니다.");
                return;
            }

            string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
            string query = "DELETE FROM divtbl WHERE division = @division";

            ObservableCollection<Genre> genres = new ObservableCollection<Genre>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@division", SelectedGenre.Division);

                int resultCnt = cmd.ExecuteNonQuery(); // 한건 삭제가되면 resultCnt = 1, 안지워지면 resultCnt = 0

                if (resultCnt > 0)
                {
                    MessageBox.Show("삭제 성공~!");
                }
                else
                {
                    MessageBox.Show("삭제 실패~!");
                }
            }
        }
    }
}
