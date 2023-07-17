using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public class NumbersUtils
    {
        /// <summary>
        /// Определяет одинаковые ли знаки (+ или -) у данных чисел
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool AreNumbersNotTheSameBySign(double value1, double value2)
        {
            if (value1 < 0 && value2 > 0)
                return false;
            else if (value1 > 0 && value2 < 0)
                return false;

            return true;
        }

        public static double FindNearestValue(List<double> values, double targetNumber)
        {
            var nearest = values.OrderBy(value => Math.Abs(value - targetNumber)).First();
            return nearest;
        }

        public static double ParseStringToDouble(string stringToParse)
        {
            double value = 0;
            if (stringToParse == string.Empty || stringToParse == null)
            {
                return value;
            }
            stringToParse = stringToParse.Replace(',', '.');
            if (double.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }


        public static List<double> ParseStringToDoubleList(List<string> stringsToParse)
        {
            var parsedNumbers = stringsToParse.Select(s => ParseStringToDouble(s)).ToList();
            return parsedNumbers;
        }

        public static int ParseStringToInt(string stringToParse)
        {
            int value = 0;
            if (string.IsNullOrEmpty(stringToParse))
            {
                return 0;
            }

            stringToParse = stringToParse.Replace(',', '.');
            if (int.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public static long ParseStringToLong(string stringToParse)
        {
            long value = 0;
            stringToParse = stringToParse.Replace(',', '.');
            if (long.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public static bool IsInRange(double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(double value, double min, double max, bool isInclude)
        {
            if (isInclude)
            {
                return value >= min && value <= max;
            }
            else
            {
                return value > min && value < max;
            }
        }

        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(int value, int min, int max, bool isInclude)
        {
            if (isInclude)
            {
                return value >= min && value <= max;
            }
            else
            {
                return value > min && value < max;
            }
        }

        /// <summary>
        /// Возвращает ближайшее к целевому значению число.
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="targetNumber"></param>
        /// <returns></returns>
        public static int ClosestToTargetNumber(List<int> numbers, int targetNumber)
        {
            int closest = numbers.OrderBy(number => Math.Abs(targetNumber - number)).First();
            return closest;
        }


        public static double ClosestToTargetNumber(List<double> numbers, double targetNumber)
        {
            double closest = numbers.OrderBy(number => Math.Abs(targetNumber - number)).First();
            return closest;
        }

        /// <summary>
        /// Берётся следующее большее значение из листа
        /// Например, для 180 - берём 200, а для 210 - 300
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="targetNumber"></param>
        /// <returns></returns>
        public static double GetNextBiggestValue(
            List<double> numbers,
            double targetNumber)
        {
            var orderedNumbers = numbers.OrderBy(n => n);
            double nextBiggestValue = 0;
            foreach (double number in orderedNumbers)
            {
                if (number > targetNumber)
                {
                    nextBiggestValue = number;
                    break;
                }
            }
            return nextBiggestValue;
        }

        public static Double RoundUpToNearest(Double passednumber, Double roundto)
        {
            // 105.5 up to nearest 1 = 106
            // 105.5 up to nearest 10 = 110
            // 105.5 up to nearest 7 = 112
            // 105.5 up to nearest 100 = 200
            // 105.5 up to nearest 0.2 = 105.6
            // 105.5 up to nearest 0.3 = 105.6

            //if no rounto then just pass original number back
            if (roundto == 0)
            {
                return passednumber;
            }
            else
            {
                return Math.Ceiling(passednumber / roundto) * roundto;
            }
        }

        public static Double RoundDownToNearest(Double passednumber, Double roundto)
        {
            // 105.5 down to nearest 1 = 105
            // 105.5 down to nearest 10 = 100
            // 105.5 down to nearest 7 = 105
            // 105.5 down to nearest 100 = 100
            // 105.5 down to nearest 0.2 = 105.4
            // 105.5 down to nearest 0.3 = 105.3

            //if no rounto then just pass original number back
            if (roundto == 0)
            {
                return passednumber;
            }
            else
            {
                return Math.Floor(passednumber / roundto) * roundto;
            }
        }

        /// <summary>
        /// Возвращает список значений, которые получаются путём деления числа на другое число
        /// Например, при делении 100 на 30, получаюятся значения 30,30,30,10
        /// </summary>
        /// <param name="numberToSplit">Число для деления</param>
        /// <param name="partValue">Число, которое делит</param>
        /// <returns></returns>
        public static List<double> SplitNumberToParts(double numberToSplit, double partValue)
        {
            var splitCount = (int)(numberToSplit / partValue);
            var splitAddition = numberToSplit % partValue;

            var numbersList = new List<double>();

            for (int i = 0; i < splitCount; i++)
            {
                numbersList.Add(partValue);
            }
            numbersList.Add(splitAddition);

            return numbersList;
        }

        public static List<double> SplitNumberToStepParts(double numberToSplit, double partValue)
        {
            var splitCount = (int)(numberToSplit / partValue);
            var splitAddition = numberToSplit % partValue;

            var numbersList = new List<double>();

            double steppedValue = 0;
            numbersList.Add(steppedValue);
            for (int i = 0; i < splitCount; i++)
            {
                steppedValue += partValue;
                numbersList.Add(steppedValue);
            }
            numbersList.Add(steppedValue + splitAddition);

            return numbersList;
        }

        /// <summary>
        /// Возвращает значение sourceValue делимое на sourceValue
        /// К примеру, если sourceValue == 75.9, то вернётся значение 75
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="separatorValue"></param>
        /// <returns></returns>
        public static double RoundToNearestSeparator(double sourceValue, int separatorValue)
        {
            return Math.Round(sourceValue / separatorValue) * separatorValue;
        }

        public static string ConvertDecimalDegreesToBearing(double startAngle, int accurracy)
        {
            startAngle = Math.Round(startAngle, accurracy);
            if (startAngle >= 90 && startAngle <= 180)
            {
                startAngle = 180 - startAngle;
            }
            else if (startAngle >= 180 && startAngle <= 270)
            {
                startAngle = startAngle - 180;
            }
            else if (startAngle >= 270 && startAngle <= 360)
            {
                startAngle = 360 - startAngle;
            }

            var startAngleDegrees = (int)startAngle;
            var hoursAdd = startAngle - startAngleDegrees;
            var minutes = (int)TimeSpan.FromHours(hoursAdd).TotalMinutes;
            var minutesAdd = TimeSpan.FromHours(hoursAdd).TotalMinutes - minutes;

            var seconds = (int)TimeSpan.FromMinutes(minutesAdd).TotalSeconds;
            var secondsAdd = TimeSpan.FromMinutes(minutesAdd).TotalSeconds - seconds;

            var milliseconds = Math.Round(TimeSpan.FromSeconds(secondsAdd).TotalMilliseconds);
            //return $@"{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}.{milliseconds.ToString().PadLeft(2, '0')}''";
            return $@"{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}'";
        }

        public static string ConvertDecimalDegreesToBearing(double startAngle)
        {
            //if (startAngle >= 90 && startAngle <= 180)
            //{
            //    startAngle = 180 - startAngle;
            //}
            //else if (startAngle >= 180 && startAngle <= 270)
            //{
            //    startAngle = startAngle - 180;
            //}
            //else if (startAngle >= 270 && startAngle <= 360)
            //{
            //    startAngle = 360 - startAngle;
            //}

            var startAngleDegrees = (int)startAngle;
            var hoursAdd = startAngle - startAngleDegrees;
            var minutes = (int)TimeSpan.FromHours(hoursAdd).TotalMinutes;
            //Если минут 60, то добавляем градус

            double minutesAdd = 0;
            if (minutes == 60)
            {
                minutes = 0;
                startAngleDegrees++;
            }
            else
            {
                minutesAdd = TimeSpan.FromHours(hoursAdd).TotalMinutes - minutes;
            }

            var seconds = Math.Round(TimeSpan.FromMinutes(minutesAdd).TotalSeconds);

            double secondsAdd = 0;
            if (seconds == 60)
            {
                seconds = 0;
                minutes++;
                if (minutes == 60)
                {
                    minutes = 0;
                    startAngleDegrees++;
                }
            }
            else
            {
                secondsAdd = TimeSpan.FromMinutes(minutesAdd).TotalSeconds - seconds;
            }

            var milliseconds = Math.Round(TimeSpan.FromSeconds(secondsAdd).TotalMilliseconds);
            //return $@"{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}.{milliseconds.ToString().PadLeft(2, '0')}''";
            return $@"{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}''";
        }

        public static string ConvertDecimalDegreesToBearingDegreesAndMinutes(double startAngle)
        {
            var startAngleDegrees = (int)startAngle;
            var hoursAdd = startAngle - startAngleDegrees;
            var minutes = (int)TimeSpan.FromHours(hoursAdd).TotalMinutes;
            //Если минут 60, то добавляем градус

            double minutesAdd = 0;
            if (minutes == 60)
            {
                minutes = 0;
                startAngleDegrees++;
            }
            else
            {
                minutesAdd = TimeSpan.FromHours(hoursAdd).TotalMinutes - minutes;
            }

            var seconds = Math.Round(TimeSpan.FromMinutes(minutesAdd).TotalSeconds);

            double secondsAdd = 0;
            if (seconds == 60)
            {
                seconds = 0;
                minutes++;
                if (minutes == 60)
                {
                    minutes = 0;
                    startAngleDegrees++;
                }
            }
            else
            {
                secondsAdd = TimeSpan.FromMinutes(minutesAdd).TotalSeconds - seconds;
            }

            //return $@"{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}.{milliseconds.ToString().PadLeft(2, '0')}''";
            return $@"{startAngleDegrees}°{minutes}'";
        }

        public static string ConvertDirectionAngleDegreesToBearing(double angle)
        {
            string bearingSymbol = string.Empty;
            if (angle >= 0 && angle <= 90)
            {
                bearingSymbol = "СВ";
            }
            else if (angle >= 90 && angle <= 180)
            {
                bearingSymbol = "ЮВ";
                angle = 180 - angle;
            }
            else if (angle >= 180 && angle <= 270)
            {
                bearingSymbol = "ЮЗ";
                angle = angle - 180;
            }
            else if (angle >= 270 && angle <= 360)
            {
                bearingSymbol = "СЗ";
                angle = 360 - angle;
            }

            var startAngleDegrees = (int)angle;
            var hoursAdd = angle - startAngleDegrees;
            var minutes = (int)TimeSpan.FromHours(hoursAdd).TotalMinutes;
            //Если минут 60, то добавляем градус
            //Если минут 60, то добавляем градус

            double minutesAdd = 0;
            if (minutes == 60)
            {
                minutes = 0;
                startAngleDegrees++;
            }
            else
            {
                minutesAdd = TimeSpan.FromHours(hoursAdd).TotalMinutes - minutes;
            }

            var seconds = Math.Round(TimeSpan.FromMinutes(minutesAdd).TotalSeconds);

            double secondsAdd = 0;
            if (seconds == 60)
            {
                seconds = 0;
                minutes++;
                if (minutes == 60)
                {
                    minutes = 0;
                    startAngleDegrees++;
                }
            }
            else
            {
                secondsAdd = TimeSpan.FromMinutes(minutesAdd).TotalSeconds - seconds;
            }

            var milliseconds = Math.Round(TimeSpan.FromSeconds(secondsAdd).TotalMilliseconds);
            //return $@"{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}.{milliseconds.ToString().PadLeft(2, '0')}''";
            return $@"{bearingSymbol}:{startAngleDegrees}°{minutes}'{seconds.ToString().PadLeft(2, '0')}''";

        }
    }
}
