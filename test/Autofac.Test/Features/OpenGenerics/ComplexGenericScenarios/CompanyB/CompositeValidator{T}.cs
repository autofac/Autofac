namespace CompanyB
{
    internal class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidatorSomeOtherName<T>
    {
    }
}
