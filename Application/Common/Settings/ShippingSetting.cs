namespace Application.Common.Settings;

public class ShippingSetting
{
    // Маркази фиристониш (анбор/дафтари марказӣ)
    public string WarehouseCity { get; set; } = "Душанбе";
    public double WarehouseLatitude { get; set; } = 38.5598;
    public double WarehouseLongitude { get; set; } = 68.7870;

    // Дохили ҳамон шаҳр — нархи собит
    public decimal SameCityFlatRate { get; set; } = 15;

    // Берун аз шаҳр — нархи ҳадди ақал (агар координата набошад ё масофа хеле кам бошад)
    public decimal MinOtherCityRate { get; set; } = 30;

    // Берун аз шаҳр — нарх барои ҳар километр (агар координата дастрас бошад)
    public decimal PricePerKm { get; set; } = 1.5m;
}