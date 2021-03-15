using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BenchmarkPlayground
{
    public class BoxingWrappedReferenceScenario
    {
        public Int32[] Int32FieldCache;
        public Single[] SingleFieldCache;

        public FieldComparer[] Comparers;
        public object[] Fields;

        public abstract class FieldComparer // This is simply to be able to use these APIs on all FieldComparer<T> closing types without knowing them
        {
            public abstract object GetValue(int slot);

            public abstract void SetTopValue(object value);

            public abstract void Copy(int slot, int doc);
        }

        public abstract class FieldComparer<T> : FieldComparer
            where T : class // Never allow value types here so we never box on these APIs or the APIs of FieldComparer, which uses object
        {
            protected T[] m_values;
            protected T m_topValue;

            protected FieldComparer(int numHits)
            {
                m_values = new T[numHits];
            }

            public override object GetValue(int slot)
            {
                return m_values[slot];
            }

            public override void SetTopValue(object value)
            {
                m_topValue = value as T; // Always a reference type, so casting is okay.
            }
        }

        public abstract class NumericComparer<TValue, TWrapper> : FieldComparer<TWrapper>
            where TWrapper : class, IConvertible // Allows the value to be retrived from the object simply by calling Convert.ToInt32(<the Number>)
            where TValue : struct
        {
            // currentReaderValues is an abstract represntation of the field cache. Since the field cache
            // is internal, we should change it to use wrapper types as well.
            protected TWrapper[] m_currentReaderValues;

            public NumericComparer(int numHits, TWrapper[] currentReaderValues)
                : base(numHits)
            {
                m_currentReaderValues = currentReaderValues;
            }

            // This overload is for end users who may attempt to call this with a numeric type
            public abstract void SetTopValue(TValue value);

            // Convenience method (replacement for GetValue(slot)) for getting the strongly typed value
            public abstract TValue this[int slot] { get; }

            public override void Copy(int slot, int doc)
            {
                var v2 = m_currentReaderValues[doc];

                m_values[slot] = v2;
            }
        }

        public class Int32Comparer : NumericComparer<int, Int32>
        {
            public Int32Comparer(int numHits, Int32[] currentReaderValues)
                : base(numHits, currentReaderValues)
            {
                // In Java, this is an int reference and there is an implicit conversion from Integer (class) to int (primitive).
                // But at the end of the day, it is initialized to the default value of int (0) and is never null.
                m_topValue = new Int32(); 
            }

            public override int this[int slot] => m_values[slot].Value;

            public override void SetTopValue(int value)
            {
                m_topValue.Value = value;
            }
        }

        public class SingleComparer : NumericComparer<float, Single>
        {
            public SingleComparer(int numHits, Single[] currentReaderValues)
                : base(numHits, currentReaderValues)
            {
                m_topValue = new Single();
            }

            public override float this[int slot] => m_values[slot].Value;

            public override void SetTopValue(float value)
            {
                m_topValue.Value = value;
            }
        }

        public class StringComparer : FieldComparer<string>
        {
            private readonly string[] currentReaderValues;

            public StringComparer(int numHits, string[] currentReaderValues)
                : base(numHits)
            {
                this.currentReaderValues = currentReaderValues;
            }

            public override void Copy(int slot, int doc)
            {
                var v2 = currentReaderValues[doc];

                m_values[slot] = v2;
            }
        }


        #region Number class types

        public abstract class Number : IConvertible
        {
            public abstract int ToInt32(IFormatProvider provider);
            public abstract float ToSingle(IFormatProvider provider);

            #region Not Implemented Members of IConvertible

            TypeCode IConvertible.GetTypeCode()
            {
                throw new NotImplementedException();
            }

            bool IConvertible.ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            byte IConvertible.ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            char IConvertible.ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            DateTime IConvertible.ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            decimal IConvertible.ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            double IConvertible.ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            short IConvertible.ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            long IConvertible.ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            sbyte IConvertible.ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            string IConvertible.ToString(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            object IConvertible.ToType(Type conversionType, IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            ushort IConvertible.ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            uint IConvertible.ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            ulong IConvertible.ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            #endregion Not Implemented Members of IConvertible
        }

        public class Int32 : Number
        {
            public int Value;

            public override int ToInt32(IFormatProvider provider)
            {
                return Value;
            }

            public override float ToSingle(IFormatProvider provider)
            {
                return Value;
            }
        }

        public class Single : Number
        {
            public float Value;

            public override int ToInt32(IFormatProvider provider)
            {
                return (int)Value;
            }

            public override float ToSingle(IFormatProvider provider)
            {
                return Value;
            }
        }

        # endregion Number class types
    }
}
