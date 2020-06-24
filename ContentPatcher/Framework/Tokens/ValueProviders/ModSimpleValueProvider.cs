using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider defined by a mod that which contains a set of values, and is considered unavailable when that set is empty.</summary>
    internal class ModSimpleValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>A function which returns the current value for a given input argument (if any).</summary>
        private readonly Func<IEnumerable<string>> GetValueImpl;

        /// <summary>The current values.</summary>
        private readonly InvariantHashSet Values = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="getValue">A function which returns the current token value (if any). If this returns null, the token is considered unavailable for use.</param>
        public ModSimpleValueProvider(string name, Func<IEnumerable<string>> getValue)
            : base(name, canHaveMultipleValuesForRoot: true)
        {
            this.GetValueImpl = getValue;

            this.EnableInputArguments(required: false, canHaveMultipleValues: false);
            this.MarkReady(false);
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (!this.IsReady)
                return Enumerable.Empty<string>();

            if (!input.IsMeaningful())
                return this.Values.ToArray();

            return new[] { this.Values.Contains(input.Value).ToString() };
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
        {
            if (input.IsMeaningful())
            {
                allowedValues = InvariantHashSet.Boolean();
                return true;
            }

            allowedValues = null;
            return false;
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Values, () =>
            {
                this.Values.Clear();
                foreach (string value in this.GetValueImpl() ?? Enumerable.Empty<string>())
                    this.Values.Add(value);

                this.MarkReady(this.Values.Any());
            });
        }
    }
}
