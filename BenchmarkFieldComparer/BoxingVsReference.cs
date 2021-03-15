using BenchmarkDotNet.Attributes;
using System;
using System.Globalization;

namespace BenchmarkPlayground
{
    [MemoryDiagnoser]
    public class BoxingVsReference
    {
        private const int Docs = 1000;

        private string[] StringFieldCache = new string[Docs];

        protected PureValueScenario PureValueType;
        protected BoxingWrappedReferenceScenario ReferenceType;
        protected BoxingValueTypeScenario ValueType;
        

        // Stuff that is not relevant to the test. Your point that using wrappers here is slower is taken, but
        // this happens less frequently than the code under test and these fields are cached in the real app.
        [GlobalSetup]
        public void GlobalSetup()
        {
            PureValueType = new PureValueScenario();
            PureValueType.Int32FieldCache = new int[Docs];
            PureValueType.SingleFieldCache = new float[Docs];

            ValueType = new BoxingValueTypeScenario();
            ValueType.Int32FieldCache = new int[Docs];
            ValueType.SingleFieldCache = new float[Docs];

            ReferenceType = new BoxingWrappedReferenceScenario();
            ReferenceType.Int32FieldCache = new BoxingWrappedReferenceScenario.Int32[Docs];
            ReferenceType.SingleFieldCache = new BoxingWrappedReferenceScenario.Single[Docs];

            StringFieldCache = new string[Docs];

            for (int i = 0; i < Docs; i++)
            {
                PureValueType.Int32FieldCache[i] = i;
                PureValueType.SingleFieldCache[i] = i;

                ValueType.Int32FieldCache[i] = i;
                ValueType.SingleFieldCache[i] = i;

                ReferenceType.Int32FieldCache[i] = new BoxingWrappedReferenceScenario.Int32 { Value = i };
                ReferenceType.SingleFieldCache[i] = new BoxingWrappedReferenceScenario.Single { Value = i };

                StringFieldCache[i] = Convert.ToString(i, CultureInfo.InvariantCulture);
            }
        }

        public virtual void IterationSetup()
        {
            PureValueType.Comparers = new PureValueScenario.FieldComparer[3];
            PureValueType.Comparers[0] = new PureValueScenario.Int32Comparer(Docs, ValueType.Int32FieldCache);
            PureValueType.Comparers[1] = new PureValueScenario.SingleComparer(Docs, ValueType.SingleFieldCache);
            PureValueType.Comparers[2] = new PureValueScenario.StringComparer(Docs, StringFieldCache);


            ValueType.Comparers = new BoxingValueTypeScenario.FieldComparer[3];
            ValueType.Comparers[0] = new BoxingValueTypeScenario.Int32Comparer(Docs, ValueType.Int32FieldCache);
            ValueType.Comparers[1] = new BoxingValueTypeScenario.SingleComparer(Docs, ValueType.SingleFieldCache);
            ValueType.Comparers[2] = new BoxingValueTypeScenario.StringComparer(Docs, StringFieldCache);

            ReferenceType.Comparers = new BoxingWrappedReferenceScenario.FieldComparer[3];
            ReferenceType.Comparers[0] = new BoxingWrappedReferenceScenario.Int32Comparer(Docs, ReferenceType.Int32FieldCache);
            ReferenceType.Comparers[1] = new BoxingWrappedReferenceScenario.SingleComparer(Docs, ReferenceType.SingleFieldCache);
            ReferenceType.Comparers[2] = new BoxingWrappedReferenceScenario.StringComparer(Docs, StringFieldCache);
        }

        public void CopyValues()
        {

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < Docs; j++) {
                    if(PureValueType.Comparers[i] is PureValueScenario.Int32Comparer intComparer) {
                        intComparer.Copy(j, j);
                    } else if (PureValueType.Comparers[i] is PureValueScenario.SingleComparer singleComparer) {
                        singleComparer.Copy(j, j);
                    } else if (PureValueType.Comparers[i] is PureValueScenario.StringComparer stringComparer) {
                        stringComparer.Copy(j, j);
                    } else {
                        throw new Exception("Unexpected comparer type.");
                    }
                }

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < Docs; j++)
                    ValueType.Comparers[i].Copy(j, j);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < Docs; j++)
                    ReferenceType.Comparers[i].Copy(j, j);
        }

        public class Copy : BoxingVsReference
        {
            private const int Iterations = 10000;

            [IterationSetup]
            public override void IterationSetup()
            {
                base.IterationSetup();
            }

            [Benchmark]
            public void ValueType_Copy()
            {
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < Docs; j++)
                            ValueType.Comparers[i].Copy(j, j);
                }
            }

            [Benchmark]
            public void ReferenceType_Copy()
            {
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < Docs; j++)
                            ReferenceType.Comparers[i].Copy(j, j);
                }
            }
        }

        public class FillFields : BoxingVsReference
        {
            private const int Iterations = 10;

            [IterationSetup]
            public override void IterationSetup()
            {
                base.IterationSetup();

                CopyValues();
            }

            [Benchmark]
            public void ValueType_FillFields() // Similar to FieldValueHitQueue<T>.FillFields()
            {
                for (int iters = 0; iters < Iterations; iters++)
                {
                    var comparers = ValueType.Comparers;
                    int n = comparers.Length;
                    object[] fields = new object[n];

                    for (int slot = 0; slot < Docs; slot++)
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            var field = comparers[i][slot];
                            fields[i] = field;
                        }
                    }
                }
            }

            [Benchmark]
            public void ReferenceType_FillFields() // Similar to FieldValueHitQueue<T>.FillFields()
            {
                for (int iters = 0; iters < Iterations; iters++)
                {
                    var comparers = ReferenceType.Comparers;
                    int n = comparers.Length;
                    object[] fields = new object[n];

                    for (int slot = 0; slot < Docs; slot++)
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            var field = comparers[i].GetValue(slot);
                            fields[i] = field;
                        }
                    }
                }
            }
        }

        public class SetTopValue : BoxingVsReference
        {
            private const int Iterations = 10000000;

            private object[] ValueTypeFields;
            private object[] ReferenceTypeFields;

            [IterationSetup]
            public override void IterationSetup()
            {
                base.IterationSetup();

                CopyValues();

                {
                    var comparers = ValueType.Comparers;
                    int n = comparers.Length;
                    object[] fields = new object[n];

                    for (int slot = 0; slot < Docs; slot++)
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            var field = comparers[i][slot];
                            fields[i] = field;
                        }
                    }
                    ValueTypeFields = fields;
                }
                {
                    var comparers = ReferenceType.Comparers;
                    int n = comparers.Length;
                    object[] fields = new object[n];

                    for (int slot = 0; slot < Docs; slot++)
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            var field = comparers[i].GetValue(slot);
                            fields[i] = field;
                        }
                    }
                    ReferenceTypeFields = fields;
                }
            }

            [Benchmark]
            public void ValueType_SetTopValue()
            {
                for (int iters = 0; iters < Iterations; iters++)
                {
                    var comparers = ValueType.Comparers;

                    // Tell all comparers their top value:
                    for (int i = 0; i < comparers.Length; i++)
                    {
                        var comparer = comparers[i];
                        comparer.SetTopValue(ValueTypeFields[i]);
                    }
                }
            }

            [Benchmark]
            public void ReferenceType_SetTopValue()
            {
                for (int iters = 0; iters < Iterations; iters++)
                {
                    var comparers = ReferenceType.Comparers;

                    // Tell all comparers their top value:
                    for (int i = 0; i < comparers.Length; i++)
                    {
                        var comparer = comparers[i];
                        comparer.SetTopValue(ReferenceTypeFields[i]);
                    }
                }
            }
        }

        public class CastToNumber : BoxingVsReference
        {
            private const int Iterations = 100000;
            private const int Int32ComparerIndex = 0;
            private const int SingleComparerIndex = 1;

            [IterationSetup]
            public override void IterationSetup()
            {
                base.IterationSetup();

                CopyValues();
            }

            [Benchmark]
            public int[] CastComparer_PureInt32() {
                var result = new int[Docs];
                for (int iters = 0; iters < Iterations; iters++) {
                    for (int i = 0; i < Docs; i++) {
                        if (PureValueType.Comparers[Int32ComparerIndex] is PureValueScenario.Int32Comparer int32Comparer) {     //if it's known that this is the only type of comparer that will used in the loop, this cast to Int32Comparer can happen outside the loop.
                            result[i] = int32Comparer[i];
                        } else {
                            throw new Exception("Unexpected comparer type");
                        }
                    }
                }
                return result;
            }

            [Benchmark]
            public float[] CastComparer_PureFloat() {
                var result = new float[Docs];
                for (int iters = 0; iters < Iterations; iters++) {
                    for (int i = 0; i < Docs; i++) {
                        if (PureValueType.Comparers[SingleComparerIndex] is PureValueScenario.SingleComparer singleComparer) {      //if it's known that this is the only type of comparer that will used in the loop, this cast to SingleComparer can happen outside the loop.
                            result[i] = singleComparer[i];
                        } else {
                            throw new Exception("Unexpected comparer type");
                        }
                    }
                }
                return result;
            }

            [Benchmark]
            public int[] ValueType_CastToNumber_Int32()
            {
                var result = new int[Docs];
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < Docs; i++)
                    {
                        result[i] = (int)ValueType.Comparers[Int32ComparerIndex][i];
                    }
                }
                return result;
            }

            [Benchmark]
            public float[] ValueType_CastToNumber_Float()
            {
                var result = new float[Docs];
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < Docs; i++)
                    {
                        result[i] = (float)ValueType.Comparers[SingleComparerIndex][i];
                    }
                }
                return result;
            }

            [Benchmark]
            public int[] ReferenceType_CastToNumber_Int32_CastComparer()
            {
                var result = new int[Docs];
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < Docs; i++)
                    {
                        var comparerRaw = ReferenceType.Comparers[Int32ComparerIndex];
                        var comparer = (BoxingWrappedReferenceScenario.Int32Comparer)comparerRaw;
                        result[i] = comparer[i];
                    }
                }
                return result;
            }

            [Benchmark]
            public float[] ReferenceType_CastToNumber_Float_CastComparer()
            {
                var result = new float[Docs];
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < Docs; i++)
                    {
                        var comparerRaw = ReferenceType.Comparers[SingleComparerIndex];
                        var comparer = (BoxingWrappedReferenceScenario.SingleComparer)comparerRaw;
                        result[i] = comparer[i];
                    }
                }
                return result;
            }

            [Benchmark]
            public int[] ReferenceType_CastToNumber_Int32_IConvertible()
            {
                var result = new int[Docs];
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < Docs; i++)
                    {
                        result[i] = Convert.ToInt32(ReferenceType.Comparers[Int32ComparerIndex].GetValue(i));
                    }
                }
                return result;
            }

            [Benchmark]
            public float[] ReferenceType_CastToNumber_Float_IConvertible()
            {
                var result = new float[Docs];
                for (int iters = 0; iters < Iterations; iters++)
                {
                    for (int i = 0; i < Docs; i++)
                    {
                        result[i] = Convert.ToSingle(ReferenceType.Comparers[SingleComparerIndex].GetValue(i));
                    }
                }
                return result;
            }
        }
    }
}
