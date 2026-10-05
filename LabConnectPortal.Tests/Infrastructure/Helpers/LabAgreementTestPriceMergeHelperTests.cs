using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Tests.Infrastructure.Helpers;

public class LabAgreementTestPriceMergeHelperTests
{
    [Fact]
    public void Merge_ReturnsOnlyParentTests_WhenNoChildren()
    {
        var parent = new[]
        {
            CreateTestPrice(1, 100, "A"),
            CreateTestPrice(2, 200, "B"),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(parent, null);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.TestId == 100 && x.AddendumTitle == null);
        Assert.Contains(result, x => x.TestId == 200 && x.AddendumTitle == null);
    }

    [Fact]
    public void Merge_IncludesChildTests_WhenNoOverlap()
    {
        var parent = new[] { CreateTestPrice(1, 100, "Parent Test") };
        var children = new[]
        {
            CreateChild(
                id: 10,
                title: "Addendum 1",
                startDate: new DateTime(2026, 1, 1),
                CreateTestPrice(2, 200, "Child Test")),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(parent, children);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.TestId == 100 && x.AddendumTitle == null);
        Assert.Contains(result, x => x.TestId == 200 && x.AddendumTitle == "Addendum 1");
    }

    [Fact]
    public void Merge_ChildOverridesParent_ForSameTestIdAndCpnCode()
    {
        var parent = new[] { CreateTestPrice(1, 100, "Parent Test", "CPN-1", approved: 1000) };
        var children = new[]
        {
            CreateChild(
                id: 10,
                title: "Addendum Override",
                startDate: new DateTime(2026, 2, 1),
                CreateTestPrice(2, 100, "Child Test", "CPN-1", approved: 2000)),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(parent, children);

        Assert.Single(result);
        Assert.Equal(2000, result[0].Approved);
        Assert.Equal("Addendum Override", result[0].AddendumTitle);
    }

    [Fact]
    public void Merge_KeepsBothEntries_WhenSameTestIdButDifferentCpnCode()
    {
        var parent = new[] { CreateTestPrice(1, 100, "Parent Test", "CPN-1") };
        var children = new[]
        {
            CreateChild(
                id: 10,
                title: "Addendum 1",
                startDate: new DateTime(2026, 2, 1),
                CreateTestPrice(2, 100, "Child Test", "CPN-2")),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(parent, children);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.CPNCode == "CPN-1" && x.AddendumTitle == null);
        Assert.Contains(result, x => x.CPNCode == "CPN-2" && x.AddendumTitle == "Addendum 1");
    }

    [Fact]
    public void Merge_LaterChildWins_WhenMultipleChildrenOverrideSameKey()
    {
        var parent = new[] { CreateTestPrice(1, 100, "Parent Test", "CPN-1", approved: 1000) };
        var children = new[]
        {
            CreateChild(
                id: 10,
                title: "Older Addendum",
                startDate: new DateTime(2026, 1, 1),
                CreateTestPrice(2, 100, "Older", "CPN-1", approved: 1500)),
            CreateChild(
                id: 20,
                title: "Newer Addendum",
                startDate: new DateTime(2026, 3, 1),
                CreateTestPrice(3, 100, "Newer", "CPN-1", approved: 2500)),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(parent, children);

        Assert.Single(result);
        Assert.Equal(2500, result[0].Approved);
        Assert.Equal("Newer Addendum", result[0].AddendumTitle);
    }

    [Fact]
    public void Merge_SortsChildrenByStartDateThenId()
    {
        var children = new[]
        {
            CreateChild(
                id: 30,
                title: "Later by date",
                startDate: new DateTime(2026, 3, 1),
                CreateTestPrice(1, 100, "Later", "CPN-1", approved: 3000)),
            CreateChild(
                id: 10,
                title: "Earlier by date",
                startDate: new DateTime(2026, 1, 1),
                CreateTestPrice(2, 100, "Earlier", "CPN-1", approved: 1000)),
            CreateChild(
                id: 20,
                title: "Same date later id",
                startDate: new DateTime(2026, 2, 1),
                CreateTestPrice(3, 100, "Middle", "CPN-1", approved: 2000)),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(null, children);

        Assert.Single(result);
        Assert.Equal(3000, result[0].Approved);
        Assert.Equal("Later by date", result[0].AddendumTitle);
    }

    [Fact]
    public void Merge_IgnoresChildrenWithoutTestPrices()
    {
        var children = new[]
        {
            new LabAgreementCommand
            {
                Id = 10,
                Title = "Empty Addendum",
                StartDate = new DateTime(2026, 1, 1),
                TestPrices = [],
            },
            CreateChild(
                id: 20,
                title: "With Tests",
                startDate: new DateTime(2026, 2, 1),
                CreateTestPrice(1, 100, "Child Test")),
        };

        var result = LabAgreementTestPriceMergeHelper.Merge(null, children);

        Assert.Single(result);
        Assert.Equal("With Tests", result[0].AddendumTitle);
    }

    private static LabAgreementTestPriceCommand CreateTestPrice(
        long id,
        long testId,
        string testName,
        string? cpnCode = null,
        decimal approved = 0)
        => new()
        {
            Id = id,
            TestId = testId,
            TestName = testName,
            CPNCode = cpnCode,
            Approved = approved,
        };

    private static LabAgreementCommand CreateChild(
        long id,
        string title,
        DateTime startDate,
        params LabAgreementTestPriceCommand[] testPrices)
        => new()
        {
            Id = id,
            Title = title,
            StartDate = startDate,
            TestPrices = testPrices,
        };
}
