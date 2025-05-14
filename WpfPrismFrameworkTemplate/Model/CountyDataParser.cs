using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfPrismFrameworkTemplate.Model
{
    public class CountyDataParser
    {
        public List<County> Parse(string rawData)
        {
            var counties = new List<County>();

            // Regular expressions to identify county blocks and holder entries
            var countyPattern = new Regex(@"c_([a-zA-Z]+)\s*=\s*{([^}]+)}", RegexOptions.Singleline);
            var holderPattern = new Regex(@"(\d+)\.(\d+)\.(\d+)\s*=\s*{\s*holder\s*=\s*([a-zA-Z_]+)_(\d+)\s*}", RegexOptions.Singleline);

            // Find all county blocks
            var countyMatches = countyPattern.Matches(rawData);

            foreach (Match countyMatch in countyMatches)
            {
                var countyName = countyMatch.Groups[1].Value;
                var countyContent = countyMatch.Groups[2].Value;

                var county = new County { Name = countyName };

                // Find all holder entries within this county
                var holderMatches = holderPattern.Matches(countyContent);
                foreach (Match holderMatch in holderMatches)
                {
                    int year = int.Parse(holderMatch.Groups[1].Value);
                    int month = int.Parse(holderMatch.Groups[2].Value);
                    int day = int.Parse(holderMatch.Groups[3].Value);
                    string familyName = holderMatch.Groups[4].Value;
                    string holderId = holderMatch.Groups[5].Value;

                    county.HolderPeriods.Add(new HolderPeriod
                    {
                        StartDate = new DateTime(year, month, day),
                        HolderName = $"{familyName}_{holderId}"
                    });
                }

                counties.Add(county);
            }

            return counties;
        }

        public List<County> ParseFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("County data file not found", filePath);

            string content = File.ReadAllText(filePath);
            return Parse(content);
        }
    }
}
