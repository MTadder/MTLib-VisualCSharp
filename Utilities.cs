namespace MTLib;
public class Utilities {
    private static readonly HashSet<Type> ValTupleTypes = new([
        typeof(ValueTuple<>), typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>) ]
    );
    public static Boolean IsValueTuple(Object obj) {
        var type = obj.GetType();
        return type.IsGenericType && ValTupleTypes.Contains(type.GetGenericTypeDefinition());
    }
    public static IEnumerable<Object?> GetValuesFromTuple(System.Runtime.CompilerServices.ITuple tuple) {
        for ( Int32 i = 0; i < tuple.Length; i++ ) {
            yield return tuple[ i ];
        }
    }
}
