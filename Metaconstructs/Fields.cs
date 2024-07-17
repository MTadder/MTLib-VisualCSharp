namespace MTLib.Metaconstructs.Fields;

public class Environment : Metaconstruct {
    public Environment(Flavor Class) : base(Class) {
        // TODO
    }
}
public sealed class Content : Environment {
    public Content() : base(Flavor.X) {
        // TODO
    }
}
public sealed class Experience : Environment {
    public Experience() : base(Flavor.Z) {
        // TODO
    }
}
public sealed class Data : Environment {
    public Data() : base(Flavor.Y) {
        // TODO
    }
}
public sealed class Information : Environment {
    public Information() : base(Flavor.ZERO) {
        // TODO
    }
}