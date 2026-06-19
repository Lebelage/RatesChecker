using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RatesChecker.Models.CurrencyModel
{
    internal class CurrencyModel
    {
        public class NbrbCurrency
        {
            [JsonProperty("Cur_ID")]
            public int Cur_ID { get; set; }

            [JsonProperty("Cur_Abbreviation")]
            public string Cur_Abbreviation { get; set; }

            [JsonProperty("Cur_Name")]
            public string Cur_Name { get; set; }

            [JsonProperty("Cur_DateStart")]
            public DateTime Cur_DateStart { get; set; }

            [JsonProperty("Cur_DateEnd")]
            public DateTime Cur_DateEnd { get; set; }
        }

        public class NbrbRateDynamic
        {
            [JsonProperty("Cur_ID")]
            public int Cur_ID { get; set; }

            [JsonProperty("Date")]
            public DateTime Date { get; set; }

            [JsonProperty("Cur_OfficialRate")]
            public decimal? Cur_OfficialRate { get; set; }
        }


        public class CurrencyDisplayModel : INotifyPropertyChanged
        {
            private DateTime _date;
            private string _name;
            private string _abbreviation;
            private decimal? _officialRate;

            public DateTime Date
            {
                get => _date;
                set { _date = value; OnPropertyChanged(); }
            }

            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(); }
            }

            public string Abbreviation
            {
                get => _abbreviation;
                set { _abbreviation = value; OnPropertyChanged(); }
            }

            public decimal? OfficialRate
            {
                get => _officialRate;
                set { _officialRate = value; OnPropertyChanged(); }
            }


            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
