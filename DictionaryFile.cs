using System.Text.Json;

namespace MTLibrary {
    public class DictionaryFile<T, Q>: IDisposable
        where T : notnull {
        #region Statics
        #endregion
        #region Internals
        protected internal FileInfo _target;
        protected internal Dictionary<T, Q> _memory = new();
        protected internal bool _synced = false;
        #endregion
        #region Properties
        public Q this[T key] {
            get { return this._memory[key]; }
            set {
                this._memory[key]=value;
                this._synced=false;
            }
        }
        public static implicit operator Dictionary<T, Q>(DictionaryFile<T, Q> df) { return df._memory; }
        public static explicit operator DictionaryFile<T, Q>(Dictionary<T, Q> dict) {
            DictionaryFile<T, Q> df = new() { _memory=dict };
            return df;
        }
        public Dictionary<T, Q> Memory { get { return _memory; } set { this._memory=value; } }
        public bool Synced { get { return this._synced; } }
        public bool IsTemporary { get { return this._target.Extension.Equals(".tmp"); } }
        public string FileName { get { return this._target.Name; } }
        public string FilePath { get { return this._target.FullName; } }
        public FileInfo TargetFileInfo {
            get { return this._target; }
            set { this._target=value; }
        }
        public int BytesRead, BytesWritten;
        #endregion
        #region Constructors
        public DictionaryFile(string targetPath = "") {
            this._target=string.IsNullOrEmpty(targetPath) ?
                new(Path.GetTempFileName()) : new(targetPath);
        }
        #endregion
        #region Methods
        public bool Validate() {
            using FileStream fStr = this._target.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using BinaryReader bReader = new(fStr);
            bool hasMemory = this.Memory.Count>0;
            return bReader.PeekChar().Equals(-1) ?
                hasMemory.Equals(false) : this.Memory.Count.Equals(bReader.ReadInt32());
        }
        public void Save() {
            if (this._synced) return;
            using FileStream fStr = this._target.Open(FileMode.Truncate, FileAccess.Write, FileShare.Write);
            using BinaryWriter bWriter = new(fStr);
            var explorer = this._memory.GetEnumerator();
            bWriter.Write(this._memory.Count);
            while (explorer.MoveNext()) {
                string key = JsonSerializer.Serialize<T>(explorer.Current.Key);
                string val = JsonSerializer.Serialize<Q>(explorer.Current.Value);
                bWriter.Write(key); bWriter.Write(val);
            }
            this.BytesWritten+=(int) fStr.Length;
            this._synced=true;
        }
        public void Read() {
            using FileStream fStr = this._target.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            int len = (int) fStr.Length;
            if (len.Equals(0)&&this._memory.Count.Equals(0)) {
                this._synced=true;
                return;
            }
            this.BytesRead+=len;
            using BinaryReader bReader = new(fStr);
            int pairs = bReader.ReadInt32();
            for (int i = 0; i<pairs; i++) {
                string nextStr = bReader.ReadString();
                T key = JsonSerializer.Deserialize<T>(nextStr)??throw new InvalidDataException();
                nextStr=bReader.ReadString();
                Q value = JsonSerializer.Deserialize<Q>(nextStr)??throw new InvalidDataException();
                this[key]=value;
            }
            this._synced=this._memory.Count.Equals(pairs);
        }
        public void Dispose() {
            GC.SuppressFinalize(this);
            if (this.TargetFileInfo.Extension.Equals(".tmp")) {
                this.TargetFileInfo.Delete();
            }
        }
        #endregion
    }
}
