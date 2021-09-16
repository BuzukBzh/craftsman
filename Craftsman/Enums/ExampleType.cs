﻿namespace Craftsman.Enums
{
    using System;
    using Ardalis.SmartEnum;
    
    public abstract class ExampleType : SmartEnum<ExampleType>
    {
        public static readonly ExampleType Basic = new BasicType();
        public static readonly ExampleType WithAuth = new WithAuthType();

        protected ExampleType(string name, int value) : base(name, value)
        {
        }
        
        private class BasicType : ExampleType
        {
            public BasicType() : base(nameof(Basic), 1) {}
        }

        private class WithAuthType : ExampleType
        {
            public WithAuthType() : base(nameof(WithAuth), 2) {}
        }
    }
}