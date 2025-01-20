using System.Text.Json;

namespace MTLib.Secure;
/// <summary>
/// A generic class that manages a dictionary of key-value
/// pairs, allowing for temporary storage and synchronization with a file, 
/// while providing functionality to revert changes and track read/write operations.
/// </summary>
/// <typeparam name="T1">Key type</typeparam>
/// <typeparam name="T2">Value type</typeparam>
public sealed class DictionaryFile<T1, T2> // TODO: DOCUMENT
        where T1 : notnull {
    private Dictionary<T1, T2> revertMemory = [];

    public Dictionary<T1, T2> Memory = [];
    public bool Synced;
    public FileInfo TargetFileInfo;

    public DictionaryFile(FileInfo targetFileInfo) {
        TargetFileInfo = targetFileInfo;
    }

    public DictionaryFile(String? targetPath = null) {
        TargetFileInfo = !String.IsNullOrEmpty(targetPath) ?
            new(targetPath) : new(Path.GetTempFileName());
    }

    public bool IsTemporary {
        get {
            return TargetFileInfo.Extension.Equals(@".tmp", StringComparison.Ordinal);
        }
    }
    public int BytesWritten, BytesRead;
    public int BytesTotal => BytesWritten + BytesRead;
    public T2 this[T1 key] {
        get { return Memory[key]; }
        set {
            if (Memory.TryGetValue(key, out T2? oldValue)) {
                revertMemory[key] = oldValue;
            }
            Memory[key] = value;
            Synced = false;
        }
    }
    public void Save() {
        if (Synced) { return; }
        FileStream fStr = TargetFileInfo.Exists.Equals(false) ?
            TargetFileInfo.Create() : TargetFileInfo.Open(FileMode.Truncate, FileAccess.Write, FileShare.Write);
        using BinaryWriter bWriter = new(fStr);
        bWriter.Write(Memory.Count);
        var explorer = Memory.GetEnumerator();
        while (explorer.MoveNext()) {
            string key = JsonSerializer.Serialize(explorer.Current.Key);
            string val = JsonSerializer.Serialize(explorer.Current.Value);
            bWriter.Write(key);
            bWriter.Write(val);
        }
        BytesWritten += ((int) bWriter.Seek(0, SeekOrigin.End));
        Synced = true;
    }
    public void Read() {
        using FileStream fStr = TargetFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        int len = ((int) fStr.Length);
        if (len.Equals(0) && Memory.Count.Equals(0)) {
            Synced = true;
            return;
        }
        BytesRead += len;
        using BinaryReader bReader = new(fStr);
        int pairs = bReader.ReadInt32();
        for (int i = 0; i < pairs; i++) {
            T1 key = JsonSerializer.Deserialize<T1>(bReader.ReadString())
                ?? throw new InvalidDataException();
            T2 value = JsonSerializer.Deserialize<T2>(bReader.ReadString())
                ?? throw new InvalidDataException();
            this[key] = value;
        }
        Synced = Memory.Count.Equals(pairs);
    }

    public Boolean HasKey(T1 key) => Memory.ContainsKey(key);
}