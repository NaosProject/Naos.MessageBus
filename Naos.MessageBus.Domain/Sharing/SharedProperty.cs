// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedProperty.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using Naos.Serialization.Domain;

    using Spritely.Recipes;

    /// <summary>
    /// Model class to hold a single property to be shared.
    /// </summary>
    public class SharedProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharedProperty"/> class.
        /// </summary>
        /// <param name="name">Name of the property from the object.</param>
        /// <param name="serializedValue">Value of the property as a <see cref="DescribedSerialization" />.</param>
        public SharedProperty(string name, DescribedSerialization serializedValue)
        {
            new { name }.Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();
            new { serializedValue }.Must().NotBeNull().OrThrowFirstFailure();

            this.Name = name;
            this.SerializedValue = serializedValue;
        }

        /// <summary>
        /// Gets the name of the property from the object.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the property as a <see cref="DescribedSerialization" />.
        /// </summary>
        public DescribedSerialization SerializedValue { get; private set; }
    }
}