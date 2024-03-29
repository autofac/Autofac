﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CompanyB;

internal class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidatorSomeOtherName<T>
{
}
