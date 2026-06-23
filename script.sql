-- ============================================================
--  PARTIU DESTINO — BANCO DE DADOS COMPLETO
--  Organizado por: Autores do TCC
--  Última atualização: junho de 2026
--
--  ESTRUTURA:
--    1. CRIAÇÃO DO BANCO
--    2. CRIAÇÃO DAS TABELAS
--    3. INSERÇÃO DE DADOS (INSERTs)
--    4. ALTERAÇÕES E ATUALIZAÇÕES (ALTERs / UPDATEs)
--    5. ÍNDICES
--    6. SELECTs DE VERIFICAÇÃO
-- ============================================================


-- ============================================================
-- 1. CRIAÇÃO DO BANCO DE DADOS
-- ============================================================

CREATE DATABASE bdpartiudestino;
USE bdpartiudestino;
select * from usuarios;
-- ============================================================
-- 2. CRIAÇÃO DAS TABELAS
-- ============================================================

-- ------------------------------------------------------------
-- 2.1 Tabela: usuarios
-- ------------------------------------------------------------
CREATE TABLE usuarios (
    id    INT          PRIMARY KEY AUTO_INCREMENT,
    nome  VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    senha VARCHAR(255) NOT NULL,
    tipo  VARCHAR(20)  NOT NULL DEFAULT 'usuario'   -- 'usuario' | 'admin'
);

ALTER TABLE usuarios
    ADD COLUMN ativo TINYINT(1) NOT NULL DEFAULT 1;


DELETE FROM usuarios 
WHERE id = 2;

-- ------------------------------------------------------------
-- 2.2 Tabela: destinos
--     preco_por_pessoa: preço base de referência do destino

-- ------------------------------------------------------------




CREATE TABLE destinos (
    id               INT            PRIMARY KEY AUTO_INCREMENT,
    origem_pais      VARCHAR(100)   NOT NULL,
    origem_estado    VARCHAR(100)   NOT NULL,
    pais             VARCHAR(100)   NOT NULL,
    estado           VARCHAR(100)   NOT NULL,
    imagem_url       VARCHAR(500),
    preco_por_pessoa DECIMAL(10,2)  NOT NULL DEFAULT 0.00
);

-- ------------------------------------------------------------
-- 2.3 Tabela: pacotes
--     imagem_url: imagem própria de cada pacote
-- ------------------------------------------------------------
CREATE TABLE pacotes (
    id                INT            PRIMARY KEY AUTO_INCREMENT,
    destino_id        INT            NOT NULL,
    nome              VARCHAR(200)   NOT NULL,
    descricao         TEXT,
    tipo_viagem       VARCHAR(50),
    duracao_dias      INT,
    data_partida      DATE,
    data_retorno      DATE,
    preco_por_pessoa  DECIMAL(10,2)  NOT NULL,
    vagas_disponiveis INT,
    imagem_url        VARCHAR(500)
);

-- ------------------------------------------------------------
-- 2.4 Tabela: viagem_personalizada
-- ------------------------------------------------------------
CREATE TABLE viagem_personalizada (
    id                INT          PRIMARY KEY AUTO_INCREMENT,
    usuario_id        INT          NOT NULL,
    nome_completo     VARCHAR(150),
    cpf               VARCHAR(14),
    email             VARCHAR(150),
    whatsapp          VARCHAR(20),
    destino           VARCHAR(100),
    hospedagem        VARCHAR(100),
    data_partida      DATE,
    duracao_dias      INT,
    clima_viagem      VARCHAR(100),
    orcamento         VARCHAR(100),
    adultos           INT,
    criancas          INT,
    desejos_especiais TEXT,
    data_criacao      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 2.5 Tabela: carrinho
-- ------------------------------------------------------------
CREATE TABLE carrinho (
    id              INT           PRIMARY KEY AUTO_INCREMENT,
    usuario_id      INT           NOT NULL,
    tipo_item       VARCHAR(30)   NOT NULL,    -- 'pacote' | 'destino' | 'viagem_personalizada'
    item_id         INT           NOT NULL,
    nome_item       VARCHAR(255)  NOT NULL,
    preco_unitario  DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    quantidade      INT           NOT NULL DEFAULT 1,
    data_adicionado TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 2.6 Tabela: pedidos
-- ------------------------------------------------------------
CREATE TABLE pedidos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    tipo_item VARCHAR(50) NOT NULL,
    item_id INT NOT NULL,
    nome_item VARCHAR(255) NOT NULL,
    preco_unitario DECIMAL(10,2) DEFAULT 0,
    quantidade INT DEFAULT 1,
    data_pedido DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ------------------------------------------------------------
-- 2.7 Tabela: hospedagens
--     Cada pacote pode ter VÁRIAS opções de hospedagem
-- ------------------------------------------------------------
CREATE TABLE hospedagens (
    id          INT           PRIMARY KEY AUTO_INCREMENT,
    pacote_id   INT           NOT NULL,
    nome        VARCHAR(150)  NOT NULL,
    categoria   VARCHAR(50),      -- ex: '3 estrelas', 'Resort', 'Pousada'
    descricao   TEXT,
    endereco    VARCHAR(255),
    imagem_url  VARCHAR(500),
    FOREIGN KEY (pacote_id) REFERENCES pacotes(id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 2.8 Tabela: quartos
--     Cada hospedagem pode ter VÁRIOS tipos de quarto
--     preco_adicional é somado ao preco_por_pessoa do pacote
-- ------------------------------------------------------------
CREATE TABLE quartos (
    id                    INT           PRIMARY KEY AUTO_INCREMENT,
    hospedagem_id         INT           NOT NULL,
    tipo_quarto           VARCHAR(100)  NOT NULL,   -- ex: Standard, Luxo, Suíte
    capacidade_adultos    INT           NOT NULL DEFAULT 2,
    capacidade_criancas   INT           NOT NULL DEFAULT 0,
    preco_adicional       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    quantidade_disponivel INT           NOT NULL DEFAULT 1,
    comodidades           VARCHAR(255),             -- ex: 'Wi-Fi, Ar-condicionado, Vista mar'
    imagem_url            VARCHAR(500),
    FOREIGN KEY (hospedagem_id) REFERENCES hospedagens(id) ON DELETE CASCADE
);

CREATE INDEX idx_hospedagens_pacote ON hospedagens(pacote_id);
CREATE INDEX idx_quartos_hospedagem ON quartos(hospedagem_id);
-- ============================================================
-- 3. INSERÇÃO DE DADOS
-- ============================================================

-- ------------------------------------------------------------
-- 3.1 Usuário administrador padrão
-- ------------------------------------------------------------
INSERT INTO usuarios (nome, email, senha, tipo) VALUES
    ('Julia Costa', 'julia@gmail.com', '$2a$11$KG8AxkIziG2A6C9aOIzWkeD82eW96KTcXrDiM2JMYZGlSmLVoU2am', 'admin');
-- ------------------------------------------------------------
-- 3.2 Destinos (com imagem_url e preco_por_pessoa)
-- ------------------------------------------------------------
INSERT INTO destinos (origem_pais, origem_estado, pais, estado, imagem_url, preco_por_pessoa) VALUES

    -- Nacionais (saindo de São Paulo)
    ('Brasil', 'São Paulo', 'Brasil', 'Rio de Janeiro',
        'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=700&q=80',
        1200.00),

    ('Brasil', 'São Paulo', 'Brasil', 'Bahia',
        'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80',
        980.00),

    ('Brasil', 'São Paulo', 'Brasil', 'Ceará',
        'https://images.unsplash.com/photo-1538565756327-7e5b9dc67c3f?w=700&q=80',
        850.00),

    -- Internacionais (saindo de São Paulo)
    ('Brasil', 'São Paulo', 'Estados Unidos', 'Califórnia',
        'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=700&q=80',
        8500.00),

    ('Brasil', 'São Paulo', 'Estados Unidos', 'Flórida',
        'https://images.unsplash.com/photo-1533106418989-88406c7cc8ca?w=700&q=80',
        7900.00),

    ('Brasil', 'São Paulo', 'França', 'Provença-Alpes-Costa Azul',
        'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=700&q=80',
        11500.00),

    ('Brasil', 'São Paulo', 'Itália', 'Toscana',
        'https://images.unsplash.com/photo-1543429776-2782fc8e3e56?w=700&q=80',
        10800.00),

    ('Brasil', 'São Paulo', 'Japão', 'Tóquio',
        'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=700&q=80',
        13200.00),

    -- Internacionais (saindo do Rio de Janeiro)
    ('Brasil', 'Rio de Janeiro', 'Portugal', 'Lisboa',
        'https://images.unsplash.com/photo-1555881400-74d7acaacd8b?w=700&q=80',
        9400.00),

    ('Brasil', 'Rio de Janeiro', 'Argentina', 'Buenos Aires',
        'https://images.unsplash.com/photo-1583285233058-4a9e6a5e34d8?w=700&q=80',
        4200.00),

    -- Internacionais (saindo de Minas Gerais / Paraná)
    ('Brasil', 'Minas Gerais', 'Chile', 'Região Metropolitana de Santiago',
        'https://images.unsplash.com/photo-1554254648-2d58a1bc3fd5?w=700&q=80',
        5600.00),

    ('Brasil', 'Paraná', 'México', 'Quintana Roo',
        'https://images.unsplash.com/photo-1552074284-5e88ef1aef18?w=700&q=80',
        6300.00);

-- ------------------------------------------------------------
-- 3.3 Pacotes (com imagem_url própria de cada pacote)
-- ------------------------------------------------------------
INSERT INTO pacotes
    (destino_id, nome, descricao, tipo_viagem, duracao_dias,
     data_partida, data_retorno, preco_por_pessoa, vagas_disponiveis, imagem_url)
VALUES

    (1, 'Rio Premium Experience',
        'Pacote completo para conhecer as praias e pontos turísticos do Rio de Janeiro.',
        'Praia', 7, '2026-07-10', '2026-07-17', 4599.90, 20,
        'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=700&q=80'),

    (2, 'Bahia All Inclusive',
        'Experiência incrível em resort all inclusive na Bahia.',
        'Relaxamento', 6, '2026-08-05', '2026-08-11', 3899.50, 15,
        'https://images.unsplash.com/photo-1590060879041-cfab4e95d716?w=700&q=80'),

    (4, 'Califórnia Dreams',
        'Conheça Los Angeles, praias e parques famosos da Califórnia.',
        'Internacional', 10, '2026-09-12', '2026-09-22', 12999.99, 12,
        'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=700&q=80'),

    (4, 'Nova York Experience',
        'Pacote completo para explorar Nova York e seus principais pontos turísticos.',
        'Internacional', 8, '2026-11-03', '2026-11-11', 14500.00, 10,
        'https://images.unsplash.com/photo-1490644658840-3f2e3f8c5625?w=700&q=80'),

    (6, 'Paris Romântica',
        'Uma viagem inesquecível para casais na cidade luz.',
        'Romance', 7, '2026-06-15', '2026-06-22', 16990.90, 8,
        'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=700&q=80'),

    (7, 'Toscana Gourmet',
        'Experiência gastronômica e cultural na Toscana.',
        'Gastronomia', 9, '2026-10-02', '2026-10-11', 15200.00, 10,
        'https://images.unsplash.com/photo-1543429776-2782fc8e3e56?w=700&q=80'),


    (8, 'Tóquio Tech Tour',
        'Conheça o Japão moderno e tradicional em uma experiência única.',
        'Cultura', 12, '2026-09-05', '2026-09-17', 18990.00, 9,
        'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=700&q=80'),

    (12, 'Bali Paradise',
        'Pacote de luxo em Bali com hospedagem premium.',
        'Relaxamento', 10, '2026-12-01', '2026-12-11', 17499.99, 6,
        'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=700&q=80'),

    (12, 'Dubai Lux Experience',
        'Explore o luxo e modernidade de Dubai.',
        'Luxo', 7, '2026-08-18', '2026-08-25', 19990.00, 5,
        'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=700&q=80');

-- Hospedagens + Quartos de exemplo para o pacote "Rio Premium Experience" (id 1)
INSERT INTO hospedagens (pacote_id, nome, categoria, descricao, endereco, imagem_url) VALUES
(1, 'Copacabana Palace Inn', '4 estrelas', 'Hotel a 200m da praia de Copacabana, café da manhã incluso.', 'Av. Atlântica, 1500 - Copacabana, RJ',
 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=700&q=80'),
(1, 'Pousada Vista Mar RJ', 'Pousada', 'Pousada charmosa e econômica, a 5 minutos a pé da praia.', 'Rua Barata Ribeiro, 300 - Copacabana, RJ',
 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=700&q=80');

INSERT INTO quartos (hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades) VALUES
(1, 'Standard', 2, 1, 0.00, 10, 'Wi-Fi, Ar-condicionado, TV a cabo'),
(1, 'Luxo Vista Mar', 2, 1, 450.00, 5, 'Wi-Fi, Ar-condicionado, Varanda, Vista mar'),
(1, 'Suíte Master', 4, 2, 890.00, 3, 'Wi-Fi, Ar-condicionado, Jacuzzi, Vista mar, Frigobar'),
(2, 'Quarto Simples', 2, 0, 0.00, 8, 'Wi-Fi, Ventilador'),
(2, 'Quarto Família', 4, 2, 180.00, 4, 'Wi-Fi, Ar-condicionado, 2 camas');

SET SQL_SAFE_UPDATES = 0;

-- IMAGENS DOS QUARTOS DO PACOTE RIO
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=600&q=80' WHERE tipo_quarto = 'Standard';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=600&q=80' WHERE tipo_quarto = 'Luxo Vista Mar';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1582719508461-905c673771fd?w=600&q=80' WHERE tipo_quarto = 'Suíte Master';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&q=80' WHERE tipo_quarto = 'Quarto Simples';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=600&q=80' WHERE tipo_quarto = 'Quarto Família';

SET SQL_SAFE_UPDATES = 1;

-- ============================================================
-- 4. ALTERAÇÕES E ATUALIZAÇÕES
-- ============================================================

SET SQL_SAFE_UPDATES = 0;

-- ------------------------------------------------------------
-- 4.1 Promover usuário para administrador
-- ------------------------------------------------------------
UPDATE usuarios
SET tipo = 'admin'
WHERE email = 'julia@gmail.com';

-- ============================================================
-- PACOTES
-- ============================================================
-- Mapa de IDs:
--   1  → Rio Premium Experience        (destino 1 – RJ)
--   2  → Bahia All Inclusive           (destino 2 – BA)
--   3  → Califórnia Dreams             (destino 4 – CA)
--   4  → Nova York Experience          (destino 4 – NY)
--   5  → Paris Romântica               (destino 6 – FR)
--   6  → Toscana Gourmet               (destino 7 – IT)
--   7  → Patagônia Argentina           (destino 10 – AR)
--   8  → Tóquio Tech Tour              (destino 8 – JP)
--   9  → Bali Paradise                 (destino 12 – ID)
--  10  → Dubai Lux Experience          (destino 12 – AE)
-- ============================================================

-- 1 · Rio Premium Experience — praia de Copacabana / Ipanema
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 1;

-- 2 · Bahia All Inclusive — praia de Morro de São Paulo / Trancoso
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 2;

-- 3 · Califórnia Dreams — rodovia 1 / costa da Califórnia
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'
WHERE id = 3;

-- 4 · Nova York Experience — Times Square / Manhattan skyline
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1534430480872-3498386e7856?w=800&q=80'
WHERE id = 4;

-- 5 · Paris Romântica — Torre Eiffel ao entardecer
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'
WHERE id = 5;

-- 6 · Toscana Gourmet — vinhedos e villa toscana
UPDATE pacotes
SET imagem_url = 'https://americachip.com/wp-content/uploads/2023/10/wikipedia-toscana.jpg'
WHERE id = 6;

-- 7 · Patagônia Argentina — Torres del Paine / Perito Moreno
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1501854248509-c7e427ccd5ae?w=800&q=80'
WHERE id = 7;

-- 8 · Tóquio Tech Tour — Shibuya crossing / templos
UPDATE pacotes
SET imagem_url = 'https://media.cntraveller.com/photos/6343df288d5d266e2e66f082/16:9/w_2560%2Cc_limit/tokyoGettyImages-1031467664.jpeg'
WHERE id = 8;

-- 9 · Bali Paradise — terraços de arroz / templo Uluwatu
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800&q=80'
WHERE id = 9;

-- 10 · Dubai Lux Experience — Burj Khalifa / skyline Dubai
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80'
WHERE id = 10;

SET SQL_SAFE_UPDATES = 1;


-- ============================================================
-- VERIFICAÇÃO FINAL
-- ============================================================

-- Destinos com imagem
SELECT id,
       CONCAT(pais, ' / ', estado) AS destino,
       imagem_url
FROM destinos
ORDER BY id;

-- Pacotes com imagem
SELECT p.id,
       p.nome                           AS pacote,
       CONCAT(d.pais, ' / ', d.estado) AS destino_vinculado,
       p.imagem_url
FROM pacotes p
JOIN destinos d ON d.id = p.destino_id
ORDER BY p.id;

-- ------------------------------------------------------------
-- 4.2 Atualizar imagem_url dos destinos (por id — chave primária)
-- ------------------------------------------------------------
-- ============================================================
-- DESTINOS
-- ============================================================
-- Mapa de IDs:
--   1  → Brasil / Rio de Janeiro        (SP → RJ)
--   2  → Brasil / Bahia                 (SP → BA)
--   3  → Brasil / Ceará                 (SP → CE)
--   4  → Estados Unidos / Califórnia    (SP → CA)
--   5  → Estados Unidos / Flórida       (SP → FL)
--   6  → França / Provença-Alpes-Costa Azul (SP → FR)
--   7  → Itália / Toscana              (SP → IT)
--   8  → Japão / Tóquio               (SP → JP)
--   9  → Portugal / Lisboa             (RJ → PT)
--  10  → Argentina / Buenos Aires      (RJ → AR)
--  11  → Chile / Região Metropolitana  (MG → CL)
--  12  → México / Quintana Roo         (PR → MX)
-- ============================================================

-- 1 · Rio de Janeiro — Cristo Redentor e Pão de Açúcar
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 1;

-- 2 · Bahia — Pelourinho / Salvador colorido
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80'
WHERE id = 2;

-- 3 · Ceará — Jericoacoara / dunas e lagoa
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1661692612848-37801f680815?w=800&q=80'
WHERE id = 3;

-- 4 · Califórnia — Golden Gate / São Francisco
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'
WHERE id = 4;

-- 5 · Flórida — Miami Beach / South Beach
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1754269675202-6fb0016d9f21?w=800&q=80'
WHERE id = 5;

-- 6 · França / Provença — Torre Eiffel Paris
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'
WHERE id = 6;

-- 7 · Itália / Toscana — vinhedos e colinas típicas
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1759062012196-ab43aef31a6f?w=800&q=80'
WHERE id = 7;

-- 8 · Japão / Tóquio — skyline noturno de Tóquio
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=800&q=80'
WHERE id = 8;

-- 9 · Portugal / Lisboa — Belém / Torre de Belém
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1585208798174-6cedd86e019a?w=800&q=80'
WHERE id = 9;

-- 10 · Argentina / Buenos Aires — obelisco / avenida 9 de Julho
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1612294037637-ec328d0e075e?w=800&q=80'
WHERE id = 10;

-- 11 · Chile / Santiago — vista panorâmica cidade + Andes
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1689850543263-01a52ccc6943?w=800&q=80'
WHERE id = 11;

-- 12 · México / Quintana Roo — Cancún / Riviera Maya praia turquesa
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1552074284-5e88ef1aef18?w=800&q=80'
WHERE id = 12;


-- ------------------------------------------------------------
-- 4.3 Atualizar preco_por_pessoa dos destinos (por id)
-- ------------------------------------------------------------
UPDATE destinos SET preco_por_pessoa = 1200.00  WHERE id = 1;   -- Rio de Janeiro
UPDATE destinos SET preco_por_pessoa = 980.00   WHERE id = 2;   -- Bahia
UPDATE destinos SET preco_por_pessoa = 850.00   WHERE id = 3;   -- Ceará
UPDATE destinos SET preco_por_pessoa = 8500.00  WHERE id = 4;   -- Califórnia
UPDATE destinos SET preco_por_pessoa = 7900.00  WHERE id = 5;   -- Flórida
UPDATE destinos SET preco_por_pessoa = 11500.00 WHERE id = 6;   -- França
UPDATE destinos SET preco_por_pessoa = 10800.00 WHERE id = 7;   -- Itália
UPDATE destinos SET preco_por_pessoa = 13200.00 WHERE id = 8;   -- Japão
UPDATE destinos SET preco_por_pessoa = 9400.00  WHERE id = 9;   -- Portugal
UPDATE destinos SET preco_por_pessoa = 4200.00  WHERE id = 10;  -- Argentina
UPDATE destinos SET preco_por_pessoa = 5600.00  WHERE id = 11;  -- Chile
UPDATE destinos SET preco_por_pessoa = 6300.00  WHERE id = 12;  -- México

-- ------------------------------------------------------------
-- 4.4 Atualizar imagem_url dos pacotes (por id)
-- ------------------------------------------------------------
-- ============================================================
-- PACOTES
-- ============================================================
-- Mapa de IDs:
--   1  → Rio Premium Experience        (destino 1 – RJ)
--   2  → Bahia All Inclusive           (destino 2 – BA)
--   3  → Califórnia Dreams             (destino 4 – CA)
--   4  → Nova York Experience          (destino 4 – NY)
--   5  → Paris Romântica               (destino 6 – FR)
--   6  → Toscana Gourmet               (destino 7 – IT)
--   7  → Patagônia Argentina           (destino 10 – AR)
--   8  → Tóquio Tech Tour              (destino 8 – JP)
--   9  → Bali Paradise                 (destino 12 – ID)
--  10  → Dubai Lux Experience          (destino 12 – AE)
-- ============================================================

-- 1 · Rio Premium Experience — praia de Copacabana / Ipanema
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 1;

-- 2 · Bahia All Inclusive — praia de Morro de São Paulo / Trancoso
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80'
WHERE id = 2;

-- 3 · Califórnia Dreams — rodovia 1 / costa da Califórnia
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'
WHERE id = 3;

-- 4 · Nova York Experience — Times Square / Manhattan skyline
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1534430480872-3498386e7856?w=800&q=80'
WHERE id = 4;

-- 5 · Paris Romântica — Torre Eiffel ao entardecer
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'
WHERE id = 5;

-- 6 · Toscana Gourmet — vinhedos e villa toscana
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1759062012196-ab43aef31a6f?w=800&q=80'
WHERE id = 6;

-- 7 · Patagônia Argentina — Torres del Paine / Perito Moreno
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1741684650296-19f452c8814f?w=800&q=80'
WHERE id = 7;

-- 8 · Tóquio Tech Tour — Shibuya crossing / templos
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800&q=80'
WHERE id = 8;

-- 9 · Bali Paradise — terraços de arroz / templo Uluwatu
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80'
WHERE id = 9;

-- 10 · Dubai Lux Experience — Burj Khalifa / skyline Dubai
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80'
WHERE id = 10;


-- ============================================================
-- 5. ÍNDICES
-- ============================================================

CREATE INDEX idx_carrinho_usuario ON carrinho(usuario_id);
CREATE INDEX idx_pacotes_destino  ON pacotes(destino_id);
CREATE INDEX idx_viagem_usuario   ON viagem_personalizada(usuario_id);


-- ============================================================
-- 6. SELECTs DE VERIFICAÇÃO
-- ============================================================

-- 6.1 Todos os usuários
SELECT id, nome, email, tipo FROM usuarios ORDER BY id;

-- 6.2 Todos os destinos com imagem e preço
SELECT id, origem_pais, origem_estado, pais, estado, preco_por_pessoa, imagem_url
FROM destinos ORDER BY id;

-- 6.3 Todos os pacotes com imagem e destino relacionado
SELECT
    p.id,
    p.nome                              AS pacote,
    CONCAT(d.pais, ' - ', d.estado)    AS destino,
    p.tipo_viagem,
    p.duracao_dias,
    p.data_partida,
    p.data_retorno,
    p.preco_por_pessoa,
    p.vagas_disponiveis,
    p.imagem_url
FROM pacotes p
JOIN destinos d ON d.id = p.destino_id
ORDER BY p.id;

-- 6.4 Viagens personalizadas (mais recentes primeiro)
SELECT
    vp.id, vp.usuario_id, u.nome AS nome_usuario,
    vp.nome_completo, vp.destino, vp.data_partida,
    vp.duracao_dias, vp.orcamento, vp.data_criacao
FROM viagem_personalizada vp
JOIN usuarios u ON u.id = vp.usuario_id
ORDER BY vp.id DESC;

-- 6.5 Carrinho com subtotal calculado
SELECT
    c.id, u.nome AS usuario, c.tipo_item, c.nome_item,
    c.quantidade, c.preco_unitario,
    (c.quantidade * c.preco_unitario) AS subtotal,
    c.data_adicionado
FROM carrinho c
JOIN usuarios u ON u.id = c.usuario_id
ORDER BY c.usuario_id, c.data_adicionado DESC;

-- 6.6 Contagem geral — painel do administrador
SELECT
    (SELECT COUNT(*) FROM usuarios)             AS total_usuarios,
    (SELECT COUNT(*) FROM destinos)             AS total_destinos,
    (SELECT COUNT(*) FROM pacotes)              AS total_pacotes,
    (SELECT COUNT(*) FROM viagem_personalizada) AS total_viagens_personalizadas,
    (SELECT COUNT(*) FROM carrinho)             AS total_itens_carrinho,
    (SELECT COUNT(*) FROM pedidos)             AS total_pedidos;
    
    SELECT * FROM pacotes;
    SELECT * FROM usuarios;
    SELECT * FROM destinos;
    SELECT * FROM pedidos;