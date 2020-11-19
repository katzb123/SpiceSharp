﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.Resistors;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    [Pin(0, "R+"), Pin(1, "R-")]
    public partial class Resistor : Component<ComponentBindingContext>,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int ResistorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Resistor(string name)
            : base(name, ResistorPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="res">The resistance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or any node is <c>null</c>.</exception>
        public Resistor(string name, string pos, string neg, double res)
            : this(name)
        {
            Parameters.Resistance = res;
            Connect(pos, neg);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="model">The model name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or any node is <c>null</c>.</exception>
        public Resistor(string name, string pos, string neg, string model)
            : this(name)
        {
            Model = model;
            Connect(pos, neg);
        }
    }
}
