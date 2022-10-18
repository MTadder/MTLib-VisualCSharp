using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;

namespace MTLibrary {

    /// <summary>
    /// 
    /// </summary>
    public class DictionaryFile {
        #region Internals
        /// <summary>
        /// Checks if a filename has a certain extension.
        /// </summary>
        /// <param name="fileName">file name to test for extension</param>
        /// <param name="ext">extension to search for</param>
        /// <returns>a <see cref="Boolean">Bool</see></returns>
        internal static Boolean HasExtension(String fileName, String ext) {
            return String.IsNullOrEmpty(fileName) is not true && fileName.Contains($".{ext}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        internal static String GetAnonymousFilename(String ext = "bin") {
            String name = $"{Guid.NewGuid()}.{ext}";
            name = name.Replace('-', '0');
            return File.Exists(name) ? GetAnonymousFilename(ext) : name;
        }
        #endregion
        #region Constructors
        public DictionaryFile(DictionaryFile origin) {
            this._memory = new(origin._memory);
            this._targetInfo = new(origin._targetInfo.Name);
            this.Load();
        }
        public DictionaryFile(String fName) {
            this._memory = new();
            this._targetInfo = new(fName);
            this.Load();
        }
        public DictionaryFile() {
            this._memory = new();
            this._targetInfo = new(GetAnonymousFilename());
            this.Load();
        }
        #endregion
        #region Properties
        private Dictionary<String, String> _memory;
        private Boolean _inSync;
        private FileInfo _targetInfo;
        private Int32 BytesTransferred;
        public Int32 Count { get { return this._memory.Count; } }
        public Boolean IsSynchronized { get { return this._inSync; } }
        public String FileName {
            get { this._targetInfo.Refresh(); return this._targetInfo.Name; }
            set {
                if (value is null) {
                    _ = this.Delete();
                } else {
                    try { this._targetInfo.Delete(); } catch { }
                    this._targetInfo = new(value);
                    if (this._targetInfo.Exists is false) {
                        this.Load();
                    }
                }
            }
        }
        public String FilePath {
            get { this._targetInfo.Refresh(); return this._targetInfo.FullName; }
        }
        public String this[String key] {
            get { return this.Get(key); }
            set {
                if (value is not null) { this.Set(key, value); } else { _ = this.Remove(key); }
            }
        }
        public static implicit operator Dictionary<String, String>(DictionaryFile df) { return new(df._memory); }
        public static explicit operator DictionaryFile(Dictionary<String, String> pairs) {
            DictionaryFile df = new();
            df._memory = pairs;
            return df;
        }
        public static explicit operator DictionaryFile(String[] pairs) {
            if (pairs.Length % 2 == 0) {
                DictionaryFile df = new();
                for (Int32 i = 0; i < pairs.Length; i += 2) {
                    try {
                        String key = pairs[i];
                        try {
                            String val = pairs[i + 1];
                            df.Set(key, val);
                        } catch { throw; }
                    } catch { throw; }
                }
                return df;
            } else {
                throw new ArgumentOutOfRangeException($"{nameof(pairs)}",
               $"Cannot explicity parse an uneven Array!");
            }
        }
        public static DictionaryFile operator +(DictionaryFile df1, DictionaryFile df2) {
            var explorer = ((Dictionary<String, String>) df2).GetEnumerator();
            while (explorer.MoveNext()) { df1.Set(explorer.Current.Key, explorer.Current.Value); }
            return df1;
        }
        public static DictionaryFile operator -(DictionaryFile df1, DictionaryFile df2) {
            var explorer = ((Dictionary<String, String>) df2).GetEnumerator();
            while (explorer.MoveNext()) { _ = df1.Remove(explorer.Current.Key); }
            return df1;
        }
        #endregion
        #region Methods
        public void Save() {
            if (this.Count is 0) {
                this._inSync = this.Delete();
                return;
            }
            if (this._inSync is false) {
                using (FileStream targetStream = this._targetInfo.Open(FileMode.Truncate, FileAccess.Write)) {
                    using (BinaryWriter binWriter = new(targetStream)) {
                        binWriter.Write(this.Count);
                        var explorer = this._memory.GetEnumerator();
                        while (explorer.MoveNext()) {
                            binWriter.Write(explorer.Current.Key);
                            binWriter.Write(explorer.Current.Value);
                        }
                        binWriter.Flush();
                        this.BytesTransferred += (Int32) targetStream.Length;
                        binWriter.Close();
                    }
                } this._inSync = true;
            }
        }
        /// <summary>
        /// Opens/Creates the <see cref="DictionaryFile">DictionaryFile</see> on Disk,
        /// reads it's contents and emplaces the read data pairs into a <see cref="Dictionary{TKey, TValue}">
        /// Dictionary</see> (discarding non-valid pairs).
        /// </summary>
        /// <returns><see cref="void"/></returns>
        public void Load() {
            this._targetInfo.Refresh();
            try {
                using (FileStream targetStream = this._targetInfo.Open(FileMode.OpenOrCreate, FileAccess.Read)) {
                    Int32 len = (Int32) targetStream.Length;
                    if (len < 13) {
                        this._inSync = this.Count.Equals(0);
                        return; // stop short if no content is to be written
                    }
                    this.BytesTransferred += len;
                    Byte[] targetData = new Byte[len];
                    _ = targetStream.Read(targetData);
                    using (MemoryStream memStream = new(targetData)) {
                        using (BinaryReader binReader = new(memStream)) {
                            Int32 pairsToRead = binReader.ReadInt32();
                            for (Int32 i = 0; i < pairsToRead; i++) {
                                try {
                                    String gotKey = binReader.ReadString();
                                    try {
                                        String gotValue = binReader.ReadString();
                                        this._memory[gotKey] = gotValue;
                                    } catch { continue; }
                                } catch { continue; }
                            }
                            this._inSync = this.Count.Equals(pairsToRead);
                            binReader.Close();
                        }
                    }
                }
            } catch { throw; }
        }
        public void Clear() {
            this._memory.Clear();
            this._inSync = false;
        }
        public void Set(String key, String value) {
            (this._memory[key], this._inSync) = (value, false);
        }
        public void Set(String key) {
            this.Set(key, String.Empty);
            this._inSync = false;
        }
        public Boolean Remove(String key) {
            Boolean rem = this._memory.Remove(key);
            this._inSync = rem is false; // good logic
            return rem;
        }
        /// <summary>
        /// Deletes the DF from Storage.
        /// </summary>
        /// <returns>true, if the DF was properly deleted from the disk</returns>
        public Boolean Delete() {
            this._targetInfo.Refresh();
            if (this._targetInfo.Exists) {
                try { this._targetInfo.Delete(); } catch { return false; };
            }
            this._targetInfo.Refresh();
            return this._targetInfo.Exists is false;
        }
        public void Destroy() {
            this.Clear();
            _ = this.Delete();
        }
        public Boolean IsKey(String key) {
            try {
                _ = this._memory[key];
                return true;
            } catch { return false; }
        }
        public Boolean IsKey(String key, out String val) {
            try {
                val = this._memory[key];
                return true;
            } catch { val = String.Empty; return false; }
        }
        public Boolean IsValue(String value) {
            var explorer = this._memory.GetEnumerator();
            while (explorer.MoveNext()) {
                if (explorer.Current.Value.Equals(value, StringComparison.Ordinal)) { return true; }
            }
            return false;
        }
        public String Get(String key) {
            try { return this._memory[key]; } catch { return String.Empty; }
        }
        #endregion
    }
}