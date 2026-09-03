#!/usr/bin/env bash
# Script de conveniência — executa a aplicação a partir da raiz do repositório
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dotnet run --project "$SCRIPT_DIR/src/AgendamentoApp" --urls "http://localhost:5555"
