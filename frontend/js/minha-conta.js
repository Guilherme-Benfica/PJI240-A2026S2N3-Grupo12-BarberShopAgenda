document.addEventListener("DOMContentLoaded", async () => {
  const auth = requireAuth(["Cliente"]);
  if (!auth) return;

  const feedback = document.getElementById("feedback");
  const listaAgendamentos = document.getElementById("lista-agendamentos");

  try {
    const agendamentos = await api.agendamentos.minhaConta();
    renderizarAgendamentos(agendamentos);
  } catch (erro) {
    listaAgendamentos.innerHTML = "<p>Não foi possível carregar seus agendamentos.</p>";
    mostrarFeedback(feedback, erro.message || "Erro ao carregar seus agendamentos.", "erro");
  }

  function renderizarAgendamentos(agendamentos) {
    if (!agendamentos.length) {
      listaAgendamentos.innerHTML = "<p class=\"dica-horarios\">Você ainda não tem agendamentos. <a href=\"agendar.html\">Agendar agora</a>.</p>";
      return;
    }

    listaAgendamentos.innerHTML = agendamentos.map(montarItem).join("");
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
          <p class="item-agendamento-servico">${escaparHtmlMinhaConta(a.servicoNome)}</p>
          <p class="item-agendamento-detalhe">com ${escaparHtmlMinhaConta(a.barbeiroNome)} · ${hora}</p>
          <p class="item-agendamento-preco">${formatarMoeda(a.servicoPreco)}</p>
        </div>
        ${badgeStatus(a.status)}
      </div>`;
  }
});

function escaparHtmlMinhaConta(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
