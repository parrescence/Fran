namespace Fran.Components;

/// <summary>
/// The set of source languages <see cref="FaCodeBlock"/> knows how to label and
/// lightly syntax-highlight. Not an exhaustive parser for any of these — see
/// <c>Rendering/CodeHighlighter.cs</c> — just enough regex-driven token
/// classification (comments/strings/numbers/keywords, or tags/attributes for the
/// markup languages) that a snippet reads correctly at a glance without pulling in
/// a third-party highlighting library.
/// </summary>
public enum FaCodeLanguage
{
    PlainText,
    CSharp,
    Razor,
    JavaScript,
    TypeScript,
    Html,
    Xml,
    Css,
    Scss,
    Json,
    Bash,
    PowerShell,
    Sql,
    Yaml,
    Markdown
}
