using FluentAssertions;
using NSubstitute;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Application.Commands.Turmas.CriarTurma;
using SchoolManager.Domain.CoreDomain.Turmas;
using SchoolManager.Domain.CoreDomain.Turmas.Enums;
using Xunit;

namespace SchoolManager.Application.Tests.Commands.Turmas;

public sealed class CriarTurmaCommandHandlerTests
{
    private readonly ITurmaRepository        _repo    = Substitute.For<ITurmaRepository>();
    private readonly CriarTurmaCommandHandler _handler;

    public CriarTurmaCommandHandlerTests()
        => _handler = new CriarTurmaCommandHandler(_repo);

    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveCriarEPersistirTurma()
    {
        // Arrange
        var command = new CriarTurmaCommand(Turno.Manha, 2025);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.TurmaId.Should().NotBeEmpty();
        result.Turno.Should().Be("Manha");
        result.Periodo.Should().Be(2025);
        result.Ativo.Should().BeTrue();

        await _repo.Received(1).AdicionarAsync(
            Arg.Is<Turma>(t => t.Turno == Turno.Manha && t.Periodo == 2025),
            Arg.Any<CancellationToken>());

        await _repo.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TurnoTarde_DeveRetornarTurnoCorreto()
    {
        var command = new CriarTurmaCommand(Turno.Tarde, 2024);

        var result = await _handler.HandleAsync(command);

        result.Turno.Should().Be("Tarde");
        result.Periodo.Should().Be(2024);
    }

    [Fact]
    public async Task HandleAsync_QuandoRepositorioFalha_DevePropagar()
    {
        _repo.SalvarAlteracoesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Banco indisponível.")));

        var command = new CriarTurmaCommand(Turno.Manha, 2025);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Banco indisponível*");
    }
}
