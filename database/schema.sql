-- ============================================================
-- BarberShop Agenda — Script de criação do banco de dados
-- MySQL 8.0
-- ============================================================

CREATE DATABASE IF NOT EXISTS barbershop_agenda
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE barbershop_agenda;

-- Garante que o cliente mysql envie/interprete este script como utf8mb4.
-- Sem isso, o cliente usado pelo docker-entrypoint na inicialização automática
-- (docker-entrypoint-initdb.d) usa o charset padrão dele (não necessariamente
-- utf8mb4), corrompendo acentos mesmo com o banco/tabelas já configurados como utf8mb4.
SET NAMES utf8mb4;

-- ------------------------------------------------------------
-- Tabela: Clientes
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Clientes (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(150) NOT NULL,
  Telefone VARCHAR(20) NOT NULL,
  Email VARCHAR(150) NULL,
  DataCadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UsuarioId INT NULL,
  INDEX IX_Clientes_Email (Email),
  UNIQUE INDEX IX_Clientes_UsuarioId (UsuarioId)
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- Tabela: Usuarios (contas de login — Admin, Barbeiro e Cliente)
-- Papel: 0 = Admin, 1 = Barbeiro, 2 = Cliente
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Usuarios (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(150) NOT NULL,
  Email VARCHAR(150) NOT NULL,
  SenhaHash LONGTEXT NOT NULL,
  Papel TINYINT NOT NULL,
  Ativo TINYINT(1) NOT NULL DEFAULT 1,
  DataCadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  EmailConfirmado TINYINT(1) NOT NULL DEFAULT 0,
  TokenVerificacaoEmail LONGTEXT NULL,
  TokenVerificacaoExpiraEm DATETIME NULL,
  TokenResetSenha LONGTEXT NULL,
  TokenResetSenhaExpiraEm DATETIME NULL,
  UNIQUE INDEX IX_Usuarios_Email (Email)
) ENGINE=InnoDB;

ALTER TABLE Clientes
  ADD CONSTRAINT FK_Clientes_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE SET NULL;

-- ------------------------------------------------------------
-- Tabela: Barbeiros
-- DiasTrabalho: bitmask — segunda=1, terça=2, quarta=4, quinta=8, sexta=16, sábado=32, domingo=64
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Barbeiros (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(150) NOT NULL,
  Especialidade VARCHAR(150) NULL,
  Ativo TINYINT(1) NOT NULL DEFAULT 1,
  UsuarioId INT NULL,
  HorarioInicioManha TIME NULL,
  HorarioFimManha TIME NULL,
  HorarioInicioTarde TIME NULL,
  HorarioFimTarde TIME NULL,
  DiasTrabalho TINYINT UNSIGNED NOT NULL DEFAULT 63,
  FeriasInicio DATE NULL,
  FeriasFim DATE NULL,
  CONSTRAINT FK_Barbeiros_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE SET NULL,
  UNIQUE INDEX IX_Barbeiros_UsuarioId (UsuarioId)
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- Tabela: Servicos
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Servicos (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(150) NOT NULL,
  Descricao VARCHAR(500) NULL,
  Preco DECIMAL(10,2) NOT NULL,
  DuracaoMinutos INT NOT NULL
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- Tabela: Agendamentos
-- Status: 0 = Pendente, 1 = Confirmado, 2 = Cancelado, 3 = Concluido
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Agendamentos (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ClienteId INT NOT NULL,
  BarbeiroId INT NOT NULL,
  ServicoId INT NOT NULL,
  DataHora DATETIME NOT NULL,
  Status TINYINT NOT NULL DEFAULT 0,
  Observacao VARCHAR(500) NULL,
  CodigoConfirmacao VARCHAR(6) NOT NULL DEFAULT '',
  CONSTRAINT FK_Agendamentos_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
  CONSTRAINT FK_Agendamentos_Barbeiros FOREIGN KEY (BarbeiroId) REFERENCES Barbeiros(Id),
  CONSTRAINT FK_Agendamentos_Servicos FOREIGN KEY (ServicoId) REFERENCES Servicos(Id),
  INDEX IX_Agendamentos_DataHora (DataHora),
  INDEX IX_Agendamentos_BarbeiroId (BarbeiroId)
) ENGINE=InnoDB;

-- ============================================================
-- Seeds — dados iniciais
-- ============================================================

-- Usuários: 1 admin + 3 contas de barbeiro (senha padrão documentada no README — troque em produção)
INSERT INTO Usuarios (Id, Nome, Email, SenhaHash, Papel, Ativo, DataCadastro) VALUES
  (1, 'Administrador', 'admin@barbershop.com', 'AQAAAAIAAYagAAAAEALS4Lb5vWiYPmmQFgUKKs5kmYjZALMI7i4meu9fPlAxq15d8thqwG9Ns75FRbzA4g==', 0, 1, '2026-01-01 00:00:00'),
  (2, 'Carlos Silva', 'carlos.silva@barbershop.com', 'AQAAAAIAAYagAAAAEB/8Wz4FFhsxpPNegqz3iqcE1lO4G46znRK6GQ7gGZY0J2WdKaeuHZlGUgu4GhLMTQ==', 1, 1, '2026-01-01 00:00:00'),
  (3, 'João Pereira', 'joao.pereira@barbershop.com', 'AQAAAAIAAYagAAAAEBy7KwJ9F4W7DZzBXQle0B0NrCquufmoJIbVSNJ30VQBaMVTzhYjtGkHZ05KR1r19w==', 1, 1, '2026-01-01 00:00:00'),
  (4, 'Marcos Souza', 'marcos.souza@barbershop.com', 'AQAAAAIAAYagAAAAEFeTriNcRDe10YZZpLPfi+s+UfCm1EwRMEbHlX355or+Y+IESPd6txvMMI7ajmtkqQ==', 1, 1, '2026-01-01 00:00:00');

INSERT INTO Barbeiros (Nome, Especialidade, Ativo, UsuarioId, HorarioInicioManha, HorarioFimManha, HorarioInicioTarde, HorarioFimTarde, DiasTrabalho) VALUES
  ('Carlos Silva', 'Cortes clássicos', 1, 2, '09:00', '12:00', '13:00', '19:00', 63),
  ('João Pereira', 'Barba e navalha', 1, 3, '09:00', '12:00', '13:00', '19:00', 63),
  ('Marcos Souza', 'Cortes modernos e degradê', 1, 4, '09:00', '12:00', '13:00', '19:00', 63);

INSERT INTO Servicos (Nome, Descricao, Preco, DuracaoMinutos) VALUES
  ('Corte de Cabelo', 'Corte tradicional masculino', 40.00, 30),
  ('Barba', 'Aparar e desenhar barba', 30.00, 20),
  ('Corte + Barba', 'Combo corte de cabelo e barba', 60.00, 50),
  ('Sobrancelha', 'Design de sobrancelha na navalha', 15.00, 15),
  ('Pigmentação de Barba', 'Pigmentação para uniformizar a barba', 45.00, 40);
