using System.Text;

namespace MTLib.General;

// 0 | § | ¡
// £ | ¥ | µ
// ¶ | ¢ | ¿

/// <summary>
/// A grid of Symbols.
/// </summary>
public class SymbolicGrid<T> { // TODO: TEST ME
	private Int32 size;
	private T?[,] symbols;
	public Single Area => size * size;
	public Single Perimeter => 4 * size;
	/// <summary>
	/// Counts all non-null Symbols.
	/// </summary>
	public Int32 Count {
		get {
			Int32 n = 0;
			for (Int32 x = 0; x < size; x++) {
				for (Int32 y = 0; y < size; y++) {
					if (symbols[x, y] is not null) { continue; }
					n++;
				}
			}
			return n;
		}
	}

	public SymbolicGrid(Int32 size, T? fillWith) {
		this.size = size;
		this.symbols = new T[size, size];
		this.Clear(fillWith);
	}

	public SymbolicGrid(Int32 size) {
		this.size = size;
		this.symbols = new T[size, size];
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override string ToString() {
		StringBuilder res = new();
		for (Int32 x = 0; x < 3; x++) {
			for (Int32 y = 0; y < 3; y++) {
				if (x > 0) { _ = res.Append(' '); }
				_ = res.Append(this[x, y]);
			}
			if (x != 2) {
				_ = res.Append(" |");
			}
		}
		return res.ToString();
	}

	public T? this[Int32 x, Int32 y] {
		get => this.symbols[x, y];
		set => this.symbols[x, y] = value;
	}

	/// <summary>
	/// Clears the Symbolic grid of all symbols.
	/// </summary>
	public void Clear(T? clearWith) {
		for (Int32 x = 0; x < 3; x++) {
			for (Int32 y = 0; y < 3; y++) {
				this[x, y] = clearWith;
			}
		}
	}

	/// <summary>
	/// Determines if the <paramref name="symbol"/> is in this <see cref="SymbolicGrid"/>.
	/// </summary>
	/// <returns><c>true</c>, if the <paramref name="symbol"/> is contained.</returns>
	public Boolean Contains(T? symbol) {
		for (Int32 x = 0; x < 3; x++) {
			for (Int32 y = 0; y < 3; y++) {
				if (this[x, y]?.Equals(symbol) ?? false) { return true; }
				if (this[x, y] is null && (symbol is null)) { return true; }
			}
		}
		return false;
	}
}
