using System;
using System.Linq;
using TradingPlatform.BusinessLayer;


namespace CustomIndicator
{
    public class HistoricalDataExample : Indicator
    {
        private HistoricalData historicalData;

        #region Parameters

        private const string START_TIME_SI = "Start time";
        private const string END_TIME_SI = "End time";

        [InputParameter(START_TIME_SI, 10)]
        public DateTime StartTime
        {
            get
            {
                if (this.startTime == default)
                    this.startTime = Core.Instance.TimeUtils.DateTimeUtcNow.Date;

                return this.startTime;
            }
            set => this.startTime = value;
        }
        private DateTime startTime;

        [InputParameter(END_TIME_SI, 20)]
        public DateTime EndTime
        {
            get
            {
                if (this.endTime == default)
                    this.endTime = Core.Instance.TimeUtils.DateTimeUtcNow.Date.AddDays(1).AddTicks(-1);

                return this.endTime;
            }
            set => this.endTime = value;
        }
        private DateTime endTime;

        #endregion Parameters

        public HistoricalDataExample() : base()
        {
            Name = "HistoricalDataExample";
            SeparateWindow = false;
            OnBackGround = true;
        }

        protected override void OnInit()
        {
            // Pobieranie danych historycznych 1-minutowych
            var fromTime = DateTime.UtcNow.AddDays(-7); // Na przykład, 7 dni wstecz
            var toTime = DateTime.UtcNow; // Do teraz

            Symbol symbol = Core.Instance.Symbols.FirstOrDefault();

            historicalData = symbol.GetHistory(new HistoryRequestParameters
            {
                Symbol = this.Symbol,
                Aggregation = new HistoryAggregationTime(Period.MIN1),
                FromTime = fromTime,
                ToTime = toTime
            });

            if (this.historicalData != null)
            {
                Core.Loggers.Log($"Pobrano {this.historicalData.Count()} 1-minutowych danych od {fromTime} do {toTime}");
            }
            else
            {
                Core.Loggers.Log("Nie udało się pobrać danych historycznych.");
            }
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            // Przykład użycia danych historycznych w metodzie OnUpdate
            if (this.historicalData != null && this.historicalData.Count() > 0)
            {
                var lastBar = this.historicalData[0]; // Pobierz ostatni bar

                // Wyświetl informacje o czasie i cenie zamknięcia ostatniego bara
                Core.Loggers.Log($"Ostatni bar: Czas={lastBar.TimeLeft}, Cena zamknięcia={lastBar[PriceType.Close]}");
            }
        }

        public void ComputeSessions(HistoricalData data)
        {
            for (int i = 0; i < data.Count(); i++) {

                var lastBar = this.historicalData[0]; // Pobierz ostatni bar

            }
        }
    }
}
