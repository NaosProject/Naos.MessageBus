// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedProperty.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Serialization;

    /// <summary>
    /// Model class to hold a single property to be shared.
    /// </summary>
    public class SharedProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharedProperty"/> class.
        /// </summary>
        /// <param name="name">Name of the property from the object.</param>
        /// <param name="serializedValue">Value of the property as a <see cref="DescribedSerializationBase" />.</param>
        public SharedProperty(string name, DescribedSerializationBase serializedValue)
        {
            new { name }.AsArg().Must().NotBeNullNorWhiteSpace();
            new { serializedValue }.AsArg().Must().NotBeNull();

            this.Name = name;
            this.SerializedValue = serializedValue;
        }

        /// <summary>
        /// Gets the name of the property from the object.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the property as a <see cref="DescribedSerializationBase" />.
        /// </summary>
        public DescribedSerializationBase SerializedValue { get; private set; }
    }
}
