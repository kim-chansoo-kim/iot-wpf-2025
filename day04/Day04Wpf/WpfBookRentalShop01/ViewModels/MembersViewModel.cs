using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using WpfBookRentalShop01.Helpers;
using WpfBookRentalShop01.Models;

namespace WpfBookRentalShop01.ViewModels
{
    public partial class MembersViewModel : ObservableObject
    {
        private IDialogCoordinator dialogCoordinator;

        private ObservableCollection<Member> _members;
        public ObservableCollection<Member> Members
        {
            get => _members;
            set => SetProperty(ref _members, value);
        }

        private Member _selectedMember;

        public Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                SetProperty(ref _selectedMember, value);
                _isUpdate = true; // 수정할 상태
            }
        }

        private bool _isUpdate;

        public MembersViewModel(IDialogCoordinator coordinator)
        {
            this.dialogCoordinator = coordinator;
            InitVariable();
            LoadGridFromDb();
        }

        private void InitVariable()
        {
            SelectedMember = new Member
            {
                Idx = 0,
                Names = string.Empty,
                Levels = string.Empty,
                Addr = string.Empty,
                Mobile = string.Empty,
                Email = string.Empty,
            };
            _isUpdate = false;
        }
        private async void LoadGridFromDb()
        {
            try
            {
                string query = "SELECT idx, names, levels, addr, mobile, email FROM membertbl";

                ObservableCollection<Member> members = new ObservableCollection<Member>();

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var idx = reader.GetInt32("idx");
                        var names = reader.GetString("names");
                        var levels = reader.GetString("levels");
                        var addr = reader.GetString("addr");
                        var mobile = reader.GetString("mobile");
                        var email = reader.GetString("email");

                        members.Add(new Member
                        {
                            Idx = idx,
                            Names = names,
                            Levels = levels,
                            Addr = addr,
                            Mobile = mobile,
                            Email = email
                        });
                    }
                }
                Members = members; // View에 바인딩필요
            }
            catch (Exception ex)
            {
                Common.LOGGER.Error(ex.Message);
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            Common.LOGGER.Info("회원 데이터 로드");
        }

        #region
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
                        query = @"UPDATE membertbl
                                     SET names = @names,
                                         levels = @levels,
                                         addr = @addr,
                                         mobile = @mobile,
                                         email = @email
                                   WHERE idx = @idx";
                    }
                    // 새 데이터 신규 등록
                    else
                    {
                        query = @"INSERT INTO membertbl (names, levels, addr, mobile, email)
                                  VALUES (@names, @levels, @addr, @mobile, @email);";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@levels", SelectedMember.Levels);
                    cmd.Parameters.AddWithValue("@names", SelectedMember.Names);
                    cmd.Parameters.AddWithValue("@addr", SelectedMember.Addr);
                    cmd.Parameters.AddWithValue("@mobile", SelectedMember.Mobile);
                    cmd.Parameters.AddWithValue("@email", SelectedMember.Email);
                    // 업데이트일때만 @idx필요
                    if (_isUpdate) cmd.Parameters.AddWithValue("@idx", SelectedMember.Idx);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info("회원 데이터 저장완료!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "저장", "저장성공!");
                    }
                    else
                    {
                        Common.LOGGER.Info("회원 데이터 저장실패!");
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
                string query = "DELETE FROM membertbl WHERE idx = @idx";

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", SelectedMember.Idx);

                    int resultCnt = cmd.ExecuteNonQuery(); // 한건 삭제가되면 resultCnt = 1, 안지워지면 resultCnt = 0

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info($"회원 데이터 {SelectedMember.Idx} / {SelectedMember.Names} 삭제완료!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "삭제", "삭제성공!");
                    }
                    else
                    {
                        Common.LOGGER.Info("회원 데이터 삭제실패!");
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
        #endregion
    }
}
