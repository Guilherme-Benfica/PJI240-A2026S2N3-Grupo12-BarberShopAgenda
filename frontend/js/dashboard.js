document.addEventListener("DOMContentLoaded", async () => {
  if (!requireAuth(["Admin"])) return;

  const feedback = document.getElementById("feedback");
  const listaAgendaHoje = document.getElementById("lista-agenda-hoje");

  try {
    const [resumo, agendamentos] = await Promise.all([
      api.dashboard.hoje(),
      api.agendamentos.porData(formatarDataParaApi(new Date())),
    ]);

    document.getElementById("valor-total").textContent = resumo.totalAgendamentos;
    document.getElementById("valor-pendentes").textContent = resumo.pendentes;
    document.getElementById("valor-confirmados").textContent = resumo.confirmados;
    document.getElementById("valor-concluidos").textContent = resumo.concluidos;
    document.getElementById("valor-cancelados").textContent = resumo.cancelados;
    document.getElementById("valor-receita").textContent = formatarMoeda(resumo.receitaPrevista);

    renderizarAgenda(agendamentos, listaAgendaHoje);
  } catch (erro) {
    mostrarFeedback(feedback, erro.message || "Não foi possível carregar o dashboard.", "erro");
    listaAgendaHoje.innerHTML = "<p>Não foi possível carregar a agenda de hoje.</p>";
  }
});

function formatarDataParaApi(data) {
  const ano = data.getFullYear();
  const mes = String(data.getMonth() + 1).padStart(2, "0");
  const dia = String(data.getDate()).padStart(2, "0");
  return `${ano}-${mes}-${dia}`;
}

function renderizarAgenda(agendamentos, container) {
  if (!agendamentos.length) {
    container.innerHTML = "<p>Nenhum agendamento para hoje.</p>";
    return;
  }

  const linhas = agendamentos
    .map(
      (a) => `
      <tr>
        <td>${formatarDataHora(a.dataHora)}</td>
        <td>${a.clienteNome}</td>
        <td>${a.barbeiroNome}</td>
        <td>${a.servicoNome}</td>
        <td>${badgeStatus(a.status)}</td>
      </tr>`
    )
    .join("");

  container.innerHTML = `
    <table>
      <caption class="sr-only">Agendamentos de hoje</caption>
      <thead>
        <tr>
          <th scope="col">Horário</th>
          <th scope="col">Cliente</th>
          <th scope="col">Barbeiro</th>
          <th scope="col">Serviço</th>
          <th scope="col">Status</th>
        </tr>
      </thead>
      <tbody>${linhas}</tbody>
    </table>`;
}
