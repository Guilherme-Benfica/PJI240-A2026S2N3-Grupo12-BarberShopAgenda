document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("form-cadastro");
  const campoNome = document.getElementById("cadastro-nome");
  const campoTelefone = document.getElementById("cadastro-telefone");
  const campoEmail = document.getElementById("cadastro-email");
  const campoSenha = document.getElementById("cadastro-senha");
  const campoConfirmarSenha = document.getElementById("cadastro-confirmar-senha");
  const feedback = document.getElementById("feedback");
  const secaoFormulario = document.getElementById("secao-formulario");
  const secaoSucesso = document.getElementById("secao-sucesso");

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (campoSenha.value !== campoConfirmarSenha.value) {
      mostrarFeedback(feedback, "As senhas não conferem.", "erro");
      return;
    }

    if (campoSenha.value.length < 6) {
      mostrarFeedback(feedback, "A senha precisa ter pelo menos 6 caracteres.", "erro");
      return;
    }

    const botaoCriar = form.querySelector("button[type=submit]");
    botaoCriar.disabled = true;

    try {
      await api.auth.registrar({
        nome: campoNome.value.trim(),
        telefone: campoTelefone.value.trim(),
        email: campoEmail.value.trim(),
        senha: campoSenha.value,
      });

      secaoFormulario.hidden = true;
      secaoSucesso.hidden = false;
      secaoSucesso.scrollIntoView({ behavior: "smooth" });
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível criar a conta.", "erro");
    } finally {
      botaoCriar.disabled = false;
    }
  });
});
