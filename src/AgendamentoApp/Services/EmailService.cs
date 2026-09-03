using AgendamentoApp.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AgendamentoApp.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> EnviarNotificacaoAgendamentoAsync(Agendamento agendamento)
    {
        var smtpHost = _config["SMTP_HOST"] ?? _config["Smtp:Host"];
        var smtpPortStr = _config["SMTP_PORT"] ?? _config["Smtp:Port"];
        var smtpUser = _config["SMTP_USER"] ?? _config["Smtp:User"];
        var smtpPass = _config["SMTP_PASS"] ?? _config["Smtp:Pass"];
        var adminEmail = _config["ADMIN_EMAIL"] ?? _config["Smtp:AdminEmail"] ?? "geovaneparedes2@gmail.com";

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            _logger.LogWarning("Configurações de SMTP incompletas. E-mail de notificação não foi enviado.");
            return false;
        }

        try
        {
            int smtpPort = int.TryParse(smtpPortStr, out var p) ? p : 587;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema de Agendamento", smtpUser));
            
            // 1. Destinatário principal: Administrador
            message.To.Add(new MailboxAddress("Administrador", adminEmail));

            // 2. Destinatário em cópia: Solicitante (se informado)
            if (!string.IsNullOrWhiteSpace(agendamento.EmailSolicitante))
            {
                var nomeDest = string.IsNullOrWhiteSpace(agendamento.NomeSolicitante) ? "Solicitante" : agendamento.NomeSolicitante;
                message.Cc.Add(new MailboxAddress(nomeDest, agendamento.EmailSolicitante.Trim()));
            }

            message.Subject = $"🔔 Confirmação de Reserva: {agendamento.Salao} - {agendamento.DataReserva}";

            // Formatação amigável de data DD/MM/AAAA
            string dataFormatada = agendamento.DataReserva;
            if (DateTime.TryParse(agendamento.DataReserva, out var dt))
            {
                dataFormatada = dt.ToString("dd/MM/yyyy (dddd)");
            }

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; background-color: #ffffff;'>
                    <h2 style='color: #2563eb; border-bottom: 2px solid #2563eb; padding-bottom: 10px; margin-top: 0;'>Reserva Confirmada com Sucesso</h2>
                    <p style='color: #475569; font-size: 15px;'>Olá <strong>{(string.IsNullOrWhiteSpace(agendamento.NomeSolicitante) ? "Irmão(ã)" : agendamento.NomeSolicitante)}</strong>, seu agendamento de local foi registrado com sucesso:</p>
                    
                    <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                        <tr style='background-color: #f8fafc;'>
                            <td style='padding: 10px; font-weight: bold; border-bottom: 1px solid #e2e8f0; width: 40%; color: #334155;'>Local / Salão:</td>
                            <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a;'><strong>{agendamento.Salao}</strong></td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; font-weight: bold; border-bottom: 1px solid #e2e8f0; color: #334155;'>Data Reservada:</td>
                            <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; color: #2563eb; font-weight: bold;'>{dataFormatada}</td>
                        </tr>
                        <tr style='background-color: #f8fafc;'>
                            <td style='padding: 10px; font-weight: bold; border-bottom: 1px solid #e2e8f0; color: #334155;'>Congregação:</td>
                            <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a;'>{agendamento.Congregacao}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; font-weight: bold; border-bottom: 1px solid #e2e8f0; color: #334155;'>Categoria de Reunião:</td>
                            <td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><span style='background-color: #dbeafe; color: #1e40af; padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 13px;'>{agendamento.Categoria}</span></td>
                        </tr>
                        <tr style='background-color: #f8fafc;'>
                            <td style='padding: 10px; font-weight: bold; border-bottom: 1px solid #e2e8f0; color: #334155;'>Solicitante:</td>
                            <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a;'>{agendamento.NomeSolicitante} ({agendamento.EmailSolicitante})</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; font-weight: bold; color: #64748b; font-size: 13px;'>Data do Registro:</td>
                            <td style='padding: 10px; color: #64748b; font-size: 13px;'>{DateTime.UtcNow:dd/MM/yyyy HH:mm} (UTC)</td>
                        </tr>
                    </table>

                    <div style='margin-top: 25px; padding: 12px; background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 6px; color: #166534; font-size: 13px;'>
                        ✅ Guarde este e-mail como comprovante do agendamento da sua congregação.
                    </div>

                    <p style='margin-top: 25px; font-size: 12px; color: #94a3b8; text-align: center;'>Este é um e-mail automático gerado pelo Sistema de Agendamento.</p>
                </div>"
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secureOption = smtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(smtpHost, smtpPort, secureOption);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("E-mail de confirmação enviado para ADM ({Admin}) e Solicitante ({Solicitante})", 
                adminEmail, agendamento.EmailSolicitante);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de confirmação");
            return false;
        }
    }
}
