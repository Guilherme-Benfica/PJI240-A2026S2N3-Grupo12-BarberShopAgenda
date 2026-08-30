document.addEventListener("DOMContentLoaded", () => {
  if (!requireAuth(["Admin"])) return;

  const form = document.getElementById("form-servico");
  const campoId = document.getElementById("servico-id");
  const campoNome = document.getElementById("servico-nome");
  const campoPreco = document.getElementById("servico-preco");
  const campoDuracao = document.getElementById("servico-duracao");
  const campoDescricao = document.getElementById("servico-descricao");
  const botaoCancelarEdicao = document.getElementById("botao-cancelar-edicao");
  const feedback = document.getElementById("feedback");
  const listaServicos = document.getElementById("lista-servicos");

  carregarServicos();

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const dados = {
      nome: campoNome.value.trim(),
      descricao: campoDescricao.value.trim() || null,
      preco: parseFloat(campoPreco.value),
      duracaoMinutos: parseInt(campoDuracao.value, 10),
    };

    const botaoSalvar = form.querySelector("button[type=submit]");
    botaoSalvar.disabled = true;

    try {
      if (campoId.value) {
        await api.servicos.atualizar(campoId.value, dados);
        mostrarFeedback(feedback, "Serviço atualizado com sucesso.");
      } else {
        await api.servicos.criar(dados);
        mostrarFeedback(feedback, "Serviço cadastrado com sucesso.");
      }
      resetarFormulario();
      await carregarServicos();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível salvar o serviço.", "erro");
    } finally {
      botaoSalvar.disabled = false;
    }
  });

  botaoCancelarEdicao.addEventListener("click", resetarFormulario);

  function resetarFormulario() {
    form.reset();
    campoId.value = "";
    botaoCancelarEdicao.hidden = true;
  }

  async function carregarServicos() {
    try {
      const servicos = await api.servicos.listar();
      renderizarServicos(servicos);
    } catch (erro) {
      listaServicos.innerHTML = "<p>Não foi possível carregar os serviços.</p>";
      mostrarFeedback(feedback, erro.message || "Erro ao carregar serviços.", "erro");
    }
  }

  function renderizarServicos(servicos) {
    if (!servicos.length) {
      listaServicos.innerHTML = "<p>Nenhum serviço cadastrado ainda.</p>";
      return;
    }

    const linhas = servicos
      .map(
        (s) => `
        <tr>
          <td>${escaparHtmlServico(s.nome)}</td>
          <td>${escaparHtmlServico(s.descricao || "-")}</td>
          <td>${formatarMoeda(s.preco)}</td>
          <td>${s.duracaoMinutos} min</td>
          <td>
            <div class="acoes-tabela">
              <button type="button" class="botao-secundario botao-pequeno" data-acao="editar" data-id="${s.id}" aria-label="Editar serviço ${escaparHtmlServico(s.nome)}">Editar</button>
            </div>
          </td>
        </tr>`
      )
      .join("");

    listaServicos.innerHTML = `
      <table>
        <thead>
          <tr>
            <th scope="col">Nome</th>
            <th scope="col">Descrição</th>
            <th scope="col">Preço</th>
            <th scope="col">Duração</th>
            <th scope="col">Ações</th>
          </tr>
        </thead>
        <tbody>${linhas}</tbody>
      </table>`;

    listaServicos.querySelectorAll("button[data-acao=editar]").forEach((botao) => {
      botao.addEventListener("click", () => editarServico(botao.dataset.id, servicos));
    });
  }

  function editarServico(id, servicos) {
    const servico = servicos.find((s) => String(s.id) === String(id));
    if (!servico) return;

    campoId.value = servico.id;
    campoNome.value = servico.nome;
    campoPreco.value = servico.preco;
    campoDuracao.value = servico.duracaoMinutos;
    campoDescricao.value = servico.descricao || "";
    botaoCancelarEdicao.hidden = false;
    campoNome.focus();
  }
});

function escaparHtmlServico(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
