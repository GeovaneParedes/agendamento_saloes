using System.Collections.ObjectModel;

namespace AgendamentoApp.Models;

public static class CategoriasReuniao
{
    private static readonly string[] CategoriasInternas = ["Confidencial", "Ancião", "Treinamento", "Troca / Visita"];

    public static readonly ReadOnlyCollection<string> Todas = Array.AsReadOnly(CategoriasInternas);
}
