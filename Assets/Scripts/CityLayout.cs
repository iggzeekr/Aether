using UnityEngine;

public static class CityLayout
{
    public const int Blocks = 10;
    public const float Cell = 44f;
    public const float Road = 14f;
    public const float SidewalkWidth = 3.4f;
    public const float SidewalkHeight = 0.2f;
    public const float Half = Blocks * Cell * 0.5f;

    public static Vector3 Intersection(int ix, int iz)
    {
        return new Vector3(-Half + ix * Cell, 0.2f, -Half + iz * Cell);
    }
}
