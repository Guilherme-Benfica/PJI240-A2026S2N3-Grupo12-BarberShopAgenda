const DIAS_SEMANA_ABREV = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];
const DIAS_SEMANA_NOME = ["Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado"];
const MESES_ABREV = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];
const QUANTIDADE_DIAS_EXIBIDOS = 14;

/**
 * Dados fixos da barbearia (endereço e horário de funcionamento da casa).
 * Alterou o endereço ou o horário na porta? É só mudar aqui.
 */
const BARBEARIA = {
  nome: "BarberShop Agenda",
  endereco: "Rua Pitangueiras, 300 — Vila Arens",
  // Índice = dia da semana (0 = domingo). null = fechado.
  funcionamento: [
    null,
    { abre: "09:00", fecha: "19:00" },
    { abre: "09:00", fecha: "19:00" },
    { abre: "09:00", fecha: "19:00" },
    { abre: "09:00", fecha: "19:00" },
    { abre: "09:00", fecha: "19:00" },
    { abre: "09:00", fecha: "16:00" },
  ],
};

const URL_MAPA = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(BARBEARIA.endereco)}`;

document.addEventListener("DOMContentLoaded", async () => {
  const feedback = document.getElementById("feedback");
  const abas = document.getElementById("abas");
  const indicadorEtapas = document.getElementById("indicador-etapas");

  const painelServicos = document.getElementById("painel-servicos");
  const painelProfissionais = document.getElementById("painel-profissionais");
  const painelDetalhes = document.getElementById("painel-detalhes");
  const paineis = { servicos: painelServicos, profissionais: painelProfissionais, detalhes: painelDetalhes };

  const etapaHorario = document.getElementById("etapa-horario");
  const etapaSucesso = document.getElementById("etapa-sucesso");

  const buscaServico = document.getElementById("busca-servico");
  const listaServicos = document.getElementById("lista-servicos");
  const listaProfissionais = document.getElementById("lista-profissionais");
  const avisoProfissional = document.getElementById("aviso-profissional");

  const resumoServicoEscolhido = document.getElementById("resumo-servico-escolhido");
  const tiraDatas = document.getElementById("tira-datas");
  const tiraProfissionais = document.getElementById("tira-profissionais");
  const blocoHorarios = document.getElementById("bloco-horarios");

  const sheetFundo = document.getElementById("sheet-fundo");
  const sheetConfirmacao = document.getElementById("sheet-confirmacao");
  const sheetData = document.getElementById("titulo-sheet");
  const sheetResumo = document.getElementById("sheet-resumo");
  const sheetFechar = document.getElementById("sheet-fechar");
  const sheetCancelar = document.getElementById("sheet-cancelar");
  const toggleSilencio = document.getElementById("toggle-silencio");

  const formConfirmacao = document.getElementById("form-confirmacao");
  const campoNome = document.getElementById("cliente-nome");
  const campoTelefone = document.getElementById("cliente-telefone");
  const campoEmail = document.getElementById("cliente-email");
  const campoObservacao = document.getElementById("agendamento-observacao");

  const cartaoReserva = document.getElementById("cartao-reserva");
  const codigoConfirmacao = document.getElementById("codigo-confirmacao");
  const acaoCalendario = document.getElementById("acao-calendario");
  const acaoMapa = document.getElementById("acao-mapa");
  const botaoNovoAgendamento = document.getElementById("botao-novo-agendamento");

  let servicos = [];
  let barbeiros = [];
  let elementoFocoAnterior = null;

  const estado = {
    servico: null,
    barbeiro: null,
    // Já começa no primeiro dia em que a barbearia abre — sem cair num domingo sem horário.
    data: formatarDataParaApi(primeiroDiaAberto()),
    horario: null,
  };

  preencherDadosDaBarbearia();
  configurarAbas();

  await carregarListas();
  renderizarTiraDatas();

  buscaServico.addEventListener("input", () => renderizarServicos(filtrarServicos(buscaServico.value)));

  sheetFechar.addEventListener("click", fecharSheet);
  sheetCancelar.addEventListener("click", fecharSheet);
  sheetFundo.addEventListener("click", fecharSheet);
  document.addEventListener("keydown", (evento) => {
    if (evento.key === "Escape" && !sheetConfirmacao.hidden) fecharSheet();
  });

  formConfirmacao.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (!campoNome.value.trim() || !campoTelefone.value.trim() || !campoEmail.value.trim()) {
      mostrarFeedback(feedback, "Preencha nome, telefone e e-mail.", "erro");
      return;
    }

    const botaoConfirmar = formConfirmacao.querySelector("button[type=submit]");
    botaoConfirmar.disabled = true;

    try {
      const cliente = await api.clientes.criar({
        nome: campoNome.value.trim(),
        telefone: campoTelefone.value.trim(),
        email: campoEmail.value.trim() || null,
      });

      const agendamento = await api.agendamentos.criar({
        clienteId: cliente.id,
        barbeiroId: estado.barbeiro.id,
        servicoId: estado.servico.id,
        dataHora: `${estado.data}T${estado.horario}:00`,
        observacao: montarObservacao(),
      });

      exibirSucesso(agendamento);
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível concluir o agendamento.", "erro");
    } finally {
      botaoConfirmar.disabled = false;
    }
  });

  botaoNovoAgendamento.addEventListener("click", () => {
    estado.servico = null;
    estado.barbeiro = null;
    estado.horario = null;
    formConfirmacao.reset();
    buscaServico.value = "";
    renderizarServicos(servicos);
    atualizarAvisoProfissional();
    entrarModoCatalogo("servicos");
  });

  /* ---------- Dados fixos da casa ---------- */

  function preencherDadosDaBarbearia() {
    document.getElementById("topo-endereco").href = URL_MAPA;
    document.getElementById("topo-endereco-texto").textContent = BARBEARIA.endereco;
    document.getElementById("detalhe-endereco").textContent = BARBEARIA.endereco;
    document.getElementById("detalhe-mapa").href = URL_MAPA;
    document.getElementById("rodape-endereco").textContent = BARBEARIA.endereco;
    acaoMapa.href = URL_MAPA;

    document.getElementById("topo-status").innerHTML = montarStatusFuncionamento();
    document.getElementById("lista-horario-funcionamento").innerHTML = BARBEARIA.funcionamento
      .map((faixa, dia) => {
        const hoje = dia === new Date().getDay();
        const texto = faixa ? `${faixa.abre} às ${faixa.fecha}` : "Fechado";
        return `<li class="${hoje ? "dia-de-hoje" : ""}"><span>${DIAS_SEMANA_NOME[dia]}${hoje ? " (hoje)" : ""}</span><span>${texto}</span></li>`;
      })
      .join("");
  }

  function montarStatusFuncionamento() {
    const agora = new Date();
    const faixaHoje = BARBEARIA.funcionamento[agora.getDay()];
    const minutosAgora = agora.getHours() * 60 + agora.getMinutes();

    if (faixaHoje && minutosAgora >= paraMinutos(faixaHoje.abre) && minutosAgora < paraMinutos(faixaHoje.fecha)) {
      return `<span class="status-aberto">Aberto agora</span> · fecha às ${faixaHoje.fecha}`;
    }

    if (faixaHoje && minutosAgora < paraMinutos(faixaHoje.abre)) {
      return `<span class="status-fechado">Fechado</span> · abre hoje às ${faixaHoje.abre}`;
    }

    for (let i = 1; i <= 7; i += 1) {
      const dia = (agora.getDay() + i) % 7;
      const faixa = BARBEARIA.funcionamento[dia];
      if (faixa) {
        const quando = i === 1 ? "amanhã" : DIAS_SEMANA_NOME[dia].toLowerCase();
        return `<span class="status-fechado">Fechado</span> · abre ${quando} às ${faixa.abre}`;
      }
    }

    return "Agendamento online";
  }

  /* ---------- Abas e modos de navegação ---------- */

  function configurarAbas() {
    abas.querySelectorAll(".aba").forEach((botao) => {
      botao.addEventListener("click", () => entrarModoCatalogo(botao.dataset.painel));
    });
  }

  function entrarModoCatalogo(nomePainel) {
    abas.hidden = false;
    indicadorEtapas.hidden = true;
    etapaHorario.hidden = true;
    etapaSucesso.hidden = true;

    Object.entries(paineis).forEach(([nome, painel]) => {
      painel.hidden = nome !== nomePainel;
    });

    abas.querySelectorAll(".aba").forEach((botao) => {
      const ativa = botao.dataset.painel === nomePainel;
      botao.classList.toggle("aba-ativa", ativa);
      if (ativa) {
        botao.setAttribute("aria-current", "true");
      } else {
        botao.removeAttribute("aria-current");
      }
    });

    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function entrarModoAgendamento() {
    abas.hidden = true;
    Object.values(paineis).forEach((painel) => {
      painel.hidden = true;
    });
    etapaSucesso.hidden = true;
    etapaHorario.hidden = false;
    indicadorEtapas.hidden = false;
    marcarEtapa(2);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function marcarEtapa(numero) {
    indicadorEtapas.querySelectorAll("li").forEach((item) => {
      const etapa = Number(item.dataset.etapa);
      item.classList.toggle("etapa-ativa", etapa <= numero);
      if (etapa === numero) {
        item.setAttribute("aria-current", "step");
      } else {
        item.removeAttribute("aria-current");
      }
    });
  }

  /* ---------- Carregamento ---------- */

  async function carregarListas() {
    try {
      [servicos, barbeiros] = await Promise.all([api.servicos.listar(), api.barbeiros.listar()]);
      barbeiros = barbeiros.filter((b) => b.ativo);
      renderizarServicos(servicos);
      renderizarListaProfissionais();
    } catch (erro) {
      listaServicos.innerHTML = "<p>Não foi possível carregar os serviços.</p>";
      listaProfissionais.innerHTML = "<p>Não foi possível carregar os profissionais.</p>";
      mostrarFeedback(feedback, "Não foi possível carregar os serviços e profissionais.", "erro");
    }
  }

  /* ---------- Aba: serviços ---------- */

  function filtrarServicos(termo) {
    const termoNormalizado = termo.trim().toLowerCase();
    if (!termoNormalizado) return servicos;
    return servicos.filter((s) => s.nome.toLowerCase().includes(termoNormalizado));
  }

  function renderizarServicos(lista) {
    if (!lista.length) {
      listaServicos.innerHTML = "<p>Nenhum serviço encontrado.</p>";
      return;
    }

    listaServicos.innerHTML = lista
      .map(
        (s) => `
        <button type="button" class="item-servico" data-id="${s.id}" aria-label="Agendar ${escaparHtmlAgendar(s.nome)}, ${formatarMoeda(s.preco)}, ${s.duracaoMinutos} minutos">
          <span class="avatar-servico" aria-hidden="true">${iniciais(s.nome)}</span>
          <span class="item-servico-info">
            <span class="item-servico-nome">${escaparHtmlAgendar(s.nome)}</span>
            <span class="item-servico-detalhes">
              <span class="item-servico-preco">${formatarMoeda(s.preco)}</span>
              <span class="item-servico-duracao">⏱ ${s.duracaoMinutos} min</span>
            </span>
          </span>
          <span class="item-servico-selecionar" aria-hidden="true">Agendar</span>
        </button>`
      )
      .join("");

    listaServicos.querySelectorAll(".item-servico").forEach((item) => {
      item.addEventListener("click", () => selecionarServico(item.dataset.id));
    });
  }

  /* ---------- Aba: profissionais ---------- */

  function renderizarListaProfissionais() {
    if (!barbeiros.length) {
      listaProfissionais.innerHTML = "<p>Nenhum profissional disponível no momento.</p>";
      return;
    }

    listaProfissionais.innerHTML = barbeiros
      .map(
        (b) => `
        <div class="cartao-profissional">
          <span class="avatar-profissional avatar-profissional-grande" aria-hidden="true">${iniciais(b.nome)}</span>
          <div class="cartao-profissional-info">
            <p class="cartao-profissional-nome">${escaparHtmlAgendar(b.nome)}</p>
            <p class="cartao-profissional-especialidade">${escaparHtmlAgendar(b.especialidade || "Barbeiro")}</p>
          </div>
          <button type="button" class="botao-secundario botao-agendar-com" data-id="${b.id}">Agendar</button>
        </div>`
      )
      .join("");

    listaProfissionais.querySelectorAll(".botao-agendar-com").forEach((botao) => {
      botao.addEventListener("click", () => {
        estado.barbeiro = barbeiros.find((b) => String(b.id) === botao.dataset.id) || null;
        atualizarAvisoProfissional();
        entrarModoCatalogo("servicos");
      });
    });
  }

  function atualizarAvisoProfissional() {
    if (!estado.barbeiro) {
      avisoProfissional.hidden = true;
      avisoProfissional.innerHTML = "";
      return;
    }

    avisoProfissional.hidden = false;
    avisoProfissional.innerHTML = `
      <span>Agendando com <strong>${escaparHtmlAgendar(estado.barbeiro.nome)}</strong>. Agora escolha o serviço.</span>
      <button type="button" class="botao-texto-inline" id="botao-limpar-profissional">Trocar</button>
    `;
    avisoProfissional.querySelector("#botao-limpar-profissional").addEventListener("click", () => {
      estado.barbeiro = null;
      atualizarAvisoProfissional();
    });
  }

  /* ---------- Etapa 2: data, profissional e horário ---------- */

  function selecionarServico(id) {
    estado.servico = servicos.find((s) => String(s.id) === String(id));
    if (!estado.servico) return;

    resumoServicoEscolhido.innerHTML = `
      <span class="avatar-servico" aria-hidden="true">${iniciais(estado.servico.nome)}</span>
      <span class="resumo-servico-nome">${escaparHtmlAgendar(estado.servico.nome)}<br><span class="resumo-servico-preco">${formatarMoeda(estado.servico.preco)} · ${estado.servico.duracaoMinutos} min</span></span>
      <button type="button" class="resumo-servico-trocar" id="botao-trocar-servico">Trocar</button>
    `;
    resumoServicoEscolhido.querySelector("#botao-trocar-servico").addEventListener("click", () => {
      entrarModoCatalogo("servicos");
    });

    // Com um único profissional não faz sentido obrigar a escolha — já deixa selecionado.
    if (!estado.barbeiro && barbeiros.length === 1) estado.barbeiro = barbeiros[0];

    entrarModoAgendamento();
    renderizarTiraProfissionais();
    atualizarHorarios();
  }

  function renderizarTiraDatas() {
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);

    const dias = Array.from({ length: QUANTIDADE_DIAS_EXIBIDOS }, (_, i) => {
      const data = new Date(hoje);
      data.setDate(data.getDate() + i);
      return data;
    });

    tiraDatas.innerHTML = dias
      .map((data, indice) => {
        const valor = formatarDataParaApi(data);
        const selecionada = valor === estado.data;
        const fechado = !BARBEARIA.funcionamento[data.getDay()];
        return `
          <button type="button" class="dia-data${selecionada ? " dia-selecionado" : ""}${fechado ? " dia-fechado" : ""}" data-data="${valor}" aria-pressed="${selecionada}"${fechado ? ' title="Fechado neste dia"' : ""}>
            <span class="dia-data-semana">${indice === 0 ? "Hoje" : DIAS_SEMANA_ABREV[data.getDay()]}</span>
            <span class="dia-data-numero">${data.getDate()}</span>
            <span class="dia-data-mes">${MESES_ABREV[data.getMonth()]}</span>
          </button>`;
      })
      .join("");

    tiraDatas.querySelectorAll(".dia-data").forEach((botao) => {
      botao.addEventListener("click", () => {
        estado.data = botao.dataset.data;
        renderizarTiraDatas();
        atualizarHorarios();
      });
    });
  }

  function renderizarTiraProfissionais() {
    if (!barbeiros.length) {
      tiraProfissionais.innerHTML = "<p>Nenhum profissional disponível no momento.</p>";
      return;
    }

    tiraProfissionais.innerHTML = barbeiros
      .map((b) => {
        const selecionado = estado.barbeiro && String(estado.barbeiro.id) === String(b.id);
        return `
          <button type="button" class="item-profissional${selecionado ? " profissional-selecionado" : ""}" data-id="${b.id}" aria-pressed="${!!selecionado}">
            <span class="avatar-profissional" aria-hidden="true">${iniciais(b.nome)}</span>
            <span class="item-profissional-nome">${escaparHtmlAgendar(b.nome)}</span>
          </button>`;
      })
      .join("");

    tiraProfissionais.querySelectorAll(".item-profissional").forEach((botao) => {
      botao.addEventListener("click", () => {
        estado.barbeiro = barbeiros.find((b) => String(b.id) === botao.dataset.id);
        renderizarTiraProfissionais();
        atualizarHorarios();
      });
    });
  }

  async function atualizarHorarios() {
    if (!estado.barbeiro) {
      blocoHorarios.innerHTML = '<p class="dica-horarios">Escolha um profissional para ver os horários disponíveis.</p>';
      return;
    }

    blocoHorarios.innerHTML = '<p class="carregando">Buscando horários…</p>';

    try {
      const horarios = await api.horarios.disponiveis(estado.barbeiro.id, estado.data, estado.servico.id);
      renderizarHorarios(horarios);
    } catch (erro) {
      blocoHorarios.innerHTML = '<p class="dica-horarios">Não foi possível buscar os horários disponíveis.</p>';
      mostrarFeedback(feedback, erro.message || "Erro ao buscar horários.", "erro");
    }
  }

  function renderizarHorarios(horarios) {
    if (!horarios.length) {
      const diaSemana = new Date(`${estado.data}T00:00:00`).getDay();

      if (!BARBEARIA.funcionamento[diaSemana]) {
        blocoHorarios.innerHTML = '<p class="dica-horarios">A barbearia não abre neste dia. Escolha outra data.</p>';
      } else if (estaDeFerias(estado.barbeiro, estado.data)) {
        blocoHorarios.innerHTML = `<p class="dica-horarios">${escaparHtmlAgendar(estado.barbeiro.nome)} está de férias/ausente até ${formatarDataExibicaoAgendar(estado.barbeiro.feriasFim)}. Escolha outra data ou outro profissional.</p>`;
      } else {
        blocoHorarios.innerHTML = '<p class="dica-horarios">Nenhum horário disponível nesta data. Tente outro dia ou profissional.</p>';
      }
      return;
    }

    const manha = horarios.filter((h) => h < "12:00");
    const tarde = horarios.filter((h) => h >= "12:00");

    blocoHorarios.innerHTML = [montarPeriodo("Manhã", manha), montarPeriodo("Tarde", tarde)]
      .filter(Boolean)
      .join("");

    blocoHorarios.querySelectorAll(".horario-slot").forEach((botao) => {
      botao.addEventListener("click", () => abrirSheet(botao.dataset.horario));
    });
  }

  function montarPeriodo(titulo, horarios) {
    if (!horarios.length) return "";
    return `
      <div class="periodo-horarios">
        <div class="periodo-horarios-cabecalho">
          <span>${titulo}</span>
          <span class="periodo-horarios-linha" aria-hidden="true"></span>
          <span class="periodo-horarios-contagem">${horarios.length} horário${horarios.length > 1 ? "s" : ""}</span>
        </div>
        <div class="grade-horarios">
          ${horarios.map((h) => `<button type="button" class="horario-slot" data-horario="${h}">${h}</button>`).join("")}
        </div>
      </div>`;
  }

  /* ---------- Bottom sheet ---------- */

  function abrirSheet(horario) {
    estado.horario = horario;
    marcarEtapa(3);

    const data = new Date(`${estado.data}T00:00:00`);
    sheetData.textContent = `${DIAS_SEMANA_ABREV[data.getDay()]}, ${data.getDate()} ${MESES_ABREV[data.getMonth()]}`;

    sheetResumo.innerHTML = `
      <span class="avatar-servico" aria-hidden="true">${iniciais(estado.servico.nome)}</span>
      <span class="sheet-resumo-info">
        <p class="sheet-resumo-servico">${escaparHtmlAgendar(estado.servico.nome)}</p>
        <p class="sheet-resumo-barbeiro">com ${escaparHtmlAgendar(estado.barbeiro.nome)}</p>
        <p class="sheet-resumo-preco">${formatarMoeda(estado.servico.preco)}</p>
      </span>
      <span class="sheet-resumo-quando">
        ${horarioParaFimTexto(estado.horario, estado.servico.duracaoMinutos)}
        <strong>${estado.servico.duracaoMinutos} min</strong>
      </span>
    `;

    toggleSilencio.checked = false;
    formConfirmacao.reset();
    elementoFocoAnterior = document.activeElement;
    sheetFundo.hidden = false;
    sheetConfirmacao.hidden = false;
    document.body.style.overflow = "hidden";
    campoNome.focus();
  }

  function fecharSheet() {
    sheetFundo.hidden = true;
    sheetConfirmacao.hidden = true;
    document.body.style.overflow = "";
    if (!etapaHorario.hidden) marcarEtapa(2);
    elementoFocoAnterior?.focus?.();
  }

  function montarObservacao() {
    const partes = [];
    if (toggleSilencio.checked) partes.push("Cliente prefere não conversar durante o atendimento.");
    if (campoObservacao.value.trim()) partes.push(campoObservacao.value.trim());
    return partes.join(" ") || null;
  }

  /* ---------- Sucesso ---------- */

  function exibirSucesso(agendamento) {
    fecharSheet();

    const data = new Date(`${estado.data}T00:00:00`);
    const dataTexto = `${DIAS_SEMANA_NOME[data.getDay()]}, ${String(data.getDate()).padStart(2, "0")}/${String(data.getMonth() + 1).padStart(2, "0")}`;

    cartaoReserva.innerHTML = `
      <div class="cartao-reserva-linha">
        <span class="avatar-servico" aria-hidden="true">${iniciais(estado.servico.nome)}</span>
        <div class="cartao-reserva-info">
          <p class="cartao-reserva-servico">${escaparHtmlAgendar(estado.servico.nome)}</p>
          <p class="cartao-reserva-detalhe">com ${escaparHtmlAgendar(estado.barbeiro.nome)}</p>
        </div>
        <span class="cartao-reserva-preco">${formatarMoeda(estado.servico.preco)}</span>
      </div>
      <dl class="cartao-reserva-dados">
        <div><dt>Data</dt><dd>${dataTexto}</dd></div>
        <div><dt>Horário</dt><dd>${horarioParaFimTexto(estado.horario, estado.servico.duracaoMinutos)}</dd></div>
        <div><dt>Onde</dt><dd>${escaparHtmlAgendar(BARBEARIA.endereco)}</dd></div>
      </dl>
    `;

    codigoConfirmacao.textContent = agendamento.codigoConfirmacao;
    acaoCalendario.href = montarLinkGoogleAgenda();

    abas.hidden = true;
    indicadorEtapas.hidden = true;
    Object.values(paineis).forEach((painel) => {
      painel.hidden = true;
    });
    etapaHorario.hidden = true;
    etapaSucesso.hidden = false;
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function montarLinkGoogleAgenda() {
    const inicio = `${estado.data.replace(/-/g, "")}T${estado.horario.replace(":", "")}00`;
    const [h, m] = estado.horario.split(":").map(Number);
    const fim = new Date(2000, 0, 1, h, m + estado.servico.duracaoMinutos);
    const fimTexto = `${estado.data.replace(/-/g, "")}T${String(fim.getHours()).padStart(2, "0")}${String(fim.getMinutes()).padStart(2, "0")}00`;

    const parametros = new URLSearchParams({
      action: "TEMPLATE",
      text: `${estado.servico.nome} — ${BARBEARIA.nome}`,
      dates: `${inicio}/${fimTexto}`,
      ctz: "America/Sao_Paulo",
      details: `Agendamento com ${estado.barbeiro.nome}.`,
      location: BARBEARIA.endereco,
    });

    return `https://calendar.google.com/calendar/render?${parametros.toString()}`;
  }

  function horarioParaFimTexto(horario, duracaoMinutos) {
    const [h, m] = horario.split(":").map(Number);
    const inicio = new Date(2000, 0, 1, h, m);
    const fim = new Date(inicio.getTime() + duracaoMinutos * 60000);
    const fimTexto = `${String(fim.getHours()).padStart(2, "0")}:${String(fim.getMinutes()).padStart(2, "0")}`;
    return `${horario} - ${fimTexto}`;
  }
});

function paraMinutos(horario) {
  const [h, m] = horario.split(":").map(Number);
  return h * 60 + m;
}

/** Hoje, se a barbearia abre hoje; senão o próximo dia em que ela abre. */
function primeiroDiaAberto() {
  const data = new Date();
  for (let i = 0; i < 7; i += 1) {
    if (BARBEARIA.funcionamento[data.getDay()]) return data;
    data.setDate(data.getDate() + 1);
  }
  return new Date();
}

function estaDeFerias(barbeiro, dataISO) {
  return Boolean(barbeiro?.feriasInicio && barbeiro?.feriasFim && dataISO >= barbeiro.feriasInicio && dataISO <= barbeiro.feriasFim);
}

function formatarDataExibicaoAgendar(dataISO) {
  const [ano, mes, dia] = dataISO.split("-");
  return `${dia}/${mes}/${ano}`;
}

function formatarDataParaApi(data) {
  const ano = data.getFullYear();
  const mes = String(data.getMonth() + 1).padStart(2, "0");
  const dia = String(data.getDate()).padStart(2, "0");
  return `${ano}-${mes}-${dia}`;
}

function iniciais(nome) {
  const partes = nome.trim().split(/\s+/);
  const primeira = partes[0]?.[0] || "";
  const segunda = partes.length > 1 ? partes[partes.length - 1][0] : "";
  return (primeira + segunda).toUpperCase();
}

function escaparHtmlAgendar(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
