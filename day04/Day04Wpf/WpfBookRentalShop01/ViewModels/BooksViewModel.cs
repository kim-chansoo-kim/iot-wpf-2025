﻿using CommunityToolkit.Mvvm.ComponentModel;
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

namespace WpfBookRentalShop01.ViewModels
{
    public partial class BooksViewModel : ObservableObject
    {
        private IDialogCoordinator dialogCoordinator;

        private ObservableCollection<Book> _books;

        public ObservableCollection<Book> Books
        {
            get => _books;
            set => SetProperty(ref _books, value);
        }

        private Book _selectedBook;

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                SetProperty(ref _selectedBook, value);
                _isUpdate = true;
            }
        }

        private bool _isUpdate;

        public BooksViewModel(IDialogCoordinator coordinator)
        {
            this.dialogCoordinator = coordinator;
            InitVariable();
            LoadGridFromDb();
        }

        private void InitVariable()
        {
            SelectedBook = new Book
            {
                Idx = 0,
                Division = string.Empty,
                Names = string.Empty,
                Author = string.Empty,
                ISBN = string.Empty,
                ReleaseDate = DateTime.Now,
                Price = 0,
            };
            _isUpdate = false;
        }

        private async void LoadGridFromDb()
        {
            try
            {
                string query = "SELECT idx, division, names, author, isbn, releasedate, price FROM bookstbl";

                ObservableCollection<Book> books = new ObservableCollection<Book>();

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var idx = reader.GetInt32("idx");
                        var division = reader.GetString("division");
                        var names = reader.GetString("names");
                        var author = reader.GetString("author");
                        var isbn = reader.GetString("isbn");
                        var releaseDate = reader.GetDateTime("releasedate");
                        var price = reader.GetInt32("price");

                        books.Add(new Book
                        {
                            Idx = idx,
                            Division = division,
                            Names = names,
                            Author = author,
                            ISBN = isbn,
                            ReleaseDate = releaseDate,
                            Price = price
                        });
                    }
                }
                Books = books; // View에 바인딩필요
            }
            catch (Exception ex)
            {
                Common.LOGGER.Error(ex.Message);
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
            }
            Common.LOGGER.Info("책 데이터 로드");
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
                        query = @"UPDATE bookstbl
                                     SET division = @division,
                                         names = @names,
                                         author = @author,
                                         isbn = @isbn,
                                         releaseDate = @releaseDate,
                                         price = @price
                                   WHERE idx = @idx";
                    }
                    // 새 데이터 신규 등록
                    else
                    {
                        query = @"INSERT INTO bookstbl (division, names, author, isbn, releaseDate, price)
                                  VALUES (@division, @names, @author, @isbn, @releaseDate, @price);";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@division", SelectedBook.Division);
                    cmd.Parameters.AddWithValue("@names", SelectedBook.Names);
                    cmd.Parameters.AddWithValue("@author", SelectedBook.Author);
                    cmd.Parameters.AddWithValue("@isbn", SelectedBook.ISBN);
                    cmd.Parameters.AddWithValue("@releaseDate", SelectedBook.ReleaseDate);
                    cmd.Parameters.AddWithValue("@price", SelectedBook.Price);
                    // 업데이트일때만 @idx필요
                    if (_isUpdate) cmd.Parameters.AddWithValue("@idx", SelectedBook.Idx);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info("책 데이터 저장완료!");
                        await this.dialogCoordinator.ShowMessageAsync(this, "저장", "저장성공!");
                    }
                    else
                    {
                        Common.LOGGER.Info("책 데이터 저장실패!");
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
                string query = "DELETE FROM bookstbl WHERE idx = @idx";

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", SelectedBook.Idx);

                    int resultCnt = cmd.ExecuteNonQuery(); // 한건 삭제가되면 resultCnt = 1, 안지워지면 resultCnt = 0

                    if (resultCnt > 0)
                    {
                        Common.LOGGER.Info($"책 데이터 {SelectedBook.Idx} / {SelectedBook.Names} 삭제완료!");
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
        #endregion
    }
}
