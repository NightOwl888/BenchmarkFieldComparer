using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkPlayground
{
    // This uses the current design (although more basic)
    public class BoxingValueTypeScenario
    {
        public int[] Int32FieldCache;
        public float[] SingleFieldCache;

        public FieldComparer[] Comparers;
        public object[] Fields;

        public abstract class FieldComparer // This is simply to be able to use these APIs on all FieldComparer<T> closing types without knowing them
        {
            public abstract object this[int slot] { get; }

            public abstract void SetTopValue(object value);

            public abstract void Copy(int slot, int doc);
        }

        public abstract class FieldComparer<T> : FieldComparer
        {
        }

        public abstract class NumericComparer<T> : FieldComparer<T>
            where T : struct
        {
        }

        public sealed class Int32Comparer : NumericComparer<int>
        {
            private readonly int[] values;
            private int topValue;

            private int[] currentReaderValues;

            internal Int32Comparer(int numHits, int[] currentReaderValues)
            {
                values = new int[numHits];
                this.currentReaderValues = currentReaderValues;
            }

            public override object this[int slot] => values[slot];

            public override void Copy(int slot, int doc)
            {
                var v2 = currentReaderValues[doc];

                values[slot] = v2;
            }

            public override void SetTopValue(object value)
            {
                topValue = (int)value;
            }
        }

        public sealed class SingleComparer : NumericComparer<float>
        {
            private readonly float[] values;
            private float topValue;

            private float[] currentReaderValues;

            internal SingleComparer(int numHits, float[] currentReaderValues)
            {
                values = new float[numHits];
                this.currentReaderValues = currentReaderValues;
            }

            public override object this[int slot] => values[slot];

            public override void Copy(int slot, int doc)
            {
                var v2 = currentReaderValues[doc];

                values[slot] = v2;
            }

            public override void SetTopValue(object value)
            {
                topValue = (float)value;
            }
        }

        public class StringComparer : FieldComparer<string>
        {
            private readonly string[] values;
            private string topValue;

            private readonly string[] currentReaderValues;

            public override object this[int slot] => values[slot];

            public StringComparer(int numHits, string[] currentReaderValues)
            {
                this.values = new string[numHits];
                this.currentReaderValues = currentReaderValues;
            }

            public override void Copy(int slot, int doc)
            {
                var v2 = currentReaderValues[doc];

                values[slot] = v2;
            }

            public override void SetTopValue(object value)
            {
                topValue = (string)value;
            }
        }
    }
}
