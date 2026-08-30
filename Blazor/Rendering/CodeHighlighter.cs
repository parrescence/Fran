using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FaFa.Components;

namespace FaFa.Rendering;

/// <summary>
/// Regex-driven, single-pass token classification backing <see cref="FaCodeBlock"/> —
/// deliberately not a real parser or a third-party highlighting library (Prism/
/// highlight.js/...), matching this package's "no external JS dependency" stance
/// (see Blazor/CLAUDE.md). Each language gets one combined regex with named capture
/// groups (comment/string/number/keyword, or tag/attr for markup, or property/literal
/// for JSON); <see cref="Highlight"/> walks the matches once, HTML-encoding both the
/// matched tokens and the untouched text between them, and wraps a matched token in a
/// <c>&lt;span class="fa-code-tok-{group}"&gt;</c> when a named group actually
/// succeeded. Good enough for a snippet to read correctly at a glance — not a claim
/// of 100% correctness for any one language's full grammar.
/// </summary>
internal static class CodeHighlighter
{
    private static readonly string[] TokenGroupNames =
        ["comment", "string", "number", "keyword", "tag", "attr", "literal", "property"];

    public static (string Slug, string Label) Describe(FaCodeLanguage language) => language switch
    {
        FaCodeLanguage.CSharp => ("csharp", "C#"),
        FaCodeLanguage.Razor => ("razor", "Razor"),
        FaCodeLanguage.JavaScript => ("javascript", "JavaScript"),
        FaCodeLanguage.TypeScript => ("typescript", "TypeScript"),
        FaCodeLanguage.Html => ("html", "HTML"),
        FaCodeLanguage.Xml => ("xml", "XML"),
        FaCodeLanguage.Css => ("css", "CSS"),
        FaCodeLanguage.Scss => ("scss", "SCSS"),
        FaCodeLanguage.Json => ("json", "JSON"),
        FaCodeLanguage.Bash => ("bash", "Bash"),
        FaCodeLanguage.PowerShell => ("powershell", "PowerShell"),
        FaCodeLanguage.Sql => ("sql", "SQL"),
        FaCodeLanguage.Yaml => ("yaml", "YAML"),
        FaCodeLanguage.Markdown => ("markdown", "Markdown"),
        _ => ("plaintext", "Plain Text")
    };

    /// <summary>
    /// Returns the code as highlighted HTML (token spans + HTML-encoded text),
    /// ready to render via <see cref="Microsoft.AspNetCore.Components.MarkupString"/>.
    /// Every character of <paramref name="code"/> is HTML-encoded somewhere in the
    /// output — matched tokens and the untouched gaps between them alike — so this
    /// is safe against a snippet that itself contains <c>&lt;</c>/<c>&amp;</c>.
    /// </summary>
    public static string Highlight(string code, FaCodeLanguage language)
    {
        var regex = GetRegex(language);
        if (regex is null)
        {
            return WebUtility.HtmlEncode(code);
        }

        var sb = new StringBuilder(code.Length + 64);
        var lastIndex = 0;
        foreach (Match m in regex.Matches(code))
        {
            sb.Append(WebUtility.HtmlEncode(code[lastIndex..m.Index]));
            var tokenClass = ClassifyMatch(m);
            var encoded = WebUtility.HtmlEncode(m.Value);
            if (tokenClass is null)
            {
                sb.Append(encoded);
            }
            else
            {
                sb.Append("<span class=\"fa-code-tok-").Append(tokenClass).Append("\">").Append(encoded).Append("</span>");
            }
            lastIndex = m.Index + m.Length;
        }
        sb.Append(WebUtility.HtmlEncode(code[lastIndex..]));
        return sb.ToString();
    }

    private static string? ClassifyMatch(Match m)
    {
        foreach (var name in TokenGroupNames)
        {
            if (m.Groups[name].Success)
            {
                return name;
            }
        }
        return null;
    }

    // Built once, reused for every FaCodeBlock instance/render of that language.
    private static Regex? GetRegex(FaCodeLanguage language) => language switch
    {
        FaCodeLanguage.CSharp => CSharpRegex,
        FaCodeLanguage.Razor or FaCodeLanguage.Html or FaCodeLanguage.Xml => MarkupRegex,
        FaCodeLanguage.JavaScript => JavaScriptRegex,
        FaCodeLanguage.TypeScript => TypeScriptRegex,
        FaCodeLanguage.Css => CssRegex,
        FaCodeLanguage.Scss => ScssRegex,
        FaCodeLanguage.Json => JsonRegex,
        FaCodeLanguage.Bash => BashRegex,
        FaCodeLanguage.PowerShell => PowerShellRegex,
        FaCodeLanguage.Sql => SqlRegex,
        FaCodeLanguage.Yaml => YamlRegex,
        _ => null // PlainText, Markdown — rendered as-is, no token spans
    };

    private static Regex BuildCLike(string[] lineCommentPrefixes, bool blockComment, string[] keywords, bool ignoreCase = false)
    {
        var alts = new List<string>();

        var commentAlts = new List<string>();
        if (blockComment)
        {
            commentAlts.Add(@"/\*[\s\S]*?\*/");
        }
        foreach (var prefix in lineCommentPrefixes)
        {
            commentAlts.Add(Regex.Escape(prefix) + ".*?$");
        }
        if (commentAlts.Count > 0)
        {
            alts.Add($"(?<comment>{string.Join("|", commentAlts)})");
        }

        alts.Add(@"(?<string>""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')");
        alts.Add(@"(?<number>\b\d+(?:\.\d+)?\b)");

        if (keywords.Length > 0)
        {
            alts.Add($@"(?<keyword>\b(?:{string.Join("|", keywords)})\b)");
        }

        var options = RegexOptions.Multiline;
        if (ignoreCase)
        {
            options |= RegexOptions.IgnoreCase;
        }
        return new Regex(string.Join("|", alts), options);
    }

    private static readonly string[] CSharpKeywords =
    [
        "using", "namespace", "class", "record", "struct", "interface", "enum",
        "public", "private", "protected", "internal", "static", "sealed", "abstract",
        "virtual", "override", "readonly", "const", "required", "init", "partial",
        "void", "string", "int", "long", "double", "float", "decimal", "bool", "var",
        "new", "return", "if", "else", "for", "foreach", "while", "do", "async",
        "await", "true", "false", "null", "this", "base", "get", "set", "try",
        "catch", "finally", "throw", "switch", "case", "default", "break", "continue",
        "yield", "in", "is", "as", "out", "ref"
    ];

    private static readonly string[] JavaScriptKeywords =
    [
        "function", "return", "const", "let", "var", "if", "else", "for", "while",
        "do", "class", "extends", "new", "this", "true", "false", "null",
        "undefined", "async", "await", "import", "export", "default", "from",
        "typeof", "instanceof", "switch", "case", "break", "continue", "try",
        "catch", "finally", "throw", "yield", "of", "in"
    ];

    private static readonly string[] TypeScriptKeywords =
    [
        .. JavaScriptKeywords,
        "interface", "type", "implements", "public", "private", "protected",
        "readonly", "enum", "namespace", "declare", "abstract", "is", "keyof", "as"
    ];

    private static readonly string[] BashKeywords =
    [
        "if", "then", "else", "elif", "fi", "for", "do", "done", "while",
        "function", "echo", "export", "return", "case", "esac", "in", "local",
        "exit", "break", "continue", "select", "until"
    ];

    private static readonly string[] PowerShellKeywords =
    [
        "function", "param", "if", "else", "elseif", "foreach", "while", "return",
        "try", "catch", "finally", "throw", "switch", "begin", "process", "end",
        "break", "continue", "do", "until"
    ];

    private static readonly string[] SqlKeywords =
    [
        "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER",
        "ON", "GROUP", "BY", "ORDER", "HAVING", "INSERT", "INTO", "VALUES", "UPDATE",
        "SET", "DELETE", "CREATE", "TABLE", "ALTER", "DROP", "AND", "OR", "NOT",
        "NULL", "AS", "DISTINCT", "LIMIT", "UNION", "ALL", "EXISTS", "IN", "BETWEEN",
        "LIKE", "CASE", "WHEN", "THEN", "END", "IS"
    ];

    private static readonly string[] YamlKeywords = ["true", "false", "null", "yes", "no"];

    private static readonly Regex CSharpRegex = BuildCLike(["//"], true, CSharpKeywords);
    private static readonly Regex JavaScriptRegex = BuildCLike(["//"], true, JavaScriptKeywords);
    private static readonly Regex TypeScriptRegex = BuildCLike(["//"], true, TypeScriptKeywords);
    private static readonly Regex CssRegex = BuildCLike([], true, []);
    private static readonly Regex ScssRegex = BuildCLike(["//"], true, []);
    private static readonly Regex BashRegex = BuildCLike(["#"], false, BashKeywords);
    private static readonly Regex PowerShellRegex = BuildCLike(["#"], false, PowerShellKeywords);
    private static readonly Regex SqlRegex = BuildCLike(["--"], true, SqlKeywords, ignoreCase: true);
    private static readonly Regex YamlRegex = BuildCLike(["#"], false, YamlKeywords);

    // Markup languages (Html/Xml, and Razor best-effort) get their own shape —
    // tags and attribute names instead of keywords.
    private static readonly Regex MarkupRegex = new(
        """(?<comment><!--[\s\S]*?-->)|(?<tag></?[A-Za-z][\w:.-]*)|(?<attr>(?<=\s)[A-Za-z:-][\w:-]*(?==))|(?<string>"[^"]*"|'[^']*')""",
        RegexOptions.None);

    // JSON has no comments and no bare keywords — object keys ("property") are
    // distinguished from ordinary string values by the trailing ':' lookahead, and
    // true/false/null are "literal" rather than "keyword" (JSON isn't a programming
    // language with control-flow keywords).
    private static readonly Regex JsonRegex = new(
        """(?<property>"(?:\\.|[^"\\])*"(?=\s*:))|(?<string>"(?:\\.|[^"\\])*")|(?<number>-?\b\d+(?:\.\d+)?(?:[eE][+-]?\d+)?\b)|(?<literal>\btrue\b|\bfalse\b|\bnull\b)""",
        RegexOptions.None);
}
