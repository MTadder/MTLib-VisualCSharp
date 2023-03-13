using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace MTLib {
    public class Interaction {
        public class Text: Networking.Cypherable {
            public string text = "";
            public ConsoleColor color = ConsoleColor.White;
            public bool Empty() {
                return this.text.Length==0;
            }
            public Byte[] Cypher() {
                MemoryStream WriteStream = new MemoryStream();
                BinaryWriter BinWriter = new BinaryWriter(WriteStream);
                BinWriter.Write(this.text);
                BinWriter.Close();
                return WriteStream.ToArray();
            }
            public void Decypher(Byte[] buffr) {
                MemoryStream WriteStream = new MemoryStream(buffr);
                BinaryReader BinReader = new BinaryReader(WriteStream);
                string gotBin = BinReader.ReadString();
                if (gotBin.Length==0) { return; }
                this.text=gotBin;
            }
            public Text() {

            }
            public Text(string txt) {
                this.text=txt;
            }
            public Text(string txt, ConsoleColor color) {
                this.text=txt;
                this.color=color;
            }
        }

        public class TextCollection: Networking.Cypherable {
            private List<Text> coll = new List<Text>();
            public Text this[int index] {
                get { return this.coll[index]; }
                set { this.coll[index]=value; }
            }
            public void Add(string txt) {
                this.coll.Add(new Text(txt));
            }
            public void Add(Text txt) {
                this.coll.Add(txt);
            }
            public bool Remove(Text txt) {
                if (this.coll.Contains(txt)) {
                    return this.coll.Remove(txt);
                }
                return false;
            }
            public byte[] Cypher() {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(this.coll.Count);
                foreach (Text T in this.coll) {
                    writer.Write(T.text);
                    writer.Write(T.color.ToString());
                }
                writer.Close();
                return stream.ToArray();
            }
            public void Decypher(byte[] buffer) {
                MemoryStream stream = new MemoryStream(buffer);
                BinaryReader reader = new BinaryReader(stream);
                List<Text> GotTexts = new List<Text>();
                for (int i = 0; i<reader.ReadInt32(); i++) {
                    string txt = reader.ReadString();
                    string clr = reader.ReadString();
                    ConsoleColor Color = (ConsoleColor) Enum.Parse(typeof(ConsoleColor), clr);
                    GotTexts.Add(new Text(txt, Color));
                }
            }
        }

        public interface ISlate {
            //void AddText(Text text);
            //void AddOption(Text text, ConsoleColor color = ConsoleColor.White);
            void Draw();
        }

        public class SlateProperties {
            bool RequiresInput = false;
            bool IsReadOnly = false;
            bool EchoOnDisplay = false;
            bool Listening = false;
        }

        public class Slate: ISlate {
            public string TransmissionType = "Slate";
            public SlateProperties Properties = new SlateProperties();
            public Text Description = new Text();
            public TextCollection Options = new TextCollection();
            public void Draw() {
                throw new NotImplementedException();
            }
            public void Transmit(Socket sock) {
                if (!sock.Connected) { return; }
                this.Transmit(sock);

                var t = new Networking.Transmission(this.TransmissionType);
                t.AddCypherable(this.Description);
                t.AddCypherable(this.Options);
                try {
                    if (!t.Transmit(sock)) {

                    }
                    return;
                } catch { }
                
            }
        }
    }
}
