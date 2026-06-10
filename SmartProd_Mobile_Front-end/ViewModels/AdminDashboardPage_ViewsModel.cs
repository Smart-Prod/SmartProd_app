using System.Collections.ObjectModel;
using System.Windows.Input;
using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class HistoricalRecord
    {
        public string Date { get; set; } = "";
        public string Throughput { get; set; } = "";
        public string Status { get; set; } = "";
        public Color StatusColor { get; set; } = Colors.Transparent;
    }

    public class AdminDashboardPage_ViewsModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private int _activeOps;
        private int _totalOps;
        private string _efficiency = "";
        private string _efficiencyTrend = "";
        private int _alertsCount;
        private string _alertMessage = "";
        private string _throughput = "";
        private double _oeeTarget;
        private string _currentShift = "";
        private string _shiftProgress = "";
        private string _lineEfficiency = "";
        private string _latency = "";
        private string _traceability = "";
        private string _oeeMeta = "";
        private string _oeeGap = "";
        private bool _isLiveView = true;
        private string _liveTabColor = "White";
        private string _historyTabColor = "#636E72";
        private string _liveTabBg = "#FF8C00";
        private string _historyTabBg = "#F5F6FA";
        private bool _isBusy;

        public AdminDashboardPage_ViewsModel(ApiService apiService)
        {
            _apiService = apiService;
            ShowLiveViewCommand = new Command(ShowLiveView);
            ShowHistoryCommand = new Command(ShowHistory);
            HistoricalRecords = new ObservableCollection<HistoricalRecord>();
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var (dashboard, error) = await _apiService.GetDashboardAsync();

                if (error != null || dashboard == null)
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Erro", error ?? "Erro ao carregar dashboard", "OK");
                    return;
                }

                ActiveOps = dashboard.ActiveOps;
                TotalOps = dashboard.TotalOps;
                Efficiency = dashboard.Efficiency;
                EfficiencyTrend = dashboard.EfficiencyTrend;
                Throughput = dashboard.Throughput;
                OeeTarget = dashboard.OeeTarget;
                AlertsCount = dashboard.AlertsCount;
                AlertMessage = dashboard.AlertMessage;
                CurrentShift = dashboard.CurrentShift;
                ShiftProgress = dashboard.ShiftProgress;
                LineEfficiency = dashboard.LineEfficiency;
                Latency = dashboard.Latency;
                Traceability = dashboard.Traceability;
                OeeMeta = dashboard.OeeMeta;
                OeeGap = dashboard.OeeGap;

                HistoricalRecords.Clear();
                if (dashboard.History != null)
                {
                    foreach (var h in dashboard.History)
                    {
                        HistoricalRecords.Add(new HistoricalRecord
                        {
                            Date = h.Date,
                            Throughput = h.Throughput,
                            Status = h.Status,
                            StatusColor = h.Status switch
                            {
                                "CONCLUÍDO" => Colors.LimeGreen,
                                "PARCIAL" => Colors.Orange,
                                _ => Colors.Gray
                            }
                        });
                    }
                }
            }
            catch (HttpRequestException)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Erro", "Não foi possível conectar ao servidor.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Application.Current.Windows[0].Page.DisplayAlert("Erro", "Falha ao carregar dashboard.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand ShowLiveViewCommand { get; }
        public ICommand ShowHistoryCommand { get; }

        public ObservableCollection<HistoricalRecord> HistoricalRecords { get; }

        public int ActiveOps
        {
            get => _activeOps;
            set => SetProperty(ref _activeOps, value);
        }

        public int TotalOps
        {
            get => _totalOps;
            set => SetProperty(ref _totalOps, value);
        }

        public string Efficiency
        {
            get => _efficiency;
            set => SetProperty(ref _efficiency, value);
        }

        public string EfficiencyTrend
        {
            get => _efficiencyTrend;
            set => SetProperty(ref _efficiencyTrend, value);
        }

        public int AlertsCount
        {
            get => _alertsCount;
            set => SetProperty(ref _alertsCount, value);
        }

        public string AlertMessage
        {
            get => _alertMessage;
            set => SetProperty(ref _alertMessage, value);
        }

        public string Throughput
        {
            get => _throughput;
            set => SetProperty(ref _throughput, value);
        }

        public double OeeTarget
        {
            get => _oeeTarget;
            set => SetProperty(ref _oeeTarget, value);
        }

        public string CurrentShift
        {
            get => _currentShift;
            set => SetProperty(ref _currentShift, value);
        }

        public string ShiftProgress
        {
            get => _shiftProgress;
            set => SetProperty(ref _shiftProgress, value);
        }

        public string LineEfficiency
        {
            get => _lineEfficiency;
            set => SetProperty(ref _lineEfficiency, value);
        }

        public string Latency
        {
            get => _latency;
            set => SetProperty(ref _latency, value);
        }

        public string Traceability
        {
            get => _traceability;
            set => SetProperty(ref _traceability, value);
        }

        public string OeeMeta
        {
            get => _oeeMeta;
            set => SetProperty(ref _oeeMeta, value);
        }

        public string OeeGap
        {
            get => _oeeGap;
            set => SetProperty(ref _oeeGap, value);
        }

        public bool IsLiveView
        {
            get => _isLiveView;
            set => SetProperty(ref _isLiveView, value);
        }

        public bool IsHistoryView => !IsLiveView;

        public string LiveTabColor
        {
            get => _liveTabColor;
            set => SetProperty(ref _liveTabColor, value);
        }

        public string HistoryTabColor
        {
            get => _historyTabColor;
            set => SetProperty(ref _historyTabColor, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string LiveTabBg
        {
            get => _liveTabBg;
            set => SetProperty(ref _liveTabBg, value);
        }

        public string HistoryTabBg
        {
            get => _historyTabBg;
            set => SetProperty(ref _historyTabBg, value);
        }

        private void ShowLiveView()
        {
            IsLiveView = true;
            LiveTabColor = "White";
            LiveTabBg = "#FF8C00";
            HistoryTabColor = "#636E72";
            HistoryTabBg = "#F5F6FA";
            OnPropertyChanged(nameof(IsHistoryView));
        }

        private void ShowHistory()
        {
            IsLiveView = false;
            LiveTabColor = "#636E72";
            LiveTabBg = "#F5F6FA";
            HistoryTabColor = "White";
            HistoryTabBg = "#FF8C00";
            OnPropertyChanged(nameof(IsHistoryView));
        }
    }
}
