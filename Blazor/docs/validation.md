[← Back to index](index.md)

# Validation

Three tiers of validation, most-specific wins, all feeding the same `EditContext`
`ValidationMessageStore` that Blazor's own `DataAnnotationsValidator`/
`ValidationSummary`/`ValidationMessage<TValue>` already read from:

| Tier | Where it lives | Analogous to |
|---|---|---|
| **Root/DTO** | `IFaValidator<TModel>`, one class per model, registered in DI | EF Core's `IEntityTypeConfiguration<TEntity>` |
| **Form** | a `ConfigureValidation` delegate passed to `FaModelValidator<TModel>` (or `FaForm<TModel>`'s own parameter of the same name) | a per-`DbContext` override of a shared entity config |
| **Element** | a `Validate` delegate parameter on the input itself (`FaInput`, `FaSelect`, `FaTextarea`, `FaCheckbox`) | a one-off rule that has no business living on the model at all |

Deliberately **not** a FluentValidation reimplementation — no `Expression<Func<T,
TProp>>` property-selector parsing, no fluent `RuleFor(x => x.Y)` DSL. Rules are
plain `Func` delegates; fields are keyed by plain strings (`nameof(Model.Prop)`),
the same thing `FieldIdentifier`/`ValidationMessageStore` already key on.

## Wiring it up, end to end

Four pieces, in the order you'd actually add them to a new model. Everything below
is one working example — copy it as a starting point, then trim whichever tier
your form doesn't need.

**1. The model** — plain C#, no attributes required:

```csharp
public class ProductModel
{
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Country { get; set; } = "";
    public string Category { get; set; } = "";
}
```

**2. The root/DTO validator** — one class, one rule per line, every variation of
rule side by side:

```csharp
public class ProductModelValidator : IFaValidator<ProductModel>
{
    public void Configure(FaValidationBuilder<ProductModel> builder)
    {
        // Required — non-null, and non-whitespace for strings.
        builder.Field(nameof(ProductModel.Name), m => m.Name)
            .Required("Name is required.");

        // Between — inclusive range, needs TValue : IComparable<TValue>.
        builder.Field(nameof(ProductModel.Price), m => m.Price)
            .Between(0.01m, 10_000m, "Price must be between $0.01 and $10,000.");

        builder.Field(nameof(ProductModel.Quantity), m => m.Quantity)
            .Between(1, 999);

        // EqualTo — exact match against a fixed value.
        builder.Field(nameof(ProductModel.Country), m => m.Country)
            .EqualTo("US", "Only US is supported right now.");

        // MaxLength — string-only, chainable with Required.
        builder.Field(nameof(ProductModel.Name), m => m.Name)
            .MaxLength(40);

        // Must — the general-purpose primitive everything above is sugar over.
        // Takes any predicate, so it also covers things the shorthands don't:
        // contains, starts-with, cross-field/conditional, whatever plain C# can express.
        builder.Field(nameof(ProductModel.Sku), m => m.Sku)
            .Must(sku => sku.StartsWith("SKU-"), "SKU must start with \"SKU-\".");

        // Must against the whole model (accessor returns m, not one property) —
        // how a conditional/cross-field rule looks: Category only required when
        // Country is US.
        builder.Field(nameof(ProductModel.Category), m => m)
            .Must(m => m.Country != "US" || !string.IsNullOrWhiteSpace(m.Category),
                "Category is required for US products.");
    }
}
```

**3. Register it once at startup** (`Program.cs`):

```csharp
builder.Services.AddFaValidator<ProductModel, ProductModelValidator>();
```

**4. The page** — every tier represented, so you can see how they stack:

```razor
@page "/products/new"

<EditForm Model="_model" OnValidSubmit="SaveAsync">
    @* Root tier (step 2) runs automatically; ConfigureValidation adds/overrides
       just for this one form (form tier). *@
    <FaModelValidator TModel="ProductModel" ConfigureValidation="ConfigureFormValidation" />
    <ValidationSummary />

    <FaInput TValue="string" @bind-Value="_model.Name" Label="Name" />
    <FaInput TValue="string" @bind-Value="_model.Sku" Label="SKU" />
    <FaInput TValue="decimal" @bind-Value="_model.Price" Type="number" Step="0.01" Label="Price" />
    <FaInput TValue="int" @bind-Value="_model.Quantity" Type="number" Label="Quantity" />
    <FaInput TValue="string" @bind-Value="_model.Country" Label="Country" />
    <FaInput TValue="string" @bind-Value="_model.Category" Label="Category" />

    @* Element tier — a rule that only exists on this one field, this one form,
       independent of the model entirely. Always shows in addition to whatever
       the root/form tiers already produced for Category. *@
    <FaInput TValue="string" @bind-Value="_model.Category" Label="Category (again, element-tier example)"
             Validate="@(v => v == "Uncategorized" ? "Pick a real category." : null)" />

    <FaButton Type="submit">Save</FaButton>
</EditForm>

@code {
    private ProductModel _model = new();

    private void ConfigureFormValidation(FaValidationBuilder<ProductModel> builder)
    {
        // Form tier — this form alone wants a tighter quantity cap than the
        // root validator's 1-999; RemoveField first since Between/Must both
        // just append, and this form wants to replace, not add to, that rule.
        builder.RemoveField(nameof(ProductModel.Quantity));
        builder.Field(nameof(ProductModel.Quantity), m => m.Quantity)
            .Between(1, 50, "This form caps orders at 50 units.");
    }

    private async Task SaveAsync()
    {
        // _model is already populated and passed every tier's validation.
        await ProductService.SaveAsync(_model);
    }
}
```

That's the whole system — a model, a root validator with whatever mix of
`Required`/`Between`/`EqualTo`/`MaxLength`/`Must` each field needs, a DI
registration, and a page that opts in via `FaModelValidator` (or `FaForm`'s
`UseFaValidation`, see [FaForm](fa-form.md#validation)) and optionally layers a
form- or element-tier rule on top. The sections below go deeper on each piece.

## Root tier: `IFaValidator<TModel>`

One implementation per model type, declaring that model's validation rules in one
place instead of scattering `[Required]`/`[Range]` attributes across its properties
(or instead of a model that can't carry attributes at all — a record from another
layer, a DTO you don't own):

```csharp
public class ProductModel
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public class ProductModelValidator : IFaValidator<ProductModel>
{
    public void Configure(FaValidationBuilder<ProductModel> builder)
    {
        builder.Field(nameof(ProductModel.Name), m => m.Name)
            .Required("Name is required.");

        builder.Field(nameof(ProductModel.Price), m => m.Price)
            .Between(0.01m, 10_000m, "Price must be between $0.01 and $10,000.");
    }
}
```

Register it once at startup:

```csharp
builder.Services.AddFaValidator<ProductModel, ProductModelValidator>();
```

Every `FaModelValidator<ProductModel>` (directly, or via `FaForm<ProductModel>`'s
`UseFaValidation`) picks this up automatically from DI — no per-form wiring needed
beyond opting in.

## Rule shorthands

`Must(predicate, message)` — an arbitrary `Func<TValue, bool>` — is the one real
primitive; every other rule is sugar over it. `Required` (shown above) is one such
shorthand; a few more common ones ship as extension methods on
`FaFieldValidationBuilder<TModel, TValue>`:

```csharp
builder.Field(nameof(ProductModel.Age), m => m.Age)
    .Between(18, 65); // inclusive; default message "Must be between 18 and 65."

builder.Field(nameof(ProductModel.Country), m => m.Country)
    .EqualTo("US", "Only US is supported right now.");

builder.Field(nameof(ProductModel.Name), m => m.Name)
    .Required()
    .MaxLength(40); // chainable — both rules run, both messages can show
```

`Between` needs `TValue : IComparable<TValue>` (works for `int`, `decimal`,
`DateOnly`, `string`, ... — not for a nullable value type like `int?`, since
`Nullable<T>` doesn't implement `IComparable<T>` itself; use `Must` for those).
`EqualTo` is named that, not `Equals`, so it can't collide with `object.Equals` at
the call site. `MaxLength` is `string`-only; a null value passes it (pair with
`Required` on the same field if empty shouldn't be allowed either).

Nothing stops a rule from reaching across the whole model instead of just one
property — the accessor passed to `Field` can return `m` itself:

```csharp
builder.Field(nameof(ProductModel.State), m => m)
    .Must(m => m.Country != "US" || !string.IsNullOrEmpty(m.State),
        "State is required for US addresses.");
```

## Form tier: `ConfigureValidation`

A delegate that runs after the root validator, on the same `FaValidationBuilder<TModel>`
— "override" means "this runs last and can add to, replace, or remove any field's
rules for just this one form," not a merge algorithm:

```razor
<EditForm Model="_model">
    <FaModelValidator TModel="ProductModel" ConfigureValidation="ConfigureFormValidation" />
    <ValidationSummary />

    <FaInput TValue="string" @bind-Value="_model.Name" Label="Name" />
    <FaInput TValue="string" @bind-Value="_model.Category" Label="Category" />
</EditForm>

@code {
    private ProductModel _model = new();

    private void ConfigureFormValidation(FaValidationBuilder<ProductModel> builder)
    {
        // Category has no root-tier rule at all — this form alone requires it.
        builder.Field(nameof(ProductModel.Category), m => m.Category)
            .Required("Category is required on this form.");
    }
}
```

`FaForm<TModel>` wires the same thing through its own `UseFaValidation`/
`ConfigureValidation` parameters instead of requiring a separate
`FaModelValidator<TModel>` tag — see [FaForm](fa-form.md).

A form with no registered `IFaValidator<TModel>` for its model can still use
`ConfigureValidation` on its own — the root tier resolves to an empty rule set if
nothing's registered, `ConfigureValidation` still runs on top of it either way.

## Element tier: `Validate`

Every `InputBase<TValue>`-derived field (`FaInput`, `FaSelect`, `FaTextarea`,
`FaCheckbox`) takes its own `Validate` delegate, evaluated fresh every render
against the field's current value — independent of the model entirely, no
`IFaValidator`/`FaModelValidator` involved:

```razor
<FaInput TValue="string" @bind-Value="_model.Sku" Label="SKU"
         Validate="@(v => v?.StartsWith("SKU-") == true ? null : "Must start with \"SKU-\".")" />
```

A non-null return always shows **in addition to** whatever the model/form tiers
already produced for that same field — it's additive, not an override of the other
two tiers' messages.

## Inline display

Every one of those four input components renders its own error message under
itself automatically, and adds `.fa-input-invalid`/`.fa-select-invalid` to its own
box, whenever `EditContext.GetValidationMessages` has anything for its field —
on by default (`ShowValidationMessage`, defaults `true`), so a form doesn't need
individual `<ValidationMessage>` tags added by hand for every field to actually
show anything. Flip `ShowValidationMessage="false"` on a specific instance for a
layout that shows errors somewhere else instead, paired with a standalone
[`FaValidationMessage<TValue>`](fa-input.md) placed wherever that layout wants it:

```razor
<FaInput TValue="string" @bind-Value="_model.Name" ShowValidationMessage="false" />
...
<FaValidationMessage For="@(() => _model.Name)" />
```

## FaDate's own Min/Max check

`FaDate`'s `Min`/`Max` are plain component parameters, not bound model fields — no
`FieldIdentifier` applies to them, so this doesn't route through `IFaValidator` at
all. `FaDate` (and `FaCurrency`) check their own `Min`/`Max` directly and render the
same `.fa-validation-message` look when `Max` ends up before `Min` — see
[FaDate](fa-date.md#parameters) and [FaCurrency](fa-currency.md#parameters). It's
shown, not thrown — a transient bad combination (Min/Max still loading from data,
say) doesn't crash the render tree, it just shows the message and leaves the rest
of the picker interactive.

## CSS classes

| Class | Applied to |
|---|---|
| `.fa-input-invalid` / `.fa-select-invalid` | the input/select box itself, when it has any validation messages |
| `.fa-validation-message` | the inline error text under a field |

Both use `var(--fa-ember)`, this library's existing danger/error color token.

[← Back to index](index.md)
