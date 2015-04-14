using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TradeSharp.Util;

namespace NewsRobot
{
    public class RobotNews : IEquatable<RobotNews>
    {
        public DateTime Time { get; set; }
        public string Title { get; set; }
        public bool Valuable;
        public string CountryCode { get; set; }
        // value projected in the past
        public double ProjectedValue { get; set; }
        // current value
        public double Value { get; set; }

        public bool Equals(RobotNews other)
        {
            if (Time != other.Time)
                return false;
            if (Title != other.Title)
                return false;
            if (Valuable != other.Valuable)
                return false;
            if (CountryCode != other.CountryCode)
                return false;
            if (ProjectedValue != other.ProjectedValue)
                return false;
            if (Value != other.Value)
                return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}] {2}: proj = {3} curr = {4}", Time, CountryCode, Title, ProjectedValue, Value);
        }

        public static void SaveToFile(string fileName, List<RobotNews> newsList)
        {
            using (var sr = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                const string sep = "#;";
                foreach (var news in newsList)
                {
                    sr.WriteLine(news.Time.ToString("o") + sep + news.CountryCode + sep + news.Title +
                        sep + (news.Valuable ? "1" : "0") + sep + news.ProjectedValue + sep + news.Value);
                }
            }
        }

        public static List<RobotNews> LoadFromFile(string fileName)
        {
            var result = new List<RobotNews>();
            try
            {
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    const string sep = "#;";
                    while (sr.Peek() >= 0)
                    {
                        var str = sr.ReadLine();
                        var strs = str.Split(new[] { sep }, StringSplitOptions.None);
                        if (strs.Length != 6)
                            continue;
                        var news = new RobotNews();
                        try
                        {
                            news.Time = DateTime.Parse(strs[0]);
                            news.CountryCode = strs[1];
                            news.Title = strs[2];
                            news.Valuable = strs[3] == "1";
                            news.ProjectedValue = strs[4].ToDoubleUniform();
                            news.Value = strs[5].ToDoubleUniform();
                        }
                        catch
                        {
                            Console.WriteLine("RobotNews::LoadFromFile: file format error at {0}", sr.Peek());
                        }
                        result.Add(news);
                    }
                }
            }
            catch
            {
                Console.WriteLine("RobotNews::LoadFromFile: can't read file {0}", fileName);
            }
            return result;
        }
    }
}
