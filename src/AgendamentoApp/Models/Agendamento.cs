using System.Text.Json.Serialization;

namespace AgendamentoApp.Models;

public class Agendamento
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("salao")]
    public string Salao { get; set; } = string.Empty;

    [JsonPropertyName("data_reserva")]
    public string DataReserva { get; set; } = string.Empty;

    [JsonPropertyName("congregacao")]
    public string Congregacao { get; set; } = string.Empty;

    [JsonPropertyName("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [JsonPropertyName("nome_solicitante")]
    public string NomeSolicitante { get; set; } = string.Empty;

    [JsonPropertyName("email_solicitante")]
    public string EmailSolicitante { get; set; } = string.Empty;

    [JsonPropertyName("criado_em")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
