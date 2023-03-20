namespace MTLibrary {
    public class Helpers {
        private static readonly HashSet<Type> ValTupleTypes = new(
            new Type[] { typeof(ValueTuple<>), typeof(ValueTuple<,>),
                 typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
                 typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>),
                 typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>) });
        public static bool IsValueTuple(object obj) {
            var type = obj.GetType();
            return type.IsGenericType&&ValTupleTypes.Contains(type.GetGenericTypeDefinition());
        }
        public static IEnumerable<object?> GetValuesFromTuple(System.Runtime.CompilerServices.ITuple tuple) {
            for (int i = 0; i<tuple.Length; i++) {
                yield return tuple[i];
            }
            Tron.Net.Client.WalletAddress walletAddress = Tron.Net.Client.WalletAddress.MainNetWalletAddress();
        }
    }
}
