// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedPropertyHelperTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public static class SharedPropertyHelperTest
    {
        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfComplexType_CanBeExtractedAndApplied()
        {
            var testHandler = new TestComplexShareHandler { ComplexShareObject = new ComplexShareObject("we did it!"), OtherProp = "monkey" };
            var testMessage = new TestComplexShareMessage();

            var sharedProperties = SharedPropertyHelper.GetSharedInterfaceStates(testHandler);
            var sharedPropertiesAsJson = sharedProperties.ToJson();
            var sharedPropertiesFromJson = sharedPropertiesAsJson.FromJson<IList<SharedInterfaceState>>();
            SharedPropertyHelper.ApplySharedInterfaceState(
                TypeMatchStrategy.NamespaceAndName,
                sharedPropertiesFromJson.Single(),
                testMessage);

            Assert.Equal(testHandler.ComplexShareObject.Prop, testMessage.ComplexShareObject.Prop);
            Assert.Equal(testHandler.OtherProp, testMessage.OtherProp);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfString_CanBeExtractedAndApplied()
        {
            var testHandler = new CopyFileHandler() { FilePath = "This should be set on the message" };
            var testMessage = new DeleteFileMessage();

            var sharedProperties = SharedPropertyHelper.GetSharedInterfaceStates(testHandler);
            var sharedPropertiesAsJson = sharedProperties.ToJson();
            var sharedPropertiesFromJson = sharedPropertiesAsJson.FromJson<IList<SharedInterfaceState>>();
            SharedPropertyHelper.ApplySharedInterfaceState(
                TypeMatchStrategy.NamespaceAndName,
                sharedPropertiesFromJson.Single(),
                testMessage);

            Assert.Equal(testHandler.FilePath, testMessage.FilePath);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfInt_CanBeExtractedAndApplied()
        {
            var testHandler = new CountHandler() { Count = 15 };
            var testMessage = new CountMessage();

            var sharedProperties = SharedPropertyHelper.GetSharedInterfaceStates(testHandler);
            var sharedPropertiesAsJson = sharedProperties.ToJson();
            var sharedPropertiesFromJson = sharedPropertiesAsJson.FromJson<IList<SharedInterfaceState>>();
            SharedPropertyHelper.ApplySharedInterfaceState(
                TypeMatchStrategy.NamespaceAndName,
                sharedPropertiesFromJson.Single(),
                testMessage);

            Assert.Equal(testHandler.Count, testMessage.Count);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfEnum_CanBeExtractedAndApplied()
        {
            var testHandler = new FirstEnumHandler() { EnumValueToShare = MyEnum.OtherOtherValue };
            var testMessage = new SecondEnumMessage();

            var sharedProperties = SharedPropertyHelper.GetSharedInterfaceStates(testHandler);
            var sharedPropertiesAsJson = sharedProperties.ToJson();
            var sharedPropertiesFromJson = sharedPropertiesAsJson.FromJson<IList<SharedInterfaceState>>();
            SharedPropertyHelper.ApplySharedInterfaceState(
                TypeMatchStrategy.NamespaceAndName,
                sharedPropertiesFromJson.Single(),
                testMessage);

            Assert.Equal(testHandler.EnumValueToShare, testMessage.EnumValueToShare);
        }

        [Fact]
        public static void GetSharedPropertySetFromShareObject_SourceNull_Throws()
        {
            // arrange
            Action testCode = () => SharedPropertyHelper.GetSharedInterfaceStates(null);

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("SourceObject can not be null");
        }

        [Fact]
        public static void ApplySharedPropertySetToShareObject_InvalidType_Throws()
        {
            // arrange
            Action testCode = () =>
                {
                    var sharedPropertyEntry = new SharedProperty
                                                  {
                                                      Name = "FilePath",
                                                      ValueAsJson = "Value",
                                                      ValueType =
                                                          new TypeDescription
                                                              {
                                                                  AssemblyQualifiedName
                                                                      = "NotReal",
                                                                  Namespace =
                                                                      "NotReal",
                                                                  Name = "NotReal"
                                                              }
                                                  };

                    SharedPropertyHelper.ApplySharedInterfaceState(
                        TypeMatchStrategy.NamespaceAndName,
                        new SharedInterfaceState
                            {
                                InterfaceType = typeof(IShareFilePath).ToTypeDescription(),
                                Properties =
                                    new[]
                                        {
                                            sharedPropertyEntry
                                        }
                            },
                        new CopyFileMessage());
                };

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage("Can not find loaded type; Namespace: NotReal, Name: NotReal, AssemblyQualifiedName: NotReal");
        }

        [Fact]
        public static void ApplySharedPropertySetToShareObject_TargetNull_Throws()
        {
            // arrange
            Action testCode = () =>
            {
                SharedPropertyHelper.ApplySharedInterfaceState(TypeMatchStrategy.NamespaceAndName, new SharedInterfaceState(), null);
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither targetObject nor propertySet can be null");
        }

        [Fact]
        public static void ApplySharedPropertySetToShareObject_PropertySetNull_Throws()
        {
            // arrange
            Action testCode = () =>
            {
                SharedPropertyHelper.ApplySharedInterfaceState(TypeMatchStrategy.NamespaceAndName, null, new CopyFileMessage());
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither targetObject nor propertySet can be null");
        }

        [Fact]
        public static void ApplySharedProperties_Source_Throws()
        {
            // arrange
            Action testCode = () =>
            {
                SharedPropertyHelper.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, null, new CopyFileMessage());
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither source nor target can be null");
        }

        [Fact]
        public static void ApplySharedProperties_Target_Throws()
        {
            // arrange
            Action testCode = () =>
            {
                SharedPropertyHelper.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, null, new CopyFileMessage());
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither source nor target can be null");
        }

        [Fact]
        public static void ApplySharedProperties_ValidMatch_PropertiesSet()
        {
            var testHandler = new CopyFileHandler() { FilePath = "This should be set on the message" };
            var testMessage = new DeleteFileMessage();

            Assert.NotEqual(testHandler.FilePath, testMessage.FilePath);
            SharedPropertyHelper.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, testHandler, testMessage);
            Assert.Equal(testHandler.FilePath, testMessage.FilePath);
        }

        [Fact]
        public static void ApplySharedProperties_ValidMatchOnEnum_PropertiesSet()
        {
            var testHandler = new FirstEnumHandler { EnumValueToShare = MyEnum.OtherValue };
            var testMessage = new SecondEnumMessage();

            Assert.NotEqual(testHandler.EnumValueToShare, testMessage.EnumValueToShare);
            SharedPropertyHelper.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, testHandler, testMessage);
            Assert.Equal(testHandler.EnumValueToShare, testMessage.EnumValueToShare);
        }
    }

    public class ComplexShareObject
    {
        public ComplexShareObject(string prop)
        {
            this.Prop = prop;
        }

        public string Prop { get; set; }
    }

    public interface IShareComplexType : IShare
    {
        ComplexShareObject ComplexShareObject { get; set; }

        string OtherProp { get; set; }
    }

    public class TestComplexShareMessage : IShareComplexType
    {
        public ComplexShareObject ComplexShareObject { get; set; }

        public string OtherProp { get; set; }
    }

    public class TestComplexShareHandler : IShareComplexType
    {
        public ComplexShareObject ComplexShareObject { get; set; }

        public string OtherProp { get; set; }
    }

    public class FirstEnumHandler : IHandleMessages<FirstEnumMessage>, IShareEnum
    {
        public async Task HandleAsync(FirstEnumMessage message)
        {
            this.EnumValueToShare = await Task.FromResult(message.SeedValue);
        }

        public MyEnum EnumValueToShare { get; set; }
    }

    public class SecondEnumHandler : IHandleMessages<SecondEnumMessage>, IShareEnum
    {
        public async Task HandleAsync(SecondEnumMessage message)
        {
            this.EnumValueToShare = await Task.FromResult(this.EnumValueToSet);
            if (this.SetFromMessageInsteadOfToSet)
            {
                this.EnumValueToShare = message.EnumValueToShare;
            }
        }

        public MyEnum EnumValueToShare { get; set; }

        public MyEnum EnumValueToSet { get; set; }

        public bool SetFromMessageInsteadOfToSet { get; set; }
    }

    public class FirstEnumMessage : IMessage
    {
        public string Description { get; set; }

        public MyEnum EnumValueToTest { get; set; }

        public MyEnum SeedValue { get; set; }
    }

    public class SecondEnumMessage : IMessage, IShareEnum
    {
        public string Description { get; set; }

        public MyEnum EnumValueToShare { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Spelling/name is correct.")]
    public enum MyEnum
    {
        /// <summary>
        /// Test value.
        /// </summary>
        ShouldNotGet,

        /// <summary>
        /// Test value.
        /// </summary>
        OtherValue,

        /// <summary>
        /// Test value.
        /// </summary>
        OtherOtherValue
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Spelling/name is correct.")]
    public interface IShareEnum : IShare
    {
        MyEnum EnumValueToShare { get; set; }
    }

    public class CopyFileHandler : IHandleMessages<CopyFileMessage>, IShareFilePath
    {
        public string FilePath { get; set; }

        public async Task HandleAsync(CopyFileMessage message)
        {
            await Task.Run(() => File.Copy(message.FilePath, "new file path"));
            this.FilePath = message.FilePath;
        }
    }

    public class CopyFileMessage : IMessage, IShareFilePath
    {
        public string FilePath { get; set; }

        public string Description { get; set; }
    }

    public class DeleteFileMessage : IMessage, IShareFilePath
    {
        public string FilePath { get; set; }

        public string Description { get; set; }
    }

    public interface IShareFilePath : IShare
    {
        string FilePath { get; set; }
    }

    public class CountMessage : IMessage, IShareCount
    {
        public string Description { get; set; }

        public int Count { get; set; }
    }

    public class CountHandler : IHandleMessages<CountMessage>, IShareCount
    {
        public int Count { get; set; }

        public async Task HandleAsync(CountMessage message)
        {
            this.Count = await Task.FromResult(message.Count);
        }
    }

    public interface IShareCount : IShare
    {
        int Count { get; set; }
    }
}
