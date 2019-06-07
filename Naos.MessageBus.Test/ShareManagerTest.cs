// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareManagerTest.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Naos.Compression.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.Serialization.Domain;
    using Naos.Serialization.Factory;

    using OBeautifulCode.Type;

    using Xunit;

    public static class ShareManagerTest
    {
        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfIntArray_CanBeExtractedAndApplied()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new ShareArrayOfIntHandler { IntArray = new[] { 1, 2, 3 } };
            var testMessage = new ShareArrayOfIntMessage();

            var sharedProperties = shareManager.GetSharedInterfaceStates(testHandler, PostOffice.MessageSerializationDescription.ConfigurationTypeDescription);
            shareManager.ApplySharedInterfaceState(sharedProperties.Single(), testMessage);

            Assert.Equal(testHandler.IntArray, testMessage.IntArray);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfComplexType_CanBeExtractedAndApplied()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new TestComplexShareHandler { ComplexShareObject = new ComplexShareObject("we did it!"), OtherProp = "monkey" };
            var testMessage = new TestComplexShareMessage();

            var sharedProperties = shareManager.GetSharedInterfaceStates(testHandler, PostOffice.MessageSerializationDescription.ConfigurationTypeDescription);
            shareManager.ApplySharedInterfaceState(sharedProperties.Single(), testMessage);

            Assert.Equal(testHandler.ComplexShareObject.Prop, testMessage.ComplexShareObject.Prop);
            Assert.Equal(testHandler.OtherProp, testMessage.OtherProp);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfString_CanBeExtractedAndApplied()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new CopyFileHandler() { FilePath = "This should be set on the message" };
            var testMessage = new DeleteFileMessage();

            var sharedProperties = shareManager.GetSharedInterfaceStates(testHandler, PostOffice.MessageSerializationDescription.ConfigurationTypeDescription);
            shareManager.ApplySharedInterfaceState(sharedProperties.Single(), testMessage);

            Assert.Equal(testHandler.FilePath, testMessage.FilePath);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfInt_CanBeExtractedAndApplied()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new CountHandler() { Count = 15 };
            var testMessage = new CountMessage();

            var sharedProperties = shareManager.GetSharedInterfaceStates(testHandler, PostOffice.MessageSerializationDescription.ConfigurationTypeDescription);
            shareManager.ApplySharedInterfaceState(sharedProperties.Single(), testMessage);

            Assert.Equal(testHandler.Count, testMessage.Count);
        }

        [Fact]
        public static void GetAndApplySharedPropertySet_ShareContractOfEnum_CanBeExtractedAndApplied()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new FirstEnumHandler() { EnumValueToShare = MyEnum.OtherOtherValue };
            var testMessage = new SecondEnumMessage();

            var sharedProperties = shareManager.GetSharedInterfaceStates(testHandler, PostOffice.MessageSerializationDescription.ConfigurationTypeDescription);
            shareManager.ApplySharedInterfaceState(sharedProperties.Single(), testMessage);

            Assert.Equal(testHandler.EnumValueToShare, testMessage.EnumValueToShare);
        }

        [Fact]
        public static void GetSharedPropertySetFromShareObject_SourceNull_Throws()
        {
            // Arrange
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            Action testCode = () => shareManager.GetSharedInterfaceStates(null, PostOffice.MessageSerializationDescription.ConfigurationTypeDescription);

            // Act & Assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("objectToShareFrom can not be null");
        }

        [Fact]
        public static void ApplySharedPropertySetToShareObject_InvalidType_Throws()
        {
            // arrange
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            Action testCode = () =>
                {
                    var sharedPropertyEntry = new SharedProperty(
                        "FilePath",
                        new DescribedSerialization(
                            new TypeDescription() { AssemblyQualifiedName = "NotReal", Namespace = "NotReal", Name = "NotReal" },
                            "Value",
                            ShareManager.SharedPropertySerializationDescription));

                    shareManager.ApplySharedInterfaceState(
                        new SharedInterfaceState { InterfaceType = typeof(IShareFilePath).ToTypeDescription(), Properties = new[] { sharedPropertyEntry } },
                        new CopyFileMessage());
                };

            // act & assert
            testCode.ShouldThrow<ArgumentNullException>().WithMessage("Parameter 'type' is null.");
        }

        [Fact]
        public static void ApplySharedPropertySetToShareObject_TargetNull_Throws()
        {
            // arrange
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            Action testCode = () =>
            {
                shareManager.ApplySharedInterfaceState(new SharedInterfaceState(), null);
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither sharedPropertiesFromAnotherShareObject nor objectToShareTo can be null");
        }

        [Fact]
        public static void ApplySharedPropertySetToShareObject_PropertySetNull_Throws()
        {
            // arrange
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            Action testCode = () =>
            {
                shareManager.ApplySharedInterfaceState(null, new CopyFileMessage());
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither sharedPropertiesFromAnotherShareObject nor objectToShareTo can be null");
        }

        [Fact]
        public static void ApplySharedProperties_Source_Throws()
        {
            // arrange
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            Action testCode = () =>
            {
                shareManager.ApplySharedProperties(null, new CopyFileMessage());
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither source nor target can be null");
        }

        [Fact]
        public static void ApplySharedProperties_Target_Throws()
        {
            // arrange
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            Action testCode = () =>
            {
                shareManager.ApplySharedProperties(null, new CopyFileMessage());
            };

            // act & assert
            testCode.ShouldThrow<SharePropertyException>().WithMessage("Neither source nor target can be null");
        }

        [Fact]
        public static void ApplySharedProperties_ValidMatch_PropertiesSet()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new CopyFileHandler() { FilePath = "This should be set on the message" };
            var testMessage = new DeleteFileMessage();

            Assert.NotEqual(testHandler.FilePath, testMessage.FilePath);
            shareManager.ApplySharedProperties(testHandler, testMessage);
            Assert.Equal(testHandler.FilePath, testMessage.FilePath);
        }

        [Fact]
        public static void ApplySharedProperties_ValidMatchOnEnum_PropertiesSet()
        {
            var shareManager = new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
            var testHandler = new FirstEnumHandler { EnumValueToShare = MyEnum.OtherValue };
            var testMessage = new SecondEnumMessage();

            Assert.NotEqual(testHandler.EnumValueToShare, testMessage.EnumValueToShare);
            shareManager.ApplySharedProperties(testHandler, testMessage);
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

    public class FirstEnumHandler : MessageHandlerBase<FirstEnumMessage>, IShareEnum
    {
        public override async Task HandleAsync(FirstEnumMessage message)
        {
            this.EnumValueToShare = await Task.FromResult(message.SeedValue);
        }

        public MyEnum EnumValueToShare { get; set; }
    }

    public class SecondEnumHandler : MessageHandlerBase<SecondEnumMessage>, IShareEnum
    {
        public override async Task HandleAsync(SecondEnumMessage message)
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
        OtherOtherValue,
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Spelling/name is correct.")]
    public interface IShareEnum : IShare
    {
        MyEnum EnumValueToShare { get; set; }
    }

    public interface IShareArrayOfInt : IShare
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Testing this case explicitly.")]
        int[] IntArray { get; set; }
    }

    public class ShareArrayOfIntHandler : MessageHandlerBase<ShareArrayOfIntMessage>, IShareArrayOfInt
    {
        public int[] IntArray { get; set; }

        public override async Task HandleAsync(ShareArrayOfIntMessage message)
        {
            await Task.Run(() => { });
        }
    }

    public class ShareArrayOfIntMessage : IMessage, IShareArrayOfInt
    {
        public string Description { get; set; }

        public int[] IntArray { get; set; }
    }

    public class CopyFileHandler : MessageHandlerBase<CopyFileMessage>, IShareFilePath
    {
        public string FilePath { get; set; }

        public override async Task HandleAsync(CopyFileMessage message)
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

    public class CountHandler : MessageHandlerBase<CountMessage>, IShareCount
    {
        public int Count { get; set; }

        public override async Task HandleAsync(CountMessage message)
        {
            this.Count = await Task.FromResult(message.Count);
        }
    }

    public interface IShareCount : IShare
    {
        int Count { get; set; }
    }
}
