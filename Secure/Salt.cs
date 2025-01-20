namespace MTLib.Secure;
public sealed class Salt {
    private Char[] data;
    private static Char[] GetChaos(Int32 amount) {
        static Char[] Chaos() {
            return Guid.NewGuid().ToString().Replace("-", "", StringComparison.Ordinal).ToCharArray();
        }
        Char[] initialSegment = Chaos();
        Int32 SegmentLength = initialSegment.Length;
        List<Char> Segments = new(SegmentLength * amount);
        Segments.AddRange(initialSegment);
        for (Int32 idx = 1; idx < amount; idx++) {
            Segments.AddRange(Chaos());
        }
        return [.. Segments];
    }
    public Salt(Int32 segments = 13) => this.data = GetChaos(segments);
    public Salt(Salt salt) {
        ArgumentNullException.ThrowIfNull(salt);
        this.data = salt.data;
    }
    public Salt(Char[] data) => this.data = data;
    public Salt(String data) {
        ArgumentNullException.ThrowIfNull(data);
        this.data = data.ToCharArray();
    }
    /// <inheritdoc/>
    public override String ToString() => new(this.data);

    /// <inheritdoc/>
    public override Int32 GetHashCode() {
        return this.data.GetHashCode();
    }

    /// <inheritdoc/>
    public override Boolean Equals(Object? obj) {
        if (obj is Salt target) {
            return this.data.SequenceEqual(target.data);
        }
        if (obj is String salt) {
            return this.data.SequenceEqual(salt.ToCharArray());
        }
        return false;
    }
    public String Hash(String data) {
        ArgumentNullException.ThrowIfNull(data);
        String result = "";
        for (Int32 char_idx = 0; char_idx < data.Length; char_idx++) {
            Char saltChar = this.data[char_idx % this.data.Length];
            Char dataChar = data[char_idx];
            result += Char.ConvertFromUtf32(((Int32) saltChar % (Int32) dataChar) + 0xE) ?? "+";
        }
        return result.Replace('?', '%');
    }
}