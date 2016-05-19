namespace CompanyA
{
    internal class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidator<T>
    {
    }
}
