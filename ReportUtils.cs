namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ReportUtils {

        public static double GetEfficiencyChangeForOneReport(EfficiencySpeedData data) {

            if(data.TransportCount == 0 ) {
                data.TransportCount = 1;
            }

            return Math.Abs((data.MinEfficiency - data.MaxEfficiency) / data.TransportCount);

        }

        public static int GetGeneratedReports(ReportGenerationData data) {
            if(data.CurrentReportCount > data.TransportCount ) {
                return 0;
            }
            int result = (int)Math.Floor(data.TransportCount * ((data.MaxEfficiency - data.CurrentEfficiency) / (data.MaxEfficiency - data.MinEfficiency)));

            if(data.CurrentReportCount + result > data.TransportCount) {
                result = data.TransportCount - data.CurrentReportCount;
            }

            if(result < 0 ) {
                result = 0;
            }
            return result;
        }
    }

    public class EfficiencySpeedData {
        public double MinEfficiency { get; set; }
        public double MaxEfficiency { get; set; }
        public int TransportCount { get; set; }
    }

    public class ReportGenerationData {
        public double MinEfficiency { get; set; }
        public double MaxEfficiency { get; set; }
        public double CurrentEfficiency { get; set; }
        public int TransportCount { get; set; }
        public int CurrentReportCount { get; set; }
    }
}