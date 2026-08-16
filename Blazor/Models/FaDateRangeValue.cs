namespace FaFa.Components;

/// <summary>
/// The bound value for <see cref="FaDateRange"/> — a from/to pair, either end
/// optionally unset. A plain record struct rather than a tuple so it has a stable,
/// nameable type for parameters/fields that hold it (<c>FaDateRangeValue Range</c>
/// reads better than <c>(DateOnly?, DateOnly?) Range</c> at call sites).
/// </summary>
public readonly record struct FaDateRangeValue(DateOnly? From, DateOnly? To);
