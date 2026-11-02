namespace Template.BlazorServer.Host.Reports;

public sealed record ProductItem(string Code, string Name, string Category, int Quantity, int Price)
{
    public long Amount => (long)Quantity * Price;
}

public static class ProductSample
{
    private static readonly string[] Categories = ["文具", "日用品", "食品", "家電"];

    private static readonly string[][] Names =
    [
        ["ボールペン", "ノート", "付箋", "ホッチキス", "クリップ"],
        ["タオル", "洗剤", "ハンドソープ", "ティッシュ", "歯ブラシ"],
        ["緑茶", "コーヒー", "クッキー", "カップ麺", "ミネラルウォーター"],
        ["電池", "延長コード", "USB ケーブル", "LED 電球", "イヤホン"]
    ];

    private static readonly int[] BasePrices = [120, 280, 160, 980];

    public static IReadOnlyList<ProductItem> LoadItems(int count) =>
        Enumerable.Range(1, count).Select(MakeItem).ToList();

    private static ProductItem MakeItem(int no)
    {
        var category = (no - 1) % Categories.Length;
        var names = Names[category];
        return new ProductItem(
            $"P-{no:0000}",
            names[((no - 1) / Categories.Length) % names.Length],
            Categories[category],
            1 + ((no * 7) % 24),
            BasePrices[category] + (((no * 37) % 10) * 20));
    }
}
