using ToursService.Dtos;

namespace ToursService.Domain
{
    public static class GeoDistance
    {
        private const double EarthRadiusKm = 6371.0088;
        private static double ToRad(double deg) => deg * Math.PI / 180.0;

        public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            lat1 = ToRad(lat1);
            lat2 = ToRad(lat2);

            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        public static double TotalLengthKm(IReadOnlyList<KeyPointBriefDto> points)
        {
            if (points == null || points.Count < 2) return 0;

            // Koristi redoslijed kakav je stigao:
            var list = points.ToList();

            double total = 0;
            for (int i = 1; i < list.Count; i++)
            {
                var a = list[i - 1];
                var b = list[i];
                total += HaversineKm(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
            }
            return Math.Round(total, 3);
        }
    }
}
