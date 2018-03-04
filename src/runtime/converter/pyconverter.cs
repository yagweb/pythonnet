using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Python.Runtime
{
    public class PyConvert
    {
        internal static bool ToString(IntPtr value, out object result, bool setError)
        {
            Type obType = typeof(string);
            if (value == Runtime.PyNone)
            {
                result = null;
                return true;
            }

            result = Runtime.GetManagedString(value);
            if (result == null)
            {
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            return true;
        }

        private static void SetTypeError(IntPtr value, Type obType)
        {
            string tpName = Runtime.PyObject_GetTypeName(value);
            Exceptions.SetError(Exceptions.TypeError, $"'{tpName}' value cannot be converted to {obType}");
        }

        private static void SetOverflowError()
        {
            Exceptions.SetError(Exceptions.OverflowError, "value too large to convert");
        }

        internal static bool ToInt32(IntPtr value, out object result, bool setError)
        {
            result = null;
            IntPtr op;
            Type obType = typeof(int);

            // Trickery to support 64-bit platforms.
            if (Runtime.IsPython2 && Runtime.Is32Bit)
            {
                op = Runtime.PyNumber_Int(value);

                // As of Python 2.3, large ints magically convert :(
                if (Runtime.PyLong_Check(op))
                {
                    Runtime.XDecref(op);
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }

                if (op == IntPtr.Zero)
                {
                    if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                    {
                        if (setError)
                        {
                            SetOverflowError();
                        }
                        return false;
                    }
                    if (setError)
                    {
                        SetTypeError(value, obType);
                    }
                    return false;
                }
                result = (int)Runtime.PyInt_AsLong(op);
                Runtime.XDecref(op);
                return true;
            }
            else // Python3 always use PyLong API
            {
                op = Runtime.PyNumber_Long(value);
                if (op == IntPtr.Zero)
                {
                    Exceptions.Clear();
                    if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                    {
                        if (setError)
                        {
                            SetOverflowError();
                        }
                        return false;
                    }
                    if (setError)
                    {
                        SetTypeError(value, obType);
                    }
                    return false;
                }
                long ll = (long)Runtime.PyLong_AsLongLong(op);
                Runtime.XDecref(op);
                if (ll == -1 && Exceptions.ErrorOccurred())
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (ll > Int32.MaxValue || ll < Int32.MinValue)
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                result = (int)ll;
                return true;
            }
        }

        internal static bool ToBoolean(IntPtr value, out object result, bool setError)
        {
            result = Runtime.PyObject_IsTrue(value) != 0;
            return true;
        }

        internal static bool ToByte(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(byte);
#if PYTHON3
            if (Runtime.PyObject_TypeCheck(value, Runtime.PyBytesType))
            {
                if (Runtime.PyBytes_Size(value) == 1)
                {
                    op = Runtime.PyBytes_AS_STRING(value);
                    result = (byte)Marshal.ReadByte(op);
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
#elif PYTHON2
            if (Runtime.PyObject_TypeCheck(value, Runtime.PyStringType))
            {
                if (Runtime.PyString_Size(value) == 1)
                {
                    op = Runtime.PyString_AsString(value);
                    result = (byte)Marshal.ReadByte(op);
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
#endif

            op = Runtime.PyNumber_Int(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            var ival = (int)Runtime.PyInt_AsLong(op);
            Runtime.XDecref(op);

            if (ival > Byte.MaxValue || ival < Byte.MinValue)
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            result = (byte)ival;
            return true;
        }

        internal static bool ToSByte(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(sbyte);
#if PYTHON3
            if (Runtime.PyObject_TypeCheck(value, Runtime.PyBytesType))
            {
                if (Runtime.PyBytes_Size(value) == 1)
                {
                    op = Runtime.PyBytes_AS_STRING(value);
                    result = (sbyte)Marshal.ReadByte(op);
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
#elif PYTHON2
            if (Runtime.PyObject_TypeCheck(value, Runtime.PyStringType))
            {
                if (Runtime.PyString_Size(value) == 1)
                {
                    op = Runtime.PyString_AsString(value);
                    result = (sbyte)Marshal.ReadByte(op);
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
#endif

            op = Runtime.PyNumber_Int(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            var ival = (int)Runtime.PyInt_AsLong(op);
            Runtime.XDecref(op);

            if (ival > SByte.MaxValue || ival < SByte.MinValue)
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            result = (sbyte)ival;
            return true;
        }

        internal static bool ToChar(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(char);
#if PYTHON3
            if (Runtime.PyObject_TypeCheck(value, Runtime.PyBytesType))
            {
                if (Runtime.PyBytes_Size(value) == 1)
                {
                    op = Runtime.PyBytes_AS_STRING(value);
                    result = (byte)Marshal.ReadByte(op);
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
#elif PYTHON2
            if (Runtime.PyObject_TypeCheck(value, Runtime.PyStringType))
            {
                if (Runtime.PyString_Size(value) == 1)
                {
                    op = Runtime.PyString_AsString(value);
                    result = (char)Marshal.ReadByte(op);
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
#endif
            else if (Runtime.PyObject_TypeCheck(value, Runtime.PyUnicodeType))
            {
                if (Runtime.PyUnicode_GetSize(value) == 1)
                {
                    op = Runtime.PyUnicode_AsUnicode(value);
                    Char[] buff = new Char[1];
                    Marshal.Copy(op, buff, 0, 1);
                    result = buff[0];
                    return true;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }

            op = Runtime.PyNumber_Int(value);
            if (op == IntPtr.Zero)
            {
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            var ival = Runtime.PyInt_AsLong(op);
            Runtime.XDecref(op);
            if (ival > Char.MaxValue || ival < Char.MinValue)
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            result = (char)ival;
            return true;
        }

        internal static bool ToInt16(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(Int16);

            op = Runtime.PyNumber_Int(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            var ival = (int)Runtime.PyInt_AsLong(op);
            Runtime.XDecref(op);
            if (ival > Int16.MaxValue || ival < Int16.MinValue)
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            short s = (short)ival;
            result = s;
            return true;
        }

        internal static bool ToInt64(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(Int64);

            op = Runtime.PyNumber_Long(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            long l = (long)Runtime.PyLong_AsLongLong(op);
            Runtime.XDecref(op);
            if ((l == -1) && Exceptions.ErrorOccurred())
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            result = l;
            return true;
        }

        internal static bool ToUInt16(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(UInt16);

            op = Runtime.PyNumber_Int(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            var ival = (int)Runtime.PyInt_AsLong(op);
            Runtime.XDecref(op);
            if (ival > UInt16.MaxValue || ival < UInt16.MinValue)
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            ushort us = (ushort)ival;
            result = us;
            return true;
        }

        internal static bool ToUInt32(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(UInt32);

            op = Runtime.PyNumber_Long(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            uint ui = (uint)Runtime.PyLong_AsUnsignedLong(op);

            if (Exceptions.ErrorOccurred())
            {
                Runtime.XDecref(op);
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }

            IntPtr check = Runtime.PyLong_FromUnsignedLong(ui);
            int err = Runtime.PyObject_Compare(check, op);
            Runtime.XDecref(check);
            Runtime.XDecref(op);
            if (0 != err || Exceptions.ErrorOccurred())
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }

            result = ui;
            return true;
        }

        internal static bool ToUInt64(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(UInt64);

            op = Runtime.PyNumber_Long(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            ulong ul = (ulong)Runtime.PyLong_AsUnsignedLongLong(op);
            Runtime.XDecref(op);
            if (Exceptions.ErrorOccurred())
            {
                if (setError)
                {
                    SetOverflowError();
                }
                return false;
            }
            result = ul;
            return true;
        }

        internal static bool ToSingle(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(float);

            op = Runtime.PyNumber_Float(value);
            if (op == IntPtr.Zero)
            {
                if (Exceptions.ExceptionMatches(Exceptions.OverflowError))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            double dd = Runtime.PyFloat_AsDouble(op);
            Runtime.CheckExceptionOccurred();
            Runtime.XDecref(op);
            if (dd > Single.MaxValue || dd < Single.MinValue)
            {
                if (!double.IsInfinity(dd))
                {
                    if (setError)
                    {
                        SetOverflowError();
                    }
                    return false;
                }
            }
            result = (float)dd;
            return true;
        }

        internal static bool ToDouble(IntPtr value, out object result, bool setError)
        {
            IntPtr op;
            result = null;
            Type obType = typeof(Double);

            op = Runtime.PyNumber_Float(value);
            if (op == IntPtr.Zero)
            {
                if (setError)
                {
                    SetTypeError(value, obType);
                }
                return false;
            }
            double d = Runtime.PyFloat_AsDouble(op);
            Runtime.CheckExceptionOccurred();
            Runtime.XDecref(op);
            result = d;
            return true;
        }

        internal static bool ToPrimitive(IntPtr value, Type obType, out object result, bool setError)
        {
            TypeCode tc = Type.GetTypeCode(obType);

            switch (tc)
            {
                case TypeCode.String:                    
                    return ToString(value, out result, setError);

                case TypeCode.Int32:
                    return ToInt32(value, out result, setError);

                case TypeCode.Boolean:
                    return ToBoolean(value, out result, setError);

                case TypeCode.Byte:
                    return ToByte(value, out result, setError);

                case TypeCode.SByte:
                    return ToSByte(value, out result, setError);

                case TypeCode.Char:
                    return ToChar(value, out result, setError);

                case TypeCode.Int16:
                    return ToInt16(value, out result, setError);

                case TypeCode.Int64:
                    return ToInt64(value, out result, setError);

                case TypeCode.UInt16:
                    return ToUInt16(value, out result, setError);

                case TypeCode.UInt32:
                    return ToUInt32(value, out result, setError);

                case TypeCode.UInt64:
                    return ToUInt64(value, out result, setError);

                case TypeCode.Single:
                    return ToSingle(value, out result, setError);

                case TypeCode.Double:
                    return ToDouble(value, out result, setError);
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Convert a Python value to a correctly typed managed enum instance.
        /// </summary>
        private static bool ToEnum(IntPtr value, Type obType, out object result, bool setError)
        {
            Type etype = Enum.GetUnderlyingType(obType);
            result = null;

            if (!ToPrimitive(value, etype, out result, setError))
            {
                return false;
            }

            if (Enum.IsDefined(obType, result))
            {
                result = Enum.ToObject(obType, result);
                return true;
            }

            if (obType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0)
            {
                result = Enum.ToObject(obType, result);
                return true;
            }

            if (setError)
            {
                Exceptions.SetError(Exceptions.ValueError, "invalid enumeration value");
            }

            return false;
        }
    }

    public class PyTypeConvert
    {

    }
}
