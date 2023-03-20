namespace MTLibrary {
    /// <summary>
    /// Secure Encapsulatations and Implementations for Data Transmission/Transmutation
    /// </summary>
    public static class Secure {
        /// <summary>
        /// Represents a trustworthy <see cref="_keyset">keyset</see> holder and/or verifier
        /// </summary>
        public class Authenticator {
            protected internal List<string> _keyset = new();
            protected internal Salt _salt = new();
            public Salt Salt {
                get { return this._salt; }
                set { this._salt=value; }
            }
            public void Clear() {
                this._keyset.Clear();
            }
            public void Register(string key) {
                this._keyset.Add(this.Hash(key));
            }
            public void Register(string[] keys) {
                string[] hashedKeys = new string[keys.Length];
                for (int i = 0; i<keys.Length; i++) {
                    hashedKeys[i]=this.Hash(keys[i]);
                }
                this._keyset.AddRange(hashedKeys);
            }
            public void Register(List<string> keys) {
                this.Register(keys.ToArray());
            }
            public void Register(char key) {
                this.Register(key.ToString());
            }
            public void Register(float key) {
                this.Register(key.ToString());
            }
            public void Register(double key) {
                this.Register(key.ToString());
            }
            public bool IsRegistered(string query, bool doHash = true) {
                string hashedQuery = doHash ? this.Hash(query) : query;
                foreach (string key in this._keyset) {
                    if (key.Equals(hashedQuery)) { return true; }
                }
                return false;
            }
            public void Persist(DictionaryFile<string, string> into) {
                int idx = 0;
                foreach (string key in this._keyset) {
                    into[idx.ToString()]=key;
                }
            }
            public Authenticator() { }
            public Authenticator(DictionaryFile<string, string> df) {
                int idx = 0;
                while (df.Memory.ContainsKey(idx.ToString())) {
                    this._keyset.Insert(idx, df[idx.ToString()]);
                }
            }
            public string Hash(string data) {
                return this.Salt.Hash(data);
            }
        }
        public class Salt {
            internal char[] _salt;
            internal static char[] GenerateSegments(Int32 amount) {
                static char[] GenerateSegment() {
                    return Guid.NewGuid().ToString().Replace("-", "").ToCharArray();
                }
                char[] initialSegment = GenerateSegment();
                int SegmentLength = initialSegment.Length;
                List<char> Segments = new(SegmentLength*amount);
                Segments.AddRange(initialSegment);
                for (int idx = 1; idx<amount; idx++) {
                    Segments.AddRange(GenerateSegment());
                }
                return Segments.ToArray();
            }
            public Salt(int segments = 13) {
                this._salt=GenerateSegments(segments);
            }
            public Salt(Salt target) {
                this._salt=target._salt;
            }
            public Salt(string salt) {
                this._salt=salt.ToCharArray();
            }
            //public override string ToString() {
            //return Meta.Serialize(this._salt, "", false).Trim();
            //}
            public string Hash(string data) {
                string result = "";
                for (int dataCharIdx = 0; dataCharIdx<data.Length; dataCharIdx++) {
                    char saltChar = this._salt[dataCharIdx%this._salt.Length];
                    char dataChar = data[dataCharIdx];
                    result+=char.ConvertFromUtf32(((int) saltChar%(int) dataChar)+0xE)??"+";
                }
                return result.Replace('?', '%');
            }
        }
    }
}
