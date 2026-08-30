document.addEventListener("DOMContentLoaded", () => {
  const auth = requireAuth(["Admin", "Barbeiro", "Cliente"]);
  if (!auth) return;

  const form = document.getElementById("form-trocar-senha");
  const campoSenhaAtual = document.getElementById("senha-atual");
  const campoSenhaNova = document.getElementById("senha-nova");
  const campoSenhaNovaConfirmar = document.getElementById("senha-nova-confirmar");
  const feedback = document.getElementById("feedback");

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (campoSenhaNova.value !== campoSenhaNovaConfirmar.value) {
      mostrarFeedback(feedback, "As senhas novas não conferem.", "erro");
      return;
    }

    const botaoSalvar = form.querySelector("button[type=submit]");
    botaoSalvar.disabled = true;

    try {
      await api.auth.alterarSenha(campoSenhaAtual.value, campoSenhaNova.value);
      mostrarFeedback(feedback, "Senha alterada com sucesso.");
      form.reset();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível trocar a senha.", "erro");
    } finally {
      botaoSalvar.disabled = false;
    }
  });
});
