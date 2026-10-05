using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Helpers;

public static class LabAgreementTestPriceMergeHelper
{
    public static IReadOnlyList<LabAgreementTestPriceCommand> Merge(
        IReadOnlyList<LabAgreementTestPriceCommand>? parentTestPrices,
        IReadOnlyList<LabAgreementCommand>? childrenWithTestPrices)
    {
        var resultMap = new Dictionary<string, LabAgreementTestPriceCommand>(StringComparer.Ordinal);

        foreach (var testPrice in parentTestPrices ?? [])
        {
            resultMap[GetTestPriceKey(testPrice)] = Clone(testPrice);
        }

        var sortedChildren = (childrenWithTestPrices ?? [])
            .Where(child => child.TestPrices is { Count: > 0 })
            .OrderBy(child => child.StartDate)
            .ThenBy(child => child.Id ?? 0);

        foreach (var child in sortedChildren)
        {
            foreach (var testPrice in child.TestPrices!)
            {
                var merged = Clone(testPrice);
                merged.AddendumTitle = child.Title;
                resultMap[GetTestPriceKey(testPrice)] = merged;
            }
        }

        return resultMap.Values.ToList();
    }

    public static string GetTestPriceKey(LabAgreementTestPriceCommand testPrice)
        => $"{testPrice.TestId}-{testPrice.CPNCode ?? string.Empty}";

    private static LabAgreementTestPriceCommand Clone(LabAgreementTestPriceCommand source)
        => new()
        {
            Id = source.Id,
            TestId = source.TestId,
            TestName = source.TestName,
            Approved = source.Approved,
            BaseTariffApproved = source.BaseTariffApproved,
            FirstAdditions = source.FirstAdditions,
            SecondAdditions = source.SecondAdditions,
            UrgentAmount = source.UrgentAmount,
            CPNCode = source.CPNCode,
            NationalCode = source.NationalCode,
            AddendumTitle = source.AddendumTitle,
        };
}
