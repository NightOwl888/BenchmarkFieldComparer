using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BenchmarkPlayground {

    [MemoryDiagnoser]
    public class Boxing{
        private const int Iterations = 100000;


        [Benchmark]
        public void NotBoxed_Int() {
            long total = 0;
            for (int i = 0; i < Iterations; i++) {
                total = total + (int)DoWorkNotBoxed_int(i);
            }
        }


        [Benchmark]
        public void NotBoxed_ValueType() {
            long total = 0;
            for (int i = 0; i < Iterations; i++) {
                total = total + (int)DoWorkNotBoxed_ValueType(i);
            }
        }


        [Benchmark]
        public void NotBoxed_IConvertable() {
            long total = 0;
            for (int i = 0; i < Iterations; i++) {
                total = total + DoWorkNotBoxed_IConvertible(i).ToInt32(null);
            }
        }


        [Benchmark]
        public void Boxed() {
            long total = 0;
            for (int i = 0; i < Iterations; i++) {
                total = total + (int)DoWorkBoxed(i);
            }
        }


        [Benchmark]
        public void HandWrapping() {
            long total = 0;
            IntWrap intWrap;
            for (int i = 0; i < Iterations; i++) {

                intWrap = new IntWrap(i);
                total = total + (int) DoWorkWrapped(intWrap);
            }
        }

        public int DoWorkNotBoxed_int(int value) {
            return value + 5;
        }

        public IConvertible DoWorkNotBoxed_IConvertible(IConvertible value) {
            return value.ToInt32(null) + 5;
        }

        public ValueType DoWorkNotBoxed_ValueType(ValueType value) {
            return (int)value + 5;
        }

        public object DoWorkBoxed(object value) {
            return (int)value + 5;
        }

        public object DoWorkWrapped(object value) {
            return ((IntWrap)value).Value + 5;
        }


        public class IntWrap {
            public int Value;

            public IntWrap(int value) {
                Value = value;
            }
            
        }

    }

}
