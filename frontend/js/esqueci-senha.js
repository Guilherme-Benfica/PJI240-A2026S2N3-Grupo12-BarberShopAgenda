document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("form-esqueci-senha");
  const campoEmail = document.getElementById("esqueci-email");
  const feedback = document.getElementById("feedback");

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const botaoEnviar = form.querySelector("button[type=submit]");
    botaoEnviar.disabled = true;

    try {
      await api.auth.esqueciSenha(campoEmail.value.trim());
      mostrarFeedback(feedback, "Se esse e-mail estiver cadastrado, você vai receber um link de redefinição em instantes.");
      form.reset();
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível processar o pedido.", "erro");
    } finally {
      botaoEnviar.disabled = false;
    }
  });
});
