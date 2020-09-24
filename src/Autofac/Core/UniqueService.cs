// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core
{
    /// <summary>
    /// A handy unique service identifier type - all instances will be regarded as unequal.
    /// </summary>
    public sealed class UniqueService : Service
    {
        private readonly Guid _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueService"/> class.
        /// </summary>
        public UniqueService()
            : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueService"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public UniqueService(Guid id)
        {
            _id = id;
        }

        /// <summary>
        /// Gets a programmer-readable description of the identifying feature of the service.
        /// </summary>
        public override string Description => _id.ToString();

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="object"/> is equal to the current <see cref="object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            var that = obj as UniqueService;

            return that != null && _id == that._id;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
