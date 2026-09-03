document.addEventListener('DOMContentLoaded', () => {
  const selectSalao = document.getElementById('selectSalao');
  const salaoInfo = document.getElementById('salaoInfo');
  const salaoInfoText = document.getElementById('salaoInfoText');
  const containerData = document.getElementById('containerData');
  const inputData = document.getElementById('inputData');
  const formAgendamento = document.getElementById('formAgendamento');
  const selectCategoria = document.getElementById('selectCategoria');
  const btnSubmit = document.getElementById('btnSubmit');
  const mensagemFeedback = document.getElementById('mensagemFeedback');

  let fpInstance = null;
  let datasOcupadas = [];

  async function carregarCategorias() {
    try {
      const res = await fetch('/api/categorias');
      if (!res.ok) throw new Error('Falha ao buscar categorias');

      const categorias = await res.json();
      categorias.forEach((categoria) => {
        const option = document.createElement('option');
        option.value = categoria;
        option.textContent = categoria;
        selectCategoria.appendChild(option);
      });
    } catch (err) {
      console.warn('Não foi possível carregar categorias:', err);
      exibirFeedback('error', 'Não foi possível carregar as categorias de reunião. Recarregue a página.');
      selectCategoria.disabled = true;
    }
  }

  // 1. Inicializa o Flatpickr com as regras de negócio de cada salão
  function initFlatpickr(salao, ocupadas = []) {
    if (fpInstance) {
      fpInstance.destroy();
    }

    inputData.value = '';

    const regras = {
      locale: 'pt',
      dateFormat: 'Y-m-d',
      altInput: true,
      altFormat: 'd/m/Y (l)',
      minDate: 'today',
      disableMobile: true,
    };

    if (salao === 'Salão Nova Lima') {
      // Nova Lima: Segunda a Sexta permitidos. Bloqueia Sáb (6), Dom (0) e datas já ocupadas
      regras.disable = [
        function (date) {
          const day = date.getDay();
          return day === 0 || day === 6;
        },
        ...ocupadas,
      ];
    } else if (salao === 'Salão Marques Herval') {
      // Marques Herval: Apenas Segundas-feiras permitidas (Day 1) e que não estejam ocupadas
      regras.enable = [
        function (date) {
          if (date.getDay() !== 1) return false;
          
          const ano = date.getFullYear();
          const mes = String(date.getMonth() + 1).padStart(2, '0');
          const dia = String(date.getDate()).padStart(2, '0');
          const dataStr = `${ano}-${mes}-${dia}`;
          
          return !ocupadas.includes(dataStr);
        },
      ];
    }

    fpInstance = flatpickr(inputData, regras);
  }

  // 2. Mudança no Seletor de Salão
  selectSalao.addEventListener('change', async (e) => {
    const salaoSelecionado = e.target.value;
    if (!salaoSelecionado) return;

    // Feedback visual
    containerData.classList.remove('opacity-50', 'pointer-events-none');
    salaoInfo.classList.remove('hidden');

    if (salaoSelecionado === 'Salão Nova Lima') {
      salaoInfoText.textContent = 'Disponível de Segunda a Sexta-feira.';
    } else {
      salaoInfoText.textContent = 'Disponível exclusivamente às Segundas-feiras.';
    }

    // Busca datas ocupadas no Backend
    try {
      inputData.placeholder = 'Carregando disponibilidade...';
      const res = await fetch(`/api/agendamentos/ocupados?salao=${encodeURIComponent(salaoSelecionado)}`);
      if (res.ok) {
        datasOcupadas = await res.json();
      } else {
        datasOcupadas = [];
      }
    } catch (err) {
      console.warn('Não foi possível buscar ocupados:', err);
      datasOcupadas = [];
    } finally {
      inputData.placeholder = 'Clique para escolher a data...';
      initFlatpickr(salaoSelecionado, datasOcupadas);
    }
  });

  // 3. Envio do Formulário
  formAgendamento.addEventListener('submit', async (e) => {
    e.preventDefault();

    const salao = selectSalao.value;
    const data_reserva = inputData.value;
    const congregacao = document.getElementById('inputCongregacao').value.trim();
    const categoria = document.getElementById('selectCategoria').value;
    const nome_solicitante = document.getElementById('inputNomeSolicitante').value.trim();
    const email_solicitante = document.getElementById('inputEmailSolicitante').value.trim();

    if (!salao || !data_reserva || !congregacao || !categoria || !nome_solicitante || !email_solicitante) {
      exibirFeedback('error', 'Por favor, preencha todos os campos do formulário.');
      return;
    }

    // Estado de carregamento
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = `<i class="fa-solid fa-spinner fa-spin"></i> <span>Confirmando reserva e enviando e-mails...</span>`;
    mensagemFeedback.classList.add('hidden');

    try {
      const payload = {
        salao,
        data_reserva,
        congregacao,
        categoria,
        nome_solicitante,
        email_solicitante,
      };

      const res = await fetch('/api/agendamentos', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      const data = await res.json();

      if (res.ok) {
        exibirFeedback('success', `🎉 Reserva confirmada para <strong>${salao}</strong> no dia <strong>${formatarDataBr(data_reserva)}</strong>!<br>Enviamos um e-mail com os detalhes para <strong>${email_solicitante}</strong> e para o Administrador.`);
        formAgendamento.reset();
        containerData.classList.add('opacity-50', 'pointer-events-none');
        salaoInfo.classList.add('hidden');
        if (fpInstance) fpInstance.clear();
      } else {
        exibirFeedback('error', data.erro || 'Não foi possível concluir o agendamento.');
      }
    } catch (err) {
      exibirFeedback('error', 'Erro de conexão com o servidor. Tente novamente.');
    } finally {
      btnSubmit.disabled = false;
      btnSubmit.innerHTML = `<i class="fa-solid fa-paper-plane"></i> <span>Confirmar Agendamento</span>`;
    }
  });

  function exibirFeedback(tipo, texto) {
    mensagemFeedback.classList.remove('hidden', 'bg-emerald-50', 'text-emerald-800', 'border-emerald-200', 'bg-rose-50', 'text-rose-800', 'border-rose-200');

    if (tipo === 'success') {
      mensagemFeedback.classList.add('bg-emerald-50', 'text-emerald-800', 'border-emerald-200');
      mensagemFeedback.innerHTML = `<i class="fa-solid fa-circle-check text-emerald-600 text-xl flex-shrink-0"></i> <div>${texto}</div>`;
    } else {
      mensagemFeedback.classList.add('bg-rose-50', 'text-rose-800', 'border-rose-200');
      mensagemFeedback.innerHTML = `<i class="fa-solid fa-circle-exclamation text-rose-600 text-xl flex-shrink-0"></i> <div>${texto}</div>`;
    }

    mensagemFeedback.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }

  function formatarDataBr(dataStr) {
    if (!dataStr) return '';
    const partes = dataStr.split('-');
    if (partes.length === 3) {
      return `${partes[2]}/${partes[1]}/${partes[0]}`;
    }
    return dataStr;
  }

  carregarCategorias();
});
