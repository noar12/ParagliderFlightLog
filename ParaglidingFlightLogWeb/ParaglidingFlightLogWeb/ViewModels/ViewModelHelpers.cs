using ParagliderFlightLog;
using ParagliderFlightLog.Models;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public static class ViewModelHelpers
    {
        public static double GetTraceLength(List<FlightPoint> points)
        {
            double traceLength = 0.0;
            for (int i = 1; i < points.Count; i++)
            {
                traceLength += points[i].DistanceFrom(points[i - 1]) / 1000;
            }

            return traceLength;
        }

        public static double[] GetVerticalRate(List<FlightPoint> points, int integrationStep)
        {
            if (points.Count != 0)
            {
                var verticalRates = new List<double>();
                for (int i = integrationStep; i < points.Count; i++)
                {
                    verticalRates.Add((points[i].Altitude - points[i - integrationStep].Altitude) / integrationStep);
                }

                return [.. verticalRates];
            }

            return [];
        }

        /// <summary>
        /// jagged array specifically designed to be passed to leaflet lib
        /// </summary>
        /// <returns></returns>
        public static double[][] GetLatLngsArray(List<FlightPoint> flightPoints)
        {
            double[][] output = new double[flightPoints.Count][];
            for (int i = 0; i < output.Length; i++)
            {
                var currentFlightPoint = flightPoints[i];
                output[i] = [currentFlightPoint.Latitude, currentFlightPoint.Longitude];
            }

            return output;
        }
    }
}