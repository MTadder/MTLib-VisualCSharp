namespace MTLib;
public static class Utilities {
    private static readonly HashSet<Type> ValTupleTypes = new([
        typeof(ValueTuple<>), typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>) ]
    );
    public static Boolean IsObjectTuple(Object obj) {
        ArgumentNullException.ThrowIfNull(obj);
        var type = obj.GetType();
        return (type.IsGenericType && ValTupleTypes.Contains(
            type.GetGenericTypeDefinition()
        ));
    }
    public static IEnumerable<Object?> GetValuesFromTuple(
        System.Runtime.CompilerServices.ITuple tuple
    ) {
        ArgumentNullException.ThrowIfNull(tuple);
        for (Int32 i = 0; i < tuple.Length; i++) {
            yield return tuple[i];
        }
    }
}
