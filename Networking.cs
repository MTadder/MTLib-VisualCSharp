using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace MTLib {
    public class Networking {

        public interface Cypherable {
            byte[] Cypher();
            void Decypher(byte[] buffer);
        }

        public const int BufferSize = 1024;

        public const int ServerPort = 11942;
        public const string ServerIP = "138.197.197.92";

        public class TransmissionException: Exception { }
        public class CypherException: Exception { }

        public class Transmission: Networking.Cypherable {
            public string _Type;
            private Dictionary<string, string> Content = new Dictionary<string, string>();
            public Transmission(string type) {
                this._Type=type;
            }
            public Transmission(byte[] data) {
                this.Decypher(data);
            }
            public Transmission(Socket sock) {
                byte[] data = new byte[BufferSize];
                try {
                    if (sock.Receive(data)<=0) { throw new TransmissionException(); }
                    this.Decypher(data);
                } catch { throw; }
            }
            public bool Transmit(Socket sock) {
                try {
                    return sock.Send(this.Cypher())<=0 ? throw new SocketException() : true;
                } catch (SocketException) {
                    return false;
                }
            }
            public byte[] Cypher() {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(this._Type);

                byte[] ContentBytes = this.CypherContent(out int len);
                writer.Write(len);
                writer.Write(ContentBytes);

                writer.Close();
                return stream.ToArray();
            }
            public void Decypher(byte[] buffer) {
                MemoryStream stream = new MemoryStream(buffer);
                BinaryReader reader = new BinaryReader(stream);

                this._Type=reader.ReadString();
                int ContentLen = reader.ReadInt32();
                byte[] ContentBytes = reader.ReadBytes(ContentLen);

                this.DecypherContent(ContentBytes);
                reader.Close();
            }
            private byte[] CypherContent(out int bytes) {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(this.Content.Count);
                Dictionary<string, string>.Enumerator E = this.Content.GetEnumerator();
                if (!E.MoveNext()) {
                    throw new CypherException();
                }
                for (int i = 0; i<this.Content.Count; i++) {
                    writer.Write(E.Current.Key);
                    writer.Write(E.Current.Value);
                    if (!E.MoveNext()) { break; }
                }
                writer.Close();

                byte[] buffer = stream.ToArray();
                bytes=buffer.Length;
                return buffer;
            }
            private void DecypherContent(byte[] content) {
                MemoryStream stream = new MemoryStream(content);
                BinaryReader reader = new BinaryReader(stream);

                int Pairs = reader.ReadInt32();
                for (int i = 0; i<Pairs; i++) {
                    this.Content.Add(reader.ReadString(), reader.ReadString());
                }
                reader.Close();
            }
            public void AddCypherable(Cypherable cypher) {
                this.Add(cypher.Cypher().ToString());
            }
            public void Add(string val) {
                this.Content.Add((this.Content.Count+1).ToString(), val);
            }
            public void AddValue(string key, string val) {
                this.Content.Add(key, val);
            }
            public string GetValue(string key) {
                try {
                    return this.Content[key];
                } catch (KeyNotFoundException) {
                    Debug.WriteLine("Could not find Key: "+key);
                    return string.Empty;
                }
            }

        }
    }
}
