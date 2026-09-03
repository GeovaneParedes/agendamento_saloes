using AgendamentoApp.Models;
using FluentAssertions;
using Xunit;

namespace AgendamentoApp.UnitTests;

public class RegrasNegocioTests
{
    [Theory]
    [InlineData("2026-09-07", DayOfWeek.Monday, true)]   // Segunda-feira
    [InlineData("2026-09-08", DayOfWeek.Tuesday, true)]  // Terça-feira
    [InlineData("2026-09-09", DayOfWeek.Wednesday, true)]// Quarta-feira
    [InlineData("2026-09-10", DayOfWeek.Thursday, true)] // Quinta-feira
    [InlineData("2026-09-11", DayOfWeek.Friday, true)]   // Sexta-feira
    [InlineData("2026-09-12", DayOfWeek.Saturday, false)]// Sábado (Bloqueado)
    [InlineData("2026-09-13", DayOfWeek.Sunday, false)]  // Domingo (Bloqueado)
    public void SalaoNovaLima_DeveAceitarApenasSegundaASexta(string dataStr, DayOfWeek diaEsperado, bool deveSerValido)
    {
        // Arrange
        var data = DateTime.Parse(dataStr);
        data.DayOfWeek.Should().Be(diaEsperado);

        // Act
        var ehValido = data.DayOfWeek != DayOfWeek.Saturday && data.DayOfWeek != DayOfWeek.Sunday;

        // Assert
        ehValido.Should().Be(deveSerValido);
    }

    [Theory]
    [InlineData("2026-09-07", DayOfWeek.Monday, true)]   // Segunda-feira (Permitido)
    [InlineData("2026-09-08", DayOfWeek.Tuesday, false)]  // Terça-feira (Bloqueado)
    [InlineData("2026-09-09", DayOfWeek.Wednesday, false)]// Quarta-feira (Bloqueado)
    [InlineData("2026-09-10", DayOfWeek.Thursday, false)] // Quinta-feira (Bloqueado)
    [InlineData("2026-09-11", DayOfWeek.Friday, false)]   // Sexta-feira (Bloqueado)
    [InlineData("2026-09-12", DayOfWeek.Saturday, false)] // Sábado (Bloqueado)
    [InlineData("2026-09-13", DayOfWeek.Sunday, false)]   // Domingo (Bloqueado)
    public void SalaoMarquesHerval_DeveAceitarApenasSegundaFeira(string dataStr, DayOfWeek diaEsperado, bool deveSerValido)
    {
        // Arrange
        var data = DateTime.Parse(dataStr);
        data.DayOfWeek.Should().Be(diaEsperado);

        // Act
        var ehValido = data.DayOfWeek == DayOfWeek.Monday;

        // Assert
        ehValido.Should().Be(deveSerValido);
    }

    [Theory]
    [InlineData("Confidencial", true)]
    [InlineData("Ancião", true)]
    [InlineData("Treinamento", true)]
    [InlineData("OutraCategoria", false)]
    [InlineData("", false)]
    public void CategoriasReuniao_DevemSerValidas(string categoria, bool esperado)
    {
        // Arrange
        var categoriasPermitidas = new[] { "Confidencial", "Ancião", "Treinamento" };

        // Act
        var contem = categoriasPermitidas.Contains(categoria);

        // Assert
        contem.Should().Be(esperado);
    }

    [Fact]
    public void ModeloAgendamento_DeveInstanciarComValoresCorretos()
    {
        // Arrange & Act
        var agendamento = new Agendamento
        {
            Salao = "Salão Nova Lima",
            DataReserva = "2026-09-10",
            Congregacao = "Nova Lima",
            Categoria = "Treinamento",
            NomeSolicitante = "Irmão Teste",
            EmailSolicitante = "teste@exemplo.com"
        };

        // Assert
        agendamento.Salao.Should().Be("Salão Nova Lima");
        agendamento.DataReserva.Should().Be("2026-09-10");
        agendamento.Congregacao.Should().Be("Nova Lima");
        agendamento.Categoria.Should().Be("Treinamento");
        agendamento.NomeSolicitante.Should().Be("Irmão Teste");
        agendamento.EmailSolicitante.Should().Be("teste@exemplo.com");
    }
}
