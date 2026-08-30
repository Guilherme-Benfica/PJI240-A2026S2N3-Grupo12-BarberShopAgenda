const DIAS_SEMANA_ABREV = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];
const MESES_ABREV = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];
const QUANTIDADE_DIAS_EXIBIDOS = 14;

document.addEventListener("DOMContentLoaded", async () => {
  const feedback = document.getElementById("feedback");
  const indicadorEtapas = document.getElementById("indicador-etapas");

  const etapaServicos = document.getElementById("etapa-servicos");
  const etapaHorario = document.getElementById("etapa-horario");
  const etapaSucesso = document.getElementById("etapa-sucesso");

  const buscaServico = document.getElementById("busca-servico");
  const listaServicos = document.getElementById("lista-servicos");

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

  const resumoSucesso = document.getElementById("resumo-sucesso");
  const codigoConfirmacao = document.getElementById("codigo-confirmacao");
  const botaoNovoAgendamento = document.getElementById("botao-novo-agendamento");

  let servicos = [];
  let barbeiros = [];
  let elementoFocoAnterior = null;

  const estado = {
    servico: null,
    barbeiro: null,
    data: formatarDataParaApi(new Date()),
    horario: null,
  };

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

      const observacao = montarObservacao();

      const agendamento = await api.agendamentos.criar({
        clienteId: cliente.id,
        barbeiroId: estado.barbeiro.id,
        servicoId: estado.servico.id,
        dataHora: `${estado.data}T${estado.horario}:00`,
        observacao,
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
    etapaSucesso.hidden = true;
    etapaHorario.hidden = true;
    etapaServicos.hidden = false;
    indicadorEtapas.hidden = false;
    irParaEtapa(1);
  });

  async function carregarListas() {
    try {
      [servicos, barbeiros] = await Promise.all([api.servicos.listar(), api.barbeiros.listar()]);
      barbeiros = barbeiros.filter((b) => b.ativo);
      renderizarServicos(servicos);
    } catch (erro) {
      listaServicos.innerHTML = "<p>Não foi possível carregar os serviços.</p>";
      mostrarFeedback(feedback, "Não foi possível carregar os serviços e profissionais.", "erro");
    }
  }

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

  function selecionarServico(id) {
    estado.servico = servicos.find((s) => String(s.id) === String(id));
    if (!estado.servico) return;

    resumoServicoEscolhido.innerHTML = `
      <span class="avatar-servico" aria-hidden="true">${iniciais(estado.servico.nome)}</span>
      <span class="resumo-servico-nome">${escaparHtmlAgendar(estado.servico.nome)}<br><span class="resumo-servico-preco">${formatarMoeda(estado.servico.preco)} · ${estado.servico.duracaoMinutos} min</span></span>
      <button type="button" class="resumo-servico-trocar" id="botao-trocar-servico">Trocar serviço</button>
    `;
    resumoServicoEscolhido.querySelector("#botao-trocar-servico").addEventListener("click", () => {
      etapaHorario.hidden = true;
      etapaServicos.hidden = false;
      irParaEtapa(1);
    });

    etapaServicos.hidden = true;
    etapaHorario.hidden = false;
    irParaEtapa(2);

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
      .map((data) => {
        const valor = formatarDataParaApi(data);
        const selecionada = valor === estado.data;
        return `
          <button type="button" class="dia-data${selecionada ? " dia-selecionado" : ""}" data-data="${valor}" aria-pressed="${selecionada}">
            <span class="dia-data-semana">${DIAS_SEMANA_ABREV[data.getDay()]}</span>
            <span class="dia-data-numero">${data.getDate()}</span>
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
      blocoHorarios.innerHTML = '<p class="dica-horarios">Escolha um profissional para buscar os horários disponíveis.</p>';
      return;
    }

    blocoHorarios.innerHTML = '<p class="carregando">Buscando horários…</p>';

    try {
      const horarios = await api.horarios.disponiveis(estado.barbeiro.id, estado.data, estado.servico.id);
      renderizarHorarios(horarios);
    } catch (erro) {
      blocoHorarios.innerHTML = "<p class=\"dica-horarios\">Não foi possível buscar os horários disponíveis.</p>";
      mostrarFeedback(feedback, erro.message || "Erro ao buscar horários.", "erro");
    }
  }

  function renderizarHorarios(horarios) {
    if (!horarios.length) {
      if (estaDeFerias(estado.barbeiro, estado.data)) {
        blocoHorarios.innerHTML = `<p class="dica-horarios">${escaparHtmlAgendar(estado.barbeiro.nome)} está de férias/ausente até ${formatarDataExibicaoAgendar(estado.barbeiro.feriasFim)}. Escolha outra data ou outro profissional.</p>`;
      } else {
        blocoHorarios.innerHTML = '<p class="dica-horarios">Nenhum horário disponível nesta data. Tente outro dia ou profissional.</p>';
      }
      return;
    }

    const manha = horarios.filter((h) => h < "12:00");
    const tarde = horarios.filter((h) => h >= "12:00");

    blocoHorarios.innerHTML = [
      montarPeriodo("Manhã", manha),
      montarPeriodo("Tarde", tarde),
    ]
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

  function abrirSheet(horario) {
    estado.horario = horario;

    const data = new Date(`${estado.data}T00:00:00`);
    sheetData.textContent = `${DIAS_SEMANA_ABREV[data.getDay()]}, ${data.getDate()} ${MESES_ABREV[data.getMonth()]}`;

    sheetResumo.innerHTML = `
      <span class="avatar-servico" aria-hidden="true">${iniciais(estado.servico.nome)}</span>
      <span class="sheet-resumo-info">
        <p class="sheet-resumo-servico">${escaparHtmlAgendar(estado.servico.nome)}</p>
        <p class="sheet-resumo-barbeiro">
          <span class="avatar-profissional" style="width:1.3rem;height:1.3rem;font-size:0.65rem;display:inline-flex;">${iniciais(estado.barbeiro.nome)}</span>
          ${escaparHtmlAgendar(estado.barbeiro.nome)}
        </p>
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
    elementoFocoAnterior?.focus?.();
  }

  function montarObservacao() {
    const partes = [];
    if (toggleSilencio.checked) partes.push("Cliente prefere não conversar durante o atendimento.");
    if (campoObservacao.value.trim()) partes.push(campoObservacao.value.trim());
    return partes.join(" ") || null;
  }

  function exibirSucesso(agendamento) {
    fecharSheet();

    const data = new Date(`${estado.data}T00:00:00`);
    resumoSucesso.textContent = `${estado.servico.nome} com ${estado.barbeiro.nome}, ${DIAS_SEMANA_ABREV[data.getDay()]} ${String(data.getDate()).padStart(2, "0")}/${String(data.getMonth() + 1).padStart(2, "0")} às ${estado.horario}.`;
    codigoConfirmacao.textContent = agendamento.codigoConfirmacao;

    etapaServicos.hidden = true;
    etapaHorario.hidden = true;
    indicadorEtapas.hidden = true;
    etapaSucesso.hidden = false;
    etapaSucesso.scrollIntoView({ behavior: "smooth" });
  }

  function irParaEtapa(numero) {
    indicadorEtapas.querySelectorAll("li").forEach((item) => {
      const ativa = Number(item.dataset.etapa) <= numero;
      item.classList.toggle("etapa-ativa", ativa);
      if (Number(item.dataset.etapa) === numero) {
        item.setAttribute("aria-current", "step");
      } else {
        item.removeAttribute("aria-current");
      }
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function horarioParaFimTexto(horario, duracaoMinutos) {
    const [h, m] = horario.split(":").map(Number);
    const inicio = new Date(2000, 0, 1, h, m);
    const fim = new Date(inicio.getTime() + duracaoMinutos * 60000);
    const fimTexto = `${String(fim.getHours()).padStart(2, "0")}:${String(fim.getMinutes()).padStart(2, "0")}`;
    return `${horario} - ${fimTexto}`;
  }
});

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
