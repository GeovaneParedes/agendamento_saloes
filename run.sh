#!/usr/bin/env bash
# Script de conveniência — executa a aplicação a partir da raiz do repositório
set -e
dotnet run --project src/AgendamentoApp --urls "http://localhost:5555"
