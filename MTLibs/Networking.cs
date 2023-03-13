using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTLib
{
    public class Networking
    {
        public const int BufferSize = 1024;

        public class Transmission
        {
            public enum Type
            {
                Unknown, // Used when the Transmission class fails to discern a Type.
                Authorization, // Used when logging in to the Dedicated Server.
                RoomRequest, // Used when attempting to join a Room.
                Message, // Used when sending messages in Rooms.
            }

            public Type CurrentType;
            private Dictionary<string, string> Content = new Dictionary<string, string>();

            public Transmission(Type NewType)
            {
                this.CurrentType = NewType;
            }
            public Transmission(byte[] Data)
            {
                this.Decypher(Data);
            }

            public bool Send(Socket S)
            {
                try
                {
                    S.Send(this.Cypher());
                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }
            }

            public byte[] Cypher()
            {
                /// Initialize Writers
                MemoryStream WriteStream = new MemoryStream();
                BinaryWriter BinWriter = new BinaryWriter(WriteStream);

                /// Firstly, Write the Transmission Type
                BinWriter.Write(this.CurrentType.ToString());

                /// Get Content data
                int ContentLen;
                byte[] ContentBytes = this.CypherContent(out ContentLen);

                /// Secondly, Write the Length of Content
                BinWriter.Write(ContentLen);

                /// Lastly, Write the entirety of Content
                BinWriter.Write(ContentBytes);

                /// Close Writers
                BinWriter.Close();

                return WriteStream.ToArray();
            }
            private void Decypher(byte[] Data)
            {
                /// Initialize Readers
                MemoryStream ReaderStream = new MemoryStream(Data);
                BinaryReader BinReader = new BinaryReader(ReaderStream);

                /// Firstly, Read the Tranmission Type
                this.CurrentType = (Type)Enum.Parse(typeof(Type), BinReader.ReadString());

                /// Secondly, Read the Length of Content
                int ContentLen = BinReader.ReadInt32();

                /// Lastly, Read the Content bytes
                byte[] ContentBytes = BinReader.ReadBytes(ContentLen);
                this.DecypherContent(ContentBytes);

                /// Close Readers
                BinReader.Close();
            }
            private byte[] CypherContent(out int bytes)
            {
                /// Initialize Writers
                MemoryStream WriteStream = new MemoryStream();
                BinaryWriter BinWriter = new BinaryWriter(WriteStream);

                /// Write total number of Key,Value pairs
                BinWriter.Write(this.Content.Count);

                /// Get Content Enumerator
                Dictionary<string, string>.Enumerator E = this.Content.GetEnumerator();
                E.MoveNext();

                /// Write each Key and Value
                for (int i = 0; i < this.Content.Count; i++)
                {
                    BinWriter.Write(E.Current.Key);
                    BinWriter.Write(E.Current.Value);
                    E.MoveNext();
                }

                /// Close Writers
                BinWriter.Close();

                /// Return
                byte[] Finished = WriteStream.ToArray();
                bytes = Finished.Length;
                return Finished;
            }
            private void DecypherContent(byte[] Content)
            {
                /// Initialize Readers
                MemoryStream ReaderStream = new MemoryStream(Content);
                BinaryReader BinReader = new BinaryReader(ReaderStream);

                /// Read total number of Key,Value pairs
                int Pairs = BinReader.ReadInt32();

                /// Read Pairs into Current Content
                for (int i = 0; i < Pairs; i++)
                {
                    this.Content.Add(BinReader.ReadString(), BinReader.ReadString());
                }

                /// Close Readers
                BinReader.Close();
            }

            public void AddValue(string Key, string Value)
            {
                this.Content.Add(Key, Value);
            }
            public string GetValue(string Key)
            {
                try
                {
                    return this.Content[Key];
                }
                catch (KeyNotFoundException)
                {
                    Debug.WriteLine("Could not find Key: " + Key);
                    return string.Empty;
                }
            }

        }
    }
}
