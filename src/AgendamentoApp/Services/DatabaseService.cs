using AgendamentoApp.Models;
using Npgsql;

namespace AgendamentoApp.Services;

public class DatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=127.0.0.1;Port=5432;Database=agendamentos_db;Username=devgege;";
    }

    public async Task<List<string>> ObterDatasOcupadasAsync(string salao)
    {
        var datas = new List<string>();
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = "SELECT data_reserva FROM agendamentos WHERE salao = @salao ORDER BY data_reserva ASC;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("salao", salao);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var dt = reader.GetDateTime(0);
                datas.Add(dt.ToString("yyyy-MM-dd"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar datas ocupadas para {Salao}", salao);
        }

        return datas;
    }

    public async Task<(bool Sucesso, string Mensagem)> CriarAgendamentoAsync(Agendamento agendamento)
    {
        try
        {
            if (!DateTime.TryParse(agendamento.DataReserva, out var dataReservaDate))
            {
                return (false, "Data de reserva em formato inválido.");
            }

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Verifica se já existe agendamento para o mesmo salão e data
            const string checkSql = "SELECT COUNT(1) FROM agendamentos WHERE salao = @salao AND data_reserva = @data_reserva;";
            await using (var checkCmd = new NpgsqlCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("salao", agendamento.Salao);
                checkCmd.Parameters.AddWithValue("data_reserva", dataReservaDate.Date);

                var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    return (false, "Já existe uma reserva confirmada para este salão nesta data.");
                }
            }

            // 2. Insere a nova reserva com os dados do solicitante
            const string insertSql = @"
                INSERT INTO agendamentos (salao, data_reserva, congregacao, categoria, nome_solicitante, email_solicitante, criado_em)
                VALUES (@salao, @data_reserva, @congregacao, @categoria, @nome_solicitante, @email_solicitante, @criado_em)
                RETURNING id;
            ";

            await using (var insertCmd = new NpgsqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("salao", agendamento.Salao);
                insertCmd.Parameters.AddWithValue("data_reserva", dataReservaDate.Date);
                insertCmd.Parameters.AddWithValue("congregacao", agendamento.Congregacao);
                insertCmd.Parameters.AddWithValue("categoria", agendamento.Categoria);
                insertCmd.Parameters.AddWithValue("nome_solicitante", agendamento.NomeSolicitante ?? string.Empty);
                insertCmd.Parameters.AddWithValue("email_solicitante", agendamento.EmailSolicitante ?? string.Empty);
                insertCmd.Parameters.AddWithValue("criado_em", DateTime.UtcNow);

                var newId = await insertCmd.ExecuteScalarAsync();
                if (newId != null)
                {
                    agendamento.Id = Convert.ToInt32(newId);
                }
            }

            _logger.LogInformation("Agendamento criado com ID {Id} para {Salao} em {Data} por {Solicitante}",
                agendamento.Id, agendamento.Salao, agendamento.DataReserva, agendamento.NomeSolicitante);
            return (true, "Agendamento realizado com sucesso!");
        }
        catch (PostgresException pex) when (pex.SqlState == "23505")
        {
            return (false, "Já existe uma reserva confirmada para este salão nesta data.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gravar agendamento no PostgreSQL");
            return (false, $"Erro ao salvar no banco de dados: {ex.Message}");
        }
    }
}
