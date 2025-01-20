namespace MTLib.Secure;
public sealed class Salt {
    private Char[] _data;
    private static Char[] _GetChaos(Int32 amount) {
        static Char[] Chaos() {
            return Guid.NewGuid().ToString().Replace("-", "").ToCharArray();
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
    public Salt(Int32 segments = 13) => this._data = _GetChaos(segments);
    public Salt(Salt target) => this._data = target._data;
    public Salt(Char[] data) => this._data = data;
    public Salt(String salt) => this._data = salt.ToCharArray();
    public override String ToString() => new(this._data);
    public override Boolean Equals(Object? obj) {
        if (obj is Salt target) {
            return this._data.SequenceEqual(target._data);
        }
        if (obj is String salt) {
            return this._data.SequenceEqual(salt.ToCharArray());
        }
        return false;
    }
    public String Hash(String data) {
        String result = "";
        for (Int32 char_idx = 0; char_idx < data.Length; char_idx++) {
            Char saltChar = this._data[char_idx % this._data.Length];
            Char dataChar = data[char_idx];
            result += Char.ConvertFromUtf32(((Int32) saltChar % (Int32) dataChar) + 0xE) ?? "+";
        }
        return result.Replace('?', '%');
    }
}