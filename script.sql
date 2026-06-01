CREATE DATABASE bdpartiudestino;
USE bdpartiudestino;

CREATE TABLE usuarios (
  id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255),
    email VARCHAR(255) UNIQUE,
    senha VARCHAR(255)
);
ALTER TABLE usuarios
ADD COLUMN tipo VARCHAR(20) NOT NULL DEFAULT 'usuario';

insert into usuarios (id,nome,email,senha) values (default,'Julia Costa','julia@gmail.com','12345');

-- aqui define quem vai ser admin
UPDATE usuarios
SET tipo = 'admin'
WHERE email = 'julia@gmail.com';

SELECT tipo FROM usuarios WHERE email = @Email AND senha = @Senha;

CREATE TABLE viagem_personalizada (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    nome_completo VARCHAR(150),
    cpf VARCHAR(14),
    email VARCHAR(150),
    whatsapp VARCHAR(20),
    destino VARCHAR(100),
    hospedagem VARCHAR(100),
    data_partida DATE,
    duracao_dias INT,
    clima_viagem VARCHAR(100),
    orcamento VARCHAR(100),
    adultos INT,
    criancas INT,
    desejos_especiais TEXT,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);
CREATE TABLE destinos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    origem_pais VARCHAR(100) NOT NULL,
    origem_estado VARCHAR(100) NOT NULL,
    pais VARCHAR(100) NOT NULL,
    estado VARCHAR(100) NOT NULL
);

INSERT INTO destinos (origem_pais, origem_estado, pais, estado) VALUES
-- Saindo do Brasil/SP para destinos nacionais
('Brasil', 'São Paulo', 'Brasil', 'Rio de Janeiro'),
('Brasil', 'São Paulo', 'Brasil', 'Bahia'),
('Brasil', 'São Paulo', 'Brasil', 'Ceará'),
('Brasil', 'São Paulo', 'Estados Unidos', 'Califórnia'),
('Brasil', 'São Paulo', 'Estados Unidos', 'Flórida'),
('Brasil', 'São Paulo', 'França', 'Provença-Alpes-Costa Azul'),
('Brasil', 'São Paulo', 'Itália', 'Toscana'),
('Brasil', 'São Paulo', 'Japão', 'Tóquio'),
('Brasil', 'Rio de Janeiro', 'Portugal', 'Lisboa'),
('Brasil', 'Rio de Janeiro', 'Argentina', 'Buenos Aires'),
('Brasil', 'Minas Gerais', 'Chile', 'Região Metropolitana de Santiago'),
('Brasil', 'Paraná', 'México', 'Quintana Roo');

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

INSERT INTO pacotes 
(destino_id, nome, descricao, tipo_viagem, duracao_dias, data_partida, data_retorno, preco_por_pessoa, vagas_disponiveis)
VALUES

(
    1,
    'Rio Premium Experience',
    'Pacote completo para conhecer as praias e pontos turísticos do Rio de Janeiro.',
    'Praia',
    7,
    '2026-07-10',
    '2026-07-17',
    4599.90,
    20
),
(
    3,
    'Bahia All Inclusive',
    'Experiência incrível em resort all inclusive na Bahia.',
    'Relaxamento',
    6,
    '2026-08-05',
    '2026-08-11',
    3899.50,
    15
),
(
    16,
    'Califórnia Dreams',
    'Conheça Los Angeles, praias e parques famosos da Califórnia.',
    'Internacional',
    10,
    '2026-09-12',
    '2026-09-22',
    12999.99,
    12
),
(
    18,
    'Nova York Experience',
    'Pacote completo para explorar Nova York e seus principais pontos turísticos.',
    'Internacional',
    8,
    '2026-11-03',
    '2026-11-11',
    14500.00,
    10
),
(
    21,
    'Paris Romântica',
    'Uma viagem inesquecível para casais na cidade luz.',
    'Romance',
    7,
    '2026-06-15',
    '2026-06-22',
    16990.90,
    8
),
(
    22,
    'Toscana Gourmet',
    'Experiência gastronômica e cultural na Toscana.',
    'Gastronomia',
    9,
    '2026-10-02',
    '2026-10-11',
    15200.00,
    10
),
(
    31,
    'Patagônia Argentina',
    'Aventura nas paisagens geladas da Patagônia.',
    'Aventura',
    8,
    '2026-07-20',
    '2026-07-28',
    8990.00,
    14
),
(
    41,
    'Tóquio Tech Tour',
    'Conheça o Japão moderno e tradicional em uma experiência única.',
    'Cultura',
    12,
    '2026-09-05',
    '2026-09-17',
    18990.00,
    9
),
(
    45,
    'Bali Paradise',
    'Pacote de luxo em Bali com hospedagem premium.',
    'Relaxamento',
    10,
    '2026-12-01',
    '2026-12-11',
    17499.99,
    6
),
(
    50,
    'Dubai Lux Experience',
    'Explore o luxo e modernidade de Dubai.',
    'Luxo',
    7,
    '2026-08-18',
    '2026-08-25',
    19990.00,
    5
);

-- consultas
SELECT * FROM usuarios;
SELECT * FROM destinos;
SELECT * FROM pacotes;
SELECT * FROM viagem_personalizada;