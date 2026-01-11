using System;
using System.Globalization;

namespace BiblCalCore
{
    /// <summary>
    /// Local Moon Visibility calculations
    /// Direct port from Windows modBiblcalFunctions.CalcNewMoons with LocalCalcFlag=true
    /// Matches Windows app calculations exactly
    /// </summary>
    public class LocalMoonCalculations
    {
        private readonly IOutputWriter _outputWriter;
        private readonly BiblicalCalendarCalculator _calculator;

        // Constants
        private const double PI = 3.14159265358979;
        private const double DR = 0.01745329251993; // Degrees to radians

        /// <summary>
        /// Rounds a double value to match VB.NET's precision behavior
        /// VB.NET uses banker's rounding (round to even) by default, but for exact matching
        /// we round to a specific number of decimal places to minimize floating-point differences
        /// </summary>
        private static double RoundToPrecision(double value, int decimalPlaces = 10)
        {
            // Round to specified decimal places to match VB.NET intermediate precision
            // This helps minimize floating-point differences between C# and VB.NET
            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        // Month names - use abbreviated names to match Windows output format
        private static readonly string[] MonthName = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Calculation variables (matching Windows app)
        private double _ln = 0; // Month number
        private double _ji = 0; // Julian Day integer part
        private double _jf = 0; // Julian Day fractional part
        private double _jd = 0; // Julian Day number
        private double _jt = 0; // Julian centuries
        private double _ct = 0;
        private double _dt = 0;
        private double _s = 0;
        private double _sa = 0; // Sun's anomaly
        private double _ma = 0; // Moon's anomaly
        private double _ml = 0; // Moon's mean longitude
        private double _ad = 0; // Adjustment
        private double _m = 0; // Month number
        private double _daye = 0; // Day
        private double _gyear = 0; // Gregorian year
        private double _hs = 0; // Hour of sunset
        private double _ht = 0; // Hour of sunset (local)
        private double _hm = 0; // Minute of sunset
        private double _ho = 0; // Hour of moonset
        private double _hq = 0; // Minute of moonset
        private float _il = 0; // Illumination (Windows uses float, not double)
        private double _azs = 0; // Sun's azimuth
        private double _az = 0; // Moon's azimuth
        private double _hal = 0; // Moon's altitude
        private double _sam = 0; // Sun's altitude at moonset
        private double _tl = 0; // Time lag
        private double _visibilityNumber = 0;
        private int _vs = 0; // Visibility status: 1=prob not, 2=prob, 3=visible
        private double _aa = 0; // First day of month Julian Day
        private double _dateOfFirstMonth = 0;
        private int _m0 = 0; // Month counter

        // Additional variables for calculations
        private double _a4 = 0, _a5 = 0, _a6 = 0, _a7 = 0, _a9 = 0;
        private double _an = 0, _ax = 0;
        private double _b2 = 0, _b3 = 0, _b4 = 0, _b6 = 0, _b9 = 0;
        private double _c9 = 0;
        private double _d7 = 0, _d8 = 0, _d9 = 0, _d81 = 0;
        private double _e1 = 0, _e9 = 0, _et = 0, _yu = 0;
        private double _w1 = 0, _w2 = 0, _w3 = 0;
        private double _dnl = 0, _dno = 0;
        private double _hp = 0;
        private double _h1 = 0, _hz = 0, _hw = 0, _hv = 0, _ha = 0, _had = 0, _has = 0;
        private double _cs = 0, _sd = 0, _ds = 0;
        private double _ls = 0, _lo = 0, _lo1 = 0;
        private double _ms = 0, _mm = 0, _mm1 = 0;
        private double _dm = 0, _fm = 0;
        private double _s1 = 0, _s11 = 0, _s4 = 0, _s5 = 0, _s51 = 0;
        private double _l = 0, _l1 = 0, _lb = 0, _lb1 = 0, _lm = 0;
        private double _im = 0, _di = 0;
        private double _ee = 0, _eo = 0;
        private double _ras = 0, _tra2 = 0, _tra4 = 0;
        private double _azc = 0, _aza = 0, _azb = 0, _cazs = 0;
        private double _halp = 0, _ssam = 0;
        private double _kt = 0, _tm = 0;
        private double _mc = 0;
        private double _lt1 = 0, _b31 = 0;
        private double _aj = 0, _sj = 0, _mj = 0;
        private double _originalGmtOffset = double.NaN; // Store original GMT offset for display
        private double _dmt = 0;
        private double _hl = 0;

        // String variables
        private string _htString = "";
        private string _hmString = "";
        private string _hoString = "";
        private string _hqString = "";
        private string _ilString = "";
        private string _azsString = "";
        private string _azString = "";
        private string _halString = "";
        private string _samString = "";
        private string _visibilityNumberString = "";
        private string _dayString = "";

        public LocalMoonCalculations(IOutputWriter outputWriter, BiblicalCalendarCalculator calculator)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        /// <summary>
        /// Calculates new moons for local location (matching CalcNewMoons with LocalCalcFlag=true)
        /// </summary>
        public void CalculateLocalMoons(double year, double longitude, double latitude, double hr, string locationName = "", double originalGmtOffset = double.NaN)
        {
            // Windows GetLocation: LG = Degrees(txtLongDeg) = Longitude, LT = Degrees(txtLatDeg) = Latitude
            // LG = Longitude, LT = Latitude (variable names match their meaning)
            _calculator.GregorianYear = year;
            _calculator.LG = longitude;  // LG is Longitude
            _calculator.LT = latitude;   // LT is Latitude
            _calculator.HR = hr;
            // Store original GMT offset for display in header
            _originalGmtOffset = originalGmtOffset;

            // Initialize variables (matching InitializeVariables)
            InitializeCalculationVariables();

            _dateOfFirstMonth = 0;
            _m0 = 0;
            _dmt = 0;

            // Print header
            _outputWriter.WriteLine($"{FormatYear(year)} CALCULATED NEW MOONS");
            _outputWriter.WriteLine("");

            // Print location
            PrintLocation(locationName, latitude, longitude, hr);

            // Calculate starting month number
            double ly = year + 0.256;
            if (ly >= 0)
            {
                _ln = Math.Floor((ly - 1900) * 12.368277);
            }
            else
            {
                _ln = Math.Floor((ly - 1899) * 12.368277);
            }

            // Calculate for 12-13 months
            while (_m0 < 14)
            {
                FindFirstDay();
                _ln++;
                _m0++;
                _outputWriter.WriteLine(""); // Add blank line after each month
            }
            _m0 = 0;
        }

        private void InitializeCalculationVariables()
        {
            // Matching Windows InitializeVariables (lines 1331-1338)
            // We need to use original values before conversion
            // LG is in degrees (longitude), LT is in degrees (latitude)
            double lgOriginal = _calculator.LG; // In degrees (longitude)
            double ltOriginal = _calculator.LT; // In degrees (latitude)
            double hr = _calculator.HR;

            _sj = 0;
            _mj = 0;
            hr = 12 - hr; // Get International Date Line hour
            // Windows uses LG (which stores latitude) in AJ calculation, even though comment says "longitude hour adjustment"
            // This matches Windows behavior exactly
            _aj = (lgOriginal - hr * 15) * 0.066667;

            // Convert LG to fraction (0-1 range) and LT to radians (matching Windows exactly)
            _calculator.LG = lgOriginal / 360.0;
            _calculator.LT = ltOriginal * DR;

            _sj /= 60.0; // Set up Sunset adjustment
            _mj /= 60.0; // Set up Moonset adjustment
        }

        private void FindFirstDay()
        {
            // Matching Windows FindFirstDay (lines 552-589)
            _outputWriter.WriteLine(" Date     Sunset Moonset   Illum. Sun's  [Moon's at Sunset]  Sun's    Visib   Visible?");
            _outputWriter.WriteLine("(Evening)                    %    Azimuth Azimuth Altitude   Alt(M)   Number");

            _ct = _ln / 1236.85;
            _s = 166.56 + 132.87 * _ct - 0.009173 * Math.Pow(_ct, 2);
            _s -= (Math.Floor(_s / 360)) * 360;

            // Calculate Julian Day of new moon
            _jd = 2415020.75933 + 29.53058868 * _ln + 0.0001178 * Math.Pow(_ct, 2);
            _jd = _jd - 0.000000155 * Math.Pow(_ct, 3) + 0.00033 * Math.Sin(_s * DR);

            _sa = 359.2242 + 29.10535608 * _ln - 0.0000333 * Math.Pow(_ct, 2) - 0.00000347 * Math.Pow(_ct, 3);
            _sa = (_sa - (Math.Floor(_sa / 360)) * 360) * DR;

            _ma = 306.0253 + 385.816918 * _ln + 0.0107306 * Math.Pow(_ct, 2) + 0.00001236 * Math.Pow(_ct, 3);
            _ma = (_ma - (Math.Floor(_ma / 360)) * 360) * DR;

            _ml = 21.2964 + 390.6705065 * _ln - 0.0016528 * Math.Pow(_ct, 2) - 0.00000239 * Math.Pow(_ct, 3);
            _ml = (_ml - (Math.Floor(_ml / 360)) * 360) * DR;

            // Calculate adjustment
            _ad = (0.1734 - 0.000393 * _ct) * Math.Sin(_sa) + 0.0021 * Math.Sin(2 * _sa) - 0.4068 * Math.Sin(_ma);
            _ad = _ad + 0.0161 * Math.Sin(2 * _ma) - 0.0004 * Math.Sin(3 * _ma) + 0.0104 * Math.Sin(2 * _ml);
            _ad = _ad - 0.0051 * Math.Sin(_sa + _ma) - 0.0074 * Math.Sin(_sa - _ma) + 0.0004 * Math.Sin(2 * _ml + _sa);
            _ad = _ad - 0.0004 * Math.Sin(2 * _ml - _sa) - 0.0006 * Math.Sin(2 * _ml + _ma);
            _ad = _ad + 0.001 * Math.Sin(2 * _ml - _ma) + 0.0005 * Math.Sin(_sa + 2 * _ma);

            _jd += _ad;

            if (_jd < 1483746)
            {
                _jd -= 0.02778;
            }

            _dt = (0.41 + 1.2053 * _ct + 0.4992 * Math.Pow(_ct, 2)) / 1440;
            _ji = _jd - _dt;
            _jf = _ji - Math.Floor(_ji);
            _ji = Math.Floor(_ji);

            if (_jf > 0.7)
            {
                _ji++;
            }

            // Track starting month - calculate it first to know when we've moved to next month
            double h = Math.Floor((_ji - 1867216.25) / 36524.25);
            h = _ji + 1 + h - Math.Floor(h / 4);
            double b = h + 1524;
            double c = Math.Floor((b - 122.1) / 365.25);
            double g = Math.Floor(365.25 * c);
            double e = Math.Floor((b - g) / 30.6001);
            FiftyTwoFifty();
        }

        private void FiftyTwoFifty()
        {
            // Matching Windows FiftyTwoFifty (lines 591-637)
            // Convert JDN to Calendar Date (F in Windows code should be _jf)
            double h = Math.Floor((_ji - 1867216.25) / 36524.25);
            h = _ji + 1 + h - Math.Floor(h / 4);
            double b = h + 1524;
            double c = Math.Floor((b - 122.1) / 365.25);
            double g = Math.Floor(365.25 * c);
            double e = Math.Floor((b - g) / 30.6001);
            _daye = b - g - Math.Floor(30.6001 * e) + _jf; // F in Windows code = _jf

            if (e < 13.5)
            {
                _m = e - 1;
            }
            else
            {
                _m = e - 13;
            }

            if (_m > 2.5)
            {
                _gyear = c - 4716;
            }
            else
            {
                _gyear = c - 4715;
            }

            if (_gyear < 1)
            {
                _gyear--;
            }

            int dayInt = (int)Math.Floor(_daye);
            _dayString = dayInt >= 0 ? " " + dayInt.ToString() : dayInt.ToString();
            _dayString = _dayString.Substring(1);
            if (_dayString.Length < 2)
            {
                _dayString = _dayString + " ";
            }
            _outputWriter.Write(_dayString + " " + MonthName[(int)_m]);

            if (_ji < 1483746)
            {
                _jt = (_ji + _calculator.LG + 0.2222 + _dt - 2415020) / 36525;
            }
            else
            {
                _jt = (_ji + _calculator.LG + 0.25 + _dt - 2415020) / 36525;
            }

            FiftyFourHundred();
        }

        private void FiftyFourHundred()
        {
            bool moonsetFound = false;
            PrintSunset();
            if (_dmt == 0)
            {
                _cazs = -(Math.Sin(_ds) / Math.Cos(_calculator.LT));
                _azs = (-Math.Atan(_cazs / Math.Sqrt((-_cazs) * _cazs + 1)) + 1.570796326795) / DR;
                _azsString = FormatToString(_azs, 5);

                if (_ji < 1483746)
                {
                    _jt = (_ji + _hs / 24.0 + _dt + _calculator.LG - 2415020.0278) / 36525.0;
                }
                else
                {
                    _jt = (_ji + _hs / 24.0 + _dt + _calculator.LG - 2415020) / 36525.0;
                }

                do
                {
                    FindPositionOfMoon();
                    if (_mc != 0)
                    {
                        moonsetFound = true;
                        // Windows: HOString = Conversion.Str(HO); // adds leading space for positive numbers
                        _hoString = _ho >= 0 ? " " + _ho.ToString() : _ho.ToString();
                        // Windows: HQString = Conversion.Str(HQ); HQString = HQString.Substring(1);
                        _hqString = _hq >= 0 ? " " + _hq.ToString() : _hq.ToString();
                        _hqString = _hqString.Substring(1);
                    }
                    else
                    {
                        moonsetFound = false;
                        _l1 = _l;
                        _lo1 = _lo;
                        _lb1 = _lb;
                        _lt1 = _calculator.LT;
                        _d81 = _d8;
                        _b31 = _b3;
                        _s51 = _s5;
                        _mm1 = _mm;
                        _s11 = _s1;
                        _mc = _hw / 24.0;
                        if (_ji < 1483746)
                        {
                            _jt = (_ji + _mc + _dt + _calculator.LG - 2415020.0278) / 36525.0;
                        }
                        else
                        {
                            _jt = (_ji + _mc + _dt + _calculator.LG - 2415020) / 36525.0;
                        }
                    }
                }
                while (!moonsetFound);

                if (_ho > 9)
                {
                    _hoString = _hoString.Substring(Math.Max(_hoString.Length - 2, 0));
                }
                if (_hq < 10)
                {
                    _outputWriter.Write("  " + _hoString + ":0" + _hqString);
                }
                else
                {
                    _outputWriter.Write("  " + _hoString + ":" + _hqString);
                }
                _tl = (_ho * 60 + _hq) - (_ht * 60 + _hm);
                _di = Math.Cos(_l1 - _lo1) * Math.Cos(_lb1);
                _di = -Math.Atan(_di / Math.Sqrt((-_di) * _di + 1)) + PI / 2.0;
                _im = PI - _di - (0.1468 * ((1 - 0.0549 * Math.Sin(_mm1)) / (1 - 0.0167 * Math.Sin(_s11))) * DR) * Math.Sin(_di);
                _il = (float)((1 + Math.Cos(_im)) / 2.0);
                _il = (float)(Convert.ToInt32(_il * 10000) / 100.0);
                _ilString = FormatToString(_il, 4);
                _outputWriter.Write("    " + _ilString);
                _outputWriter.Write("  " + _azsString);
                _had = (_s51 + (_hl + 12) * 1.002737908 - _aj - _b31);
                _ha = _had / 3.8197186342055;
                _aza = Math.Sin(_ha);
                _azb = Math.Cos(_ha) * Math.Sin(_lt1) - Math.Tan(_d81) * Math.Cos(_lt1);
                _az = Math.Atan(_aza / _azb);
                _azc = _az / DR;
                _halp = Math.Sin(_lt1) * Math.Sin(_d81) + Math.Cos(_lt1) * Math.Cos(_d81) * Math.Cos(_ha);
                _hal = (Math.Atan(_halp / Math.Sqrt((-_halp) * _halp + 1))) / DR;
                _hal = Math.Floor(_hal * 10000) / 10000.0;
                _halString = FormatToString(_hal, 4);
                if (_aza > 0 && _azb < 0)
                {
                    _az += PI;
                }
                if (_aza < 0 && _azb < 0)
                {
                    _az += PI;
                }
                if (_aza < 0 && _azb > 0)
                {
                    _az += 2 * PI;
                }
                _az /= DR;
                _azString = FormatToString(_az, 5);
                _outputWriter.Write("  " + _azString);
                _outputWriter.Write("   " + _halString);
                NauticalTwilightCalc();
                _visibilityNumber = (_tl + _il * 27 + _hal * 5.5 - _sam * 5) / 1.7;
                
                _visibilityNumberString = FormatToString(_visibilityNumber, 5);
                _outputWriter.Write("    " + _visibilityNumberString);
                if (_visibilityNumber <= 88)
                {
                    _ji++;
                    _mc = 0;
                    _outputWriter.WriteLine("  Not Visible");
                    FiftyTwoFifty();
                    return;
                }
                if (_visibilityNumber > 88 && _visibilityNumber <= 100)
                {
                    _vs = 1;
                    _aa = _ji + 2;
                    _mc = 0;
                    if (_dateOfFirstMonth == 0)
                    {
                        _dateOfFirstMonth = _aa;
                    }
                    _outputWriter.WriteLine("  Prob Not Visible");
                    return;
                }
                else
                {
                    if (_visibilityNumber > 100 && _visibilityNumber <= 112)
                    {
                        _vs = 2;
                        _aa = _ji + 1;
                        _mc = 0;
                        if (_dateOfFirstMonth == 0)
                        {
                            _dateOfFirstMonth = _aa;
                        }
                        _outputWriter.WriteLine("  Prob Visible");
                        return;
                    }
                    else
                    {
                        _vs = 3;
                        _aa = _ji + 1;
                        _mc = 0;
                        _outputWriter.WriteLine("  Visible");
                    }
                }
            }
            if (_dateOfFirstMonth == 0)
            {
                _dateOfFirstMonth = _aa;
            }
        }

        private string FormatYear(double year)
        {
            string yearStr = Math.Abs(year).ToString("F0");
            if (year < 0)
            {
                yearStr = yearStr + " BCE";
            }
            else
            {
                yearStr = yearStr + " CE";
            }
            while (yearStr.Length < 9)
            {
                yearStr = yearStr + " ";
            }
            return yearStr;
        }

        private void PrintSunset()
        {
            // Matching Windows PrintSunset (lines 1055-1109)
            // Solar Coordinates calculations
            _an = (259.18 - 1934.142 * _jt) * DR;
            _eo = (23.452294 - 0.0130125 * _jt - 0.00000164 * Math.Pow(_jt, 2) + 0.000000503 * Math.Pow(_jt, 3) + 0.00256 * Math.Cos(_an)) * DR;
            _a9 = 153.23 + 22518.7541 * _jt;
            _a9 = (_a9 - (Math.Floor(_a9 / 360)) * 360) * DR;
            _b9 = 216.57 + 45037.5082 * _jt;
            _b9 = (_b9 - (Math.Floor(_b9 / 360)) * 360) * DR;
            _c9 = 312.69 + 32964.3577 * _jt;
            _c9 = (_c9 - (Math.Floor(_c9 / 360)) * 360) * DR;
            _d9 = 350.74 + 445267.1142 * _jt - 0.00144 * Math.Pow(_jt, 2);
            _d9 = (_d9 - (Math.Floor(_d9 / 360)) * 360) * DR;
            _e9 = 231.19 + 20.2 * _jt;
            _e9 = (_e9 - (Math.Floor(_e9 / 360)) * 360) * DR;
            _ls = 279.69668 + 36000.76892 * _jt + 0.0003025 * Math.Pow(_jt, 2);
            _ls = (_ls - (Math.Floor(_ls / 360)) * 360) * DR;
            _ms = 358.47583 + 35999.04975 * _jt - 0.00015 * Math.Pow(_jt, 2) - 0.0000033 * Math.Pow(_jt, 3);
            _ms = (_ms - (Math.Floor(_ms / 360)) * 360) * DR;
            _ee = 0.01675104 - 0.0000418 * _jt - 0.000000126 * Math.Pow(_jt, 2);
            _yu = Math.Pow(Math.Tan(_eo / 2), 2);
            _et = _yu * Math.Sin(2 * _ls) - 2 * _ee * Math.Sin(_ms) + 4 * _ee * _yu * Math.Sin(_ms) * Math.Cos(2 * _ls);
            _et = (_et - 1.0 / 2 * Math.Pow(_yu, 2) * Math.Sin(4 * _ls) - 5.0 / 4 * Math.Pow(_ee, 2) * Math.Sin(2 * _ms)) * 3.819718634;
            _cs = (1.91946 - 0.004789 * _jt - 0.000014 * Math.Pow(_jt, 2)) * Math.Sin(_ms);
            _cs = (_cs + (0.020094 - 0.0001 * _jt) * Math.Sin(2 * _ms) + 0.000293 * Math.Sin(3 * _ms)) * DR;
            _lo = _ls + _cs + (0.00134 * Math.Cos(_a9) + 0.00154 * Math.Cos(_b9) + 0.002 * Math.Cos(_c9) + 0.00179 * Math.Sin(_d9) + 0.00178 * Math.Sin(_e9) - 0.00569 - 0.00479 * Math.Sin(_an)) * DR;
            _sd = Math.Sin(_eo) * Math.Sin(_lo);

            if (_dmt == 0)
            {
                _ds = Math.Atan(_sd / Math.Sqrt((-_sd) * _sd + 1));
                _h1 = (-0.01454 - Math.Sin(_calculator.LT) * Math.Sin(_ds)) / (Math.Cos(_calculator.LT) * Math.Cos(_ds));
                _hs = ((-Math.Atan(_h1 / Math.Sqrt((-_h1) * _h1 + 1)) + 1.570796326795) * 3.8197186342055) - _et;
                _hl = _hs + _aj + _sj + 0.00833333333;
                _hm = Math.Floor((_hl - Math.Floor(_hl)) * 60);
                _ht = Math.Floor(_hl);
                _htString = _ht >= 0 ? " " + _ht.ToString() : _ht.ToString();
                _hmString = _hm >= 0 ? " " + _hm.ToString() : _hm.ToString();
                _hmString = _hmString.Substring(1);
                _outputWriter.Write("   ");
                if (_ht < 10)
                {
                    _htString = " " + _htString;
                }
                if (_hm < 10)
                {
                    _outputWriter.Write(_htString + ":0" + _hmString);
                }
                else
                {
                    _outputWriter.Write(_htString + ":" + _hmString);
                }
            }
        }

        private void FindPositionOfMoon()
        {
            // Matching Windows FindPositionOfMoon (lines 827-970) - Formula 30, Position of the Moon
            _a4 = 51.2 + 20.2 * _jt;
            _a4 = (_a4 - Math.Floor(_a4 / 360) * 360) * DR;
            _a5 = 346.56 + 132.87 * _jt - 0.0091731 * Math.Pow(_jt, 2);
            _a5 = (_a5 - Math.Floor(_a5 / 360) * 360) * DR;
            _a6 = 0.003964 * Math.Sin(_a5);
            _ax = 259.183275 - 1934.142 * _jt + 0.002078 * Math.Pow(_jt, 2) + 0.0000022 * Math.Pow(_jt, 3);
            _an = (_ax - Math.Floor(_ax / 360) * 360) * DR;
            _a7 = _ax + 275.05 - 2.3 * _jt;
            _a7 = (_a7 - (Math.Floor(_a7 / 360)) * 360) * DR;
            _mm = 296.104608 + 477198.8491 * _jt + 0.009192 * Math.Pow(_jt, 2);
            _mm = _mm + 0.0000144 * Math.Pow(_jt, 3) + 0.000817 * Math.Sin(_a4);
            _mm = _mm + _a6 + 0.002541 * Math.Sin(_an);
            _mm = (_mm - (Math.Floor(_mm / 360)) * 360) * DR;
            _lm = 270.434164 + 481267.8831 * _jt - 0.001133 * Math.Pow(_jt, 2) + 0.0000019 * Math.Pow(_jt, 3);
            _lm = _lm + 0.000233 * Math.Sin(_a4) + _a6 + 0.001964 * Math.Sin(_an);
            _lm = (_lm - (Math.Floor(_lm / 360)) * 360);
            _dnl = (-(17.2327 + 0.01737 * _jt)) * Math.Sin(_an) - (1.2729 + 0.00013 * _jt) * Math.Sin(2 * _ls) + 0.2088 * Math.Sin(2 * _an) - 0.2037 * Math.Sin(2 * _lm) + (0.1261 - 0.00031 * _jt) * Math.Sin(_ms) + 0.0675 * Math.Sin(_mm);
            _dnl = _dnl - (0.0497 - 0.00012 * _jt) * Math.Sin(2 * _ls + _ms) - 0.0342 * Math.Sin(2 * _lm - _an) - 0.0261 * Math.Sin(2 * _lm + _mm) + 0.0214 * Math.Sin(2 * _ls - _ms) - 0.0149 * Math.Sin(2 * _ls - 2 * _lm + _mm);
            _dnl = _dnl + 0.0124 * Math.Sin(2 * _ls - _an) + 0.114 * Math.Sin(2 * _lm - _mm);
            _dno += 0.0183 * Math.Cos(2 * _lm - _an);
            _s1 = 358.475833 + 35999.0498 * _jt - 0.00015 * Math.Pow(_jt, 2) - 0.0000033 * Math.Pow(_jt, 3);
            _s1 -= 0.001778 * Math.Sin(_a4);
            _s1 = (_s1 - (Math.Floor(_s1 / 360)) * 360) * DR;
            _dm = 350.737486 + 445267.1142 * _jt - 0.001436 * Math.Pow(_jt, 2) + 0.0000019 * Math.Pow(_jt, 3);
            _dm = _dm + 0.002011 * Math.Sin(_a4) + _a6 + 0.001964 * Math.Sin(_an);
            _dm = (_dm - (Math.Floor(_dm / 360)) * 360) * DR;
            _fm = 11.250889 + 483202.0251 * _jt - 0.003211 * Math.Pow(_jt, 2) - 0.0000003 * Math.Pow(_jt, 3);
            _fm = _fm + _a6 - 0.024691 * Math.Sin(_an) - 0.004328 * Math.Sin(_a7);
            _fm = (_fm - (Math.Floor(_fm / 360)) * 360) * DR;
            _e1 = 1 - 0.002495 * _jt - 0.00000752 * Math.Pow(_jt, 2);

            // Calculate Moon's longitude (L) - Full series
            _l = _lm + 6.28875 * Math.Sin(_mm) + 1.274018 * Math.Sin(2 * _dm - _mm) + 0.658309 * Math.Sin(2 * _dm);
            _l = _l + 0.213616 * Math.Sin(2 * _mm) - (0.185596 * Math.Sin(_s1)) * _e1 - 0.114336 * Math.Sin(2 * _fm);
            _l = _l + 0.058793 * Math.Sin(2 * _dm - 2 * _mm) + (0.057212 * Math.Sin(2 * _dm - _s1 - _mm)) * _e1;
            _l = _l + 0.05332 * Math.Sin(2 * _dm + _mm) + (0.045874 * Math.Sin(2 * _dm - _s1)) * _e1;
            _l = _l + (0.041024 * Math.Sin(_mm - _s1)) * _e1 - 0.034718 * Math.Sin(_dm);
            _l = _l - _e1 * 0.030465 * Math.Sin(_s1 + _mm) + 0.015326 * Math.Sin(2 * _dm - 2 * _fm);
            _l = _l - 0.012528 * Math.Sin(2 * _fm + _mm) - 0.01098 * Math.Sin(2 * _fm - _mm) + 0.010674 * Math.Sin(4 * _dm - _mm);
            _l = _l + 0.010034 * Math.Sin(3 * _mm) + 0.008548 * Math.Sin(4 * _dm - 2 * _mm);
            _l = _l - 0.00791 * Math.Sin(_s1 - _mm + 2 * _dm) * _e1 - _e1 * 0.006783 * Math.Sin(2 * _dm + _s1);
            _l = _l + 0.005162 * Math.Sin(_mm - _dm) + _e1 * 0.005 * Math.Sin(_s1 + _dm);
            _l = _l + _e1 * 0.004049 * Math.Sin(_mm - _s1 + 2 * _dm) + 0.003996 * Math.Sin(2 * _mm + 2 * _dm);
            _l = _l + 0.003862 * Math.Sin(4 * _dm) + 0.003665 * Math.Sin(2 * _dm - 3 * _mm);
            _l = _l + _e1 * 0.002695 * Math.Sin(2 * _mm - _s1) + 0.002602 * Math.Sin(_mm - 2 * _fm - 2 * _dm);
            _l = _l + _e1 * 0.002396 * Math.Sin(2 * _dm - _s1 - 2 * _mm) - 0.002349 * Math.Sin(_mm + _dm);
            _l = _l + Math.Pow(_e1, 2) * 0.002249 * Math.Sin(2 * _dm - 2 * _s1) - _e1 * 0.002125 * Math.Sin(2 * _mm + _s1);
            _l = _l - Math.Pow(_e1, 2) * 0.002079 * Math.Sin(2 * _s1) + Math.Pow(_e1, 2) * 0.002059 * Math.Sin(2 * _dm - _mm - 2 * _s1);
            _l = _l - 0.001773 * Math.Sin(_mm + 2 * _dm - 2 * _fm) - 0.001595 * Math.Sin(2 * _fm + 2 * _dm);
            _l = _l + _e1 * 0.00122 * Math.Sin(4 * _dm - _s1 - _mm) - 0.00111 * Math.Sin(2 * _mm + 2 * _fm);
            _l = _l + 0.000892 * Math.Sin(_mm - 3 * _dm) - _e1 * 0.000811 * Math.Sin(_s1 + _mm + 2 * _dm);
            _l = _l + _e1 * 0.000761 * Math.Sin(4 * _dm - _s1 - 2 * _mm) + Math.Pow(_e1, 2) * 0.000717 * Math.Sin(_mm - 2 * _s1);
            _l = _l + Math.Pow(_e1, 2) * 0.000704 * Math.Sin(_mm - 2 * _s1 - 2 * _dm) + _e1 * 0.000693 * Math.Sin(_s1 - 2 * _mm + 2 * _dm);
            _l += _e1 * 0.000598 * Math.Sin(2 * _dm - _s1 - 2 * _fm);
            _l = _l + 0.00055 * Math.Sin(_mm + 4 * _dm) + 0.000538 * Math.Sin(4 * _mm);
            _l = _l + _e1 * 0.000521 * Math.Sin(4 * _dm - _s1) + 0.000486 * Math.Sin(2 * _mm - _dm);

            // Calculate Moon's latitude (LB) - Full series
            _lb = 5.128189 * Math.Sin(_fm) + 0.280606 * Math.Sin(_mm + _fm) + 0.277693 * Math.Sin(_mm - _fm);
            _lb = _lb + 0.173238 * Math.Sin(2 * _dm - _fm) + 0.055413 * Math.Sin(2 * _dm + _fm - _mm);
            _lb = _lb + 0.046272 * Math.Sin(2 * _dm - _fm - _mm) + 0.032573 * Math.Sin(2 * _dm + _fm);
            _lb = _lb + 0.017198 * Math.Sin(2 * _mm + _fm) + 0.009267 * Math.Sin(2 * _dm + _mm - _fm);
            _lb = _lb + 0.008823 * Math.Sin(2 * _mm - _fm) + _e1 * 0.008247 * Math.Sin(2 * _dm - _s1 - _fm);
            _lb = _lb + 0.004323 * Math.Sin(2 * _dm - _fm - 2 * _mm) + 0.0042 * Math.Sin(2 * _dm + _fm + _mm);
            _lb = _lb + _e1 * 0.003372 * Math.Sin(_fm - _s1 - 2 * _dm) + _e1 * 0.002472 * Math.Sin(2 * _dm + _fm - _s1 - _mm);
            _lb = _lb + _e1 * 0.002222 * Math.Sin(2 * _dm + _fm - _s1) + _e1 * 0.002072 * Math.Sin(2 * _dm - _fm - _s1 - _mm);
            _lb = _lb + _e1 * 0.001877 * Math.Sin(_fm - _s1 + _mm) + 0.001828 * Math.Sin(4 * _dm - _fm - _mm);
            _lb = _lb - _e1 * 0.001803 * Math.Sin(_fm + _s1) - 0.00175 * Math.Sin(3 * _fm) + _e1 * 0.00157 * Math.Sin(_mm - _s1 - _fm);
            _lb = _lb - 0.001487 * Math.Sin(_fm + _dm) - _e1 * 0.001481 * Math.Sin(_fm + _s1 + _mm);
            _lb = _lb + _e1 * 0.001417 * Math.Sin(_fm - _s1 - _mm) + _e1 * 0.00135 * Math.Sin(_fm - _s1) + 0.00133 * Math.Sin(_fm - _dm);
            _lb = _lb + 0.001106 * Math.Sin(_fm + 3 * _mm) + 0.00102 * Math.Sin(4 * _dm - _fm);
            _lb = _lb + 0.000833 * Math.Sin(_fm + 4 * _dm - _mm) + 0.000781 * Math.Sin(_mm - 3 * _fm);
            _lb = _lb + 0.00067 * Math.Sin(_fm + 4 * _dm - 2 * _mm) + 0.000606 * Math.Sin(2 * _dm - 3 * _fm);
            _lb = _lb + 0.000597 * Math.Sin(2 * _dm + 2 * _mm - _fm) + _e1 * 0.000492 * Math.Sin(2 * _dm + _mm - _s1 - _fm);
            _lb = _lb + 0.00045 * Math.Sin(2 * _mm - _fm - 2 * _dm) + 0.000439 * Math.Sin(3 * _mm - _fm);
            _lb = _lb + 0.000423 * Math.Sin(_fm + 2 * _dm + 2 * _mm) + 0.000422 * Math.Sin(2 * _dm - _fm - 3 * _mm);
            _lb = _lb - _e1 * 0.000367 * Math.Sin(_s1 + _fm + 2 * _dm - _mm) - _e1 * 0.000353 * Math.Sin(_s1 + _fm + 2 * _dm);
            _lb = _lb + 0.000331 * Math.Sin(_fm + 4 * _dm) + _e1 * 0.000317 * Math.Sin(2 * _dm + _fm - _s1 + _mm);
            _lb = _lb + Math.Pow(_e1, 2) * 0.000306 * Math.Sin(2 * _dm - 2 * _s1 - _fm) - 0.000283 * Math.Sin(_mm + 3 * _fm);

            _hp = 0.950724 + 0.051818 * Math.Cos(_mm) + 0.009531 * Math.Cos(2 * _dm - _mm);
            _hp = _hp + 0.007843 * Math.Cos(2 * _dm) + 0.002824 * Math.Cos(2 * _mm);
            _hp = _hp + 0.000857 * Math.Cos(2 * _dm + _mm) + _e1 * (0.000533 * Math.Cos(2 * _dm - _s1));
            _hp = _hp + _e1 * (0.000401 * Math.Cos(2 * _dm - _s1 - _mm)) + _e1 * (0.00032 * Math.Cos(_mm - _s1));
            _hp = _hp - 0.000271 * Math.Cos(_dm) - _e1 * (0.000264 * Math.Cos(_s1 + _mm));
            _hp = _hp - 0.000198 * Math.Cos(2 * _fm - _mm) + 0.000173 * Math.Cos(3 * _mm);
            _hp = _hp + 0.000167 * Math.Cos(4 * _dm - _mm) - _e1 * (0.000111 * Math.Cos(_s1));
            _hp = _hp + 0.000103 * Math.Cos(4 * _dm - 2 * _mm) - 0.000084 * Math.Cos(2 * _mm - 2 * _dm);
            _hp = _hp - _e1 * (0.000083 * Math.Cos(2 * _dm + _s1)) + 0.000079 * Math.Cos(2 * _dm + 2 * _mm);
            _hp = _hp + 0.000072 * Math.Cos(4 * _dm) + _e1 * (0.000064 * Math.Cos(2 * _dm - _s1 + _mm));
            _hp = _hp - _e1 * (0.000063 * Math.Cos(2 * _dm + _s1 - _mm)) + _e1 * (0.000041 * Math.Cos(_s1 + _dm));
            _hp = _hp + _e1 * (0.000035 * Math.Cos(2 * _mm - _s1)) - 0.000033 * Math.Cos(3 * _mm - 2 * _dm);
            _hp = _hp - 0.00003 * Math.Cos(_mm + _dm) - 0.000029 * Math.Cos(2 * _fm - 2 * _dm);
            _hp = _hp - _e1 * (0.000029 * Math.Cos(2 * _mm + _s1)) + Math.Pow(_e1, 2) * (0.000026 * Math.Cos(2 * _dm - 2 * _s1));
            _hp = _hp - 0.000023 * Math.Cos(2 * _fm - 2 * _dm + _mm) + _e1 * (0.000019 * Math.Cos(4 * _dm - _s1 - _mm));

            _w1 = 0.0004664 * Math.Cos(_an);
            _w3 = (275.05 - 2.3 * _jt) * DR;
            _w2 = 0.0000754 * Math.Cos(_an + _w3);
            _lb *= (1 - _w1 - _w2);
            _l *= DR;
            _lb *= DR;

            _d7 = Math.Sin(_lb) * Math.Cos(_eo) + Math.Cos(_lb) * Math.Sin(_eo) * Math.Sin(_l);
            _d8 = Math.Atan(_d7 / Math.Sqrt((-_d7) * _d7 + 1));
            _hz = (((0.7275 * _hp - 0.5666667) * DR) - Math.Sin(_calculator.LT) * Math.Sin(_d8)) / (Math.Cos(_calculator.LT) * Math.Cos(_d8));
            _hw = ((-Math.Atan(_hz / Math.Sqrt((-_hz) * _hz + 1)) + 1.570796326795) * 3.8197186342055);
            _b2 = (Math.Sin(_l) * Math.Cos(_eo) - Math.Tan(_lb) * Math.Sin(_eo));
            _b4 = Math.Cos(_l);
            _b6 = Math.Atan(_b2 / _b4);
            if (_b2 > 0 && _b4 < 0)
            {
                _b6 += PI;
            }
            if (_b2 < 0 && _b4 < 0)
            {
                _b6 += PI;
            }
            if (_b2 < 0 && _b4 > 0)
            {
                _b6 += 2 * PI;
            }
            _b3 = _b6 * 3.8197186342055;
            _kt = (_ji - 2415019.5) / 36525;
            _s4 = 6.6460656 + 2400.051262 * _kt + 0.00002581 * Math.Pow(_kt, 2);
            _s5 = _s4 - Math.Floor(_s4 / 24) * 24;
            _tm = 12 + _s5 - _b3 - 0.065712 * _dt;
            if (_tm > 0)
            {
                _tm = 24 - _tm;
            }
            else
            {
                _tm = -_tm;
            }
            _hw = _hw + _tm + _mj + 0.0241666666;
            if (_hw > 24)
            {
                _hw -= 24;
            }
            _hv = _hw + _aj;
            if (_hv > 24)
            {
                _hv -= 24;
            }
            _hq = Math.Floor((_hv - Math.Floor(_hv)) * 60);
            _ho = Math.Floor(_hv);
        }

        private void NauticalTwilightCalc()
        {
            // Matching Windows NauticalTwilightCalc (lines 972-1029)
            _an = (259.18 - 1934.142 * _jt) * DR;
            _eo = (23.452294 - 0.0130125 * _jt - 0.00000164 * Math.Pow(_jt, 2) + 0.000000503 * Math.Pow(_jt, 3) + 0.00256 * Math.Cos(_an)) * DR;
            _a9 = 153.23 + 22518.7541 * _jt;
            _a9 = (_a9 - (Math.Floor(_a9 / 360)) * 360) * DR;
            _b9 = 216.57 + 45037.5082 * _jt;
            _b9 = (_b9 - (Math.Floor(_b9 / 360)) * 360) * DR;
            _c9 = 312.69 + 32964.3577 * _jt;
            _c9 = (_c9 - (Math.Floor(_c9 / 360)) * 360) * DR;
            _d9 = 350.74 + 445267.1142 * _jt - 0.00144 * Math.Pow(_jt, 2);
            _d9 = (_d9 - (Math.Floor(_d9 / 360)) * 360) * DR;
            _e9 = 231.19 + 20.2 * _jt;
            _e9 = (_e9 - (Math.Floor(_e9 / 360)) * 360) * DR;
            _ls = 279.69668 + 36000.76892 * _jt + 0.0003025 * Math.Pow(_jt, 2);
            _ls = (_ls - (Math.Floor(_ls / 360)) * 360) * DR;
            _ms = 358.47583 + 35999.04975 * _jt - 0.00015 * Math.Pow(_jt, 2) - 0.0000033 * Math.Pow(_jt, 3);
            _ms = (_ms - (Math.Floor(_ms / 360)) * 360) * DR;
            _ee = 0.01675104 - 0.0000418 * _jt - 0.000000126 * Math.Pow(_jt, 2);
            _yu = Math.Pow(Math.Tan(_eo / 2), 2);
            _et = _yu * Math.Sin(2 * _ls) - 2 * _ee * Math.Sin(_ms) + 4 * _ee * _yu * Math.Sin(_ms) * Math.Cos(2 * _ls);
            _et = (_et - 1.0 / 2 * Math.Pow(_yu, 2) * Math.Sin(4 * _ls) - 5.0 / 4 * Math.Pow(_ee, 2) * Math.Sin(2 * _ms)) * 3.819718634;
            _cs = (1.91946 - 0.004789 * _jt - 0.000014 * Math.Pow(_jt, 2)) * Math.Sin(_ms);
            _cs = (_cs + (0.020094 - 0.0001 * _jt) * Math.Sin(2 * _ms) + 0.000293 * Math.Sin(3 * _ms)) * DR;
            _lo = _ls + _cs + (0.00134 * Math.Cos(_a9) + 0.00154 * Math.Cos(_b9) + 0.002 * Math.Cos(_c9) + 0.00179 * Math.Sin(_d9) + 0.00178 * Math.Sin(_e9) - 0.00569 - 0.00479 * Math.Sin(_an)) * DR;
            _sd = Math.Sin(_eo) * Math.Sin(_lo);
            _ds = Math.Atan(_sd / Math.Sqrt((-_sd) * _sd + 1));
            _tra2 = Math.Cos(_eo) * Math.Sin(_lo);
            _tra4 = Math.Cos(_lo);
            _ras = Math.Atan(_tra2 / _tra4);
            if (_tra2 > 0 && _tra4 < 0)
            {
                _ras += PI;
            }
            if (_tra2 < 0 && _tra4 < 0)
            {
                _ras += PI;
            }
            if (_tra2 < 0 && _tra4 > 0)
            {
                _ras += 2 * PI;
            }
            _ras *= 3.8197186342055;
            // Windows: HAS = (S5 + (HV + 12) * 1.002737908d - AJ - RAS);
            _has = (_s5 + (_hv + 12) * 1.002737908 - _aj - _ras);
            _has /= 3.8197186342055;
            _ssam = Math.Sin(_calculator.LT) * Math.Sin(_ds) + Math.Cos(_calculator.LT) * Math.Cos(_ds) * Math.Cos(_has);
            _sam = (Math.Atan(_ssam / Math.Sqrt((-_ssam) * _ssam + 1))) / DR;
            _sam += 1.75;
            _sam = Math.Floor(_sam * 10000) / 10000.0;
            _samString = FormatToString(_sam, 4);
            _outputWriter.Write("      " + _samString);
        }

        private string FormatToString(double number, int length)
        {
            // Matching Windows FormatToString (lines 1211-1279)
            string result = "";
            string tempString = "";
            int integerLength = 0;
            double tempNumber = 0;
            bool longInteger = false;
            int tempLength = 0;

            if (length < 2)
            {
                length = 2;
            }
            tempNumber = number;
            while (Math.Abs(tempNumber) > 1)
            {
                tempNumber /= 10.0;
                integerLength++;
            }
            if (integerLength >= length)
            {
                longInteger = true;
                tempLength = length;
                length = integerLength + 1;
            }
            // Calculate decimal places needed
            int decimalPlaces;
            if (Math.Abs(number) < 1)
            {
                decimalPlaces = length - integerLength - 2; // Account for leading "0."
            }
            else
            {
                decimalPlaces = length - integerLength - 1; // Account for decimal point
            }
            // Round to appropriate decimal places
            number = Math.Round(number, decimalPlaces);
            if (longInteger)
            {
                length = tempLength;
            }
            tempString = number.ToString(CultureInfo.InvariantCulture);
            if (Math.Abs(number) < 1 && number != 0)
            {
                if (number < 0)
                {
                    // For negative numbers < 1, use "-0.XX" format (no leading space)
                    tempString = "-0" + tempString.Substring(Math.Max(tempString.Length - (tempString.Length - 1), 0));
                }
                else
                {
                    // For positive numbers < 1, Windows uses "0.XX" format (no leading space in FormatToString)
                    // The column spacing ("    " before illumination) handles the spacing
                    tempString = "0" + tempString.Substring(Math.Max(tempString.Length - (tempString.Length - 1), 0));
                }
            }
            if (Math.Floor(number) == number && tempString.Length + 2 <= length + 1)
            {
                tempString = tempString + ".0";
            }
            if (Math.Floor(number) == number && (tempString.Length < 2 || tempString.Substring(Math.Max(tempString.Length - 2, 0)) != ".0"))
            {
                while (tempString.Length < length + 1)
                {
                    tempString = tempString + " ";
                }
            }
            while (tempString.Length < length + 1)
            {
                tempString = tempString + "0";
            }
            // Remove trailing zeros after decimal point (matching Windows VB.NET Conversion.Str behavior)
            // But ensure final string maintains exact target length for column alignment
            int targetLength = length + 1; // Target length including decimal point
            if (tempString.Contains(".") && tempString.Length >= targetLength)
            {
                string original = tempString;
                tempString = tempString.TrimEnd('0');
                // If we removed all zeros, restore .0 for integers, or remove decimal for non-integers
                if (tempString.EndsWith("."))
                {
                    if (Math.Floor(number) == number)
                    {
                        tempString = tempString + "0"; // Keep .0 for integers
                    }
                    else
                    {
                        tempString = tempString.Substring(0, tempString.Length - 1); // Remove trailing decimal
                    }
                }
            }
            // Always pad to exact target length for perfect column alignment
            while (tempString.Length < targetLength)
            {
                tempString = tempString + " ";
            }
            // Ensure we don't exceed the target length
            if (tempString.Length > targetLength)
            {
                tempString = tempString.Substring(0, targetLength);
            }
            result = tempString;
            return result;
        }

        private void PrintLocation(string locationName, double latitude, double longitude, double hr)
        {
            // Matching Windows PrintLocation (lines 1282-1298) - simplified for our use
            int latDeg = (int)Math.Floor(Math.Abs(latitude));
            double latMinDecimal = (Math.Abs(latitude) - latDeg) * 60;
            int latMin = (int)Math.Round(latMinDecimal, MidpointRounding.AwayFromZero);
            string latDir = latitude >= 0 ? "N" : "S";

            int longDeg = (int)Math.Floor(Math.Abs(longitude));
            double longMinDecimal = (Math.Abs(longitude) - longDeg) * 60;
            int longMin = (int)Math.Round(longMinDecimal, MidpointRounding.AwayFromZero);
            string longDir = longitude < 0 ? "E" : "W";

            // Use stored original GMT offset if available, otherwise reverse-calculate from hr
            double gmtOffset;
            if (!double.IsNaN(_originalGmtOffset))
            {
                // Use the original GMT offset that was passed in
                gmtOffset = _originalGmtOffset;
            }
            else
            {
                // Fallback: reverse-calculate from hr (may not work correctly for negative offsets)
                gmtOffset = hr - 12;
                if (longDir == "W")
                {
                    gmtOffset = 12 - hr;
                }
            }

            string gmtStr = gmtOffset >= 0 ? $"+{gmtOffset}" : gmtOffset.ToString(CultureInfo.InvariantCulture);

            _outputWriter.WriteLine($"({locationName}, {latDeg}°{latMin}'{latDir} {longDeg}°{longMin}'{longDir} GMT {gmtStr})");
            _outputWriter.WriteLine("");
        }
    }
}

