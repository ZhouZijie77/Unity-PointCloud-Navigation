/// <summary>
/// 处理六边形方向相关问题
/// </summary>
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}
public static class HexDirectionExtensions
{

    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
}

