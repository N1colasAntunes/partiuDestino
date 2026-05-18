CREATE DATABASE bdpartiudestino;
USE bdpartiudestino;

drop database bdpartiudestino;

-- tabela de login / controle de acesso
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

select * from usuarios;
SELECT tipo FROM usuarios WHERE email = @Email AND senha = @Senha;

CREATE TABLE viagem_personalizada (
    id INT AUTO_INCREMENT PRIMARY KEY,
    -- usuário logado
    usuario_id INT NOT NULL,
    -- dados pessoais
    nome_completo VARCHAR(150),
    cpf VARCHAR(14),
    email VARCHAR(150),
    whatsapp VARCHAR(20),
    -- viagem
    destino VARCHAR(100),
    hospedagem VARCHAR(100),
    data_partida DATE,
    duracao_dias INT,
    -- experiência
    clima_viagem VARCHAR(100),
    orcamento VARCHAR(100),
    adultos INT,
    criancas INT,
    -- detalhes finais
    desejos_especiais TEXT,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
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