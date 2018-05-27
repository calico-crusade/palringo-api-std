using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalApi
{
    using Types;

    public static class Extensions
    {
        private static Encoding Encoding => Networking.PacketSerializer.Outbound;

        static readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0);

        static readonly DateTimeOffset epochDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// Splits a collection of data into chunks
        /// </summary>
        /// <typeparam name="T">The type of data to split</typeparam>
        /// <param name="inData">The data to split</param>
        /// <param name="chunkSize">The max length of a chunk</param>
        /// <returns>The chunked data</returns>
        public static IEnumerable<T[]> SplitChunks<T>(this IEnumerable<T> inData, int chunkSize)
        {
            var data = inData.ToArray();
            var chunk = new List<T>();
            for (var i = 0; i < data.Length; i++)
            {
                if (chunk.Count == chunkSize)
                {
                    yield return chunk.ToArray();
                    chunk = new List<T>();
                }

                chunk.Add(data[i]);
            }
            if (chunk.Count > 0)
                yield return chunk.ToArray();
        }

        /// <summary>
        /// Safely removes a chunk from a string ignoring out of bounds errors
        /// </summary>
        /// <param name="str">The string to remove from</param>
        /// <param name="start">The start index to remove from</param>
        /// <param name="length">How long the chunk to remove is</param>
        /// <returns>The data without the specified chunk</returns>
        public static string SafeRemove(this string str, int start, int length)
        {
            string o = "";
            for (var i = 0; i < str.Length; i++)
            {
                if (i >= start && i < start + length)
                    continue;
                o += str[i];
            }
            return o;
        }

        public static IEnumerable<T> FirstInstanceOf<T>(this IEnumerable<T> data, params T[] chunks)
        {
            var ar = data.ToArray();

            if (chunks.Length <= 0)
                return ar;

            int i = -1;
            while ((i = Array.IndexOf(ar, chunks[0], i + 1)) != -1)
            {
                bool worked = true;
                for(var x = 1; x < chunks.Length; x++)
                {
                    if (i + x >= ar.Length)
                    {
                        return ar;
                    }

                    if (!ar[i + x].Equals(chunks[x]))
                    {
                        worked = false;
                        break;
                    }
                }

                if (worked)
                    return ar.Take(i);
            }

            return ar;

        }

        public static IEnumerable<T> Extend<T>(this IEnumerable<T> data, params T[] items)
        {
            foreach (var item in data)
                yield return item;

            foreach (var item in items)
                yield return item;
        }

        /// <summary>
        /// Gets the palringo device code from the specific device type
        /// </summary>
        /// <param name="type">The device type</param>
        /// <returns>The palringo device code.</returns>
        public static string GetStrDevice(this DeviceType type)
        {
            switch (type)
            {
                ///////////Device //////////what you type
                case DeviceType.Android: return "android";
                case DeviceType.PC: return "Windows x86";
                case DeviceType.Mac: return "Apple/Intel";
                case DeviceType.iPad: return "Apple/iPad/Premium";
                case DeviceType.iPhone: return "Apple/iPhone/Premium";
                case DeviceType.WindowsPhone7: return "Win/P7";
                case DeviceType.Web: return "WEB";
                default:
                case DeviceType.Generic: return "Java";
            }
        }

        /// <summary>
        /// Gets all of the posible Enum Flags from a sepific enum
        /// </summary>
        /// <typeparam name="T">The type of enum</typeparam>
        /// <param name="enumFlag">The enum to get the flags of</param>
        /// <returns>A collection of all the flags in the enum</returns>
        public static T[] AllFlags<T>(this T enumFlag) where T : IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("enumFlag must be an Enum type");

            return Enum.GetValues(typeof(T)).Cast<T>().OrderBy(t => t.ToInt32(null)).ToArray();
        }

        /// <summary>
        /// From pal unix to ours
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime FromPalUnix(this string ts)
        {
            return FromPalUnix(int.Parse(ts.Split('.')[0]), int.Parse(ts.Split('.')[1]));
        }

        /// <summary>
        /// from regular unix
        /// </summary>
        /// <param name="secondsSinceepoch"></param>
        /// <returns></returns>
        public static DateTime FromUnix(this int secondsSinceepoch)
        {
            return epochStart.AddSeconds(secondsSinceepoch).ToLocalTime();
        }

        /// <summary>
        /// From pal unix in intergers
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="nanoseconds"></param>
        /// <returns></returns>
        public static DateTime FromPalUnix(int seconds, int nanoseconds)
        {
            var dt = epochStart;
            dt = epochStart.AddSeconds(seconds);
            dt = dt.AddMilliseconds(nanoseconds / 1000);
            return dt.ToLocalTime();
        }

        /// <summary>
        /// Converts our timestamps to pal timestamps
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToPalUnix(this DateTime time)
        {
            var current = time.ToUniversalTime().ToUnix();
            return current + "." + ((int)(time.Ticks % TimeSpan.TicksPerMillisecond % 10) * 100);
        }

        /// <summary>
        /// Converts an offset from unix to windows
        /// </summary>
        /// <param name="secondsSinceEpoch"></param>
        /// <param name="timeZoneOffsetInMinutes"></param>
        /// <returns></returns>
        public static DateTimeOffset FromUnix(this int secondsSinceEpoch, int timeZoneOffsetInMinutes)
        {
            var utcDateTime = epochDateTimeOffset.AddSeconds(secondsSinceEpoch);
            var offset = TimeSpan.FromMinutes(timeZoneOffsetInMinutes);
            return new DateTimeOffset(utcDateTime.DateTime.Add(offset), offset);
        }

        /// <summary>
        /// Converts our timestamp to unix
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToUnix(this DateTime dateTime)
        {
            return (int)(dateTime - epochStart).TotalSeconds;
        }

        /// <summary>
        /// Gets now in unix epoch
        /// </summary>
        public static int Now
        {
            get
            {
                return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
            }
        }

        /// <summary>
        /// Gets the palringo mimetype from the specified DataType
        /// </summary>
        /// <param name="type">The datatype</param>
        /// <returns>The palringo mimetype</returns>
        public static string FromDataType(this DataType type)
        {
            switch (type)
            {
                case DataType.Image: return "image/jpeg";
                case DataType.Text: return "text/plain";
                case DataType.VoiceMessage: return "audio/x-speex";
                default: return "text/html";
            }
        }

        /// <summary>
        /// Converts the palringo mimetype to the specified DataType
        /// </summary>
        /// <param name="type">The mimetype</param>
        /// <returns>The datatype</returns>
        public static DataType FromMimeType(this string type)
        {
            switch (type)
            {
                case "text/plain":
                    return DataType.Text;
                case "image/jpeg":
                case "text/image_link":
                    return DataType.Image;
                case "audio/x-speex":
                    return DataType.VoiceMessage;
                default:
                    return DataType.RichMessage;
            }
        }

        public static IEnumerable<TOut> Select<TIn, TOut>(this IEnumerable<TIn> data, Func<int, TIn, TOut> func)
        {
            var ar = data.ToArray();
            for(var i = 0; i < ar.Length; i++)
            {
                yield return func(i, ar[i]);
            }
        }

        private static ConsoleColor ColorFromCarat(char item)
        {
            switch (item)
            {
                case 'b': return ConsoleColor.Blue;
                case 'c': return ConsoleColor.Cyan;
                case 'g': return ConsoleColor.Gray;
                case 'e': return ConsoleColor.Green;
                case 'm': return ConsoleColor.Magenta;
                case 'r': return ConsoleColor.Red;
                case 'w': return ConsoleColor.White;
                case 'y': return ConsoleColor.Yellow;
                default: return ConsoleColor.Black;
            }
        }

        private static bool Working = false;

        /// <summary>
        /// Coloured console writer takes letters after ^ and converts into different colors.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endline"></param>
        public static void ColouredConsole(this string data, bool endline = true)
        {
            if (Working)
            {
                System.Threading.Thread.Sleep(1);
                ColouredConsole(data, endline);
                return;
            }

            Working = true;

            var start = Console.ForegroundColor;
            var parts = (data.StartsWith("^") ? data : "w" + data).Split('^');

            foreach (var part in parts)
            {
                if (part.Length <= 0)
                    continue;
                var c = part[0];
                var color = ColorFromCarat(c);

                var sub = part.Substring(1);
                Console.ForegroundColor = color;
                Console.Write(sub);
            }
            if (endline)
                Console.WriteLine();

            Console.ForegroundColor = start;
            Working = false;
        }

        /// <summary>
        /// Parses an Enum from a string
        /// </summary>
        /// <typeparam name="T">The type of enum to parse</typeparam>
        /// <param name="message">The string enum</param>
        /// <returns>The enum</returns>
        public static T ParseEnum<T>(this string message)
        {
            if (!typeof(T).IsEnum)
                return default(T);
            return (T)message.ChangeType(typeof(T));
        }

        /// <summary>
        /// Nullable safe method of converting object between types.
        /// </summary>
        /// <param name="value">The object to convert</param>
        /// <param name="type">The type to convert to</param>
        /// <returns>The converted object</returns>
        public static object ChangeType(this object value, System.Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            if (t.IsEnum)
            {
                try
                {
                    return Enum.ToObject(t, Convert.ChangeType(value, typeof(int)));
                }
                catch
                {
                    return Enum.Parse(t, (string)Convert.ChangeType(value, typeof(string)), true);
                }
            }
            return Convert.ChangeType(value, t);
        }

        public static string FromConstCase(this string data, char split = '_', string putTogether = " ")
        {
            var parts = new List<string>();
            foreach(var part in data.ToLower().Split(split))
            {
                var chrs = part.ToCharArray();
                if (chrs.Length > 1)
                    chrs[0] = chrs[0].ToString().ToUpper()[0];

                parts.Add(new string(chrs));
            }
            return string.Join(putTogether, parts);
        }
    }
}
