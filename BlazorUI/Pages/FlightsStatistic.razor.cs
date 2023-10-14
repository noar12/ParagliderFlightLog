using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Threading.Tasks;
using global::Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using BlazorUI;
using BlazorUI.Shared;
using Radzen;
using Radzen.Blazor;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.ViewModels;

namespace BlazorUI.Pages
{
    public partial class FlightsStatistic
    {
        protected override void OnInitialized()
        {
            YearToAnalyse = DateTime.Now.Year;
            if (fsvm.FlightsCount == 0)
                return;
            DurationAnalysisResult = HistDataToDurationItem(fsvm.FlightsDurationHistData);
        }
        private Variant variant = Variant.Outlined;
        StatisticalFlightsAnalysis AnalysisToDo;
        int YearToAnalyse;
        void OnAnalyze()
        {
            string[] l_MonthList =
            {
                "January",
                "February",
                "March",
                "April",
                "Mai",
                "June",
                "July",
                "August",
                "Septempber",
                "October",
                "November",
                "December"
            };
            string[] l_YearsText = new string[mvm.YearsOfFlying.Count];
            int i = 0;
            switch (AnalysisToDo)
            {
                case StatisticalFlightsAnalysis.MontlyMedian:
                    MonthlyMedianAnalysisResult = new YearMonthlyStatistic[mvm.YearsOfFlying.Count];
                    foreach (int l_FlightYear in mvm.YearsOfFlying)
                    {
                        double[] l_MonthlyMedians = fsvm.GetMonthlyMedian(l_FlightYear);
                        MonthlyItem[] currentYearMonthlyMedian = new MonthlyItem[l_MonthList.Length];
                        int j = 0;
                        foreach (double monthMedian in l_MonthlyMedians)
                        {
                            currentYearMonthlyMedian[j] = new MonthlyItem()
                            {
                                BarValue = monthMedian,
                                Month = l_MonthList[j],
                            };
                            j++;
                        }

                        MonthlyMedianAnalysisResult[i] = new YearMonthlyStatistic()
                        {
                            MonthlyMedianItems = currentYearMonthlyMedian,
                        };
                        l_YearsText[i] = l_FlightYear.ToString();
                        i++;
                    }

                    break;
                case StatisticalFlightsAnalysis.MonthlyFlightDuration:
                    MonthlyDurationAnalysisResult = new YearMonthlyStatistic[mvm.YearsOfFlying.Count];
                    foreach (int l_FlightYear in mvm.YearsOfFlying)
                    {
                        double[] l_MonthlyDuration = fsvm.GetMonthlyFlightHours(l_FlightYear);
                        MonthlyItem[] currentYearMonthlyDuration = new MonthlyItem[l_MonthList.Length];
                        int j = 0;
                        foreach (double monthDuration in l_MonthlyDuration)
                        {
                            currentYearMonthlyDuration[j] = new MonthlyItem()
                            {
                                BarValue = monthDuration,
                                Month = l_MonthList[j],
                            };
                            j++;
                        }

                        MonthlyDurationAnalysisResult[i] = new YearMonthlyStatistic()
                        {
                            MonthlyMedianItems = currentYearMonthlyDuration,
                        };
                        l_YearsText[i] = l_FlightYear.ToString();
                        ++i;
                    }

                    break;
                case StatisticalFlightsAnalysis.DurationDistribution:
                    fsvm = new FlightsStatisticsViewModel(mvm, new DateTime(YearToAnalyse, 1, 1), new DateTime(YearToAnalyse, 12, 31));
                    if (fsvm.FlightsCount == 0)
                        return;
                    DurationAnalysisResult = HistDataToDurationItem(fsvm.FlightsDurationHistData);
                    break;
                default:
                    break;
            }
        }

        string GetAnalyseName(StatisticalFlightsAnalysis analyse)
        {
            switch (analyse)
            {
                case StatisticalFlightsAnalysis.MontlyMedian:
                    return "Monthly median";
                case StatisticalFlightsAnalysis.DurationDistribution:
                    return "Duration Distribution";
                case StatisticalFlightsAnalysis.MonthlyFlightDuration:
                    return "Monthly flight duration";
                default:
                    return "";
            }
        }

        class DurationItem
        {
            public double BarLocation { get; set; }
            public double BarValue { get; set; }
        }

        class MonthlyItem
        {
            public string Month { get; set; } = string.Empty;
            public double BarValue { get; set; }
        }

        class YearMonthlyStatistic
        {
            public MonthlyItem[] MonthlyMedianItems { get; set; } = new MonthlyItem[0];
        }

        DurationItem[] DurationAnalysisResult = new DurationItem[0];
        YearMonthlyStatistic[] MonthlyMedianAnalysisResult = new YearMonthlyStatistic[0];
        YearMonthlyStatistic[] MonthlyDurationAnalysisResult = new YearMonthlyStatistic[0];
        DurationItem[] HistDataToDurationItem(HistData histData)
        {
            List<DurationItem> durationItems = new List<DurationItem>();
            for (int i = 0; i < histData.Counts.Length; ++i)
            {
                durationItems.Add(new DurationItem() { BarValue = histData.Counts[i], BarLocation = histData.BinEdges[i], });
            }

            return durationItems.ToArray();
        }
    }
}