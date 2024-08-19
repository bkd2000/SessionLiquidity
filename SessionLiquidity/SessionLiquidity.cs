using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace BKD
{
    public class SessionLiquidity : Indicator
    {
        private HistoricalData historicalData;

        #region Parameters

        [InputParameter("UTC", 1)]
        public int offsetUTC = -4;
        
        [InputParameter("Session 1 name", 2)]
        public string session1name;

        [InputParameter("Session 1 start", 3)]
        public DateTime session1start;

        [InputParameter("Session 1 end", 4)]
        public DateTime session1end;

        #endregion Parameters

        public SessionLiquidity() : base()
        {
            Name = "Sessions Liquidity";
            SeparateWindow = false;
            OnBackGround = true;
        }

        protected override void OnInit()
        {
            // Pobieranie danych historycznych 1-minutowych
            var fromTime = DateTime.UtcNow.AddDays(-3); // Na przykład, 3 dni wstecz
            var toTime = DateTime.UtcNow; // Do teraz

            //Symbol symbol = Core.Instance.Symbols.FirstOrDefault();

            //if (symbol == null)
            //{
            //    Core.Loggers.Log("Brak dostępnych symboli.");
            //    return;
            //}

            historicalData = this.Symbol.GetHistory(new HistoryRequestParameters
            {
                //Symbol = symbol,
                Aggregation = new HistoryAggregationTime(Period.MIN1),
                FromTime = fromTime,
                ToTime = toTime
            });

            if (historicalData != null && historicalData.Count > 0)
            {
                Core.Loggers.Log($"Symbol: {this.Symbol}  Pobrano {historicalData.Count} 1-minutowych danych od {fromTime} do {toTime}");
                var lastBar = historicalData[historicalData.Count - 1];
                Core.Loggers.Log($"Ostatni bar: Czas={lastBar.TimeLeft}, Cena zamknięcia={lastBar[PriceType.Close]}");

                GetHighLowBetweenHours(historicalData,"Asia", 4, 8);
            }
            else
            {
                Core.Loggers.Log("Nie udało się pobrać danych historycznych.");
            }
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            // Przykład użycia danych historycznych w metodzie OnUpdate
            if (historicalData != null && historicalData.Count > 0)
            {
                var lastBar = historicalData[historicalData.Count - 1]; // Pobierz ostatni bar

                // Wyświetl informacje o czasie i cenie zamknięcia ostatniego bara
                //Core.Loggers.Log($"Ostatni bar: Czas={lastBar.TimeLeft}, Cena zamknięcia={lastBar[PriceType.Close]}");
            }
        }

        
        public (double High, double Low) GetHighLowBetweenHours(HistoricalData historicalData, string name, int startHour, int endHour)
        {
            // Prepare start and end times for today's date
            DateTime today = DateTime.Now.Date;
            
            DateTime from = today.AddHours(startHour);
            DateTime to = today.AddHours(endHour);

            Core.Loggers.Log($"W funkcji, dane: {historicalData.Count()}, start {from.ToString()}, end {to}");

            // Get history for the specified period
            double highestPrice = double.MinValue;
            double lowestPrice = double.MaxValue;
            DateTime highestTime = from;
            DateTime lowestTime = from;

            // Go through each history item in the period
            for (int i = 0; i < historicalData.Count; i++)
            {
                IHistoryItem historyItem = historicalData[i]; 
                if( historyItem.TimeLeft >= from && historyItem.TimeLeft <= to)
                {
                    // Check if this history item's price is the highest we've seen
                    if (historyItem[PriceType.High] > highestPrice)
                    {
                        Core.Loggers.Log($"High {historicalData[i][PriceType.High]}, HH: {highestPrice}, i={i}");
                        highestPrice = historyItem[PriceType.High];
                        highestTime = historyItem.TimeLeft;
                    }

                    // Check if this history item's price is the lowest we've seen
                    if (historyItem[PriceType.Low] < lowestPrice) 
                    {
                        lowestPrice = historyItem[PriceType.Low];
                        lowestTime = historyItem.TimeLeft;
                    }
                }
                
            }

            // Return the highest and lowest prices we found
            Core.Loggers.Log($"Max: {highestPrice.ToString()} @ {highestTime} - Low: {lowestPrice} @ {lowestTime}");
            sessionsliquidites.Add(highestTime, new SessionLQ(highestTime, highestPrice, name + "high", true));
            sessionsliquidites.Add(lowestTime, new SessionLQ(lowestTime, lowestPrice, name + "low", true));
            return (highestPrice, lowestPrice);
        }
        class SessionLQ
        {
            public SessionLQ(DateTime a, double b, string c, bool e)
            {
                this.start = a;
                this.price = b;
                this.name = c;
                this.high = e;
            }

            public DateTime start { get; set; }
            public DateTime? end { get; set; }
            public double price { get; set; }
            public string name { get; set; }
            public bool high { get; set; }
        }

        SortedDictionary<DateTime, SessionLQ> sessionsliquidites = new SortedDictionary<DateTime, SessionLQ>();
    }
}
