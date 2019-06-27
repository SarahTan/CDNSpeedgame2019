/// <summary>
/// Don't use this enum in serialized content as the values may change as the layers do.
/// </summary>
public enum Layers
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,
    Cloud = 8,
    CloudSegment = 9,
    Player = 10,
    Star = 11,
    Wall = 12,
}

/// <summary>
/// Don't use this enum in serialized content as the values may change as the layers do.
/// </summary>
public enum LayerMasks
{
    Cloud = 1 << Layers.Cloud,
    CloudSegment = 1 << Layers.CloudSegment,
    LaserBlocker = (1 << Layers.Cloud) | (1 << Layers.CloudSegment),
    Wall = 1 << Layers.Wall,
}

public static class Tags
{
    public const string Enemy = "Enemy";
}