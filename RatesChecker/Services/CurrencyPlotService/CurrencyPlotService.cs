using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RatesChecker.Models.CurrencyModel;
using RatesChecker.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.CurrencyPlotService
{
    struct CurrencyChartPoint
    {
        public DateTime Date { get; set; }
        public double Rate { get; set; }
    }
    internal class CurrencyPlotService : ICurrencyPlotService
    {

        #region Singleton - обход DI
        private static readonly Lazy<ICurrencyPlotService> _instance =
        new Lazy<ICurrencyPlotService>(() => new CurrencyPlotService());

        public static ICurrencyPlotService Instance => _instance.Value;
        #endregion

        #region Variables
        private PlotModel _PlotModel;
        #endregion

        #region Methods
        public void Initialize(PlotModel plotModel)
        {
            if (plotModel == null)
                throw new ArgumentNullException(nameof(plotModel));

            _PlotModel = plotModel;
        }

        public void ShowPlot(IEnumerable<CurrencyDisplayModel> currencyDynamics, string selectedCurrency)
        {
            EnsureInitialized();

            ClearPlotModel();

            if (currencyDynamics == null)
            {
                ShowEmptyChart("No data to display");
                return;
            }

            List<CurrencyChartPoint> points = currencyDynamics
                .Where(x => x.OfficialRate.HasValue)
                .OrderBy(x => x.Date)
                .Select(x => new CurrencyChartPoint
                {
                    Date = x.Date.Date,
                    Rate = Convert.ToDouble(x.OfficialRate.Value)
                })
                .ToList();

            if (points.Count == 0)
            {
                ShowEmptyChart("No data to display");
                return;
            }

            DateTime minDate = points.Min(x => x.Date);
            DateTime maxDate = points.Max(x => x.Date);

            double totalDays = Math.Max(1, (maxDate - minDate).TotalDays);

            double minRate = points.Min(x => x.Rate);
            double maxRate = points.Max(x => x.Rate);
            double yPadding = CalculateYAxisPadding(minRate, maxRate);

            string currencyName = string.IsNullOrWhiteSpace(selectedCurrency)
                ? "Currency"
                : selectedCurrency;

            ConfigurePlotModel(currencyName, minDate, maxDate);

            ConfigureDateAxis(minDate, maxDate, totalDays);
            ConfigureValueAxis(minRate, maxRate, yPadding);

            AddCurrencyLineSeries(points, currencyName, totalDays);

            _PlotModel.InvalidatePlot(true);
        }

        private void EnsureInitialized()
        {
            if (_PlotModel == null)
                throw new InvalidOperationException("Currency chart service is not initialized. Call Initialize first.");
        }

        private void ClearPlotModel()
        {
            _PlotModel.Axes.Clear();
            _PlotModel.Series.Clear();
            _PlotModel.Annotations.Clear();

            _PlotModel.Title = string.Empty;
            _PlotModel.Subtitle = string.Empty;
        }

        private void ConfigurePlotModel(string currencyName, DateTime minDate, DateTime maxDate)
        {
            _PlotModel.Title = "Exchange Rate Dynamics: " + currencyName;
            _PlotModel.Subtitle = string.Format(
                "{0:dd.MM.yyyy} - {1:dd.MM.yyyy}",
                minDate,
                maxDate);

            _PlotModel.TitleFontSize = 18;
            _PlotModel.SubtitleFontSize = 12;

            _PlotModel.Background = OxyColors.White;
            _PlotModel.PlotAreaBackground = OxyColor.FromRgb(250, 252, 255);
            _PlotModel.PlotAreaBorderColor = OxyColor.FromRgb(210, 220, 235);
            _PlotModel.PlotAreaBorderThickness = new OxyThickness(1);

            _PlotModel.TextColor = OxyColor.FromRgb(35, 40, 50);
        }

        private void ConfigureDateAxis(DateTime minDate, DateTime maxDate, double totalDays)
        {
            DateTimeAxis dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Date",

                FontSize = 11,
                TitleFontSize = 13,

                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(225, 232, 242),
                MajorGridlineThickness = 1,

                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,

                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(170, 180, 195),
                TicklineColor = OxyColor.FromRgb(170, 180, 195),

                Angle = 0,

                Minimum = DateTimeAxis.ToDouble(minDate),
                Maximum = DateTimeAxis.ToDouble(maxDate),

                IsZoomEnabled = false,
                IsPanEnabled = false
            };

            ConfigureDateAxisStep(dateAxis, totalDays);

            _PlotModel.Axes.Add(dateAxis);
        }

        private void ConfigureDateAxisStep(DateTimeAxis dateAxis, double totalDays)
        {
            if (totalDays <= 10)
            {
                dateAxis.IntervalType = DateTimeIntervalType.Days;
                dateAxis.MajorStep = 1;
                dateAxis.StringFormat = "dd.MM";
            }
            else if (totalDays <= 40)
            {
                dateAxis.IntervalType = DateTimeIntervalType.Days;
                dateAxis.MajorStep = 5;
                dateAxis.StringFormat = "dd.MM";
            }
            else if (totalDays <= 180)
            {
                dateAxis.IntervalType = DateTimeIntervalType.Days;
                dateAxis.MajorStep = 30;
                dateAxis.StringFormat = "MM.yyyy";
            }
            else
            {
                dateAxis.IntervalType = DateTimeIntervalType.Days;
                dateAxis.MajorStep = 90;
                dateAxis.StringFormat = "MM.yyyy";
            }
        }

        private void ConfigureValueAxis(double minRate, double maxRate, double yPadding)
        {
            LinearAxis valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Rate, BYN",

                FontSize = 11,
                TitleFontSize = 13,

                Minimum = minRate - yPadding,
                Maximum = maxRate + yPadding,

                StringFormat = "0.0000",

                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(225, 232, 242),
                MajorGridlineThickness = 1,

                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,

                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(170, 180, 195),
                TicklineColor = OxyColor.FromRgb(170, 180, 195),

                IsZoomEnabled = false,
                IsPanEnabled = false
            };

            _PlotModel.Axes.Add(valueAxis);
        }

        private void AddCurrencyLineSeries(IEnumerable<CurrencyChartPoint> points, string currencyName, double totalDays)
        {
            bool showMarkers = totalDays <= 40;

            LineSeries lineSeries = new LineSeries
            {
                Title = currencyName,

                Color = OxyColor.FromRgb(45, 105, 220),
                StrokeThickness = 3,
                MarkerType = showMarkers ? MarkerType.Circle : MarkerType.None,
                MarkerSize = 4,
                MarkerFill = OxyColors.White,
                MarkerStroke = OxyColor.FromRgb(45, 105, 220),
                MarkerStrokeThickness = 2,
                TrackerFormatString = "{0}\nDate: {2:dd.MM.yyyy}\nRate: {4:0.0000} BYN"
            };

            foreach (CurrencyChartPoint point in points)
            {
                lineSeries.Points.Add(new DataPoint(
                    DateTimeAxis.ToDouble(point.Date),
                    point.Rate));
            }

            _PlotModel.Series.Add(lineSeries);
        }

        private double CalculateYAxisPadding(double minRate, double maxRate)
        {
            double range = maxRate - minRate;

            if (range == 0)
                return Math.Max(0.01, Math.Abs(maxRate) * 0.1);

            return range * 0.1;
        }

        private void ShowEmptyChart(string message)
        {
            _PlotModel.Title = message;
            _PlotModel.Subtitle = string.Empty;

            _PlotModel.Background = OxyColors.White;
            _PlotModel.PlotAreaBackground = OxyColors.White;
            _PlotModel.TextColor = OxyColor.FromRgb(35, 40, 50);

            _PlotModel.InvalidatePlot(true);
        }

        #endregion

        #region Constructor
        public CurrencyPlotService()
        {
            _PlotModel = null;
        }
        #endregion
    }

}
