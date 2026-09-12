using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Fran.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Fran.Tests;

public class FaDropdownTests : BunitContext
{
    [Fact]
    public void ReopeningDropdown_WithExistingValue_RendersAllItemsInList()
    {
        var items = new[] { "Apples", "Bananas", "Cherries", "Dates" };
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.Value, "Bananas"));

        var input = cut.Find("input[role='combobox']");
        Assert.Equal("Bananas", input.GetAttribute("value"));

        // Click to open
        input.Click();

        // Verify all 4 options are rendered, not just "Bananas"
        var options = cut.FindAll("button.fa-dropdown-option");
        Assert.Equal(4, options.Count);
        Assert.Equal(new[] { "Apples", "Bananas", "Cherries", "Dates" }, options.Select(o => o.TextContent.Trim()));

        // Verify "Bananas" is highlighted/active
        Assert.Equal("true", options[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void TypingInDropdown_FiltersList()
    {
        var items = new[] { "Apples", "Bananas", "Cherries", "Dates" };
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.Value, "Bananas"));

        var input = cut.Find("input[role='combobox']");
        input.Input("Cher");

        var options = cut.FindAll("button.fa-dropdown-option");
        Assert.Single(options);
        Assert.Equal("Cherries", options[0].TextContent.Trim());
    }

    [Fact]
    public void ClickingOption_SelectsItem_UpdatesInputAndCloses()
    {
        var items = new[] { "Apples", "Bananas", "Cherries", "Dates" };
        string? selected = "Bananas";
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.Value, selected)
            .Add(x => x.ValueChanged, v => selected = v));

        var input = cut.Find("input[role='combobox']");
        input.Click();

        var options = cut.FindAll("button.fa-dropdown-option");
        options[2].Click(); // Cherries

        Assert.Equal("Cherries", selected);
        Assert.Equal("Cherries", input.GetAttribute("value"));
        Assert.Empty(cut.FindAll("button.fa-dropdown-option"));
    }

    [Fact]
    public void PressingEscape_ResetsInputToSelectedValueAndCloses()
    {
        var items = new[] { "Apples", "Bananas", "Cherries", "Dates" };
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.Value, "Bananas"));

        var input = cut.Find("input[role='combobox']");
        input.Input("Cher");
        Assert.Equal("Cher", input.GetAttribute("value"));

        var dropdown = cut.Find("div.fa-dropdown");
        dropdown.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Equal("Bananas", input.GetAttribute("value"));
        Assert.Empty(cut.FindAll("button.fa-dropdown-option"));
    }

    [Fact]
    public async Task FocusOut_WithoutSelection_ResetsInputToSelectedValue()
    {
        var items = new[] { "Apples", "Bananas", "Cherries", "Dates" };
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.Value, "Bananas"));

        var input = cut.Find("input[role='combobox']");
        input.Input("Cher");
        Assert.Equal("Cher", input.GetAttribute("value"));

        var dropdown = cut.Find("div.fa-dropdown");
        dropdown.FocusOut();

        // Wait for the 150ms grace period to expire
        await Task.Delay(250);

        Assert.Equal("Bananas", input.GetAttribute("value"));
        Assert.Empty(cut.FindAll("button.fa-dropdown-option"));
    }

    [Fact]
    public void OpeningDropdown_WithSelectedValueBeyondMaxVisible_ScrollsWindowToIncludeSelection()
    {
        var items = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToArray();
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.MaxVisibleItems, 5)
            .Add(x => x.Value, "Item 15"));

        var input = cut.Find("input[role='combobox']");
        input.Click();

        var options = cut.FindAll("button.fa-dropdown-option");
        Assert.Equal(5, options.Count);

        // "Item 15" should be visible in the window and highlighted
        var item15 = options.FirstOrDefault(o => o.TextContent.Trim() == "Item 15");
        Assert.NotNull(item15);
        Assert.Equal("true", item15.GetAttribute("aria-selected"));
    }

    [Fact]
    public void RemoteDropdown_OpeningWithExistingValue_QueriesWithEmptyFilter()
    {
        string? capturedQuery = null;
        var cut = Render<FaDropdown<string>>(p => p
            .Add(x => x.QueryPageAsync, (query, skip, take) =>
            {
                capturedQuery = query;
                var list = (IReadOnlyList<string>)new[] { "Alpha", "Beta", "Gamma" };
                return Task.FromResult((list, 3));
            })
            .Add(x => x.ItemLabel, x => x)
            .Add(x => x.Searchable, true)
            .Add(x => x.Value, "Beta"));

        var input = cut.Find("input[role='combobox']");
        Assert.Equal("Beta", input.GetAttribute("value"));

        input.Click();

        // Remote query should be called with "" (unfiltered), not "Beta"
        Assert.Equal("", capturedQuery);
    }
}
