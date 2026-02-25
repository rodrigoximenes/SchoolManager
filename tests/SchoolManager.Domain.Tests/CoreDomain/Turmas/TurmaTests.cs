using FluentAssertions;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.CoreDomain.Turmas;
using SchoolManager.Domain.CoreDomain.Turmas.Enums;
using SchoolManager.Domain.CoreDomain.Turmas.Events;
using Xunit;

namespace SchoolManager.Domain.Tests.CoreDomain.Turmas;

public sealed class TurmaTests
{
    // ── Criar ────────────────────────────────────────────────────────────────

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarTurmaAtiva()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);

        turma.Id.Should().NotBeEmpty();
        turma.Turno.Should().Be(Turno.Manha);
        turma.Periodo.Should().Be(2025);
        turma.Ativo.Should().BeTrue();
        turma.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Criar_DeveEmitirTurmaCriadaEvent()
    {
        var turma = Turma.Criar(Turno.Tarde, 2025);

        turma.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TurmaCriadaEvent>()
            .Which.TurmaId.Should().Be(turma.Id);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComPeriodoInvalido_DeveLancarDomainException(int periodoInvalido)
    {
        var act = () => Turma.Criar(Turno.Manha, periodoInvalido);

        act.Should().Throw<DomainException>()
            .WithMessage("*2000*2100*");
    }

    // ── AdicionarProfessor ────────────────────────────────────────────────────

    [Fact]
    public void AdicionarProfessor_EmTurmaAtiva_DeveAdicionarEEmitirEvento()
    {
        var turma       = Turma.Criar(Turno.Manha, 2025);
        var professorId = Guid.NewGuid();
        turma.ClearDomainEvents();

        turma.AdicionarProfessor(professorId);

        turma.Professores.Should().ContainSingle()
            .Which.ProfessorId.Should().Be(professorId);
        turma.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProfessorAdicionadoTurmaEvent>();
    }

    [Fact]
    public void AdicionarProfessor_MesmoProfessorDuasVezes_DeveSerIdempotente()
    {
        var turma       = Turma.Criar(Turno.Manha, 2025);
        var professorId = Guid.NewGuid();

        turma.AdicionarProfessor(professorId);
        turma.AdicionarProfessor(professorId);

        turma.Professores.Should().HaveCount(1);
    }

    [Fact]
    public void AdicionarProfessor_EmTurmaInativa_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);
        turma.Desativar();

        var act = () => turma.AdicionarProfessor(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*inativa*");
    }

    [Fact]
    public void AdicionarProfessor_ComGuidVazio_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);

        var act = () => turma.AdicionarProfessor(Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    // ── AdicionarAluno ────────────────────────────────────────────────────────

    [Fact]
    public void AdicionarAluno_EmTurmaAtiva_DeveAdicionarEEmitirEvento()
    {
        var turma   = Turma.Criar(Turno.Tarde, 2025);
        var alunoId = Guid.NewGuid();
        turma.ClearDomainEvents();

        turma.AdicionarAluno(alunoId);

        turma.Alunos.Should().ContainSingle()
            .Which.AlunoId.Should().Be(alunoId);
        turma.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AlunoAdicionadoTurmaEvent>();
    }

    [Fact]
    public void AdicionarAluno_EmTurmaInativa_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Tarde, 2025);
        turma.Desativar();

        var act = () => turma.AdicionarAluno(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*inativa*");
    }

    // ── RemoverAluno ──────────────────────────────────────────────────────────

    [Fact]
    public void RemoverAluno_AlunoExistente_DeveRemoverEEmitirEvento()
    {
        var turma   = Turma.Criar(Turno.Manha, 2025);
        var alunoId = Guid.NewGuid();
        turma.AdicionarAluno(alunoId);
        turma.ClearDomainEvents();

        turma.RemoverAluno(alunoId);

        turma.Alunos.Should().BeEmpty();
        turma.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AlunoRemovidoTurmaEvent>();
    }

    [Fact]
    public void RemoverAluno_AlunoNaoExistente_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);

        var act = () => turma.RemoverAluno(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*não encontrado*");
    }

    // ── Desativar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Desativar_TurmaAtiva_DeveDesativarEEmitirEvento()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);
        turma.ClearDomainEvents();

        turma.Desativar();

        turma.Ativo.Should().BeFalse();
        turma.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TurmaDesativadaEvent>();
    }

    [Fact]
    public void Desativar_TurmaJaInativa_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);
        turma.Desativar();

        var act = () => turma.Desativar();

        act.Should().Throw<DomainException>().WithMessage("*já está inativa*");
    }

    // ── Excluir ───────────────────────────────────────────────────────────────

    [Fact]
    public void Excluir_TurmaInativa_DeveMarcarSoftDeleteEEmitirEvento()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);
        turma.Desativar();
        turma.ClearDomainEvents();

        turma.Excluir();

        turma.IsDeleted.Should().BeTrue();
        turma.DataExclusao.Should().NotBeNull();
        turma.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TurmaExcluidaEvent>();
    }

    [Fact]
    public void Excluir_TurmaAindaAtiva_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);

        var act = () => turma.Excluir();

        act.Should().Throw<DomainException>().WithMessage("*Desative*");
    }

    [Fact]
    public void Excluir_TurmaJaExcluida_DeveLancarDomainException()
    {
        var turma = Turma.Criar(Turno.Manha, 2025);
        turma.Desativar();
        turma.Excluir();

        var act = () => turma.Excluir();

        act.Should().Throw<DomainException>().WithMessage("*já foi excluída*");
    }
}
