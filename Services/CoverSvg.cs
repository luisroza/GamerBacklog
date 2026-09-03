using System.Security;

namespace GamerBacklog.Services;

/// <summary>
/// Gera capas e avatares SVG determinísticos (initials + cor por hash) localmente,
/// sem depender de internet. Garante que nenhuma imagem quebre.
/// </summary>
public static class CoverSvg
{
    private static readonly string[][] Palettes =
    {
        new[] { "#0EA5E9", "#6366F1" },
        new[] { "#8B5CF6", "#D946EF" },
        new[] { "#14B8A6", "#0F766E" },
        new[] { "#F59E0B", "#DC2626" },
        new[] { "#10B981", "#065F46" },
        new[] { "#EF4444", "#7F1D1D" },
        new[] { "#6366F1", "#312E81" },
        new[] { "#EC4899", "#831843" },
        new[] { "#22D3EE", "#155E75" },
        new[] { "#A78BFA", "#4C1D95" },
    };

    public static string Game(string source)
    {
        var (initials, hash) = Analyze(source);
        var palette = Palettes[Math.Abs(hash) % Palettes.Length];
        var text = SecurityElement.Escape(initials) ?? "?";
        return
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"300\" height=\"450\" viewBox=\"0 0 300 450\" role=\"img\">" +
            "<defs><linearGradient id=\"g\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">" +
            $"<stop offset=\"0\" stop-color=\"{palette[0]}\"/><stop offset=\"1\" stop-color=\"{palette[1]}\"/>" +
            "</linearGradient></defs>" +
            "<rect width=\"300\" height=\"450\" fill=\"url(#g)\"/>" +
            "<circle cx=\"245\" cy=\"70\" r=\"95\" fill=\"#ffffff\" opacity=\"0.08\"/>" +
            "<circle cx=\"30\" cy=\"410\" r=\"130\" fill=\"#000000\" opacity=\"0.15\"/>" +
            "<rect x=\"0\" y=\"0\" width=\"300\" height=\"450\" fill=\"none\" stroke=\"#0F1115\" stroke-opacity=\"0.25\" stroke-width=\"6\"/>" +
            $"<text x=\"150\" y=\"248\" font-family=\"Outfit, Segoe UI, sans-serif\" font-size=\"110\" font-weight=\"800\" fill=\"#ffffff\" fill-opacity=\"0.92\" text-anchor=\"middle\">{text}</text>" +
            "</svg>";
    }

    public static string Avatar(string source)
    {
        var (initials, hash) = Analyze(source);
        var palette = Palettes[Math.Abs(hash) % Palettes.Length];
        var text = SecurityElement.Escape(initials) ?? "?";
        return
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"128\" height=\"128\" viewBox=\"0 0 128 128\" role=\"img\">" +
            "<defs><linearGradient id=\"g\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">" +
            $"<stop offset=\"0\" stop-color=\"{palette[0]}\"/><stop offset=\"1\" stop-color=\"{palette[1]}\"/>" +
            "</linearGradient></defs>" +
            "<defs><clipPath id=\"c\"><circle cx=\"64\" cy=\"64\" r=\"64\"/></clipPath></defs>" +
            "<g clip-path=\"url(#c)\">" +
            "<rect width=\"128\" height=\"128\" fill=\"url(#g)\"/>" +
            "<circle cx=\"100\" cy=\"20\" r=\"40\" fill=\"#ffffff\" opacity=\"0.1\"/>" +
            $"<text x=\"64\" y=\"82\" font-family=\"Outfit, Segoe UI, sans-serif\" font-size=\"48\" font-weight=\"800\" fill=\"#ffffff\" fill-opacity=\"0.95\" text-anchor=\"middle\">{text}</text>" +
            "</g></svg>";
    }

    private static (string Initials, int Hash) Analyze(string source)
    {
        var text = (source ?? "").Trim();
        if (text.Length == 0) return ("?", 0);

        var words = text.Split(new[] { ' ', '-', ':', ',', '\'', '.', '’' }, StringSplitOptions.RemoveEmptyEntries);
        var initials = "";
        if (words.Length >= 2)
        {
            initials = string.Concat(char.ToUpperInvariant(words[0][0]), char.ToUpperInvariant(words[1][0]));
        }
        else
        {
            initials = text.Length >= 2 ? text.Substring(0, 2).ToUpperInvariant() : text.ToUpperInvariant();
        }

        unchecked
        {
            var hash = 17;
            foreach (var c in text)
            {
                hash = hash * 31 + c;
            }
            return (initials, hash);
        }
    }
}
