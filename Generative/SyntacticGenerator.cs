using System.Text;

namespace MTLib.Generative;
/// <summary>
/// Provides a high level interface for sequential syntax generation.
/// </summary>
public sealed class SyntacticGenerator {
    public SyntacticGenerator() { }
    public SyntacticGenerator(String languageName) {
        this.Language = languageName;
    }
    public enum TabType {
        MAINTAIN,
        INCREASE,
        DECREASE,
    }
    public String? Language;
    public Dictionary<Byte, String> Literals = [];
    public StringBuilder Result = new();
    public UInt16 TabCount;
    public String TabString {
        get {
            return new String('\t', TabCount);
        }
    }
    public SyntacticGenerator WriteLine(Byte literal, TabType tabType = TabType.MAINTAIN) {
        ArgumentNullException.ThrowIfNull(
                Literals[literal], nameof(literal));
        Result.Append(TabString + Literals[literal] + "\n");
        switch (tabType) {
            case TabType.MAINTAIN:
                break;
            case TabType.INCREASE:
                TabCount++;
                break;
            case TabType.DECREASE:
                TabCount--;
                break;
        }
        return this;
    }
    /// <summary>
    /// Writes the given <c>Literal</c> to the <see cref="StringBuilder"/>.
    /// </summary>
    /// <returns><c>this</c> instance, for consecutive calls.</returns>
    public SyntacticGenerator Write(Byte literal, Boolean putTab = false, TabType tabType = TabType.MAINTAIN) {
        ArgumentNullException.ThrowIfNull(
        Literals[literal], nameof(literal));
        Result.Append((putTab ? TabString : "") + Literals[literal]);
        switch (tabType) {
            case TabType.MAINTAIN:
                break;
            case TabType.INCREASE:
                TabCount++;
                break;
            case TabType.DECREASE:
                TabCount--;
                break;
        }
        return this;
    }
    /// <summary>
    /// Writes the given <see cref="String"/> to the <see cref="StringBuilder"/>.
    /// </summary>
    /// <returns><c>this</c> instance, for consecutive calls.</returns>
    public SyntacticGenerator Write(String data, Boolean putTab = false, TabType tabType = TabType.MAINTAIN) {
        Result.Append((putTab ? TabString : "") + data);
        switch (tabType) {
            case TabType.MAINTAIN:
                break;
            case TabType.INCREASE:
                TabCount++;
                break;
            case TabType.DECREASE:
                TabCount--;
                break;
        }
        return this;
    }
    /// <inheritdoc/>
    public override String ToString() {
        return Result.ToString();
    }
}
