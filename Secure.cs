namespace MTLib;
public static class Secure {
    public class Authenticator {
        private List<String> _keySet = [];
        private Salt _salt = new();
        public Salt Salt {
            get => this._salt;
            set => this._salt = value;
        }
        public void Clear() => this._keySet.Clear();
        public void Register(String key) => this._keySet.Add(this.Hash(key));
        public void Register(String[] keys) {
            String[] hashedKeys = new String[ keys.Length ];
            for ( Int32 i = 0; i < keys.Length; i++ ) {
                hashedKeys[ i ] = this.Hash(keys[ i ]);
            }
            this._keySet.AddRange(hashedKeys);
        }
        public void Register(List<String> keys) => this.Register(keys.ToArray());
        public void Register(Char key) => this.Register(key.ToString());
        public void Register(Single key) => this.Register(key.ToString());
        public void Register(Double key) => this.Register(key.ToString());
        public bool IsRegistered(String query, Boolean doHash = true) {
            String hashedQuery = doHash ? this.Hash(query) : query;
            foreach ( String key in this._keySet ) {
                if ( key.Equals(hashedQuery) ) { return true; }
            }
            return false;
        }
        public Authenticator() { }
        public String Hash(String data) => this._salt.Hash(data);
    }
    public class Salt {
        private Char[] _data;
        private static Char[] _GetChaos(Int32 amount) {
            static Char[] Chaos() {
                return Guid.NewGuid().ToString().Replace("-", "").ToCharArray();
            }
            Char[] initialSegment = Chaos();
            Int32 SegmentLength = initialSegment.Length;
            List<Char> Segments = new(SegmentLength * amount);
            Segments.AddRange(initialSegment);
            for ( Int32 idx = 1; idx < amount; idx++ ) {
                Segments.AddRange(Chaos());
            }
            return [ .. Segments ];
        }
        public Salt(Int32 segments = 13) => this._data = _GetChaos(segments);
        public Salt(Salt target) => this._data = target._data;
        public Salt(Char[] data) => this._data = data;
        public Salt(String salt) => this._data = salt.ToCharArray();

        public String Hash(String data) {
            String result = "";
            for ( Int32 char_idx = 0; char_idx < data.Length; char_idx++ ) {
                Char saltChar = this._data[ char_idx % this._data.Length ];
                Char dataChar = data[ char_idx ];
                result += Char.ConvertFromUtf32(((Int32) saltChar % (Int32) dataChar) + 0xE) ?? "+";
            }
            return result.Replace('?', '%');
        }
    }
}
