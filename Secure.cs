using System.Text.Json;

namespace MTLib;
public static class Secure {
    public sealed class DictionaryFile<T1, T2>
        where T1 : notnull {
        private Dictionary<T1, T2> revertMemory = [];

        public Dictionary<T1, T2> Memory = [];
        public Boolean Synced;
        public FileInfo TargetFileInfo;

        public DictionaryFile(FileInfo targetFileInfo) {
            this.TargetFileInfo = targetFileInfo;
        }

        public DictionaryFile(String? targetPath = null) {
            this.TargetFileInfo = !String.IsNullOrEmpty(targetPath) ?
                new(targetPath) : new(Path.GetTempFileName());
        }

        public Boolean IsTemporary {
            get {
                return this.TargetFileInfo.Extension.Equals(@".tmp", StringComparison.Ordinal);
            }
        }
        public Int64 BytesWritten, BytesRead;
        public Int64 BytesTotal => BytesWritten + BytesRead;
        public T2 this[T1 key] {
            get { return this.Memory[key]; }
            set {
                if (this.Memory.TryGetValue(key, out T2? oldValue))
                    this.revertMemory[key] = oldValue;
                this.Memory[key] = value;
                this.Synced = false;
            }
        }
        public static implicit operator Dictionary<T1, T2>(DictionaryFile<T1, T2> df) {
            ArgumentNullException.ThrowIfNull(df);
            return df.Memory;
        }
        public static explicit operator DictionaryFile<T1, T2>(Dictionary<T1, T2> dict) {
            ArgumentNullException.ThrowIfNull(dict);
            DictionaryFile<T1, T2> df = new() { Memory = dict };
            return df;
        }
        public void Save() {
            if (this.Synced)
                return;
            FileStream fStr = this.TargetFileInfo.Exists.Equals(false) ?
                this.TargetFileInfo.Create() : this.TargetFileInfo.Open(FileMode.Truncate, FileAccess.Write, FileShare.Write);
            using BinaryWriter bWriter = new(fStr);
            bWriter.Write(this.Memory.Count);
            var explorer = this.Memory.GetEnumerator();
            while (explorer.MoveNext()) {
                string key = JsonSerializer.Serialize<T1>(explorer.Current.Key);
                string val = JsonSerializer.Serialize<T2>(explorer.Current.Value);
                bWriter.Write(key);
                bWriter.Write(val);
            }
            this.BytesWritten += (int) bWriter.Seek(0, SeekOrigin.End);
            this.Synced = true;
        }
        public void Read() {
            using FileStream fStr = this.TargetFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            int len = (int) fStr.Length;
            if (len.Equals(0) && this.Memory.Count.Equals(0)) {
                this.Synced = true;
                return;
            }
            this.BytesRead += len;
            using BinaryReader bReader = new(fStr);
            int pairs = bReader.ReadInt32();
            for (int i = 0; i < pairs; i++) {
                string nextStr = bReader.ReadString();
                T1 key = JsonSerializer.Deserialize<T1>(nextStr) ?? throw new InvalidDataException();
                nextStr = bReader.ReadString();
                T2 value = JsonSerializer.Deserialize<T2>(nextStr) ?? throw new InvalidDataException();
                this[key] = value;
            }
            this.Synced = this.Memory.Count.Equals(pairs);
        }
    }
    public sealed class Authenticator {
        private List<String> _keySet = [];
        private Salt _salt = new();
        public Salt Salt {
            get => this._salt;
            set => this._salt = value;
        }
        public void Clear() => this._keySet.Clear();
        public void Register(String key) => this._keySet.Add(this.Hash(key));
        public void Register(String[] keys) {
            String[] hashedKeys = new String[keys.Length];
            for (Int32 i = 0; i < keys.Length; i++) {
                hashedKeys[i] = this.Hash(keys[i]);
            }
            this._keySet.AddRange(hashedKeys);
        }
        public void Register(List<String> keys) => this.Register(keys.ToArray());
        public void Register(Char key) => this.Register(key.ToString());
        public void Register(Single key) => this.Register(key.ToString());
        public void Register(Double key) => this.Register(key.ToString());
        public bool IsRegistered(String query, Boolean doHash = true) {
            String hashedQuery = doHash ? this.Hash(query) : query;
            foreach (String key in this._keySet) {
                if (key.Equals(hashedQuery)) { return true; }
            }
            return false;
        }
        public Authenticator() { }
        public String Hash(String data) => this._salt.Hash(data);
    }
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
}
