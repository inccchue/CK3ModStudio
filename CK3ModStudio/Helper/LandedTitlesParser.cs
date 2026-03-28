using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class LandedTitlesParser
    {
        public List<LandedTitle> ParseFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            string raw;
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                raw = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            else
                raw = Encoding.UTF8.GetString(bytes);
            return ParseContent(raw);
        }

        public List<LandedTitle> ParseContent(string content)
        {
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");
            var tokens = Tokenize(content);
            var roots = new List<LandedTitle>();
            int pos = 0;
            while (pos < tokens.Count)
            {
                if (IsTitleKey(tokens[pos]))
                {
                    var title = ParseTitle(tokens, ref pos, null);
                    if (title != null) roots.Add(title);
                }
                else
                {
                    pos++;
                }
            }
            return roots;
        }

        private LandedTitle ParseTitle(List<string> tokens, ref int pos, LandedTitle parent)
        {
            if (pos >= tokens.Count || !IsTitleKey(tokens[pos])) return null;

            var key = tokens[pos++];

            if (pos >= tokens.Count || tokens[pos] != "=") return null;
            pos++;

            if (pos >= tokens.Count || tokens[pos] != "{") return null;
            pos++;

            var title = new LandedTitle
            {
                Key = key,
                Rank = GetRank(key),
                Parent = parent
            };

            while (pos < tokens.Count && tokens[pos] != "}")
            {
                var tok = tokens[pos];

                if (tok == "color" && LookAhead(tokens, pos) == "=")
                {
                    pos += 2;
                    title.Color = ParseColorBlock(tokens, ref pos);
                }
                else if (tok == "capital" && LookAhead(tokens, pos) == "=")
                {
                    pos += 2;
                    if (pos < tokens.Count) title.Capital = tokens[pos++];
                }
                else if (tok == "province" && LookAhead(tokens, pos) == "=")
                {
                    pos += 2;
                    int prov;
                    if (pos < tokens.Count && int.TryParse(tokens[pos], out prov))
                    {
                        title.Province = prov;
                        pos++;
                    }
                }
                else if (tok == "definite_form" && LookAhead(tokens, pos) == "=")
                {
                    pos += 2;
                    if (pos < tokens.Count) { title.DefiniteForm = tokens[pos] == "yes"; pos++; }
                }
                else if (IsTitleKey(tok) && LookAhead(tokens, pos) == "=")
                {
                    var child = ParseTitle(tokens, ref pos, title);
                    if (child != null) title.Children.Add(child);
                }
                else if (LookAhead(tokens, pos) == "=")
                {
                    pos += 2;
                    if (pos < tokens.Count && tokens[pos] == "{")
                        SkipBlock(tokens, ref pos);
                    else if (pos < tokens.Count)
                        pos++;
                }
                else
                {
                    pos++;
                }
            }

            if (pos < tokens.Count) pos++;
            return title;
        }

        private string LookAhead(List<string> tokens, int pos)
            => (pos + 1 < tokens.Count) ? tokens[pos + 1] : "";

        private void SkipBlock(List<string> tokens, ref int pos)
        {
            if (pos >= tokens.Count || tokens[pos] != "{") return;
            int depth = 0;
            while (pos < tokens.Count)
            {
                var t = tokens[pos++];
                if (t == "{") depth++;
                else if (t == "}") { depth--; if (depth == 0) break; }
            }
        }

        private Color ParseColorBlock(List<string> tokens, ref int pos)
        {
            if (pos >= tokens.Count || tokens[pos] != "{") return Colors.Gray;
            pos++;
            byte r = 128, g = 128, b = 128;
            if (pos < tokens.Count && byte.TryParse(tokens[pos], out r)) pos++;
            if (pos < tokens.Count && byte.TryParse(tokens[pos], out g)) pos++;
            if (pos < tokens.Count && byte.TryParse(tokens[pos], out b)) pos++;
            while (pos < tokens.Count && tokens[pos] != "}") pos++;
            if (pos < tokens.Count) pos++;
            return Color.FromRgb(r, g, b);
        }

        private bool IsTitleKey(string key)
            => key.Length > 2 &&
               (key.StartsWith("e_") || key.StartsWith("k_") ||
                key.StartsWith("d_") || key.StartsWith("c_") ||
                key.StartsWith("b_"));

        private TitleRank GetRank(string key)
        {
            switch (key[0])
            {
                case 'e': return TitleRank.Empire;
                case 'k': return TitleRank.Kingdom;
                case 'd': return TitleRank.Duchy;
                case 'c': return TitleRank.County;
                default: return TitleRank.Barony;
            }
        }

        private List<string> Tokenize(string content)
        {
            var tokens = new List<string>(content.Length / 6);
            int i = 0;
            int len = content.Length;

            while (i < len)
            {
                char c = content[i];

                if (c == '#')
                {
                    while (i < len && content[i] != '\n') i++;
                    continue;
                }

                if (c <= ' ') { i++; continue; }

                if (c == '{' || c == '}')
                {
                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                if (c == '=' || c == '>' || c == '<' || c == '!')
                {
                    var sb2 = new StringBuilder();
                    sb2.Append(c); i++;
                    if (i < len && content[i] == '=') { sb2.Append('='); i++; }
                    tokens.Add(sb2.ToString());
                    continue;
                }

                if (c == '"')
                {
                    i++;
                    var sb = new StringBuilder();
                    while (i < len && content[i] != '"') sb.Append(content[i++]);
                    if (i < len) i++;
                    tokens.Add(sb.ToString());
                    continue;
                }

                {
                    var sb = new StringBuilder();
                    while (i < len && content[i] > ' ' &&
                           content[i] != '{' && content[i] != '}' &&
                           content[i] != '=' && content[i] != '>' &&
                           content[i] != '<' && content[i] != '!' &&
                           content[i] != '#' && content[i] != '"')
                    {
                        sb.Append(content[i++]);
                    }
                    if (sb.Length > 0) tokens.Add(sb.ToString());
                }
            }
            return tokens;
        }

        public string Serialize(IEnumerable<LandedTitle> roots, int indent = 0)
        {
            var sb = new StringBuilder();
            foreach (var t in roots) SerializeTitle(t, sb, indent);
            return sb.ToString();
        }

        private void SerializeTitle(LandedTitle title, StringBuilder sb, int indent)
        {
            var pad = new string('\t', indent);
            sb.AppendLine(string.Format("{0}{1} = {{", pad, title.Key));
            var c = title.Color;
            sb.AppendLine(string.Format("{0}\tcolor = {{ {1} {2} {3} }}", pad, c.R, c.G, c.B));
            if (!string.IsNullOrEmpty(title.Capital))
                sb.AppendLine(string.Format("{0}\tcapital = {1}", pad, title.Capital));
            if (title.Province.HasValue)
                sb.AppendLine(string.Format("{0}\tprovince = {1}", pad, title.Province));
            if (title.DefiniteForm)
                sb.AppendLine(string.Format("{0}\tdefinite_form = yes", pad));
            if (!string.IsNullOrEmpty(title.CanCreate))
                sb.AppendLine(string.Format("{0}\tcan_create = {1}", pad, title.CanCreate));
            foreach (var child in title.Children)
                SerializeTitle(child, sb, indent + 1);
            sb.AppendLine(string.Format("{0}}}", pad));
        }
    }
}
