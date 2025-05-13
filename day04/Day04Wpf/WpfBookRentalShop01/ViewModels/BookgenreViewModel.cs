using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;
using WpfBookRentalShop01.Helpers;
using WpfBookRentalShop01.Models;

namespace WpfBookRentalShop01.ViewModels
{
    public partial class BookgenreViewModel : ObservableObject
    {
        // MahApps.Metro 형태 다이얼로그 코디네이터
        private readonly IDialogCoordinator dialogCoordinator; // 메인뷰 모델과 동일

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

        public BookgenreViewModel(IDialogCoordinator coordinator)
        {
            this.dialogCoordinator = coordinator; // 다이얼로그 코디네이터 초기화

            InitVariable();
            LoadGridFromDb();
        }

        private void InitVariable() // 메서드로 묶기
        {
            SelectedGenre = new Genre();
            SelectedGenre.Names = string.Empty;
            SelectedGenre.Division = string.Empty;
            // 순서가 중요 셀렉트 한후에 업데이트
            _isUpdate = false; // 신규 상태로 변경
        }

        private async void LoadGridFromDb()
        {
            try
            {
                //string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
                string query = "SELECT division, names FROM divtbl";

                ObservableCollection<Genre> genres = new ObservableCollection<Genre>();

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
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
                Common.LOGGER.Error(ex.Message);
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            Common.LOGGER.Info("책장르 데이터 로드");
        }
        
        //SetInitCommand, SaveDataCommand, DeleteDataCommand
        [RelayCommand]
        public void SetInit()
        {
            InitVariable();
        }

        [RelayCommand]
        public async void SaveData()
        {
            // 신규추가, 기존데이터 수정
            //Debug.WriteLine(SelectedGenre.Division);
            //Debug.WriteLine(SelectedGenre.Names);
            //Debug.WriteLine(_isUpdate);
            try
            {
            //string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
            string query = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
            {
                conn.Open();
                // 기존 데이터 수정
                if (_isUpdate) query = "UPDATE divtbl SET names = @names WHERE division = @division";
                // 새 데이터 신규 등록
                else query = "INSERT INTO divtbl VALUES (@division, @names)";

                MySqlCommand cmd = new MySqlCommand( query, conn);
                cmd.Parameters.AddWithValue("@division", SelectedGenre.Division);
                cmd.Parameters.AddWithValue("@names", SelectedGenre.Names);

                var resultCnt = cmd.ExecuteNonQuery();

                if (resultCnt > 0)
                {
                    Common.LOGGER.Info("책장르 데이터 저장완료!");
                        //MessageBox.Show("저장 성공~!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "저장", "저장성공!");
                    }
                else
                {
                    Common.LOGGER.Info("책장르 데이터 저장실패!");
                    //MessageBox.Show("저장 실패~!");
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
                //string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
                string query = "DELETE FROM divtbl WHERE division = @division";

                ObservableCollection<Genre> genres = new ObservableCollection<Genre>();

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@division", SelectedGenre.Division);

                    int resultCnt = cmd.ExecuteNonQuery(); // 한건 삭제가되면 resultCnt = 1, 안지워지면 resultCnt = 0

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info($"책장르 데이터 {SelectedGenre.Division} 삭제완료!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제성공!");
                    }
                    else
                    {
                        Common.LOGGER.Info("책장르 데이터 삭제실패!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제실패...");
                    }
                }
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            LoadGridFromDb(); // 저장이 끝난 후 다시 DB내용을 그리드에 다시 그림
        }
    }
}
