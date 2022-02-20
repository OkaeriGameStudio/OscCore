namespace BuildSoft.OscCore.UnityObjects;

public struct Vector2
{
    public float x;
    public float y;

    public Vector2(float x, float y) => (this.x, this.y) = (x, y);
    public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);
    public override bool Equals(object other) => other is Vector2 vector && Equals(vector);
    public bool Equals(Vector2 other) => x == other.x && y == other.y;
    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        float num = lhs.x - rhs.x;
        float num2 = lhs.y - rhs.y;
        return num * num + num2 * num2 < 9.99999944E-11f;
    }
    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return !(lhs == rhs);
    }
}
