#!/usr/bin/env bash
# Script de conveniência — executa a aplicação resolvendo o caminho do projeto a partir do diretório do script
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dotnet run --project "$SCRIPT_DIR/src/AgendamentoApp" --urls "http://localhost:5555"
