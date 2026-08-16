document.addEventListener("DOMContentLoaded", () => {
  const feedback = document.getElementById("feedback");
  const formBusca = document.getElementById("form-busca");
  const campoTelefone = document.getElementById("busca-telefone");
  const campoCodigo = document.getElementById("busca-codigo");
  const secaoResultados = document.getElementById("secao-resultados");
  const listaResultados = document.getElementById("lista-resultados");

  formBusca.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const telefone = campoTelefone.value.trim();
    const codigo = campoCodigo.value.trim();
    if (!telefone || !codigo) {
      mostrarFeedback(feedback, "Informe o telefone e o código de confirmação.", "erro");
      return;
    }

    const botaoBuscar = formBusca.querySelector("button[type=submit]");
    botaoBuscar.disabled = true;

    try {
      const agendamentos = await api.agendamentos.porCliente(telefone, codigo);
      renderizarResultados(agendamentos);
      secaoResultados.hidden = false;
      secaoResultados.scrollIntoView({ behavior: "smooth" });
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível buscar seus agendamentos.", "erro");
    } finally {
      botaoBuscar.disabled = false;
    }
  });

  function renderizarResultados(agendamentos) {
    if (!agendamentos.length) {
      listaResultados.innerHTML = "<p class=\"dica-horarios\">Nenhum agendamento encontrado para esse telefone.</p>";
      return;
    }

    listaResultados.innerHTML = agendamentos.map(montarItem).join("");
  }

  function montarItem(a) {
    const data = new Date(a.dataHora);
    const dia = String(data.getDate()).padStart(2, "0");
    const mes = data.toLocaleDateString("pt-BR", { month: "short" }).replace(".", "").toUpperCase();
    const hora = data.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });

    return `
      <div class="item-agendamento">
        <div class="item-agendamento-data">
          <span class="item-agendamento-dia">${dia}</span>
          <span class="item-agendamento-mes">${mes}</span>
        </div>
        <div class="item-agendamento-info">
          <p class="item-agendamento-servico">${escaparHtmlMeusAgendamentos(a.servicoNome)}</p>
          <p class="item-agendamento-detalhe">com ${escaparHtmlMeusAgendamentos(a.barbeiroNome)} · ${hora}</p>
          <p class="item-agendamento-preco">${formatarMoeda(a.servicoPreco)}</p>
        </div>
        ${badgeStatus(a.status)}
      </div>`;
  }
});

function escaparHtmlMeusAgendamentos(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
