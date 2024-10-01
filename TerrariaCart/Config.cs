
using System.Text.Json;
using System.Text.Json.Serialization;
using MorMor;
using MorMor.Model.Terraria;

namespace TerrariaCart;

public class Config
{
    [JsonIgnore]
    public string PATH = Path.Combine(MorMorAPI.SAVE_PATH, "Cart.json");
    public Dictionary<long, Dictionary<string, List<int>>> Carts { get; set; } = new();

    public void Add(long uin, string cartName, int id)
    {
        if (Carts.TryGetValue(uin, out var carts) && carts != null)
        {
            if (carts.TryGetValue(cartName, out var shops) && shops != null)
            {
                var shop = MorMorAPI.TerrariaShop.GetShop(id);
                if (shop is not null)
                    Carts[uin][cartName].Add(id);
                else
                    throw new NullReferenceException("不存在的商品!");

            }
            else
            {
                Carts[uin][cartName] = [id];
            }

        }
        else
        {
            Carts[uin] = new()
            {
                { cartName, [id] }
            };
        }
    }

    public void Remove(long uin, string cartName, int id)
    {
        if (Carts.TryGetValue(uin, out var carts) && carts != null)
        {
            if (carts.TryGetValue(cartName, out var shops) && shops != null)
            {
                if (shops.Contains(id))
                    Carts[uin][cartName].Remove(id);
                else
                    throw new NullReferenceException("此商品不存在于购物车!");

            }
            else
            {
                throw new NullReferenceException("不存在的购物车!");
            }

        }
        else
        {
            throw new NullReferenceException("不存在的购物车!");
        }
    }

    public void ClearCart(long uin, string cartName)
    {
        if (Carts.TryGetValue(uin, out var carts) && carts != null)
        {
            carts.Remove(cartName);
        }
    }

    public Dictionary<string, List<int>> GetCarts(long uin)
    {
        if (Carts.TryGetValue(uin, out var carts) && carts != null)
        {
            return carts;
        }
        throw new NullReferenceException("没有购物车被添加!");
    }
    public List<int> GetCart(long uin, string cartName)
    {
        if (Carts.TryGetValue(uin, out var carts) && carts != null)
        {
            if (carts.TryGetValue(cartName, out var shops) && shops != null)
            {
                return shops;
            }
            else
            {
                throw new NullReferenceException("不存在的购物车!");
            }

        }
        else
        {
            throw new NullReferenceException("不存在的购物车!");
        }
    }

    public List<Shop> GetCartShop(long uin, string cartName)
    {
        var items = GetCart(uin, cartName);
        var shopping = new List<Shop>();
        foreach (var id in items)
        {
            var item = MorMorAPI.TerrariaShop.GetShop(id);
            if (item != null)
                shopping.Add(item);
        }
        return shopping;

    }

    public Config LoadConfig()
    {
        if (File.Exists(PATH))
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(PATH)) ?? new();
        Save();
        return new();
    }

    public void Save()
    {
        File.WriteAllText(PATH, JsonSerializer.Serialize(this, new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        }));
    }
}
