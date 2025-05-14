using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MovieFinder2025.Helpers;
using MovieFinder2025.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Web;

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

        private string _base_url =  "https://image.tmdb.org/t/p/w300_and_h450_bestv2";

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
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "예외", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
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

    }
}
