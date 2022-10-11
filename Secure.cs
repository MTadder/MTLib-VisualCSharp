using System;
using System.Collections.Generic;
using System.Text;

namespace MTLibrary {
    public static class Secure {
        public class Authenticator {
            internal List<String> _keyset = new();
            internal Salt _salt = new();
            public Salt Salt { 
                get { return this._salt; }
                set { this._salt = value; }
            }
            public override String ToString() {
                return Meta.Serialize(_keyset.ToArray(), "", false);
            }
            public void Clear() {
                this._keyset.Clear();
            }
            public void Register(String key) {
                this._keyset.Add(this.Hash(key));
            }
            public void Register(String[] keys) {
                String[] hashedKeys = new String[keys.Length];
                for (Int32 i = 0; i < keys.Length; i++) {
                    hashedKeys[i] = this.Hash(keys[i]);
                } this._keyset.AddRange(hashedKeys);
            }
            public void Register(List<String> keys) {
                this.Register(keys.ToArray());
            }
            public void Register(Char key) {
                this.Register(key.ToString());
            }
            public void Register(Single key) {
                this.Register(key.ToString());
            }
            public void Register(Double key) {
                this.Register(key.ToString());
            }
            public Boolean IsRegistered(String query) {
                String hashedQuery = this.Hash(query);
                foreach (String key in this._keyset) {
                    if (key.Equals(hashedQuery)) {
                        return true;
                    }
                }
                return false;
            }
            public DictionaryFile Persist(DictionaryFile into) {
                Int32 idx = 0;
                foreach (String key in this._keyset) {
                    into[idx.ToString()] = key;
                }
                return into;
            }
            public Authenticator() { }
            public Authenticator(DictionaryFile sourceDF) {
                Int32 idx = 0;
                while (sourceDF.IsKey(idx.ToString(), out String val)) {
                    this._keyset.Insert(idx, val);
                }
            }
            public String Hash(String data) { return this.Salt.Hash(data); }
        }
        public class Salt {
            internal Char[] _salt;
            internal static Char[] GenerateSegments(Int32 amount) {
                static Char[] GenerateSegment() {
                    return Guid.NewGuid().ToString().Replace("-", "").ToCharArray();
                }
                Char[] initialSegment = GenerateSegment();
                Int32 SegmentLength = initialSegment.Length;
                List<Char> Segments = new(SegmentLength * amount);
                Segments.AddRange(initialSegment);
                for (Int32 idx = 1; idx < amount; idx++) {
                    Segments.AddRange(GenerateSegment());
                }
                return Segments.ToArray();
            }
            public Salt(Int32 segments = 13) {
                this._salt = GenerateSegments(segments);
            }
            public Salt(Salt target) {
                this._salt = target._salt;
            }
            public Salt(String content) {
                this._salt = content.ToCharArray();
            }
            public override String ToString() {
                return Meta.Serialize(this._salt, "", false).Trim();
            }
            public String Hash(String data) {
                String saltString = this.ToString();
                if (data.Equals(String.Empty)) { return saltString; }
                String newData = data;
                foreach (Char dataChar in data) {
                    Char newChar = dataChar;
                    foreach (Char saltChar in saltString) {
                        newChar += (Char) ((Byte) saltChar + (Byte) dataChar);
                    }
                    newData += (Char) ((Byte) newChar % 0xFFFF);
                }
                return newData.Replace(data, "");
            }
        }
    }
}
