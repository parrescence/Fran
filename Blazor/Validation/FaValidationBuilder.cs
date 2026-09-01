namespace FaFa.Validation;

/// <summary>
/// The rule set an <see cref="IFaValidator{TModel}"/> (root tier) declares against,
/// and that a form's own <c>ConfigureValidation</c> delegate (form tier — see
/// <see cref="Components.FaModelValidator{TModel}"/>) can add to, replace, or remove
/// from afterward. "Override" here just means later code runs later and can call
/// <see cref="RemoveField"/>/<see cref="Field{TValue}"/> again on the same property —
/// there's no separate merge algorithm to reason about, the whole builder is a plain
/// mutable dictionary keyed by property name.
/// </summary>
public sealed class FaValidationBuilder<TModel> where TModel : class
{
    private readonly Dictionary<string, List<Func<TModel, string?>>> _rules = new();

    /// <summary>
    /// Starts (or resumes — calling this twice for the same <paramref name="propertyName"/>
    /// adds to the same rule list rather than replacing it; use <see cref="RemoveField"/>
    /// first if a form-tier override needs to start clean) declaring rules for one
    /// property. <paramref name="accessor"/> is a plain delegate (<c>m =&gt; m.Price</c>),
    /// not an <c>Expression&lt;Func&lt;...&gt;&gt;</c> — FaFa's validation system
    /// deliberately doesn't parse property-selector expression trees the way
    /// FluentValidation's <c>RuleFor</c> does (a large, separate undertaking this
    /// library isn't taking on); <paramref name="propertyName"/> is a plain string
    /// (pass <c>nameof(TModel.Price)</c>) for the same reason, and because that's
    /// exactly what <c>FieldIdentifier</c>/<c>ValidationMessageStore</c> already key
    /// validation messages on.
    /// </summary>
    public FaFieldValidationBuilder<TModel, TValue> Field<TValue>(string propertyName, Func<TModel, TValue> accessor)
    {
        if (!_rules.TryGetValue(propertyName, out var list))
        {
            list = [];
            _rules[propertyName] = list;
        }

        return new FaFieldValidationBuilder<TModel, TValue>(propertyName, accessor, list);
    }

    /// <summary>Drops every rule declared for a property so far — the one place a form-tier override can start a field's rules over from nothing instead of only adding to the root validator's.</summary>
    public void RemoveField(string propertyName) => _rules.Remove(propertyName);

    /// <summary>Every rule declared so far, keyed by property name — read by <see cref="FaValidationResolver"/> once resolution (root tier, then form-tier override) is complete.</summary>
    internal IReadOnlyDictionary<string, List<Func<TModel, string?>>> Rules => _rules;
}

/// <summary>
/// The single-property rule-building handle <see cref="FaValidationBuilder{TModel}.Field{TValue}"/>
/// returns — each call here appends one more rule delegate to that property's list.
/// </summary>
public sealed class FaFieldValidationBuilder<TModel, TValue> where TModel : class
{
    private readonly string _propertyName;
    private readonly Func<TModel, TValue> _accessor;
    private readonly List<Func<TModel, string?>> _rules;

    internal FaFieldValidationBuilder(string propertyName, Func<TModel, TValue> accessor, List<Func<TModel, string?>> rules)
    {
        _propertyName = propertyName;
        _accessor = accessor;
        _rules = rules;
    }

    /// <summary>Adds a rule: <paramref name="predicate"/> false against the property's current value means <paramref name="errorMessage"/> shows for this field. Chainable — each call adds one more independent rule, all evaluated, all their messages surfaced.</summary>
    public FaFieldValidationBuilder<TModel, TValue> Must(Func<TValue, bool> predicate, string errorMessage)
    {
        _rules.Add(model => predicate(_accessor(model)) ? null : errorMessage);
        return this;
    }

    /// <summary>Shorthand for the single most common rule — non-null and, for strings, non-whitespace.</summary>
    public FaFieldValidationBuilder<TModel, TValue> Required(string errorMessage = "This field is required.")
    {
        _rules.Add(model =>
        {
            var value = _accessor(model);
            var isEmpty = value is null || (value is string s && string.IsNullOrWhiteSpace(s));
            return isEmpty ? errorMessage : null;
        });
        return this;
    }
}

/// <summary>
/// Shorthand rules built on top of <see cref="FaFieldValidationBuilder{TModel,TValue}.Must"/>
/// for the handful of checks common enough to not want a predicate written out by
/// hand every time. Extension methods rather than instance methods on the class
/// itself — each one needs a narrower constraint on <c>TValue</c> than the
/// unconstrained class declares (<see cref="IComparable{T}"/> for <see cref="Between"/>,
/// <c>string</c> for <see cref="MaxLength"/>), and C# can't vary a single class's own
/// type-parameter constraints per method.
/// </summary>
public static class FaFieldValidationBuilderExtensions
{
    /// <summary>Inclusive range check (<paramref name="min"/> and <paramref name="max"/> both pass). <paramref name="message"/> defaults to "Must be between {min} and {max}."</summary>
    public static FaFieldValidationBuilder<TModel, TValue> Between<TModel, TValue>(
        this FaFieldValidationBuilder<TModel, TValue> builder, TValue min, TValue max, string? message = null)
        where TModel : class
        where TValue : IComparable<TValue>
        => builder.Must(v => v.CompareTo(min) >= 0 && v.CompareTo(max) <= 0,
            message ?? $"Must be between {min} and {max}.");

    /// <summary>
    /// Exact-match check via <see cref="EqualityComparer{T}.Default"/>. Named
    /// <c>EqualTo</c>, not <c>Equals</c>, so it can't collide with — or get silently
    /// shadowed by — <see cref="object.Equals(object?)"/> at the call site.
    /// </summary>
    public static FaFieldValidationBuilder<TModel, TValue> EqualTo<TModel, TValue>(
        this FaFieldValidationBuilder<TModel, TValue> builder, TValue expected, string? message = null)
        where TModel : class
        => builder.Must(v => EqualityComparer<TValue>.Default.Equals(v, expected),
            message ?? $"Must equal {expected}.");

    /// <summary>Caps a string field's length. A null value passes — pair with <see cref="FaFieldValidationBuilder{TModel,TValue}.Required"/> on the same field if empty shouldn't be allowed either.</summary>
    public static FaFieldValidationBuilder<TModel, string> MaxLength<TModel>(
        this FaFieldValidationBuilder<TModel, string> builder, int max, string? message = null)
        where TModel : class
        => builder.Must(v => v is null || v.Length <= max,
            message ?? $"Must be {max} characters or fewer.");
}
