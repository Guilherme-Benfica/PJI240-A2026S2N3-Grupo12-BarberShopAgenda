document.addEventListener("DOMContentLoaded", () => {
  if (!requireAuth(["Admin"])) return;

  const form = document.getElementById("form-barbeiro");
  const campoId = document.getElementById("barbeiro-id");
  const campoNome = document.getElementById("barbeiro-nome");
  const campoEspecialidade = document.getElementById("barbeiro-especialidade");
  const campoAtivo = document.getElementById("barbeiro-ativo");
  const campoFeriasInicio = document.getElementById("barbeiro-ferias-inicio");
  const campoFeriasFim = document.getElementById("barbeiro-ferias-fim");
  const campoEmail = document.getElementById("barbeiro-email");
  const campoSenha = document.getElementById("barbeiro-senha");
  const linhaContaAcesso = document.getElementById("linha-conta-acesso");
  const botaoCancelarEdicao = document.getElementById("botao-cancelar-edicao");
  const feedback = document.getElementById("feedback");
  const listaBarbeiros = document.getElementById("lista-barbeiros");

  carregarBarbeiros();

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const emEdicao = Boolean(campoId.value);

    if (!emEdicao && (!campoEmail.value.trim() || campoSenha.value.length < 6)) {
      mostrarFeedback(feedback, "Informe um e-mail e uma senha de pelo menos 6 caracteres pra criar o login do barbeiro.", "erro");
      return;
    }

    if (campoFeriasInicio.value && campoFeriasFim.value && campoFeriasFim.value < campoFeriasInicio.value) {
      mostrarFeedback(feedback, "A data de fim das férias precisa ser depois da data de início.", "erro");
      return;
    }

    const dados = {
      nome: campoNome.value.trim(),
      especialidade: campoEspecialidade.value.trim() || null,
      ativo: campoAtivo.value === "true",
      feriasInicio: campoFeriasInicio.value || null,
      feriasFim: campoFeriasFim.value || null,
    };

    if (!emEdicao) {
      dados.email = campoEmail.value.trim();
      dados.senha = campoSenha.value;
    }

    const botaoSalvar = form.querySelector("button[type=submit]");
    botaoSalvar.disabled = true;

    try {
      if (emEdicao) {
        await api.barbeiros.atualizar(campoId.value, dados);
        mostrarFeedback(feedback, "Agenda do barbeiro atualizada com sucesso.");
      } else {
        await api.barbeiros.criar(dados);
        mostrarFeedback(feedback, "Barbeiro cadastrado com sucesso — já pode entrar com o e-mail e senha definidos.");
      }
      resetarFormulario();
      await carregarBarbeiros();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível salvar o barbeiro.", "erro");
    } finally {
      botaoSalvar.disabled = false;
    }
  });

  botaoCancelarEdicao.addEventListener("click", resetarFormulario);

  function resetarFormulario() {
    form.reset();
    campoId.value = "";
    botaoCancelarEdicao.hidden = true;
    mostrarCamposDeConta(true);
  }

  function mostrarCamposDeConta(mostrar) {
    linhaContaAcesso.hidden = !mostrar;
    campoEmail.disabled = !mostrar;
    campoSenha.disabled = !mostrar;
  }

  async function carregarBarbeiros() {
    try {
      const barbeiros = await api.barbeiros.listarTodos();
      renderizarBarbeiros(barbeiros);
    } catch (erro) {
      listaBarbeiros.innerHTML = "<p>Não foi possível carregar os barbeiros.</p>";
      mostrarFeedback(feedback, erro.message || "Erro ao carregar barbeiros.", "erro");
    }
  }

  function renderizarBarbeiros(barbeiros) {
    if (!barbeiros.length) {
      listaBarbeiros.innerHTML = "<p>Nenhum barbeiro cadastrado ainda.</p>";
      return;
    }

    const linhas = barbeiros
      .map((b) => {
        const temConta = b.contaAtiva !== null && b.contaAtiva !== undefined;
        const badgeConta = !temConta
          ? '<span class="badge badge-pendente">Sem login</span>'
          : b.contaAtiva
          ? '<span class="badge badge-confirmado">Ativa</span>'
          : '<span class="badge badge-cancelado">Inativa</span>';
        const acaoConta = temConta
          ? `<button type="button" class="botao-secundario botao-pequeno" data-acao="${b.contaAtiva ? "inativar-conta" : "ativar-conta"}" data-id="${b.id}">${b.contaAtiva ? "Inativar conta" : "Ativar conta"}</button>`
          : "";
        const ferias = b.feriasInicio && b.feriasFim
          ? `${formatarDataLocal(b.feriasInicio)} a ${formatarDataLocal(b.feriasFim)}`
          : "-";

        return `
        <tr>
          <td>${escaparHtmlLocal(b.nome)}</td>
          <td>${escaparHtmlLocal(b.especialidade || "-")}</td>
          <td>${escaparHtmlLocal(b.email || "-")}</td>
          <td>${b.ativo ? '<span class="badge badge-confirmado">Ativa</span>' : '<span class="badge badge-cancelado">Inativa</span>'}</td>
          <td>${badgeConta}</td>
          <td>${ferias}</td>
          <td>
            <div class="acoes-tabela">
              <button type="button" class="botao-secundario botao-pequeno" data-acao="editar" data-id="${b.id}" aria-label="Editar agenda de ${escaparHtmlLocal(b.nome)}">Editar agenda</button>
              ${acaoConta}
            </div>
          </td>
        </tr>`;
      })
      .join("");

    listaBarbeiros.innerHTML = `
      <table>
        <thead>
          <tr>
            <th scope="col">Nome</th>
            <th scope="col">Especialidade</th>
            <th scope="col">E-mail de login</th>
            <th scope="col">Agenda</th>
            <th scope="col">Conta</th>
            <th scope="col">Férias</th>
            <th scope="col">Ações</th>
          </tr>
        </thead>
        <tbody>${linhas}</tbody>
      </table>`;

    listaBarbeiros.querySelectorAll("button[data-acao=editar]").forEach((botao) => {
      botao.addEventListener("click", () => editarBarbeiro(botao.dataset.id, barbeiros));
    });
    listaBarbeiros.querySelectorAll("button[data-acao=ativar-conta]").forEach((botao) => {
      botao.addEventListener("click", () => alterarContaBarbeiro(botao.dataset.id, true));
    });
    listaBarbeiros.querySelectorAll("button[data-acao=inativar-conta]").forEach((botao) => {
      botao.addEventListener("click", () => alterarContaBarbeiro(botao.dataset.id, false));
    });
  }

  async function alterarContaBarbeiro(id, ativar) {
    if (!ativar && !confirm("Inativar a conta faz o barbeiro sumir do sistema pra clientes e outros usuários, e ele não consegue mais entrar. Continuar?")) {
      return;
    }

    try {
      if (ativar) {
        await api.barbeiros.ativarConta(id);
        mostrarFeedback(feedback, "Conta do barbeiro ativada.");
      } else {
        await api.barbeiros.inativarConta(id);
        mostrarFeedback(feedback, "Conta do barbeiro inativada.");
      }
      await carregarBarbeiros();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível alterar a conta do barbeiro.", "erro");
    }
  }

  function editarBarbeiro(id, barbeiros) {
    const barbeiro = barbeiros.find((b) => String(b.id) === String(id));
    if (!barbeiro) return;

    campoId.value = barbeiro.id;
    campoNome.value = barbeiro.nome;
    campoEspecialidade.value = barbeiro.especialidade || "";
    campoAtivo.value = String(barbeiro.ativo);
    campoFeriasInicio.value = barbeiro.feriasInicio || "";
    campoFeriasFim.value = barbeiro.feriasFim || "";
    mostrarCamposDeConta(false);
    botaoCancelarEdicao.hidden = false;
    campoNome.focus();
  }
});

function escaparHtmlLocal(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}

function formatarDataLocal(dataISO) {
  const [ano, mes, dia] = dataISO.split("-");
  return `${dia}/${mes}/${ano}`;
}
