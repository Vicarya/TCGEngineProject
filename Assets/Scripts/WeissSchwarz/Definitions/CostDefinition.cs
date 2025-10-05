public class CostDefinition
{
    public int Stock { get; set; } = 0;      // 支払うストック枚数
    public int Discard { get; set; } = 0;    // 捨てる手札枚数
    public bool RestSelf { get; set; } = false; // このキャラをレスト
    public bool SendSelfToMemory { get; set; } = false; // このキャラを思い出へ
    // 将来的にはリスト形式で汎用化も可能
}
