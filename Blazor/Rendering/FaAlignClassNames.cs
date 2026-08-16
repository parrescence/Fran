using FaFa.Components;

namespace FaFa.Rendering;

/// <summary>
/// Maps <see cref="FaAlign"/> to its <c>.fa-align-*</c> utility class (_utilities.scss)
/// — shared by every component that takes a button-row alignment (<see cref="FaModal"/>'s
/// footer, <see cref="FaForm{TModel}"/>/<see cref="FaLogoutForm"/>'s action bar) instead
/// of each one repeating its own switch.
/// </summary>
internal static class FaAlignClassNames
{
    public static string ToClass(FaAlign align) => align switch
    {
        FaAlign.Start => "fa-align-start",
        FaAlign.Center => "fa-align-center",
        FaAlign.Between => "fa-align-between",
        FaAlign.Around => "fa-align-around",
        FaAlign.Evenly => "fa-align-evenly",
        _ => "fa-align-end"
    };
}
