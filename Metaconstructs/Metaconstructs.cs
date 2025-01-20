namespace MTLib.Metaconstructs;

public enum Flavor {
    None = 0,
    ZERO = '0',
    X = 'X',
    Y = 'Y',
    Z = 'Z',
    MAXIMA = '⬆',
    MINIMA = '⬇',
}

public class Metaconstruct {
    public Metaconstruct(Flavor flavor) {
        // TODO
    }
}
public sealed class Vector: Metaconstruct {
    public Vector() : base(Flavor.ZERO) {
        // TODO
    }
}
public sealed class Minima: Metaconstruct {
    public Minima() : base(Flavor.MINIMA) {
        // TODO
    }
}
public sealed class Maxima: Metaconstruct {
    public Maxima() : base(Flavor.MAXIMA) {
        // TODO
    }
}
public sealed class Time: Metaconstruct {
    public Time() : base(Flavor.X) {
        // TODO
    }
}
public sealed class Emotion: Metaconstruct {
    public Emotion() : base(Flavor.Y) {
        // TODO
    }
}
public sealed class Projection: Metaconstruct {
    public Projection() : base(Flavor.Z) {
        // TODO
    }
}