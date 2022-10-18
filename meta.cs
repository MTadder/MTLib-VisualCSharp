using System;
using System.Collections;

namespace MTLibrary {
    public static class Meta {
        public static readonly String Projectname = "MTLibrary";
        public static readonly String Author = "MTadder";
        public static readonly String Email = "MTadder@pm.me";
        public static readonly String Codename = "izu x taey";
        public static readonly ConsoleColor ColorCode = ConsoleColor.Cyan;

        public static String MetaInformation() {
            return $"{Projectname} _{Codename}_ <{Author} @ {Email}>";
        }

        public static String Serialize(Array target, String seperator = ", ", Boolean showIndexes = true) {
            String serial = "{";
            if (target.Length < 1) { return String.Empty; }
            Int32 index = 0;
            for (IEnumerator arrEnum = target.GetEnumerator(); arrEnum.MoveNext();) {
                serial += showIndexes ? $"[{index}]: " : "";
                if (arrEnum.Current is Array || arrEnum.Current.GetType().IsArray) {
                    serial += Serialize((Array) arrEnum.Current, seperator);
                } else if (arrEnum.Current is Int32 arrInt) {
                    serial += arrInt + seperator;
                } else if (arrEnum.Current is String arrStr) {
                    serial += $"\"{arrStr}\"{seperator}";
                } else if (arrEnum.Current.GetType().IsSerializable) {
                    serial += $"{arrEnum.Current}{seperator}";
                } else {
                    throw new ArrayTypeMismatchException($"object '{nameof(target)}' is not serializable!");
                }
                index++;
            }
            serial = serial.Substring(0, serial.Length - 2);
            return $"{serial.Trim()}{'}'}"; // } must be escaped with a {'}'}
        }
    }
}