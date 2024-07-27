using System.Text;

namespace MTLib.Generative;
public sealed class SyntacticGenerator {
    public enum TabType {
        MAINTAIN,
        INCREASE,
        DECREASE,
    }
    public Dictionary<Byte, String> Literals = [];
    public StringBuilder Result = new();
    public UInt16 TabCount;
    public String TabString {
        get {
            return new String('\t', TabCount);
        }
    }
    public void Commit(Byte literal, TabType tabType = TabType.MAINTAIN) {
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
    }
}
