CREATE DATABASE bdpartiudestino;
USE bdpartiudestino;

-- tabela de login / controle de acesso
CREATE TABLE usuarios (
  id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255),
    email VARCHAR(255) UNIQUE,
    senha VARCHAR(255)
);
ALTER TABLE usuarios
ADD COLUMN tipo VARCHAR(20) NOT NULL DEFAULT 'usuario';

-- aqui define quem vai ser admin
UPDATE usuarios
SET tipo = 'admin'
WHERE email = 'juliacostacarvalho0928@gmail.com';

SELECT tipo FROM usuarios WHERE email = @Email AND senha = @Senha;

select * from usuarios;

-- destinos disponíveis
CREATE TABLE destinos (
  id        INT          PRIMARY KEY AUTO_INCREMENT,
  nome      VARCHAR(100) NOT NULL,
  pais      VARCHAR(100) NOT NULL,
  descricao TEXT,
  ativo     TINYINT(1)   DEFAULT 1
);

-- pacotes de viagem
CREATE TABLE pacotes (
  id                INT            PRIMARY KEY AUTO_INCREMENT,
  destino_id        INT            NOT NULL,
  nome              VARCHAR(200)   NOT NULL,
  descricao         TEXT,
  tipo_viagem       VARCHAR(50),
  duracao_dias      INT,
  data_partida      DATE,
  data_retorno      DATE,
  preco_por_pessoa  DECIMAL(10, 2) NOT NULL,
  vagas_disponiveis INT
);

ALTER TABLE pacotes
  ADD CONSTRAINT fk_pacotes_destino
  FOREIGN KEY (destino_id) REFERENCES destinos (id);

-- clientes
CREATE TABLE clientes (
  id              INT          PRIMARY KEY AUTO_INCREMENT,
  usuario_id      INT          NOT NULL,
  nome            VARCHAR(150) NOT NULL,
  email           VARCHAR(150) NOT NULL,
  telefone        VARCHAR(20),
  cpf             VARCHAR(14)  NOT NULL,
  data_nascimento DATE
);

ALTER TABLE clientes
  ADD CONSTRAINT fk_clientes_usuario
  FOREIGN KEY (usuario_id) REFERENCES usuarios (id);

ALTER TABLE clientes
  ADD CONSTRAINT uq_clientes_email UNIQUE (email);

ALTER TABLE clientes
  ADD CONSTRAINT uq_clientes_cpf UNIQUE (cpf);

-- reservas
CREATE TABLE reservas (
  id             INT            PRIMARY KEY AUTO_INCREMENT,
  cliente_id     INT            NOT NULL,
  pacote_id      INT            NOT NULL,
  codigo_reserva VARCHAR(20)    NOT NULL,
  status         VARCHAR(30)    DEFAULT 'pendente',
  qtd_viajantes  INT            DEFAULT 1,
  valor_total    DECIMAL(10, 2) NOT NULL,
  criado_em      DATETIME       DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE reservas
  ADD CONSTRAINT fk_reservas_cliente
  FOREIGN KEY (cliente_id) REFERENCES clientes (id);

ALTER TABLE reservas
  ADD CONSTRAINT fk_reservas_pacote
  FOREIGN KEY (pacote_id) REFERENCES pacotes (id);

ALTER TABLE reservas
  ADD CONSTRAINT uq_reservas_codigo UNIQUE (codigo_reserva);

-- pagamentos
CREATE TABLE pagamentos (
  id         INT            PRIMARY KEY AUTO_INCREMENT,
  reserva_id INT            NOT NULL,
  metodo     VARCHAR(30)    NOT NULL,
  status     VARCHAR(30)    DEFAULT 'aguardando',
  valor      DECIMAL(10, 2) NOT NULL,
  parcelas   INT            DEFAULT 1,
  pago_em    DATETIME,
  criado_em  DATETIME       DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE pagamentos
  ADD CONSTRAINT fk_pagamentos_reserva
  FOREIGN KEY (reserva_id) REFERENCES reservas (id);

-- avaliações dos pacotes
CREATE TABLE avaliacoes (
  id         INT      PRIMARY KEY AUTO_INCREMENT,
  pacote_id  INT      NOT NULL,
  cliente_id INT      NOT NULL,
  nota       TINYINT  NOT NULL,
  comentario TEXT,
  criado_em  DATETIME DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE avaliacoes
  ADD CONSTRAINT fk_avaliacoes_pacote
  FOREIGN KEY (pacote_id) REFERENCES pacotes (id);

ALTER TABLE avaliacoes
  ADD CONSTRAINT fk_avaliacoes_cliente
  FOREIGN KEY (cliente_id) REFERENCES clientes (id);

-- consultas
SELECT * FROM usuarios;
SELECT * FROM destinos;
SELECT * FROM pacotes;
SELECT * FROM clientes;
SELECT * FROM reservas;
SELECT * FROM pagamentos;
SELECT * FROM avaliacoes;

SELECT COUNT(*) AS 'total de pacotes'  FROM pacotes;
SELECT COUNT(*) AS 'total de reservas' FROM reservas;
SELECT COUNT(*) AS 'total de clientes' FROM clientes;

SELECT MAX(preco_por_pessoa) AS maior_preco FROM pacotes;
SELECT MIN(preco_por_pessoa) AS menor_preco FROM pacotes;
SELECT AVG(preco_por_pessoa) AS preco_medio FROM pacotes;