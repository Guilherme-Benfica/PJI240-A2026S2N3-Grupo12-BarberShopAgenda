document.addEventListener("DOMContentLoaded", async () => {
  const auth = requireAuth(["Admin", "Barbeiro"]);
  if (!auth) return;

  const ehBarbeiro = auth.papel === "Barbeiro";

  if (ehBarbeiro) {
    document.getElementById("secao-novo-agendamento").hidden = true;
  }

  const form = document.getElementById("form-agendamento");
  const selectCliente = document.getElementById("agendamento-cliente");
  const selectBarbeiro = document.getElementById("agendamento-barbeiro");
  const selectServico = document.getElementById("agendamento-servico");
  const campoData = document.getElementById("agendamento-data");
  const campoHora = document.getElementById("agendamento-hora");
  const campoObservacao = document.getElementById("agendamento-observacao");

  const formFiltro = document.getElementById("form-filtro");
  const filtroBarbeiro = document.getElementById("filtro-barbeiro");
  const filtroData = document.getElementById("filtro-data");
  const botaoLimparFiltro = document.getElementById("botao-limpar-filtro");

  const feedback = document.getElementById("feedback");
  const listaAgendamentos = document.getElementById("lista-agendamentos");

  await carregarListasDeApoio();
  await carregarAgendamentos();

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (!selectCliente.value || !selectBarbeiro.value || !selectServico.value || !campoData.value || !campoHora.value) {
      mostrarFeedback(feedback, "Preencha todos os campos obrigatórios.", "erro");
      return;
    }

    const dataHora = `${campoData.value}T${campoHora.value}:00`;

    const dados = {
      clienteId: parseInt(selectCliente.value, 10),
      barbeiroId: parseInt(selectBarbeiro.value, 10),
      servicoId: parseInt(selectServico.value, 10),
      dataHora,
      observacao: campoObservacao.value.trim() || null,
    };

    const botaoSalvar = form.querySelector("button[type=submit]");
    botaoSalvar.disabled = true;

    try {
      await api.agendamentos.criar(dados);
      mostrarFeedback(feedback, "Agendamento criado com sucesso.");
      form.reset();
      await carregarAgendamentos();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível criar o agendamento.", "erro");
    } finally {
      botaoSalvar.disabled = false;
    }
  });

  formFiltro.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    await carregarAgendamentos();
  });

  botaoLimparFiltro.addEventListener("click", async () => {
    if (!ehBarbeiro) {
      filtroBarbeiro.value = "";
    }
    filtroData.value = "";
    await carregarAgendamentos();
  });

  async function carregarListasDeApoio() {
    try {
      const [clientes, barbeiros, servicos] = await Promise.all([
        ehBarbeiro ? Promise.resolve([]) : api.clientes.listar(),
        ehBarbeiro ? api.barbeiros.listar() : api.barbeiros.listarTodos(),
        api.servicos.listar(),
      ]);

      if (!ehBarbeiro) {
        preencherSelect(selectCliente, clientes, (c) => c.nome);
        preencherSelect(selectBarbeiro, barbeiros.filter((b) => b.ativo && b.contaAtiva !== false), (b) => b.nome);
        preencherSelect(selectServico, servicos, (s) => `${s.nome} — ${formatarMoeda(s.preco)}`);
      }

      if (ehBarbeiro) {
        filtroBarbeiro.innerHTML = `<option value="${auth.barbeiroId}">${escaparHtmlAgendamento(
          barbeiros.find((b) => String(b.id) === String(auth.barbeiroId))?.nome || "Minha agenda"
        )}</option>`;
        filtroBarbeiro.value = String(auth.barbeiroId);
        filtroBarbeiro.disabled = true;
      } else {
        preencherSelect(filtroBarbeiro, barbeiros, (b) => b.nome, false);
      }
    } catch (erro) {
      mostrarFeedback(feedback, "Não foi possível carregar clientes, barbeiros ou serviços.", "erro");
    }
  }

  function preencherSelect(select, itens, rotulo, manterPrimeiraOpcao = true) {
    const opcoesExtras = itens.map((item) => `<option value="${item.id}">${escaparHtmlAgendamento(rotulo(item))}</option>`).join("");
    if (manterPrimeiraOpcao) {
      select.innerHTML = select.querySelector("option")?.outerHTML + opcoesExtras;
    } else {
      select.innerHTML = select.querySelector("option")?.outerHTML + opcoesExtras;
    }
  }

  async function carregarAgendamentos() {
    listaAgendamentos.innerHTML = '<p class="carregando">Carregando agendamentos...</p>';
    try {
      let agendamentos;
      if (filtroBarbeiro.value) {
        agendamentos = await api.agendamentos.porBarbeiro(filtroBarbeiro.value);
      } else if (filtroData.value) {
        agendamentos = await api.agendamentos.porData(filtroData.value);
      } else {
        agendamentos = await api.agendamentos.listar();
      }

      if (filtroBarbeiro.value && filtroData.value) {
        agendamentos = agendamentos.filter((a) => a.dataHora.startsWith(filtroData.value));
      }

      renderizarAgendamentos(agendamentos);
    } catch (erro) {
      listaAgendamentos.innerHTML = "<p>Não foi possível carregar os agendamentos.</p>";
      mostrarFeedback(feedback, erro.message || "Erro ao carregar agendamentos.", "erro");
    }
  }

  function renderizarAgendamentos(agendamentos) {
    if (!agendamentos.length) {
      listaAgendamentos.innerHTML = "<p>Nenhum agendamento encontrado para o filtro selecionado.</p>";
      return;
    }

    const ordenados = [...agendamentos].sort((a, b) => new Date(a.dataHora) - new Date(b.dataHora));

    const linhas = ordenados
      .map((a) => {
        const podeConfirmar = a.status === "Pendente";
        const podeCancelar = a.status === "Pendente" || a.status === "Confirmado";
        const podeConcluir = a.status === "Confirmado";

        return `
        <tr>
          <td>${formatarDataHora(a.dataHora)}</td>
          <td>${escaparHtmlAgendamento(a.clienteNome)}</td>
          <td>${escaparHtmlAgendamento(a.barbeiroNome)}</td>
          <td>${escaparHtmlAgendamento(a.servicoNome)}</td>
          <td>${formatarMoeda(a.servicoPreco)}</td>
          <td>${badgeStatus(a.status)}</td>
          <td>
            <div class="acoes-tabela">
              ${podeConfirmar ? `<button type="button" class="botao-secundario botao-pequeno" data-acao="confirmar" data-id="${a.id}" aria-label="Confirmar agendamento de ${escaparHtmlAgendamento(a.clienteNome)}">Confirmar</button>` : ""}
              ${podeConcluir ? `<button type="button" class="botao-secundario botao-pequeno" data-acao="concluir" data-id="${a.id}" aria-label="Concluir agendamento de ${escaparHtmlAgendamento(a.clienteNome)}">Concluir</button>` : ""}
              ${podeCancelar ? `<button type="button" class="botao-perigo botao-pequeno" data-acao="cancelar" data-id="${a.id}" aria-label="Cancelar agendamento de ${escaparHtmlAgendamento(a.clienteNome)}">Cancelar</button>` : ""}
            </div>
          </td>
        </tr>`;
      })
      .join("");

    listaAgendamentos.innerHTML = `
      <table>
        <thead>
          <tr>
            <th scope="col">Data/Hora</th>
            <th scope="col">Cliente</th>
            <th scope="col">Barbeiro</th>
            <th scope="col">Serviço</th>
            <th scope="col">Preço</th>
            <th scope="col">Status</th>
            <th scope="col">Ações</th>
          </tr>
        </thead>
        <tbody>${linhas}</tbody>
      </table>`;

    listaAgendamentos.querySelectorAll("button[data-acao]").forEach((botao) => {
      botao.addEventListener("click", () => executarAcao(botao.dataset.acao, botao.dataset.id));
    });
  }

  async function executarAcao(acao, id) {
    const acoes = {
      confirmar: () => api.agendamentos.confirmar(id),
      cancelar: () => api.agendamentos.cancelar(id),
      concluir: () => api.agendamentos.concluir(id),
    };

    const mensagens = {
      confirmar: "Agendamento confirmado.",
      cancelar: "Agendamento cancelado.",
      concluir: "Agendamento concluído.",
    };

    if (acao === "cancelar" && !confirm("Tem certeza que deseja cancelar este agendamento?")) {
      return;
    }

    try {
      await acoes[acao]();
      mostrarFeedback(feedback, mensagens[acao]);
      await carregarAgendamentos();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível executar a ação.", "erro");
    }
  }
});

function escaparHtmlAgendamento(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
