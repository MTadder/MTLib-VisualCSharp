namespace MTLibrary {
    /// <summary>
    /// Secure Encapsulatations and Implementations for Data Transmission/Transmutation
    /// </summary>
    public static class Secure {
        /// <summary>
        /// Represents a trustworthy <see cref="keyset">keyset</see> holder and/or verifier
        /// </summary>
        public class Authenticator {
            private List<string> keyset = new();
            private Salt salt = new();
            public Salt Salt {
                get { return this.salt; }
                set { this.salt=value; }
            }
            public void Clear() => this.keyset.Clear();
            public void Register(string key) => this.keyset.Add(this.Hash(key));
            public void Register(string[] keys) {
                string[] hashedKeys = new string[keys.Length];
                for (int i = 0; i<keys.Length; i++) {
                    hashedKeys[i]=this.Hash(keys[i]);
                }
                this.keyset.AddRange(hashedKeys);
            }
            public void Register(List<string> keys) => this.Register(keys.ToArray());
            public void Register(char key) => Register(key.ToString());
            public void Register(float key) => Register(key.ToString());
            public void Register(double key) => Register(key.ToString());
            public bool IsRegistered(string query, bool doHash = true) {
                string hashedQuery = doHash ? this.Hash(query) : query;
                foreach (string key in this.keyset) {
                    if (key.Equals(hashedQuery)) { return true; }
                }
                return false;
            }
            public void Persist(DictionaryFile<string, string> into) {
                int idx = 0;
                foreach (string key in this.keyset) {
                    into[idx.ToString()]=key;
                }
            }
            public Authenticator() { }
            public Authenticator(DictionaryFile<string, string> df) {
                int idx = 0;
                while (df.Memory.ContainsKey(idx.ToString())) {
                    this.keyset.Insert(idx, df[idx.ToString()]);
                }
            }
            public string Hash(string data) {
                return this.Salt.Hash(data);
            }
        }
        public class Salt {
            private char[] data;
            protected internal static char[] GenerateSegments(Int32 amount) {
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
            public Salt(int segments = 13) => this.data=GenerateSegments(segments);
            public Salt(Salt target) => this.data=target.data;
            public Salt(string salt) => this.data=salt.ToCharArray();
            public string Hash(string data) {
                string result = "";
                for (int dataCharIdx = 0; dataCharIdx<data.Length; dataCharIdx++) {
                    char saltChar = this.data[dataCharIdx%this.data.Length];
                    char dataChar = data[dataCharIdx];
                    result+=char.ConvertFromUtf32(((int) saltChar%(int) dataChar)+0xE)??"+";
                }
                return result.Replace('?', '%');
            }
        }
    }
}
