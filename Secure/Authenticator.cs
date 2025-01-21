using System.Collections.ObjectModel;

namespace MTLib.Secure;
public sealed class Authenticator {
    private List<String> keys;
    private Salt salt;
    public void Clear() => this.keys.Clear();
    public void Register(String key) => this.keys.Add(this.Hash(key));
    public void Register(String[] keys) {
        ArgumentNullException.ThrowIfNull(keys);
        String[] hashedKeys = new String[keys.Length];
        for (Int32 i = 0; i < keys.Length; i++) {
            hashedKeys[i] = this.Hash(keys[i]);
        }
        this.keys.AddRange(hashedKeys);
    }
    public void Register(Collection<String> keys) => this.Register(keys.ToArray());
    public void Register(Char key) => this.Register(key.ToString());
    public void Register(Single key) => this.Register(key.ToString());
    public void Register(Double key) => this.Register(Convert.ToString(key));
    public bool IsRegistered(String data, Boolean doHash = true) {
        String hashedQuery = doHash ? this.Hash(data) : data;
        foreach (String key in this.keys) {
            if (key.Equals(hashedQuery, StringComparison.Ordinal)) {
                return true;
            }
        }
        return false;
    }
    public Authenticator() {
        this.keys = [];
        this.salt = new();
    }
    public String Hash(String data) => this.salt.Hash(data);
}