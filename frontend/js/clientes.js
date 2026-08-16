document.addEventListener("DOMContentLoaded", () => {
  if (!requireAuth(["Admin"])) return;

  const form = document.getElementById("form-cliente");
  const campoId = document.getElementById("cliente-id");
  const campoNome = document.getElementById("cliente-nome");
  const campoTelefone = document.getElementById("cliente-telefone");
  const campoEmail = document.getElementById("cliente-email");
  const botaoCancelarEdicao = document.getElementById("botao-cancelar-edicao");
  const feedback = document.getElementById("feedback");
  const listaClientes = document.getElementById("lista-clientes");

  carregarClientes();

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const dados = {
      nome: campoNome.value.trim(),
      telefone: campoTelefone.value.trim(),
      email: campoEmail.value.trim() || null,
    };

    const botaoSalvar = form.querySelector("button[type=submit]");
    botaoSalvar.disabled = true;

    try {
      if (campoId.value) {
        await api.clientes.atualizar(campoId.value, dados);
        mostrarFeedback(feedback, "Cliente atualizado com sucesso.");
      } else {
        await api.clientes.criar(dados);
        mostrarFeedback(feedback, "Cliente cadastrado com sucesso.");
      }
      resetarFormulario();
      await carregarClientes();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível salvar o cliente.", "erro");
    } finally {
      botaoSalvar.disabled = false;
    }
  });

  botaoCancelarEdicao.addEventListener("click", resetarFormulario);

  function resetarFormulario() {
    form.reset();
    campoId.value = "";
    botaoCancelarEdicao.hidden = true;
    form.querySelector("h2, legend")?.focus?.();
  }

  async function carregarClientes() {
    try {
      const clientes = await api.clientes.listar();
      renderizarClientes(clientes);
    } catch (erro) {
      listaClientes.innerHTML = "<p>Não foi possível carregar os clientes.</p>";
      mostrarFeedback(feedback, erro.message || "Erro ao carregar clientes.", "erro");
    }
  }

  function renderizarClientes(clientes) {
    if (!clientes.length) {
      listaClientes.innerHTML = "<p>Nenhum cliente cadastrado ainda.</p>";
      return;
    }

    const linhas = clientes
      .map(
        (c) => `
        <tr>
          <td>${escaparHtml(c.nome)}</td>
          <td>${escaparHtml(c.telefone)}</td>
          <td>${escaparHtml(c.email || "-")}</td>
          <td>
            <div class="acoes-tabela">
              <button type="button" class="botao-secundario botao-pequeno" data-acao="editar" data-id="${c.id}" aria-label="Editar cliente ${escaparHtml(c.nome)}">Editar</button>
              <button type="button" class="botao-perigo botao-pequeno" data-acao="remover" data-id="${c.id}" aria-label="Remover cliente ${escaparHtml(c.nome)}">Remover</button>
            </div>
          </td>
        </tr>`
      )
      .join("");

    listaClientes.innerHTML = `
      <table>
        <thead>
          <tr>
            <th scope="col">Nome</th>
            <th scope="col">Telefone</th>
            <th scope="col">E-mail</th>
            <th scope="col">Ações</th>
          </tr>
        </thead>
        <tbody>${linhas}</tbody>
      </table>`;

    listaClientes.querySelectorAll("button[data-acao=editar]").forEach((botao) => {
      botao.addEventListener("click", () => editarCliente(botao.dataset.id, clientes));
    });
    listaClientes.querySelectorAll("button[data-acao=remover]").forEach((botao) => {
      botao.addEventListener("click", () => removerCliente(botao.dataset.id));
    });
  }

  function editarCliente(id, clientes) {
    const cliente = clientes.find((c) => String(c.id) === String(id));
    if (!cliente) return;

    campoId.value = cliente.id;
    campoNome.value = cliente.nome;
    campoTelefone.value = cliente.telefone;
    campoEmail.value = cliente.email || "";
    botaoCancelarEdicao.hidden = false;
    campoNome.focus();
  }

  async function removerCliente(id) {
    if (!confirm("Tem certeza que deseja remover este cliente?")) return;

    try {
      await api.clientes.remover(id);
      mostrarFeedback(feedback, "Cliente removido com sucesso.");
      await carregarClientes();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível remover o cliente.", "erro");
    }
  }
});

function escaparHtml(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
