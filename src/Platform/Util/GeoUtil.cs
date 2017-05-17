namespace Platform.Util
{
    using System;

    public static class GeoUtil
    {
        #region GeoHash算法
        #region Direction enum

        /// <summary>
        /// 枚举4个方向
        /// </summary>
        public enum Direction
        {
            Top = 0,
            Right = 1,
            Bottom = 2,
            Left = 3
        }

        #endregion
        /// <summary>
        /// 默认精度
        /// </summary>
        private const int precision = 4;
        private const string Base32 = "0123456789bcdefghjkmnpqrstuvwxyz";
        private static readonly int[] Bits = new[] { 16, 8, 4, 2, 1 };

        /// <summary>
        /// 相邻矩形
        /// </summary>
        private static readonly string[][] Neighbors = {
                                                           new[]
                                                               {
                                                                   "p0r21436x8zb9dcf5h7kjnmqesgutwvy", // Top
                                                                   "bc01fg45238967deuvhjyznpkmstqrwx", // Right
                                                                   "14365h7k9dcfesgujnmqp0r2twvyx8zb", // Bottom
                                                                   "238967debc01fg45kmstqrwxuvhjyznp", // Left
                                                               }, new[]
                                                                      {
                                                                          "bc01fg45238967deuvhjyznpkmstqrwx", // Top
                                                                          "p0r21436x8zb9dcf5h7kjnmqesgutwvy", // Right
                                                                          "238967debc01fg45kmstqrwxuvhjyznp", // Bottom
                                                                          "14365h7k9dcfesgujnmqp0r2twvyx8zb", // Left
                                                                      }
                                                       };

        private static readonly string[][] Borders = {
                                                         new[] {"prxz", "bcfguvyz", "028b", "0145hjnp"},
                                                         new[] {"bcfguvyz", "prxz", "0145hjnp", "028b"}
                                                     };

        /// <summary>
        /// 根据geoHash前缀与方向取相邻区域的geoHash前缀
        /// </summary>
        /// <param name="hash">前缀</param>
        /// <param name="direction">方向</param>
        /// <returns></returns>
        public static String CalculateAdjacent(String hash, Direction direction)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                hash = hash.ToLower();

                char lastChr = hash[hash.Length - 1];
                int type = hash.Length % 2;
                var dir = (int)direction;
                string nHash = hash.Substring(0, hash.Length - 1);

                if (Borders[type][dir].IndexOf(lastChr) != -1)
                {
                    nHash = CalculateAdjacent(nHash, (Direction)dir);
                }
                return nHash + Base32[Neighbors[type][dir].IndexOf(lastChr)];
            }
            return "";
        }

        public static void RefineInterval(ref double[] interval, int cd, int mask)
        {
            if ((cd & mask) != 0)
            {
                interval[0] = (interval[0] + interval[1]) / 2;
            }
            else
            {
                interval[1] = (interval[0] + interval[1]) / 2;
            }
        }

        /// <summary>
        /// 根据geoHash取经纬度
        /// </summary>
        /// <param name="geohash"></param>
        /// <returns></returns>
        public static double[] Decode(String geohash)
        {
            bool even = true;
            double[] lat = { -90.0, 90.0 };
            double[] lon = { -180.0, 180.0 };

            foreach (char c in geohash)
            {
                int cd = Base32.IndexOf(c);
                for (int j = 0; j < 5; j++)
                {
                    int mask = Bits[j];
                    if (even)
                    {
                        RefineInterval(ref lon, cd, mask);
                    }
                    else
                    {
                        RefineInterval(ref lat, cd, mask);
                    }
                    even = !even;
                }
            }

            return new[] { (lat[0] + lat[1]) / 2, (lon[0] + lon[1]) / 2 };
        }

        /// <summary>
        /// 根据经纬度与精度计算geoHash前缀
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static String Encode(double latitude, double longitude, int precision)
        {
            bool even = true;
            int bit = 0;
            int ch = 0;
            string geohash = "";

            double[] lat = { -90.0, 90.0 };
            double[] lon = { -180.0, 180.0 };

            if (precision < 1 || precision > 20) precision = 12;

            while (geohash.Length < precision)
            {
                double mid;

                if (even)
                {
                    mid = (lon[0] + lon[1]) / 2;
                    if (longitude > mid)
                    {
                        ch |= Bits[bit];
                        lon[0] = mid;
                    }
                    else
                        lon[1] = mid;
                }
                else
                {
                    mid = (lat[0] + lat[1]) / 2;
                    if (latitude > mid)
                    {
                        ch |= Bits[bit];
                        lat[0] = mid;
                    }
                    else
                        lat[1] = mid;
                }

                even = !even;
                if (bit < 4)
                    bit++;
                else
                {
                    geohash += Base32[ch];
                    bit = 0;
                    ch = 0;
                }
            }
            return geohash;
        }

        /// <summary>
        /// 根据经纬度与默认精度计算geoHash前缀
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static String EncodeByPrecision(double latitude, double longitude)
        {
            return Encode(latitude, longitude, precision);
        }
        #endregion

        #region 计算两经纬度之前直线距离(单位KM)
        private const double EARTH_RADIUS = 6378.137;//地球半径
        /// <summary>
        /// 计算两个经纬度之间的直线距离(公里)
        /// </summary>
        /// <param name="p1Lat">第一个点十进制纬度</param>
        /// <param name="p1Lng">第一个点十进制经度</param>
        /// <param name="p2Lat">第二个点十进制纬度</param>
        /// <param name="p2Lng">第二个点十进制经度</param>
        /// <returns>距离(千米)公里数</returns>
        public static double GetDistance(double p1Lat, double p1Lng, double p2Lat, double p2Lng)
        {
            double radian = Math.PI / 180;
            double a = radian * p1Lat;
            double b = radian * p2Lat;
            double c = radian * p1Lng;
            double d = radian * p2Lng;

            return EARTH_RADIUS * Math.Acos(Math.Cos(a) * Math.Cos(b) * Math.Cos(c - d) + Math.Sin(a) * Math.Sin(b));
        }
        /// <summary>
        /// 计算弧度
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double rad(double d)
        {
            return d * 3.14159 / 180.0;
        }
        #endregion

        #region 根据经纬度与距离取矩形
        /// <summary>
        ///  根据经纬度与距离取矩形区域
        /// </summary>
        /// <param name="GLAT">原点的纬度坐标值</param>
        /// <param name="GLON">原点的经度坐标值</param>
        /// <param name="distance">距离(km)</param>
        /// <returns>二维数组,[0,0]最小纬度,[0,1]最小经度,[1,0]最大纬度,[1,1]最大经度</returns>
        public static double[,] GetRangeByDistance(double GLAT, double GLON, double distance)
        {
            double[,] result = new double[2, 2];
            double temp;
            double MaxGLAT;
            double MinGLAT;
            double MaxGLON;
            double MinGLON;
            GetJWDB(GLAT, GLON, distance, 0, out temp, out MaxGLAT);
            GetJWDB(GLAT, GLON, distance, 180, out temp, out MinGLAT);
            GetJWDB(GLAT, GLON, distance, 90, out MaxGLON, out temp);
            GetJWDB(GLAT, GLON, distance, 270, out MinGLON, out temp);
            if (MinGLAT > MaxGLAT)
            {
                result[0, 0] = MaxGLAT;
                result[1, 0] = MinGLAT;
            }
            else
            {
                result[0, 0] = MinGLAT;
                result[1, 0] = MaxGLAT;
            }

            if (MinGLON > MaxGLON)
            {
                result[0, 1] = MaxGLON;
                result[1, 1] = MinGLON;
            }
            else
            {
                result[0, 1] = MinGLON;
                result[1, 1] = MaxGLON;
            }
            return result;
        }

        /// <summary>
        /// 根据经纬度与距离、角度计算矩形四个角纬度
        /// </summary>
        /// <param name="GLAT"></param>
        /// <param name="GLON"></param>
        /// <param name="distance"></param>
        /// <param name="angle"></param>
        /// <param name="BJD"></param>
        /// <param name="BWD"></param>
        private static void GetJWDB(double GLAT, double GLON, double distance, double angle, out double BJD, out double BWD)
        {
            double dx = distance * 1000 * Math.Sin(angle * 3.14159 / 180.0);
            double dy = distance * 1000 * Math.Cos(angle * 3.14159 / 180.0);
            double ec = 6356725 + 21412 * (90.0 - GLAT) / 90.0;
            double ed = ec * Math.Cos(GLAT * 3.14159 / 180);
            BJD = (dx / ed + GLON * 3.14159 / 180.0) * 180.0 / 3.14159;
            BWD = (dy / ec + GLAT * 3.14159 / 180.0) * 180.0 / 3.14159;
        }
        #endregion
    }
}
