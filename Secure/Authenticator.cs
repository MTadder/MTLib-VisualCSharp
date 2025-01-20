using System.Collections.ObjectModel;

namespace MTLib.Secure;
public sealed class Authenticator {
    private List<String> _keys;
    private Salt _salt;
    public void Clear() => this._keys.Clear();
    public void Register(String key) => this._keys.Add(this.Hash(key));
    public void Register(String[] keys) {
        ArgumentNullException.ThrowIfNull(keys);
        String[] hashedKeys = new String[keys.Length];
        for (Int32 i = 0; i < keys.Length; i++) {
            hashedKeys[i] = this.Hash(keys[i]);
        }
        this._keys.AddRange(hashedKeys);
    }
    public void Register(Collection<String> keys) => this.Register(keys.ToArray());
    public void Register(Char key) => this.Register(key.ToString());
    public void Register(Single key) => this.Register(key.ToString());
    public void Register(Double key) => this.Register(Convert.ToString(key));
    public bool IsRegistered(String data, Boolean doHash = true) {
        String hashedQuery = doHash ? this.Hash(data) : data;
        foreach (String key in this._keys) {
            if (key.Equals(hashedQuery, StringComparison.Ordinal)) {
                return true;
            }
        }
        return false;
    }
    public Authenticator() {
        this._keys = [];
        this._salt = new();
    }
    public String Hash(String data) => this._salt.Hash(data);
}