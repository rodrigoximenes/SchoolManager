using SchoolManager.Domain.Common.Exceptions;

namespace SchoolManager.Domain.Common.Validations;

/// <summary>
/// Utilitário de validação de pré-condições nos construtores e métodos de domínio.
/// Todos os métodos lançam DomainException — nunca ArgumentException ou similares.
/// Uso: Guard.AgainstNull(valor, nameof(valor), "mensagem opcional");
/// </summary>
internal static class Guard
{
    public static void AgainstEmptyGuid(Guid id, string paramName, string? message = null)
    {
        if (id == Guid.Empty)
            throw new DomainException(message ?? $"{paramName} não pode ser Guid vazio.");
    }

    public static void AgainstNull<T>(T? value, string paramName, string? message = null)
    {
        if (value is null)
            throw new DomainException(message ?? $"{paramName} não pode ser nulo.");
    }

    public static void AgainstNullOrWhiteSpace(string? value, string paramName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(message ?? $"{paramName} não pode ser nulo ou vazio.");
    }

    public static void AgainstNegativeOrZero(decimal value, string paramName, string? message = null)
    {
        if (value <= 0)
            throw new DomainException(message ?? $"{paramName} deve ser maior que zero.");
    }

    public static void AgainstOutOfRange(int value, int min, int max, string paramName, string? message = null)
    {
        if (value < min || value > max)
            throw new DomainException(message ?? $"{paramName} deve estar entre {min} e {max}.");
    }

    /// <summary>
    /// Lança TException se a condição for verdadeira.
    /// Ex: Guard.Against&lt;DomainException&gt;(!Ativo, "Turma inativa.")
    /// </summary>
    public static void Against<TException>(bool condition, string message)
        where TException : Exception
    {
        if (condition)
            throw (TException)Activator.CreateInstance(typeof(TException), message)!;
    }
}
