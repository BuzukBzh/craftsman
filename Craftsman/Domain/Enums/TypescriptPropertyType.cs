﻿namespace Craftsman.Domain.Enums;

using System;
using Ardalis.SmartEnum;
using Helpers;

public abstract class TypescriptPropertyType : SmartEnum<TypescriptPropertyType>
{
    public static readonly TypescriptPropertyType StringProperty = new StringPropertyType();
    public static readonly TypescriptPropertyType BooleanProperty = new BooleanPropertyType();
    public static readonly TypescriptPropertyType DateProperty = new DatePropertyType();
    public static readonly TypescriptPropertyType NumberProperty = new NumberPropertyType();
    public static readonly TypescriptPropertyType Other = new OtherType();
    public static readonly TypescriptPropertyType NullableStringProperty = new StringPropertyType(true);
    public static readonly TypescriptPropertyType NullableBooleanProperty = new BooleanPropertyType(true);
    public static readonly TypescriptPropertyType NullableDateProperty = new DatePropertyType(true);
    public static readonly TypescriptPropertyType NullableNumberProperty = new NumberPropertyType(true);
    public static readonly TypescriptPropertyType NullableOther = new OtherType(true);

    private readonly bool _isNullable;
    protected TypescriptPropertyType(string name, int value, bool isNullable = false) : base(name, value)
    {
        _isNullable = isNullable;
    }
    public abstract string TypescriptPropType();
    public abstract string FormDefaultValue(string propName);
    public abstract string YupValidation(string propName);
    public abstract string FormControl(string propName, string label, string validationSchema);
    public abstract string FormSetValue(string propName, string entityName);

    private class StringPropertyType : TypescriptPropertyType
    {
        public StringPropertyType(bool isNullable = false) : base(nameof(StringProperty), 1, isNullable) { }

        public override string TypescriptPropType()
            => _isNullable ? "string?" : "string";
            
        public override string YupValidation(string propName)
            => _isNullable 
                ? @$"
  {propName.LowercaseFirstLetter()}: yup.string().nullable()," 
                : @$"
  {propName.LowercaseFirstLetter()}: yup.string(),";

        public override string FormDefaultValue(string propName) 
            => @$"
      {propName.LowercaseFirstLetter()}: """",";

        public override string FormControl(string propName, string label, string validationSchema)
        {
            var lowerFirst = propName.LowercaseFirstLetter();
            var upperFirst = propName.UppercaseFirstLetter();
            return $@"

        <div className=""w-full sm:w-80 lg:w-96"">
          <Controller
            name=""{lowerFirst}""
            control={{control}}
            render={{({{ field, fieldState }}) => (
              <TextInput
                label={{""{label}""}}
                placeholder=""{upperFirst}...""
                testSelector=""{lowerFirst}""
                required={{
                  // @ts-ignore
                  {validationSchema}.fields?.{lowerFirst}
                    ?.exclusiveTests?.required
                }}
                error={{fieldState.error?.message}}
                {{...field}}
              />
            )}}
          />
        </div>

        {{/* OR use a TextArea... */}}

        <div className=""w-full sm:w-80 lg:w-96"">
          <Controller
            name=""{lowerFirst}""
            control={{control}}
            render={{({{ field, fieldState }}) => (
              <TextArea
                {{...field}}
                label={{""{label}""}}
                placeholder=""{upperFirst}...""
                testSelector=""{lowerFirst}""
                minRows={{2}}
                autosize
                resize=""y""
                required={{
                  // @ts-ignore
                  {validationSchema}.fields?.{lowerFirst}?.exclusiveTests
                    ?.required
                }}
                error={{fieldState.error?.message}}
              />
            )}}
          />
        </div>";    
        }

        public override string FormSetValue(string propName, string entityName) 
            => @$"
      setValue(""{propName.LowercaseFirstLetter()}"", {entityName.LowercaseFirstLetter()}Data?.{propName.LowercaseFirstLetter()} ?? """");";
    }

    private class BooleanPropertyType : TypescriptPropertyType
    {
        public BooleanPropertyType(bool isNullable = false) : base(nameof(BooleanProperty), 2, isNullable) { }

        public override string TypescriptPropType()
            => _isNullable ? "boolean?" : "boolean";
            
        public override string YupValidation(string propName)
            => _isNullable 
                ? @$"
  {propName.LowercaseFirstLetter()}: yup.boolean().nullable()," 
                : @$"
  {propName.LowercaseFirstLetter()}: yup.boolean(),";

        public override string FormDefaultValue(string propName) 
            => @$"
      {propName.LowercaseFirstLetter()}: false,";

        public override string FormControl(string propName, string label, string validationSchema)
        {
            var lowerFirst = propName.LowercaseFirstLetter();
            
            return $@"

        <div className=""w-full sm:w-80 lg:w-96"">
          <Controller
            name=""{lowerFirst}""
            control={{control}}
            render={{({{ field, fieldState }}) => (
              <Checkbox
                label={{""{label}""}}
                testSelector=""{lowerFirst}""
                required={{
                  {validationSchema}.fields?.{lowerFirst} // @ts-ignore
                    ?.exclusiveTests?.required
                }}
                isSelected={{field.value}}
                error={{fieldState?.error?.message}}
                {{...field}}
              />
            )}}
          />
        </div>";    
        }

        public override string FormSetValue(string propName, string entityName) 
            => @$"
      setValue(""{propName.LowercaseFirstLetter()}"", {entityName.LowercaseFirstLetter()}Data?.{propName.LowercaseFirstLetter()});";
    }

    private class DatePropertyType : TypescriptPropertyType
    {
        public DatePropertyType(bool isNullable = false) : base(nameof(DateProperty), 3, isNullable) { }

        public override string TypescriptPropType()
            => _isNullable ? "Date?" : "Date";
            
        public override string YupValidation(string propName)
            => _isNullable 
                ? @$"
  {propName.LowercaseFirstLetter()}: yup.date().nullable()," 
                : @$"
  {propName.LowercaseFirstLetter()}: yup.date(),";

        public override string FormDefaultValue(string propName) 
            => @$"
      // @ts-ignore -- need default value to reset form
      {propName.LowercaseFirstLetter()}: null,";
        
        public override string FormControl(string propName, string label, string validationSchema)
        {
            var lowerFirst = propName.LowercaseFirstLetter();
            
            return $@"

        <div className=""w-full sm:w-80 lg:w-96"">
          <Controller
            name=""{lowerFirst}""
            control={{control}}
            render={{({{ field, fieldState }}) => (
              <DatePicker
                {{...field}}
                label={{""{label}""}}
                placeholder=""Pick a date""
                testSelector=""{lowerFirst}""
                withAsterisk={{
                  // @ts-ignore
                  {validationSchema}.fields?.{lowerFirst}?.exclusiveTests
                    ?.required
                }}
                required={{
                  // @ts-ignore
                  {validationSchema}.fields?.{lowerFirst}?.exclusiveTests
                    ?.required
                }}
                error={{fieldState.error?.message}}
              />
            )}}
          />
        </div>";    
        }

        public override string FormSetValue(string propName, string entityName) 
            => @$"
      setValue(""{propName.LowercaseFirstLetter()}"", {entityName.LowercaseFirstLetter()}Data?.{propName.LowercaseFirstLetter()});";
    }

    private class NumberPropertyType : TypescriptPropertyType
    {
        public NumberPropertyType(bool isNullable = false) : base(nameof(NumberProperty), 4, isNullable) { }

        public override string TypescriptPropType()
            => _isNullable ? "number?" : "number";
            
        public override string YupValidation(string propName)
            => _isNullable 
                ? @$"
  {propName.LowercaseFirstLetter()}: yup.number().nullable()," 
                : @$"
  {propName.LowercaseFirstLetter()}: yup.number(),";

        public override string FormDefaultValue(string propName) 
            => @$"
      //TODO possible default for {propName}";
        
        public override string FormControl(string propName, string label, string validationSchema)
        {
            var lowerFirst = propName.LowercaseFirstLetter();
            var upperFirst = propName.UppercaseFirstLetter();
            
            return $@"

        <div className=""w-full sm:w-80 lg:w-96"">
          <Controller
            name=""{lowerFirst}""
            control={{control}}
            render={{({{ field, fieldState }}) => (
              <NumberInput
                {{...field}}
                label={{""{label}""}}
                placeholder=""{upperFirst}...""
                testSelector=""{lowerFirst}""
                required={{
                  // @ts-ignore
                  {validationSchema}.fields?.{lowerFirst}?.exclusiveTests
                    ?.required
                }}
                error={{fieldState.error?.message}}
              />
            )}}
          />
        </div>";    
        }

        public override string FormSetValue(string propName, string entityName) 
            => @$"
      setValue(""{propName.LowercaseFirstLetter()}"", {entityName.LowercaseFirstLetter()}Data?.{propName.LowercaseFirstLetter()} ?? false);";
    }


    private class OtherType : TypescriptPropertyType
    {
        public OtherType(bool isNullable = false) : base(nameof(Other), 5, isNullable) { }

        public override string TypescriptPropType()
            => _isNullable ? "unknown?" : "unknown";
            
        public override string YupValidation(string propName)
            => _isNullable ? "unknown?" 
                : @$"
  // TODO possible validation for {propName}";

        public override string FormDefaultValue(string propName) 
            => @$"
      // TODO possible default for {propName}";

        public override string FormControl(string propName, string label, string validationSchema)
            => $@"

        {{/* TODO form control for {propName} */}}";

        public override string FormSetValue(string propName, string entityName) 
            => @$"
      // TODO possibly need setValue for {propName}";

    }
}