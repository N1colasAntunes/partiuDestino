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
WHERE email = 'nick@gmail.com';

SELECT tipo FROM usuarios WHERE email = @Email AND senha = @Senha;

CREATE TABLE viagem_personalizada (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    destino VARCHAR(100) NOT NULL,
    pais VARCHAR(100) NOT NULL,
    data_ida DATE NOT NULL,
    data_volta DATE NOT NULL,
    tipo_viagem VARCHAR(50) NOT NULL,
    numero_pessoas INT NOT NULL,
    orcamento DECIMAL(10,2) NOT NULL,
    hospedagem VARCHAR(50) NOT NULL,
    transporte VARCHAR(50) NOT NULL,
    experiencias_desejadas TEXT,
    observacoes TEXT,
    observacoes_finais TEXT,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- destinos disponíveis
CREATE TABLE destinos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pais VARCHAR(100) NOT NULL,
    estado VARCHAR(100) NOT NULL
);

INSERT INTO destinos (pais, estado) VALUES
-- Brasil (Turismo Nacional Forte)
('Brasil', 'Rio de Janeiro'),
('Brasil', 'São Paulo'),
('Brasil', 'Bahia'),
('Brasil', 'Ceará'),
('Brasil', 'Alagoas'),
('Brasil', 'Rio Grande do Norte'),
('Brasil', 'Santa Catarina'),
('Brasil', 'Rio Grande do Sul'),
('Brasil', 'Minas Gerais'),
('Brasil', 'Goiás'),
('Brasil', 'Amazonas'),
('Brasil', 'Mato Grosso do Sul'),
('Brasil', 'Paraná'),
('Brasil', 'Maranhão'),
('Brasil', 'Paraíba'),

-- Estados Unidos
('Estados Unidos', 'Califórnia'),
('Estados Unidos', 'Flórida'),
('Estados Unidos', 'Nova York'),
('Estados Unidos', 'Nevada'),
('Estados Unidos', 'Havaí'),

-- Europa Ocidental
('França', 'Provença-Alpes-Costa Azul'),
('Itália', 'Toscana'),
('Itália', 'Lácio'),
('Itália', 'Vêneto'),
('Espanha', 'Catalunha'),
('Espanha', 'Andaluzia'),
('Portugal', 'Lisboa'),
('Portugal', 'Algarve'),
('Reino Unido', 'Inglaterra'),
('Alemanha', 'Baviera'),

-- América do Sul e Caribe
('Argentina', 'Buenos Aires'),
('Argentina', 'Terra do Fogo'),
('Chile', 'Antofagasta'),
('Chile', 'Região Metropolitana de Santiago'),
('Peru', 'Cusco'),
('Peru', 'Lima'),
('Uruguai', 'Maldonado'),
('México', 'Quintana Roo'),
('México', 'Baja California Sur'),
('Colômbia', 'Bolívar'),

-- Ásia e Oceania
('Japão', 'Tóquio'),
('Japão', 'Hokkaido'),
('Tailândia', 'Phuket'),
('Tailândia', 'Bangkok'),
('Indonésia', 'Bali'),
('Austrália', 'Nova Gales do Sul'),
('Austrália', 'Queensland'),
('Nova Zelândia', 'Otago'),

-- África e Oriente Médio
('Egito', 'Cairo'),
('África do Sul', 'Cabo Ocidental'),
('Emirados Árabes Unidos', 'Dubai'),
('Marrocos', 'Marraquexe-Safi');







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
