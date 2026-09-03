using AgendamentoApp.Models;
using AgendamentoApp.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Injeção de dependências
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// Garante que qualquer rota não encontrada retorne a página index.html
app.MapFallbackToFile("index.html");

// Endpoint de HealthCheck
app.MapGet("/health", () => Results.Text("{\"status\":\"healthy\"}", "application/json"));

// 1. Obter datas ocupadas para o salão selecionado
app.MapGet("/api/agendamentos/ocupados", async ([FromQuery] string salao, DatabaseService db) =>
{
    if (string.IsNullOrWhiteSpace(salao))
    {
        return Results.BadRequest(new { erro = "O parâmetro 'salao' é obrigatório." });
    }

    var datasOcupadas = await db.ObterDatasOcupadasAsync(salao);
    return Results.Ok(datasOcupadas);
});

// 2. Registrar novo agendamento
app.MapPost("/api/agendamentos", async ([FromBody] Agendamento agendamento, DatabaseService db, EmailService email, ILogger<Program> logger) =>
{
    // Validação básica dos campos
    if (string.IsNullOrWhiteSpace(agendamento.Salao) ||
        string.IsNullOrWhiteSpace(agendamento.DataReserva) ||
        string.IsNullOrWhiteSpace(agendamento.Congregacao) ||
        string.IsNullOrWhiteSpace(agendamento.Categoria) ||
        string.IsNullOrWhiteSpace(agendamento.NomeSolicitante) ||
        string.IsNullOrWhiteSpace(agendamento.EmailSolicitante))
    {
        logger.LogWarning("Campos recebidos incompletos: {@Agendamento}", agendamento);
        return Results.BadRequest(new { erro = "Todos os campos, incluindo nome e e-mail do solicitante, são obrigatórios." });
    }

    // Validação dos salões permitidos
    var saloesValidos = new[] { "Salão Nova Lima", "Salão Marques Herval" };
    if (!saloesValidos.Contains(agendamento.Salao))
    {
        return Results.BadRequest(new { erro = "Salão inválido selecionado." });
    }

    // Validação de categorias permitidas
    var categoriasValidas = new[] { "Confidencial", "Ancião", "Treinamento", "Troca / Visita" };
    if (!categoriasValidas.Contains(agendamento.Categoria))
    {
        return Results.BadRequest(new { erro = "Categoria de reunião inválida." });
    }

    // Validação das regras de dia da semana
    if (DateTime.TryParse(agendamento.DataReserva, out var data))
    {
        var diaSemana = data.DayOfWeek;
        if (agendamento.Salao == "Salão Nova Lima")
        {
            // Nova Lima: Segunda a Sexta (DayOfWeek: Monday=1 a Friday=5)
            if (diaSemana == DayOfWeek.Saturday || diaSemana == DayOfWeek.Sunday)
            {
                return Results.BadRequest(new { erro = "O Salão Nova Lima aceita reservas apenas de Segunda a Sexta-feira." });
            }
        }
        else if (agendamento.Salao == "Salão Marques Herval")
        {
            // Marques Herval: Apenas Segunda (DayOfWeek: Monday=1)
            if (diaSemana != DayOfWeek.Monday)
            {
                return Results.BadRequest(new { erro = "O Salão Marques Herval aceita reservas apenas às Segundas-feiras." });
            }
        }
    }
    else
    {
        return Results.BadRequest(new { erro = "Formato de data inválido. Use YYYY-MM-DD." });
    }

    // 1. Salva no PostgreSQL
    var (sucesso, mensagem) = await db.CriarAgendamentoAsync(agendamento);
    if (!sucesso)
    {
        return Results.Conflict(new { erro = mensagem });
    }

    // 2. Dispara e-mail de notificação em background para o Administrador
    _ = Task.Run(async () =>
    {
        try
        {
            await email.EnviarNotificacaoAgendamentoAsync(agendamento);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao disparar e-mail em background");
        }
    });

    return Results.Created($"/api/agendamentos/{agendamento.Id}", new
    {
        mensagem = "Reserva confirmada com sucesso!",
        agendamento
    });
});

app.Run();

public partial class Program { }
