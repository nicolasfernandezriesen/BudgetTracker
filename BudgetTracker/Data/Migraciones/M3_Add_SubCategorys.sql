INSERT INTO categorys (category_name) VALUES
('Otro'),
('Alquiler'),
('Comida'),
('Internet'),
('Sueldo'),
('Tarjeta');


	ALTER TABLE Categorys 
ADD ParentCategoryId INT NULL;

-- 2. Establecer la relación de Clave Foránea (Self-Referencing)
ALTER TABLE Categorys
ADD CONSTRAINT FK_Categorys_Parent 
FOREIGN KEY (ParentCategoryId) REFERENCES Categorys(category_id);


INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Vivienda', NULL);    -- Supongamos ID 7
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Transporte', NULL);  -- Supongamos ID 8
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Alimentación', NULL);-- Supongamos ID 9
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Servicios', NULL);    -- Supongamos ID 10

-- --- SUBCATEGORÍAS (HIJOS) ---
-- Hijos de Vivienda (ID 7)
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Mantenimiento', 7);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Impuestos Hogar', 7);

-- Hijos de Transporte (ID 8)
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Uber/Cabify', 8);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Nafta', 8);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Peajes', 8);

-- Hijos de Alimentación (ID 9)
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Supermercado', 9);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Delivery', 9);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Restaurantes', 9);

-- Hijos de Servicios (ID 10)
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Luz', 10);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Agua', 10);
INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Gas', 10);


INSERT INTO Categorys (category_name, ParentCategoryId) VALUES ('Finanza', NULL);

INSERT INTO public.categorys (category_name, parentcategoryid) VALUES ('Otro', 1);