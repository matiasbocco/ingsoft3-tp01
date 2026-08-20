namespace InventarioApi.Models;

public class Item
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
}
