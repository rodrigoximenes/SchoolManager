namespace SchoolManager.Domain.Common.Exceptions;

/// <summary>
/// Exceção de regra de negócio do domínio.
/// Lançada pelo Guard e pelos métodos de domínio.
/// Capturada pelo ExceptionMiddleware e convertida em HTTP 422.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    /// <summary>
    /// Atalho para validações de pré-condição inline.
    /// Ex: DomainException.When(!Ativo, "Turma inativa.")
    /// </summary>
    public static void When(bool hasError, string errorMessage)
    {
        if (hasError) throw new DomainException(errorMessage);
    }
}
