namespace Fran.Components;

/// <summary>
/// Whether an <see cref="FaForm{TModel}"/> is creating a new record or editing an
/// existing one. Purely cosmetic — it only changes the default submit-button text
/// ("Create" vs "Save") and whether the delete action shows; FaForm has no concept
/// of "new" vs "existing" beyond what this flag says, since it never talks to
/// storage itself.
/// </summary>
public enum FaFormMode
{
    Create,
    Edit
}
