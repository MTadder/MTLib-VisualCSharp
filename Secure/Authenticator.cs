namespace MTLib.Secure;
public sealed class Authenticator {
    private List<String> keySet = [];
    public Salt Salt = new();
    public void Clear() => this.keySet.Clear();
    public void Register(String key) => this.keySet.Add(this.Hash(key));
    public void Register(String[] keys) {
        ArgumentNullException.ThrowIfNull(keys);
        String[] hashedKeys = new String[keys.Length];
        for (Int32 i = 0; i < keys.Length; i++) {
            hashedKeys[i] = this.Hash(keys[i]);
        }
        this.keySet.AddRange(hashedKeys);
    }
    public void Register(List<String> keys) => this.Register(keys.ToArray());
    public void Register(Char key) => this.Register(key.ToString());
    public void Register(Single key) => this.Register(key.ToString());
    public void Register(Double key) => this.Register(Convert.ToString(key));
    public bool IsRegistered(String query, Boolean doHash = true) {
        String hashedQuery = doHash ? this.Hash(query) : query;
        foreach (String key in this.keySet) {
            if (key.Equals(hashedQuery, StringComparison.Ordinal)) {
                return true;
            }
        }
        return false;
    }
    public Authenticator() { }
    public String Hash(String data) => this.Salt.Hash(data);
}