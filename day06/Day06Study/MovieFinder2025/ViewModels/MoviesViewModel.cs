using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MovieFinder2025.Helpers;
using MovieFinder2025.Models;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using System.Windows.Threading;
using ZstdSharp.Unsafe;

namespace _2025_MovieFinder.ViewModels
{
    public partial class MoviesViewModel : ObservableObject
    {
        private readonly IDialogCoordinator dialogCoordinator;

        public MoviesViewModel(IDialogCoordinator coordinator)
        {
            this.dialogCoordinator = coordinator;
            Common.LOGGER.Info("MovieFinder2025 Start");

            PosterUri = new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute);

            // 시계 작업
            CurrDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // 1초마다 변경
            _timer.Tick += (sender, e) =>
            {
                CurrDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            _timer.Start();
        }
        private string _base_url =  "https://image.tmdb.org/t/p/w300_and_h450_bestv2";
        private readonly DispatcherTimer _timer;


        private string _currDateTime;

        public string CurrDateTime
        {
            get => _currDateTime;
            set => SetProperty(ref _currDateTime, value);
        }



        private string _movieName;

        public string MovieName
        {
            get => _movieName;
            set => SetProperty(ref _movieName, value);
        }

        private ObservableCollection<MovieItem> _movieItems;
        public ObservableCollection<MovieItem> MovieItems
        {
            get => _movieItems;
            set => SetProperty(ref _movieItems, value);
        }

        private MovieItem _selectedMovieItem;
        public MovieItem SelectedMovieItem
        {
            get => _selectedMovieItem;
            set
            {
                SetProperty(ref _selectedMovieItem, value);
                Common.LOGGER.Info($"Selected Movie Item > {value.Poster_path}");
                // 포스터이미지를 포스터영역에 표시
                PosterUri = new Uri($"{_base_url}{value.Poster_path}", UriKind.Absolute);
            }
        }

        private Uri _posteruri;
        

        public Uri PosterUri
        {
            get => _posteruri;
            set => SetProperty(ref _posteruri, value);
        }



        [RelayCommand]
        public async Task SearchMovie()
        {
            //await this.dialogCoordinator.ShowMessageAsync(this, "영화 검색", MovieName);
            if (string.IsNullOrEmpty(MovieName))
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "영화 검색", "영화 제목 입력");
                return;
            }
            var controller = await dialogCoordinator.ShowProgressAsync(this, "대긴중", "검색 중...");
            controller.SetIndeterminate();
            SearchMovie(MovieName);
            await Task.Delay(1000);
            await controller.CloseAsync();
        }
        private string _searchresult;
        public string SearchResult
        {
            get => _searchresult;
            set => SetProperty(ref _searchresult, value);
        }
        private async void SearchMovie(string movieName)
        {
            string tmdb_apiKey = "ad3884608731e93c48ae539f6fa5a37a";   // TMDB에서 신청한 API키
            string encoding_movieName = HttpUtility.UrlEncode(movieName, Encoding.UTF8);    // 입력한 한글을 UTF-8로 변경
            string openApiUri = $"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}" +
                                $"&language=ko-KR&page=1&include_adult=false&query={encoding_movieName}";
            //Debug.WriteLine(openApiUri);
            Common.LOGGER.Info($"TMDB URI : {openApiUri}");
            string result = string.Empty;

            // OpenAPI 실행할 웹 객체. WebRequest, WebResponse -> Deprecated: 추후 삭제될 예정
            //WebRequest req = null;
            //WebResponse res = null;
            HttpClient client = new HttpClient();
            ObservableCollection<MovieItem> movieItems = new ObservableCollection<MovieItem>();
            //Task<MovieSearchResponse?> response;
            //HttpResponseMessage response;
            string reader; // 응답 결과를 담는 객체

            try
            {
                //response = await client.GetAsync(openApiUri);
                var response = await client.GetFromJsonAsync<MovieSearchResponse>(openApiUri);

                foreach (var movie in response.Results)
                {
                    movieItems.Add(movie);
                }
                SearchResult = $" 검색 결과 : {response.Total_results}건";
                Common.LOGGER.Info(MovieName + SearchResult + "검색완료!");
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "예외", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
                SearchResult = $"오류발생";
            }
            MovieItems = movieItems;    // View에 가져갈 속성에 데이터 할당
        }

        [RelayCommand]
        public async Task MovieItemDoubleClick()
        {
            var currMovie = SelectedMovieItem;
            if (currMovie != null)
            {
                StringBuilder sb = new StringBuilder();

                string formattedDate = currMovie.Release_date?.ToString("yyyy-MM-dd") ?? "개봉일 정보 없음";

                sb.Append($"{currMovie.Original_title} ({formattedDate}){Environment.NewLine}");
                sb.Append(currMovie.Overview);

                await this.dialogCoordinator.ShowMessageAsync(this, currMovie.Title, sb.ToString());
            }
        }

        [RelayCommand]
        public async Task AddFavoriteMovies()
        {
            //await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "");
            if (SelectedMovieItem == null)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "추가할 영화를 선택하세요");
                return;
            }
            try
            {
                var query = @"INSERT INTO movieitems
                            (id, adult, backdrop_path, original_language, original_title, overview,
                            popularity, poster_path, release_date, title, vote_average, vote_count)
                            VALUES
                            (@id, @adult, @backdrop_path, @original_language, @original_title, @overview,
                            @popularity,  @poster_path, @release_date, @title, @vote_average, @vote_count)";

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", SelectedMovieItem.Id);
                    cmd.Parameters.AddWithValue("@adult", SelectedMovieItem.Adult);
                    cmd.Parameters.AddWithValue("@backdrop_path", SelectedMovieItem.Backdrop_path);
                    cmd.Parameters.AddWithValue("@original_language", SelectedMovieItem.Original_language);
                    cmd.Parameters.AddWithValue("@original_title", SelectedMovieItem.Original_title);
                    cmd.Parameters.AddWithValue("@overview", SelectedMovieItem.Overview);
                    cmd.Parameters.AddWithValue("@popularity", SelectedMovieItem.Popularity);
                    cmd.Parameters.AddWithValue("@poster_path", SelectedMovieItem.Poster_path);
                    cmd.Parameters.AddWithValue("@release_date", SelectedMovieItem.Release_date);
                    cmd.Parameters.AddWithValue("@title", SelectedMovieItem.Title);
                    cmd.Parameters.AddWithValue("@vote_average", SelectedMovieItem.Vote_average);
                    cmd.Parameters.AddWithValue("@vote_count", SelectedMovieItem.Vote_count);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "즐겨찾기 추가 성공");
                    }
                    else
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "즐겨찾기 추가 실패");
                    }
                }


            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
            }
        }


        [RelayCommand]
        public async Task ViewFavoriteMovies()
        {
            //await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기보기", "즐찾합니당!");
            ObservableCollection<MovieItem> movieItems = new ObservableCollection<MovieItem>();

            try
            {

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    var query = @"SELECT id, adult, backdrop_path, original_language, original_title, overview,
                                         popularity, poster_path, release_date, title, vote_average, vote_count
                                    FROM movieitems";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        movieItems.Add(new MovieItem
                        {
                            Id = reader.GetInt32("id"),
                            Adult = reader.GetBoolean("adult"),
                            Backdrop_path = reader.GetString("backdrop_path"),
                            Original_language = reader.GetString("original_language"),
                            Original_title = reader.GetString("original_title"),
                            Overview = reader.GetString("overview"),
                            Popularity = reader.GetDouble("popularity"),
                            Poster_path = reader.GetString("poster_path"),
                            Release_date = reader.GetDateTime("release_date"),
                            Title = reader.GetString("Title"),
                            Vote_average = reader.GetDouble("vote_average"),
                            Vote_count = reader.GetInt32("vote_count"),
                        });
                    }
                }
                MovieItems = movieItems;
                SearchResult = $" 검색 결과 : {MovieItems.Count}건";
                Common.LOGGER.Info(SearchResult + "검색완료!");
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
            }
        }

        [RelayCommand]
        public async Task DelFavoriteMovies()
        {
            if (SelectedMovieItem == null)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기삭제", "삭제할 영화를 선택하세요.");
                return;
            }
            try
            {

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    var query = @"DELETE FROM movieitems WHERE id = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", SelectedMovieItem.Id);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기삭제", "즐겨찾기 삭제 성공");
                    }
                    else
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기삭제", "즐겨찾기 삭제 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("DUPLICATE ENTRY"))
                {
                    await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기삭제", "이미 추가된 즐겨찾기 입니다.");
                }
                else
                {
                    await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                }
                Common.LOGGER.Fatal(ex.Message);
            }
            await ViewFavoriteMovies(); // 삭제 후 다시 즐겨찾기 보기 실행
        }

        [RelayCommand]
        public async Task ViewMovieTrailer()
        {
            await this.dialogCoordinator.ShowMessageAsync(this, "예고편보기", "즐찾합니당!");
        }
    }
}
